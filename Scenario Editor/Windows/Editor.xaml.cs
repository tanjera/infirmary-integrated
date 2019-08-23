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

        private Canvas DesignerCanvas;
        private UIElement selectedElement = null;

        private int siIndex = -1,
            selectedProgression = -1;

        private List<StageItem> Stages = new List<StageItem> ();

        public Editor () {
            InitializeComponent ();

            DesignerCanvas = cnvsDesigner;
        }

        private void addStage (StageItem si = null) {
            if (si == null)
                si = new StageItem ();

            si.Height = 25;
            si.Width = 50;
            si.Stroke = Brushes.Black;
            si.Fill = Brushes.LightGray;
            si.MouseLeftButtonDown += elementMouseLeftButtonDown;
            si.MouseLeftButtonUp += elementMouseLeftButtonUp;
            si.MouseMove += elementMouseMove;

            Stages.Add (si);
            DesignerCanvas.Children.Add (si);
            DesignerCanvas.Children.Add (si.Label);

            Canvas.SetLeft (si, (cnvsDesigner.ActualWidth / 2) - (si.Width / 2));
            Canvas.SetTop (si, (cnvsDesigner.ActualHeight / 2) - (si.Height / 2));
            Canvas.SetLeft (si.Label, (cnvsDesigner.ActualWidth / 2) - (si.Width / 2));
            Canvas.SetTop (si.Label, (cnvsDesigner.ActualHeight / 2) - (si.Height / 2));

            selectedElement = si;
            updatePropertiesView ();
        }

        private void btnAddStage_Click (object sender, RoutedEventArgs e)
            => addStage ();

        private void btnDuplicateStage_Click (object sender, RoutedEventArgs e) {
            if (selectedElement == null)
                return;

            if (selectedElement is StageItem) {
                StageItem orig = (StageItem)selectedElement;
                addStage (orig.Duplicate ());
            }
        }

        private void btnAddProgression_Click (object sender, RoutedEventArgs e) {
            throw new NotImplementedException ();
        }

        private void menuExit_Click (object sender, RoutedEventArgs e)
            => Application.Current.Shutdown ();

        private void menuAbout_Click (object sender, RoutedEventArgs e) {
            About dlgAbout = new About ();
            dlgAbout.ShowDialog ();
        }

        private void elementMouseLeftButtonDown (object sender, MouseButtonEventArgs e) {
            selectedElement = sender as UIElement;

            Mouse.Capture (selectedElement);
            mouseCaptured = true;

            xShape = Canvas.GetLeft (selectedElement);
            yShape = Canvas.GetTop (selectedElement);
            xCanvas = e.GetPosition (LayoutRoot).X;
            yCanvas = e.GetPosition (LayoutRoot).Y;

            updatePropertiesView ();
        }

        private void elementMouseLeftButtonUp (object sender, MouseButtonEventArgs e) {
            Mouse.Capture (null);
            mouseCaptured = false;
        }

        private void elementMouseMove (object sender, MouseEventArgs e) {
            if (mouseCaptured) {
                double x = e.GetPosition (LayoutRoot).X;
                double y = e.GetPosition (LayoutRoot).Y;
                xShape += x - xCanvas;
                xCanvas = x;
                yShape += y - yCanvas;
                yCanvas = y;
                Canvas.SetLeft (selectedElement, Utility.Clamp (xShape, 0, DesignerCanvas.ActualWidth - (sender as Shape).Width));
                Canvas.SetTop (selectedElement, Utility.Clamp (yShape, 0, DesignerCanvas.ActualHeight - (sender as Shape).Height));

                if (selectedElement is StageItem) {
                    Canvas.SetLeft (((StageItem)selectedElement).Label, Utility.Clamp (xShape, 0, DesignerCanvas.ActualWidth - (sender as Shape).Width));
                    Canvas.SetTop (((StageItem)selectedElement).Label, Utility.Clamp (yShape, 0, DesignerCanvas.ActualHeight - (sender as Shape).Height));
                }
            }
        }

        private void updatePropertiesView () {
            gridProperties.Children.Clear ();
            gridProperties.RowDefinitions.Clear ();
            gridProperties.ColumnDefinitions.Clear ();

            if (selectedElement == null)
                return;

            if (selectedElement is StageItem) {
                siIndex = Stages.FindIndex (o => { return o == selectedElement; });
                lblProperties.Content = String.Concat ("Edit Stage: #", siIndex.ToString ("000"));

                StageItem si = (StageItem)selectedElement;

                gridAddRows (gridProperties, 7);

                PropertyString ps;
                PropertyInt pi;
                PropertyDouble pd;
                PropertyBP pbp;

                ps = new PropertyString (0, PropertyString.Keys.Name, si.Stage.Name ?? "");
                ps.PropertyChanged += updateProperty;
                gridProperties.Children.Add (ps);

                ps = new PropertyString (1, PropertyString.Keys.Description, si.Stage.Description ?? "");
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
            if (selectedElement is StageItem) {
                StageItem si = (StageItem)selectedElement;
                switch (e.Key) {
                    default: break;
                    case PropertyString.Keys.Name:
                        si.Stage.Name = e.Value;
                        si.Label.Content = e.Value;
                        si.Width = Math.Max (50, si.Label.ActualWidth + 8);
                        break;

                    case PropertyString.Keys.Description: si.Stage.Description = e.Value; break;
                }
            }
        }

        private void updateProperty (object sender, PropertyInt.PropertyIntEventArgs e) {
            if (selectedElement is StageItem) {
                StageItem si = (StageItem)selectedElement;
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
            if (selectedElement is StageItem) {
                StageItem si = (StageItem)selectedElement;
                switch (e.Key) {
                    default: break;
                    case PropertyDouble.Keys.T: si.Patient.T = e.Value; break;
                }
            }
        }

        private void updateProperty (object sender, PropertyBP.PropertyIntEventArgs e) {
            if (selectedElement is StageItem) {
                StageItem si = (StageItem)selectedElement;
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
    }
}