/* Settings.Simulator.cs
 * Infirmary Integrated
 * By Ibi Keller (Tanjera) (c) 2023
 *
 * Stores settings for persistence between sessions and for loading/saving simulations
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace II.Settings {
    public class Instance {
        /* Simulation-wide Settings:
         * Specific to the active/running simulation- (e.g. Time, State:Running/Stopped) and the physical device
         * and user running the simulation- may be loaded from the user's preferences file at runtime!
         * (e.g. whether the user has accepted the EULA, turned the audio off, and
         * where the windows have been placed in previous runs) for persistent UX settings.
         *
         * Do *NOT* place Scenario-specific settings here! (e.g. this Scenario should have a cardiac monitor with
         * 5 Numerics ... that should go into Settings.Device.cs)
         */
        
        public string Language;

        public States State;
        
        public bool AcceptedEULA;
        
        public bool AudioEnabled;
        public bool AutoApplyChanges;

        public int DefibEnergyMaximum;
        public int DefibEnergyIncrement;
        public ToneSources DefibAudioSource;

        public WindowStates UI = new WindowStates ();

        public bool MuteUpgrade;
        public DateTime MuteUpgradeDate;

        public enum States {
            Running,
            Paused,
            Closed
        }

        public enum ToneSources {
            Mute,
            Defibrillator,
            ECG,
            SPO2
        }
        
        public class WindowStates {
            public II.Settings.Window? Control;
            public II.Settings.Window? DeviceMonitor;
            public II.Settings.Window? DeviceDefib;
            public II.Settings.Window? DeviceECG;
            public II.Settings.Window? DeviceEFM;
            public II.Settings.Window? DeviceIABP;
            public II.Settings.Window? ScenarioEditor;
        }

        public Instance () {
            /* Note: These are the DEFAULT settings on 1st run (or Load() failure to parse)! */

            Language = "ENG";

            AcceptedEULA = false;

            AudioEnabled = true;
            AutoApplyChanges = false;

            DefibEnergyMaximum = 200;
            DefibEnergyIncrement = 20;
            DefibAudioSource = ToneSources.Defibrillator;

            MuteUpgrade = false;
            MuteUpgradeDate = new DateTime (2000, 1, 1);
            
            State = States.Running;
        }

        public static bool Exists () {
            return System.IO.File.Exists (File.GetConfigPath ());
        }

        public void Load () {
            if (!System.IO.File.Exists (File.GetConfigPath ()))
                return;

            StreamReader sr = new (File.GetConfigPath ());

            string? line;
            bool parseBool;
            int parseInt;

            while ((line = sr.ReadLine ()) != null) {
                if (line.Contains (":")) {
                    string pName = line.Substring (0, line.IndexOf (':')),
                            pValue = line.Substring (line.IndexOf (':') + 1).Trim ();
                    switch (pName) {
                        default: break;

                        case "Language": Language = pValue; break;

                        case "AcceptedEULA":
                            if (bool.TryParse (pValue, out parseBool))
                                AcceptedEULA = parseBool;
                            break;
                        
                        case "AudioEnabled":
                            if (bool.TryParse (pValue, out parseBool))
                                AudioEnabled = parseBool;
                            break;

                        case "AutoApplyChanges":
                            if (bool.TryParse (pValue, out parseBool))
                                AutoApplyChanges = parseBool;
                            break;

                        // Settings for Defibrillator
                        case "DefibEnergyMaximum":
                            if (int.TryParse (pValue, out parseInt))
                                DefibEnergyMaximum = parseInt;
                            break;

                        case "DefibEnergyIncrement":
                            if (int.TryParse (pValue, out parseInt))
                                DefibEnergyIncrement = parseInt;
                            break;

                        case "DefibAudioSource": DefibAudioSource = (ToneSources)Enum.Parse (typeof (ToneSources), pValue); break;

                        // Settings for muting whether new program upgrades are available for download
                        case "MuteUpgrade":
                            if (bool.TryParse (pValue, out parseBool))
                                MuteUpgrade = parseBool;
                            break;

                        case "MuteUpgradeDate":
                            MuteUpgradeDate = Utility.DateTime_FromString (pValue);
                            break;
                        
                        case "WindowStates_Control":
                            UI.Control = Window.Load (pValue);
                            break;
                        
                        case "WindowStates_DeviceDefib":
                            UI.DeviceDefib = Window.Load (pValue);
                            break;
                        
                        case "WindowStates_DeviceECG":
                            UI.DeviceECG = Window.Load (pValue);
                            break;
                        
                        case "WindowStates_DeviceEFM":
                            UI.DeviceEFM = Window.Load (pValue);
                            break;
                        
                        case "WindowStates_DeviceIABP":
                            UI.DeviceIABP = Window.Load (pValue);
                            break;
                        
                        case "WindowStates_DeviceMonitor":
                            UI.DeviceMonitor = Window.Load (pValue);
                            break;
                        
                        case "WindowStates_ScenarioEditor":
                            UI.ScenarioEditor = Window.Load (pValue);
                            break;
                    }
                }
            }

            sr.Close ();
            sr.Dispose ();
        }

        public void Save () {
            StreamWriter sw = new (File.GetConfigPath (), false);
            sw.WriteLine ($"Language:{Language}");
            sw.WriteLine ($"AcceptedEULA:{AcceptedEULA}");
            sw.WriteLine ($"AudioEnabled:{AudioEnabled}");
            sw.WriteLine ($"AutoApplyChanges:{AutoApplyChanges}");
            sw.WriteLine ($"DefibEnergyMaximum:{DefibEnergyMaximum}");
            sw.WriteLine ($"DefibEnergyIncrement:{DefibEnergyIncrement}");
            sw.WriteLine ($"DefibAudioSource:{DefibAudioSource}");
            sw.WriteLine ($"MuteUpgrade:{MuteUpgrade}");
            sw.WriteLine ($"MuteUpgradeDate:{Utility.DateTime_ToString (MuteUpgradeDate)}");
            
            if (UI.Control != null)
                sw.WriteLine ($"WindowStates_Control:{UI.Control.Save()}");
                
            if (UI.DeviceDefib != null)
                sw.WriteLine ($"WindowStates_DeviceDefib:{UI.DeviceDefib.Save()}");
            
            if (UI.DeviceECG != null)
                sw.WriteLine ($"WindowStates_DeviceECG:{UI.DeviceECG.Save()}");
            
            if (UI.DeviceEFM != null)
                sw.WriteLine ($"WindowStates_DeviceEFM:{UI.DeviceEFM.Save()}");
            
            if (UI.DeviceIABP != null)
                sw.WriteLine ($"WindowStates_DeviceIABP:{UI.DeviceIABP.Save()}");
            
            if (UI.DeviceMonitor != null)
                sw.WriteLine ($"WindowStates_DeviceMonitor:{UI.DeviceMonitor.Save()}");
            
            if (UI.ScenarioEditor != null)
                sw.WriteLine ($"WindowStates_ScenarioEditor:{UI.ScenarioEditor.Save()}");

            sw.Close ();
            sw.Dispose ();
        }
    }
}