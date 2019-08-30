using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using II;
using II.Scenario_Editor.Controls;

namespace II.Scenario_Editor {

    public partial class Editor : Window {

        // Variables for capturing mouse and dragging UI elements
        private bool mouseCaptured = false;

        private double xShape, yShape,
            xCanvas, yCanvas;

        private Canvas canvasDesigner;
        private UIElement selectedElement = null;
        private Patient copiedPatient;

        private int selectedStep = -1,
            selectedProgression = -1;

        private List<ItemStep> Steps = new List<ItemStep> ();

        private Point DefaultSize_StepItem = new Point (50, 50);
        private Point DefaultSize_Progression = new Point (50, 50);

        private Brush Fill = (SolidColorBrush)(new BrushConverter ().ConvertFrom ("#dddddd")),
            Stroke = (SolidColorBrush)(new BrushConverter ().ConvertFrom ("#555555"));

        public Editor () {
            InitializeComponent ();

            canvasDesigner = cnvsDesigner;
        }

        private void setPropertiesView () {
            if (selectedElement == null)
                return;

            if (selectedElement is ItemStep) {
                selectedStep = Steps.FindIndex (o => { return o == selectedElement; });
                lblProperties.Content = String.Concat ("Edit Step: #", selectedStep.ToString ("000"));

                ItemStep si = (ItemStep)selectedElement;
                scrlPropertiesPatient.Visibility = Visibility.Visible;

                // Populate enum string lists for readable display
                List<string> cardiacRhythms = new List<string> (),
                    respiratoryRhythms = new List<string> (),
                    pulmonaryRhythms = new List<string> (),
                    cardiacAxes = new List<string> (),
                    intensityScale = new List<string> (),
                    fetalHeartRhythms = new List<string> ();

                foreach (Cardiac_Rhythms.Values v in Enum.GetValues (typeof (Cardiac_Rhythms.Values)))
                    cardiacRhythms.Add (App.Language.Dictionary [Cardiac_Rhythms.LookupString (v)]);

                foreach (Respiratory_Rhythms.Values v in Enum.GetValues (typeof (Respiratory_Rhythms.Values)))
                    respiratoryRhythms.Add (App.Language.Dictionary [Respiratory_Rhythms.LookupString (v)]);

                foreach (PulmonaryArtery_Rhythms.Values v in Enum.GetValues (typeof (PulmonaryArtery_Rhythms.Values)))
                    pulmonaryRhythms.Add (App.Language.Dictionary [PulmonaryArtery_Rhythms.LookupString (v)]);

                foreach (Cardiac_Axes.Values v in Enum.GetValues (typeof (Cardiac_Axes.Values)))
                    cardiacAxes.Add (App.Language.Dictionary [Cardiac_Axes.LookupString (v)]);

                // Initiate controls for editing Patient values
                pstrName.Init (PropertyString.Keys.Name, si.Step.Name ?? "");
                pstrName.PropertyChanged += updateProperty;

                pstrDescription.Init (PropertyString.Keys.Description, si.Step.Description ?? "");
                pstrDescription.PropertyChanged += updateProperty;

                pintHR.Init (PropertyInt.Keys.HR, si.Patient.VS_Settings.HR, 5, 0, 500);
                pintHR.PropertyChanged += updateProperty;

                pbpNBP.Init (PropertyBP.Keys.NSBP,
                    si.Patient.VS_Settings.NSBP, si.Patient.VS_Settings.NDBP,
                    5, 0, 300,
                    5, 0, 200);
                pbpNBP.PropertyChanged += updateProperty;

                pintRR.Init (PropertyInt.Keys.RR, si.Patient.VS_Settings.RR, 2, 0, 100);
                pintRR.PropertyChanged += updateProperty;

                pintSPO2.Init (PropertyInt.Keys.SPO2, si.Patient.VS_Settings.SPO2, 2, 0, 100);
                pintSPO2.PropertyChanged += updateProperty;

                pdblT.Init (PropertyDouble.Keys.T, si.Patient.VS_Settings.T, 0.2, 0, 100);
                pdblT.PropertyChanged += updateProperty;

                penmCardiacRhythms.Init (PropertyEnum.Keys.Cardiac_Rhythms,
                    Enum.GetNames (typeof (Cardiac_Rhythms.Values)),
                    cardiacRhythms, (int)si.Patient.Cardiac_Rhythm.Value);
                penmCardiacRhythms.PropertyChanged += updateProperty;
                penmCardiacRhythms.PropertyChanged += updateCardiacRhythm;

                penmRespiratoryRhythms.Init (PropertyEnum.Keys.Respiratory_Rhythms,
                    Enum.GetNames (typeof (Respiratory_Rhythms.Values)),
                    respiratoryRhythms, (int)si.Patient.Respiratory_Rhythm.Value);
                penmRespiratoryRhythms.PropertyChanged += updateProperty;
                penmRespiratoryRhythms.PropertyChanged += updateRespiratoryRhythm;

                pintETCO2.Init (PropertyInt.Keys.ETCO2, si.Patient.VS_Settings.ETCO2, 2, 0, 100);
                pintETCO2.PropertyChanged += updateProperty;

                pintCVP.Init (PropertyInt.Keys.CVP, si.Patient.VS_Settings.CVP, 1, -100, 100);
                pintCVP.PropertyChanged += updateProperty;

                pbpABP.Init (PropertyBP.Keys.ASBP,
                    si.Patient.VS_Settings.ASBP, si.Patient.VS_Settings.ADBP,
                    5, 0, 300,
                    5, 0, 200);
                pbpABP.PropertyChanged += updateProperty;

                penmPACatheterRhythm.Init (PropertyEnum.Keys.PACatheter_Rhythms,
                    Enum.GetNames (typeof (PulmonaryArtery_Rhythms.Values)),
                    pulmonaryRhythms, (int)si.Patient.PulmonaryArtery_Placement.Value);
                penmPACatheterRhythm.PropertyChanged += updateProperty;
                penmPACatheterRhythm.PropertyChanged += updatePACatheterRhythm;

                pbpPBP.Init (PropertyBP.Keys.PSP,
                    si.Patient.VS_Settings.PSP, si.Patient.VS_Settings.PDP,
                    5, 0, 200,
                    5, 0, 200);
                pbpPBP.PropertyChanged += updateProperty;

                pintICP.Init (PropertyInt.Keys.ICP, si.Patient.VS_Settings.ICP, 1, -100, 100);
                pintICP.PropertyChanged += updateProperty;

                pintIAP.Init (PropertyInt.Keys.IAP, si.Patient.VS_Settings.IAP, 1, -100, 100);
                pintIAP.PropertyChanged += updateProperty;

                pchkMechanicallyVentilated.Init (PropertyCheck.Keys.MechanicallyVentilated, si.Patient.Mechanically_Ventilated);
                pchkMechanicallyVentilated.PropertyChanged += updateProperty;

                pdblInspiratoryRatio.Init (PropertyFloat.Keys.RRInspiratoryRatio, si.Patient.VS_Settings.RR_IE_I, 0.1, 0.1, 10);
                pdblInspiratoryRatio.PropertyChanged += updateProperty;

                pdblExpiratoryRatio.Init (PropertyFloat.Keys.RRExpiratoryRatio, si.Patient.VS_Settings.RR_IE_E, 0.1, 0.1, 10);
                pdblExpiratoryRatio.PropertyChanged += updateProperty;

                pintPacemakerThreshold.Init (PropertyInt.Keys.PacemakerThreshold, si.Patient.Pacemaker_Threshold, 5, 0, 200);
                pintPacemakerThreshold.PropertyChanged += updateProperty;

                pchkPulsusParadoxus.Init (PropertyCheck.Keys.PulsusParadoxus, si.Patient.Pulsus_Paradoxus);
                pchkPulsusParadoxus.PropertyChanged += updateProperty;

                pchkPulsusAlternans.Init (PropertyCheck.Keys.PulsusAlternans, si.Patient.Pulsus_Alternans);
                pchkPulsusAlternans.PropertyChanged += updateProperty;

                penmCardiacAxis.Init (PropertyEnum.Keys.Cardiac_Axis,
                    Enum.GetNames (typeof (Cardiac_Axes.Values)),
                    cardiacAxes, (int)si.Patient.Cardiac_Axis.Value);
                penmCardiacAxis.PropertyChanged += updateProperty;

                pecgSTSegment.Init (PropertyECGSegment.Keys.STElevation, si.Patient.ST_Elevation);
                pecgSTSegment.PropertyChanged += updateProperty;

                pecgTWave.Init (PropertyECGSegment.Keys.TWave, si.Patient.T_Elevation);
                pecgTWave.PropertyChanged += updateProperty;
            }
        }

