#!/bin/bash

# ####
# Usage: Run in Linux or MacOS *after* II Dev Tools Publishing Utility (called by publishing-windows.bat)!
# Will repackage build output into a .tar.gz with appropriate MacOS file structure
# ####

# Location of the main II solution folder
SOLUTION_PATH="/home/ibi/Documents/Infirmary Integrated"

# Location of published executable files in the build structure
# Published by II Developer Tools\Publishing Utility
RELEASE_PATH="$SOLUTION_PATH/Release"
BASE_PUB_FOLDER="Infirmary Integrated"
SIM_PUB_FOLDER="$BASE_PUB_FOLDER/Infirmary Integrated"
SCENED_PUB_FOLDER="$BASE_PUB_FOLDER/Infirmary Integrated Scenario Editor"

# ####
# Error checking before beginning script
# ####

if ! command -v uuidgen &> /dev/null; then
    echo "Error: uuidgen could not be found"
    echo "Please install package uuid-runtime"
    exit
fi

ZIPFILE=$( ls "$RELEASE_PATH/"_osx-x64*tar.gz )

if ! test -f "$ZIPFILE" ; then
    echo "Error: _osx-x64.[version].tar.gz missing from Infirmary Integrated/Release publish folder"
    exit
fi

UUID=$(uuidgen)
PROCESS_PATH="/tmp/$UUID"

SIM_APP_NAME="Infirmary Integrated.app"
SIM_INFO_PLIST="$SOLUTION_PATH/Package, MacOS/Infirmary Integrated/Info.plist"
SIM_ICON_PATH="$SOLUTION_PATH/Package, MacOS/Infirmary Integrated/Icon_II.icns"
SIM_ICON_FILE="Icon_II.icns"
SIM_EXE_FILE="Infirmary Integrated"

SCENED_APP_NAME="Infirmary Integrated Scenario Editor.app"
SCENED_INFO_PLIST="$SOLUTION_PATH/Package, MacOS/Infirmary Integrated Scenario Editor/Info.plist"
SCENED_ICON_PATH="$SOLUTION_PATH/Package, MacOS/Infirmary Integrated Scenario Editor/Icon_IISE.icns"
SCENED_ICON_FILE="Icon_IISE.icns"
SCENED_EXE_FILE="Infirmary Integrated Scenario Editor"

DESTINATION_PATH="$SOLUTION_PATH/Release"


rm -rf "$PROCESS_PATH"
mkdir "$PROCESS_PATH"
cd "$PROCESS_PATH"

echo -e ""
echo -e "Creating app structure in $PROCESS_PATH\n"

mkdir -p "$SIM_APP_NAME"
mkdir -p "$SIM_APP_NAME/Contents"
mkdir -p "$SIM_APP_NAME/Contents/MacOS"
mkdir -p "$SIM_APP_NAME/Contents/Resources"

mkdir -p "$SCENED_APP_NAME"
mkdir -p "$SCENED_APP_NAME/Contents"
mkdir -p "$SCENED_APP_NAME/Contents/MacOS"
mkdir -p "$SCENED_APP_NAME/Contents/Resources"

echo -e "Copying package contents\n"

cp "$SIM_INFO_PLIST" "$SIM_APP_NAME/Contents/Info.plist"
cp "$SIM_ICON_PATH" "$SIM_APP_NAME/Contents/Resources/$SIM_ICON_FILE"

cp "$SCENED_INFO_PLIST" "$SCENED_APP_NAME/Contents/Info.plist"
cp "$SCENED_ICON_PATH" "$SCENED_APP_NAME/Contents/Resources/$SCENED_ICON_FILE"

cp "$ZIPFILE" "$PROCESS_PATH"
rm "$ZIPFILE"

echo -e "Extracting package contents\n"

cd "$PROCESS_PATH"

ZIPFILE=$( ls _osx-x64*tar.gz )
tar -xzf "$ZIPFILE"
rm "$ZIPFILE"

mv "$SIM_PUB_FOLDER"/* "$SIM_APP_NAME/Contents/MacOS"
mv "$SCENED_PUB_FOLDER"/* "$SCENED_APP_NAME/Contents/MacOS"
rmdir "$SIM_PUB_FOLDER"
rmdir "$SCENED_PUB_FOLDER"

echo -e "Settings permissions\n"

chmod +x "$SIM_APP_NAME/Contents/MacOS/$SIM_EXE_FILE"
chmod +x "$SCENED_APP_NAME/Contents/MacOS/$SCENED_EXE_FILE"

len1=`echo $ZIPFILE | wc -c`
len2=$(expr $len1 - 8)
OUTNAME=$(echo $ZIPFILE | cut -c 1-$len2)
VERSION=$(echo $OUTNAME | cut -c 10-)


OUTFILE="infirmary-integrated-$VERSION-macos.tar.gz"
echo -e "Packaging tarball $OUTFILE\n"

rmdir "$BASE_PUB_FOLDER"

tar -czf "$OUTFILE" *

echo -e "Moving package to $DESTINATION_PATH\n"

cp -a "$OUTFILE" "$DESTINATION_PATH"

echo -e "Packaging complete!\n"