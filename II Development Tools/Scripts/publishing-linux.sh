#!/bin/bash

echo "Creating temp directory"

rm -rf ~/temp
mkdir ~/temp

echo "Copying package"

cd /mnt/y/Infirmary\ Integrated\,\ Avalonia/Release/

zipfile=$( ls _linux-x64* )

cp $zipfile ~/temp
rm $zipfile

echo "Extracting package"

cd ~/temp
tar -xf $zipfile
rm $zipfile
chmod +x Infirmary\ Integrated/Infirmary\ Integrated

echo "Rebuilding package"

len1=`echo $zipfile | wc -c`
len2=$(expr $len1 - 5)
outfile=$(echo $zipfile | cut -c1-$len2)

tar -czf $outfile.tar.gz Infirmary\ Integrated

mv _linux-x64*.tar.gz /mnt/y/Infirmary\ Integrated\,\ Avalonia/Release/
rm -rf ~/temp