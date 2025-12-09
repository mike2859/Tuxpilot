#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Script de validation de licence Tuxpilot via API
Appelle l'API du site web pour valider la clé
"""

import json
import sys
import os
from pathlib import Path
from datetime import datetime

try:
    import requests
except ImportError:
    print(json.dumps({
        "valid": False,
        "error": "Module 'requests' non installé. Installez avec: pip3 install requests"
    }))
    sys.exit(1)

# URL de l'API de validation
#API_URL = "https://lechevalierdelacyber.fr/api/License/validate"
API_URL = "http://localhost:5220/api/License/validate"

# Timeout pour les requêtes (en secondes)
REQUEST_TIMEOUT = 10

def obtenir_chemin_stockage():
    """Retourne le chemin du fichier de stockage de la licence"""
    config_dir = Path.home() / ".config" / "tuxpilot"
    config_dir.mkdir(parents=True, exist_ok=True)
    return config_dir / "license.json"

def charger_licence_stockee():
    """Charge la licence depuis le fichier de stockage"""
    chemin = obtenir_chemin_stockage()
    if not chemin.exists():
        return None

    try:
        with open(chemin, 'r') as f:
            return json.load(f)
    except Exception as e:
        print(f"Erreur lecture licence: {e}", file=sys.stderr)
        return None

def sauvegarder_licence(licence_data: dict):
    """Sauvegarde la licence validée dans le fichier"""
    chemin = obtenir_chemin_stockage()

    # Ajouter timestamp d'activation
    licence_data['activated_at'] = datetime.now().isoformat()

    with open(chemin, 'w') as f:
        json.dump(licence_data, f, indent=2)

    # Définir permissions 600 (lecture/écriture user uniquement)
    chemin.chmod(0o600)

def valider_format_cle(cle: str) -> bool:
    """Valide le format de base de la clé (EA-2024-ABCD-1234)"""
    parties = cle.strip().upper().split('-')

    # Format attendu : 4 parties
    if len(parties) != 4:
        return False

    # Partie 1 : EA ou PRO ou ORG
    if parties[0] not in ['EA', 'PRO', 'ORG']:
        return False

    # Partie 2 : Année (4 chiffres)
    if not parties[1].isdigit() or len(parties[1]) != 4:
        return False

    # Parties 3 et 4 : Alphanumériques
    if len(parties[2]) < 4 or len(parties[3]) < 4:
        return False

    return True

def valider_licence_api(cle: str) -> dict:
    """
    Valide une clé de licence via l'API
    
    Args:
        cle: Clé au format EA-2024-ABCD-1234
    
    Returns:
        dict: Résultat de la validation
    """
    try:
        # Nettoyer la clé
        cle = cle.strip().upper()

        # Validation format de base
        if not valider_format_cle(cle):
            return {
                "valid": False,
                "error": "Format de clé invalide. Format attendu: EA-2024-XXXX-XXXX"
            }

        # Appel API
        response = requests.post(
            API_URL,
            json={"licenseKey": cle},  # ✅ CORRIGÉ : licenseKey (pas license_key)
            headers={
                "Content-Type": "application/json",
                "User-Agent": "Tuxpilot/1.0"
            },
            timeout=REQUEST_TIMEOUT
        )

        # Vérifier code HTTP
        if response.status_code == 200:
            api_response = response.json()

            # ✅ Mapper la réponse de l'API au format attendu
            is_valid = api_response.get('isValid', False)

            # Si pas valide, retourner l'erreur directement
            if not is_valid:
                return {
                    "valid": False,
                    "error": api_response.get('errorMessage', 'Clé de licence invalide'),
                    "stored": False
                }

            # Mapper LicenseType (enum) vers string
            license_type = api_response.get('licenseType')
            if license_type == 0:  # ProGratuit
                type_str = "Pro"
            elif license_type == 1:  # ProDiscount
                type_str = "Pro"
            elif license_type == 2:  # Community
                type_str = "Community"
            else:
                type_str = "Pro"

            # Déterminer les features selon le type
            if type_str == "Pro":
                features = [
                    "dashboard", "monitoring", "updates_auto",
                    "security_audit", "scheduling", "cleaning",
                    "diagnostics", "ai_assistant_unlimited", "reports",
                    "notifications", "backups", "priority_support"
                ]
            else:
                features = ["dashboard", "monitoring", "updates_manual"]

            # Construire la réponse normalisée
            normalized_response = {
                "valid": True,
                "type": type_str,
                "features": features,
                "key": cle,
                "expires_at": api_response.get('expirationDate'),
                "is_early_adopter": api_response.get('isEarlyAdopter', False),
                "discount_percentage": api_response.get('discountPercentage'),
                "activation_count": api_response.get('activationCount', 1),
                "stored": False
            }

            # Sauvegarder si valide
            sauvegarder_licence({
                'key': cle,
                'type': type_str,
                'features': features,
                'expires_at': api_response.get('expirationDate'),
                'is_early_adopter': api_response.get('isEarlyAdopter', False),
                'discount_percentage': api_response.get('discountPercentage'),
                'activation_count': api_response.get('activationCount', 1)
            })
            normalized_response['stored'] = True

            return normalized_response

        elif response.status_code == 400:
            # Bad Request
            try:
                error_data = response.json()
                return {
                    "valid": False,
                    "error": error_data.get('errorMessage', 'Requête invalide')
                }
            except:
                return {
                    "valid": False,
                    "error": "Clé de licence invalide"
                }

        elif response.status_code == 404:
            return {
                "valid": False,
                "error": "Clé de licence non trouvée ou invalide"
            }

        elif response.status_code == 403:
            return {
                "valid": False,
                "error": "Clé de licence révoquée ou expirée"
            }

        else:
            return {
                "valid": False,
                "error": f"Erreur serveur (code {response.status_code})"
            }

    except requests.exceptions.Timeout:
        return {
            "valid": False,
            "error": "Délai d'attente dépassé. Vérifiez votre connexion internet."
        }

    except requests.exceptions.ConnectionError:
        return {
            "valid": False,
            "error": "Impossible de contacter le serveur. Vérifiez votre connexion internet."
        }

    except requests.exceptions.RequestException as e:
        return {
            "valid": False,
            "error": f"Erreur réseau : {str(e)}"
        }

    except Exception as e:
        return {
            "valid": False,
            "error": f"Erreur inattendue : {str(e)}"
        }

def verifier_licence_stockee() -> dict:
    """Vérifie la licence actuellement stockée"""
    licence_stockee = charger_licence_stockee()

    if not licence_stockee:
        # Pas de licence = Community par défaut
        return {
            "valid": True,
            "type": "Community",
            "features": ["dashboard", "monitoring", "updates_manual"],
            "stored": False,
            "offline": True
        }

    # Retourner les infos de la licence stockée
    return {
        "valid": True,
        "type": licence_stockee.get('type', 'Pro'),
        "features": licence_stockee.get('features', []),
        "key": licence_stockee.get('key', ''),
        "activated_at": licence_stockee.get('activated_at'),
        "expires_at": licence_stockee.get('expires_at'),
        "is_early_adopter": licence_stockee.get('is_early_adopter', False),
        "stored": True,
        "offline": True
    }

def revoquer_licence() -> dict:
    """Révoque la licence locale (supprime le fichier)"""
    try:
        chemin = obtenir_chemin_stockage()
        if chemin.exists():
            chemin.unlink()

        return {
            "success": True,
            "message": "Licence révoquée avec succès"
        }
    except Exception as e:
        return {
            "success": False,
            "error": f"Erreur lors de la révocation : {str(e)}"
        }

def main():
    """Point d'entrée du script"""
    if len(sys.argv) < 2:
        print(json.dumps({
            "valid": False,
            "error": "Usage: python3 validate_license.py [check|activate|revoke] [key]"
        }))
        sys.exit(1)

    command = sys.argv[1]

    if command == "check":
        # Vérifier licence actuelle (offline)
        result = verifier_licence_stockee()

    elif command == "activate":
        if len(sys.argv) < 3:
            result = {
                "valid": False,
                "error": "Clé de licence manquante"
            }
        else:
            cle = sys.argv[2]
            # Valider via API
            result = valider_licence_api(cle)

    elif command == "revoke":
        # Révoquer licence
        result = revoquer_licence()

    else:
        result = {
            "valid": False,
            "error": f"Commande inconnue: {command}. Utilisez: check, activate, ou revoke"
        }

    # Retourner le JSON
    print(json.dumps(result, indent=2, ensure_ascii=False))
    sys.exit(0 if result.get("valid", result.get("success", False)) else 1)

if __name__ == "__main__":
    main()