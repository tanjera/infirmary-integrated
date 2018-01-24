* Commit message:




* Next steps:
	* Localization
		- Convert highlighted terms...

	* IABP
		- DeviceIABP layout for control panel
			- Automatic rearranging of layout (horizontal/vertical) depending on window aspect ratio
		- Device logic
			- Before starting:
				1) Arterial line must be zeroed (if pressure trigger)
				2) Balloon must be filled
			- When changing modes:
				1) Put into standby mode
		- IABPNumeric for IABP readings (augmentation, augmented BP)
		- References
			Console: https://image.slidesharecdn.com/chakri-160614085310/95/iabp-13-638.jpg?cb=1466134420


	* EFM
		- DeviceEFM layout
		- EFMTracing control
		- Rhythms (Rhythms.cs)
			- FHR, UA
		- References
			- Perinatology EFM: http://perinatology.com/Fetal%20Monitoring/Intrapartum%20Monitoring.htm
			- 3-Tier FHR Interpretation: https://perinatalweb.org/themes/wapc/assets/docs/three%20tiered,%20three%20category%20fhr%20interpretation%20system%20_july,%202010_.pdf


	- Splash screen?
		- Compiler #if release version only...

	- Strips X axis not time-based, locked to DateTime...
		- Causes "arrhythmia" on CPU load/window dragging



* To debug:

* Known bugs:
	- Pulsed rhythms (ABP, IABP_ABP, SPO2, PA) generate waveform too soon to QRS complex
		- Needs time for electrical capture to translate to mechanical capture...



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



* Localization Volunteers:
	- DEU: ??
    - ESP: ??
    - FRA: ??
    - ITA: ??
	- KOR: Sujung C
    - PTB: ??
    - RUS: ??
	- SWK: Ruth G



* References:
- Assembly signing with certificate
	https://www.linkedin.com/pulse/code-signing-visual-studio-jason-brower

- Icons: Blue Series by Nicola Simpson
	https://www.iconfinder.com/families/blue-series