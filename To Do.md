* Commit message:



* To debug:
	- Timers are not perfectly synchronous... during loading or CPU bogging
		- Create synchronous timer state machine based on DateTime elapsed?
	- Slower tracings (e.g. ETCO2) glitch during parameter changes



* Versions and features to implement:

	- V 0.8
		- Respiratory rhythm vital sign clamping
		- Respiratory rhythms: Kussmaul, Cheyne-Stokes



* Rewrite to multi-platform:
	- XF? or Native UI?
	- Individual VS projects for each platform
		- Shared code in II.dll


		
* Planned features:
	- Devices
		- 12 lead ECG
		- Defibrillator
		- IABP
		- Ventilator
		- Lab Values
	- Clinical aspects
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