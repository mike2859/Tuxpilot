#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Script de gestion de la planification des tâches avec systemd timers
Gère les timers systemd pour Tuxpilot
"""

import json
import subprocess
import sys
import os
from pathlib import Path
from datetime import datetime

# Dossier systemd user
SYSTEMD_USER_DIR = Path.home() / ".config" / "systemd" / "user"
TUXPILOT_PREFIX = "tuxpilot-"

def executer_commande(cmd):
    """Exécute une commande et retourne le résultat"""
    try:
        result = subprocess.run(
            cmd,
            shell=True,
            capture_output=True,
            text=True,
            timeout=30
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

def creer_dossier_systemd():
    """Crée le dossier systemd user s'il n'existe pas"""
    SYSTEMD_USER_DIR.mkdir(parents=True, exist_ok=True)

def jour_semaine_vers_systemd(jour):
    """Convertit le numéro de jour en format systemd"""
    if jour == -1:
        return "*-*-*"  # Tous les jours

    # systemd: Mon=1, Tue=2, ..., Sun=7
    # Notre format: Sun=0, Mon=1, ..., Sat=6
    jours_map = {
        0: "Sun",  # Dimanche
        1: "Mon",  # Lundi
        2: "Tue",  # Mardi
        3: "Wed",  # Mercredi
        4: "Thu",  # Jeudi
        5: "Fri",  # Vendredi
        6: "Sat"   # Samedi
    }
    return jours_map.get(jour, "Mon")

def creer_service_file(task_id, task_type, nom, description):
    """Crée le fichier .service"""
    service_name = f"{TUXPILOT_PREFIX}{task_id}.service"
    service_path = SYSTEMD_USER_DIR / service_name

    # Commande selon le type de tâche
    if task_type == "MisesAJour":
        command = "echo 'Mises à jour' >> /tmp/tuxpilot_tasks.log"
    elif task_type == "Nettoyage":
        command = "echo 'Nettoyage' >> /tmp/tuxpilot_tasks.log"
    elif task_type == "Rapport":
        command = "echo 'Rapport' >> /tmp/tuxpilot_tasks.log"
    else:
        command = "echo 'Tâche inconnue' >> /tmp/tuxpilot_tasks.log"

    service_content = f"""[Unit]
Description=Tuxpilot - {nom}
After=network.target

[Service]
Type=oneshot
ExecStart=/bin/bash -c "{command}"

[Install]
WantedBy=default.target
"""

    with open(service_path, 'w') as f:
        f.write(service_content)

    return service_name

def creer_timer_file(task_id, nom, jour_semaine, heure, minute):
    """Crée le fichier .timer"""
    timer_name = f"{TUXPILOT_PREFIX}{task_id}.timer"
    timer_path = SYSTEMD_USER_DIR / timer_name

    # Convertir le jour
    jour_systemd = jour_semaine_vers_systemd(jour_semaine)

    # Format OnCalendar
    if jour_semaine == -1:
        # Tous les jours
        on_calendar = f"*-*-* {heure:02d}:{minute:02d}:00"
    else:
        # Jour spécifique
        on_calendar = f"{jour_systemd} *-*-* {heure:02d}:{minute:02d}:00"

    timer_content = f"""[Unit]
Description=Timer pour Tuxpilot - {nom}

[Timer]
OnCalendar={on_calendar}
Persistent=true

