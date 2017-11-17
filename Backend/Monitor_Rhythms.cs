/* Ibi Keller
 * Cardiac_Monitor.cs
 *
 * Simulates a cardiac monitor
 */

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Cardiac_Monitor : MonoBehaviour {
	
	class Rhythm_Strip {
	
		Game_Handler _Game;
		public float Line_Width = 0.001f;
		public LineRenderer Line_Renderer;
		
		float Timing_Resolution = 0.01f;
	
		float Draw_Length = 5.0f;
	
		// Three arrays for the strip, draw functions use _Current
		// and Scroll() keeps them flowing
		List<Vector2> Strip_Future,
						Strip_Current,
						Strip_History;
	
		public Rhythm_Strip () {
			_Game = GameObject.Find("__Game Controller").GetComponent<Game_Handler>();
			Line_Renderer = new LineRenderer();
			Strip_Future = new List<Vector2>();
			Strip_Current = new List<Vector2>();
			Strip_History = new List<Vector2>();
		}
		
		public void Draw_Marquee () {
			Scroll();
			Line_Renderer.SetVertexCount(Strip_Current.Count);
			
			for (int i = 0; i < Strip_Current.Count; i++)
				Line_Renderer.SetPosition(i, new Vector3(Strip_Current[i].x - _Game._Time, Strip_Current[i].y, 0));
		}
		
		public void Reset () {
			Strip_Future.Clear();
			Strip_Current.Clear();
			Strip_History.Clear();
		}
		
		public void Stop () {
			Strip_Future.Clear();	
		}
		
		Vector2 Last (List<Vector2> _In) {
			if (_In.Count < 1)
				return new Vector2(_Game._Time, 0);
			else
				return _In[_In.Count - 1];
		}
		
		public void Concatenate (List<Vector2> _Addition) {
			if (_Addition.Count == 0)
				return;
			else
				_Addition = Timed_Waveform(_Addition);
			
			float _Offset = 0f;
			if (Strip_Future.Count == 0)
				_Offset = _Game._Time;
			else if (Strip_Future.Count > 0)
				_Offset = Last(Strip_Future).x;
		
			foreach (Vector2 eachVector in _Addition)
				Strip_Future.Add(new Vector2(eachVector.x + _Offset, eachVector.y));
		}
		
		List<Vector2> Timed_Waveform (List<Vector2> _Buffer) {
			List<Vector2> _Out = new List<Vector2>();
			float _Length = Last(_Buffer).x;
		
			if (_Buffer.Count == 0)
				return new List<Vector2>();
			
			_Out.Add(_Buffer[0]);
			float i = _Buffer[0].x + Timing_Resolution;
			int n = 0;
			
			while (i < _Length) {
				if ((_Buffer[n].x <= i) && (_Buffer[n + 1].x >= i)) {
					_Out.Add (Vector2.Lerp(_Buffer[n], _Buffer[n + 1], 
						Mathf.InverseLerp(_Buffer[n].x, _Buffer[n + 1].x, i)));
					i += Timing_Resolution;
				} else if (i < _Buffer[n].x) {
					i += Timing_Resolution;
				} else if (i > _Buffer[n].x) {
					if (n < _Buffer.Count - 1) 
						n++;
					else
						break;
				}
			}
		
			return _Out;
		}
		
		void Scroll() {
			// Knock the future strip into the current strip and/or clean
			// up leftovers, but don't overshoot into the future
			if (Strip_Future.Count > 0) {
				for (int i = 0; i < Strip_Future.Count; i++) {
					if (Strip_Future[i].x > _Game._Time) {
						continue;
					} else if (Strip_Future[i].x < _Game._Time - Draw_Length) {
						Strip_History.Add(Strip_Future[i]);
						Strip_Future.RemoveAt(i);
						i--;	
					} else if (Strip_Future[i].x >= _Game._Time - Draw_Length
							&& Strip_Future[i].x <= _Game._Time) {
						Strip_Current.Add(Strip_Future[i]);
						Strip_Future.RemoveAt(i);
						i--;
					}
				}
			}
			
			if (Strip_Current.Count > 0) {
				for (int i = 0; i < Strip_Current.Count; i++) {
					if (Strip_Current[i].x < _Game._Time - Draw_Length) {
						Strip_History.Add(Strip_Current[i]);
						Strip_Current.RemoveAt(i);
						i--;
					}
				}
			}
		}
	}
	
	Game_Handler _Game;
	Action_Handler _Action;
	Audio_Handler _Audio_Handler;
	
	// Menu to be displayed when using the monitor
	Menu_Handler _Menu__Default;
	
	
	// Options for various uses
	public bool _Option__Menu__Default = true,
				_Option__Silenced = false,
				_Option__Label__Heart_Rhythm = true,
				_Option__Label__Alarm_Text = true;
	
	Transform _Zoom;
	AudioSource _Audio;
	GameObject _Player;
	
	Person _Patient;
	
	List<Rhythm_Strip> Strip_List;
	
	Rhythm_Strip EKG__L2,
				SpO2__Lead;
	
	public Material Material_EKG_Strip,
				Material_SpO2_Strip;
	
	public GameObject Screen_Overlay,
				Screen_Strip__EKG,
				Screen_Strip__SpO2;
				
	public GameObject[] Wires;
	
	List<TextMesh> Screen_Labels;
	public TextMesh[] Text_Green,
				Text_Yellow,
				Text_Blue,
				Text_Red;
	
	Color Color_EKG = Color.green,
				Color_BP = Color.blue,
				Color_SpO2 = Color.yellow,
				Color_Alarm = Color.red;
	
	float Drawing_Resolution = 0.01f;
	
	__Machine_States Machine_State = __Machine_States.INITIALIZE;
	
	public enum __Machine_States { 
		INITIALIZE,
		
		OFF,
		BOOTING,
		STANDBY,
		SHUTTING_DOWN,
		
		MARQUEE
	}
	
	
	void Awake () {
		_Game = GameObject.Find("__Game Controller").GetComponent<Game_Handler>();
		_Action = GameObject.Find("__Game Controller").GetComponent<Action_Handler>();
		_Audio_Handler = GameObject.Find("__Game Controller").GetComponent<Audio_Handler>();
		_Menu__Default = (Instantiate(GameObject.Find("__Cardiac Monitor GUI")) as GameObject).GetComponent<Menu_Handler>().Instantiate(this.gameObject);
		
		_Zoom = transform.FindChild("__Zoom");
		_Audio = gameObject.AddComponent<AudioSource>();
		_Player = GameObject.Find("__Player");
		
		Screen_Labels = new List<TextMesh>();
		foreach (Component eachComponent in (GetComponentsInChildren(typeof(Component)) as Component[]))
			if (eachComponent is TextMesh)
				Screen_Labels.Add(eachComponent as TextMesh);
	}
	void Start () {
		EKG__L2 = new Rhythm_Strip();
		SpO2__Lead = new Rhythm_Strip();
		
		Strip_List = new List<Rhythm_Strip>();
		Strip_List.AddRange(new []{ EKG__L2, SpO2__Lead });
		
	    EKG__L2.Line_Renderer = Screen_Strip__EKG.AddComponent<LineRenderer>();
	    EKG__L2.Line_Renderer.material = Material_EKG_Strip;
	    EKG__L2.Line_Renderer.SetColors(Color_EKG, Color_EKG);
	    EKG__L2.Line_Renderer.SetWidth(EKG__L2.Line_Width, EKG__L2.Line_Width);
	    EKG__L2.Line_Renderer.enabled = false;
		EKG__L2.Line_Renderer.useWorldSpace = false;
	     
	    SpO2__Lead.Line_Renderer = Screen_Strip__SpO2.AddComponent<LineRenderer>();
	    SpO2__Lead.Line_Renderer.material = Material_SpO2_Strip;
	    SpO2__Lead.Line_Renderer.SetColors(Color_SpO2, Color_SpO2);
	    SpO2__Lead.Line_Renderer.SetWidth(SpO2__Lead.Line_Width, SpO2__Lead.Line_Width);
	    SpO2__Lead.Line_Renderer.enabled = false;
		SpO2__Lead.Line_Renderer.useWorldSpace = false;
		
		foreach (TextMesh eachMesh in Text_Green)
			eachMesh.renderer.material.color = Color.green;
	    foreach (TextMesh eachMesh in Text_Yellow)
			eachMesh.renderer.material.color = Color.yellow;
		foreach (TextMesh eachMesh in Text_Blue)
			eachMesh.renderer.material.color = Color.blue;
		foreach (TextMesh eachMesh in Text_Red)
			eachMesh.renderer.material.color = Color.red;
		
		StartCoroutine(Machine__Process_State());
	}
	
	void __Input (Action_Handler.Input_Collision _Input) {
		
		if (_Input.Action__Interact && _Input.Key__Pressed 
				&& _Action.Reach__Attempt(_Input, false)) {
			_Action.Zoom__Toggle(_Zoom, .5f);
		}
		
		if (_Input.Action__Use && _Input.Key__Pressed) {
			
			// Can the player reach it?
			if (!_Action.Reach__Attempt(_Input, true))
				return;
			
			if (_Input.Type__Object) {
				if (_Option__Menu__Default)
					_Menu__Default.Show(true, this.transform);
				
			} else if (_Input.Type__GUI) {
				switch (_Input.Name) {
				default: 
					break;
				
				case "_Button__Attach":
					if (Patient__Attached)
						Patient__Detach();
					else if (!Patient__Attached)
						_Action.Capture(Action_Handler.Actions.USE, Action_Handler.Actions.ESCAPE, Patient__Attach, false);
					break;
					
				case "_Button__Cycle_BP":
					Update_Vitals__Blood_Pressure__Cyle();
					break;
					
				case "_Button__Power":
					Machine__Cycle_Power();
					break;
				}
			}
		}
	}
	
	public bool __Goal__Is_Running() {
		return Machine_State == __Machine_States.MARQUEE;
	}
	public bool __Goal__Is_Attached() {
		return _Patient != null;
	}
	public bool __Goal__Is_Attached(Person _Attached) {
		return _Patient != null
			&& _Patient == _Attached;
	}
	public bool __Goal__Idenfity_Rhythm(Person.__Cardiac_Rhythms _Rhythm) {
		return _Patient._Cardiac_Rhythm	== _Rhythm;
	}
	
	/* 
	 * Rendering, manipulation, and machine functions 
	 */
	
	IEnumerator Machine__Process_State () {
		while (true) {
			switch (Machine_State) {
			
			case __Machine_States.INITIALIZE:
				yield return new WaitForSeconds(0.1f);
				Patient_Wires(false);
				
				Machine_State = __Machine_States.OFF;
				break;
				
			case __Machine_States.OFF:
				yield return new WaitForSeconds(0.25f);
				break;
				
			case __Machine_States.BOOTING:
				
				Screen_Overlay.renderer.enabled = true;
			
				foreach (Rhythm_Strip eachStrip in Strip_List)
					eachStrip.Line_Renderer.enabled = true;
				
				foreach (TextMesh eachMesh in Screen_Labels) {
					eachMesh.renderer.enabled = true;
					
					if (eachMesh.name.StartsWith("Alarm-"))
						eachMesh.renderer.material.color = Color_Alarm;
					else if (eachMesh.name.StartsWith("Blood Pressure-"))	
						eachMesh.renderer.material.color = Color_BP;
					else if (eachMesh.name.StartsWith("Heart Rate-"))	
						eachMesh.renderer.material.color = Color_EKG;
					else if (eachMesh.name.StartsWith("Pulse Oximeter-"))	
						eachMesh.renderer.material.color = Color_SpO2;
				}
				
				StartCoroutine("Update_Time");
				
				Machine_State = __Machine_States.MARQUEE;
				break;
				
			case __Machine_States.SHUTTING_DOWN:
				Screen_Overlay.renderer.enabled = false;
			
				foreach (Rhythm_Strip eachStrip in Strip_List)
					eachStrip.Line_Renderer.enabled = false;
				
				foreach (TextMesh eachMesh in Screen_Labels)
					eachMesh.renderer.enabled = false;
				
				StopCoroutine("Update_Time");
				
				_Audio_Handler.Trigger__Clear(_Audio);
				
				Machine_State = __Machine_States.OFF;
				break;
				
			case __Machine_States.STANDBY: 
				yield return new WaitForSeconds(0.25f);
				break;
				
			case __Machine_States.MARQUEE: 
				EKG__L2.Draw_Marquee();
				SpO2__Lead.Draw_Marquee();
				yield return null;
				break;
			}
		}
	}
	public void Machine__Set_State(__Machine_States _Inc) {
		Machine_State = _Inc;	
	}
	void Machine__Cycle_Power () {
		if (Machine_State == __Machine_States.OFF) {
			Machine_State = __Machine_States.BOOTING;
			_Menu__Default.Text__Set("_Button__Power", "Turn Off", "black");
		} else {
			Machine_State = __Machine_States.SHUTTING_DOWN;
			_Menu__Default.Text__Set("_Button__Power", "Turn On", "black");
		}
	}
	
	bool Patient__Attached {
		get { return _Patient != null; }
	}
	public void Patient__Attach(Person _Incoming) {
		_Patient = _Incoming;
		_Patient.Cardiac_Monitor__Set(this);
		Patient_Wires(true);
				
		StartCoroutine("Update_Vitals__Timer");
		_Menu__Default.Text__Set("_Button__Attach", "Detach Patient", "black");
	}
	bool Patient__Attach (Action_Handler.Input_Collision _Collision) {
		if (_Collision._Hit.collider == null)
			return false;
		
		GameObject _Object = _Collision._Hit.collider.gameObject;
		Person _Incoming;
		
		for ( ; _Object.transform.parent != null; _Object = _Object.transform.parent.gameObject) {
			_Incoming = _Object.GetComponent<Person>();
			if (_Incoming != null && _Incoming._Role == Person.__Roles.Patient) {
				_Patient = _Incoming;
				_Patient.Cardiac_Monitor__Set(this);
				
				Patient_Wires(true);
				
				StartCoroutine("Update_Vitals__Timer");
				_Menu__Default.Text__Set("_Button__Attach", "Detach Patient", "black");
				return true;
			}
		}
		return false;
	}
	void Patient__Detach () {
		_Patient.Cardiac_Monitor__Set(null);
		_Patient = null;
		
		Update_Vitals__Process();
		StopCoroutine("Update_Vitals__Timer");
		
		Patient_Wires(false);
		
		EKG__L2.Stop();
		SpO2__Lead.Stop();
		
		_Menu__Default.Text__Set("_Button__Attach", "Attach to Patient", "black");
	}
	void Patient_Discharge () {
		EKG__L2.Reset();
		SpO2__Lead.Reset();
		
		Machine_State = __Machine_States.SHUTTING_DOWN;
	}
	void Patient_Wires (bool Status) {
		List<Component> _Beziers;
		
		foreach (GameObject eachWire in Wires) {
			_Beziers = new List<Component>(eachWire.GetComponentsInChildren(typeof(LineRenderer)) as Component[]);
			foreach (LineRenderer eachBezier in _Beziers)
				eachBezier.enabled = Status;
		}
	}
	
	IEnumerator Update_Vitals__Timer() {
		while (true) {
			if (_Game._Paused
					|| _Patient == null 
					|| Machine_State != __Machine_States.MARQUEE) {
				yield return new WaitForSeconds(1.0f);
				continue;
			}
			
			Update_Vitals__Process();
			
			if (_Patient._Heart_Rate > 0)
				yield return new WaitForSeconds(60 / _Patient._Heart_Rate);
			else
				yield return new WaitForSeconds(1.0f);
		}
	}
	public void Update_Vitals__Forced(Person _Inc) {
		if (_Patient == _Inc) {
			foreach (Rhythm_Strip eachStrip in Strip_List)
				eachStrip.Stop();
			Update_Vitals__Process();
		}
	}
	void Update_Vitals__Process() {
		
		if (_Patient == null) {
			Screen_Text("Heart Rate- Rate").text = "---";
			Screen_Text("Pulse Oximeter- Saturation").text = "---";
			Screen_Text("Pulse Oximeter- Rate").text = "";
			
			Screen_Text("Heart Rate- Rhythm").text = "LEAD FAILURE";
			return;
		}
		
		if (_Patient._Heart_Rate > 0) {
			Screen_Text("Heart Rate- Rate").text = _Patient._Heart_Rate.ToString();
			Screen_Text("Pulse Oximeter- Saturation").text = _Patient._SpO2.ToString();
			Screen_Text("Pulse Oximeter- Rate").text = _Patient._Heart_Rate.ToString();
		} else {
			Screen_Text("Heart Rate- Rate").text = "---";
			Screen_Text("Pulse Oximeter- Saturation").text = "---";
			Screen_Text("Pulse Oximeter- Rate").text = "";
		}
		
		switch (_Patient._Cardiac_Rhythm) {
			case Person.__Cardiac_Rhythms.Normal_Sinus:
			case Person.__Cardiac_Rhythms.Normal_Sinus_Bradycardia:
			case Person.__Cardiac_Rhythms.Normal_Sinus_Tachycardia:
				if (_Patient._Heart_Rate <= 100 && _Patient._Heart_Rate >= 60)
					Screen_Text("Heart Rate- Rhythm").text = !_Option__Label__Heart_Rhythm ? "" : "NSR";
				else if (_Patient._Heart_Rate < 60)
					Screen_Text("Heart Rate- Rhythm").text = !_Option__Label__Heart_Rhythm ? "" : "NS BRADY";
				else if (_Patient._Heart_Rate > 100)
					Screen_Text("Heart Rate- Rhythm").text = !_Option__Label__Heart_Rhythm ? "" : "NS TACH";
				EKG_Rhythm__Normal_Sinus(_Patient._Heart_Rate, 0.0f);
				SpO2_Rhythm__Normal_Sinus(_Patient._Heart_Rate);
				_Audio_Handler.Trigger__Clear(_Audio);
				break;
			
			case Person.__Cardiac_Rhythms.Atrial_Flutter:
				Screen_Text("Heart Rate- Rhythm").text = !_Option__Label__Heart_Rhythm ? "" : "A FLUTTER";
				EKG_Rhythm__Atrial_Flutter(_Patient._Heart_Rate, 0.0f);
				SpO2_Rhythm__Normal_Sinus(_Patient._Heart_Rate);	
				_Audio_Handler.Trigger__Clear(_Audio);
				break;
			
			case Person.__Cardiac_Rhythms.Supraventricular_Tachycardia:
				Screen_Text("Heart Rate- Rhythm").text = !_Option__Label__Heart_Rhythm ? "" : "SVT";
				Screen_Text("Alarm- Text").text = !_Option__Label__Alarm_Text ? "" : "RHYTHM: SVT";
				EKG_Rhythm__Supraventricular_Tachycardia(_Patient._Heart_Rate, 0.0f);
				SpO2_Rhythm__Normal_Sinus(_Patient._Heart_Rate);
				if (!_Option__Silenced)
					_Audio_Handler.Trigger__Loop(_Audio, Audio_Handler.Clip_Flags.BEEP);
				break;
			
			case Person.__Cardiac_Rhythms.Junctional:
				Screen_Text("Heart Rate- Rhythm").text = !_Option__Label__Heart_Rhythm ? "" : "JUNCT";
				EKG_Rhythm__Junctional(_Patient._Heart_Rate, 0.0f);
				SpO2_Rhythm__Normal_Sinus(_Patient._Heart_Rate);	
				_Audio_Handler.Trigger__Clear(_Audio);
				break;
			
			case Person.__Cardiac_Rhythms.Block__1st_Degree:
				Screen_Text("Heart Rate- Rhythm").text = !_Option__Label__Heart_Rhythm ? "" : "1ST DEG HB";
				EKG_Rhythm__AV_Block__1st_Degree(_Patient._Heart_Rate, 0.0f);
				SpO2_Rhythm__Normal_Sinus(_Patient._Heart_Rate);	
				_Audio_Handler.Trigger__Clear(_Audio);
				break;
			
			case Person.__Cardiac_Rhythms.Block__Wenckebach:
				Screen_Text("Heart Rate- Rhythm").text = !_Option__Label__Heart_Rhythm ? "" : "WENCKE";
				EKG_Rhythm__AV_Block__Wenckebach(_Patient._Heart_Rate, 0.0f, 4);
				SpO2_Rhythm__Normal_Sinus(_Patient._Heart_Rate);	
				_Audio_Handler.Trigger__Clear(_Audio);
				break;
			
			case Person.__Cardiac_Rhythms.Block__Mobitz_II:
				Screen_Text("Heart Rate- Rhythm").text = !_Option__Label__Heart_Rhythm ? "" : "MOBITZ 2";
				EKG_Rhythm__AV_Block__Mobitz_II(_Patient._Heart_Rate, 0.0f, .3f);
				SpO2_Rhythm__Normal_Sinus(_Patient._Heart_Rate);	
				_Audio_Handler.Trigger__Clear(_Audio);
				break;
			
			case Person.__Cardiac_Rhythms.Premature_Atrial_Contractions:
				Screen_Text("Heart Rate- Rhythm").text = !_Option__Label__Heart_Rhythm ? "" : "PAC";
				EKG_Rhythm__Premature_Atrial_Contractions(_Patient._Heart_Rate, 0.0f, .4f, 30f);
				SpO2_Rhythm__Normal_Sinus(_Patient._Heart_Rate);	
				_Audio_Handler.Trigger__Clear(_Audio);
				break;
			
			case Person.__Cardiac_Rhythms.Idioventricular:
				Screen_Text("Heart Rate- Rhythm").text = !_Option__Label__Heart_Rhythm ? "" : "IDIOVENT";
				EKG_Rhythm__Idioventricular(_Patient._Heart_Rate, 0.0f);
				SpO2_Rhythm__Normal_Sinus(_Patient._Heart_Rate);
				if (!_Option__Silenced)
					_Audio_Handler.Trigger__Loop(_Audio, Audio_Handler.Clip_Flags.BEEP);
				break;
			
			case Person.__Cardiac_Rhythms.Ventricular_Fibrillation:
				Screen_Text("Heart Rate- Rhythm").text = !_Option__Label__Heart_Rhythm ? "" : "V-FIB";
				EKG_Rhythm__Ventricular_Fibrillation(60 / _Patient._Heart_Rate, 0.0f);
				SpO2_Rhythm__Absent(60 / _Patient._Heart_Rate);
				if (!_Option__Silenced)
					_Audio_Handler.Trigger__Loop(_Audio, Audio_Handler.Clip_Flags.BEEP);
				break;
			
			case Person.__Cardiac_Rhythms.Ventricular_Standstill:
				Screen_Text("Heart Rate- Rhythm").text = !_Option__Label__Heart_Rhythm ? "" : "VENT SS";
				EKG_Rhythm__Ventricular_Standstill(_Patient._Heart_Rate, 0.0f);
				SpO2_Rhythm__Absent(60 / _Patient._Heart_Rate);	
			if (!_Option__Silenced)
					_Audio_Handler.Trigger__Loop(_Audio, Audio_Handler.Clip_Flags.BEEP);
				break;
			
			case Person.__Cardiac_Rhythms.Asystole:
				Screen_Text("Heart Rate- Rhythm").text = !_Option__Label__Heart_Rhythm ? "" : "ASYSTOLE";
				Screen_Text("Alarm- Text").text = !_Option__Label__Alarm_Text ? "" : "RHYTHM: ASYSTOLE";
				EKG_Rhythm__Asystole(60 / _Patient._Heart_Rate, 0.0f);
				SpO2_Rhythm__Absent(60 / _Patient._Heart_Rate);
				if (!_Option__Silenced)
					_Audio_Handler.Trigger__Loop(_Audio, Audio_Handler.Clip_Flags.BEEP);
				break;
		}
	}
	void Update_Vitals__Blood_Pressure__Cyle() {	
		if (_Patient == null)
			return;
		
		Screen_Text("Blood Pressure- Systolic").text = _Patient._Systolic.ToString();
		Screen_Text("Blood Pressure- Diastolic").text = _Patient._Diastolic.ToString();
		Screen_Text("Blood Pressure- MAP").text = BP__Mean_Arterial_Pressure(_Patient._Systolic, _Patient._Diastolic).ToString();
		Screen_Text("Blood Pressure- Time").text = DateTime.Now.ToString("HH:mm:ss");
	}
	IEnumerator Update_Time() {
		while (true) {
			while (_Game._State == Game_Handler.Game_States.PAUSED)
				yield return new WaitForSeconds(_Game._Delta_Time);	
			
			Screen_Text("Time- Time").text = DateTime.Now.ToString("HH:mm:ss");
			Screen_Text("Time- Date").text = DateTime.Now.ToString("dd MMM yyyy");
			yield return new WaitForSeconds(1.0f);
		}
	}
	
	public Transform Zoom__Point 
		{ get { return _Zoom;	} }
	TextMesh Screen_Text(string Name) {
		foreach (TextMesh eachMesh in Screen_Labels)
			if (eachMesh.name == Name)
				return eachMesh;
		
		return null;
	}
	
	/* 
	 * Biometric simulation functions
	 */
	
	public int BP__Mean_Arterial_Pressure (int _Systolic, int _Diastolic) {
		return _Diastolic + ((_Systolic - _Diastolic) / 3); }
	
	public void SpO2_Rhythm__Normal_Sinus (float _Rate) {
		/* SpO2 during normal sinus perfusion is similar to a sine wave leaning right
		 */
		float _Length = 60 / _Rate;
		
		List<Vector2> thisBeat = new List<Vector2>();
		thisBeat = Concatenate(thisBeat, Curve(_Length / 2, 0.5f, 0.0f, Last(thisBeat)));
		thisBeat = Concatenate(thisBeat, Line(_Length / 2, 0.0f, Last(thisBeat)));
		SpO2__Lead.Concatenate(thisBeat);
	}
	public void SpO2_Rhythm__Absent (float _Length) {
		/* SpO2 waveform non-existant
		 */
		
		List<Vector2> thisBeat = new List<Vector2>(); 
		thisBeat.Add(new Vector2(0, 0));
		thisBeat = Concatenate(thisBeat, Line(_Length, 0.0f, new Vector2(0, 0)));
		SpO2__Lead.Concatenate(thisBeat);
	}
	
	public void EKG_Rhythm__Normal_Sinus (float _Rate, float _Isoelectric) {
		/* Normal sinus rhythm (NSR) includes bradycardia (1-60), normocardia (60-100), 
		 * and sinus tachycardia (100 - 160)
		 */
		
	 	_Rate = Mathf.Clamp(_Rate, 1, 160);
	 	// Determine speed of some waves and segments based on rate using lerp
	 	float lerpCoeff = Mathf.Clamp01(Mathf.InverseLerp(160, 60, _Rate)),
				PR = Mathf.Lerp(0.16f, 0.2f, lerpCoeff),
				QRS = Mathf.Lerp(0.08f, 0.12f, lerpCoeff),
				QT = Mathf.Lerp(0.235f, 0.4f, lerpCoeff),
				TP = ((60 / _Rate) - (PR + QT));
	 	
	 	List<Vector2> thisBeat = new List<Vector2>();
		thisBeat = Concatenate(thisBeat, L2_P((PR * 2) / 3, _Isoelectric + .05f, _Isoelectric, new Vector2(0, _Isoelectric)));
		thisBeat = Concatenate(thisBeat, L2_PR_Segment(PR / 3, _Isoelectric, Last(thisBeat)));
		thisBeat = Concatenate(thisBeat, L2_Q(QRS / 4, _Isoelectric - 0.1f, Last(thisBeat)));
		thisBeat = Concatenate(thisBeat, L2_R(QRS / 4, _Isoelectric + 0.9f, Last(thisBeat)));
		thisBeat = Concatenate(thisBeat, L2_S(QRS / 4, _Isoelectric - 0.3f, Last(thisBeat)));
		thisBeat = Concatenate(thisBeat, L2_J(QRS / 4, _Isoelectric - 0.1f, Last(thisBeat)));
		thisBeat = Concatenate(thisBeat, L2_ST_Segment(((QT - QRS) * 2) / 5, _Isoelectric, Last(thisBeat)));
		thisBeat = Concatenate(thisBeat, L2_T(((QT - QRS) * 3) / 5, _Isoelectric + 0.1f, _Isoelectric, Last(thisBeat)));
		thisBeat = Concatenate(thisBeat, L2_TP_Segment(TP, _Isoelectric, Last(thisBeat)));
		EKG__L2.Concatenate(thisBeat);
	}
	
	public void EKG_Rhythm__Atrial_Flutter (float _Rate, float _Isoelectric)
		{ EKG_Rhythm__Atrial_Flutter(_Rate, 4, _Isoelectric); }
	public void EKG_Rhythm__Atrial_Flutter (float _Rate, int _Flutters, float _Isoelectric) {
		/* Atrial flutter is normal sinus rhythm with repeated P waves throughout
		 * TP interval. Clamped from 1-160.
		 */
		
	 	_Rate = Mathf.Clamp(_Rate, 1, 160);
	 	_Flutters = Mathf.Clamp(_Flutters, 2, 5);
	 	// Determine speed of some waves and segments based on rate using lerp
	 	float lerpCoeff = Mathf.Clamp01(Mathf.InverseLerp(160, 60, _Rate)),
				QRS = Mathf.Lerp(0.08f, 0.12f, lerpCoeff),
				QT = Mathf.Lerp(0.10f, 0.16f, lerpCoeff),
				TP = ((60 / _Rate) - QT);
		
		List<Vector2> thisBeat = new List<Vector2>(); 
		thisBeat = Concatenate(thisBeat, L2_P(TP / _Flutters, _Isoelectric + .1f, _Isoelectric, new Vector2(0, _Isoelectric)));
		for (int i = 1; i < _Flutters; i++)
			thisBeat = Concatenate(thisBeat, L2_P(TP / _Flutters, _Isoelectric + .1f, _Isoelectric, Last(thisBeat)));
		
		thisBeat = Concatenate(thisBeat, L2_Q(QRS / 4, _Isoelectric - 0.1f, Last(thisBeat)));
		thisBeat = Concatenate(thisBeat, L2_R(QRS / 4, _Isoelectric + 0.9f, Last(thisBeat)));
		thisBeat = Concatenate(thisBeat, L2_S(QRS / 4, _Isoelectric - 0.3f, Last(thisBeat)));
		thisBeat = Concatenate(thisBeat, L2_J(QRS / 4, _Isoelectric - 0.1f, Last(thisBeat)));
		thisBeat = Concatenate(thisBeat, L2_ST_Segment(QT - QRS, _Isoelectric, Last(thisBeat)));
		EKG__L2.Concatenate(thisBeat);
	}
	
	public void EKG_Rhythm__Supraventricular_Tachycardia (float _Rate, float _Isoelectric) {
		/* Supraventricular tachycardia (SVT) includes heart rates between 160
		 * to 240 beats per minute. Essentially it is NSR without a PR interval
		 * or a P wave (P is mixed with T).
		 */
		 
	 	_Rate = Mathf.Clamp(_Rate, 160, 240);
	 	// Determine speed of some waves and segments based on rate using lerp
	 	float lerpCoeff = Mathf.Clamp01(Mathf.InverseLerp(240, 160, _Rate)),
					PR = Mathf.Lerp(0.03f, 0.05f, lerpCoeff),
					QRS = Mathf.Lerp(0.05f, 0.08f, lerpCoeff),
					QT = Mathf.Lerp(0.17f, 0.235f, lerpCoeff);
	 	
	 	List<Vector2> thisBeat = new List<Vector2>();
		thisBeat.Add(new Vector2(0, _Isoelectric)); 
		thisBeat = Concatenate(thisBeat, L2_PR_Segment(PR, _Isoelectric, Last(thisBeat)));
		thisBeat = Concatenate(thisBeat, L2_Q(QRS / 4, _Isoelectric - 0.1f, Last(thisBeat)));
		thisBeat = Concatenate(thisBeat, L2_R(QRS / 4, _Isoelectric + 0.9f, Last(thisBeat)));
		thisBeat = Concatenate(thisBeat, L2_S(QRS / 4, _Isoelectric - 0.3f, Last(thisBeat)));
		thisBeat = Concatenate(thisBeat, L2_J(QRS / 4, _Isoelectric - 0.1f, Last(thisBeat)));
		thisBeat = Concatenate(thisBeat, L2_ST_Segment(((QT - QRS) * 2) / 5, _Isoelectric - 0.06f, Last(thisBeat)));
		thisBeat = Concatenate(thisBeat, L2_T(((QT - QRS) * 3) / 5, _Isoelectric + 0.15f, _Isoelectric, Last(thisBeat)));
		EKG__L2.Concatenate(thisBeat);
	}
	
	public void EKG_Rhythm__Junctional (float _Rate, float _Isoelectric)
		{ EKG_Rhythm__Junctional (_Rate, _Isoelectric, false); }
	public void EKG_Rhythm__Junctional (float _Rate, float _Isoelectric, bool _P_Inverted) {
		/* Junctional rhythm is normal sinus with either an absent or inverted P wave,
		 * regularly between 40-60 bpm, with accelerated junctional tachycardia from
		 * 60-115 bpm. Function clamped at 1-130.
		 */
		 
	 	_Rate = Mathf.Clamp(_Rate, 1, 130);
	 	// Determine speed of some waves and segments based on rate using lerp
	 	float lerpCoeff = Mathf.Clamp01(Mathf.InverseLerp(130, 60, _Rate)),
					PR = Mathf.Lerp(0.16f, 0.2f, lerpCoeff),
					QRS = Mathf.Lerp(0.08f, 0.12f, lerpCoeff),
					QT = Mathf.Lerp(0.235f, 0.4f, lerpCoeff),
					TP = ((60 / _Rate) - (PR + QT));
	 	
	 	List<Vector2> thisBeat = new List<Vector2>();
		thisBeat = Concatenate(thisBeat, L2_P((PR * 2) / 3, 
			(_P_Inverted ? _Isoelectric - .05f : _Isoelectric),
			_Isoelectric, new Vector2(0, _Isoelectric)));
		thisBeat = Concatenate(thisBeat, L2_PR_Segment(PR / 3, _Isoelectric, Last(thisBeat)));
		thisBeat = Concatenate(thisBeat, L2_Q(QRS / 4, _Isoelectric - 0.1f, Last(thisBeat)));
		thisBeat = Concatenate(thisBeat, L2_R(QRS / 4, _Isoelectric + 0.9f, Last(thisBeat)));
		thisBeat = Concatenate(thisBeat, L2_S(QRS / 4, _Isoelectric - 0.3f, Last(thisBeat)));
		thisBeat = Concatenate(thisBeat, L2_J(QRS / 4, _Isoelectric - 0.1f, Last(thisBeat)));
		thisBeat = Concatenate(thisBeat, L2_ST_Segment(((QT - QRS) * 2) / 5, _Isoelectric - 0.06f, Last(thisBeat)));
		thisBeat = Concatenate(thisBeat, L2_T(((QT - QRS) * 3) / 5, _Isoelectric + 0.1f, _Isoelectric, Last(thisBeat)));
		thisBeat = Concatenate(thisBeat, L2_TP_Segment(TP, _Isoelectric, Last(thisBeat)));
		EKG__L2.Concatenate(thisBeat);
	}
	
	public void EKG_Rhythm__AV_Block__1st_Degree (float _Rate, float _Isoelectric) {
		/* 1st degree AV block consists of normal sinus rhythm with a PR interval > .20 seconds
		 */
		
	 	_Rate = Mathf.Clamp(_Rate, 1, 160);
	 	// Determine speed of some waves and segments based on rate using lerp
	 	float lerpCoeff = Mathf.Clamp01(Mathf.InverseLerp(160, 60, _Rate)),
				PR = Mathf.Lerp(0.26f, 0.36f, lerpCoeff),
				QRS = Mathf.Lerp(0.08f, 0.12f, lerpCoeff),
				QT = Mathf.Lerp(0.235f, 0.4f, lerpCoeff),
				TP = ((60 / _Rate) - (PR + QT));
	 	
	 	List<Vector2> thisBeat = new List<Vector2>();
		thisBeat = Concatenate(thisBeat, L2_P(PR / 3, _Isoelectric + .05f, _Isoelectric, new Vector2(0, _Isoelectric)));
		thisBeat = Concatenate(thisBeat, L2_PR_Segment((PR * 2) / 3, _Isoelectric, Last(thisBeat)));
		thisBeat = Concatenate(thisBeat, L2_Q(QRS / 4, _Isoelectric - 0.1f, Last(thisBeat)));
		thisBeat = Concatenate(thisBeat, L2_R(QRS / 4, _Isoelectric + 0.9f, Last(thisBeat)));
		thisBeat = Concatenate(thisBeat, L2_S(QRS / 4, _Isoelectric - 0.3f, Last(thisBeat)));
		thisBeat = Concatenate(thisBeat, L2_J(QRS / 4, _Isoelectric - 0.1f, Last(thisBeat)));
		thisBeat = Concatenate(thisBeat, L2_ST_Segment(((QT - QRS) * 2) / 5, _Isoelectric, Last(thisBeat)));
		thisBeat = Concatenate(thisBeat, L2_T(((QT - QRS) * 3) / 5, _Isoelectric + 0.1f, _Isoelectric, Last(thisBeat)));
		thisBeat = Concatenate(thisBeat, L2_TP_Segment(TP, _Isoelectric, Last(thisBeat)));
		EKG__L2.Concatenate(thisBeat);
	}
	public void EKG_Rhythm__AV_Block__Wenckebach (float _Rate, float _Isoelectric, int _Drops_On = 4) {
		/* AV Block 2nd degree Wenckebach is a normal sinus rhythm marked by lengthening
		 * PQ intervals for 2-4 beats then a dropped QRS complex.
		 * 
		 * Renders amount of beats _Drops_On, PQ interval lengthens and QRS drops on _Drops_On
		 */
		
		_Rate = Mathf.Clamp (_Rate, 40, 120);
		_Drops_On = Mathf.Clamp(_Drops_On, 2, 4);
		
		for (int currBeat = 1; currBeat <= _Drops_On; currBeat++) {
			// Determine speed of some waves and segments based on rate using lerp
			float lerpCoeff = Mathf.Clamp01(Mathf.InverseLerp(120, 40, _Rate)),
					PR = Mathf.Lerp(0.16f, 0.2f, lerpCoeff),
					// PR segment varies due to Wenckebach
					PR_Segment = (PR / 3) * currBeat,
					QRS = Mathf.Lerp(0.08f, 0.12f, lerpCoeff),
					QT = Mathf.Lerp(0.235f, 0.4f, lerpCoeff),
					TP = ((60 / _Rate) - (PR + QT + PR_Segment));
			
		 	List<Vector2> thisBeat = new List<Vector2>();
			thisBeat = Concatenate(thisBeat, L2_P((PR * 2) / 3, _Isoelectric + .05f, _Isoelectric, new Vector2(0, _Isoelectric)));
			thisBeat = Concatenate(thisBeat, L2_PR_Segment(PR_Segment, _Isoelectric, Last(thisBeat)));
			
			if (currBeat != _Drops_On) {
				// Render QRS complex on the beats it's not dropped...
				thisBeat = Concatenate(thisBeat, L2_Q(QRS / 4, _Isoelectric - 0.1f, Last(thisBeat)));
				thisBeat = Concatenate(thisBeat, L2_R(QRS / 4, _Isoelectric + 0.9f, Last(thisBeat)));
				thisBeat = Concatenate(thisBeat, L2_S(QRS / 4, _Isoelectric - 0.3f, Last(thisBeat)));
				thisBeat = Concatenate(thisBeat, L2_J(QRS / 4, _Isoelectric - 0.1f, Last(thisBeat)));
				thisBeat = Concatenate(thisBeat, L2_ST_Segment(((QT - QRS) * 2) / 5, _Isoelectric, Last(thisBeat)));
				thisBeat = Concatenate(thisBeat, L2_T(((QT - QRS) * 3) / 5, _Isoelectric + 0.1f, _Isoelectric, Last(thisBeat)));
			} else {
				// If QRS is dropped, add its time to the TP segment to keep the rhythm at a normal rate
				TP += QRS;
				TP += QT - QRS;
			}
				
			thisBeat = Concatenate(thisBeat, L2_TP_Segment(TP, _Isoelectric, Last(thisBeat)));
			EKG__L2.Concatenate(thisBeat);
		}
	}
	public void EKG_Rhythm__AV_Block__Mobitz_II (float _Rate, float _Isoelectric, float _Occurrance) {
		/* AV Block 2nd degree Mobitz Type II is a normal sinus rhythm with occasional
		 * dropped QRS complexes.
		 */
		
	 	_Rate = Mathf.Clamp(_Rate, 1, 160);
	 	// Determine speed of some waves and segments based on rate using lerp
	 	float lerpCoeff = Mathf.Clamp01(Mathf.InverseLerp(160, 60, _Rate)),
					PR = Mathf.Lerp(0.16f, 0.2f, lerpCoeff),
					QRS = Mathf.Lerp(0.08f, 0.12f, lerpCoeff),
					QT = Mathf.Lerp(0.235f, 0.4f, lerpCoeff),
					TP = ((60 / _Rate) - (PR + QT));
	 	
	 	List<Vector2> thisBeat = new List<Vector2>(); 
		thisBeat = Concatenate(thisBeat, L2_P((PR * 2) / 3, _Isoelectric + .05f, _Isoelectric, new Vector2(0, _Isoelectric)));
	
		if (UnityEngine.Random.Range(0.0f, 1.0f) <= _Occurrance)
			thisBeat = Concatenate(thisBeat, Line((PR / 3) + QT + TP, _Isoelectric, Last(thisBeat)));
		else {
			thisBeat = Concatenate(thisBeat, L2_PR_Segment(PR / 3, _Isoelectric, Last(thisBeat)));
			thisBeat = Concatenate(thisBeat, L2_Q(QRS / 4, _Isoelectric - 0.1f, Last(thisBeat)));
			thisBeat = Concatenate(thisBeat, L2_R(QRS / 4, _Isoelectric + 0.9f, Last(thisBeat)));
			thisBeat = Concatenate(thisBeat, L2_S(QRS / 4, _Isoelectric - 0.3f, Last(thisBeat)));
			thisBeat = Concatenate(thisBeat, L2_J(QRS / 4, _Isoelectric - 0.1f, Last(thisBeat)));
			thisBeat = Concatenate(thisBeat, L2_ST_Segment(((QT - QRS) * 2) / 5, _Isoelectric, Last(thisBeat)));
			thisBeat = Concatenate(thisBeat, L2_T(((QT - QRS) * 3) / 5, _Isoelectric + 0.1f, _Isoelectric, Last(thisBeat)));
			thisBeat = Concatenate(thisBeat, L2_TP_Segment(TP, _Isoelectric, Last(thisBeat)));
		}
		
		EKG__L2.Concatenate(thisBeat);
	}
	
	public void EKG_Rhythm__Premature_Atrial_Contractions (float _Rate, float _Isoelectric, float _Occurrance, float _Variance)
		/* Quick overload for single beat, does not prevent 2 consecutive PACs or lengthen following beat... */
		{ EKG_Rhythm__Premature_Atrial_Contractions(_Rate, 1, _Isoelectric, _Occurrance, _Variance); }
	public void EKG_Rhythm__Premature_Atrial_Contractions (float _Rate, int _Beats, float _Isoelectric, float _Occurrance, float _Variance) {
		/* Premature atrial contractions (PAC) are normal sinus rhythm with occasionally shortening 
		 * TP segments, so will just run normal sinus with a random range of heart rate.
		 * Occurrance is percentage chance that a PAC will occur rather than an NSR.
		 */
		
		bool wasPAC = false;
		
		List<Vector2> theseBeats = new List<Vector2>();
		theseBeats.Add(new Vector2(0, _Isoelectric)); 
		for (int i = 0; i < _Beats; i++) {
			// Prevent 2 PAC's from happening consecutively by checking wasPAC
			if ((UnityEngine.Random.Range(0.0f, 1.0f) <= _Occurrance) && !wasPAC) {
				wasPAC = true;
				EKG_Rhythm__Normal_Sinus(_Rate + _Variance, _Isoelectric);
			} else {
				// If there was a PAC last beat...
				if (wasPAC) {
					wasPAC = false;
					EKG_Rhythm__Normal_Sinus(_Rate - (_Variance  / 4), _Isoelectric);
				}
				// If there was no PAC last beat and no occurrance this beat
				else if (!wasPAC)
					EKG_Rhythm__Normal_Sinus(_Rate, _Isoelectric);
			}
		} 
	}
	
	public void EKG_Rhythm__Idioventricular(float _Rate, float _Isoelectric) {
		/* Idioventricular rhythms originate in the ventricules (fascicles, bundle branches,
		 * or Bundle of His) and can have different and erratic shapes varying by origin.
		 * Marked by absent P waves, wide and distorted QRS complexes. Regular idioventricular
		 * rhythms run from 15-45 bpm, accelerated idioventricular rhythms are 45-100 bpm.
		 */
		
	 	_Rate = Mathf.Clamp(_Rate, 1, 100);
	 	// Determine speed of some waves and segments based on rate using lerp
	 	float lerpCoeff = Mathf.Clamp01(Mathf.InverseLerp(75, 25, _Rate)),
				QRS = Mathf.Lerp(0.3f, 0.4f, lerpCoeff),
				SQ = ((60 / _Rate) - QRS);
	 	
	 	List<Vector2> thisBeat = new List<Vector2>();
		thisBeat = Concatenate(thisBeat, Curve(QRS / 2, _Isoelectric + 1.0f, _Isoelectric - 0.3f, Last(thisBeat)));
		thisBeat = Concatenate(thisBeat, Curve(QRS / 2, _Isoelectric - 0.3f, _Isoelectric - 0.4f, Last(thisBeat)));
		thisBeat = Concatenate(thisBeat, Curve(SQ / 3, _Isoelectric + 0.1f, _Isoelectric, Last(thisBeat)));
		thisBeat = Concatenate(thisBeat, Line((SQ * 2) / 3, _Isoelectric, Last(thisBeat)));
		EKG__L2.Concatenate(thisBeat);
	}
	public void EKG_Rhythm__Ventricular_Fibrillation(float _Length, float _Isoelectric) {
		/* Ventricular fibrillation is random peaks/curves with no recognizable waves, no regularity.
		 */
		
	 	float _Wave = 0f,
				_Amplitude = UnityEngine.Random.Range(-0.6f, 0.6f);
		List<Vector2> thisBeat = new List<Vector2>();
		
		while (_Length > 0f) {
			_Wave = UnityEngine.Random.Range(0.1f, 0.2f);
			
			thisBeat = Concatenate(thisBeat, Curve(_Wave, _Isoelectric + _Amplitude, _Isoelectric, Last(thisBeat)));	
			// Flip the sign of amplitude and randomly crawl larger/smaller, models the
			// flippant waves in v-fib.
			_Amplitude = 0 - Mathf.Clamp(UnityEngine.Random.Range(_Amplitude - 0.1f, _Amplitude + 0.1f), -1f, 1f);
			_Length -= _Wave;
		}
		
		EKG__L2.Concatenate(thisBeat);
	}
	public void EKG_Rhythm__Ventricular_Standstill (float _Rate, float _Isoelectric) {
		/* Ventricular standstill is the absence of ventricular activity- only P waves exist
		 */
		
	 	_Rate = Mathf.Clamp(_Rate, 1, 160);
	 	// Determine speed of some waves and segments based on rate using lerp
	 	float lerpCoeff = Mathf.Clamp01(Mathf.InverseLerp(160, 60, _Rate)),
					P = Mathf.Lerp(0.10f, 0.14f, lerpCoeff),
					TP = ((60 / _Rate) - P);
	 	
	 	List<Vector2> thisBeat = new List<Vector2>(); 
		thisBeat = Concatenate(thisBeat, L2_P(P, _Isoelectric + .05f, _Isoelectric, new Vector2(0, _Isoelectric)));
		thisBeat = Concatenate(thisBeat, L2_TP_Segment(TP, _Isoelectric, Last(thisBeat)));
		EKG__L2.Concatenate(thisBeat);
	}
	
	public void EKG_Rhythm__Asystole (float _Length, float _Isoelectric) {
		/* Asystole is the absence of electrical activity.
		 */
	
		List<Vector2> thisBeat = new List<Vector2>(); 
		thisBeat.Add(new Vector2(0, _Isoelectric));
		thisBeat = Concatenate(thisBeat, Line(_Length, _Isoelectric, new Vector2(0, _Isoelectric)));
		EKG__L2.Concatenate(thisBeat);
	}
	
	
	
	/* 
	 * Shaping and point plotting functions 
	 */
	
	Vector2 Last (List<Vector2> _Original) {
		if (_Original.Count < 1)
			return new Vector2(0, 0);
		else
			return _Original[_Original.Count - 1];
	}
	List<Vector2> Concatenate (List<Vector2> _Original, List<Vector2> _Addition) {
		// Offsets the X value of a Vector2[] so that it can be placed at the end
		// of an existing Vector2[] and continue from that point on. 
	
		// Nothing to add? Return something.
		if (_Original.Count == 0 && _Addition.Count == 0)
			return new List<Vector2>();
		else if (_Addition.Count == 0)
			return _Original;
	
		float _Offset = 0f;
		if (_Original.Count == 0)
			_Offset = 0;
		else if (_Original.Count > 0)
			_Offset = _Original[_Original.Count - 1].x;
	
		foreach (Vector2 eachVector in _Addition)
			_Original.Add(new Vector2(eachVector.x + _Offset, eachVector.y));
	
		return _Original;
	}
	float Slope (Vector2 _P1, Vector2 _P2){
		return ((_P2.y - _P1.y) / (_P2.x - _P1.x));
	}
	Vector2 Bezier(Vector2 _Start, Vector2 _Control, Vector2 _End, float _Percent) {
	    return (((1 - _Percent) * (1 - _Percent)) * _Start) + (2 * _Percent * (1 - _Percent) * _Control) + ((_Percent * _Percent) * _End);
	}
	List<Vector2> Curve (float _Length, float _mV, float _mV_End, Vector2 _Start) {
		int i;
		float x;
		List<Vector2> _Out = new List<Vector2>();
	  
	    for (i = 1; i * ((2 *Drawing_Resolution) / _Length) <= 1; i++) {
	    	x = i * ((2 *Drawing_Resolution) / _Length);
			_Out.Add(Bezier(new Vector2(0, _Start.y), new Vector2(_Length / 4, _mV), new Vector2(_Length / 2, _mV), x));
		}
		
		for (i = 1; i * ((2 *Drawing_Resolution) / _Length) <= 1; i++) {
			x = i * ((2 *Drawing_Resolution) / _Length);
			_Out.Add(Bezier(new Vector2(_Length / 2, _mV), new Vector2(_Length / 4 * 3, _mV), new Vector2(_Length, _mV_End), x));
		}
		
		_Out.Add(new Vector2(_Length, _mV_End));		// Finish the curve
		
		return _Out;
	}
	List<Vector2> Peak (float _Length, float _mV, float _mV_End, Vector2 _Start) {
		int i;
		float x;
		List<Vector2> _Out = new List<Vector2>();
	  
	    for (i = 1; i * ((2 *Drawing_Resolution) / _Length) <= 1; i++) {
	    	x = i * ((2 *Drawing_Resolution) / _Length);
			_Out.Add(Bezier(new Vector2(0, _Start.y), new Vector2(_Length / 3, _mV / 1), new Vector2(_Length / 2, _mV), x));
		}
		
		for (i = 1; i * ((2 *Drawing_Resolution) / _Length) <= 1; i++) {
			x = i * ((2 *Drawing_Resolution) / _Length);
			_Out.Add(Bezier(new Vector2(_Length / 2, _mV), new Vector2(_Length / 5 * 3, _mV / 1), new Vector2(_Length, _mV_End), x));
		}
		
		_Out.Add(new Vector2(_Length, _mV_End));		// Finish the curve
		
		return _Out;
	}
	List<Vector2> Line (float _Length, float _mV, Vector2 _Start) {
		List<Vector2> _Out = new List<Vector2>();
		_Out.Add(new Vector2(_Length, _mV));
		
		return _Out;
	}
	
	
	/* 
	 * Individual cardiac wave functions-
	 * Note: The most simplified overload is for normal sinus rhythm @ 60 bpm,
	 * 		but modifications can be made with the more complex overloads to simulate
	 *		dysrhythmias.
	 */
	
	List<Vector2> L2_P(Vector2 _Start) 
		{ return L2_P(.08f, .1f, 0f, _Start); }
	List<Vector2> L2_P(float _Length, float _mV, float _mV_End, Vector2 _Start)
		{ return Peak(_Length, _mV, _mV_End, _Start); }
	List<Vector2> L2_Q(Vector2 _Start) 
		{ return L2_Q(1f, -.1f, _Start); }
	List<Vector2> L2_Q(float _Length, float _mV, Vector2 _Start)
		{ return Line(_Length, _mV, _Start); }
	List<Vector2> L2_R(Vector2 _Start) 
		{ return L2_R(1f, .9f, _Start); }
	List<Vector2> L2_R(float _Length, float _mV, Vector2 _Start)
		{ return Line(_Length, _mV, _Start); }
	List<Vector2> L2_S(Vector2 _Start) 
		{ return L2_S(1f, -.3f, _Start); }
	List<Vector2> L2_S(float _Length, float _mV, Vector2 _Start)
		{ return Line(_Length, _mV, _Start); }
	List<Vector2> L2_J(Vector2 _Start) 
		{ return L2_J(1f, -.1f, _Start); }
	List<Vector2> L2_J(float _Length, float _mV, Vector2 _Start)
		{ return Line(_Length, _mV, _Start); }
	List<Vector2> L2_T(Vector2 _Start) 
		{ return L2_T(.16f, .25f, 0f, _Start); }
	List<Vector2> L2_T(float _Length, float _mV, float _mV_End, Vector2 _Start)
		{ return Peak(_Length, _mV, _mV_End, _Start); }
	List<Vector2> L2_PR_Segment(Vector2 _Start) 
		{ return L2_PR_Segment(.08f, 0f, _Start); }
	List<Vector2> L2_PR_Segment(float _Length, float _mV, Vector2 _Start)
		{ return Line(_Length, _mV, _Start); }
	List<Vector2> L2_ST_Segment(Vector2 _Start) 
		{ return L2_ST_Segment(.1f, 0f, _Start); }
	List<Vector2> L2_ST_Segment(float _Length, float _mV, Vector2 _Start)
		{ return Line(_Length, _mV, _Start); }
	List<Vector2> L2_TP_Segment(Vector2 _Start) 
		{ return L2_TP_Segment(.48f, .0f, _Start); }
	List<Vector2> L2_TP_Segment(float _Length, float _mV, Vector2 _Start)
		{ return Line(_Length, _mV, _Start); }

}