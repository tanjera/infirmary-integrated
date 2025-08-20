#!/bin/bash

# ####
# Color Constants
# ####

# Reset
Reset='\033[0m'           # Text Reset
# Regular Colors
Black='\033[0;30m'        # Black
Red='\033[0;31m'          # Red
Green='\033[0;32m'        # Green
Yellow='\033[0;33m'       # Yellow
Blue='\033[0;34m'         # Blue
Purple='\033[0;35m'       # Purple
Cyan='\033[0;36m'         # Cyan
White='\033[0;37m'        # White
# Bold
BBlack='\033[1;30m'       # Black
BRed='\033[1;31m'         # Red
BGreen='\033[1;32m'       # Green
BYellow='\033[1;33m'      # Yellow
BBlue='\033[1;34m'        # Blue
BPurple='\033[1;35m'      # Purple
BCyan='\033[1;36m'        # Cyan
BWhite='\033[1;37m'       # White


# ####
# General Constants
# ####

SCRIPT_PATH=$( pwd )
cd ..
SOLUTION_PATH=$( pwd )

OUT_PREFIX="\n${BCyan}>>>>>${Reset} "


# ####
# Error checking before beginning script
# ####

if ! command -v uuidgen &> /dev/null; then
    echo "Error: uuidgen could not be found"
    echo "Please install package uuid-runtime"
    exit
fi

if ! command -v zip &> /dev/null; then
    echo "Error: zip could not be found"
    echo "Please install package zip"
    exit
fi

if ! command -v 7z &> /dev/null; then
    echo "Error: 7z could not be found"
    echo "Please install package p7zip"
    exit
fi

if ! command -v dpkg-deb &> /dev/null; then
    echo "Error: dpkg-deb could not be found"
    echo "Please install package"
    exit
fi

if ! command -v rpmbuild &> /dev/null; then
    echo "Error: rpmbuild could not be found"
    echo "Please install package"
    exit
fi