        private void updatePropertiesView () {
            if (selectedElement == null)
                return;

            if (selectedElement is ItemStep) {
                selectedStep = Steps.FindIndex (o => { return o == selectedElement; });
                lblProperties.Content = String.Concat ("Edit Step: #", selectedStep.ToString ("000"));

                ItemStep si = (ItemStep)selectedElement;
                scrlPropertiesPatient.Visibility = Visibility.Visible;

                // Update all controls with Patient values
                pstrName.Set (si.Step.Name ?? "");
                pstrDescription.Set (si.Step.Description ?? "");
                pintHR.Set (si.Patient.VS_Settings.HR);
                pbpNBP.Set (si.Patient.VS_Settings.NSBP, si.Patient.VS_Settings.NDBP);
                pintRR.Set (si.Patient.VS_Settings.RR);
                pintSPO2.Set (si.Patient.VS_Settings.SPO2);
                pdblT.Set (si.Patient.VS_Settings.T);
                penmCardiacRhythms.Set ((int)si.Patient.Cardiac_Rhythm.Value);
                penmRespiratoryRhythms.Set ((int)si.Patient.Respiratory_Rhythm.Value);
                pintETCO2.Set (si.Patient.VS_Settings.ETCO2);
                pintCVP.Set (si.Patient.VS_Settings.CVP);
                pbpABP.Set (si.Patient.VS_Settings.ASBP, si.Patient.VS_Settings.ADBP);
                pbpPBP.Set (si.Patient.VS_Settings.PSP, si.Patient.VS_Settings.PDP);
                pintICP.Set (si.Patient.VS_Settings.ICP);
                pintIAP.Set (si.Patient.VS_Settings.IAP);
                pchkMechanicallyVentilated.Set (si.Patient.Mechanically_Ventilated);
                pdblInspiratoryRatio.Set (si.Patient.VS_Settings.RR_IE_I);
                pdblExpiratoryRatio.Set (si.Patient.VS_Settings.RR_IE_E);
                pintPacemakerThreshold.Set (si.Patient.Pacemaker_Threshold);
                pchkPulsusParadoxus.Set (si.Patient.Pulsus_Paradoxus);
                pchkPulsusAlternans.Set (si.Patient.Pulsus_Alternans);
                penmCardiacAxis.Set ((int)si.Patient.Cardiac_Axis.Value);
                pecgSTSegment.Set (si.Patient.ST_Elevation);
                pecgTWave.Set (si.Patient.T_Elevation);
            }
        }

