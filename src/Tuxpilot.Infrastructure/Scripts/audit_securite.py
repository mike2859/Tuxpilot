#!/usr/bin/env python3
# -*- coding: utf-8 -*-

import json
import subprocess
import sys
import re
from pathlib import Path

def executer_commande(cmd, timeout=10):
    try:
        r = subprocess.run(cmd, shell=True, capture_output=True, text=True, timeout=timeout)
        return {"success": r.returncode == 0, "output": (r.stdout or "").strip(), "error": (r.stderr or "").strip(), "code": r.returncode}
    except Exception as e:
        return {"success": False, "output": "", "error": str(e), "code": -1}

def cmd_exists(name: str) -> bool:
    return executer_commande(f"command -v {name} >/dev/null 2>&1")["success"]

def make_verif(id_, nom, description, categorie, impact, points_max=20):
    return {
        "id": id_,
        "nom": nom,
        "description": description,
        "categorie": categorie,
        "impact": impact,
        "points_max": points_max,
        "reussie": True,
        "niveau": "Aucun",
        "points": points_max,
        "details": "",
        "preuve": "",
        "recommandation": "",
        "commande_correction": None,
        "auto_fix_safe": False
    }

def fail(verif, niveau, points, details, recommandation, preuve="", commande=None, auto_fix_safe=False):
    verif["reussie"] = False
    verif["niveau"] = niveau
    verif["points"] = max(0, min(points, verif["points_max"]))
    verif["details"] = details
    verif["recommandation"] = recommandation
    verif["preuve"] = preuve
    verif["commande_correction"] = commande
    verif["auto_fix_safe"] = auto_fix_safe
    return verif

def ok(verif, details, recommandation, preuve=""):
    verif["reussie"] = True
    verif["niveau"] = "Aucun"
    verif["points"] = verif["points_max"]
    verif["details"] = details
    verif["recommandation"] = recommandation
    verif["preuve"] = preuve
    return verif

def info(verif, details, recommandation, preuve=""):
    # "Info" = ne pas pénaliser : faible risque mais score max
    verif["reussie"] = True
    verif["niveau"] = "Faible"
    verif["points"] = verif["points_max"]
    verif["details"] = details
    verif["recommandation"] = recommandation
    verif["preuve"] = preuve
    return verif

# ---------------- CHECKS ----------------

def verifier_firewall():
    v = make_verif(
        "firewall",
        "Firewall",
        "Vérification de l'état du pare-feu système",
        "Réseau",
        "Limite les connexions entrantes non désirées."
    )

    ufw = executer_commande("systemctl is-active ufw 2>/dev/null")
    firewalld = executer_commande("systemctl is-active firewalld 2>/dev/null")

    if ufw["output"] == "active":
        st = executer_commande("ufw status 2>/dev/null | head -n 30")
        return ok(v, "UFW est actif", "✅ Pare-feu activé", preuve=st["output"])
    if firewalld["output"] == "active":
        zones = executer_commande("firewall-cmd --get-active-zones 2>/dev/null")
        ports = executer_commande("firewall-cmd --list-ports 2>/dev/null")
        preuve = f"{zones['output']}\nports: {ports['output']}".strip()
        return ok(v, "Firewalld est actif", "✅ Pare-feu activé", preuve=preuve)

    return fail(
        v, "Eleve", 5,
        "Aucun pare-feu actif détecté",
        "Activez un pare-feu (UFW ou firewalld).",
        preuve="systemctl is-active ufw/firewalld = inactive",
        commande="pkexec ufw enable || pkexec systemctl enable --now firewalld",
        auto_fix_safe=True
    )

