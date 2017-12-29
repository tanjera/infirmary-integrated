* Commit message:



* Next steps:
	- WPF UI
		- Finish recreating UI
		  - Patient editor
			- All parameters
			- Main menu
		  - About program dialog
		  - Language selection dialog
		  - Cardiac monitor device window

		- Implement NumUpDown functionality



* To debug:
	- Timer wrapper
* Known bugs:
	- PA catheter populates beats even on pulseless rhythms...



* Versions and features to implement:

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
		- Lab Values
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