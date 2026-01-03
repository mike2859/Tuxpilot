#!/usr/bin/env python3
"""
Script automatique de remplacement des couleurs hardcodées par DynamicResource
Pour le projet Tuxpilot - Support thème Light/Dark

Usage:
    python3 fix_colors_auto.py /path/to/Tuxpilot/src/Tuxpilot.UI/Views/

Créé le: 28 décembre 2024
"""

import os
import re
import sys
from pathlib import Path
from typing import Dict, Tuple

# Mapping couleur hex → DynamicResource
COLOR_REPLACEMENTS: Dict[str, Tuple[str, str]] = {
    # Format: "hex_color": ("DynamicResource", "context/description")
    
    # Backgrounds clairs (problématiques en Light)
    "#DBEAFE": ("BackgroundInfo", "Info background clair"),
    "#F0F9FF": ("BackgroundInfo", "Info ultra clair"),
    "#EFF6FF": ("BackgroundInfo", "Info subtle"),
    "#FEF3C7": ("BackgroundWarning", "Warning background"),
    "#FFFBEB": ("BackgroundWarning", "Warning ultra clair"),
    "#D1FAE5": ("BackgroundSuccess", "Success background"),
    "#ECFDF5": ("BackgroundSuccess", "Success ultra clair"),
    "#FEE2E2": ("BackgroundDanger", "Danger background"),
    "#F9FAFB": ("BackgroundSecondary", "Neutral très clair"),
    
    # Backgrounds foncés
    "#334155": ("BackgroundTertiary", "Gris foncé separator"),
    "#1E1E1E": ("BackgroundPrimary", "Noir dark mode"),
    "#1e293b": ("BackgroundSecondary", "Dark button background"),
    
    # Bordures
    "#3B82F6": ("Primary", "Bleu primaire"),
    "#3b82f6": ("Primary", "Bleu primaire (lowercase)"),
    "#BAE6FD": ("BorderPrimary", "Bordure bleu clair"),
    "#10B981": ("Success", "Vert succès"),
    "#DC2626": ("Danger", "Rouge danger foncé"),
    "#EF4444": ("Danger", "Rouge danger"),
    "#F59E0B": ("Warning", "Orange warning"),
    "#FCD34D": ("Warning", "Jaune warning"),
    "#E5E7EB": ("BorderPrimary", "Gris border"),
    
    # Texte
    "#1E40AF": ("TextInfo", "Bleu foncé info"),
    "#1E3A8A": ("TextInfo", "Bleu très foncé"),
    "#0284C7": ("Info", "Bleu info"),
    "#92400E": ("TextWarning", "Brun warning"),
    "#78350F": ("TextWarning", "Brun foncé warning"),
    "#065F46": ("TextSuccess", "Vert foncé success"),
    "#047857": ("TextSuccess", "Vert success"),
    "#991B1B": ("TextDanger", "Rouge foncé danger"),
    "#6B7280": ("TextMuted", "Gris texte"),
    "#64748b": ("TextSecondary", "Gris secondaire"),
    "#94a3b8": ("TextMuted", "Gris clair"),
    "#D4D4D4": ("TextSecondary", "Gris clair dark"),
    
    # Overlays/Shadows (cas spéciaux - voir plus bas)
    "#80000000": ("OverlayBackground", "Overlay noir 50%"),
    "#60000000": ("ShadowColor", "Shadow noir 37.5%"),
    "#40000000": ("ShadowColor", "Shadow noir 25%"),
}

