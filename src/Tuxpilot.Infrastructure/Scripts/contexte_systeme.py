#!/usr/bin/env python3
import json
import subprocess

# Réutiliser les scripts existants
from audit_securite import get_audit_results
from info_systeme import get_system_info
# etc.

def get_full_context():
    """Collecte TOUT le contexte système pour l'IA"""

    context = {
        "audit_securite": get_audit_data(),
        "monitoring": get_monitoring_data(),
        "system_config": get_system_config(),
        "services": get_services_data(),
        "logs": get_logs_data(),
        "network": get_network_data(),
        "timestamp": datetime.now().isoformat()
    }

    return context

if __name__ == "__main__":
    try:
        context = get_full_context()
        print(json.dumps(context, indent=2))
    except Exception as e:
        print(json.dumps({"error": str(e)}), file=sys.stderr)
        sys.exit(1)