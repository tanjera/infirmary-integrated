
* Next Steps:
	- Add containers to form window:
		- Strips
		- Rhythm_Renderer (handler class)
	- Create timer (by second?)
		- Process rhythm generation -> strip concatenation by timer
	- Create/port old LineRenderer functions to Rhythm_Renderer
		- And implement!!

* Strip
	- Switch from 3 sections (future, current, past) to 2 sections (future, past)

* Numerics
	- Heart rate, BP, SpO2 values (read from vitals container)



* References:
	Original I3D cardiac monitor line renderer, machine states, rhythm generation -> List<Vector2>
	https://github.com/tanjera/infirmary-3d/blob/cade40fa678d328fcfbe0f688d422929ee907c59/Unity/Assets/Scripts/Objects/Cardiac_Monitor.cs

	Original I3D cardiac rhythm function caller
	https://github.com/tanjera/infirmary-3d/blob/cade40fa678d328fcfbe0f688d422929ee907c59/Unity/Assets/Scripts/Classes/Cardiac_Rhythms.cs

	Drawing graphics in C#
	http://www.techotopia.com/index.php/Drawing_Graphics_in_C_Sharp