def verifier_selinux():
    v = make_verif(
        "selinux",
        "SELinux",
        "Vérification du mode SELinux",
        "Durcissement",
        "Réduit fortement l'impact d'un service compromis."
    )

    if cmd_exists("getenforce"):
        r = executer_commande("getenforce 2>/dev/null")
        mode = r["output"]
        if mode == "Enforcing":
            return ok(v, "SELinux actif (Enforcing)", "✅ Très bon niveau de durcissement", preuve=mode)
        if mode == "Permissive":
            return fail(v, "Moyen", 12, "SELinux en Permissive", "Passez SELinux en Enforcing si possible.", preuve=mode)
        if mode == "Disabled":
            return fail(v, "Eleve", 8, "SELinux désactivé", "Activez SELinux si votre distribution le supporte.", preuve=mode)
        return info(v, "SELinux : état inconnu", "Vérifiez manuellement SELinux.", preuve=mode)

    return info(v, "SELinux non disponible", "Non applicable (distribution sans SELinux).", preuve="getenforce absent")

def verifier_fail2ban():
    v = make_verif(
        "fail2ban",
        "Fail2Ban",
        "Protection contre les tentatives de brute-force (SSH, etc.)",
        "Accès",
        "Bloque automatiquement les IP après tentatives de connexion."
    )

    # installed?
    installed = executer_commande("rpm -q fail2ban 2>/dev/null || dpkg -l fail2ban 2>/dev/null")
    active = executer_commande("systemctl is-active fail2ban 2>/dev/null")
    enabled = executer_commande("systemctl is-enabled fail2ban 2>/dev/null")

    if "fail2ban" in installed["output"] and active["output"] == "active":
        return ok(v, "Fail2Ban actif", "✅ Protection brute-force en place", preuve=f"{enabled['output']} / {active['output']}")
    if "fail2ban" in installed["output"]:
        return fail(v, "Faible", 15, "Fail2Ban installé mais inactif", "Activez Fail2Ban si vous exposez SSH.", preuve=f"{enabled['output']} / {active['output']}",
                   commande="pkexec systemctl enable --now fail2ban", auto_fix_safe=True)

    return info(v, "Fail2Ban non installé", "Optionnel (recommandé si SSH exposé).", preuve="package absent")

def verifier_ssh():
    v = make_verif(
        "ssh",
        "SSH",
        "Vérification de l'exposition et configuration SSH",
        "Accès",
        "SSH exposé mal configuré = risque d'accès non autorisé."
    )

    # écoute ?
    ss = executer_commande("ss -H -tulnp 2>/dev/null | grep -E ':(22)\\s' || true")
    ssh_listen = ss["output"].strip() != ""

    # config lisible ?
    conf = executer_commande("cat /etc/ssh/sshd_config 2>/dev/null")
    if not conf["success"] or conf["output"] == "":
        if ssh_listen:
            return fail(v, "Moyen", 10, "SSH écoute mais sshd_config inaccessible", "Vérifiez le service SSH.", preuve=ss["output"])
        return ok(v, "SSH non installé / non exposé", "✅ Aucun risque SSH (non utilisé)", preuve="pas d'écoute sur le port 22")

    config = conf["output"]
    problemes = []
    points_perdus = 0

    if re.search(r"^\s*PermitRootLogin\s+yes\s*$", config, flags=re.M):
        problemes.append("PermitRootLogin yes")
        points_perdus += 8

    if re.search(r"^\s*PasswordAuthentication\s+yes\s*$", config, flags=re.M):
        problemes.append("PasswordAuthentication yes")
        points_perdus += 6

    if re.search(r"^\s*Port\s+22\s*$", config, flags=re.M) or not re.search(r"^\s*Port\s+\d+\s*$", config, flags=re.M):
        problemes.append("Port 22 (défaut)")
        points_perdus += 3

    if not ssh_listen:
        # SSH configuré mais pas exposé : info, pas pénalité forte
        return info(v, "SSH présent mais non exposé", "OK si vous ne l'utilisez pas.", preuve="pas d'écoute sur :22")

    if problemes:
        niveau = "Eleve" if points_perdus >= 10 else "Moyen"
        return fail(v, niveau, v["points_max"] - points_perdus, " / ".join(problemes),
                    "Désactivez root login, privilégiez clés SSH, désactivez mot de passe.",
                    preuve="ss écoute sur 22 + sshd_config",
                    commande=None)
    return ok(v, "SSH exposé et correctement configuré", "✅ SSH durci", preuve="ss écoute sur 22 + config OK")

