* Commit message:


* To debug:
	- Timers are not perfectly synchronous... during loading or CPU bogging
		- Create synchronous timer state machine based on DateTime elapsed?
	- Slower tracings (e.g. ETCO2) glitch during parameter changes



* Versions and features to implement:

	V 0.9: Cross-platform rewrite
		- Implement:
			- Shared library (II_Core)
				- Timer (thread-safe!)
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