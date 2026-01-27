#!/usr/bin/perl

use File::chdir;
use Cwd qw(abs_path);

$script_dir = `pwd`;
$rel_dir = abs_path("../../Release");
$entitlements = abs_path("../../Package, MacOS/entitlements.plist");
$signing_identity = "905C6ACD8AC51DA87E79EAF47F93682CC42D56A2";

# Populate @to_process with Release packages found

opendir($dh, $rel_dir) or die "Cannot open directory $rel_dir: $!";
@rel_files = grep { -f "$rel_dir/$_" } readdir($dh);
closedir($dh);

@to_process = ();

foreach $file (@rel_files) {
    if ($file =~ /osx/) {
        push(@to_process, $file);
    }
}

print "Files found for processing: " . scalar(@to_process) . "\n";
foreach $file (@to_process) {
    print " - $file\n";
}

print "\n";

sub process_package {
    print "Processing package: " . $file . "\n";

    $tmp_uuid = `uuidgen`;
    chomp $tmp_uuid;        # Remove trailing newline!

    $tmp_dir = "/tmp/$tmp_uuid";
    print "Creating temporary directory: $tmp_dir\n";

    `mkdir "$tmp_dir"`;

    print "Unzipping $file to $tmp_dir ...\n";
    `unzip $rel_dir/$file -d $tmp_dir`;

    `rm $rel_dir/$file`;

    @to_iter = ("Infirmary Integrated.app",
        "Infirmary Integrated Scenario Editor.app");

    foreach $proj (@to_iter) {
        $app_dir = "$tmp_dir/$proj/Contents/MacOS";

        print "Iterating $app_dir\n";

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

        print "codesign verification:\n";
        `codesign --verify --verbose "$tmp_dir/$proj"`;        

        print "Zipping signed project back into $file\n\n";
        chdir("$tmp_dir");        
        `zip -r "$rel_dir/$file" "$proj"`;
    }

    print "Removing temporary directory $tmp_dir";
    chdir("$script_dir");
    `rm -r "$tmp_dir"`;
}


foreach $file (@to_process) {
    process_package($file);
}