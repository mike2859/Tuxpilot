#!/usr/bin/env python3
"""
Tuxpilot - Script de détection des mises à jour
Détecte les paquets disponibles pour mise à jour via DNF/APT
"""

import json
import sys
import subprocess
import distro


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
    elif distro_id in ['arch', 'manjaro', 'endeavouros']:
        return 'pacman'
    else:
        return 'unknown'


def verifier_mises_a_jour():
    """
    Vérifie les mises à jour disponibles
    
    Returns:
        dict: Informations sur les mises à jour disponibles
    """
    try:
        gestionnaire = detecter_gestionnaire_paquets()

        if gestionnaire == 'dnf5':
            return verifier_dnf5()
        elif gestionnaire == 'dnf':
            return verifier_dnf()
        elif gestionnaire == 'apt':
            return verifier_apt()
        else:
            return {
                "gestionnaire": gestionnaire,
                "nombre": 0,
                "paquets": [],
                "erreur": f"Gestionnaire '{gestionnaire}' non supporté"
            }

    except Exception as e:
        return {
            "gestionnaire": "unknown",
            "nombre": 0,
            "paquets": [],
            "erreur": str(e)
        }


def verifier_dnf5():
    """Vérifie les mises à jour avec DNF5"""
    try:
        # Vérifier les updates disponibles
        # result = subprocess.run(
        #     ['dnf5', 'check-update', '--quiet'],
        #     capture_output=True,
        #     text=True,
        #     timeout=30
        # )

        cmd = ['dnf5', 'check-update', '--quiet']
        try:
            result = subprocess.run(cmd, capture_output=True, text=True, timeout=120)
        except subprocess.TimeoutExpired:
            result = subprocess.run(cmd + ['--cacheonly'], capture_output=True, text=True, timeout=20)


        paquets = []

        # DNF5 retourne les paquets ligne par ligne
        for ligne in result.stdout.strip().split('\n'):
            if ligne and not ligne.startswith('Last metadata'):
                parties = ligne.split()
                if len(parties) >= 2:
                    paquets.append({
                        "nom": parties[0],
                        "versionActuelle": parties[1] if len(parties) > 1 else "inconnue",
                        "versionDisponible": parties[2] if len(parties) > 2 else parties[1],
                        "depot": parties[3] if len(parties) > 3 else "unknown"
                    })

        return {
            "gestionnaire": "dnf5",
            "nombre": len(paquets),
            "paquets": paquets
        }

    except subprocess.TimeoutExpired:
        return {
            "gestionnaire": "dnf5",
            "nombre": 0,
            "paquets": [],
            "erreur": "Timeout lors de la vérification"
        }
    except Exception as e:
        return {
            "gestionnaire": "dnf5",
            "nombre": 0,
            "paquets": [],
            "erreur": str(e)
        }


def verifier_dnf():
    """Vérifie les mises à jour avec DNF"""
    try:
        result = subprocess.run(
            ['dnf', 'check-update', '-q'],
            capture_output=True,
            text=True,
            timeout=30
        )

        paquets = []

        for ligne in result.stdout.strip().split('\n'):
            if ligne and not ligne.startswith('Last metadata'):
                parties = ligne.split()
                if len(parties) >= 2:
                    paquets.append({
                        "nom": parties[0],
                        "versionActuelle": "installée",
                        "versionDisponible": parties[1],
                        "depot": parties[2] if len(parties) > 2 else "unknown"
                    })

        return {
            "gestionnaire": "dnf",
            "nombre": len(paquets),
            "paquets": paquets
        }

    except Exception as e:
        return {
            "gestionnaire": "dnf",
            "nombre": 0,
            "paquets": [],
            "erreur": str(e)
        }


def verifier_apt():
    """Vérifie les mises à jour avec APT"""
    try:
        # Update package list
        subprocess.run(['apt-get', 'update'], capture_output=True, timeout=30)

        # Check for upgradable packages
        result = subprocess.run(
            ['apt', 'list', '--upgradable'],
            capture_output=True,
            text=True,
            timeout=30
        )

        paquets = []

        for ligne in result.stdout.strip().split('\n')[1:]:  # Skip header
            if ligne:
                parties = ligne.split()
                if len(parties) >= 2:
                    nom = parties[0].split('/')[0]
                    paquets.append({
                        "nom": nom,
                        "versionActuelle": parties[1],
                        "versionDisponible": parties[3] if len(parties) > 3 else parties[1],
                        "depot": parties[2] if len(parties) > 2 else "unknown"
                    })

        return {
            "gestionnaire": "apt",
            "nombre": len(paquets),
            "paquets": paquets
        }

    except Exception as e:
        return {
            "gestionnaire": "apt",
            "nombre": 0,
            "paquets": [],
            "erreur": str(e)
        }


if __name__ == "__main__":
    """Point d'entrée du script"""
    try:
        resultat = verifier_mises_a_jour()
        print(json.dumps(resultat, indent=2, ensure_ascii=False))
        sys.exit(0)
    except Exception as e:
        print(json.dumps({"erreur": str(e)}), file=sys.stderr)
        sys.exit(1)