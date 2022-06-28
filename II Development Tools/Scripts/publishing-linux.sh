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

# Location of .deb package filesystem template to utilize for .deb packaging
DEB_FILESYSTEM="$SOLUTION_PATH/Package, Linux/deb-fs"

# ####
# Error checking before beginning script
# ####

if ! command -v uuidgen &> /dev/null; then
    echo "Error: uuidgen could not be found"
    echo "Please install package uuid-runtime"
    exit
fi

ZIPFILE=$( ls "$RELEASE_PATH/"_linux-x64*zip )

if ! test -f "$ZIPFILE" ; then
    echo "Error: _linux-x64.[version].zip missing from Infirmary Integrated/Release publish folder"
    exit
fi

UUID=$(uuidgen)
PROCESS_PATH="/tmp/$UUID"

echo -e ""
echo -e "Creating temp directory $PROCESS_PATH\n"

mkdir "$PROCESS_PATH"

echo -e "Copying package from $RELEASE_PATH\n"

cp "$ZIPFILE" "$PROCESS_PATH"
rm "$ZIPFILE"

echo -e "Extracting package and setting file permissions\n"

cd "$PROCESS_PATH"

ZIPFILE=$( ls _linux-x64*zip )
tar -xf "$ZIPFILE"
rm "$ZIPFILE"
chmod +x "$PUB_SIMEXE"
chmod +x "$PUB_SCENEDEXE"

len1=`echo $ZIPFILE | wc -c`
len2=$(expr $len1 - 5)
OUTNAME=$(echo $ZIPFILE | cut -c 1-$len2)
VERSION=$(echo $OUTNAME | cut -c 12-)

# ####
# Package into .tar.gz
# ####

OUTFILE="infirmary-integrated-$VERSION-linux.tar.gz"

echo -e "Rebuilding package to $OUTFILE\n"
tar -czf "$OUTFILE" "$PUB_FOLDER"

echo -e "Moving package to $RELEASE_PATH\n"
mv "$OUTFILE" "$RELEASE_PATH"

# ####
# Package into .deb
# ####

if ! command -v dpkg-deb &> /dev/null; 
then
    echo "Error: dpkg-deb could not be found"
else            
    echo -e "Creating .deb file structure\n"
    DEB_FS="$PROCESS_PATH/infirmary-integrated"
    mkdir "$DEB_FS"
    cp -R "$DEB_FILESYSTEM/"* "$DEB_FS"
    printf "\nVersion: %s\n" $VERSION >> "$DEB_FS/DEBIAN/control"
    mv "$PROCESS_PATH/Infirmary Integrated" "$DEB_FS/usr/share/infirmary-integrated"
    chmod +x "$DEB_FS/usr/bin"/*

    echo -e "Packing .deb package\n"
    dpkg-deb --build "$DEB_FS" >> /dev/null
    
    DEB_PACKAGE=$(printf "infirmary-integrated_%s_amd64.deb" $VERSION)    

    echo -e "Moving .deb package to $RELEASE_PATH/$DEB_PACKAGE\n"
    mv "$PROCESS_PATH/infirmary-integrated.deb" "$RELEASE_PATH/$DEB_PACKAGE"
fi

# ####
# Clean up temp directories
# ####
exit
echo -e "Removing temp directory $PROCESS_PATH\n"
rm -rf "$PROCESS_PATH"