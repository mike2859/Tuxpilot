#!/usr/bin/env python3
"""
TuxCopilot - Script de détection système
Adapté pour Fedora et toutes distributions Linux
"""

import json
import sys
import platform
import psutil
import distro

def obtenir_info_systeme():
    """Collecte les informations système"""
    try:
        memoire = psutil.virtual_memory()
        pourcentage_cpu = psutil.cpu_percent(interval=1)
        disque = psutil.disk_usage('/')

        info = {
            "distribution": f"{distro.name()} {distro.version()}",
            "distributionId": distro.id(),
            "kernel": platform.release(),

            # ✨ NOUVEAU : Infos CPU détaillées
            "cpuModel": platform.processor(),  # Modèle du CPU
            "cpuCores": psutil.cpu_count(logical=False),  # Cœurs physiques
            "cpuThreads": psutil.cpu_count(logical=True),  # Threads logiques

            "ramTotaleMB": memoire.total // (1024 * 1024),
            "ramUtiliseeMB": memoire.used // (1024 * 1024),
            "ramLibreMB": memoire.available // (1024 * 1024),
            "pourcentageRam": memoire.percent,
            "pourcentageCpu": pourcentage_cpu,
            "pourcentageDisque": disque.percent,
            "gestionnairePaquets": detecter_gestionnaire_paquets()
        }

        return info

    except Exception as e:
        return {"error": str(e)}

def detecter_gestionnaire_paquets():
    """Détecte le gestionnaire de paquets"""
    import shutil
    
    distro_id = distro.id()
    
    # Fedora 41+ utilise dnf5
    if distro_id == 'fedora':
        version = distro.version()
        try:
            version_num = int(version.split('.')[0])
            if version_num >= 41 and shutil.which('dnf5'):
                return 'dnf5'
        except:
            pass
        return 'dnf'
    
    # Autres distributions
    if distro_id in ['rhel', 'centos', 'rocky', 'almalinux']:
        return 'dnf'
    elif distro_id in ['ubuntu', 'debian', 'linuxmint', 'pop']:
        return 'apt'
    elif distro_id in ['arch', 'manjaro', 'endeavouros']:
        return 'pacman'
    elif distro_id in ['opensuse', 'suse']:
        return 'zypper'
    else:
        return 'unknown'

if __name__ == "__main__":
    try:
        info_systeme = obtenir_info_systeme()
        print(json.dumps(info_systeme, indent=2))
        sys.exit(0)
    except Exception as e:
        print(json.dumps({"error": str(e)}), file=sys.stderr)
        sys.exit(1)