def verifier_updates():
    v = make_verif(
        "updates",
        "Mises à jour",
        "Vérification des mises à jour disponibles (sécurité si possible)",
        "Système",
        "Des correctifs manquants augmentent le risque d'exploitation."
    )

    # Fedora/RHEL: security updates
    if cmd_exists("dnf"):
        sec = executer_commande("dnf -q updateinfo list security --available 2>/dev/null | wc -l")
        try:
            n = int(sec["output"] or "0")
            if n == 0:
                return ok(v, "Système à jour (sécurité)", "✅ Aucun correctif de sécurité en attente", preuve="dnf updateinfo security = 0")
            if n <= 5:
                return fail(v, "Faible", 15, f"{n} correctif(s) sécurité dispo", "Installez les mises à jour de sécurité.",
                            preuve="dnf updateinfo list security --available",
                            commande="pkexec dnf update -y", auto_fix_safe=True)
            return fail(v, "Moyen", 10, f"{n} correctifs sécurité en attente", "Installez rapidement les mises à jour.",
                        preuve="dnf updateinfo list security --available",
                        commande="pkexec dnf update -y", auto_fix_safe=True)
        except:
            pass

    # Debian/Ubuntu fallback: upgradable (pas uniquement security)
    if cmd_exists("apt"):
        up = executer_commande("apt list --upgradable 2>/dev/null | sed 1d | wc -l")
        try:
            n = int(up["output"] or "0")
            if n == 0:
                return ok(v, "Système à jour", "✅ Aucune mise à jour en attente", preuve="apt list --upgradable = 0")
            if n <= 10:
                return fail(v, "Faible", 15, f"{n} mise(s) à jour dispo", "Installez les mises à jour.",
                            preuve="apt list --upgradable",
                            commande="pkexec apt update && pkexec apt upgrade -y", auto_fix_safe=True)
            return fail(v, "Moyen", 10, f"{n} mises à jour en attente", "Installez rapidement les mises à jour.",
                        preuve="apt list --upgradable",
                        commande="pkexec apt update && pkexec apt upgrade -y", auto_fix_safe=True)
        except:
            pass

    return info(v, "Vérification impossible", "Vérifiez manuellement les mises à jour.", preuve="dnf/apt indisponible")

def verifier_ports_exposes():
    v = make_verif(
        "ports",
        "Ports & services",
        "Détection des ports en écoute et exposition réseau",
        "Réseau",
        "Plus de services exposés = surface d'attaque plus grande."
    )

    r = executer_commande("ss -H -tulnp 2>/dev/null || true", timeout=12)
    if not r["output"]:
        return info(v, "Vérification impossible", "Vérifiez manuellement avec ss.", preuve=r["error"])

    lignes = [l.strip() for l in r["output"].splitlines() if l.strip()]
    exposed = []
    local = []

    for l in lignes:
        # proto state recv-q send-q local:port peer users...
        parts = l.split()
        if len(parts) < 5:
            continue
        local_addr = parts[4]  # ex: 0.0.0.0:631 ou [::]:22
        users = " ".join(parts[6:]) if len(parts) >= 7 else ""
        if ":" not in local_addr:
            continue
        addr = local_addr.rsplit(":", 1)[0]
        port = local_addr.rsplit(":", 1)[1]
        item = f"{port} ({users})"
        if addr in ("127.0.0.1", "::1", "localhost"):
            local.append(item)
        elif addr.startswith("127.") or addr.startswith("[::1]"):
            local.append(item)
        else:
            exposed.append(item)

    preuve = ""
    if exposed:
        preuve += "Exposés:\n" + "\n".join(exposed[:12])
    if local:
        preuve += ("\n\nLocaux:\n" if preuve else "Locaux:\n") + "\n".join(local[:12])

    if not exposed:
        return ok(v, f"{len(local)} service(s) en écoute locale uniquement", "✅ Rien d'exposé sur le réseau", preuve=preuve)

    # pénalité selon volume exposé
    n = len(exposed)
    if n <= 3:
        return fail(v, "Faible", 15, f"{n} port(s) exposé(s)", "Vérifiez que ces services sont nécessaires.", preuve=preuve)
    if n <= 8:
        return fail(v, "Moyen", 12, f"{n} ports exposés", "Désactivez les services non indispensables.", preuve=preuve)
    return fail(v, "Eleve", 8, f"{n} ports exposés (beaucoup)", "Réduisez la surface d'attaque (services inutiles).", preuve=preuve)

