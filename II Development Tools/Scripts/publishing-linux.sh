#!/bin/bash
UUID=$(uuidgen)
PROCESS_PATH="/tmp/$UUID"
SOLUTION_PATH="/mnt/c/Users/Ibi/Documents/Infirmary Integrated"
RELEASE_PATH="$SOLUTION_PATH/Release"
PUB_FOLDER="Infirmary Integrated"
PUB_EXE="Infirmary Integrated/Infirmary Integrated"

echo -e ""
echo -e "Creating temp directory $PROCESS_PATH\n"

rm -rf "$PROCESS_PATH"
mkdir "$PROCESS_PATH"

echo -e "Copying package from $RELEASE_PATH\n"

cd "$RELEASE_PATH"

ZIPFILE=$( ls _linux-x64* )

cp "$ZIPFILE" "$PROCESS_PATH"
rm "$ZIPFILE"

echo -e "Extracting package and setting file permissions\n"

cd "$PROCESS_PATH"
tar -xf "$ZIPFILE"
rm "$ZIPFILE"
chmod +x "$PUB_EXE"

len1=`echo $ZIPFILE | wc -c`
len2=$(expr $len1 - 5)
OUTNAME=$(echo $ZIPFILE | cut -c1-$len2)
OUTFILE="$OUTNAME.tar.gz"
echo -e "Rebuilding package to $OUTFILE\n"

tar -czf "$OUTFILE" "$PUB_FOLDER"

echo -e "Moving package to $RELEASE_PATH\n"

mv "$OUTFILE" "$RELEASE_PATH"

echo -e "Removing temp directory $PROCESS_PATH\n"

rm -rf "$PROCESS_PATH"