#!/usr/bin/env python3
"""
Tuxpilot - Script de nettoyage système
Analyse et nettoie les fichiers temporaires, cache, logs, etc.
"""

import json
import sys
import subprocess
import os
import distro
import shutil
from pathlib import Path


def detecter_gestionnaire_paquets():
    """Détecte le gestionnaire de paquets"""
    distro_id = distro.id()

    if distro_id == 'fedora':
        version = distro.version()
        try:
            version_num = int(version.split('.')[0])
            if version_num >= 41 and shutil.which('dnf5'):
                return 'dnf5'
        except:
            pass
        return 'dnf'
    elif distro_id in ['rhel', 'centos', 'rocky', 'almalinux']:
        return 'dnf'
    elif distro_id in ['ubuntu', 'debian', 'linuxmint', 'pop']:
        return 'apt'
    else:
        return 'unknown'


def obtenir_taille_dossier(chemin):
    """Calcule la taille totale d'un dossier en bytes"""
    try:
        if not os.path.exists(chemin):
            return 0

        taille_totale = 0
        for dirpath, dirnames, filenames in os.walk(chemin):
            for filename in filenames:
                filepath = os.path.join(dirpath, filename)
                try:
                    taille_totale += os.path.getsize(filepath)
                except (OSError, FileNotFoundError):
                    pass
        return taille_totale
    except Exception:
        return 0


def analyser_cache_paquets():
    """Analyse le cache du gestionnaire de paquets"""
    gestionnaire = detecter_gestionnaire_paquets()

    try:
        if gestionnaire in ['dnf5', 'dnf']:
            # Cache DNF : /var/cache/dnf ou /var/cache/libdnf5
            chemins_cache = ['/var/cache/dnf', '/var/cache/libdnf5']
            taille_totale = sum(obtenir_taille_dossier(c) for c in chemins_cache)

            return {
                "type": "cache_paquets",
                "nom": f"Cache {gestionnaire.upper()}",
                "chemin": ", ".join(chemins_cache),
                "tailleMB": taille_totale // (1024 * 1024),
                "description": "Cache des paquets téléchargés"
            }
        elif gestionnaire == 'apt':
            chemin = '/var/cache/apt/archives'
            taille = obtenir_taille_dossier(chemin)

            return {
                "type": "cache_paquets",
                "nom": "Cache APT",
                "chemin": chemin,
                "tailleMB": taille // (1024 * 1024),
                "description": "Cache des paquets téléchargés"
            }
        else:
            return None
    except Exception as e:
        return None


def analyser_logs_anciens():
    """Analyse les logs système anciens"""
    try:
        chemin_logs = '/var/log'

        # Compter les logs anciens (> 30 jours)
        import time
        now = time.time()
        taille_totale = 0
        nombre_fichiers = 0

        for root, dirs, files in os.walk(chemin_logs):
            for file in files:
                filepath = os.path.join(root, file)
                try:
                    # Logs compressés ou anciens
                    if file.endswith(('.gz', '.old', '.1', '.2', '.3')) or '-' in file:
                        stat = os.stat(filepath)
                        age_jours = (now - stat.st_mtime) / 86400

                        if age_jours > 30:
                            taille_totale += stat.st_size
                            nombre_fichiers += 1
                except (OSError, FileNotFoundError):
                    pass

        return {
            "type": "logs_anciens",
            "nom": "Logs anciens (>30 jours)",
            "chemin": chemin_logs,
            "tailleMB": taille_totale // (1024 * 1024),
            "nombreFichiers": nombre_fichiers,
            "description": f"{nombre_fichiers} fichiers de logs anciens"
        }
    except Exception:
        return None


def analyser_paquets_orphelins():
    """Analyse les paquets orphelins"""
    gestionnaire = detecter_gestionnaire_paquets()

    try:
        if gestionnaire == 'dnf5':
            result = subprocess.run(
                ['dnf5', 'repoquery', '--unneeded'],
                capture_output=True,
                text=True,
                timeout=10
            )
            paquets = [p.strip() for p in result.stdout.strip().split('\n') if p.strip()]
            nombre = len(paquets)

        elif gestionnaire == 'dnf':
            result = subprocess.run(
                ['package-cleanup', '--leaves', '--quiet'],
                capture_output=True,
                text=True,
                timeout=10
            )
            paquets = [p.strip() for p in result.stdout.strip().split('\n') if p.strip()]
            nombre = len(paquets)

        elif gestionnaire == 'apt':
            result = subprocess.run(
                ['apt-get', 'autoremove', '--dry-run'],
                capture_output=True,
                text=True,
                timeout=10
            )
            # Parser la sortie pour compter les paquets
            lines = result.stdout.strip().split('\n')
            nombre = 0
            for line in lines:
                if 'autoremoved' in line.lower():
                    parts = line.split()
                    if parts:
                        try:
                            nombre = int(parts[0])
                        except:
                            pass
        else:
            nombre = 0

        return {
            "type": "paquets_orphelins",
            "nom": "Paquets orphelins",
            "chemin": "Système",
            "nombrePaquets": nombre,
            "description": f"{nombre} paquet(s) non utilisé(s)"
        }
    except Exception:
        return {
            "type": "paquets_orphelins",
            "nom": "Paquets orphelins",
            "chemin": "Système",
            "nombrePaquets": 0,
            "description": "0 paquet non utilisé"
        }


def analyser_fichiers_temporaires():
    """Analyse les fichiers temporaires"""
    try:
        chemins_tmp = ['/tmp', '/var/tmp']
        taille_totale = sum(obtenir_taille_dossier(c) for c in chemins_tmp)

        return {
            "type": "fichiers_temporaires",
            "nom": "Fichiers temporaires",
            "chemin": ", ".join(chemins_tmp),
            "tailleMB": taille_totale // (1024 * 1024),
            "description": "Fichiers temporaires système"
        }
    except Exception:
        return None


def analyser_nettoyage():
    """
    Analyse tous les éléments nettoyables du système
    
    Returns:
        dict: Informations sur les éléments nettoyables
    """
    try:
        elements = []

        # Analyser chaque type
        cache = analyser_cache_paquets()
        if cache:
            elements.append(cache)

        logs = analyser_logs_anciens()
        if logs:
            elements.append(logs)

        orphelins = analyser_paquets_orphelins()
        if orphelins:
            elements.append(orphelins)

        tmp = analyser_fichiers_temporaires()
        if tmp:
            elements.append(tmp)

        # Calculer le total
        taille_totale_mb = sum(
            e.get('tailleMB', 0) for e in elements
        )

        return {
            "gestionnaire": detecter_gestionnaire_paquets(),
            "elements": elements,
            "tailleTotaleMB": taille_totale_mb,
            "nombreElements": len(elements)
        }

    except Exception as e:
        return {
            "gestionnaire": "unknown",
            "elements": [],
            "tailleTotaleMB": 0,
            "nombreElements": 0,
            "erreur": str(e)
        }


if __name__ == "__main__":
    """Point d'entrée du script"""
    try:
        resultat = analyser_nettoyage()
        print(json.dumps(resultat, indent=2, ensure_ascii=False))
        sys.exit(0)
    except Exception as e:
        print(json.dumps({"erreur": str(e)}), file=sys.stderr)
        sys.exit(1)