* Commit message:



* To debug:
	- In Dialog_Main, when button "Reset Patient" is clicked, combo box "Cardiac Rhythm" is not reset



* Versions and features to implement:
	- v 0.6	
		- More tracings: CVP, ETCO2, PA, RR
		- Paced rhythms (AAI, VVI, DDI)
		
		- Sync respiratory waveforms to inspiration/expiration events (e.g. ETCO2)
		- Sync cardiac dependent waveforms to heartbeat event (e.g. SpO2, ABP, PA, CVP)		
			- Event/delegate triggers onCardiac_PaceAtria, onCardiac_PaceVentricles
			- Event/delegate triggers for onCardiac_Beat, onRespiratory_Inspiration, onRespiratory_Expiration

		- Patient boolean for inspiration or expiration; also needs boolean for pos-pressure ventilation or natural neg-pressure ventilation
		
		- Expand Patient class and Edit Patient form	
			- Add default Rhythm vital signs for all hemodynamic parameters (Get_Rhythm().Vitals())		


	- v 0.7
		- Device options						
			- Save screenshot
			- Print screenshot			
		- Numeric control improvements
			- Add buttons for added functionality (e.g. zero ABP; cycle NiBP; run thermodilution; run 12 lead)
			* Buttons can be disabled until functionality implemented...		


	- v 0.8
		- 12 lead ECG device form (Device_ECG)
			- Fixed layout
			- Option to pause for viewing, printing, or export to .pdf


	- v 0.9
		- Implement pulsus paradoxus 
		- Implement axis deviation in ECG tracings 
		- PA catheter improvements
			- Thermodilution profile dialog form
			- Additional PA waveforms (RA, RV, PA, PAW)
			- Option to select waveform ("placement" of PA cath?)


	- v 1.0
		* General polish and debugging		
		* Save/load functionality!!
			- File extension .ii
			- SSH encrypt save files (for testing mode)
			- Import invidual fields and assign to objects (not serialized out/in)
			- For backwards compatibility with save files...
		- Installer package .msi ...
		
	
	- v 1.1
		- Defibrillator device form (Device_Defibrillator)
			- Limited amount of tracing rows (limit 3?); less dynamic layout than cardiac monitor
			- Faceplate with buttons for energy selection, charge, delivery shock
			- Additional control for display of Joule amount selected... or add to ECG tracing corner
			- Defibrillation rhythm waveform (can be 1 beat)
		

	- v 1.2
		- IABP device form
			- Faceplate with fixed layout/tracings
		- IABP waveforms



* Feature ideas, needs placing in version plan:	
	



* References:
- 12 lead ECG waveforms
	https://ekg.academy/
- 12 lead axis interpretation
	https://lifeinthefastlane.com/ecg-library/basics/axis/
- ABP waveforms
	http://www.derangedphysiology.com/main/core-topics-intensive-care/haemodynamic-monitoring/Chapter%201.1.5/normal-arterial-line-waveforms
- CVP waveform
	http://www.dynapulse.com/educator/webcurriculum/chapter%203/abnormal%20ekg%20and%20waveform.htm
- PA waveforms
	http://mdnxs.com/topics-2/procedures/swan-ganz-catheter/
- IABP waveforms
	https://rk.md/2017/intra-aortic-balloon-pump-arterial-line-ekg-waveforms/