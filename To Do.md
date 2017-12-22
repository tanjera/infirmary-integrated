* Commit message:



* To debug:
	- Timers are not perfectly synchronous... during loading or CPU bogging
		- Create synchronous timer state machine based on DateTime elapsed?
	- Slower tracings (e.g. ETCO2) glitch during parameter changes



* Versions and features to implement:

	- V 0.8
		- More tracings: CVP, PA
		* Reimplement default vital sign clamping...

		- Device options
			- Save screenshot
			- Print screenshot


	- V 0.9
		- 12 lead ECG device form (Device_ECG)
			- Fixed layout
			- Option to pause for viewing, printing, or export to .pdf
		- Implement axis deviation in ECG tracings
		- Implement pulsus paradoxus
		- Add default Rhythm vital signs for all hemodynamic parameters (Get_Rhythm().Vitals())


	- V 1.0
		* General polish and debugging


	- V 1.1
		- Defibrillator device form (Device_Defibrillator)
			- Limited amount of tracing rows (limit 3?); less dynamic layout than cardiac monitor
			- Faceplate with buttons for energy selection, charge, delivery shock
			- Additional control for display of Joule amount selected... or add to ECG tracing corner
			- Defibrillation rhythm waveform (can be 1 beat)


	- V 1.2
		- IABP device form
			- Faceplate with fixed layout/tracings
		- IABP waveforms


	- V 1.3
		- Timing indicators for strip tracings (3 second tick marks, etc)
		- Paced rhythms (AAI, VVI, DDI)
			- Event/delegate triggers onCardiac_PaceAtria, onCardiac_PaceVentricles
		- PA catheter improvements
			- Thermodilution profile dialog form
			- Additional PA waveforms (RA, RV, PA, PAW)
			- Option to select waveform ("placement" of PA cath?)



* Feature ideas, needs placing in version plan:
	- Numeric control improvements
		- Add buttons for added functionality (e.g. zero ABP; cycle NiBP; run thermodilution; run 12 lead)
		* Buttons can be disabled until functionality implemented...

	- More respiratory content
		- ETCO2 waveform coefficients
	- Ventilators... other devices...



* References:
- Assembly signing with certificate
	https://www.linkedin.com/pulse/code-signing-visual-studio-jason-brower

- 12 lead ECG waveforms
	https://ekg.academy/
- 12 lead axis interpretation
	https://lifeinthefastlane.com/ecg-library/basics/axis/
- CVP waveform
	http://www.dynapulse.com/educator/webcurriculum/chapter%203/abnormal%20ekg%20and%20waveform.htm
- PA waveforms
	http://mdnxs.com/topics-2/procedures/swan-ganz-catheter/
- IABP waveforms
	https://rk.md/2017/intra-aortic-balloon-pump-arterial-line-ekg-waveforms/
- ETCO2 waveform interpretation
	http://www.jems.com/articles/print/volume-42/issue-8/features/how-to-read-and-interpret-end-tidal-capnography-waveforms.html