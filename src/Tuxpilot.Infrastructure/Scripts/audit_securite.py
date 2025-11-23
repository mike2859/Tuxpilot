#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Script d'audit de sécurité système
Retourne un rapport JSON avec score et vérifications
"""

import json
import subprocess
import sys

def executer_commande(cmd):
    """Exécute une commande et retourne le résultat"""
    try:
        result = subprocess.run(
            cmd,
            shell=True,
            capture_output=True,
            text=True,
            timeout=10
        )
        return {
            "success": result.returncode == 0,
            "output": result.stdout.strip(),
            "error": result.stderr.strip()
        }
    except Exception as e:
        return {
            "success": False,
            "output": "",
            "error": str(e)
        }

def verifier_firewall():
    """Vérifie l'état du pare-feu"""
    verif = {
        "nom": "Firewall",
        "description": "Vérification de l'état du pare-feu système",
        "points_max": 20,
        "reussie": False,
        "niveau": "Moyen",
        "points": 10,
        "details": "",
        "recommandation": "",
        "commande_correction": None
    }

    # Vérifier UFW
    ufw = executer_commande("systemctl is-active ufw 2>/dev/null")
    ufw_actif = ufw["output"] == "active"

    # Vérifier firewalld
    firewalld = executer_commande("systemctl is-active firewalld 2>/dev/null")
    firewalld_actif = firewalld["output"] == "active"

    if ufw_actif or firewalld_actif:
        verif["reussie"] = True
        verif["niveau"] = "Aucun"
        verif["points"] = 20
        verif["details"] = "UFW est actif et configuré" if ufw_actif else "Firewalld est actif et configuré"
        verif["recommandation"] = "✅ Votre pare-feu est correctement activé"
    else:
        verif["reussie"] = False
        verif["niveau"] = "Eleve"
        verif["points"] = 5
        verif["details"] = "Aucun pare-feu actif détecté"
        verif["recommandation"] = "Activez UFW ou firewalld pour protéger votre système"
        verif["commande_correction"] = "pkexec ufw enable"

    return verif

def verifier_ssh():
    """Vérifie la configuration SSH"""
    verif = {
        "nom": "Configuration SSH",
        "description": "Vérification de la sécurité SSH",
        "points_max": 20,
        "reussie": True,
        "niveau": "Aucun",
        "points": 20,
        "details": "",
        "recommandation": "",
        "commande_correction": None
    }

    result = executer_commande("cat /etc/ssh/sshd_config 2>/dev/null")

    if not result["success"] or result["output"] == "":
        verif["details"] = "SSH n'est pas installé"
        verif["recommandation"] = "✅ Aucun risque SSH (service non installé)"
        return verif

    config = result["output"]
    problemes = []
    points_perdus = 0

    # Vérifier PermitRootLogin
    if "PermitRootLogin yes" in config and "#PermitRootLogin yes" not in config:
        problemes.append("Connexion root SSH autorisée")
        points_perdus += 8

    # Vérifier PasswordAuthentication
    if "PasswordAuthentication yes" in config and "#PasswordAuthentication yes" not in config:
        problemes.append("Authentification par mot de passe activée")
        points_perdus += 6

    # Vérifier Port par défaut
    if "Port " not in config or "Port 22" in config:
        problemes.append("Port SSH par défaut (22) utilisé")
        points_perdus += 3

    if problemes:
        verif["reussie"] = False
        verif["niveau"] = "Eleve" if points_perdus >= 10 else "Moyen"
        verif["points"] = 20 - points_perdus
        verif["details"] = ", ".join(problemes)
        verif["recommandation"] = "Sécurisez SSH : désactivez root login, utilisez des clés SSH"
    else:
        verif["details"] = "Configuration SSH sécurisée"
        verif["recommandation"] = "✅ SSH correctement configuré"

    return verif

def verifier_mises_a_jour():
    """Vérifie les mises à jour de sécurité"""
    verif = {
        "nom": "Mises à jour de sécurité",
        "description": "Vérification des correctifs de sécurité disponibles",
        "points_max": 20,
        "reussie": True,
        "niveau": "Aucun",
        "points": 20,
        "details": "",
        "recommandation": "",
        "commande_correction": None
    }

    # Essayer DNF puis APT
    result = executer_commande("(dnf check-update 2>/dev/null || apt list --upgradable 2>/dev/null) | wc -l")

    try:
        nb_updates = int(result["output"])

        if nb_updates == 0:
            verif["details"] = "Système à jour"
            verif["recommandation"] = "✅ Aucune mise à jour de sécurité en attente"
        elif nb_updates <= 5:
            verif["reussie"] = False
            verif["niveau"] = "Faible"
            verif["points"] = 15
            verif["details"] = f"{nb_updates} mise(s) à jour de sécurité disponible(s)"
            verif["recommandation"] = "Installez les mises à jour de sécurité"
            verif["commande_correction"] = "pkexec dnf update -y || pkexec apt upgrade -y"
        else:
            verif["reussie"] = False
            verif["niveau"] = "Moyen"
            verif["points"] = 10
            verif["details"] = f"{nb_updates} mises à jour de sécurité en attente"
            verif["recommandation"] = "⚠️ Installez rapidement les mises à jour"
            verif["commande_correction"] = "pkexec dnf update -y || pkexec apt upgrade -y"
    except:
        verif["niveau"] = "Faible"
        verif["points"] = 15
        verif["details"] = "Vérification impossible"
        verif["recommandation"] = "Vérifiez manuellement les mises à jour"

    return verif

