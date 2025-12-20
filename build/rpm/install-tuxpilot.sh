#!/bin/bash
# Installation automatique de Tuxpilot sur Fedora
# Résout automatiquement le problème lttng-ust

set -e

echo ""
echo "╔══════════════════════════════════════════╗"
echo "║   Installation Tuxpilot pour Fedora     ║"
echo "╚══════════════════════════════════════════╝"
echo ""

# Vérifier qu'on est sur Fedora
if [ ! -f /etc/fedora-release ]; then
    echo "❌ Ce script est conçu pour Fedora uniquement"
    echo "   Pour d'autres distributions, téléchargez l'AppImage"
    exit 1
fi

# Vérifier que le RPM existe
if [ ! -f "tuxpilot-0.9.0-1.fc41.x86_64.rpm" ]; then
    echo "❌ Fichier tuxpilot-0.9.0-1.fc41.x86_64.rpm non trouvé"
    echo ""
    echo "Téléchargez le fichier depuis :"
    echo "  https://lechevalierdelacyber.fr/download"
    echo ""
    echo "Puis placez-le dans le même dossier que ce script et relancez :"
    echo "  ./install-tuxpilot.sh"
    exit 1
fi

echo "✅ Fichier RPM trouvé ($(du -h tuxpilot-0.9.0-1.fc41.x86_64.rpm | cut -f1))"
echo ""

# Installer les dépendances nécessaires
echo "📥 Vérification des dépendances..."

# lttng-ust
if ! rpm -q lttng-ust &>/dev/null; then
    echo "   Installation de lttng-ust..."
    sudo dnf install -y lttng-ust
    echo "   ✅ lttng-ust installé"
else
    echo "   ✅ lttng-ust déjà installé"
fi

# python3-psutil
if ! rpm -q python3-psutil &>/dev/null; then
    echo "   Installation de python3-psutil..."
    sudo dnf install -y python3-psutil
    echo "   ✅ python3-psutil installé"
else
    echo "   ✅ python3-psutil déjà installé"
fi

echo ""

# Créer le lien symbolique de compatibilité si nécessaire
if [ -f /usr/lib64/liblttng-ust.so.1 ] && [ ! -f /usr/lib64/liblttng-ust.so.0 ]; then
    echo "🔧 Création du lien de compatibilité..."
    sudo ln -sf /usr/lib64/liblttng-ust.so.1 /usr/lib64/liblttng-ust.so.0
    echo "   ✅ Lien créé : liblttng-ust.so.0 → liblttng-ust.so.1"
    echo ""
fi

# Installer Tuxpilot
echo "📦 Installation de Tuxpilot..."
sudo rpm -ivh --nodeps tuxpilot-0.9.0-1.fc41.x86_64.rpm

echo ""
echo "╔══════════════════════════════════════════╗"
echo "║  ✅ Installation terminée avec succès !  ║"
echo "╚══════════════════════════════════════════╝"
echo ""
echo "🚀 Pour lancer Tuxpilot :"
echo ""
echo "   • Menu Applications → Système → Tuxpilot"
echo "   • Ou tapez : tuxpilot"
echo ""
echo "📚 Documentation : https://lechevalierdelacyber.fr/documentation"
echo "💬 Support : support@lechevalierdelacyber.fr"
echo ""
