#!/usr/bin/perl

use File::chdir;
use Cwd qw(abs_path);

$script_dir = `pwd`;
$rel_dir = abs_path("../../Release");
$entitlements = abs_path("../../Package, MacOS/entitlements.plist");
$signing_identity = "905C6ACD8AC51DA87E79EAF47F93682CC42D56A2";
$packing_identity = "922C0261BDA27C1E4C02CB454BB1853F1A5A91D3";

@apps = ("Infirmary Integrated.app",
    "Infirmary Integrated Scenario Editor.app");

# Populate @to_process with Release packages found

opendir($dh, $rel_dir) or die "Cannot open directory $rel_dir: $!";
@rel_files = grep { -f "$rel_dir/$_" } readdir($dh);
closedir($dh);

$zip_x64 = "";
$zip_arm = "";

foreach $file (@rel_files) {
    if ($file =~ /osx-arm64/) {
        $zip_arm = $file;
    } elsif ($file =~ /osx-x64/) {
        $zip_x64 = $file;
    }
}

if ($zip_arm eq "") {
    print "Missing arm64 build! Cannot 'lipo' without both arm64 & amd64 builds. Exiting.\n";
    exit();
} elsif ($zip_x64 eq "") {
    print "Missing amd64 build! Cannot 'lipo' without both arm64 & amd64 builds. Exiting.\n";
    exit();
}

$tmp_arm_uuid = `uuidgen`;
$tmp_x64_uuid = `uuidgen`;
$tmp_uni_uuid = `uuidgen`;
chomp $tmp_arm_uuid;        # Remove trailing newline!
chomp $tmp_x64_uuid;        # Remove trailing newline!
chomp $tmp_uni_uuid;        # Remove trailing newline!
$tmp_dir_arm = "/tmp/$tmp_arm_uuid";
$tmp_dir_x64 = "/tmp/$tmp_x64_uuid";
$tmp_dir_uni = "/tmp/$tmp_uni_uuid";

# ##########
# Unpack the arm and x64 packages into their respective directories
# ##########

sub unpack_files {
    my ($file, $tmp_dir) = @_;

    print "\n";
    print "Processing package for signing: " . $file . "\n";    
    print "Creating temporary directory: $tmp_dir\n";

    `mkdir "$tmp_dir"`;

    print "Unzipping $file to $tmp_dir ...\n";
    `unzip $rel_dir/$file -d $tmp_dir`;

    `rm $rel_dir/$file`;
}

unpack_files($zip_arm, $tmp_dir_arm);
unpack_files($zip_x64, $tmp_dir_x64);


# ##########
# Create the universal binary with lipo
# ##########

print "\nCreating temporary folder for universal binary: $tmp_dir_uni\n\n";
`cp -r $tmp_dir_x64 $tmp_dir_uni`;

foreach $proj (@apps) {
    $app_dir_arm = "$tmp_dir_arm/$proj/Contents/MacOS";
    $app_dir_x64 = "$tmp_dir_x64/$proj/Contents/MacOS";
    $app_dir_uni = "$tmp_dir_uni/$proj/Contents/MacOS";    

    opendir($dh, $app_dir_arm) or die "Cannot open directory $app_dir_arm: $!";
    @lipo_files = grep { -f "$app_dir_arm/$_" } readdir($dh);
    closedir($dh);

    foreach $file (@lipo_files) {

        if ($file =~ /$.ico/ || $file =~ /$.pdb/) {
            print "Skipping non-code file: $app_dir_uni/$file\n";
        } elsif ($file =~ /\Qlibvlc.dylib\E/){
            print "Skipping libvlc.dylib- only x86 and needs emulation\n";
        } else {
            $info = `lipo -info "$app_dir_uni/$file"`;
            if ($info =~ /^Non-fat/) {
                print "Creating lipo universal file: $app_dir_uni/$file\n";
                `rm "$app_dir_uni/$file"`;
                `lipo -create "$app_dir_arm/$file" "$app_dir_x64/$file" -output "$app_dir_uni/$file"`;
            } else {
                print "Skipping file: $app_dir_uni/$file\n";
            }
        }
    }
}

# ##########
# Sign the files before being packaged
# ##########

sub sign_files {
    my ($tmp_dir, @apps) = @_;

    foreach $proj (@apps) {
        $app_dir = "$tmp_dir/$proj/Contents/MacOS";

        print "\nIterating $app_dir\n";

        opendir($dh, $app_dir) or die "Cannot open directory $app_dir: $!";
        @sign_files = grep { -f "$app_dir/$_" } readdir($dh);
        closedir($dh);
        
        print "Signing all files in $app_dir ";
        
        foreach $file (@sign_files) {
            print ".";
            `codesign --force --timestamp --options=runtime --entitlements "$entitlements" --sign "$signing_identity" "$app_dir/$file"`;
        }

        print "\n";

        print "Signing the .app at $tmp_dir/$proj\n";
        `codesign --force --timestamp --options=runtime --entitlements "$entitlements" --sign "$signing_identity" "$tmp_dir/$proj"`;

        print "\ncodesign verification for $tmp_dir/$proj:\n";
        `codesign --verify --verbose "$tmp_dir/$proj"`;
        print "\n";
    }
}

sign_files($tmp_dir_uni, @apps);


# ##########
# Assemble into .pkg with 
# ##########

$pkg_file = $zip_arm;
$pkg_file =~ s/.zip/.pkg/;
$pkg_file =~ s/osx-arm64/osx-universal/;
$pkg_fp = "$rel_dir/$pkg_file";

print "Assembling .pkg to $pkg_fp\n";
`productbuild --sign "$packing_identity" --component "$tmp_dir_uni/@apps[0]" /Applications  --component "$tmp_dir_uni/@apps[1]" /Applications "$pkg_fp"`;

print "Removing temporary working directories\n";
`rm -r "$tmp_dir_arm"`;
`rm -r "$tmp_dir_x64"`;
`rm -r "$tmp_dir_uni"`;


# ##########
# Notarize the package with notarytool then staple
# ##########

print "\nNotarizing the package...\n";
system("xcrun notarytool submit \"$pkg_fp\" --keychain-profile \"notarytool-password\" --wait");

print "\nStapling the notary ticket...\n";
system("xcrun stapler staple \"$pkg_fp\"");