[Install]
WantedBy=timers.target
"""

    with open(timer_path, 'w') as f:
        f.write(timer_content)

    return timer_name

def lister_taches():
    """Liste toutes les tâches Tuxpilot planifiées"""
    creer_dossier_systemd()
    taches = []

    # Lister tous les fichiers .timer tuxpilot
    for timer_file in SYSTEMD_USER_DIR.glob(f"{TUXPILOT_PREFIX}*.timer"):
        task_id = timer_file.stem.replace(TUXPILOT_PREFIX, "")

        # Lire le fichier .service correspondant pour avoir les infos
        service_file = SYSTEMD_USER_DIR / f"{TUXPILOT_PREFIX}{task_id}.service"

        if not service_file.exists():
            continue

        # Parser les fichiers
        tache = parser_timer_et_service(timer_file, service_file, task_id)
        if tache:
            taches.append(tache)

    return {"taches": taches, "success": True}

def parser_timer_et_service(timer_path, service_path, task_id):
    """Parse les fichiers timer et service"""
    try:
        # Lire le timer
        with open(timer_path, 'r') as f:
            timer_content = f.read()

        # Lire le service
        with open(service_path, 'r') as f:
            service_content = f.read()

        # Extraire OnCalendar
        on_calendar = ""
        for line in timer_content.split('\n'):
            if line.startswith('OnCalendar='):
                on_calendar = line.split('=', 1)[1].strip()
                break

        # Extraire Description du service
        nom = "Tâche"
        for line in service_content.split('\n'):
            if line.startswith('Description='):
                desc = line.split('=', 1)[1].strip()
                if ' - ' in desc:
                    nom = desc.split(' - ', 1)[1]
                break

        # Parser OnCalendar pour extraire jour/heure/minute
        jour_semaine, heure, minute = parser_on_calendar(on_calendar)

        # Vérifier si le timer est activé
        result = executer_commande(f"systemctl --user is-enabled {TUXPILOT_PREFIX}{task_id}.timer 2>/dev/null")
        activee = result["output"] == "enabled"

        # Déterminer le type depuis le nom du service
        task_type = "MisesAJour"  # Par défaut
        if "nettoyage" in nom.lower():
            task_type = "Nettoyage"
        elif "rapport" in nom.lower():
            task_type = "Rapport"

        return {
            "id": task_id,
            "type": task_type,
            "nom": nom,
            "description": f"Planifié via systemd timer",
            "jour_semaine": jour_semaine,
            "heure": heure,
            "minute": minute,
            "activee": activee,
            "date_creation": datetime.now().isoformat()
        }
    except Exception as e:
        print(f"Erreur parsing: {e}", file=sys.stderr)
        return None

def parser_on_calendar(on_calendar):
    """Parse une expression OnCalendar"""
    try:
        # Format: "*-*-* HH:MM:SS" ou "DayOfWeek *-*-* HH:MM:SS"
        parts = on_calendar.split()

        if len(parts) == 2:
            # Tous les jours: "*-*-* HH:MM:SS"
            jour_semaine = -1
            time_part = parts[1]
        elif len(parts) == 3:
            # Jour spécifique: "Mon *-*-* HH:MM:SS"
            jour_systemd = parts[0]
            jours_map = {
                "Sun": 0, "Mon": 1, "Tue": 2, "Wed": 3,
                "Thu": 4, "Fri": 5, "Sat": 6
            }
            jour_semaine = jours_map.get(jour_systemd, 1)
            time_part = parts[2]
        else:
            return -1, 2, 0

        # Parser HH:MM:SS
        time_parts = time_part.split(':')
        heure = int(time_parts[0])
        minute = int(time_parts[1])

        return jour_semaine, heure, minute
    except:
        return -1, 2, 0

def ajouter_tache(tache_json):
    """Ajoute une tâche planifiée"""
    try:
        creer_dossier_systemd()
        tache = json.loads(tache_json)

        task_id = tache.get("id", "")
        task_type = tache.get("type", "MisesAJour")
        nom = tache.get("nom", "Tâche")
        description = tache.get("description", "")
        jour_semaine = tache.get("jour_semaine", -1)
        heure = tache.get("heure", 2)
        minute = tache.get("minute", 0)

        # Créer les fichiers .service et .timer
        service_name = creer_service_file(task_id, task_type, nom, description)
        timer_name = creer_timer_file(task_id, nom, jour_semaine, heure, minute)

        # Recharger systemd
        executer_commande("systemctl --user daemon-reload")

        # Activer et démarrer le timer
        result = executer_commande(f"systemctl --user enable {timer_name}")
        if not result["success"]:
            return {"success": False, "error": f"Erreur enable: {result['error']}"}

        result = executer_commande(f"systemctl --user start {timer_name}")
        if not result["success"]:
            return {"success": False, "error": f"Erreur start: {result['error']}"}

        return {"success": True, "message": "Tâche ajoutée"}
    except Exception as e:
        return {"success": False, "error": str(e)}

def supprimer_tache(task_id):
    """Supprime une tâche planifiée"""
    try:
        timer_name = f"{TUXPILOT_PREFIX}{task_id}.timer"
        service_name = f"{TUXPILOT_PREFIX}{task_id}.service"

        # Arrêter et désactiver le timer
        executer_commande(f"systemctl --user stop {timer_name}")
        executer_commande(f"systemctl --user disable {timer_name}")

        # Supprimer les fichiers
        timer_path = SYSTEMD_USER_DIR / timer_name
        service_path = SYSTEMD_USER_DIR / service_name

        if timer_path.exists():
            timer_path.unlink()
        if service_path.exists():
            service_path.unlink()

        # Recharger
        executer_commande("systemctl --user daemon-reload")

        return {"success": True, "message": "Tâche supprimée"}
    except Exception as e:
        return {"success": False, "error": str(e)}

def toggle_tache(task_id):
    """Active/Désactive une tâche"""
    try:
        timer_name = f"{TUXPILOT_PREFIX}{task_id}.timer"

        # Vérifier l'état actuel
        result = executer_commande(f"systemctl --user is-enabled {timer_name} 2>/dev/null")
        est_active = result["output"] == "enabled"

        if est_active:
            # Désactiver
            executer_commande(f"systemctl --user stop {timer_name}")
            result = executer_commande(f"systemctl --user disable {timer_name}")
        else:
            # Activer
            result = executer_commande(f"systemctl --user enable {timer_name}")
            executer_commande(f"systemctl --user start {timer_name}")

        if result["success"]:
            return {"success": True, "message": "Tâche modifiée"}
        else:
            return {"success": False, "error": result["error"]}
    except Exception as e:
        return {"success": False, "error": str(e)}

def main():
    """Point d'entrée principal"""
    if len(sys.argv) < 2:
        print(json.dumps({"error": "Action manquante"}), file=sys.stderr)
        sys.exit(1)

    action = sys.argv[1]

    try:
        if action == "lister":
            result = lister_taches()
        elif action == "ajouter":
            if len(sys.argv) < 3:
                result = {"success": False, "error": "Chemin fichier manquant"}
            else:
                chemin_fichier = sys.argv[2]
                with open(chemin_fichier, 'r') as f:
                    tache_json = f.read()
                result = ajouter_tache(tache_json)
        elif action == "supprimer":
            if len(sys.argv) < 3:
                result = {"success": False, "error": "ID manquant"}
            else:
                result = supprimer_tache(sys.argv[2])
        elif action == "toggle":
            if len(sys.argv) < 3:
                result = {"success": False, "error": "ID manquant"}
            else:
                result = toggle_tache(sys.argv[2])
        else:
            result = {"success": False, "error": f"Action inconnue: {action}"}

        print(json.dumps(result, ensure_ascii=False, indent=2))
        sys.exit(0 if result.get("success", False) else 1)
    except Exception as e:
        print(json.dumps({"error": str(e)}, ensure_ascii=False), file=sys.stderr)
        sys.exit(1)

if __name__ == "__main__":
    main()