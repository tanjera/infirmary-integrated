#!/bin/sh

# ####
# Fetching the new package from GitHub
# ####

if [ -z "$1" ]; then
    echo "Usage: apt-release.sh github.com/address/of/debian_package.deb"
    exit
fi

cd ~/infirmary-integrated.com/packages/apt-repo/pool/main
wget $1


# ####
# Creating the Packages file and zipping it
# ####

cd ~/infirmary-integrated.com/packages/apt-repo
dpkg-scanpackages --arch amd64 pool/main/ > dists/bullseye/main/binary-amd64/Packages
cat dists/bullseye/main/binary-amd64/Packages | gzip -9 > dists/bullseye/main/binary-amd64/Packages.gz


cd ~/infirmary-integrated.com/packages/apt-repo/dists/bullseye/
rm ./Release
rm ./InRelease

# ####
# Creating the Release file
# ####

do_hash() {
    HASH_NAME=$1
    HASH_CMD=$2
    echo "${HASH_NAME}:"
    for f in $(find -type f); do
        f=$(echo $f | cut -c3-) # remove ./ prefix
        if [ "$f" = "Release" ]; then
            continue
        fi
        echo " $(${HASH_CMD} ${f}  | cut -d" " -f1) $(wc -c $f)"
    done
}

echo "Label: Infirmary Integrated" >> Release
echo "Suite: bullseye" >> Release
echo "Codename: stable" >> Release
echo "Version: 1.0" >> Release
echo "Architectures: amd64" >> Release
echo "Components: main" >> Release
echo "Description: Infirmary Integrated's apt software repository" >> Release
echo "Date: $(date -Ru)" >> Release

MD5=$(do_hash "MD5Sum" "md5sum")
SHA1=$(do_hash "SHA1" "sha1sum")
SHA256=$(do_hash "SHA256" "sha256sum")

echo "$MD5" >> Release
echo "$SHA1" >> Release
echo "$SHA256" >> Release


# ####
# Signing the Release file into the InRelease file
# ####

export GPG_TTY=$(tty)
cat Release | gpg -abs --clearsign > InRelease