def verifier_ports():
    """Vérifie les ports ouverts"""
    verif = {
        "nom": "Ports ouverts",
        "description": "Vérification des ports réseau exposés",
        "points_max": 20,
        "reussie": True,
        "niveau": "Aucun",
        "points": 20,
        "details": "",
        "recommandation": "",
        "commande_correction": None
    }

    result = executer_commande("ss -tuln 2>/dev/null | grep LISTEN | wc -l")

    try:
        nb_ports = int(result["output"])

        if nb_ports <= 5:
            verif["details"] = f"{nb_ports} port(s) en écoute"
            verif["recommandation"] = "✅ Nombre de ports exposés acceptable"
        elif nb_ports <= 15:
            verif["niveau"] = "Faible"
            verif["points"] = 15
            verif["details"] = f"{nb_ports} ports en écoute"
            verif["recommandation"] = "Vérifiez que tous les services sont nécessaires"
        else:
            verif["reussie"] = False
            verif["niveau"] = "Moyen"
            verif["points"] = 10
            verif["details"] = f"{nb_ports} ports en écoute (beaucoup)"
            verif["recommandation"] = "⚠️ Désactivez les services inutiles"
    except:
        verif["niveau"] = "Faible"
        verif["points"] = 15
        verif["details"] = "Vérification impossible"
        verif["recommandation"] = "Vérifiez manuellement avec 'ss -tuln'"

    return verif

def verifier_permissions():
    """Vérifie les permissions des fichiers sensibles"""
    verif = {
        "nom": "Permissions fichiers",
        "description": "Vérification des permissions des fichiers critiques",
        "points_max": 20,
        "reussie": True,
        "niveau": "Aucun",
        "points": 20,
        "details": "",
        "recommandation": "",
        "commande_correction": None
    }

    problemes = []

    # Vérifier /etc/passwd
    passwd = executer_commande("stat -c '%a' /etc/passwd 2>/dev/null")
    if passwd["output"] != "644":
        problemes.append("/etc/passwd a des permissions incorrectes")

    # Vérifier /etc/shadow
    shadow = executer_commande("stat -c '%a' /etc/shadow 2>/dev/null")
    shadow_perms = shadow["output"]
    if shadow_perms and shadow_perms not in ["000", "640", "400"]:
        problemes.append("/etc/shadow a des permissions trop permissives")

    if problemes:
        verif["reussie"] = False
        verif["niveau"] = "Moyen"
        verif["points"] = 12
        verif["details"] = ", ".join(problemes)
        verif["recommandation"] = "Corrigez les permissions des fichiers système"
    else:
        verif["details"] = "Permissions correctes"
        verif["recommandation"] = "✅ Fichiers système correctement protégés"

    return verif

def executer_audit():
    """Exécute l'audit complet de sécurité"""
    verifications = [
        verifier_firewall(),
        verifier_ssh(),
        verifier_mises_a_jour(),
        verifier_ports(),
        verifier_permissions()
    ]

    # Calculer le score
    total_points = sum(v["points"] for v in verifications)
    total_max = sum(v["points_max"] for v in verifications)
    score = int((total_points / total_max) * 100) if total_max > 0 else 0

    rapport = {
        "score": score,
        "verifications": verifications,
        "verifications_reussies": sum(1 for v in verifications if v["reussie"]),
        "total_verifications": len(verifications),
        "problemes_critiques": sum(1 for v in verifications if v["niveau"] == "Critique"),
        "problemes_eleves": sum(1 for v in verifications if v["niveau"] == "Eleve"),
        "problemes_moyens": sum(1 for v in verifications if v["niveau"] == "Moyen")
    }

    return rapport

if __name__ == "__main__":
    try:
        rapport = executer_audit()
        print(json.dumps(rapport, ensure_ascii=False, indent=2))
        sys.exit(0)
    except Exception as e:
        print(json.dumps({"error": str(e)}, ensure_ascii=False), file=sys.stderr)
        sys.exit(1)