* Commit message:



- Next Steps:
  - Clean up Mirror.cs & Server.cs (function names, conventions)
  - Implement UI! for mirror host and client



* Steps for Version Publishing:
	- Update version # in:
		- II_Core.Utility.Version
		- II_Windows properties
		- II_Windows.App assembly information
		- Installer_Windows properties

	- Comment any testing/debugging shortcuts
	- Clean and compile solution and installer
	- Move .msi to Releases
	- Create .zip of .exe folder in Releases
	- GIT COMMIT; GIT PUSH

	- Then update version # in:
		- MySQL database
	- Create a Github Release
	- Update infirmary-integrated.com's “Downloads” page
	- Copy Github Release into a Wordpress post on infirmary-integrated.com


* References:
- Assembly signing with certificate
	https://www.linkedin.com/pulse/code-signing-visual-studio-jason-brower

- Icons: Blue Series by Nicola Simpson
	https://www.iconfinder.com/families/blue-series