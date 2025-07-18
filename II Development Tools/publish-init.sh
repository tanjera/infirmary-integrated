#!/bin/bash

# ####
# Color Constants
# ####

# Reset
Reset='\033[0m'           # Text Reset
# Regular Colors
Black='\033[0;30m'        # Black
Red='\033[0;31m'          # Red
Green='\033[0;32m'        # Green
Yellow='\033[0;33m'       # Yellow
Blue='\033[0;34m'         # Blue
Purple='\033[0;35m'       # Purple
Cyan='\033[0;36m'         # Cyan
White='\033[0;37m'        # White
# Bold
BBlack='\033[1;30m'       # Black
BRed='\033[1;31m'         # Red
BGreen='\033[1;32m'       # Green
BYellow='\033[1;33m'      # Yellow
BBlue='\033[1;34m'        # Blue
BPurple='\033[1;35m'      # Purple
BCyan='\033[1;36m'        # Cyan
BWhite='\033[1;37m'       # White


# ####
# General Constants
# ####

SCRIPT_PATH=$( pwd )
cd ..
SOLUTION_PATH=$( pwd )

VERSION_DOTNET="net9.0";
ARCHITECTURES=( "win-x64" "win-arm64" "linux-x64" "linux-arm64" "osx-x64" "osx-arm64" )

OUT_PREFIX="\n${BCyan}>>>>>${Reset} "

# Location of published executable files in the build structure
RELEASE_PATH="$SOLUTION_PATH/Release"
PUB_FOLDER="Infirmary Integrated"
PUB_SIMEXE="Infirmary Integrated/Infirmary Integrated/Infirmary Integrated"
PUB_SCENEDEXE="Infirmary Integrated/Infirmary Integrated Scenario Editor/Infirmary Integrated Scenario Editor"

# Location of package information templates (for .deb, .rpm, .app)
PACKAGE_FS="$SOLUTION_PATH/Package, Linux/_filesystem"
DEB_DIR="$SOLUTION_PATH/Package, Linux/DEB"
RPM_DIR="$SOLUTION_PATH/Package, Linux/RPM"
RPM_SPEC="$RPM_DIR/infirmary-integrated.spec"
RPMBUILD_DIR="/home/ibi/rpmbuild"
MAC_SIM_INFO_PLIST="$SOLUTION_PATH/Package, MacOS/Infirmary Integrated/Info.plist"
MAC_SIM_ICON_PATH="$SOLUTION_PATH/Package, MacOS/Infirmary Integrated/Icon_II.icns"
MAC_SIM_ICON_FILE="Icon_II.icns"
MAC_SCENED_INFO_PLIST="$SOLUTION_PATH/Package, MacOS/Infirmary Integrated Scenario Editor/Info.plist"
MAC_SCENED_ICON_PATH="$SOLUTION_PATH/Package, MacOS/Infirmary Integrated Scenario Editor/Icon_IISE.icns"
MAC_SCENED_ICON_FILE="Icon_IISE.icns"


# ####
# Error checking before beginning script
# ####

if ! command -v uuidgen &> /dev/null; then
    echo "Error: uuidgen could not be found"
    echo "Please install package uuid-runtime"
    exit
fi

if ! command -v zip &> /dev/null; then
    echo "Error: zip could not be found"
    echo "Please install package zip"
    exit
fi

if ! command -v 7z &> /dev/null; then
    echo "Error: 7z could not be found"
    echo "Please install package p7zip"
    exit
fi

if ! command -v dpkg-deb &> /dev/null; then
    echo "Error: dpkg-deb could not be found"
    echo "Please install package"
    exit
fi

if ! command -v rpmbuild &> /dev/null; then
    echo "Error: rpmbuild could not be found"
    echo "Please install package"
    exit
fi

# ####
# Set up directories to use, gather user information
# ####

mkdir -p "$RELEASE_PATH"

echo -ne "${OUT_PREFIX}What version number should this build use? "
read VERSION

# ####
# Clean build locations for projects
# Each OS has its own "II Simulator" project directory for native cross-platform runtimes!
# Windows & Linux use C#/.NET project structures; MacOS uses Swift/AppKit project structure!
# ####

