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

        private List<StepItem> Steps = new List<StepItem> ();

        public Editor () {
            InitializeComponent ();

            canvasDesigner = cnvsDesigner;
        }

        private void addStep (StepItem si = null) {
            if (si == null)
                si = new StepItem ();

            si.Height = 50;
            si.Width = 50;
            si.Stroke = Brushes.Black;
            si.Fill = Brushes.LightGray;
            si.MouseLeftButtonDown += UIElementMouseLeftButtonDown;
            si.MouseLeftButtonUp += UIElementMouseLeftButtonUp;
            si.MouseMove += UIElementMouseMove;

            Steps.Add (si);
            canvasDesigner.Children.Add (si);
            canvasDesigner.Children.Add (si.Label);

            Canvas.SetLeft (si, (cnvsDesigner.ActualWidth / 2) - (si.Width / 2));
            Canvas.SetTop (si, (cnvsDesigner.ActualHeight / 2) - (si.Height / 2));
            Canvas.SetLeft (si.Label, (cnvsDesigner.ActualWidth / 2) - (si.Width / 2));
            Canvas.SetTop (si.Label, (cnvsDesigner.ActualHeight / 2) - (si.Height / 2));

            selectedElement = si;
            updatePropertiesView ();
        }

        private void updatePropertiesView () {
            gridProperties.Children.Clear ();
            gridProperties.RowDefinitions.Clear ();
            gridProperties.ColumnDefinitions.Clear ();

            if (selectedElement == null)
                return;

            if (selectedElement is StepItem) {
                selectedStep = Steps.FindIndex (o => { return o == selectedElement; });
                lblProperties.Content = String.Concat ("Edit Step: #", selectedStep.ToString ("000"));

                StepItem si = (StepItem)selectedElement;

                gridAddRows (gridProperties, 7);

                PropertyString ps;
                PropertyInt pi;
                PropertyDouble pd;
                PropertyBP pbp;

                ps = new PropertyString (0, PropertyString.Keys.Name, si.Step.Name ?? "");
                ps.PropertyChanged += updateProperty;
                gridProperties.Children.Add (ps);

                ps = new PropertyString (1, PropertyString.Keys.Description, si.Step.Description ?? "");
                ps.PropertyChanged += updateProperty;
                gridProperties.Children.Add (ps);

                pi = new PropertyInt (2, PropertyInt.Keys.HR, si.Patient.HR, 5, 0, 500);
                pi.PropertyChanged += updateProperty;
                gridProperties.Children.Add (pi);

                pbp = new PropertyBP (3, PropertyBP.Keys.NSBP,
                    si.Patient.NSBP, si.Patient.NDBP,
                    5, 0, 300,
                    5, 0, 200);
                pbp.PropertyChanged += updateProperty;
                gridProperties.Children.Add (pbp);

                pi = new PropertyInt (4, PropertyInt.Keys.RR, si.Patient.RR, 2, 0, 100);
                pi.PropertyChanged += updateProperty;
                gridProperties.Children.Add (pi);

                pi = new PropertyInt (5, PropertyInt.Keys.SPO2, si.Patient.SPO2, 2, 0, 100);
                pi.PropertyChanged += updateProperty;
                gridProperties.Children.Add (pi);

                pd = new PropertyDouble (6, PropertyDouble.Keys.T, si.Patient.T, 0.2, 0, 100);
                pd.PropertyChanged += updateProperty;
                gridProperties.Children.Add (pd);
            }
        }

        private void updateProperty (object sender, PropertyString.PropertyStringEventArgs e) {
            if (selectedElement is StepItem) {
                StepItem si = (StepItem)selectedElement;
                switch (e.Key) {
                    default: break;
                    case PropertyString.Keys.Name:
                        si.Step.Name = e.Value;
                        si.Label.Content = e.Value;
                        si.Width = Math.Max (50, si.Label.ActualWidth + 8);
                        break;

                    case PropertyString.Keys.Description: si.Step.Description = e.Value; break;
                }
            }
        }

        private void updateProperty (object sender, PropertyInt.PropertyIntEventArgs e) {
            if (selectedElement is StepItem) {
                StepItem si = (StepItem)selectedElement;
                switch (e.Key) {
                    default: break;
                    case PropertyInt.Keys.HR: si.Patient.HR = e.Value; break;
                    case PropertyInt.Keys.RR: si.Patient.RR = e.Value; break;
                    case PropertyInt.Keys.ETCO2: si.Patient.ETCO2 = e.Value; break;
                    case PropertyInt.Keys.SPO2: si.Patient.SPO2 = e.Value; break;
                    case PropertyInt.Keys.CVP: si.Patient.CVP = e.Value; break;
                    case PropertyInt.Keys.ICP: si.Patient.ICP = e.Value; break;
                    case PropertyInt.Keys.IAP: si.Patient.IAP = e.Value; break;
                }
            }
        }

        private void updateProperty (object sender, PropertyDouble.PropertyDoubleEventArgs e) {
            if (selectedElement is StepItem) {
                StepItem si = (StepItem)selectedElement;
                switch (e.Key) {
                    default: break;
                    case PropertyDouble.Keys.T: si.Patient.T = e.Value; break;
                }
            }
        }

        private void updateProperty (object sender, PropertyBP.PropertyIntEventArgs e) {
            if (selectedElement is StepItem) {
                StepItem si = (StepItem)selectedElement;
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

        private void gridAddRows (Grid grid, int rows) {
            for (int i = 0; i < rows; i++) {
                RowDefinition rd = new RowDefinition ();
                rd.Height = new GridLength (1, GridUnitType.Auto);
                grid.RowDefinitions.Add (rd);
            }
        }

        private void ButtonAddStep_Click (object sender, RoutedEventArgs e)
            => addStep ();

        private void ButtonDuplicateStep_Click (object sender, RoutedEventArgs e) {
            if (selectedElement == null)
                return;

            if (selectedElement is StepItem) {
                StepItem orig = (StepItem)selectedElement;
                addStep (orig.Duplicate ());
            }
        }

        private void ButtonAddProgression_Click (object sender, RoutedEventArgs e) {
            throw new NotImplementedException ();
        }

        private void MenuItemExit_Click (object sender, RoutedEventArgs e)
            => Application.Current.Shutdown ();

        private void BtnCopyPatient_Click (object sender, RoutedEventArgs e) {
            if (selectedElement == null)
                return;

            if (selectedElement is StepItem) {
                copiedPatient = ((StepItem)selectedElement).Patient;
            }
        }

        private void BtnPastePatient_Click (object sender, RoutedEventArgs e) {
            if (selectedElement == null)
                return;

            if (selectedElement is StepItem && copiedPatient != null) {
                ((StepItem)selectedElement).Step.Patient = copiedPatient;
            }
        }

        private void MenuItemAbout_Click (object sender, RoutedEventArgs e) {
            About dlgAbout = new About ();
            dlgAbout.ShowDialog ();
        }

        private void UIElementMouseLeftButtonDown (object sender, MouseButtonEventArgs e) {
            selectedElement = sender as UIElement;

            Mouse.Capture (selectedElement);
            mouseCaptured = true;

            xShape = Canvas.GetLeft (selectedElement);
            yShape = Canvas.GetTop (selectedElement);
            xCanvas = e.GetPosition (LayoutRoot).X;
            yCanvas = e.GetPosition (LayoutRoot).Y;

            updatePropertiesView ();
        }

        private void UIElementMouseLeftButtonUp (object sender, MouseButtonEventArgs e) {
            Mouse.Capture (null);
            mouseCaptured = false;
        }

        private void UIElementMouseMove (object sender, MouseEventArgs e) {
            if (mouseCaptured) {
                double x = e.GetPosition (LayoutRoot).X;
                double y = e.GetPosition (LayoutRoot).Y;
                xShape += x - xCanvas;
                xCanvas = x;
                yShape += y - yCanvas;
                yCanvas = y;
                Canvas.SetLeft (selectedElement, Utility.Clamp (xShape, 0, canvasDesigner.ActualWidth - (sender as Shape).Width));
                Canvas.SetTop (selectedElement, Utility.Clamp (yShape, 0, canvasDesigner.ActualHeight - (sender as Shape).Height));

                if (selectedElement is StepItem) {
                    Canvas.SetLeft (((StepItem)selectedElement).Label, Utility.Clamp (xShape, 0, canvasDesigner.ActualWidth - (sender as Shape).Width));
                    Canvas.SetTop (((StepItem)selectedElement).Label, Utility.Clamp (yShape, 0, canvasDesigner.ActualHeight - (sender as Shape).Height));
                }
            }
        }
    }
}