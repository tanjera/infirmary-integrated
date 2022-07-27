#!/bin/sh

REPO_BASE="/home/tanjera/infirmary-integrated.com/packages/apt-repo"
POOL_MAIN="$REPO_BASE/pool/main"

DIST="bullseye"
RELEASE_DIR="$REPO_BASE/dists/$DIST"
PACKAGE_DIR="$REPO_BASE/dists/$DIST/main/binary-amd64"


# ####
# Function definition for hashing
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


# ####
# Ensuring filetree is created
# ####

echo -e "Creating filetree \n"

mkdir -p "$POOL_MAIN"
mkdir -p "$PACKAGE_DIR"


# ####
# Fetching the new package from GitHub
# ####

if [ -z "$1" ]; then
    echo "Usage: apt-release.sh github.com/address/of/package.deb"
    exit
fi

echo -e "Fetching package with wget \n"

cd "$POOL_MAIN"
wget "$1"


# ####
# Creating the Packages file and zipping it
# ####

echo -e "Creating 'Packages' with dpkg-scanpackages \n"

# Note: dpkg-scanpackages requires the binary path to be a RELATIVE path to repo_base
cd "$REPO_BASE"
dpkg-scanpackages --arch amd64 "pool/main" > "$PACKAGE_DIR/Packages"
cat "$PACKAGE_DIR/Packages" | gzip -9 > "$PACKAGE_DIR/Packages.gz"


# ####
# Creating the Release file
# ####

# Remove the old Release and InRelease files
rm "$RELEASE_DIR/Release"
rm "$RELEASE_DIR/InRelease"

echo -e "Creating 'Release' \n"

# Echo the new Release file
cd "$RELEASE_DIR"
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

echo -e "Signing 'Release' with gpg \n"

export GPG_TTY=$(tty)
cat "$RELEASE_DIR/Release" | gpg -abs --clearsign > "$RELEASE_DIR/InRelease"