WIN_DIR_SIMULATOR="$SOLUTION_PATH/II Simulator, Windows"
WIN_DIR_SIMULATOR_BIN="$SOLUTION_PATH/II Simulator, Windows/bin"
WIN_DIR_SIMULATOR_OBJ="$SOLUTION_PATH/II Simulator, Windows/obj"

LINUX_DIR_SIMULATOR="$SOLUTION_PATH/II Simulator, Linux"
LINUX_DIR_SIMULATOR_BIN="$SOLUTION_PATH/II Simulator, Linux/bin"
LINUX_DIR_SIMULATOR_OBJ="$SOLUTION_PATH/II Simulator, Linux/obj"

# TODO: Add MacOS build preparation HERE

echo -e "${OUT_PREFIX}Cleaning build files (bin, obj) from '$WIN_DIR_SIMULATOR'"

rm -r "$WIN_DIR_SIMULATOR_BIN"
rm -r "$WIN_DIR_SIMULATOR_OBJ"

echo -e "${OUT_PREFIX}Cleaning build files (bin, obj) from '$LINUX_DIR_SIMULATOR'"

rm -r "$LINUX_DIR_SIMULATOR_BIN"
rm -r "$LINUX_DIR_SIMULATOR_OBJ"

# ####
# All projects use Avalonia (C#/.NET) for the Scenario Editor
# ####

DIR_SCENED="$SOLUTION_PATH/II Scenario Editor"
DIR_SCENED_BIN="$SOLUTION_PATH/II Scenario Editor/bin"
DIR_SCENED_OBJ="$SOLUTION_PATH/II Scenario Editor/obj"

echo -e "${OUT_PREFIX}Cleaning build files (bin, obj) from '$DIR_SCENED'"

rm -r "$DIR_SCENED_BIN"
rm -r "$DIR_SCENED_OBJ"

echo -e "${OUT_PREFIX}Executing 'dotnet clean' on ${DIR_SIMULATOR}\n"
cd "$DIR_SIMULATOR"
dotnet clean

echo -e "${OUT_PREFIX}Executing 'dotnet clean' on ${DIR_SCENED}\n"
cd "$DIR_SCENED"
dotnet clean

# ####
# Iterate architectures and build/publish, then package
# ####

