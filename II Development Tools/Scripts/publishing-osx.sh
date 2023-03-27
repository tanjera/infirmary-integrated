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

RELEASES="osx-x64 osx-arm64"
for RELEASE in $RELEASES; do

    ZIPFILE=$( ls "$RELEASE_PATH/"infirmary-integrated-*-$RELEASE.tar.gz )

    if ! test -f "$ZIPFILE" ; then
        echo "Error: infirmary-integrated-[version]-$RELEASE.tar.gz missing from Infirmary Integrated/Release publish folder"
        exit
    fi

    # ####
    # Define items necessary for creating .app directory structures
    # ####

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

    # ####
    # Create .app directory structures
    # ####

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

    # ####
    # Move MacOS-specific package files into .app structure
    # ####

    echo -e "Copying package contents\n"

    cp "$SIM_INFO_PLIST" "$SIM_APP_NAME/Contents/Info.plist"
    cp "$SIM_ICON_PATH" "$SIM_APP_NAME/Contents/Resources/$SIM_ICON_FILE"

    cp "$SCENED_INFO_PLIST" "$SCENED_APP_NAME/Contents/Info.plist"
    cp "$SCENED_ICON_PATH" "$SCENED_APP_NAME/Contents/Resources/$SCENED_ICON_FILE"

    # ####
    # Move the Infirmary Integrated MacOS release build into the processing folder and extract
    # ####

    echo -e "Moving package for processing from $RELEASE_PATH\n"

    cp "$ZIPFILE" "$PROCESS_PATH"
    rm "$ZIPFILE"

    echo -e "Extracting package contents\n"

    cd "$PROCESS_PATH"

    ZIPFILE=$( ls infirmary-integrated-*-$RELEASE.tar.gz )
    tar -xzf "$ZIPFILE"
    rm "$ZIPFILE"

    # ####
    # Move the Infirmary Integrated into the MacOS .app directory structure
    # ####

    mv "$SIM_PUB_FOLDER"/* "$SIM_APP_NAME/Contents/MacOS"
    mv "$SCENED_PUB_FOLDER"/* "$SCENED_APP_NAME/Contents/MacOS"
    rmdir "$SIM_PUB_FOLDER"
    rmdir "$SCENED_PUB_FOLDER"

    echo -e "Setting permissions\n"

    chmod +x "$SIM_APP_NAME/Contents/MacOS/$SIM_EXE_FILE"
    chmod +x "$SCENED_APP_NAME/Contents/MacOS/$SCENED_EXE_FILE"

    # ####
    # Pack the .app's into a tarball and move back to Release folder
    # ####

    echo -e "Packaging tarball $ZIPFILE\n"

    rmdir "$BASE_PUB_FOLDER"

    tar -czf "$ZIPFILE" *

    echo -e "Moving package to $DESTINATION_PATH\n"

    cp -a "$ZIPFILE" "$DESTINATION_PATH"

    echo -e "Packaging complete for $RELEASE!\n"

done