        private void updateProperty (object sender, PropertyString.PropertyStringEventArgs e) {
            if (selectedElement is ItemStep) {
                ItemStep si = (ItemStep)selectedElement;
                switch (e.Key) {
                    default: break;
                    case PropertyString.Keys.Name:
                        si.SetName (e.Value);
                        break;

                    case PropertyString.Keys.Description: si.Step.Description = e.Value; break;
                }
            }
        }

        private void updateProperty (object sender, PropertyInt.PropertyIntEventArgs e) {
            if (selectedElement is ItemStep) {
                ItemStep si = (ItemStep)selectedElement;
                switch (e.Key) {
                    default: break;
                    case PropertyInt.Keys.HR: si.Patient.HR = e.Value; break;
                    case PropertyInt.Keys.RR: si.Patient.RR = e.Value; break;
                    case PropertyInt.Keys.ETCO2: si.Patient.ETCO2 = e.Value; break;
                    case PropertyInt.Keys.SPO2: si.Patient.SPO2 = e.Value; break;
                    case PropertyInt.Keys.CVP: si.Patient.CVP = e.Value; break;
                    case PropertyInt.Keys.ICP: si.Patient.ICP = e.Value; break;
                    case PropertyInt.Keys.IAP: si.Patient.IAP = e.Value; break;
                    case PropertyInt.Keys.PacemakerThreshold: si.Patient.Pacemaker_Threshold = e.Value; break;
                }
            }
        }

