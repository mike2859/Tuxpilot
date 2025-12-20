#!/bin/bash
# Script de création package .rpm pour Tuxpilot
# Usage: cd build/rpm && ./build-rpm.sh

set -e  # Arrêter si erreur

echo "🚀 Building Tuxpilot .rpm package..."

# Variables
APP_NAME="tuxpilot"
APP_VERSION="0.9.0"
RELEASE="1"
ARCH="x86_64"
BUILD_DIR="rpm-build"
PACKAGE_NAME="${APP_NAME}-${APP_VERSION}-${RELEASE}.${ARCH}"

# Chemin racine du projet (2 niveaux au-dessus)
PROJECT_ROOT="$(cd "$(dirname "$0")/../.." && pwd)"

# Couleurs
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m'

echo -e "${BLUE}📁 Projet : $PROJECT_ROOT${NC}"
echo ""

# Vérifier rpmbuild
if ! command -v rpmbuild &> /dev/null; then
    echo -e "${YELLOW}⚠️  rpmbuild non installé !${NC}"
    echo "Installation : sudo dnf install rpm-build rpmdevtools"
    exit 1
fi

echo -e "${BLUE}📦 Étape 1/6 : Nettoyage${NC}"
rm -rf "$BUILD_DIR"
mkdir -p "$BUILD_DIR"/{BUILD,RPMS,SOURCES,SPECS,SRPMS}

echo -e "${BLUE}📦 Étape 2/6 : Publication .NET${NC}"
INSTALL_ROOT="$BUILD_DIR/BUILDROOT/$APP_NAME-$APP_VERSION"
mkdir -p "$INSTALL_ROOT/opt/tuxpilot"

dotnet publish "$PROJECT_ROOT/src/Tuxpilot.UI/Tuxpilot.UI.csproj" \
    -c Release \
    -r linux-x64 \
    --self-contained true \
    -p:PublishSingleFile=false \
    -p:PublishTrimmed=false \
    -o "$INSTALL_ROOT/opt/tuxpilot"

echo -e "${BLUE}📦 Étape 3/6 : Copie des scripts Python${NC}"
mkdir -p "$INSTALL_ROOT/opt/tuxpilot/Scripts"
cp "$PROJECT_ROOT/src/Tuxpilot.Infrastructure/Scripts/validate_license.py" \
   "$INSTALL_ROOT/opt/tuxpilot/Scripts/"
chmod +x "$INSTALL_ROOT/opt/tuxpilot/Scripts/validate_license.py"

echo -e "${BLUE}📦 Étape 4/6 : Création des fichiers système${NC}"

# Launcher
mkdir -p "$INSTALL_ROOT/usr/bin"
cat > "$INSTALL_ROOT/usr/bin/tuxpilot" << 'EOF'
#!/bin/bash
export TUXPILOT_SCRIPTS_PATH="/opt/tuxpilot/Scripts"
cd /opt/tuxpilot
exec ./Tuxpilot.UI "$@"
EOF
chmod +x "$INSTALL_ROOT/usr/bin/tuxpilot"

# Fichier .desktop
mkdir -p "$INSTALL_ROOT/usr/share/applications"
cat > "$INSTALL_ROOT/usr/share/applications/tuxpilot.desktop" << EOF
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

# Icônes
if [ ! -f "icon.png" ]; then
    echo -e "${YELLOW}⚠️  Icône non trouvée dans build/rpm/${NC}"
    exit 1
fi

echo "✅ Icône trouvée"
mkdir -p "$INSTALL_ROOT/usr/share/icons/hicolor/256x256/apps"
cp icon.png "$INSTALL_ROOT/usr/share/icons/hicolor/256x256/apps/tuxpilot.png"

if [ -f "icon-512.png" ]; then
    mkdir -p "$INSTALL_ROOT/usr/share/icons/hicolor/512x512/apps"
    cp icon-512.png "$INSTALL_ROOT/usr/share/icons/hicolor/512x512/apps/tuxpilot.png"
fi

if [ -f "icon-128.png" ]; then
    mkdir -p "$INSTALL_ROOT/usr/share/icons/hicolor/128x128/apps"
    cp icon-128.png "$INSTALL_ROOT/usr/share/icons/hicolor/128x128/apps/tuxpilot.png"
