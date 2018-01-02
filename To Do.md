* Commit message:



* Next steps:
	- WPF UI
		- Patient editor
			- numUpDown value, interval, min, max
			- Tie .xaml into .cs functions
		- Cardiac Monitor window
			- Tie .xaml into .cs
			- Layout, Menu, UI...
			- Controls for numerics, tracings

		- ComboBox descriptions via localization lookup table...



* To debug:
	- Timer wrapper
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