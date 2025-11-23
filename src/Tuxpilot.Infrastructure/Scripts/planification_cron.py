#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Script de gestion de la planification des tâches avec cron
Gère les cron jobs pour Tuxpilot
"""

import json
import subprocess
import sys
import os
from datetime import datetime

CRON_COMMENT = "# TUXPILOT"
CRON_MARKER = "tuxpilot_"

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

def obtenir_crontab():
    """Récupère le contenu du crontab"""
    result = executer_commande("crontab -l 2>/dev/null")
    if result["success"] and result["output"]:
        return result["output"].split('\n')
    return []

def sauvegarder_crontab(lignes):
    """Sauvegarde le crontab"""
    try:
        temp_file = "/tmp/tuxpilot_cron.tmp"

        # Filtrer les lignes vides
        lignes_filtrees = [l for l in lignes if l.strip()]

        with open(temp_file, 'w') as f:
            contenu = '\n'.join(lignes_filtrees)
            if contenu:
                contenu += '\n'
            f.write(contenu)

        # Installer le nouveau crontab
        result = executer_commande(f"crontab {temp_file}")

        # Nettoyer
        try:
            os.remove(temp_file)
        except:
            pass

        return result["success"]
    except Exception as e:
        print(f"Exception sauvegarde crontab: {e}", file=sys.stderr)
        return False

def lister_taches():
    """Liste toutes les tâches Tuxpilot planifiées"""
    lignes = obtenir_crontab()
    taches = []

    i = 0
    while i < len(lignes):
        ligne = lignes[i]
        if CRON_COMMENT in ligne and i + 1 < len(lignes):
            # La ligne suivante contient le cron job
            cron_line = lignes[i + 1]
            if CRON_MARKER in cron_line:
                tache = parser_ligne_cron(cron_line, ligne)
                if tache:
                    taches.append(tache)
            i += 2
        else:
            i += 1

    return {"taches": taches, "success": True}

def parser_ligne_cron(cron_line, comment_line):
    """Parse une ligne cron et extrait les infos"""
    try:
        # Extraire l'ID depuis le commentaire
        # Format: # TUXPILOT: id=xxx type=xxx nom=xxx
        parts = comment_line.split()
        if len(parts) < 2:
            return None

        info = {}
        for part in parts[1:]:  # Skip "# TUXPILOT:"
            if '=' in part:
                key, value = part.split('=', 1)
                info[key] = value

        # Parser la ligne cron
        activee = not cron_line.startswith('#')
        if not activee:
            cron_line = cron_line[1:].strip()

        parts = cron_line.split()
        if len(parts) < 5:
            return None

        minute = parts[0]
        heure = parts[1]
        jour = parts[4]

        # Convertir en format Tuxpilot
        jour_semaine = -1 if jour == '*' else int(jour)

        return {
            "id": info.get("id", ""),
            "type": info.get("type", "MisesAJour"),
            "nom": info.get("nom", "Tâche").replace('_', ' '),
            "description": info.get("desc", "").replace('_', ' '),
            "jour_semaine": jour_semaine,
            "heure": int(heure),
            "minute": int(minute),
            "activee": activee,
            "date_creation": datetime.now().isoformat()
        }
    except Exception as e:
        print(f"Erreur parsing: {e}", file=sys.stderr)
        return None

def ajouter_tache(tache_json):
    """Ajoute une tâche planifiée"""
    try:
        tache = json.loads(tache_json)

        # Générer la ligne cron
        minute = tache.get("minute", 0)
        heure = tache.get("heure", 2)
        jour = tache.get("jour_semaine", -1)

        # Expression cron
        jour_cron = "*" if jour == -1 else str(jour)
        cron_expr = f"{minute} {heure} * * {jour_cron}"

        # Commande à exécuter
        task_id = tache.get("id", "")
        task_type = tache.get("type", "MisesAJour")

        # Commande factice pour l'instant
        commande = f"echo '{CRON_MARKER}{task_id}' >> /tmp/tuxpilot_tasks.log"

        # Commentaire avec métadonnées
        nom_safe = tache.get("nom", "Tâche").replace(' ', '_')
        desc_safe = tache.get("description", "").replace(' ', '_')
        comment = f"{CRON_COMMENT}: id={task_id} type={task_type} nom={nom_safe} desc={desc_safe}"

        # Ligne cron complète
        cron_line = f"{cron_expr} {commande}"

        # Ajouter au crontab
        lignes = obtenir_crontab()
        lignes.append(comment)
        lignes.append(cron_line)

        if sauvegarder_crontab(lignes):
            return {"success": True, "message": "Tâche ajoutée"}
        else:
            return {"success": False, "error": "Erreur sauvegarde crontab"}
    except Exception as e:
        return {"success": False, "error": str(e)}

def supprimer_tache(task_id):
    """Supprime une tâche planifiée"""
    try:
        lignes = obtenir_crontab()
        nouvelles_lignes = []
        skip_next = False

        for i, ligne in enumerate(lignes):
            if skip_next:
                skip_next = False
                continue

            if CRON_COMMENT in ligne and f"id={task_id}" in ligne:
                skip_next = True
                continue

            nouvelles_lignes.append(ligne)

        if sauvegarder_crontab(nouvelles_lignes):
            return {"success": True, "message": "Tâche supprimée"}
        else:
            return {"success": False, "error": "Erreur sauvegarde"}
    except Exception as e:
        return {"success": False, "error": str(e)}

def toggle_tache(task_id):
    """Active/Désactive une tâche"""
    try:
        lignes = obtenir_crontab()
        modifie = False

        for i, ligne in enumerate(lignes):
            if CRON_COMMENT in ligne and f"id={task_id}" in ligne:
                # Modifier la ligne suivante
                if i + 1 < len(lignes):
                    cron_line = lignes[i + 1]
                    if cron_line.startswith('#'):
                        # Activer (enlever le #)
                        lignes[i + 1] = cron_line[1:].strip()
                    else:
                        # Désactiver (ajouter #)
                        lignes[i + 1] = f"# {cron_line}"
                    modifie = True
                break

        if modifie and sauvegarder_crontab(lignes):
            return {"success": True, "message": "Tâche modifiée"}
        else:
            return {"success": False, "error": "Tâche non trouvée"}
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