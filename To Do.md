* Commit message:




* Next steps:
	- Controls.Numeric & Controls.Tracing context menus!
		- Needs complete re-implementation for context menu handling

	- Cardiac Monitor layout polish & debugging
		- Fix buggy "Add Tracing" and "Add Numeric" functionality
		- UIElement positioning... stretching, etc
		- Adjust clamping for "Increase Font Size" to allow for bigger/smaller screens?
			- Test on different window sizes??
		- Implement "Increase Tracing Size"?

	- Fix localization errors, formatting

	- Add splash screen?
		- Compiler #if release version only...

* To debug:

* Known bugs:
	- PA catheter populates beats even on pulseless rhythms...



* Versions and features to implement:

	* Purchase icons!!

	V 0.9: Cross-platform rewrite
		- Implement:
			- Shared library (II_Core)
				- Patient modeling
			- II_Windows
				- Forms
					- Editor (Patient Parameters)
					- Cardiac Monitor
					- About
				- Controls
					- Tracing
					- Numerics
			- II_OSX
				- Forms
					- Editor (Patient Parameters)
					- Cardiac Monitor
					- About
				- Controls
					- Tracing
					- Numerics



* Planned features:
	- Devices
		- 12 lead ECG
		- Defibrillator
		- IABP
		- Ventilator
		- FHM/Toco
		- Lab Values
		- IV Pump
	- Clinical aspects
		- Respiratory Rhythms: Kussmaul, Cheyne-Stokes
			- With vital sign clamping
		- Pacer spikes & rhythms
			- AAI, VVI, DDD
		- ECG axis deviation
		- Pulsus paradoxus
		- Thermodilution
		- PA catheter placement
	- Program features
		- Save/Print screenshot
		- Patient state snapshots



* References:
- Assembly signing with certificate
	https://www.linkedin.com/pulse/code-signing-visual-studio-jason-brower

- Icons: Blue Series by Nicola Simpson
	https://www.iconfinder.com/families/blue-series