for arch in ${ARCHITECTURES[*]}; do

    if [[ $arch == win* || $arch == linux* ]]; then
        # Build II Simulator
        echo -e "${OUT_PREFIX}Building II Simulator for ${arch}\n"
        cd "$DIR_SIMULATOR"
        if [[ $arch == osx* ]]; then
            dotnet publish -c Release --sc -r $arch -p:UseAppHost=true
        else
            dotnet publish -c Release --sc -r $arch
        fi
    elif [[ $arch == mac* ]]; then
        # TODO: Implement MacOS building (Swift/AppKit)
    fi

    # Build II Scenario Editor
    echo -e "${OUT_PREFIX}Building II Scenario Editor for ${arch}\n"
    cd "$DIR_SCENED"
    if [[ $arch == osx* ]]; then
        dotnet publish -c Release --sc -r $arch -p:UseAppHost=true
    else
        dotnet publish -c Release --sc -r $arch
    fi

    # Setup working (/tmp) directory and subdirectories
    UUID=$(uuidgen)
    DIR_WORKING="/tmp/$UUID"

    echo -e "${OUT_PREFIX}Creating working directory $DIR_WORKING\n"

    mkdir -p "${DIR_WORKING}/Infirmary Integrated"

    # Move the published projects to the temporary directories
    echo -e "${OUT_PREFIX}Moving project files to ${DIR_WORKING}/Infirmary Integrated/Infirmary Integrated\n"
    if [[ $arch == win* || $arch == linux* ]]; then
        mv "${DIR_SIMULATOR_BIN}/Release/${VERSION_DOTNET}/${arch}/publish" "${DIR_WORKING}/Infirmary Integrated/Infirmary Integrated"
    elif [[ $arch == mac* ]]; then
        # TODO: Implement moving MacOS project to publishing preparation directory
    fi

    echo -e "${OUT_PREFIX}Moving project files to ${DIR_WORKING}/Infirmary Integrated/Infirmary Integrated Scenario Editor\n"
    mv "${DIR_SCENED_BIN}/Release/${VERSION_DOTNET}/${arch}/publish" "${DIR_WORKING}/Infirmary Integrated/Infirmary Integrated Scenario Editor"

    
    # Set executables for *nix systems
    if [[ $arch == linux* || $arch == osx* ]]; then
        echo -e "${OUT_PREFIX}Setting *nix file permissions\n"
        chmod +x "${DIR_WORKING}/${PUB_SIMEXE}"
        chmod +x "${DIR_WORKING}/${PUB_SCENEDEXE}"
    fi


    # ####
    # Package the projects using appropriate file formats per OS
    # ####

    PACK_NAME="infirmary-integrated-${VERSION}-${arch}"

    if [[ $arch == win* ]]; then
        PACK_PATH="${SOLUTION_PATH}/Release/${PACK_NAME}.zip"
        if [ -f "${PACK_PATH}" ]; then
            rm "${PACK_PATH}"
        fi

        echo -e "${OUT_PREFIX}Packaging into ${RELEASE_PATH}/${PACK_NAME}.zip\n"
        cd "${DIR_WORKING}"
        zip -r "${RELEASE_PATH}/${PACK_NAME}.zip" "Infirmary Integrated"
    elif [[ $arch == linux* ]]; then
        PACK_PATH="${SOLUTION_PATH}/Release/${PACK_NAME}.tar.gz"
        if [ -f "${PACK_PATH}" ]; then            
            rm "${PACK_PATH}"
        fi

        echo -e "${OUT_PREFIX}Packaging into ${RELEASE_PATH}/${PACK_NAME}.tar.gz\n"
        cd "${DIR_WORKING}"
        tar -czvf "${RELEASE_PATH}/${PACK_NAME}.tar.gz" "Infirmary Integrated"        
    fi
    
    
    # ####
    # Specific publishing packages per OS
    # ####

    if [[ $arch == "linux-x64" ]]; then

        # ####
        # Package into .deb
        # ####

        DEB_PACKAGE="infirmary-integrated_${VERSION}_amd64.deb"

        echo -e "${OUT_PREFIX}Creating .deb file structure\n"
        DEB_FS="$DIR_WORKING/infirmary-integrated"
        mkdir "$DEB_FS"

        cp -R "$DEB_DIR/"* "$DEB_FS"
        cp -R "$PACKAGE_FS/"* "$DEB_FS"
        printf "\nVersion: %s\n" $VERSION >> "$DEB_FS/DEBIAN/control"
        cp -r "$DIR_WORKING/Infirmary Integrated" "$DEB_FS/usr/share/infirmary-integrated"
        chmod +x "$DEB_FS/usr/bin"/*

        echo -e "${OUT_PREFIX}Packing .deb package\n"
        dpkg-deb --build "$DEB_FS" >> /dev/null

        echo -e "Moving .deb package to $RELEASE_PATH/$DEB_PACKAGE\n"
        mv -f "$DIR_WORKING/infirmary-integrated.deb" "$RELEASE_PATH/$DEB_PACKAGE"        

        # ####
        # Package into .rpm; utilizes build files from .deb file structure!
        # ####

        echo -e "${OUT_PREFIX}Creating .rpm file structure\n"

        # Set up the build directories
        rm -r "$RPMBUILD_DIR"
        mkdir -p "$RPMBUILD_DIR/BUILD"
        mkdir -p "$RPMBUILD_DIR/BUILDROOT"
        mkdir -p "$RPMBUILD_DIR/RPMS"
        mkdir -p "$RPMBUILD_DIR/SOURCES"
        mkdir -p "$RPMBUILD_DIR/SPECS"
        mkdir -p "$RPMBUILD_DIR/SRPMS"

        # Move the file structure into the BUILDROOT
        rm -r "$DEB_FS/DEBIAN"

        BUILDROOT_PACKAGE_DIR="$RPMBUILD_DIR/BUILDROOT/infirmary-integrated-$VERSION-1.x86_64"
        echo -e "${OUT_PREFIX}Copying directory structure to $BUILDROOT_PACKAGE_DIR"
        mkdir -p "$BUILDROOT_PACKAGE_DIR"
        cp -R "$DEB_FS/"* "$BUILDROOT_PACKAGE_DIR"

        # Prepare the .spec file
        RPM_TARGET_SPEC="$RPMBUILD_DIR/SPECS/infirmary-integrated.spec"
        printf "Version: $VERSION\n" >> "$RPM_TARGET_SPEC"
        cat "$RPM_SPEC" >> "$RPM_TARGET_SPEC"

        echo -e "${OUT_PREFIX}Packing .rpm package\n"
        rpmbuild -bb "$RPM_TARGET_SPEC"

        echo -e "${OUT_PREFIX}Moving .rpm package to $RELEASE_PATH/\n"        
        mv -f "$RPMBUILD_DIR/RPMS/x86_64/"* "$RELEASE_PATH"        
        rm -r "$RPMBUILD_DIR"
    
    elif [[ $arch == osx* ]]; then

        # ####
        # Create .app directory structures
        # ####

        echo -e "${OUT_PREFIX}Creating .app structure in $PROCESS_PATH\n"

        SIM_APP_PATH="$DIR_WORKING/Infirmary Integrated.app"
        SCENED_APP_PATH="$DIR_WORKING/Infirmary Integrated Scenario Editor.app"

        mkdir -p "$SIM_APP_PATH/Contents/MacOS"
        mkdir -p "$SIM_APP_PATH/Contents/Resources"
        mkdir -p "$SCENED_APP_PATH/Contents/MacOS"
        mkdir -p "$SCENED_APP_PATH/Contents/Resources"

        # ####
        # Move MacOS-specific package files & II filetree into .app structure
        # ####

        echo -e "${OUT_PREFIX}Copying package contents into .app structure\n"

        cp "$MAC_SIM_INFO_PLIST" "$SIM_APP_PATH/Contents/Info.plist"
        cp "$MAC_SIM_ICON_PATH" "$SIM_APP_PATH/Contents/Resources/$MAC_SIM_ICON_FILE"

        cp "$MAC_SCENED_INFO_PLIST" "$SCENED_APP_PATH/Contents/Info.plist"
        cp "$MAC_SCENED_ICON_PATH" "$SCENED_APP_PATH/Contents/Resources/$MAC_SCENED_ICON_FILE"

        mv "${DIR_WORKING}/Infirmary Integrated/Infirmary Integrated"/* "$SIM_APP_PATH/Contents/MacOS"
        mv "${DIR_WORKING}/Infirmary Integrated/Infirmary Integrated Scenario Editor"/* "$SCENED_APP_PATH/Contents/MacOS"


        # ####
        # Package .app's into a .tar.gz
        # ####

        echo -e "${OUT_PREFIX}Packaging tarball ${PACK_NAME}.tar.gz\n"
        
        cd "$DIR_WORKING"

        if [ -f "${RELEASE_PATH}/${PACK_NAME}.tar.gz" ]; then
            rm "${RELEASE_PATH}/${PACK_NAME}.tar.gz"
        fi

        tar -czvf "${RELEASE_PATH}/${PACK_NAME}.tar.gz" "Infirmary Integrated.app" "Infirmary Integrated Scenario Editor.app"
    fi

    rm -r "${DIR_WORKING}"
done


# ####
# Create SHA512 checksums and sign the hash list
# ####

cd "${RELEASE_PATH}"
rm -fv sha512sums
rm -fv sha512sums.sig
sha512sum *.rpm >> "${RELEASE_PATH}/sha512sums"
sha512sum *.deb >> "${RELEASE_PATH}/sha512sums"
sha512sum *linux*.tar.gz >> "${RELEASE_PATH}/sha512sums"
gpg --detach-sign sha512sums
