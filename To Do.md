
* Next Steps:
	- Layout formatting
		- Container/Table, variable rows (default 3)
		- Device Options menuItem -> Row Amounts
		- Device Options menuItem -> Attach/Detach Devices (etc. ABP, PA, etc.)
			- Attaching devices adds to List<> of active devices
	- Context menu (right click) on tracings/rows to "Select Waveform"
		- Selectable tracings: e.g. ECG leads, ABP waveform, PA waveform, etc.
		- Can select from List<> of active devices


II-CM Roadmap:
	- More tracings: ECG 12 lead, SpO2, ABP, CVP, PA	
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
- ABP waveforms
	http://www.derangedphysiology.com/main/core-topics-intensive-care/haemodynamic-monitoring/Chapter%201.1.5/normal-arterial-line-waveforms
- CVP waveform
	http://www.dynapulse.com/educator/webcurriculum/chapter%203/abnormal%20ekg%20and%20waveform.htm
- PA waveforms
	http://mdnxs.com/topics-2/procedures/swan-ganz-catheter/
- IABP waveforms
	https://rk.md/2017/intra-aortic-balloon-pump-arterial-line-ekg-waveforms/