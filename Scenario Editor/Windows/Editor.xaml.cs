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

namespace ScenarioEditor {

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Editor : Window {

        // Variables for capturing mouse and dragging UI elements
        private bool mouseCaptured = false;

        private double xShape, yShape,
            xCanvas, yCanvas;

        private Canvas DesignerCanvas;
        private UIElement selectedElement = null;

        private int selectedStage = -1,
            selectedProgression = -1;

        private List<StageElement> Stages = new List<StageElement> ();

        public class StageElement {
            public Shape Shape;
            public Scenario.Stage Stage;

            public StageElement (Shape incShape) {
                Shape = incShape;
                Stage = new Scenario.Stage ();
            }
        }

        public Editor () {
            InitializeComponent ();

            DesignerCanvas = cnvsDesigner;
        }

        private void btnAddStage_Click (object sender, RoutedEventArgs e) {
            Rectangle rect = new Rectangle ();
            rect.Height = 50;
            rect.Width = 50;
            rect.Stroke = Brushes.Black;
            rect.Fill = Brushes.LightGray;
            rect.MouseLeftButtonDown += elementMouseLeftButtonDown;
            rect.MouseLeftButtonUp += elementMouseLeftButtonUp;
            rect.MouseMove += elementMouseMove;

            Stages.Add (new StageElement (rect));
            DesignerCanvas.Children.Add (rect);

            Canvas.SetLeft (rect, (cnvsDesigner.ActualWidth / 2) - (rect.Width / 2));
            Canvas.SetTop (rect, (cnvsDesigner.ActualHeight / 2) - (rect.Height / 2));
        }

        private void btnAddProgression_Click (object sender, RoutedEventArgs e) {
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
                Canvas.SetLeft (selectedElement, Utility.Clamp (xShape, 0, DesignerCanvas.ActualWidth - (sender as Shape).Width));
                xCanvas = x;
                yShape += y - yCanvas;
                Canvas.SetTop (selectedElement, Utility.Clamp (yShape, 0, DesignerCanvas.ActualHeight - (sender as Shape).Height));
                yCanvas = y;
            }
        }

        private void updatePropertiesView () {
            gridProperties.Children.Clear ();
            gridProperties.RowDefinitions.Clear ();
            gridProperties.ColumnDefinitions.Clear ();

            if (selectedElement == null)
                return;

            if (selectedElement is Rectangle) {
                selectedStage = Stages.FindIndex (o => { return o.Shape == selectedElement; });
                lblProperties.Content = String.Concat ("Edit Stage: #", selectedStage.ToString ("000"));

                if (selectedStage >= 0) {
                    StageElement se = Stages [selectedStage];
                    gridAddRows (gridProperties, 2);
                    gridProperties.Children.Add (new PropertyItem (0, "Name:", se.Stage.Name ?? ""));
                    gridProperties.Children.Add (new PropertyItem (1, "Description:", se.Stage.Description ?? ""));
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