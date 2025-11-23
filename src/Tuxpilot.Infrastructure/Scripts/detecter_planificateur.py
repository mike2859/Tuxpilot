#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Détecte le système de planification disponible
"""

import json
import subprocess
import sys

def executer_commande(cmd):
    """Exécute une commande silencieusement"""
    try:
        result = subprocess.run(
            cmd,
            shell=True,
            capture_output=True,
            text=True,
            timeout=5
        )
        return result.returncode == 0
    except:
        return False

def detecter_systemd():
    """Vérifie si systemd user est disponible"""
    return executer_commande("systemctl --user status 2>/dev/null")

def detecter_cron():
    """Vérifie si crontab est disponible"""
    return executer_commande("which crontab 2>/dev/null")

def main():
    """Détecte et retourne le planificateur recommandé"""
    systemd_ok = detecter_systemd()
    cron_ok = detecter_cron()

    # Préférer systemd s'il est disponible
    recommande = "systemd" if systemd_ok else ("cron" if cron_ok else "aucun")

    resultat = {
        "systemd_disponible": systemd_ok,
        "cron_disponible": cron_ok,
        "recommande": recommande,
        "success": systemd_ok or cron_ok
    }

    print(json.dumps(resultat, ensure_ascii=False, indent=2))
    sys.exit(0 if resultat["success"] else 1)

if __name__ == "__main__":
    main()