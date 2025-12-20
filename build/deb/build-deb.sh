#!/bin/bash
# Script de création package .deb pour Tuxpilot
# Usage: cd build/deb && ./build-deb.sh

set -e  # Arrêter si erreur

echo "🚀 Building Tuxpilot .deb package..."

# Variables
APP_NAME="tuxpilot"
APP_VERSION="0.9.0"
ARCH="amd64"
BUILD_DIR="deb-build"
PACKAGE_NAME="${APP_NAME}_${APP_VERSION}_${ARCH}"
DEB_ROOT="$BUILD_DIR/$PACKAGE_NAME"

# Chemin racine du projet (2 niveaux au-dessus)
PROJECT_ROOT="$(cd "$(dirname "$0")/../.." && pwd)"

# Couleurs
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m'

echo -e "${BLUE}📁 Projet : $PROJECT_ROOT${NC}"
echo ""

echo -e "${BLUE}📦 Étape 1/6 : Nettoyage${NC}"
rm -rf "$BUILD_DIR"
mkdir -p "$DEB_ROOT"

echo -e "${BLUE}📦 Étape 2/6 : Publication .NET${NC}"
mkdir -p "$DEB_ROOT/opt/tuxpilot"
dotnet publish "$PROJECT_ROOT/src/Tuxpilot.UI/Tuxpilot.UI.csproj" \
    -c Release \
    -r linux-x64 \
    --self-contained true \
    -p:PublishSingleFile=false \
    -p:PublishTrimmed=false \
    -o "$DEB_ROOT/opt/tuxpilot"

echo -e "${BLUE}📦 Étape 3/6 : Copie des scripts Python${NC}"
mkdir -p "$DEB_ROOT/opt/tuxpilot/Scripts"
cp "$PROJECT_ROOT/src/Tuxpilot.Infrastructure/Scripts/validate_license.py" \
   "$DEB_ROOT/opt/tuxpilot/Scripts/"
chmod +x "$DEB_ROOT/opt/tuxpilot/Scripts/validate_license.py"

echo -e "${BLUE}📦 Étape 4/6 : Création des fichiers système${NC}"

# Créer le launcher dans /usr/bin
mkdir -p "$DEB_ROOT/usr/bin"
cat > "$DEB_ROOT/usr/bin/tuxpilot" << 'EOF'
#!/bin/bash
export TUXPILOT_SCRIPTS_PATH="/opt/tuxpilot/Scripts"
cd /opt/tuxpilot
exec ./Tuxpilot.UI "$@"
EOF
chmod +x "$DEB_ROOT/usr/bin/tuxpilot"

# Créer le fichier .desktop
mkdir -p "$DEB_ROOT/usr/share/applications"
cat > "$DEB_ROOT/usr/share/applications/tuxpilot.desktop" << EOF
[Desktop Entry]
Name=Tuxpilot
Comment=Assistant Linux pour débutants et entreprises
Exec=/usr/bin/tuxpilot
Icon=tuxpilot
Type=Application
Categories=System;Utility;Settings;
Terminal=false
Keywords=linux;security;audit;maintenance;
StartupNotify=true
EOF

# Copier les icônes
if [ ! -f "icon.png" ]; then
    echo -e "${YELLOW}⚠️  Icône non trouvée dans build/deb/${NC}"
    exit 1
fi

echo "✅ Icône trouvée"
mkdir -p "$DEB_ROOT/usr/share/icons/hicolor/256x256/apps"
cp icon.png "$DEB_ROOT/usr/share/icons/hicolor/256x256/apps/tuxpilot.png"

if [ -f "icon-512.png" ]; then
    mkdir -p "$DEB_ROOT/usr/share/icons/hicolor/512x512/apps"
    cp icon-512.png "$DEB_ROOT/usr/share/icons/hicolor/512x512/apps/tuxpilot.png"
fi

if [ -f "icon-128.png" ]; then
    mkdir -p "$DEB_ROOT/usr/share/icons/hicolor/128x128/apps"
    cp icon-128.png "$DEB_ROOT/usr/share/icons/hicolor/128x128/apps/tuxpilot.png"