        private void updateProperty (object sender, PropertyDouble.PropertyDoubleEventArgs e) {
            if (selectedElement is ItemStep) {
                ItemStep si = (ItemStep)selectedElement;
                switch (e.Key) {
                    default: break;
                    case PropertyDouble.Keys.T: si.Patient.T = e.Value; break;
                }
            }
        }

        private void updateProperty (object sender, PropertyFloat.PropertyFloatEventArgs e) {
            if (selectedElement is ItemStep) {
                ItemStep si = (ItemStep)selectedElement;
                switch (e.Key) {
                    default: break;
                    case PropertyFloat.Keys.RRInspiratoryRatio: si.Patient.RR_IE_I = e.Value; break;
                    case PropertyFloat.Keys.RRExpiratoryRatio: si.Patient.RR_IE_E = e.Value; break;
                }
            }
        }

        private void updateProperty (object sender, PropertyBP.PropertyIntEventArgs e) {
            if (selectedElement is ItemStep) {
                ItemStep si = (ItemStep)selectedElement;
                switch (e.Key) {
                    default: break;
                    case PropertyBP.Keys.NSBP: si.Patient.NSBP = e.Value; break;
                    case PropertyBP.Keys.NDBP: si.Patient.NDBP = e.Value; break;
                    case PropertyBP.Keys.NMAP: si.Patient.NMAP = e.Value; break;
                    case PropertyBP.Keys.ASBP: si.Patient.ASBP = e.Value; break;
                    case PropertyBP.Keys.ADBP: si.Patient.ADBP = e.Value; break;
                    case PropertyBP.Keys.AMAP: si.Patient.AMAP = e.Value; break;
                    case PropertyBP.Keys.PSP: si.Patient.PSP = e.Value; break;
                    case PropertyBP.Keys.PDP: si.Patient.PDP = e.Value; break;
                    case PropertyBP.Keys.PMP: si.Patient.PMP = e.Value; break;
                }
            }
        }

        private void updateProperty (object sender, PropertyEnum.PropertyEnumEventArgs e) {
            if (selectedElement is ItemStep) {
                ItemStep si = (ItemStep)selectedElement;
                switch (e.Key) {
                    default: break;

                    case PropertyEnum.Keys.Cardiac_Axis:
                        si.Patient.Cardiac_Axis.Value = (Cardiac_Axes.Values)Enum.Parse (typeof (Cardiac_Axes.Values), e.Value);
                        break;

                    case PropertyEnum.Keys.Cardiac_Rhythms:
                        si.Patient.Cardiac_Rhythm.Value = (Cardiac_Rhythms.Values)Enum.Parse (typeof (Cardiac_Rhythms.Values), e.Value);
                        break;

                    case PropertyEnum.Keys.Respiratory_Rhythms:
                        si.Patient.Respiratory_Rhythm.Value = (Respiratory_Rhythms.Values)Enum.Parse (typeof (Respiratory_Rhythms.Values), e.Value);
                        break;

                    case PropertyEnum.Keys.PACatheter_Rhythms:
                        si.Patient.PulmonaryArtery_Placement.Value = (PulmonaryArtery_Rhythms.Values)Enum.Parse (typeof (PulmonaryArtery_Rhythms.Values), e.Value);
                        break;
                }
            }
        }