def verifier_permissions():
    v = make_verif(
        "perms",
        "Permissions système",
        "Vérification des permissions des fichiers critiques",
        "Système",
        "Des permissions trop ouvertes facilitent l'escalade."
    )

    problemes = []
    passwd = executer_commande("stat -c '%a' /etc/passwd 2>/dev/null")
    if passwd["output"] and passwd["output"] != "644":
        problemes.append(f"/etc/passwd={passwd['output']}")

    shadow = executer_commande("stat -c '%a' /etc/shadow 2>/dev/null")
    if shadow["output"] and shadow["output"] not in ["000", "640", "400"]:
        problemes.append(f"/etc/shadow={shadow['output']}")

    if problemes:
        return fail(v, "Moyen", 12, "Permissions incorrectes", "Corrigez les permissions des fichiers système.", preuve=", ".join(problemes))
    return ok(v, "Permissions correctes", "✅ Fichiers système correctement protégés", preuve="passwd=644, shadow ok")

def verifier_sudo_users():
    v = make_verif(
        "sudo_users",
        "Utilisateurs sudo",
        "Détection des comptes ayant des privilèges admin",
        "Accès",
        "Trop d'admins = risque accru en cas de compromission."
    )

    wheel = executer_commande("getent group wheel 2>/dev/null")
    sudo = executer_commande("getent group sudo 2>/dev/null")
    users = []

    def extract(line):
        # group:x:gid:user1,user2
        if ":" not in line:
            return []
        parts = line.split(":")
        if len(parts) >= 4 and parts[3].strip():
            return [u.strip() for u in parts[3].split(",") if u.strip()]
        return []

    users += extract(wheel["output"])
    users += extract(sudo["output"])
    users = sorted(set(users))

    preuve = f"wheel: {wheel['output']}\nsudo: {sudo['output']}".strip()

    if len(users) <= 1:
        return ok(v, "Un seul admin détecté", "✅ Bonne pratique", preuve=preuve)
    if len(users) <= 3:
        return fail(v, "Faible", 15, f"{len(users)} admins détectés", "Vérifiez que chaque compte admin est nécessaire.", preuve=preuve)
    return fail(v, "Moyen", 12, f"{len(users)} admins détectés", "Réduisez le nombre de comptes admin si possible.", preuve=preuve)

def verifier_root_locked():
    v = make_verif(
        "root_lock",
        "Compte root",
        "Détection du verrouillage du compte root",
        "Accès",
        "Un root avec mot de passe augmente le risque d'accès direct."
    )

    r = executer_commande("passwd -S root 2>/dev/null || true")
    if not r["output"]:
        return info(v, "Vérification impossible", "Vérifiez manuellement l'état du compte root.", preuve=r["error"])

    # Debian: root L/P ; Fedora: similar
    # output example: root L 2024-... ...
    status = r["output"].split()[1] if len(r["output"].split()) >= 2 else ""
    if status == "L":
        return ok(v, "Compte root verrouillé", "✅ Recommandé", preuve=r["output"])
    if status == "P":
        return fail(v, "Faible", 15, "Compte root avec mot de passe", "Préférez sudo et verrouillez root si possible.", preuve=r["output"])
    return info(v, "État root inconnu", "Vérifiez manuellement.", preuve=r["output"])