fi

echo -e "${BLUE}📦 Étape 5/6 : Création fichiers DEBIAN${NC}"

# Créer le répertoire DEBIAN
mkdir -p "$DEB_ROOT/DEBIAN"

# Fichier control
cat > "$DEB_ROOT/DEBIAN/control" << EOF
Package: tuxpilot
Version: $APP_VERSION
Section: utils
Priority: optional
Architecture: $ARCH
Maintainer: Le Chevalier de la Cyber <contact@lechevalierdelacyber.fr>
Homepage: https://lechevalierdelacyber.fr
Description: Assistant Linux pour débutants et entreprises
 Tuxpilot est un assistant desktop pour Linux conçu pour faciliter
 la migration depuis Windows et assurer la sécurité du système.
 .
 Fonctionnalités principales :
  - Monitoring système en temps réel
  - Audit de sécurité complet
  - Gestion des mises à jour
  - Nettoyage système intelligent
  - Diagnostics avancés
  - Assistant IA (version Pro)
Depends: libicu72 | libicu70 | libicu67, python3, python3-requests
EOF

# Script postinst (après installation)
cat > "$DEB_ROOT/DEBIAN/postinst" << 'EOF'
#!/bin/bash
set -e

# Mettre à jour le cache des icônes
if command -v update-icon-caches >/dev/null 2>&1; then
    update-icon-caches /usr/share/icons/hicolor/ >/dev/null 2>&1 || true
fi

# Mettre à jour la base de données des applications
if command -v update-desktop-database >/dev/null 2>&1; then
    update-desktop-database -q /usr/share/applications >/dev/null 2>&1 || true
fi

echo "✅ Tuxpilot installé avec succès !"
echo ""
echo "Pour lancer Tuxpilot :"
echo "  - Menu Applications > Système > Tuxpilot"
echo "  - Ou tapez : tuxpilot"
echo ""
echo "Site web : https://lechevalierdelacyber.fr"

exit 0
EOF
chmod +x "$DEB_ROOT/DEBIAN/postinst"

# Script prerm (avant désinstallation)
cat > "$DEB_ROOT/DEBIAN/prerm" << 'EOF'
#!/bin/bash
set -e

echo "Désinstallation de Tuxpilot..."

exit 0
EOF
chmod +x "$DEB_ROOT/DEBIAN/prerm"

# Script postrm (après désinstallation)
cat > "$DEB_ROOT/DEBIAN/postrm" << 'EOF'
#!/bin/bash
set -e

# Nettoyer le cache des icônes
if command -v update-icon-caches >/dev/null 2>&1; then
    update-icon-caches /usr/share/icons/hicolor/ >/dev/null 2>&1 || true
fi

# Nettoyer la base de données des applications
if command -v update-desktop-database >/dev/null 2>&1; then
    update-desktop-database -q /usr/share/applications >/dev/null 2>&1 || true
fi

echo "Tuxpilot désinstallé."

exit 0
EOF
chmod +x "$DEB_ROOT/DEBIAN/postrm"

echo -e "${BLUE}📦 Étape 6/6 : Génération du package .deb${NC}"

# Construire le package
dpkg-deb --build --root-owner-group "$DEB_ROOT"

# Déplacer à la racine du projet
mv "$BUILD_DIR/${PACKAGE_NAME}.deb" "$PROJECT_ROOT/"

echo ""
echo -e "${GREEN}✅ Package .deb créé avec succès !${NC}"
echo -e "${GREEN}📦 Fichier : $PROJECT_ROOT/${PACKAGE_NAME}.deb${NC}"
echo -e "${GREEN}📏 Taille : $(du -h "$PROJECT_ROOT/${PACKAGE_NAME}.deb" | cut -f1)${NC}"
echo ""
echo "🧪 Pour tester :"
echo "  sudo dpkg -i ${PACKAGE_NAME}.deb"
echo "  tuxpilot"
echo ""
echo "📤 Pour distribuer :"
echo "  - Upload sur ton site web"
echo "  - Compatible : Ubuntu 22.04+, Debian 12+, Linux Mint 21+"