        private void updateProperty (object sender, PropertyCheck.PropertyCheckEventArgs e) {
            if (selectedElement is ItemStep) {
                ItemStep si = (ItemStep)selectedElement;
                switch (e.Key) {
                    default: break;
                    case PropertyCheck.Keys.PulsusParadoxus: si.Patient.Pulsus_Paradoxus = e.Value; break;
                    case PropertyCheck.Keys.PulsusAlternans: si.Patient.Pulsus_Alternans = e.Value; break;
                    case PropertyCheck.Keys.MechanicallyVentilated: si.Patient.Mechanically_Ventilated = e.Value; break;
                }
            }
        }

        private void updateProperty (object sender, PropertyECGSegment.PropertyECGEventArgs e) {
            if (selectedElement is ItemStep) {
                ItemStep si = (ItemStep)selectedElement;
                switch (e.Key) {
                    default: break;
                    case PropertyECGSegment.Keys.STElevation: si.Patient.ST_Elevation = e.Values; break;
                    case PropertyECGSegment.Keys.TWave: si.Patient.T_Elevation = e.Values; break;
                }
            }
        }

        private void updateCardiacRhythm (object sender, PropertyEnum.PropertyEnumEventArgs e) {
            if (!chkClampVitals.IsChecked ?? false || selectedElement == null)
                return;

            if (selectedElement is ItemStep) {
                Patient p = ((ItemStep)selectedElement).Patient;

                Cardiac_Rhythms.Default_Vitals v = Cardiac_Rhythms.DefaultVitals (
                    (Cardiac_Rhythms.Values)Enum.Parse (typeof (Cardiac_Rhythms.Values), e.Value));

                p.HR = (int)Utility.Clamp ((double)p.VS_Settings.HR, v.HRMin, v.HRMax);
                p.RR = (int)Utility.Clamp ((double)p.VS_Settings.RR, v.RRMin, v.RRMax);
                p.SPO2 = (int)Utility.Clamp ((double)p.VS_Settings.SPO2, v.SPO2Min, v.SPO2Max);
                p.ETCO2 = (int)Utility.Clamp ((double)p.VS_Settings.ETCO2, v.ETCO2Min, v.ETCO2Max);
                p.NSBP = (int)Utility.Clamp ((double)p.VS_Settings.NSBP, v.SBPMin, v.SBPMax);
                p.NDBP = (int)Utility.Clamp ((double)p.VS_Settings.NDBP, v.DBPMin, v.DBPMax);
                p.ASBP = (int)Utility.Clamp ((double)p.VS_Settings.ASBP, v.SBPMin, v.SBPMax);
                p.ADBP = (int)Utility.Clamp ((double)p.VS_Settings.ADBP, v.DBPMin, v.DBPMax);
                p.PSP = (int)Utility.Clamp ((double)p.VS_Settings.PSP, v.PSPMin, v.PSPMax);
                p.PDP = (int)Utility.Clamp ((double)p.VS_Settings.PDP, v.PDPMin, v.PDPMax);

                updatePropertiesView ();
            }
        }

        private void updateRespiratoryRhythm (object sender, PropertyEnum.PropertyEnumEventArgs e) {
            if (!chkClampVitals.IsChecked ?? false || selectedElement == null)
                return;

            if (selectedElement is ItemStep) {
                Patient p = ((ItemStep)selectedElement).Patient;

                Respiratory_Rhythms.Default_Vitals v = Respiratory_Rhythms.DefaultVitals (
                    (Respiratory_Rhythms.Values)Enum.Parse (typeof (Respiratory_Rhythms.Values), e.Value));

                p.RR = (int)Utility.Clamp ((double)p.RR, v.RRMin, v.RRMax);
                p.RR_IE_I = (int)Utility.Clamp ((double)p.RR_IE_I, v.RR_IE_I_Min, v.RR_IE_I_Max);
                p.RR_IE_E = (int)Utility.Clamp ((double)p.RR_IE_E, v.RR_IE_E_Min, v.RR_IE_E_Max);

                updatePropertiesView ();
            }
        }

