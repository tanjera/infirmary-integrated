using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace II.Localization {

    public class Languages {
        public Values Value;
        public Languages (Values v) { Value = v; }
        public Languages () {
            if (Enum.TryParse<Values> (CultureInfo.InstalledUICulture.ThreeLetterWindowsLanguageName.ToUpper (), out Values tryParse))
                Value = tryParse;
            else
                Value = Values.ENU;
        }

        public enum Values {
            DEU,
            ENU,
            ESP,
            FRA,
            ITA,
            PTB,
            RUS,

            /* Planned for future implementation:
            ARA,
            HEB,
            JPN,
            CHS
            */
        }

        public string Description { get { return Descriptions [(int)Value]; } }

        public static List<string> MenuItem_Formats {
            get { return Descriptions; }
        }
        public static Values Parse_MenuItem (string inc) {
            try {
                int i = Descriptions.FindIndex (o => { return o == inc; });
                if (i >= 0)
                    return (Values)Enum.GetValues (typeof (Values)).GetValue (i);
                else
                    return Values.ENU;
            } catch {
                return Values.ENU;
            }
        }

        public static List<string> Descriptions = new List<string> {
            "Deutsche (German)",
            "English",
            "Español (Spanish)",
            "Français (French)",
            "Italiano (Italian)",
            "Português (Portugese)",
            "русский (Russian)",
        };
    }


    public static class Strings {
        public static string Lookup(Languages.Values lang, string key) {
            Pair pair;
            switch (lang) {

                default:
                case Languages.Values.ENU: pair = ENU.Find (o => { return o.Index == key; }); break;

                case Languages.Values.DEU: pair = DEU.Find (o => { return o.Index == key; }); break;
                case Languages.Values.ESP: pair = ESP.Find (o => { return o.Index == key; }); break;
                case Languages.Values.FRA: pair = FRA.Find (o => { return o.Index == key; }); break;
                case Languages.Values.ITA: pair = ITA.Find (o => { return o.Index == key; }); break;
                case Languages.Values.PTB: pair = PTB.Find (o => { return o.Index == key; }); break;
                case Languages.Values.RUS: pair = RUS.Find (o => { return o.Index == key; }); break;

                /* Planned for future implementation:
                case Languages.Values.ARA: pair = ARA.Find (o => { return o.Index == key; }); break;
                case Languages.Values.HEB: pair = HEB.Find (o => { return o.Index == key; }); break;
                case Languages.Values.JPN: pair = JPN.Find (o => { return o.Index == key; }); break;
                case Languages.Values.CHS: pair = CHS.Find (o => { return o.Index == key; }); break;
                */
            }
            if (pair != null)
                return pair.Value;
            else
                return "?!ERROR?!";
        }


        public class Pair {
            public string Index { get; set; }
            public string Value { get; set; }

            public Pair () {
                Index = ""; Value = "";
            }
            public Pair (string index, string value) {
                Index = index; Value = value;
            }
        }

        static List<Pair> DEU = new List<Pair> {
            new Pair("BUTTON:Continue",                         "Fortsetzen"),
            new Pair("BUTTON:ApplyChanges",                     "Änderungen übernehmen"),
            new Pair("BUTTON:ResetParameters",                  "Parameter zurücksetzen"),

            new Pair("PE:WindowTitle",                          "Krankenstation Integriert: Geduldig"),
            new Pair("PE:MenuFile",                             "Datei"),
            new Pair("PE:MenuLoadSimulation",                   "Lastsimulation"),
            new Pair("PE:MenuSaveSimulation",                   "Simulation speichern"),
            new Pair("PE:MenuExitProgram",                      "Ausfahrt Krankenstation Integriert"),
            new Pair("PE:MenuSettings",                         "die Einstellungen"),
            new Pair("PE:MenuSetLanguage",                      "Sprache wählen"),
            new Pair("PE:MenuHelp",                             "Hilfe"),
            new Pair("PE:MenuAboutProgram",                     "Über Krankenstation integriert"),

            new Pair("PE:Devices",                              "Geräte"),
            new Pair("PE:CardiacMonitor",                       "Herz-Monitor"),
            new Pair("PE:12LeadECG",                            "12-Kanal-EKG"),
            new Pair("PE:Defibrillator",                        "Defibrillator"),
            new Pair("PE:Ventilator",                           "Ventilator"),
            new Pair("PE:IABP",                                 "Intraaortale Ballonpumpe"),
            new Pair("PE:Cardiotocograph",                      "Kardiotokograph"),
            new Pair("PE:IVPump",                               "IV Pumpe"),
            new Pair("PE:LabResults",                           "Laborergebnisse"),

            new Pair("PE:HeartRate",                            "Puls"),
            new Pair("PE:BloodPressure",                        "Blutdruck"),
            new Pair("PE:RespiratoryRate",                      "Atemfrequenz"),
            new Pair("PE:PulseOximetry",                        "Pulsoximetrie"),
            new Pair("PE:Temperature",                          "Temperatur"),
            new Pair("PE:EndTidalCO2",                          "End Tide CO2"),
            new Pair("PE:ArterialBloodPressure",                "Arterieller Blutdruck"),
            new Pair("PE:CentralVenousPressure",                "Zentralvenöser Druck"),
            new Pair("PE:PulmonaryArteryPressure",              "Lungenarterien Druck"),
            new Pair("PE:RespiratoryRhythm",                    "Atemrhythmus"),
            new Pair("PE:InspiratoryExpiratoryRatio",           "Inspiratorisches-Exspiratorisches Verhältnis"),
            new Pair("PE:CardiacRhythm",                        "Herzrhythmus"),
            new Pair("PE:VitalSigns",                           "Vitalfunktionen"),
            new Pair("PE:AdvancedHemodynamics",                 "Fortgeschrittene Hämodynamik"),
            new Pair("PE:RespiratoryProfile",                   "Atmungsprofil"),
            new Pair("PE:CardiacProfile",                       "Herz-Profil"),
            new Pair("PE:UseDefaultVitalSignRanges",            "Verwenden Sie Standard-Vitalparameter für die Rhythmusauswahl?"),
            new Pair("PE:STSegmentElevation",                   "ST-Segmenthöhe"),
            new Pair("PE:TWaveElevation",                       "T Wellenhöhe"),

            new Pair("CM:MenuDeviceOptions",                    "Geräteoptionen"),
            new Pair("CM:MenuPauseDevice",                      "Gerät anhalten"),
            new Pair("CM:MenuNumericRowAmounts",                "Numerische Zeilenbeträge"),
            new Pair("CM:MenuTracingRowAmounts",                "Verfolgen von Reihenbeträgen"),
            new Pair("CM:MenuFontSize",                         "Schriftgröße"),
            new Pair("CM:MenuColorScheme",                      "Farbschema"),
            new Pair("CM:MenuToggleFullscreen",                 "Vollbild umschalten"),
            new Pair("CM:MenuCloseDevice",                      "Gerät schließen"),
            new Pair("CM:MenuPatientOptions",                   "Patientenoptionen"),
            new Pair("CM:MenuNewPatient",                       "Neuer Patient"),
            new Pair("CM:MenuEditPatient",                      "Patient bearbeiten"),

            new Pair("ABOUT:AboutProgram",                      "Über Krankenstation integriert"),
            new Pair("ABOUT:InfirmaryIntegrated",               "Krankenstation integriert"),
            new Pair("ABOUT:Version",                           "Version {0}"),
            new Pair("ABOUT:Description",                       "Infirmary Integrated ist eine kostenlose und Open-Source-Software, die entwickelt wurde, um die medizinische Ausbildung von Medizinern, Pflegekräften und Studenten zu fördern. Infirmary Integrated wurde als umfassendes, genaues und zugängliches pädagogisches Werkzeug entwickelt und kann die Anforderungen von klinischen Simulatoren in Notfall-, Intensivpflege- und vielen anderen medizinischen und pflegerischen Bereichen erfüllen."),

            new Pair("LANG:LanguageSelection",                  "Sprachauswahl"),
            new Pair("LANG:ChooseLanguage",                     "Bitte wähle deine Sprache:"),
        };

        static List<Pair> ENU = new List<Pair> {
            new Pair("BUTTON:Continue",                         "Continue"),
            new Pair("BUTTON:ApplyChanges",                     "Apply Changes"),
            new Pair("BUTTON:ResetParameters",                  "Reset Parameters"),

            new Pair("PROG:Title",                              "Infirmary Integrated"),

            new Pair("PE:WindowTitle",                          "Infirmary Integrated: Patient Editor"),
            new Pair("PE:MenuFile",                             "_File"),
            new Pair("PE:MenuLoadSimulation",                   "_Load Simulation"),
            new Pair("PE:MenuSaveSimulation",                   "_Save Simulation"),
            new Pair("PE:MenuExitProgram",                      "E_xit Infirmary Integrated"),
            new Pair("PE:MenuSettings",                         "_Settings"),
            new Pair("PE:MenuSetLanguage",                      "Set _Language"),
            new Pair("PE:MenuHelp",                             "_Help"),
            new Pair("PE:MenuAboutProgram",                     "_About Infirmary Integrated"),

            new Pair("PE:Devices",                              "Devices"),
            new Pair("PE:CardiacMonitor",                       "Cardiac Monitor"),
            new Pair("PE:12LeadECG",                            "12 Lead ECG"),
            new Pair("PE:Defibrillator",                        "Defibrillator"),
            new Pair("PE:Ventilator",                           "Ventilator"),
            new Pair("PE:IABP",                                 "Intra-Aortic Balloon Pump"),
            new Pair("PE:Cardiotocograph",                      "Cardiotocograph"),
            new Pair("PE:IVPump",                               "IV Pump"),
            new Pair("PE:LabResults",                           "Laboratory Results"),

            new Pair("PE:HeartRate",                            "Heart Rate"),
            new Pair("PE:BloodPressure",                        "Blood Pressure"),
            new Pair("PE:RespiratoryRate",                      "Respiratory Rate"),
            new Pair("PE:PulseOximetry",                        "Pulse Oximetry"),
            new Pair("PE:Temperature",                          "Temperature"),
            new Pair("PE:EndTidalCO2",                          "End Tidal CO2"),
            new Pair("PE:ArterialBloodPressure",                "Arterial Blood Pressure"),
            new Pair("PE:CentralVenousPressure",                "Central Venous Pressure"),
            new Pair("PE:PulmonaryArteryPressure",              "Pulmonary Artery Pressure"),
            new Pair("PE:RespiratoryRhythm",                    "Respiratory Rhythm"),
            new Pair("PE:InspiratoryExpiratoryRatio",           "Inspiratory-Expiratory Ratio"),
            new Pair("PE:CardiacRhythm",                        "Cardiac Rhythm"),
            new Pair("PE:VitalSigns",                           "Vital Signs"),
            new Pair("PE:AdvancedHemodynamics",                 "Advanced Hemodynamics"),
            new Pair("PE:RespiratoryProfile",                   "Respiratory Profile"),
            new Pair("PE:CardiacProfile",                       "Cardiac Profile"),
            new Pair("PE:UseDefaultVitalSignRanges",            "Use default vital sign ranges for rhythm selections?"),
            new Pair("PE:STSegmentElevation",                   "ST Segment Elevation"),
            new Pair("PE:TWaveElevation",                       "T Wave Elevation"),

            new Pair("CM:WindowTitle",                          "Infirmary Integrated: Cardiac Monitor"),
            new Pair("CM:MenuDeviceOptions",                    "_Device Options"),
            new Pair("CM:MenuPauseDevice",                      "_Pause Device"),
            new Pair("CM:MenuAddTracing",                       "Add _Numeric"),
            new Pair("CM:MenuAddNumeric",                       "Add _Tracing"),
            new Pair("CM:MenuFontSize",                         "F_ont Size"),
            new Pair("CM:MenuFontSizeDecrease",                 "_Increase Size"),
            new Pair("CM:MenuFontSizeIncrease",                 "_Decrease Size"),
            new Pair("CM:MenuColorScheme",                      "Color _Scheme"),
            new Pair("CM:MenuToggleFullscreen",                 "Toggle _Fullscreen"),
            new Pair("CM:MenuCloseDevice",                      "_Close Device"),
            new Pair("CM:MenuExitProgram",                      "E_xit Infirmary Integrated"),

            new Pair("ABOUT:AboutProgram",                      "About Infirmary Integrated"),
            new Pair("ABOUT:InfirmaryIntegrated",               "Infirmary Integrated"),
            new Pair("ABOUT:Version",                           "Version {0}"),
            new Pair("ABOUT:Description",                       "Infirmary Integrated is free and open-source software developed to advance healthcare education for medical and nursing professionals and students. Developed as in-depth, accurate, and accessible educational tools, Infirmary Integrated can meet the needs of clinical simulators in emergency, critical care, and many other medical and nursing specialties."),

            new Pair("LANG:LanguageSelection",                  "Language Selection"),
            new Pair("LANG:ChooseLanguage",                     "Please choose your language:"),
        };

        static List<Pair> ESP = new List<Pair> {
            new Pair("BUTTON:Continue",                         "Continuar"),
            new Pair("BUTTON:ApplyChanges",                     "Aplicar cambios"),
            new Pair("BUTTON:ResetParameters",                  "Restablecer parámetros"),

            new Pair("PE:WindowTitle",                          "Enfermería Integrada: Paciente Editor"),
            new Pair("PE:MenuFile",                             "Archivo"),
            new Pair("PE:MenuLoadSimulation",                   "Simulación de carga"),
            new Pair("PE:MenuSaveSimulation",                   "Guardar Simulación"),
            new Pair("PE:MenuExitProgram",                      "Salida Enfermería Integrada"),
            new Pair("PE:MenuSettings",                         "Configuraciones"),
            new Pair("PE:MenuSetLanguage",                      "Elegir lenguaje"),
            new Pair("PE:MenuHelp",                             "Ayuda"),
            new Pair("PE:MenuAboutProgram",                     "Acerca de Infirmary Integrated"),

            new Pair("PE:Devices",                              "Dispositivos"),
            new Pair("PE:CardiacMonitor",                       "Monitor cardíaco"),
            new Pair("PE:12LeadECG",                            "12 ECG de plomo"),
            new Pair("PE:Defibrillator",                        "Desfibrilador"),
            new Pair("PE:Ventilator",                           "Ventilador"),
            new Pair("PE:IABP",                                 "Bomba de globo intraaórtica"),
            new Pair("PE:Cardiotocograph",                      "Cardiotocograph"),
            new Pair("PE:IVPump",                               "Bomba IV"),
            new Pair("PE:LabResults",                           "Resultados de laboratorio"),

            new Pair("PE:HeartRate",                            "Ritmo cardiaco"),
            new Pair("PE:BloodPressure",                        "Presión sanguínea"),
            new Pair("PE:RespiratoryRate",                      "La frecuencia respiratoria"),
            new Pair("PE:PulseOximetry",                        "Oximetría de pulso"),
            new Pair("PE:Temperature",                          "Temperatura"),
            new Pair("PE:EndTidalCO2",                          "End Tidal CO2"),
            new Pair("PE:ArterialBloodPressure",                "Presión Arterial"),
            new Pair("PE:CentralVenousPressure",                "Presión venosa central"),
            new Pair("PE:PulmonaryArteryPressure",              "Presión arterial pulmonar"),
            new Pair("PE:RespiratoryRhythm",                    "Ritmo respiratorio"),
            new Pair("PE:InspiratoryExpiratoryRatio",           "Relación inspiratoria-espiratoria"),
            new Pair("PE:CardiacRhythm",                        "Ritmo cardiaco"),
            new Pair("PE:VitalSigns",                           "Signos vitales"),
            new Pair("PE:AdvancedHemodynamics",                 "Hemodinámica avanzada"),
            new Pair("PE:RespiratoryProfile",                   "Perfil respiratorio"),
            new Pair("PE:CardiacProfile",                       "Perfil cardiaco"),
            new Pair("PE:UseDefaultVitalSignRanges",            "¿Utiliza los rangos de signos vitales por defecto para las selecciones de ritmo?"),
            new Pair("PE:STSegmentElevation",                   "Elevación del segmento ST"),
            new Pair("PE:TWaveElevation",                       "Elevación de onda T"),

            new Pair("CM:MenuDeviceOptions",                    "Opciones del aparato"),
            new Pair("CM:MenuPauseDevice",                      "Pausa el dispositivo"),
            new Pair("CM:MenuNumericRowAmounts",                "Cantidad de fila numérica"),
            new Pair("CM:MenuTracingRowAmounts",                "Rastreo de cantidades de fila"),
            new Pair("CM:MenuFontSize",                         "Tamaño de fuente"),
            new Pair("CM:MenuColorScheme",                      "Esquema de colores"),
            new Pair("CM:MenuToggleFullscreen",                 "Alternar pantalla completa"),
            new Pair("CM:MenuCloseDevice",                      "Cerrar dispositivo"),
            new Pair("CM:MenuPatientOptions",                   "Opciones de paciente"),
            new Pair("CM:MenuNewPatient",                       "Paciente nuevo"),
            new Pair("CM:MenuEditPatient",                      "Editar paciente"),

            new Pair("ABOUT:AboutProgram",                      "Acerca de Infirmary Integrated"),
            new Pair("ABOUT:InfirmaryIntegrated",               "Enfermería integrada"),
            new Pair("ABOUT:Version",                           "Versión {0}"),
            new Pair("ABOUT:Description",                       "Infirmary Integrated es un software gratuito y de código abierto desarrollado para promover la educación sanitaria para profesionales y estudiantes de medicina y enfermería. Desarrollado como herramientas educativas profundas, precisas y accesibles, Infirmary Integrated puede satisfacer las necesidades de los simuladores clínicos en emergencias, cuidados intensivos y muchas otras especialidades médicas y de enfermería."),

            new Pair("LANG:LanguageSelection",                  "Selección de idioma"),
            new Pair("LANG:ChooseLanguage",                     "Por favor elija su idioma:"),
        };

        static List<Pair> FRA = new List<Pair> {
            new Pair("BUTTON:Continue",                         "Continuer"),
            new Pair("BUTTON:ApplyChanges",                     "Appliquer les modifications"),
            new Pair("BUTTON:ResetParameters",                  "Réinitialiser les paramètres"),

            new Pair("PE:WindowTitle",                          "Infirmerie Intégrée: Patient Éditeur"),
            new Pair("PE:MenuFile",                             "Fichier"),
            new Pair("PE:MenuLoadSimulation",                   "Simulation de chargement"),
            new Pair("PE:MenuSaveSimulation",                   "Enregistrer la simulation"),
            new Pair("PE:MenuExitProgram",                      "Sortie Infirmerie Intégrée"),
            new Pair("PE:MenuSettings",                         "Paramètres"),
            new Pair("PE:MenuSetLanguage",                      "Définir la langue"),
            new Pair("PE:MenuHelp",                             "Aidez-moi"),
            new Pair("PE:MenuAboutProgram",                     "À propos d'Infirmary Integrated"),

            new Pair("PE:Devices",                              "Dispositifs"),
            new Pair("PE:CardiacMonitor",                       "Moniteur cardiaque"),
            new Pair("PE:12LeadECG",                            "12 dérivations ECG"),
            new Pair("PE:Defibrillator",                        "Défibrillateur"),
            new Pair("PE:Ventilator",                           "Ventilateur"),
            new Pair("PE:IABP",                                 "Pompe à ballon intra-aortique"),
            new Pair("PE:Cardiotocograph",                      "Cardiotocographe"),
            new Pair("PE:IVPump",                               "Pompe IV"),
            new Pair("PE:LabResults",                           "Résultats de laboratoire"),

            new Pair("PE:HeartRate",                            "Rythme cardiaque"),
            new Pair("PE:BloodPressure",                        "Tension artérielle"),
            new Pair("PE:RespiratoryRate",                      "Fréquence respiratoire"),
            new Pair("PE:PulseOximetry",                        "Oxymétrie de pouls"),
            new Pair("PE:Temperature",                          "Température"),
            new Pair("PE:EndTidalCO2",                          "Fin du CO2 marémotrice"),
            new Pair("PE:ArterialBloodPressure",                "Pression artérielle artérielle"),
            new Pair("PE:CentralVenousPressure",                "Pression veineuse centrale"),
            new Pair("PE:PulmonaryArteryPressure",              "Pression artérielle pulmonaire"),
            new Pair("PE:RespiratoryRhythm",                    "Rythme respiratoire"),
            new Pair("PE:InspiratoryExpiratoryRatio",           "Ratio inspiratoire-expiratoire"),
            new Pair("PE:CardiacRhythm",                        "Rythme cardiaque"),
            new Pair("PE:VitalSigns",                           "Signes vitaux"),
            new Pair("PE:AdvancedHemodynamics",                 "Hémodynamique avancée"),
            new Pair("PE:RespiratoryProfile",                   "Profil respiratoire"),
            new Pair("PE:CardiacProfile",                       "Profil cardiaque"),
            new Pair("PE:UseDefaultVitalSignRanges",            "Utiliser les plages de signes vitaux par défaut pour les sélections de rythme?"),
            new Pair("PE:STSegmentElevation",                   "Élévation du segment ST"),
            new Pair("PE:TWaveElevation",                       "T élévation d'onde"),

            new Pair("CM:MenuDeviceOptions",                    "Options de l'appareil"),
            new Pair("CM:MenuPauseDevice",                      "Mettre le périphérique en veille"),
            new Pair("CM:MenuNumericRowAmounts",                "Montants de lignes numériques"),
            new Pair("CM:MenuTracingRowAmounts",                "Tracing Row Montants"),
            new Pair("CM:MenuFontSize",                         "Taille de police"),
            new Pair("CM:MenuColorScheme",                      "Schéma de couleur"),
            new Pair("CM:MenuToggleFullscreen",                 "Basculer en plein écran"),
            new Pair("CM:MenuCloseDevice",                      "Fermer le périphérique"),
            new Pair("CM:MenuPatientOptions",                   "Options du patient"),
            new Pair("CM:MenuNewPatient",                       "Nouveau patient"),
            new Pair("CM:MenuEditPatient",                      "Modifier le patient"),

            new Pair("ABOUT:AboutProgram",                      "À propos d'Infirmary Integrated"),
            new Pair("ABOUT:InfirmaryIntegrated",               "Infirmerie intégrée"),
            new Pair("ABOUT:Version",                           "Version {0}"),
            new Pair("ABOUT:Description",                       "Infirmary Integrated est un logiciel gratuit et open-source développé pour faire progresser l'éducation sanitaire pour les professionnels médicaux et infirmiers et les étudiants. Développé comme des outils éducatifs approfondis, précis et accessibles, Infirmary Integrated peut répondre aux besoins des simulateurs cliniques en urgence, en soins intensifs et dans de nombreuses autres spécialités médicales et infirmières."),

            new Pair("LANG:LanguageSelection",                  "Sélection de la langue"),
            new Pair("LANG:ChooseLanguage",                     "S'il vous plaît Choisissez votre langue:"),
        };

        static List<Pair> ITA = new List<Pair> {
            new Pair("BUTTON:Continue",                         "Continua"),
            new Pair("BUTTON:ApplyChanges",                     "Applica i cambiamenti"),
            new Pair("BUTTON:ResetParameters",                  "Ripristina i parametri"),

            new Pair("PE:WindowTitle",                          "Infermeria Integrata: Editor Paziente"),
            new Pair("PE:MenuFile",                             "File"),
            new Pair("PE:MenuLoadSimulation",                   "Carica simulazione"),
            new Pair("PE:MenuSaveSimulation",                   "Salva simulazione"),
            new Pair("PE:MenuExitProgram",                      "Esci dall'Infermeria Integrata"),
            new Pair("PE:MenuSettings",                         "Impostazioni"),
            new Pair("PE:MenuSetLanguage",                      "Imposta lingua"),
            new Pair("PE:MenuHelp",                             "Aiuto"),
            new Pair("PE:MenuAboutProgram",                     "Informazioni sull'infermeria integrata"),

            new Pair("PE:Devices",                              "dispositivi"),
            new Pair("PE:CardiacMonitor",                       "Monitor cardiaco"),
            new Pair("PE:12LeadECG",                            "12 elettrocatetere"),
            new Pair("PE:Defibrillator",                        "Defibrillatore"),
            new Pair("PE:Ventilator",                           "Ventilatore"),
            new Pair("PE:IABP",                                 "Pompa a palloncino intra-aortica"),
            new Pair("PE:Cardiotocograph",                      "Cardiotocografo"),
            new Pair("PE:IVPump",                               "Pompa IV"),
            new Pair("PE:LabResults",                           "Risultati di laboratorio"),

            new Pair("PE:HeartRate",                            "Frequenza cardiaca"),
            new Pair("PE:BloodPressure",                        "Pressione sanguigna"),
            new Pair("PE:RespiratoryRate",                      "Frequenza respiratoria"),
            new Pair("PE:PulseOximetry",                        "Pulsossimetria"),
            new Pair("PE:Temperature",                          "Temperatura"),
            new Pair("PE:EndTidalCO2",                          "End Tidal CO2"),
            new Pair("PE:ArterialBloodPressure",                "Pressione arteriosa"),
            new Pair("PE:CentralVenousPressure",                "Pressione venosa centrale"),
            new Pair("PE:PulmonaryArteryPressure",              "Pressione arteriosa polmonare"),
            new Pair("PE:RespiratoryRhythm",                    "Ritmo respiratorio"),
            new Pair("PE:InspiratoryExpiratoryRatio",           "Rapporto inspiratorio-espiratorio"),
            new Pair("PE:CardiacRhythm",                        "Ritmo cardiaco"),
            new Pair("PE:VitalSigns",                           "Segni vitali"),
            new Pair("PE:AdvancedHemodynamics",                 "Emodinamica avanzata"),
            new Pair("PE:RespiratoryProfile",                   "Profilo respiratorio"),
            new Pair("PE:CardiacProfile",                       "Profilo cardiaco"),
            new Pair("PE:UseDefaultVitalSignRanges",            "Utilizzare gli intervalli di segni vitali predefiniti per le selezioni del ritmo?"),
            new Pair("PE:STSegmentElevation",                   "Elevazione del segmento ST"),
            new Pair("PE:TWaveElevation",                       "T Wave Elevation"),

            new Pair("CM:MenuDeviceOptions",                    "Opzioni del dispositivo"),
            new Pair("CM:MenuPauseDevice",                      "Metti in pausa il dispositivo"),
            new Pair("CM:MenuNumericRowAmounts",                "Quantità di righe numeriche"),
            new Pair("CM:MenuTracingRowAmounts",                "Quantità di tracce di traccia"),
            new Pair("CM:MenuFontSize",                         "Dimensione del font"),
            new Pair("CM:MenuColorScheme",                      "Combinazione di colori"),
            new Pair("CM:MenuToggleFullscreen",                 "Passare a schermo intero"),
            new Pair("CM:MenuCloseDevice",                      "Chiudi dispositivo"),
            new Pair("CM:MenuPatientOptions",                   "Opzioni del paziente"),
            new Pair("CM:MenuNewPatient",                       "Nuovo paziente"),
            new Pair("CM:MenuEditPatient",                      "Modifica paziente"),

            new Pair("ABOUT:AboutProgram",                      "Informazioni sull'infermeria integrata"),
            new Pair("ABOUT:InfirmaryIntegrated",               "Infermeria integrata"),
            new Pair("ABOUT:Version",                           "Versione {0}"),
            new Pair("ABOUT:Description",                       "Infermary Integrated è un software gratuito e open source sviluppato per promuovere la formazione sanitaria per medici e infermieri e studenti. Sviluppato come strumenti educativi approfonditi, accurati e accessibili, l'infermeria integrata è in grado di soddisfare le esigenze dei simulatori clinici in emergenza, terapia intensiva e molte altre specialità mediche e infermieristiche."),

            new Pair("LANG:LanguageSelection",                  "Selezione della lingua"),
            new Pair("LANG:ChooseLanguage",                     "Per favore scegli la tua lingua:"),
        };

        static List<Pair> PTB = new List<Pair> {
            new Pair("BUTTON:Continue",                         "Continuar"),
            new Pair("BUTTON:ApplyChanges",                     "Aplicar mudanças"),
            new Pair("BUTTON:ResetParameters",                  "Parâmetros de redefinição"),

            new Pair("PE:WindowTitle",                          "Infirmary Integrated: Editor de Pacientes"),
            new Pair("PE:MenuFile",                             "Arquivo"),
            new Pair("PE:MenuLoadSimulation",                   "Simulação de carga"),
            new Pair("PE:MenuSaveSimulation",                   "Save Simulation"),
            new Pair("PE:MenuExitProgram",                      "Exit Infirmary Integrated"),
            new Pair("PE:MenuSettings",                         "Configurações"),
            new Pair("PE:MenuSetLanguage",                      "Definir idioma"),
            new Pair("PE:MenuHelp",                             "Socorro"),
            new Pair("PE:MenuAboutProgram",                     "Sobre Infirmary Integrated"),

            new Pair("PE:Devices",                              "Dispositivos"),
            new Pair("PE:CardiacMonitor",                       "Monitor cardíaco"),
            new Pair("PE:12LeadECG",                            "12 ECG de chumbo"),
            new Pair("PE:Defibrillator",                        "Desfibrilador"),
            new Pair("PE:Ventilator",                           "Ventilador"),
            new Pair("PE:IABP",                                 "Bomba de balão intra-aórtico"),
            new Pair("PE:Cardiotocograph",                      "Cardiotocografia"),
            new Pair("PE:IVPump",                               "Bomba IV"),
            new Pair("PE:LabResults",                           "Resultados laboratoriais"),

            new Pair("PE:HeartRate",                            "Frequência cardíaca"),
            new Pair("PE:BloodPressure",                        "Pressão sanguínea"),
            new Pair("PE:RespiratoryRate",                      "Frequência respiratória"),
            new Pair("PE:PulseOximetry",                        "Oximetria de pulso"),
            new Pair("PE:Temperature",                          "Temperatura"),
            new Pair("PE:EndTidalCO2",                          "End Tidal CO2"),
            new Pair("PE:ArterialBloodPressure",                "Pressão Arterial"),
            new Pair("PE:CentralVenousPressure",                "Pressão Venosa Central"),
            new Pair("PE:PulmonaryArteryPressure",              "Pressão da artéria pulmonar"),
            new Pair("PE:RespiratoryRhythm",                    "Ritmo respiratório"),
            new Pair("PE:InspiratoryExpiratoryRatio",           "Razão Inspiratório-Expiratório"),
            new Pair("PE:CardiacRhythm",                        "Ritmo Cardíaco"),
            new Pair("PE:VitalSigns",                           "Sinais vitais"),
            new Pair("PE:AdvancedHemodynamics",                 "Hemodinâmica Avançada"),
            new Pair("PE:RespiratoryProfile",                   "Perfil Respiratório"),
            new Pair("PE:CardiacProfile",                       "Perfil cardíaco"),
            new Pair("PE:UseDefaultVitalSignRanges",            "Use padrões de registro vital padrão para seleções de ritmo?"),
            new Pair("PE:STSegmentElevation",                   "Elevação do segmento ST"),
            new Pair("PE:TWaveElevation",                       "Elevação da Onda T"),

            new Pair("CM:MenuDeviceOptions",                    "Opções do dispositivo"),
            new Pair("CM:MenuPauseDevice",                      "Pause Device"),
            new Pair("CM:MenuNumericRowAmounts",                "Quantidades de linhas numéricas"),
            new Pair("CM:MenuTracingRowAmounts",                "Quantidades de linhas de rastreamento"),
            new Pair("CM:MenuFontSize",                         "Tamanho da fonte"),
            new Pair("CM:MenuColorScheme",                      "Esquema de cores"),
            new Pair("CM:MenuToggleFullscreen",                 "Alternar para o modo tela cheia"),
            new Pair("CM:MenuCloseDevice",                      "Fechar dispositivo"),
            new Pair("CM:MenuPatientOptions",                   "Opções do paciente"),
            new Pair("CM:MenuNewPatient",                       "Paciente novo"),
            new Pair("CM:MenuEditPatient",                      "Editar Paciente"),

            new Pair("ABOUT:AboutProgram",                      "Sobre Infirmary Integrated"),
            new Pair("ABOUT:InfirmaryIntegrated",               "Infirmary Integrated"),
            new Pair("ABOUT:Version",                           "Versão {0}"),
            new Pair("ABOUT:Description",                       "Infirmary Integrated é um software livre e de código aberto desenvolvido para promover a educação em saúde para profissionais e estudantes de medicina e enfermagem. Desenvolvido como ferramentas educacionais detalhadas, precisas e acessíveis, a Infirmary Integrated pode atender às necessidades de simuladores clínicos em atendimento urgente, crítico e muitas outras especialidades médicas e de enfermagem."),

            new Pair("LANG:LanguageSelection",                  "Seleção de idioma"),
            new Pair("LANG:ChooseLanguage",                     "Escolha o seu idioma:"),
        };

        static List<Pair> RUS = new List<Pair> {
            new Pair("BUTTON:Continue",                         "Продолжать"),
            new Pair("BUTTON:ApplyChanges",                     "Применить изменения"),
            new Pair("BUTTON:ResetParameters",                  "Сбросить параметры"),

            new Pair("PE:WindowTitle",                          "Интегрированный комплекс: Редактор пациента"),
            new Pair("PE:MenuFile",                             "файл"),
            new Pair("PE:MenuLoadSimulation",                   "Моделирование нагрузки"),
            new Pair("PE:MenuSaveSimulation",                   "Сохранить моделирование"),
            new Pair("PE:MenuExitProgram",                      "Выход из интернатуры Интегрированный"),
            new Pair("PE:MenuSettings",                         "настройки"),
            new Pair("PE:MenuSetLanguage",                      "Установить язык"),
            new Pair("PE:MenuHelp",                             "Помогите"),
            new Pair("PE:MenuAboutProgram",                     "О комплексе"),

            new Pair("PE:Devices",                              "приборы"),
            new Pair("PE:CardiacMonitor",                       "Сердечный монитор"),
            new Pair("PE:12LeadECG",                            "12 ЭКГ ЭКГ"),
            new Pair("PE:Defibrillator",                        "дефибриллятор"),
            new Pair("PE:Ventilator",                           "вентилятор"),
            new Pair("PE:IABP",                                 "Внутриаортальный воздушный насос"),
            new Pair("PE:Cardiotocograph",                      "Кардиотокография"),
            new Pair("PE:IVPump",                               "IV насос"),
            new Pair("PE:LabResults",                           "Лабораторные результаты"),

            new Pair("PE:HeartRate",                            "Частота сердцебиения"),
            new Pair("PE:BloodPressure",                        "Кровяное давление"),
            new Pair("PE:RespiratoryRate",                      "Частота дыхания"),
            new Pair("PE:PulseOximetry",                        "Пульсовая оксиметрия"),
            new Pair("PE:Temperature",                          "температура"),
            new Pair("PE:EndTidalCO2",                          "End Tidal CO2"),
            new Pair("PE:ArterialBloodPressure",                "Артериальное кровяное давление"),
            new Pair("PE:CentralVenousPressure",                "Центральное венозное давление"),
            new Pair("PE:PulmonaryArteryPressure",              "Давление легочной артерии"),
            new Pair("PE:RespiratoryRhythm",                    "Дыхательный ритм"),
            new Pair("PE:InspiratoryExpiratoryRatio",           "Вдохново-экспираторный коэффициент"),
            new Pair("PE:CardiacRhythm",                        "Сердечный ритм"),
            new Pair("PE:VitalSigns",                           "Жизненно важные признаки"),
            new Pair("PE:AdvancedHemodynamics",                 "Продвинутая гемодинамика"),
            new Pair("PE:RespiratoryProfile",                   "Респираторный профиль"),
            new Pair("PE:CardiacProfile",                       "Профиль сердца"),
            new Pair("PE:UseDefaultVitalSignRanges",            "Использовать диапазоны жизненно важных значений по умолчанию для выбора ритма?"),
            new Pair("PE:STSegmentElevation",                   "Высота сегмента ST"),
            new Pair("PE:TWaveElevation",                       "T Wave Elevation"),

            new Pair("CM:MenuDeviceOptions",                    "Параметры устройства"),
            new Pair("CM:MenuPauseDevice",                      "Приостановить устройство"),
            new Pair("CM:MenuNumericRowAmounts",                "Числовые значения строк"),
            new Pair("CM:MenuTracingRowAmounts",                "Трассировка строк"),
            new Pair("CM:MenuFontSize",                         "Размер шрифта"),
            new Pair("CM:MenuColorScheme",                      "Цветовая схема"),
            new Pair("CM:MenuToggleFullscreen",                 "Включить полноэкранный режим"),
            new Pair("CM:MenuCloseDevice",                      "Закрыть устройство"),
            new Pair("CM:MenuPatientOptions",                   "Параметры пациента"),
            new Pair("CM:MenuNewPatient",                       "Новый пациент"),
            new Pair("CM:MenuEditPatient",                      "Редактировать пациента"),

            new Pair("ABOUT:AboutProgram",                      "О комплексе"),
            new Pair("ABOUT:InfirmaryIntegrated",               "Интегрированный комплекс"),
            new Pair("ABOUT:Version",                           "Версия {0}"),
            new Pair("ABOUT:Description",                       "Infirmary Integrated - бесплатное программное обеспечение с открытым исходным кодом, разработанное для продвижения медицинского образования для медицинских и сестринских специалистов и студентов. Разработанный как углубленный, точный и доступный образовательный инструмент, Infirmary Integrated может удовлетворить потребности клинических тренажеров в неотложной, критической медицинской помощи и многих других медицинских и сестринских специальностях."),

            new Pair("LANG:LanguageSelection",                  "Выбор языка"),
            new Pair("LANG:ChooseLanguage",                     "Выберите язык:"),
        };
    }
}