def replace_colors_in_file(filepath: Path, dry_run: bool = False) -> Tuple[int, list]:
    """
    Remplace les couleurs hardcodées dans un fichier AXAML
    
    Returns:
        Tuple (nombre_remplacements, liste_changements)
    """
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()
    
    original_content = content
    replacements_made = []
    replacement_count = 0
    
    # Pattern pour détecter les usages de couleurs
    # Supporte: Background="#XXX", Foreground="#XXX", BorderBrush="#XXX", Value="#XXX"
    patterns = [
        (r'Background\s*=\s*"(#[0-9A-Fa-f]{6,8})"', 'Background'),
        (r'Foreground\s*=\s*"(#[0-9A-Fa-f]{6,8})"', 'Foreground'),
        (r'BorderBrush\s*=\s*"(#[0-9A-Fa-f]{6,8})"', 'BorderBrush'),
        (r'Value\s*=\s*"(#[0-9A-Fa-f]{6,8})"', 'Value'),
        # BoxShadow cas spécial
        (r'BoxShadow\s*=\s*"([^"]*)(#[0-9A-Fa-f]{8})([^"]*)"', 'BoxShadow'),
    ]
    
    for pattern, prop_name in patterns:
        if prop_name == 'BoxShadow':
            # Cas spécial BoxShadow: "0 20 50 10 #60000000"
            matches = re.finditer(pattern, content)
            for match in matches:
                full_match = match.group(0)
                prefix = match.group(1)
                color = match.group(2)
                suffix = match.group(3)
                
                if color in COLOR_REPLACEMENTS:
                    resource, desc = COLOR_REPLACEMENTS[color]
                    # Note: BoxShadow ne supporte pas DynamicResource directement
                    # Il faut utiliser un binding ou accepter la limitation
                    # Pour l'instant, on le signale
                    replacements_made.append(
                        f"⚠️  {prop_name}: {color} → {resource} (MANUEL - BoxShadow ne supporte pas DynamicResource)"
                    )
        else:
            # Cas normaux
            matches = re.finditer(pattern, content)
            for match in matches:
                full_match = match.group(0)
                color = match.group(1)
                
                if color in COLOR_REPLACEMENTS:
                    resource, desc = COLOR_REPLACEMENTS[color]
                    new_value = f'{prop_name}="{{DynamicResource {resource}}}"'
                    content = content.replace(full_match, new_value, 1)
                    replacement_count += 1
                    replacements_made.append(
                        f"✅ {prop_name}: {color} → {resource} ({desc})"
                    )
    
    # Sauvegarder si modifications et pas dry-run
    if not dry_run and content != original_content:
        # Backup original
        backup_path = filepath.with_suffix('.axaml.backup')
        with open(backup_path, 'w', encoding='utf-8') as f:
            f.write(original_content)
        
        # Écrire nouveau contenu
        with open(filepath, 'w', encoding='utf-8') as f:
            f.write(content)
        
        print(f"💾 Backup créé: {backup_path}")
    
    return replacement_count, replacements_made


def process_directory(views_dir: Path, dry_run: bool = False):
    """Traite tous les fichiers .axaml dans le répertoire"""
    
    files_to_process = [
        "AssistantIAView.axaml",
        "AuditSecuriteView.axaml",
        "DashboardView.axaml",
        "DiagnosticView.axaml",
        "LicenseActivationView.axaml",
        "MainWindow.axaml",
        "MisesAJourView.axaml",
        "NettoyageView.axaml",
        "ServicesView.axaml",
    ]
    
    print("=" * 80)
    print("🎨 CORRECTION AUTOMATIQUE DES COULEURS HARDCODÉES - TUXPILOT")
    print("=" * 80)
    print()
    
    if dry_run:
        print("⚠️  MODE DRY-RUN: Aucune modification ne sera effectuée")
        print()
    
    total_replacements = 0
    total_files = 0
    
    for filename in files_to_process:
        filepath = views_dir / filename
        
        if not filepath.exists():
            print(f"❌ Fichier non trouvé: {filename}")
            continue
        
        print(f"\n📄 Traitement de {filename}...")
        print("-" * 80)
        
        count, changes = replace_colors_in_file(filepath, dry_run)
        
        if changes:
            for change in changes:
                print(f"   {change}")
            print(f"\n   Total: {count} remplacements effectués")
            total_replacements += count
            total_files += 1
        else:
            print("   ✨ Aucune couleur hardcodée trouvée (ou déjà corrigé)")
    
    print()
    print("=" * 80)
    print(f"📊 RÉSUMÉ")
    print("=" * 80)
    print(f"Fichiers traités: {total_files}")
    print(f"Total remplacements: {total_replacements}")
    
    if dry_run:
        print("\n⚠️  Mode DRY-RUN: Relancez sans --dry-run pour appliquer les changements")
    else:
        print("\n✅ Corrections appliquées ! Les fichiers originaux sont backupés (.axaml.backup)")
    print()


def main():
    if len(sys.argv) < 2:
        print("Usage: python3 fix_colors_auto.py <path_to_Views_directory> [--dry-run]")
        print()
        print("Exemple:")
        print("  python3 fix_colors_auto.py /home/user/Tuxpilot/src/Tuxpilot.UI/Views/")
        print("  python3 fix_colors_auto.py /home/user/Tuxpilot/src/Tuxpilot.UI/Views/ --dry-run")
        sys.exit(1)
    
    views_dir = Path(sys.argv[1])
    dry_run = "--dry-run" in sys.argv
    
    if not views_dir.exists():
        print(f"❌ Répertoire non trouvé: {views_dir}")
        sys.exit(1)
    
    process_directory(views_dir, dry_run)


if __name__ == "__main__":
    main()