def verifier_logs_persistants():
    v = make_verif(
        "logs",
        "Logs système",
        "Vérifie si les logs sont persistants (journald)",
        "Système",
        "Des logs persistants aident à détecter/diagnostiquer une intrusion."
    )

    exists = Path("/var/log/journal").is_dir()
    if exists:
        st = executer_commande("stat -c '%a %U:%G' /var/log/journal 2>/dev/null")
        return ok(v, "Logs persistants activés", "✅ Bon pour l'audit/forensic", preuve=f"/var/log/journal présent\n{st['output']}")
    return fail(
        v, "Faible", 15,
        "Logs journald non persistants",
        "Activez /var/log/journal pour conserver l'historique après reboot.",
        preuve="/var/log/journal absent",
        commande="pkexec mkdir -p /var/log/journal && pkexec systemd-tmpfiles --create --prefix /var/log/journal",
        auto_fix_safe=True
    )

def verifier_verrouillage_ecran():
    v = make_verif(
        "screen_lock",
        "Verrouillage écran",
        "Vérifie la présence d'un verrouillage automatique (GNOME)",
        "Confidentialité",
        "Évite l'accès non autorisé si vous vous absentez."
    )

    if not cmd_exists("gsettings"):
        return info(v, "Non applicable", "Environnement non GNOME ou gsettings absent.", preuve="gsettings absent")

    lock_enabled = executer_commande("gsettings get org.gnome.desktop.screensaver lock-enabled 2>/dev/null")
    idle_delay = executer_commande("gsettings get org.gnome.desktop.session idle-delay 2>/dev/null")  # seconds
    lock_delay = executer_commande("gsettings get org.gnome.desktop.screensaver lock-delay 2>/dev/null")  # seconds

    preuve = f"lock-enabled={lock_enabled['output']}, idle-delay={idle_delay['output']}, lock-delay={lock_delay['output']}"
    if lock_enabled["output"] != "true":
        return fail(v, "Moyen", 12, "Verrouillage écran désactivé", "Activez le verrouillage automatique.", preuve=preuve,
                    commande="gsettings set org.gnome.desktop.screensaver lock-enabled true", auto_fix_safe=True)

    # parse idle-delay "uint32 300"
    m = re.search(r"(\d+)", idle_delay["output"] or "")
    idle = int(m.group(1)) if m else 0
    if idle == 0:
        return fail(v, "Faible", 15, "Verrouillage auto non configuré (idle=0)", "Définissez un délai d'inactivité (ex: 300s).", preuve=preuve,
                    commande="gsettings set org.gnome.desktop.session idle-delay 300", auto_fix_safe=True)

    if idle > 900:
        return fail(v, "Faible", 15, f"Délai long ({idle}s)", "Réduisez le délai de verrouillage (ex: 300s).", preuve=preuve)
    return ok(v, f"Verrouillage auto activé ({idle}s)", "✅ Bon réglage", preuve=preuve)

def verifier_chiffrement_disque():
    v = make_verif(
        "disk_crypto",
        "Chiffrement disque",
        "Détecte la présence d'un chiffrement (LUKS/crypt)",
        "Confidentialité",
        "Protège les données en cas de vol/perte de la machine."
    )

    r = executer_commande("lsblk -o NAME,TYPE,FSTYPE,MOUNTPOINT -r 2>/dev/null || true")
    if not r["output"]:
        return info(v, "Vérification impossible", "Vérifiez manuellement lsblk.", preuve=r["error"])

    crypt = []
    for line in r["output"].splitlines():
        if " crypt " in f" {line} " or " luks " in f" {line.lower()} ":
            crypt.append(line)

    if crypt:
        return ok(v, "Chiffrement détecté", "✅ Données mieux protégées", preuve="\n".join(crypt[:10]))
    return fail(v, "Moyen", 12, "Aucun chiffrement détecté", "Considérez le chiffrement (surtout laptop).", preuve="aucune ligne crypt/luks dans lsblk")

