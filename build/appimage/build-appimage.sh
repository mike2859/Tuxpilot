#!/bin/bash
# Script de création AppImage pour Tuxpilot
# Usage: cd build/appimage && ./build-appimage.sh

set -e  # Arrêter si erreur

echo "🚀 Building Tuxpilot AppImage..."

# Variables
APP_NAME="Tuxpilot"
APP_VERSION="0.9.0"
ARCH="x86_64"
BUILD_DIR="appimage-build"
APPDIR="$BUILD_DIR/$APP_NAME.AppDir"

# Chemin racine du projet (2 niveaux au-dessus)
PROJECT_ROOT="$(cd "$(dirname "$0")/../.." && pwd)"

# Couleurs pour les messages
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${BLUE}📁 Projet : $PROJECT_ROOT${NC}"
echo ""

echo -e "${BLUE}📦 Étape 1/5 : Nettoyage${NC}"
rm -rf "$BUILD_DIR"
mkdir -p "$APPDIR"

echo -e "${BLUE}📦 Étape 2/5 : Publication .NET${NC}"
dotnet publish "$PROJECT_ROOT/src/Tuxpilot.UI/Tuxpilot.UI.csproj" \
    -c Release \
    -r linux-x64 \
    --self-contained true \
    -p:PublishSingleFile=false \
    -p:PublishTrimmed=false \
    -o "$APPDIR/usr/bin"

echo -e "${BLUE}📦 Étape 3/5 : Copie des scripts Python${NC}"
# Copier les scripts Python
mkdir -p "$APPDIR/usr/bin/Scripts"
cp "$PROJECT_ROOT/src/Tuxpilot.Infrastructure/Scripts/validate_license.py" "$APPDIR/usr/bin/Scripts/"
chmod +x "$APPDIR/usr/bin/Scripts/validate_license.py"

echo -e "${BLUE}📦 Étape 4/5 : Création fichiers AppImage${NC}"

# Créer le fichier .desktop
cat > "$APPDIR/$APP_NAME.desktop" << 'EOF'
[Desktop Entry]
Name=Tuxpilot
Comment=Assistant Linux pour débutants et entreprises
Exec=Tuxpilot.UI
Icon=tuxpilot
Type=Application
Categories=System;Utility;Settings;
Terminal=false
Keywords=linux;security;audit;maintenance;
EOF

# Vérifier et copier l'icône
if [ ! -f "icon.png" ]; then
    echo -e "${YELLOW}⚠️  Icône icon.png non trouvée dans build/appimage/${NC}"
    echo "Veuillez copier icon.png dans build/appimage/"
    exit 1
fi

# Copier l'icône
echo "✅ Icône trouvée : icon.png"
cp icon.png "$APPDIR/tuxpilot.png"
cp icon.png "$APPDIR/.DirIcon"

# Si icon-512.png existe, l'utiliser aussi
if [ -f "icon-512.png" ]; then
    mkdir -p "$APPDIR/usr/share/icons/hicolor/512x512/apps"
    cp icon-512.png "$APPDIR/usr/share/icons/hicolor/512x512/apps/tuxpilot.png"
fi

# Créer AppRun
cat > "$APPDIR/AppRun" << 'EOF'
#!/bin/bash
SELF=$(readlink -f "$0")
HERE=${SELF%/*}
export PATH="${HERE}/usr/bin:${PATH}"
export LD_LIBRARY_PATH="${HERE}/usr/lib:${LD_LIBRARY_PATH}"

# Définir le chemin des scripts
export TUXPILOT_SCRIPTS_PATH="${HERE}/usr/bin/Scripts"

cd "${HERE}/usr/bin"
exec ./Tuxpilot.UI "$@"
EOF

chmod +x "$APPDIR/AppRun"

echo -e "${BLUE}📦 Étape 5/5 : Génération AppImage${NC}"

# Télécharger appimagetool si nécessaire
if [ ! -f "appimagetool-x86_64.AppImage" ]; then
    echo "📥 Téléchargement appimagetool..."
    wget -q "https://github.com/AppImage/AppImageKit/releases/download/continuous/appimagetool-x86_64.AppImage"
    chmod +x appimagetool-x86_64.AppImage
fi

# Générer l'AppImage
ARCH=$ARCH ./appimagetool-x86_64.AppImage "$APPDIR" "$APP_NAME-$APP_VERSION-$ARCH.AppImage"

# Déplacer à la racine du projet pour faciliter la distribution
mv "$APP_NAME-$APP_VERSION-$ARCH.AppImage" "$PROJECT_ROOT/"

echo ""
echo -e "${GREEN}✅ AppImage créé avec succès !${NC}"
echo -e "${GREEN}📦 Fichier : $PROJECT_ROOT/$APP_NAME-$APP_VERSION-$ARCH.AppImage${NC}"
echo -e "${GREEN}📏 Taille : $(du -h "$PROJECT_ROOT/$APP_NAME-$APP_VERSION-$ARCH.AppImage" | cut -f1)${NC}"
echo ""
echo "🧪 Pour tester :"
echo "  cd $PROJECT_ROOT"
echo "  chmod +x $APP_NAME-$APP_VERSION-$ARCH.AppImage"
echo "  ./$APP_NAME-$APP_VERSION-$ARCH.AppImage"
echo ""
echo "📤 Pour distribuer :"
echo "  - Upload sur ton site web"
echo "  - Partage avec tes Early Adopters"
echo "  - Fonctionne sur toutes les distros Linux !"
