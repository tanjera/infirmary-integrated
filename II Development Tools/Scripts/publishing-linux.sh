#!/bin/bash

# ####
# Usage: Run in Linux *after* II Dev Tools Publishing Utility (called by publishing-windows.bat)!
# Will repackage build output into a .tar.gz with Linux chmod permissions set appropriately
# ####

# Location of the main II solution folder
SOLUTION_PATH="/home/ibi/Documents/Infirmary Integrated"

# Location of published executable files in the build structure
# Published by II Developer Tools\Publishing Utility
RELEASE_PATH="$SOLUTION_PATH/Release"
PUB_FOLDER="Infirmary Integrated"
PUB_SIMEXE="Infirmary Integrated/Infirmary Integrated/Infirmary Integrated"
PUB_SCENEDEXE="Infirmary Integrated/Infirmary Integrated Scenario Editor/Infirmary Integrated Scenario Editor"

# Location of package information templates (for .deb, .rpm)
PACKAGE_FS="$SOLUTION_PATH/Package, Linux/_filesystem"
DEB_DIR="$SOLUTION_PATH/Package, Linux/DEB"
RPM_DIR="$SOLUTION_PATH/Package, Linux/RPM"
RPM_SPEC="$RPM_DIR/infirmary-integrated.spec"
RPMBUILD_DIR="/home/ibi/rpmbuild"

# ####
# Error checking before beginning script
# ####

if ! command -v uuidgen &> /dev/null; then
    echo "Error: uuidgen could not be found"
    echo "Please install package uuid-runtime"
    exit
fi

ZIPFILE=$( ls "$RELEASE_PATH/"infirmary-integrated-*-linux.tar.gz )

if ! test -f "$ZIPFILE" ; then
    echo "Error: infirmary-integrated-[version].tar.gz missing from Infirmary Integrated/Release publish folder"
    exit
fi

UUID=$(uuidgen)
PROCESS_PATH="/tmp/$UUID"

echo -e ""
echo -e "Creating temp directory $PROCESS_PATH\n"

mkdir "$PROCESS_PATH"

echo -e "Moving package for processing from $RELEASE_PATH\n"

cp "$ZIPFILE" "$PROCESS_PATH"
rm "$ZIPFILE"

echo -e "Extracting package and setting file permissions\n"

cd "$PROCESS_PATH"

ZIPFILE=$( ls infirmary-integrated-*-linux.tar.gz )
tar -xzf "$ZIPFILE"
rm "$ZIPFILE"
chmod +x "$PUB_SIMEXE"
chmod +x "$PUB_SCENEDEXE"

# ####
# Package into .tar.gz
# ####

echo -e "Rebuilding package to $ZIPFILE\n"
tar -czf "$ZIPFILE" "$PUB_FOLDER"

echo -e "Moving package to $RELEASE_PATH\n"
mv "$ZIPFILE" "$RELEASE_PATH"

# ####
# Package into .deb
# ####

len1=`echo $ZIPFILE | wc -c`
len2=$(expr $len1 - 13)
VERSION=$(echo $ZIPFILE | cut -c 22-$( expr $len1 - 14 ))
DEB_PACKAGE=$(printf "infirmary-integrated_%s_amd64.deb" $VERSION)

if ! command -v dpkg-deb &> /dev/null;
then
    echo "Error: dpkg-deb could not be found"
    exit
else
    echo -e "Creating .deb file structure\n"
    DEB_FS="$PROCESS_PATH/infirmary-integrated"
    mkdir "$DEB_FS"

    cp -R "$DEB_DIR/"* "$DEB_FS"
    cp -R "$PACKAGE_FS/"* "$DEB_FS"
    printf "\nVersion: %s\n" $VERSION >> "$DEB_FS/DEBIAN/control"
    mv "$PROCESS_PATH/Infirmary Integrated" "$DEB_FS/usr/share/infirmary-integrated"
    chmod +x "$DEB_FS/usr/bin"/*

    echo -e "Packing .deb package\n"
    dpkg-deb --build "$DEB_FS" >> /dev/null

    echo -e "Moving .deb package to $RELEASE_PATH/$DEB_PACKAGE\n"
    mv "$PROCESS_PATH/infirmary-integrated.deb" "$RELEASE_PATH/$DEB_PACKAGE"
fi

# ####
# Package into .rpm
# ####

if ! command -v rpmbuild &> /dev/null; then
    echo "Error: rpmbuild could not be found"
    exit
else
    echo -e "Creating .rpm file structure\n"

    # Set up the build directories
    rm -rf "$RPMBUILD_DIR"
    mkdir -p "$RPMBUILD_DIR/BUILD"
    mkdir -p "$RPMBUILD_DIR/BUILDROOT"
    mkdir -p "$RPMBUILD_DIR/RPMS"
    mkdir -p "$RPMBUILD_DIR/SOURCES"
    mkdir -p "$RPMBUILD_DIR/SPECS"
    mkdir -p "$RPMBUILD_DIR/SRPMS"

    # Move the file structure into the BUILDROOT
    rm -rf "$DEB_FS/DEBIAN"

    BUILDROOT_PACKAGE_DIR="$RPMBUILD_DIR/BUILDROOT/infirmary-integrated-$VERSION-1.x86_64"
    echo -e "Copying directory structure to $BUILDROOT_PACKAGE_DIR"
    mkdir -p "$BUILDROOT_PACKAGE_DIR"
    cp -R "$DEB_FS/"* "$BUILDROOT_PACKAGE_DIR"

    # Prepare the .spec file
    RPM_TARGET_SPEC="$RPMBUILD_DIR/SPECS/infirmary-integrated.spec"
    printf "Version: $VERSION\n" >> "$RPM_TARGET_SPEC"
    cat "$RPM_SPEC" >> "$RPM_TARGET_SPEC"

    echo -e "Packing .rpm package\n"
    rpmbuild -bb "$RPM_TARGET_SPEC"

    echo -e "Moving .rpm package to $RELEASE_PATH/\n"
    mv "$RPMBUILD_DIR/RPMS/x86_64/"* "$RELEASE_PATH"
fi

# ####
# Clean up temp directories
# ####

echo -e "Removing temp directory $PROCESS_PATH\n"
rm -rf "$PROCESS_PATH"