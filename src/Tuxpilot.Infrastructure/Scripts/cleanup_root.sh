#!/bin/bash
# cleanup_root.sh - Script de nettoyage système (nécessite root)
# Exécuté via pkexec pour éviter de demander le mot de passe plusieurs fois

set -e  # Arrêter en cas d'erreur

GESTIONNAIRE=$1
RESULTATS=()

# Fonction pour ajouter un résultat
add_result() {
    RESULTATS+=("$1")
}

echo "Début du nettoyage avec gestionnaire: $GESTIONNAIRE"

# 1. Nettoyer le cache des paquets
echo "Nettoyage du cache..."
if [ "$GESTIONNAIRE" = "dnf5" ]; then
    if dnf5 clean all 2>/dev/null; then
        add_result "Cache DNF5 nettoyé"
    else
        add_result "Erreur cache DNF5"
    fi
elif [ "$GESTIONNAIRE" = "dnf" ]; then
    if dnf clean all 2>/dev/null; then
        add_result "Cache DNF nettoyé"
    else
        add_result "Erreur cache DNF"
    fi
elif [ "$GESTIONNAIRE" = "apt" ]; then
    if apt-get clean 2>/dev/null; then
        add_result "Cache APT nettoyé"
    else
        add_result "Erreur cache APT"
    fi
fi

# 2. Supprimer les paquets orphelins
echo "Suppression des paquets orphelins..."
if [ "$GESTIONNAIRE" = "dnf5" ]; then
    if dnf5 autoremove -y 2>/dev/null; then
        add_result "Paquets orphelins supprimés (DNF5)"
    else
        add_result "Erreur paquets orphelins DNF5"
    fi
elif [ "$GESTIONNAIRE" = "dnf" ]; then
    if dnf autoremove -y 2>/dev/null; then
        add_result "Paquets orphelins supprimés (DNF)"
    else
        add_result "Erreur paquets orphelins DNF"
    fi
elif [ "$GESTIONNAIRE" = "apt" ]; then
    if apt-get autoremove -y 2>/dev/null; then
        add_result "Paquets orphelins supprimés (APT)"
    else
        add_result "Erreur paquets orphelins APT"
    fi
fi

# 3. Nettoyer les fichiers temporaires (>7 jours dans /tmp)
echo "Nettoyage des fichiers temporaires..."
if find /tmp -type f -atime +7 -delete 2>/dev/null; then
    add_result "Fichiers temporaires nettoyés"
else
    add_result "Erreur fichiers temporaires"
fi

# 4. Nettoyer les logs journald (>30 jours)
echo "Nettoyage des logs journald..."
if journalctl --vacuum-time=30d 2>/dev/null; then
    add_result "Logs journald nettoyés (>30 jours)"
else
    add_result "Erreur logs journald"
fi

# Retourner les résultats (un par ligne)
echo "=== RESULTATS ==="
for result in "${RESULTATS[@]}"; do
    echo "$result"
done

echo "Nettoyage terminé avec succès"
exit 0