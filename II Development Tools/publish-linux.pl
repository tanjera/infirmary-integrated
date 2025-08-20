#!/bin/perl

$pwd = `pwd`;

$rel_dir = "../Release";
$fs_dir = "../Package, Linux/_filesystem";
$deb_dir = "../Package, Linux/DEB";
$rpm_spec = "../Package, Linux/RPM/infirmary-integrated.spec";

opendir($dh, $rel_dir) or die "Cannot open directory $rel_dir: $!";
@rel_files = grep { -f "$rel_dir/$_" } readdir($dh);
closedir($dh);

@to_process = ();

print "Files found:\n";

foreach $file (@rel_files) {
    if ($file =~ /linux/) {
        print " - $file\n";
        push(@to_process, $file);
    }
}

print "\n";

# ######
# Iterate each tarball, package into DEB format
# ######

sub package_deb {
    @parts_file = split(/-/, $_[0]);

    # Get version & architecture from tarball name

    $version = $parts_file[2];
    @parts_arch = split(/\./, $parts_file[4]);
    $arch = $parts_arch[0];

    if ($arch eq "x64") {
        $arch = "amd64";
    }

    print "Version: $version\t\tArchitecture: $arch\n";

    # Create temporary working directory

    $tmp_uuid = `uuidgen`;
    chomp $tmp_uuid;        # Remove trailing newline!

    $tmp_dir = "/tmp/$tmp_uuid";
    print "Creating temporary directory: $tmp_dir\n";

    `mkdir "$tmp_dir"`;

    # Populate temporary working directory with buildroot and .deb structure

    print "Creating buildroot filesystem...\n";
    `cp -r "$fs_dir"/* $tmp_dir`;
    `cp -r "$deb_dir"/* $tmp_dir`;
    `mkdir -p $tmp_dir/usr/share/infirmary-integrated`;

    # Add version & architecture to .deb control file

    @control_append = (
        "Version: $version",
        "Architecture: $arch",
        ""      # control needs a trailing newline!
    );

    $control = "$tmp_dir/DEBIAN/control";

    open($fh, '>>', $control) or die "Could not open file '$control' for appending: $!";
    foreach $line (@control_append) {
        print $fh "$line\n";
    }
    close($fh) or die "Could not close file '$control': $!";

    # Unzip the infirmary-integrated tarball into the buildroot's /usr/share/infirmary-integrated path

    print "Unzipping infirmary-integrated into the buildroot...\n";
    `tar -xzf $rel_dir/$_[0] -C $tmp_dir/usr/share`;


    # Package .deb using dpkg-deb
    print "Packaging the .deb into $rel_dir/infirmary-integrated_$version\_$arch.deb...\n";
    `dpkg-deb --root-owner-group --build $tmp_dir $rel_dir/infirmary-integrated_$version\_$arch.deb`;


    # Remove the temporary working directory
    print "Removing the temporary working directory...\n";
    `rm -rf "$tmp_dir"`;
}

foreach $file (@to_process) {
    print "Processing file into .deb format: $file\n";
    package_deb($file);
    print "Finished processing $file into .deb\n\n";
}