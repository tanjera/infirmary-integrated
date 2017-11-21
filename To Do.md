
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
Original I3D cardiac monitor line renderer, machine states, rhythm generation -> List<Vector2>
	https://github.com/tanjera/infirmary-3d/blob/cade40fa678d328fcfbe0f688d422929ee907c59/Unity/Assets/Scripts/Objects/Cardiac_Monitor.cs
Original I3D cardiac rhythm function caller
	https://github.com/tanjera/infirmary-3d/blob/cade40fa678d328fcfbe0f688d422929ee907c59/Unity/Assets/Scripts/Classes/Cardiac_Rhythms.cs
Drawing graphics in C#
	http://www.techotopia.com/index.php/Drawing_Graphics_in_C_Sharp