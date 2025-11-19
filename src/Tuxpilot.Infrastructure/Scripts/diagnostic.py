#!/usr/bin/env python3
"""
Tuxpilot - Script de diagnostic système
Analyse l'état du système (services, logs, disque, processus)
"""

import json
import sys
import subprocess
import shutil
from datetime import datetime


def verifier_services():
    """Vérifie les services systemd en échec"""
    try:
        # Liste des services en failed
        result = subprocess.run(
            ['systemctl', '--failed', '--no-pager', '--no-legend'],
            capture_output=True,
            text=True,
            timeout=5
        )

        services_erreur = []
        for line in result.stdout.strip().split('\n'):
            if line.strip():
                parts = line.split()
                if len(parts) >= 2:
                    services_erreur.append({
                        "nom": parts[0],
                        "etat": "failed",
                        "description": ' '.join(parts[1:]) if len(parts) > 1 else "Service en échec"
                    })

        return {
            "nombreErreurs": len(services_erreur),
            "services": services_erreur[:10]  # Limite à 10
        }
    except Exception:
        return {
            "nombreErreurs": 0,
            "services": []
        }


def analyser_logs_recents():
    """Analyse les logs système récents (dernières 24h)"""
    try:
        # Logs avec priorité error ou warning depuis 24h
        result = subprocess.run(
            ['journalctl', '-p', 'err', '--since', '24 hours ago', '--no-pager', '-n', '50'],
            capture_output=True,
            text=True,
            timeout=10
        )

        logs = []
        for line in result.stdout.strip().split('\n'):
            if line.strip() and not line.startswith('--'):
                # Parser la ligne (format: date heure hostname service: message)
                parts = line.split(maxsplit=4)
                if len(parts) >= 5:
                    logs.append({
                        "timestamp": f"{parts[0]} {parts[1]}",
                        "service": parts[3].rstrip(':'),
                        "message": parts[4][:200]  # Limiter la longueur
                    })

        return {
            "nombreLogs": len(logs),
            "logs": logs[:20]  # Limite à 20 entrées
        }
    except Exception:
        return {
            "nombreLogs": 0,
            "logs": []
        }


def analyser_disque():
    """Analyse l'espace disque"""
    try:
        result = subprocess.run(
            ['df', '-h', '/'],
            capture_output=True,
            text=True,
            timeout=5
        )

        lines = result.stdout.strip().split('\n')
        if len(lines) >= 2:
            parts = lines[1].split()
            if len(parts) >= 5:
                utilise_pct = parts[4].rstrip('%')

                return {
                    "partition": parts[0],
                    "taille": parts[1],
                    "utilise": parts[2],
                    "disponible": parts[3],
                    "pourcentage": int(utilise_pct) if utilise_pct.isdigit() else 0
                }

        return {
            "partition": "/",
            "taille": "N/A",
            "utilise": "N/A",
            "disponible": "N/A",
            "pourcentage": 0
        }
    except Exception:
        return {
            "partition": "/",
            "taille": "N/A",
            "utilise": "N/A",
            "disponible": "N/A",
            "pourcentage": 0
        }


def analyser_processus_gourmands():
    """Identifie les processus les plus gourmands"""
    try:
        # Top 5 processus par CPU
        result_cpu = subprocess.run(
            ['ps', 'aux', '--sort=-%cpu'],
            capture_output=True,
            text=True,
            timeout=5
        )

        # Top 5 processus par RAM
        result_mem = subprocess.run(
            ['ps', 'aux', '--sort=-%mem'],
            capture_output=True,
            text=True,
            timeout=5
        )

        def parser_processus(output, limit=5):
            processus = []
            lines = output.strip().split('\n')[1:]  # Skip header

            for line in lines[:limit]:
                parts = line.split(maxsplit=10)
                if len(parts) >= 11:
                    processus.append({
                        "nom": parts[10][:50],  # Limiter la longueur
                        "utilisateur": parts[0],
                        "cpu": parts[2],
                        "ram": parts[3],
                        "pid": parts[1]
                    })

            return processus

        return {
            "topCpu": parser_processus(result_cpu.stdout, 5),
            "topRam": parser_processus(result_mem.stdout, 5)
        }
    except Exception:
        return {
            "topCpu": [],
            "topRam": []
        }


def calculer_score_sante():
    """Calcule un score de santé global (0-100)"""
    try:
        score = 100

        # Pénalités
        services = verifier_services()
        if services['nombreErreurs'] > 0:
            score -= min(services['nombreErreurs'] * 10, 30)  # Max -30

        logs = analyser_logs_recents()
        if logs['nombreLogs'] > 10:
            score -= min((logs['nombreLogs'] - 10) * 2, 20)  # Max -20

        disque = analyser_disque()
        if disque['pourcentage'] > 80:
            score -= (disque['pourcentage'] - 80) * 2  # -2 par % au-dessus de 80

        return max(0, min(100, score))
    except Exception:
        return 50  # Score neutre en cas d'erreur


def diagnostiquer_systeme():
    """
    Effectue un diagnostic complet du système
    
    Returns:
        dict: Informations de diagnostic
    """
    try:
        services = verifier_services()
        logs = analyser_logs_recents()
        disque = analyser_disque()
        processus = analyser_processus_gourmands()
        score_sante = calculer_score_sante()

        # Déterminer l'état global
        if score_sante >= 80:
            etat_global = "sain"
            message_global = "Votre système fonctionne correctement"
        elif score_sante >= 60:
            etat_global = "attention"
            message_global = "Quelques points nécessitent votre attention"
        else:
            etat_global = "probleme"
            message_global = "Des problèmes ont été détectés"

        return {
            "timestamp": datetime.now().isoformat(),
            "scoreSante": score_sante,
            "etatGlobal": etat_global,
            "messageGlobal": message_global,
            "services": services,
            "logs": logs,
            "disque": disque,
            "processus": processus
        }

    except Exception as e:
        return {
            "timestamp": datetime.now().isoformat(),
            "scoreSante": 0,
            "etatGlobal": "erreur",
            "messageGlobal": "Erreur lors du diagnostic",
            "services": {"nombreErreurs": 0, "services": []},
            "logs": {"nombreLogs": 0, "logs": []},
            "disque": {"partition": "/", "taille": "N/A", "utilise": "N/A",
                       "disponible": "N/A", "pourcentage": 0},
            "processus": {"topCpu": [], "topRam": []},
            "erreur": str(e)
        }


if __name__ == "__main__":
    """Point d'entrée du script"""
    try:
        resultat = diagnostiquer_systeme()
        print(json.dumps(resultat, indent=2, ensure_ascii=False))
        sys.exit(0)
    except Exception as e:
        print(json.dumps({"erreur": str(e)}), file=sys.stderr)
        sys.exit(1)