        private void updatePACatheterRhythm (object sender, PropertyEnum.PropertyEnumEventArgs e) {
            if (selectedElement == null)
                return;

            if (selectedElement is ItemStep) {
                Patient p = ((ItemStep)selectedElement).Patient;

                PulmonaryArtery_Rhythms.Default_Vitals v = PulmonaryArtery_Rhythms.DefaultVitals (
                    (PulmonaryArtery_Rhythms.Values)Enum.Parse (typeof (PulmonaryArtery_Rhythms.Values), e.Value));

                p.PSP = (int)Utility.Clamp ((double)p.PSP, v.PSPMin, v.PSPMax);
                p.PDP = (int)Utility.Clamp ((double)p.PDP, v.PDPMin, v.PDPMax);

                updatePropertiesView ();
            }
        }

        private void addStep (ItemStep ist = null, ItemProgression ip = null) {
            if (ist == null)
                ist = new ItemStep ();

            if (ip == null)
                ip = new ItemProgression ();

            // Tie the two elements together
            ist.Progression = ip;
            ip.Step = ist;

            // Set ItemStep properties
            ist.Width = DefaultSize_StepItem.X;
            ist.Height = DefaultSize_StepItem.Y;
            ist.Fill = Fill;
            ist.Stroke = Stroke;
            ist.StrokeThickness = 1.0;
            ist.MouseLeftButtonDown += UIElementMouseLeftButtonDown;
            ist.MouseLeftButtonUp += UIElementMouseLeftButtonUp;
            ist.MouseMove += UIElementMouseMove;

            // Set ItemProgression properties
            ip.Width = DefaultSize_Progression.X;
            ip.Height = DefaultSize_Progression.Y;
            ip.Fill = Fill;
            ip.Stroke = Stroke;
            ip.StrokeThickness = 0.75;
            ip.MouseLeftButtonDown += UIElementMouseLeftButtonDown;
            ip.MouseLeftButtonUp += UIElementMouseLeftButtonUp;

            // Add to lists and display elements
            Steps.Add (ist);
            canvasDesigner.Children.Add (ist);
            canvasDesigner.Children.Add (ist.Label);
            canvasDesigner.Children.Add (ip);

            // Set positions in visual space
            Canvas.SetLeft (ist, (cnvsDesigner.ActualWidth / 2) - (ist.Width / 2));
            Canvas.SetTop (ist, (cnvsDesigner.ActualHeight / 2) - (ist.Height / 2));
            Canvas.SetLeft (ist.Label, (cnvsDesigner.ActualWidth / 2) - (ist.Width / 2));
            Canvas.SetTop (ist.Label, (cnvsDesigner.ActualHeight / 2) - (ist.Height / 2));
            Canvas.SetLeft (ip, (cnvsDesigner.ActualWidth / 2) + (ist.Width / 2));
            Canvas.SetTop (ip, (cnvsDesigner.ActualHeight / 2) - (ip.Height / 2));

            // Select the added step, give a default name by its index
            selectedElement = ist;
            selectedStep = Steps.FindIndex (o => { return o == selectedElement; });
            ist.SetName (selectedStep.ToString ("000"));

            // Refresh the Properties View with the newly selected step
            setPropertiesView ();
        }

        private void addProgression (ItemProgression pfrom, ItemStep sto) {
            throw new NotImplementedException ();
        }

        private void ButtonAddStep_Click (object sender, RoutedEventArgs e)
            => addStep ();

