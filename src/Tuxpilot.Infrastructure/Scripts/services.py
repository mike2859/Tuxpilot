#!/usr/bin/env python3
"""
Gestion des services systemd
"""

import json
import subprocess
import sys

def list_services():
    """Liste les services systemd importants"""
    try:
        # Services communs à surveiller
        important_services = [
            'sshd', 'ssh', 'httpd', 'apache2', 'nginx',
            'mysqld', 'mysql', 'mariadb', 'postgresql',
            'docker', 'firewalld', 'ufw',
            'NetworkManager', 'bluetooth', 'cups'
        ]

        services = []

        for service in important_services:
            # Vérifier si le service existe
            check_result = subprocess.run(
                ['systemctl', 'list-unit-files', f'{service}.service'],
                capture_output=True,
                text=True,
                timeout=5
            )

            if service in check_result.stdout:
                # Obtenir le statut
                status_result = subprocess.run(
                    ['systemctl', 'is-active', f'{service}.service'],
                    capture_output=True,
                    text=True,
                    timeout=5
                )

                is_active = status_result.stdout.strip()

                # Obtenir si enabled
                enabled_result = subprocess.run(
                    ['systemctl', 'is-enabled', f'{service}.service'],
                    capture_output=True,
                    text=True,
                    timeout=5
                )

                is_enabled = enabled_result.stdout.strip()

                services.append({
                    'name': service,
                    'status': is_active,
                    'enabled': is_enabled,
                    'unit': f'{service}.service'
                })

        return {
            'services': services,
            'count': len(services)
        }

    except Exception as e:
        return {
            'services': [],
            'count': 0,
            'error': str(e)
        }

def get_service_logs(service_name, lines=50):
    """Récupère les logs d'un service"""
    try:
        result = subprocess.run(
            ['journalctl', '-u', f'{service_name}.service', '-n', str(lines), '--no-pager'],
            capture_output=True,
            text=True,
            timeout=10
        )

        if result.returncode == 0:
            return {
                'service': service_name,
                'logs': result.stdout,
                'success': True
            }
        else:
            return {
                'service': service_name,
                'logs': result.stderr,
                'success': False,
                'error': 'Erreur lors de la récupération des logs'
            }

    except Exception as e:
        return {
            'service': service_name,
            'logs': '',
            'success': False,
            'error': str(e)
        }

def control_service(service_name, action):
    """
    Contrôle un service (start, stop, restart)
    Utilise pkexec pour les privilèges
    """
    try:
        valid_actions = ['start', 'stop', 'restart', 'enable', 'disable']

        if action not in valid_actions:
            return {
                'success': False,
                'service': service_name,
                'action': action,
                'message': f"Action invalide: {action}"
            }

        # Exécuter avec pkexec
        result = subprocess.run(
            ['pkexec', 'systemctl', action, f'{service_name}.service'],
            capture_output=True,
            text=True,
            timeout=60
        )

        if result.returncode == 0:
            return {
                'success': True,
                'service': service_name,
                'action': action,
                'message': f"Service {service_name} {action} avec succès"
            }
        else:
            return {
                'success': False,
                'service': service_name,
                'action': action,
                'message': result.stderr or "Erreur inconnue"
            }

    except subprocess.TimeoutExpired:
        return {
            'success': False,
            'service': service_name,
            'action': action,
            'message': "Timeout: opération trop longue"
        }
    except Exception as e:
        return {
            'success': False,
            'service': service_name,
            'action': action,
            'message': str(e)
        }

def main():
    """Point d'entrée principal"""
    try:
        if len(sys.argv) < 2:
            # Mode par défaut: lister les services
            result = list_services()
        elif sys.argv[1] == 'list':
            result = list_services()
        elif sys.argv[1] == 'logs' and len(sys.argv) >= 3:
            service = sys.argv[2]
            lines = int(sys.argv[3]) if len(sys.argv) >= 4 else 50
            result = get_service_logs(service, lines)
        elif sys.argv[1] == 'control' and len(sys.argv) >= 4:
            service = sys.argv[2]
            action = sys.argv[3]
            result = control_service(service, action)
        else:
            result = {
                'error': 'Usage: services.py [list|logs <service>|control <service> <action>]'
            }

        print(json.dumps(result, ensure_ascii=False, indent=2))

    except Exception as e:
        error_result = {
            'error': str(e)
        }
        print(json.dumps(error_result, ensure_ascii=False, indent=2))

if __name__ == "__main__":
    main()