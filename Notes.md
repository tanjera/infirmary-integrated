

* Steps for publishing a Windows Release:
	- Update version # in:
		- II Core.Utility.Version
		- II Windows properties: 1) assembly information and 2) publish tab
        - II Scenario_Editor properties: 1) assembly information and 2) publish tab
		- Installer_Windows properties (and new product key)

	- Comment any testing/debugging shortcuts
	- Clean and compile solution and installer
	- Move .msi to Releases
	- Create .zip of .exe folder in Releases
	- GIT COMMIT; GIT PUSH

	- Create a Github Release

	- Update II Server MySQL database
	  - Version #
	  - Installer URL (use Github release link)
	  - Installer MD5 hash: https://emn178.github.io/online-tools/md5_checksum.html

	- Update infirmary-integrated.com's “Downloads” page
	  - Update latest version #
	  - Change link destination for Windows OS icon!
	  - Change link destination for text "click here to download"
	  - Update MD5 hash
	- Copy Github Release into a Wordpress post on infirmary-integrated.com


* Color Scheme
	- Blue: 2b79c2
	- Gray: 3b4652

* References:
- Assembly signing with certificate
	https://www.linkedin.com/pulse/code-signing-visual-studio-jason-brower

- Icons: Blue Series by Nicola Simpson
	https://www.iconfinder.com/families/blue-series