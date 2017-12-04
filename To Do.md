
* Next Steps:
	- Dynamic adding of mainLayout rows... grr...

	- Implement Patient_Vitals form variables into rhythm changes
		- ST elevation, T wave elevation
		- Axis deviation

	- More tracings: CVP, ETCO2, PA, RR

	- Expand Patient class and Edit Patient form	
		- Add default Rhythm vital signs for all hemodynamic parameters (Get_Rhythm().Vitals())

	- Update vital signs with +/- 2.5% variation (HR, NIBP (interval?), ABP, CVP, PA)

	- Different Controls for vital sign values (HR, RR/ETCO2, ABP, CVP, PA)
		- Buttons specific to each type of vital sign on the vital sign's Control
			- HR: Run 12 lead
				- -> new Form for 12 lead
			- BP: Cycle BP
			- PA: Thermodilution
				- -> new Form for thermodilution hemodynamic values


II-CM Roadmap:	
	- Include paced rhythms (VVI, DDI)
	- Thermodilution profile

	- Polish tracing context menu for selecting waveforms
	- Match ECG rhythm to SpO2/ABP tracing timing
	- Additional PA waveforms (RA, RV, PA, PAW)
		- And option to select waveform ("placement" of PA cath?)
	- IABP device, waveforms

	- Save/Load functionality (file extension .ii)
		- Read-only option, password protected 
			- Patient vitals, device tracings all un-editable
			- e.g. for professors testing students
		- SSH encrypt save files (for testing mode)
		- Import invidual fields and assign to objects (not serialized out/in)
			- For backwards compatibility with save files...
	 


* Debug:



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