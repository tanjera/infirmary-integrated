#!/bin/bash

# ####
# Usage: Run in Linux or MacOS *after* II Dev Tools Publishing Utility (called by publishing-windows.bat)!
# Will repackage build output into a .tar.gz with appropriate MacOS file structure
# ####

SOLUTION_PATH="/home/ibi/Documents/Infirmary Integrated"

UUID=$(uuidgen)
PROCESS_PATH="/tmp/$UUID"

APP_NAME="Infirmary Integrated.app"
PUBLISH_OUTPUT_DIRECTORY="$SOLUTION_PATH/II Avalonia/bin/Release/net5.0/osx-x64/publish"
INFO_PLIST="$SOLUTION_PATH/Package, MacOS/Info.plist"
ICON_PATH="$SOLUTION_PATH/Package, MacOS/Icon_II.icns"
DESTINATION_PATH="$SOLUTION_PATH/Release"
ICON_FILE="Icon_II.icns"
EXE_FILE="Infirmary Integrated"

rm -rf "$PROCESS_PATH"
mkdir "$PROCESS_PATH"
cd "$PROCESS_PATH"

echo -e ""
echo -e "Creating app structure in $PROCESS_PATH\n"

mkdir -p "$APP_NAME"
mkdir -p "$APP_NAME/Contents"
mkdir -p "$APP_NAME/Contents/MacOS"
mkdir -p "$APP_NAME/Contents/Resources"

echo -e "Copying package contents\n"

cp "$INFO_PLIST" "$APP_NAME/Contents/Info.plist"
cp "$ICON_PATH" "$APP_NAME/Contents/Resources/$ICON_FILE"
cp -a "$PUBLISH_OUTPUT_DIRECTORY"/* "$APP_NAME/Contents/MacOS"

echo -e "Settings permissions\n"

chmod +x "$APP_NAME/Contents/MacOS/$EXE_FILE"

OUTFILE="_osx-64-app-package.tar.gz"
echo -e "Packaging tarball $OUTFILE\n"

tar -czf "$OUTFILE" "$APP_NAME"

echo -e "Moving package to $DESTINATION_PATH\n"

cp -a "$OUTFILE" "$DESTINATION_PATH"

echo -e "Packaging complete!\n"