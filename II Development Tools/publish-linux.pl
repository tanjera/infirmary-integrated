#!/bin/perl

$pwd = `pwd`;

$rel_dir = "../Release";
$fs_dir = "../Package, Linux/_filesystem";
$deb_dir = "../Package, Linux/DEB";
$rpm_spec = "../Package, Linux/RPM/infirmary-integrated.spec";

# Ensure filesystem files (scripts, .desktop shortcuts) are executable

@to_chmod = ("$fs_dir/usr/bin/infirmary-integrated", 
    "$fs_dir/usr/bin/infirmary-integrated-scenario-editor",
    "$fs_dir/usr/share/applications/infirmary-integrated.desktop",
    "$fs_dir/usr/share/applications/infirmary-integrated-scenario-editor.desktop");

print "Setting chmod +x for scripts and links: ";

foreach $file (@to_chmod) {
    if (-e $file) {
        print "✓";
        `chmod +x \"$file\"`;
    } else {
        print "\nFile not found! \"$file\"\n";
    }
}
print "\n\n";

# Populate @to_process with Release packages found

opendir($dh, $rel_dir) or die "Cannot open directory $rel_dir: $!";
@rel_files = grep { -f "$rel_dir/$_" } readdir($dh);
closedir($dh);

@to_process = ();

foreach $file (@rel_files) {
    if ($file =~ /linux/) {
        push(@to_process, $file);
    }
}

print "Files found for processing: " . scalar(@to_process) . "\n";
foreach $file (@to_process) {
    print " - $file\n";
}

print "\n";

sub package_fpm {

    # #####
    # Package files using the effing package manager (fpm)
    # https://github.com/jordansissel/fpm
    # #####

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
    `mkdir -p $tmp_dir/usr/share/infirmary-integrated`;

    # Unzip the infirmary-integrated tarball into the buildroot's /usr/share/infirmary-integrated path

    print "Unzipping infirmary-integrated into the buildroot...\n";
    `tar -xzf $rel_dir/$_[0] -C $tmp_dir/usr/share`;


    # Package .deb using fpm
    print "Using fpm to build a .deb...\n\n";
    $fpm_command = "fpm \\
    -s dir -t deb --force \\
    --package \"$rel_dir\" \\
    --name infirmary-integrated \\
    --vendor \"Infirmary Integrated\" \\
    --license \"Apache 2.0\" \\
    --description \"Infirmary Integrated is free and open-source software developed to advance healthcare education for medical and nursing professionals and students. Developed as in-depth, accurate, and accessible educational tools, Infirmary Integrated can meet the needs of clinical simulators in emergency, critical care, and many other medical and nursing specialties\" \\
    --url \"https://www.infirmary-integrated.com\" \\
    --maintainer \"Ibi Keller <ibi.keller@gmail.com>\" \\
    --version $version \\
    --architecture $arch \\
    --depends vlc --depends libvlc-dev --depends libx11-dev \\
    -C $tmp_dir \\
    ."; 
    print "$fpm_command\n\n";
    `$fpm_command`;


    # Package .rpm using fpm
    print "Using fpm to build a .rpm...\n\n";
    $fpm_command = "fpm \\
    -s dir -t rpm --force \\
    --package \"$rel_dir\" \\
    --name infirmary-integrated \\
    --vendor \"Infirmary Integrated\" \\
    --license \"Apache 2.0\" \\
    --description \"Infirmary Integrated is free and open-source software developed to advance healthcare education for medical and nursing professionals and students. Developed as in-depth, accurate, and accessible educational tools, Infirmary Integrated can meet the needs of clinical simulators in emergency, critical care, and many other medical and nursing specialties\" \\
    --url \"https://www.infirmary-integrated.com\" \\
    --maintainer \"Ibi Keller <ibi.keller@gmail.com>\" \\
    --version $version \\
    --architecture $arch \\
    --depends vlc --depends vlc-devel --depends libX11-devel \\
    -C $tmp_dir \\
    ."; 
    print "$fpm_command\n\n";
    `$fpm_command`;


    # Remove the temporary working directory
    print "Removing the temporary working directory...\n";
    `rm -rf "$tmp_dir"`;
}


foreach $file (@to_process) {

    # Ensure the effing package manager (fpm) works    
    system("command -v fpm > /dev/null 2>&1");
    if ($? == 0) {
        print "fpm exists and is executable... proceeding\n\n";
    } else {
        print "fpm does *not* exist or is *not* executable!\n";
        print "Ensure fpm and dependencies are installed!\n";
        print "Read for installation instructions: https://fpm.readthedocs.io/en/latest/installation.html\n";
        print "Or visit the project page: https://github.com/jordansissel/fpm\n";
        print "Exiting now!\n";
        exit(0);
    }

    # Ensure rpmbuild is installed    
    system("command -v rpmbuild > /dev/null 2>&1");
    if ($? == 0) {
        #print "fpm exists and is executable... proceeding\n\n";
    } else {
        print "rpmbuild is missing!\n";
        print "Please install rpmbuild (`apt install rpm`)!\n";
        print "Exiting now!\n";
        exit(0);
    }


    print "Processing file into fpm package manager: $file\n";
    package_fpm($file);
    print "Finished processing $file\n\n";
}
