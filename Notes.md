

* Steps for publishing a Release:
	- Update version # in:		
		- II Avalonia properties: 1) assembly information and 2) publish tab
        - II Scenario_Editor properties: 1) assembly information and 2) publish tab
		- Installer_Windows properties (and new product key)

	- Comment any testing/debugging shortcuts	
	- Git Commit; Git Push

	- Run II Development Tools\Scripts\publishing-windows.bat (just runs II Development Tools\Publishing Utility)
		- Allow it to compile, package, and move all Releases for various platforms
		- Wait at the prompt to process the Windows package!
	- Build "Package, Windows" project as Release- compiles the .msi installer
	- Continue the II Development Tools\Publishing Utility to move and sign the Windows .msi installer

	- In Linux shell (e.g. Windows Subsystem for Linux)
		- Run II Development Tools\Scripts\publishing-linux.sh (sets file permissions, re-packs as .tar.gz)
		- Run II Development Tools\Scripts\publishing-osx.sh (sets file permissions, re-packs as .app)

	- Create a Github Release
		- Rename all packages to infirmary-integrated-x.y.z-platform
		- Upload all packages, including:
			- Windows: .zip, .msi
			- Linux: .tar.gz
			- OSX: .zip (.app.zip)

	- Update II Server MySQL database
	  - Version #
	  - Installer URL (use Github release link)
	  - Installer MD5 hash: https://emn178.github.io/online-tools/md5_checksum.html

	- Update infirmary-integrated.com's “Downloads” page
	  - Update latest version #
	  - Change link destination for all icons!
	  - Change link destination for text "click here to download"
	  - Update MD5 hash
	- Copy Github Release into a Wordpress post on infirmary-integrated.com


* Color Scheme
	- Blue: 2b79c2
	- Gray: 3b4652

* References:

- Icons: Blue Series by Nicola Simpson
	https://www.iconfinder.com/families/blue-series