        private void ButtonDuplicateStep_Click (object sender, RoutedEventArgs e) {
            if (selectedElement == null)
                return;

            if (selectedElement is ItemStep) {
                ItemStep orig = (ItemStep)selectedElement;
                ItemStep dupe = orig.Duplicate ();
                addStep (dupe, dupe.Progression);
            }
        }

        private void ButtonEditProgressions_Click (object sender, RoutedEventArgs e) {
            throw new NotImplementedException ();
        }

        private void MenuItemExit_Click (object sender, RoutedEventArgs e)
            => Application.Current.Shutdown ();

        private void BtnCopyPatient_Click (object sender, RoutedEventArgs e) {
            if (selectedElement == null)
                return;

            if (selectedElement is ItemStep) {
                copiedPatient = new Patient ();
                copiedPatient.Load_Process (((ItemStep)selectedElement).Patient.Save ());
            }
        }

        private void BtnPastePatient_Click (object sender, RoutedEventArgs e) {
            if (selectedElement == null)
                return;

            if (selectedElement is ItemStep && copiedPatient != null) {
                ((ItemStep)selectedElement).Step.Patient.Load_Process (copiedPatient.Save ());
            }

            updatePropertiesView ();
        }

        private void MenuItemAbout_Click (object sender, RoutedEventArgs e) {
            About dlgAbout = new About ();
            dlgAbout.ShowDialog ();
        }

        private void UIElementMouseLeftButtonDown (object sender, MouseButtonEventArgs e) {
            selectedElement = sender as UIElement;

            Mouse.Capture (selectedElement);
            mouseCaptured = true;

            if (selectedElement is ItemStep) {
                xShape = Canvas.GetLeft (selectedElement);
                yShape = Canvas.GetTop (selectedElement);
                xCanvas = e.GetPosition (LayoutRoot).X;
                yCanvas = e.GetPosition (LayoutRoot).Y;
            }

            updatePropertiesView ();
        }

        private void UIElementMouseLeftButtonUp (object sender, MouseButtonEventArgs e) {
            Mouse.Capture (null);
            mouseCaptured = false;

            if (selectedElement is ItemProgression) {
                if (Mouse.DirectlyOver is ItemProgression)
                    addProgression (selectedElement as ItemProgression, ((ItemProgression)Mouse.DirectlyOver).Step);
                else if (Mouse.DirectlyOver is ItemStep)
                    addProgression (selectedElement as ItemProgression, (ItemStep)Mouse.DirectlyOver);
            }
        }

        private void UIElementMouseMove (object sender, MouseEventArgs e) {
            if (mouseCaptured && selectedElement is ItemStep) {
                double x = e.GetPosition (LayoutRoot).X;
                double y = e.GetPosition (LayoutRoot).Y;
                xShape += x - xCanvas;
                xCanvas = x;
                yShape += y - yCanvas;
                yCanvas = y;
                Canvas.SetLeft (selectedElement, Utility.Clamp (xShape, 0, canvasDesigner.ActualWidth - (sender as Shape).Width));
                Canvas.SetTop (selectedElement, Utility.Clamp (yShape, 0, canvasDesigner.ActualHeight - (sender as Shape).Height));

                if (selectedElement is ItemStep) {
                    Canvas.SetLeft (((ItemStep)selectedElement).Label, Utility.Clamp (xShape, 0, canvasDesigner.ActualWidth - (sender as Shape).Width));
                    Canvas.SetTop (((ItemStep)selectedElement).Label, Utility.Clamp (yShape, 0, canvasDesigner.ActualHeight - (sender as Shape).Height));

                    Canvas.SetLeft (((ItemStep)selectedElement).Progression, Utility.Clamp (xShape + (sender as Shape).Width,
                        0, canvasDesigner.ActualWidth - (sender as Shape).Width));
                    Canvas.SetTop (((ItemStep)selectedElement).Progression, Utility.Clamp (yShape, 0, canvasDesigner.ActualHeight - (sender as Shape).Height));
                }
            }
        }
    }
}