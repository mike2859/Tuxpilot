#!/usr/bin/env python3
"""
Tuxpilot - Script d'installation des mises à jour
Installe les mises à jour via DNF/APT avec gestion sudo
"""

import json
import sys
import subprocess
import distro
import os


def detecter_gestionnaire_paquets():
    """Détecte le gestionnaire de paquets"""
    import shutil

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


def installer_mises_a_jour():
    """
    Installe toutes les mises à jour disponibles
    
    Returns:
        dict: Résultat de l'installation
    """
    try:
        gestionnaire = detecter_gestionnaire_paquets()

        # Vérifier si on est root
        est_root = os.geteuid() == 0

        if gestionnaire == 'dnf5':
            return installer_dnf5(est_root)
        elif gestionnaire == 'dnf':
            return installer_dnf(est_root)
        elif gestionnaire == 'apt':
            return installer_apt(est_root)
        else:
            return {
                "success": False,
                "gestionnaire": gestionnaire,
                "message": f"Gestionnaire '{gestionnaire}' non supporté"
            }

    except Exception as e:
        return {
            "success": False,
            "gestionnaire": "unknown",
            "message": f"Erreur: {str(e)}"
        }


def installer_dnf5(est_root):
    """Installe les mises à jour avec DNF5"""
    try:
        # Construire la commande
        if est_root:
            commande = ['dnf5', 'upgrade', '-y']
        else:
            # Utiliser pkexec pour demander les privilèges
            commande = ['pkexec', 'dnf5', 'upgrade', '-y']

        # Exécuter l'installation
        result = subprocess.run(
            commande,
            capture_output=True,
            text=True,
            timeout=300  # 5 minutes max
        )

        if result.returncode == 0:
            return {
                "success": True,
                "gestionnaire": "dnf5",
                "message": "Mises à jour installées avec succès"
            }
        else:
            return {
                "success": False,
                "gestionnaire": "dnf5",
                "message": f"Erreur lors de l'installation: {result.stderr}"
            }

    except subprocess.TimeoutExpired:
        return {
            "success": False,
            "gestionnaire": "dnf5",
            "message": "Timeout: l'installation a pris trop de temps"
        }
    except Exception as e:
        return {
            "success": False,
            "gestionnaire": "dnf5",
            "message": f"Erreur: {str(e)}"
        }


def installer_dnf(est_root):
    """Installe les mises à jour avec DNF"""
    try:
        if est_root:
            commande = ['dnf', 'upgrade', '-y']
        else:
            commande = ['pkexec', 'dnf', 'upgrade', '-y']

        result = subprocess.run(
            commande,
            capture_output=True,
            text=True,
            timeout=300
        )

        if result.returncode == 0:
            return {
                "success": True,
                "gestionnaire": "dnf",
                "message": "Mises à jour installées avec succès"
            }
        else:
            return {
                "success": False,
                "gestionnaire": "dnf",
                "message": f"Erreur: {result.stderr}"
            }

    except Exception as e:
        return {
            "success": False,
            "gestionnaire": "dnf",
            "message": f"Erreur: {str(e)}"
        }


def installer_apt(est_root):
    """Installe les mises à jour avec APT"""
    try:
        if est_root:
            commande = ['apt-get', 'upgrade', '-y']
        else:
            commande = ['pkexec', 'apt-get', 'upgrade', '-y']

        result = subprocess.run(
            commande,
            capture_output=True,
            text=True,
            timeout=300
        )

        if result.returncode == 0:
            return {
                "success": True,
                "gestionnaire": "apt",
                "message": "Mises à jour installées avec succès"
            }
        else:
            return {
                "success": False,
                "gestionnaire": "apt",
                "message": f"Erreur: {result.stderr}"
            }

    except Exception as e:
        return {
            "success": False,
            "gestionnaire": "apt",
            "message": f"Erreur: {str(e)}"
        }


if __name__ == "__main__":
    """Point d'entrée du script"""
    try:
        resultat = installer_mises_a_jour()
        print(json.dumps(resultat, indent=2, ensure_ascii=False))
        sys.exit(0 if resultat["success"] else 1)
    except Exception as e:
        print(json.dumps({
            "success": False,
            "message": str(e)
        }), file=sys.stderr)
        sys.exit(1)