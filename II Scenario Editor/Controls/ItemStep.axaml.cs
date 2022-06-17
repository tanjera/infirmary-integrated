using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;

namespace II_Scenario_Editor.Controls {

    public partial class ItemStep : UserControl {
        /* Interface items */
        public Label ILabelName;
        public Label ILabelNumber;
        public Border IStep;
        public Border IStepEnd;

        public List<Line> IProgressions = new ();

        /* Data structures */
        public string UUID;
        public II.Scenario.Step Step = new II.Scenario.Step ();

        /* Exposed properties */
        public int Index;

        public II.Patient Patient {
            get { return Step.Patient; }
            set { Step.Patient = value; }
        }

        public ItemStep () {
            InitializeComponent ();

            UUID = Guid.NewGuid ().ToString ();

            ILabelName = this.FindControl<Label> ("lblName");
            ILabelNumber = this.FindControl<Label> ("lblNumber");

            IStep = this.FindControl<Border> ("brdStep");
            IStepEnd = this.FindControl<Border> ("brdStepEnd");
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        public void SetNumber (int index) {
            Index = index;
            ILabelNumber.Content = Index.ToString ();
        }

        public void SetName (string name) {
            Step.Name = name;
            ILabelName.Content = name;
        }
    }
}