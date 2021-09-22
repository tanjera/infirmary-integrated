#!/bin/bash
APP_NAME="Infirmary Integrated.app"
PUBLISH_OUTPUT_DIRECTORY="/mnt/y/Infirmary Integrated, Avalonia/II Avalonia/bin/Release/net5.0/osx-x64/publish"
INFO_PLIST="/mnt/y/Infirmary Integrated, Avalonia/II Avalonia/Info.plist"
ICON_PATH="/mnt/y/Infirmary Integrated, Avalonia/II Avalonia/Icon_II.icns"
ICON_FILE="Icon_II.icns"
EXE_FILE="Infirmary Integrated"
DESTINATION_PATH="/mnt/y/Infirmary Integrated, Avalonia/Release"

cd ..

if [ -d "$APP_NAME" ]
then
    rm -rf "$APP_NAME"
fi

mkdir -p "$APP_NAME"

mkdir -p "$APP_NAME/Contents"
mkdir -p "$APP_NAME/Contents/MacOS"
mkdir -p "$APP_NAME/Contents/Resources"

cp "$INFO_PLIST" "$APP_NAME/Contents/Info.plist"
cp "$ICON_PATH" "$APP_NAME/Contents/Resources/$ICON_FILE"
cp -a "$PUBLISH_OUTPUT_DIRECTORY"/* "$APP_NAME/Contents/MacOS"
chmod +x "$APP_NAME/Contents/MacOS/$EXE_FILE"

echo "Moving package to destination"

cp -a "$APP_NAME" "$DESTINATION_PATH"