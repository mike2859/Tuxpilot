#!/usr/bin/env python3
"""
Script d'installation des mises à jour système avec logs progressifs
Utilise pkexec pour l'élévation de privilèges
"""

import json
import subprocess
import sys
import os
from datetime import datetime

def log_message(message, type="info"):
    """Envoie un message de log au format JSON sur stdout"""
    log = {
        "type": type,
        "message": message,
        "timestamp": datetime.now().isoformat()
    }
    print(json.dumps(log, ensure_ascii=False), flush=True)

def detect_package_manager():
    """Détecte le gestionnaire de paquets disponible"""
    if os.path.exists('/usr/bin/dnf5'):
        return 'dnf5'
    elif os.path.exists('/usr/bin/dnf'):
        return 'dnf'
    elif os.path.exists('/usr/bin/apt'):
        return 'apt'
    return None

def install_with_dnf5():
    """Installation avec DNF5"""
    try:
        log_message("🔍 Vérification des mises à jour disponibles...", "info")

        # Commande d'installation avec pkexec
        cmd = ['pkexec', 'dnf5', 'upgrade', '-y']

        log_message("🔐 Demande d'authentification...", "info")
        log_message("📦 Démarrage de l'installation...", "info")

        # Exécuter avec streaming
        process = subprocess.Popen(
            cmd,
            stdout=subprocess.PIPE,
            stderr=subprocess.STDOUT,
            text=True,
            bufsize=1
        )

        # Lire ligne par ligne
        package_count = 0
        for line in iter(process.stdout.readline, ''):
            line = line.strip()
            if not line:
                continue

            # Détecter les étapes importantes
            if 'Downloading' in line or 'Téléchargement' in line:
                log_message(f"⬇️ {line}", "download")
            elif 'Installing' in line or 'Installation' in line:
                package_count += 1
                log_message(f"📦 Installation en cours... ({package_count})", "install")
            elif 'Installed' in line or 'Installé' in line:
                log_message(f"✅ {line}", "success")
            elif 'Complete' in line or 'Terminé' in line:
                log_message(f"🎉 {line}", "success")
            else:
                # Autres messages
                log_message(line, "info")

        process.wait()

        if process.returncode == 0:
            log_message(f"✅ Installation terminée avec succès ! ({package_count} paquet(s))", "final_success")
            return True
        else:
            log_message(f"❌ Erreur lors de l'installation (code: {process.returncode})", "error")
            return False

    except subprocess.CalledProcessError as e:
        log_message(f"❌ Erreur : {str(e)}", "error")
        return False
    except Exception as e:
        log_message(f"❌ Erreur inattendue : {str(e)}", "error")
        return False

def install_with_dnf():
    """Installation avec DNF classique"""
    try:
        log_message("🔍 Vérification des mises à jour disponibles...", "info")

        cmd = ['pkexec', 'dnf', 'upgrade', '-y']

        log_message("🔐 Demande d'authentification...", "info")
        log_message("📦 Démarrage de l'installation...", "info")

        process = subprocess.Popen(
            cmd,
            stdout=subprocess.PIPE,
            stderr=subprocess.STDOUT,
            text=True,
            bufsize=1
        )

        package_count = 0
        for line in iter(process.stdout.readline, ''):
            line = line.strip()
            if not line:
                continue

            if 'Downloading' in line:
                log_message(f"⬇️ {line}", "download")
            elif 'Installing' in line:
                package_count += 1
                log_message(f"📦 Installation en cours... ({package_count})", "install")
            elif 'Installed' in line:
                log_message(f"✅ {line}", "success")
            elif 'Complete' in line:
                log_message(f"🎉 {line}", "success")
            else:
                log_message(line, "info")

        process.wait()

        if process.returncode == 0:
            log_message(f"✅ Installation terminée ! ({package_count} paquet(s))", "final_success")
            return True
        else:
            log_message(f"❌ Erreur (code: {process.returncode})", "error")
            return False

    except Exception as e:
        log_message(f"❌ Erreur : {str(e)}", "error")
        return False

def install_with_apt():
    """Installation avec APT"""
    try:
        log_message("🔍 Vérification des mises à jour disponibles...", "info")

        cmd = ['pkexec', 'apt', 'upgrade', '-y']

        log_message("🔐 Demande d'authentification...", "info")
        log_message("📦 Démarrage de l'installation...", "info")

        process = subprocess.Popen(
            cmd,
            stdout=subprocess.PIPE,
            stderr=subprocess.STDOUT,
            text=True,
            bufsize=1
        )

        package_count = 0
        for line in iter(process.stdout.readline, ''):
            line = line.strip()
            if not line:
                continue

            if 'Get:' in line or 'Réception' in line:
                log_message(f"⬇️ {line}", "download")
            elif 'Unpacking' in line or 'Dépaquetage' in line:
                package_count += 1
                log_message(f"📦 Installation en cours... ({package_count})", "install")
            elif 'Setting up' in line or 'Paramétrage' in line:
                log_message(f"⚙️ {line}", "setup")
            else:
                log_message(line, "info")

        process.wait()

        if process.returncode == 0:
            log_message(f"✅ Installation terminée ! ({package_count} paquet(s))", "final_success")
            return True
        else:
            log_message(f"❌ Erreur (code: {process.returncode})", "error")
            return False

    except Exception as e:
        log_message(f"❌ Erreur : {str(e)}", "error")
        return False

def main():
    """Point d'entrée principal"""
    try:
        # Détecter le gestionnaire de paquets
        manager = detect_package_manager()

        if not manager:
            log_message("❌ Aucun gestionnaire de paquets détecté", "error")
            sys.exit(1)

        log_message(f"🔧 Gestionnaire détecté : {manager}", "info")

        # Lancer l'installation selon le gestionnaire
        if manager == 'dnf5':
            success = install_with_dnf5()
        elif manager == 'dnf':
            success = install_with_dnf()
        elif manager == 'apt':
            success = install_with_apt()
        else:
            log_message(f"❌ Gestionnaire non supporté : {manager}", "error")
            sys.exit(1)

        sys.exit(0 if success else 1)

    except KeyboardInterrupt:
        log_message("⚠️ Installation annulée par l'utilisateur", "warning")
        sys.exit(130)
    except Exception as e:
        log_message(f"❌ Erreur fatale : {str(e)}", "error")
        sys.exit(1)

if __name__ == "__main__":
    main()