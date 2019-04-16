* Commit message:




* Next steps:
	* Defibrillator
		- Functionality for:
			- Pacing functionality
				- Will need capture threshold on Patient Editor
				- Pause button
			- Biphasic vs monophasic options?

	* EFM
		- DeviceEFM layout
		- EFMTracing control
		- Rhythms (Rhythms.cs)
			- FHR, UA
		- References
			- Perinatology EFM: http://perinatology.com/Fetal%20Monitoring/Intrapartum%20Monitoring.htm
			- 3-Tier FHR Interpretation: https://perinatalweb.org/themes/wapc/assets/docs/three%20tiered,%20three%20category%20fhr%20interpretation%20system%20_july,%202010_.pdf

	* Connect via TCP/IP for Patient Editor


* Long-term to-do:

	- DeviceIABP & DeviceDefib
		- Add timer for delays (e.g. priming balloon takes 10 seconds; charging defibrillator takes 5 seconds)

	- Splash screen?
		- Compiler #if release version only...

	- Strips X axis not time-based, locked to DateTime...
		- Causes "arrhythmia" on CPU load/window dragging



* To debug:



* Versions and features to implement:

	- Rhythm Strips
		- Normalize() rhythm strips before drawing (normalize amplitude between -1.0 to 1.0)
		- PatientEditor: Slider bar with offset needing to be zeroed?
	- Cardiac Rhythms:
		- Chest compressions

	- DeviceDefibrillator
	- DeviceVentilator



* Planned features:
	- Program features
		- Save/Print screenshot

	- Clinical aspects
		- Respiratory Rhythms: Kussmaul, Cheyne-Stokes
			- With vital sign clamping
		- Pacer spikes & rhythms
			- AAI, VVI, DDD
		- ECG axis deviation
		- Pulsus paradoxus
		- Thermodilution
		- PA catheter placement



* References:
- Assembly signing with certificate
	https://www.linkedin.com/pulse/code-signing-visual-studio-jason-brower

- Icons: Blue Series by Nicola Simpson
	https://www.iconfinder.com/families/blue-series