def verifier_flatpak_sandbox():
    v = make_verif(
        "flatpak",
        "Flatpak sandbox",
        "Analyse rapide des permissions Flatpak (si installé)",
        "Durcissement",
        "Certaines permissions (filesystem=host) réduisent l'isolation."
    )

    if not cmd_exists("flatpak"):
        return info(v, "Flatpak non installé", "Non applicable.", preuve="flatpak absent")

    apps = executer_commande("flatpak list --app --columns=application 2>/dev/null")
    if not apps["output"]:
        return info(v, "Aucune app Flatpak", "Rien à analyser.", preuve="flatpak list vide")

    # Simple: regarder les overrides globaux (si filesystem=host)
    ov = executer_commande("flatpak override --show 2>/dev/null || true")
    if "--filesystem=host" in ov["output"]:
        return fail(v, "Faible", 15, "Override global permissif", "Évitez --filesystem=host globalement.", preuve=ov["output"])
    return ok(v, "Isolation Flatpak OK (rapide)", "✅ Aucun override global dangereux détecté", preuve=ov["output"] or "override vide")

def verifier_services_auto_updates():
    v = make_verif(
        "auto_updates",
        "Mises à jour automatiques",
        "Détecte si un mécanisme d'auto-update est activé",
        "Système",
        "Réduit le temps d'exposition aux vulnérabilités connues."
    )

    # Fedora
    dnf_timer = executer_commande("systemctl is-enabled dnf-automatic.timer 2>/dev/null || true")
    if dnf_timer["output"] == "enabled":
        return ok(v, "dnf-automatic activé", "✅ Auto-updates en place", preuve="dnf-automatic.timer enabled")

    # Debian/Ubuntu
    ua = executer_commande("systemctl is-enabled unattended-upgrades 2>/dev/null || true")
    if ua["output"] == "enabled":
        return ok(v, "unattended-upgrades activé", "✅ Auto-updates en place", preuve="unattended-upgrades enabled")

    return info(v, "Auto-updates non détectées", "Optionnel mais recommandé sur machines non critiques.", preuve="dnf-automatic / unattended-upgrades non activés")

def verifier_secure_boot():
    v = make_verif(
        "secure_boot",
        "Secure Boot",
        "Vérifie l'état du Secure Boot (si outil dispo)",
        "Durcissement",
        "Limite les bootkits / modifications au démarrage."
    )
    if not cmd_exists("mokutil"):
        return info(v, "Vérification impossible", "Installez mokutil pour vérifier Secure Boot.", preuve="mokutil absent")
    r = executer_commande("mokutil --sb-state 2>/dev/null || true")
    if "enabled" in (r["output"].lower()):
        return ok(v, "Secure Boot activé", "✅ Bon signal de durcissement", preuve=r["output"])
    if "disabled" in (r["output"].lower()):
        return fail(v, "Faible", 15, "Secure Boot désactivé", "Activez-le dans l'UEFI si possible.", preuve=r["output"])
    return info(v, "État inconnu", "Vérifiez dans l'UEFI.", preuve=r["output"])

def verifier_time_sync():
    v = make_verif(
        "time_sync",
        "Synchronisation horaire",
        "Vérifie la synchro NTP",
        "Système",
        "Une heure correcte est essentielle pour la sécurité (TLS, logs)."
    )
    r = executer_commande("timedatectl show -p NTPSynchronized --value 2>/dev/null || true")
    if r["output"] == "yes":
        return ok(v, "NTP synchronisé", "✅ OK", preuve="timedatectl NTPSynchronized=yes")
    if r["output"] == "no":
        return fail(v, "Faible", 15, "NTP non synchronisé", "Activez la synchronisation horaire.", preuve="timedatectl NTPSynchronized=no",
                    commande="pkexec timedatectl set-ntp true", auto_fix_safe=True)
    return info(v, "Vérification impossible", "Vérifiez timedatectl.", preuve=r["output"] or r["error"])

# ---------------- REPORT ----------------

def executer_audit():
    verifications = [
        verifier_firewall(),
        verifier_selinux(),
        verifier_fail2ban(),
        verifier_ssh(),
        verifier_updates(),
        verifier_ports_exposes(),
        verifier_permissions(),
        verifier_sudo_users(),
        verifier_root_locked(),
        verifier_logs_persistants(),
        verifier_verrouillage_ecran(),
        verifier_chiffrement_disque(),
        verifier_flatpak_sandbox(),
        verifier_services_auto_updates(),
        verifier_secure_boot(),
        verifier_time_sync()
    ]

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
