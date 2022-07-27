Name:           infirmary-integrated
Release:        1%{?dist}
Summary:        Infirmary Integrated, a healthcare device simulator.

License:        https://github.com/tanjera/infirmary-integrated/blob/master/License.md
URL:            http://www.infirmary-integrated.com/

Requires:       vlc vlc-devel libX11-devel

%description
Infirmary Integrated is free and open-source software developed to advance healthcare education for medical and nursing professionals and students.

%files
# Ancillary files (desktop shortcuts, mime types)
"/usr/share/applications/infirmary-integrated-scenario-editor.desktop"
"/usr/share/applications/infirmary-integrated.desktop"
"/usr/share/pixmaps/infirmary-integrated.png"
"/usr/share/pixmaps/infirmary-integrated-scenario-editor.png"
"/usr/share/mime/packages/infirmary-integrated.xml"

# Executable scripts
"/usr/bin/infirmary-integrated"
"/usr/bin/infirmary-integrated-scenario-editor"

# The compiled binary filetree: dlls, executables, etc.
"/usr/share/infirmary-integrated/"