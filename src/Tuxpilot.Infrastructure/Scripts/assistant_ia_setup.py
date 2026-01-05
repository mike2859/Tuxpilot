#!/usr/bin/env python3
# -*- coding: utf-8 -*-

import json
import subprocess
import sys
from typing import List, Dict, Any

DEFAULT_ENDPOINT = "http://127.0.0.1:11434"

def run(cmd: str, timeout=20) -> Dict[str, Any]:
    try:
        p = subprocess.run(["/bin/bash", "-lc", cmd], capture_output=True, text=True, timeout=timeout)
        return {
            "ok": p.returncode == 0,
            "code": p.returncode,
            "out": (p.stdout or "").strip(),
            "err": (p.stderr or "").strip()
        }
    except Exception as e:
        return {"ok": False, "code": -1, "out": "", "err": str(e)}

def exists(bin_name: str) -> bool:
    return run(f"command -v {bin_name} >/dev/null 2>&1")["ok"]

def get_models() -> List[str]:
    if not exists("ollama"):
        return []
    r = run("ollama list 2>/dev/null | awk 'NR>1 {print $1}'")
    if not r["ok"] or not r["out"]:
        return []
    return [x.strip() for x in r["out"].splitlines() if x.strip()]

def status(model: str) -> Dict[str, Any]:
    installed = exists("ollama")
    version = run("ollama --version 2>/dev/null")["out"] if installed else ""
    # service user
    svc_active = run("systemctl --user is-active ollama 2>/dev/null")["out"] if exists("systemctl") else ""
    svc_enabled = run("systemctl --user is-enabled ollama 2>/dev/null")["out"] if exists("systemctl") else ""
    listening = run("ss -H -ltn 2>/dev/null | grep -q ':11434' && echo yes || echo no")["out"] == "yes"

    models = get_models()
    has_model = model in models

    actions = []

    # 1) install
    if not installed:
        actions.append({
            "id": "install_ollama",
            "label": "Installer Ollama",
            "needsSudo": True,
            "safe": True,
            "command": "curl -fsSL https://ollama.com/install.sh | sh",
            "notes": "Nécessite Internet. Installe le binaire + service."
        })

    # 2) start service
    if installed and svc_active != "active":
        actions.append({
            "id": "start_ollama_user_service",
            "label": "Démarrer Ollama (service utilisateur)",
            "needsSudo": False,
            "safe": True,
            "command": "systemctl --user enable --now ollama",
            "notes": "Démarre Ollama sur 127.0.0.1:11434."
        })

    # 3) fallback manual serve
    if installed and not listening:
        actions.append({
            "id": "serve_ollama_manual",
            "label": "Lancer Ollama (manuel)",
            "needsSudo": False,
            "safe": True,
            "command": "nohup ollama serve >/tmp/ollama.log 2>&1 & disown",
            "notes": "Alternative si systemd --user indisponible."
        })

    # 4) pull model
    if installed and not has_model:
        actions.append({
            "id": "pull_model",
            "label": f"Télécharger le modèle {model}",
            "needsSudo": False,
            "safe": True,
            "command": f"ollama pull {model}",
            "notes": "Téléchargement selon la taille du modèle."
        })

    ok_ready = installed and listening and (has_model or len(models) > 0)

    # message "humain"
    msg = "OK"
    if not installed:
        msg = "Ollama n'est pas installé."
    elif not listening:
        msg = "Ollama est installé mais ne semble pas démarré (port 11434 non écoute)."
    elif not has_model and len(models) == 0:
        msg = "Ollama fonctionne mais aucun modèle n'est téléchargé."
    elif not has_model:
        msg = f"Ollama fonctionne mais le modèle '{model}' n'est pas présent."
    else:
        msg = "Ollama est prêt."

    return {
        "ok": True,
        "ready": ok_ready,
        "endpoint": DEFAULT_ENDPOINT,
        "modelRequested": model,
        "ollama": {
            "installed": installed,
            "version": version,
            "serviceActive": svc_active,
            "serviceEnabled": svc_enabled,
            "listening11434": listening,
            "models": models
        },
        "actions": actions,
        "message": msg
    }

def main():
    model = "llama3.1:8b"
    if len(sys.argv) >= 2 and sys.argv[1].strip():
        model = sys.argv[1].strip()

    out = status(model)
    print(json.dumps(out, ensure_ascii=False, indent=2))

if __name__ == "__main__":
    main()