fi

echo -e "${BLUE}📦 Étape 5/6 : Création du fichier .spec${NC}"

# ✅ CORRECTION : Date au format anglais pour RPM
CHANGELOG_DATE=$(LC_ALL=C date "+%a %b %d %Y")

cat > "$BUILD_DIR/SPECS/$APP_NAME.spec" << EOF
Name:           $APP_NAME
Version:        $APP_VERSION
Release:        $RELEASE%{?dist}
Summary:        Assistant Linux pour débutants et entreprises

License:        Proprietary
URL:            https://lechevalierdelacyber.fr
BuildArch:      x86_64

Requires:       python3, python3-requests, lttng-ust, libicu

%description
Tuxpilot est un assistant desktop pour Linux conçu pour faciliter
la migration depuis Windows et assurer la sécurité du système.

Fonctionnalités principales :
- Monitoring système en temps réel
- Audit de sécurité complet
- Gestion des mises à jour
- Nettoyage système intelligent
- Diagnostics avancés
- Assistant IA (version Pro)

%prep
# Rien à préparer, les fichiers sont déjà là

%build
# Rien à builder, déjà compilé

%install
rm -rf \$RPM_BUILD_ROOT
mkdir -p \$RPM_BUILD_ROOT
cp -a $(pwd)/$INSTALL_ROOT/* \$RPM_BUILD_ROOT/

%files
/opt/tuxpilot/
/usr/bin/tuxpilot
/usr/share/applications/tuxpilot.desktop
/usr/share/icons/hicolor/*/apps/tuxpilot.png

%post
# Mettre à jour les caches après installation
update-desktop-database /usr/share/applications &> /dev/null || :
touch --no-create /usr/share/icons/hicolor &> /dev/null || :

echo "✅ Tuxpilot installé avec succès !"
echo ""
echo "Pour lancer Tuxpilot :"
echo "  - Menu Applications > Système > Tuxpilot"
echo "  - Ou tapez : tuxpilot"
echo ""
echo "Site web : https://lechevalierdelacyber.fr"

%postun
# Mettre à jour les caches après désinstallation
update-desktop-database /usr/share/applications &> /dev/null || :
if [ \$1 -eq 0 ] ; then
    touch --no-create /usr/share/icons/hicolor &> /dev/null || :
    gtk-update-icon-cache /usr/share/icons/hicolor &> /dev/null || :
fi

%changelog
* $CHANGELOG_DATE Le Chevalier de la Cyber <contact@lechevalierdelacyber.fr> - $APP_VERSION-$RELEASE
- Version Early Access initiale
- Monitoring système temps réel
- Audit de sécurité
- Système de licence
EOF

echo -e "${BLUE}📦 Étape 6/6 : Construction du package RPM${NC}"

# Builder le RPM
rpmbuild --define "_topdir $(pwd)/$BUILD_DIR" \
         -bb "$BUILD_DIR/SPECS/$APP_NAME.spec"

# Déplacer le RPM à la racine du projet
RPM_FILE=$(find "$BUILD_DIR/RPMS" -name "*.rpm" -type f)
if [ -z "$RPM_FILE" ]; then
    echo -e "${YELLOW}❌ Erreur : Package RPM non trouvé${NC}"
    exit 1
fi

cp "$RPM_FILE" "$PROJECT_ROOT/"
RPM_NAME=$(basename "$RPM_FILE")

echo ""
echo -e "${GREEN}✅ Package .rpm créé avec succès !${NC}"
echo -e "${GREEN}📦 Fichier : $PROJECT_ROOT/$RPM_NAME${NC}"
echo -e "${GREEN}📏 Taille : $(du -h "$PROJECT_ROOT/$RPM_NAME" | cut -f1)${NC}"
echo ""
echo "🧪 Pour tester (sur Fedora) :"
echo "  sudo dnf install $RPM_NAME"
echo "  tuxpilot"
echo ""
echo "📤 Pour distribuer :"
echo "  - Upload sur ton site web"
echo "  - Compatible : Fedora 40+, RHEL 9+, Rocky Linux 9+"
