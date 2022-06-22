using System;
using System.Collections.Generic;
using System.Diagnostics;
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

using II;
using II_Scenario_Editor.Controls;

namespace II_Scenario_Editor.Windows {

    public partial class PanelOverview : UserControl {
        /* Pointer to main data structure for the scenario, patient, devices, etc. */
        private Scenario Scenario;
        private WindowMain IMain;

        /* Interface items */
        public Canvas ICanvas;
        public ItemStep? ISelectedStep;
        public bool IsSelectedStepEnd = false;
        public List<ItemStep> ISteps = new List<ItemStep> ();
        public List<ItemStep.Progression> IProgressions = new List<ItemStep.Progression> ();

        // For copy/pasting Patient parameters
        private Patient CopiedPatient;

        // Variables for capturing mouse and dragging UI elements
        private Point? PointerPosition = null;

        private Avalonia.Input.Pointer? PointerCaptured = null;

        public PanelOverview () {
            InitializeComponent ();

            DataContext = this;

            ICanvas = this.FindControl<Canvas> ("cnvsDesigner");
            ICanvas.PointerPressed += Item_PointerPressed;

            _ = InitViewModel ();

            _ = DrawISteps ();
            _ = UpdateIProgressions ();
            _ = DrawIProgressions ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        public async Task InitReferences (WindowMain main) {
            IMain = main;
        }

        private async Task InitInterface () {
            // Reset buffer parameters
            ISelectedStep = null;
            IsSelectedStepEnd = false;
            PointerPosition = null;

            // Clear master lists and UI elements
            ICanvas.Children.Clear ();
            ISteps.Clear ();
        }

        public async Task SetScenario (Scenario s) {
            Scenario = s;

            await InitInterface ();
            await UpdateStepViewModel ();
        }

        public async Task InitScenario (Scenario s) {
            Scenario = s;

            await InitInterface ();

            // Clear scenario data
            Scenario.Author = "";
            Scenario.Description = "";
            Scenario.Name = "";

            await UpdateStepViewModel ();
        }

        private async Task InitViewModel () {
            PropertyString pstrScenarioAuthor = this.FindControl<PropertyString> ("pstrScenarioAuthor");
            PropertyString pstrScenarioName = this.FindControl<PropertyString> ("pstrScenarioName");
            PropertyString pstrScenarioDescription = this.FindControl<PropertyString> ("pstrScenarioDescription");
            PropertyString pstrProgressFrom = this.FindControl<PropertyString> ("pstrProgressFrom");
            PropertyString pstrProgressTo = this.FindControl<PropertyString> ("pstrProgressTo");
            PropertyString pstrStepName = this.FindControl<PropertyString> ("pstrStepName");
            PropertyString pstrStepDescription = this.FindControl<PropertyString> ("pstrStepDescription");
            PropertyInt pintProgressTimer = this.FindControl<PropertyInt> ("pintProgressTimer");

            // Initiate controls for editing Scenario properties
            pstrScenarioAuthor.Init (PropertyString.Keys.ScenarioAuthor);
            pstrScenarioName.Init (PropertyString.Keys.ScenarioName);
            pstrScenarioDescription.Init (PropertyString.Keys.ScenarioDescription);
            pstrProgressFrom.Init (PropertyString.Keys.DefaultSource);
            pstrProgressTo.Init (PropertyString.Keys.DefaultProgression);
            pstrStepName.Init (PropertyString.Keys.StepName);
            pstrStepDescription.Init (PropertyString.Keys.StepDescription);
            pintProgressTimer.Init (PropertyInt.Keys.ProgressTimer, 1, -1, 1000);

            pstrScenarioAuthor.PropertyChanged += UpdateScenario;
            pstrScenarioName.PropertyChanged += UpdateScenario;
            pstrScenarioDescription.PropertyChanged += UpdateScenario;
            pstrProgressFrom.PropertyChanged += UpdateScenario;
            pstrProgressTo.PropertyChanged += UpdateScenario;
            pstrStepName.PropertyChanged += UpdateScenario;
            pstrStepDescription.PropertyChanged += UpdateScenario;
            pintProgressTimer.PropertyChanged += UpdateScenario;
        }

        private void UpdateScenario (object? sender, PropertyString.PropertyStringEventArgs e) {
            switch (e.Key) {
                default: break;
                case PropertyString.Keys.ScenarioAuthor: Scenario.Author = e.Value ?? ""; break;
                case PropertyString.Keys.ScenarioName: Scenario.Name = e.Value ?? ""; break;
                case PropertyString.Keys.ScenarioDescription: Scenario.Description = e.Value ?? ""; break;
            }

            if (ISelectedStep != null) {
                switch (e.Key) {
                    default: break;
                    case PropertyString.Keys.DefaultSource: ISelectedStep.Step.DefaultSource = e.Value; break;
                    case PropertyString.Keys.DefaultProgression: throw new NotImplementedException (); break;
                    case PropertyString.Keys.StepName: ISelectedStep.SetName (e.Value ?? ""); break;
                    case PropertyString.Keys.StepDescription: ISelectedStep.Step.Description = e.Value ?? ""; break;
                }
            }
        }

        private void UpdateScenario (object? sender, PropertyInt.PropertyIntEventArgs e) {
            if (ISelectedStep != null) {
                switch (e.Key) {
                    default: break;
                    case PropertyInt.Keys.ProgressTimer: ISelectedStep.Step.ProgressTimer = e.Value; break;
                }
            }
        }

        private void UpdateScenario (object? sender, PropertyOptProgression.PropertyOptProgressionEventArgs e) {
            if (e.Index >= ISelectedStep?.Step.Progressions.Count)
                return;

            Scenario.Step.Progression p = ISelectedStep.Step.Progressions [e.Index];
            p.ToStepUUID = e.StepToUUID;
            p.Description = e.Description ?? "";

            // Deletes an optional progression via this route
            if (e.ToDelete) {
                ISelectedStep.Step.Progressions.RemoveAt (e.Index);
                _ = UpdateProgressionViewModel ();

                _ = UpdateIProgressions ();
                _ = DrawIProgressions ();
            }
        }

        private async Task UpdateProgressionViewModel () {
            StackPanel stackProgressions = this.FindControl<StackPanel> ("stackProgressions");
            stackProgressions.Children.Clear ();

            if (ISelectedStep != null) {
                for (int i = 0; i < ISelectedStep.Step.Progressions.Count; i++) {
                    Scenario.Step.Progression p = ISelectedStep.Step.Progressions [i];
                    PropertyOptProgression pp = new PropertyOptProgression ();
                    pp.Init (i, p.ToStepUUID, p.Description);
                    pp.PropertyChanged += UpdateScenario;
                    stackProgressions.Children.Add (pp);
                }
            }
        }

        private async Task UpdateScenarioViewModel () {
            this.FindControl<PropertyString> ("pstrScenarioAuthor").Set (Scenario.Author ?? "");
            this.FindControl<PropertyString> ("pstrScenarioName").Set (Scenario.Name ?? "");
            this.FindControl<PropertyString> ("pstrScenarioDescription").Set (Scenario.Description ?? "");
        }

        private async Task UpdateStepViewModel () {
            if (ISelectedStep == null)
                return;

            PropertyInt pintProgressTimer = this.FindControl<PropertyInt> ("pintProgressTimer");

            PropertyString pstrProgressFrom = this.FindControl<PropertyString> ("pstrProgressFrom");
            PropertyString pstrProgressTo = this.FindControl<PropertyString> ("pstrProgressTo");
            PropertyString pstrStepName = this.FindControl<PropertyString> ("pstrStepName");
            PropertyString pstrStepDescription = this.FindControl<PropertyString> ("pstrStepDescription");

            pintProgressTimer.Set (ISelectedStep.Step.ProgressTimer);

            pstrProgressFrom.Set (ISelectedStep?.Step?.DefaultSource ?? "");
            pstrProgressTo.Set (ISelectedStep?.Step?.DefaultProgression?.UUID ?? "");
            pstrStepName.Set (ISelectedStep?.Step?.Name ?? "");
            pstrStepDescription.Set (ISelectedStep?.Step?.Description ?? "");

            await UpdateProgressionViewModel ();
            await UpdateScenarioViewModel ();
        }

        private async Task AddStep (ItemStep? incItem = null) {
            ItemStep item = new ();

            if (incItem != null) {  // Duplicate
                // Copy interface properties and interface item properties
                item.SetName (incItem.Label);

                // Copy data structures
                item.Step.Name = incItem.Step.Name;
                item.Step.Description = incItem.Step.Description;

                await item.Step.Patient.Load_Process (incItem.Patient.Save ());
            }

            // Add to lists and display elements
            ISteps.Add (item);
            ICanvas.Children.Add (item);
            item.ZIndex = 1;

            item.PointerPressed += Item_PointerPressed;
            item.PointerReleased += Item_PointerReleased;
            item.PointerMoved += Item_PointerMoved;
            item.IStepEnd.PointerPressed += Item_PointerPressed;

            /* Set ItemStep.Index to currently highest Index + 1; is not actually used for indexing
             * but for end-user experience and organization */
            item.SetNumber (ISteps.Max (i => i.Index) + 1);

            // Refresh the Properties View and draw Progression elements/colors
            await UpdateStepViewModel ();
            await DrawISteps ();
            await DrawIProgressions ();
        }

        private async Task DeleteStep (ItemStep item) {
            // Remove the selected Step from the stack and visual
            ISteps.Remove (item);
            ICanvas.Children.Remove (item);

            foreach (ItemStep.Progression line in item.Progressions) {
                IProgressions.Remove (line);
                ICanvas.Children.Remove (line);
            }

            foreach (ItemStep s in ISteps) {
                // Nullify any default progression pointers that target the Step being deleted
                if (s.Step?.DefaultProgression?.UUID == item.UUID)
                    s.Step.DefaultProgression = null;

                // Remove all progressions targeting the Step being removed
                for (int i = s.Step.Progressions.Count - 1; i >= 0; i--) {
                    if (s.Step.Progressions [i].ToStepUUID == item.UUID)
                        s.Step.Progressions.RemoveAt (i);
                }
            }

            await UpdateIProgressions ();
            await DrawIProgressions ();
        }

        private async Task AddProgression (ItemStep itemFrom, ItemStep itemTo) {
            if (itemFrom == itemTo)
                return;

            /* Create the data structure and its interface model structure */
            Scenario.Step.Progression p = new Scenario.Step.Progression (Guid.NewGuid ().ToString (), itemTo.UUID);
            ItemStep.Progression ip = new ItemStep.Progression (itemFrom, itemTo, itemFrom.IStepEnd, p);

            itemFrom.Step.Progressions.Add (p);

            /* Link the steps with the progression; add default progression if this is the 1st progression added */
            if (String.IsNullOrEmpty (itemTo.Step.DefaultSource))
                itemTo.Step.DefaultSource = itemFrom.UUID;

            if (itemFrom.Step.DefaultProgression == null)
                itemFrom.Step.DefaultProgression = p;

            await UpdateIProgressions ();
            await DrawIProgressions ();
            await UpdateStepViewModel ();

            Debug.WriteLine ($"Progression created from {itemFrom.UUID} -> {itemTo.UUID}");
        }

        private async Task DrawISteps () {
            foreach (ItemStep item in ISteps) {
                await item.SetStep_Border (item == ISelectedStep);
                await item.SetEndStep_Border (IsSelectedStepEnd && item == ISelectedStep);

                // Color Step's Background depending
                if (Scenario.BeginStep == item.UUID)
                    await item.SetStep_Fill (ItemStep.Fill_FirstStep);
                else {
                    bool isLinked = false;
                    foreach (Scenario.Step s in Scenario.Steps) {
                        foreach (Scenario.Step.Progression p in s.Progressions) {
                            if (p.ToStepUUID == item.UUID) {
                                isLinked = true;
                            }
                        }
                    }

                    if (isLinked)
                        await item.SetStep_Fill (ItemStep.Fill_Linked);
                    else
                        await item.SetStep_Fill (ItemStep.Fill_Unlinked);
                }

                // Color StepEnd's Background depending on whether it has progressions
                if (item.Step.Progressions.Count == 0)
                    await item.SetEndStep_Fill (ItemStepEnd.Fill_NoProgressions);
                else if (item.Step.Progressions.Count >= 1) {
                    if (item.Step.DefaultProgression == null)
                        await item.SetEndStep_Fill (ItemStepEnd.Fill_NoDefaultProgression);
                    else {
                        if (item.Step.Progressions.Count == 1)
                            await item.SetEndStep_Fill (ItemStepEnd.Fill_NoOptionalProgression);
                        else if (item.Step.Progressions.Count > 1)
                            await item.SetEndStep_Fill (ItemStepEnd.Fill_MultipleProgressions);
                    }
                } else {
                    await item.SetEndStep_Fill (ItemStepEnd.Fill_Default);
                }
            }
        }

        private async Task DrawIProgressions () {
            /* Recalculate the positions of Progression Lines on the Canvas
             * To be called when visual elements have moved on the Canvas
             */

            foreach (ItemStep.Progression ip in IProgressions) {
                ip.StartPoint = new Point (ip.Step.Bounds.Left + ip.StepEnd.Bounds.Left + ip.StepEnd.Width, ip.Step.Bounds.Center.Y);
                ip.EndPoint = new Point (ip.StepTo.Bounds.Left, ip.StepTo.Bounds.Center.Y);
                ip.InvalidateVisual ();
            }
        }

        private async Task UpdateIProgressions () {
            /* Re-iterate all Progressions and re-populate the list of Lines accordingly
             * To be called when the list of Progressions has changed
             */

            foreach (ItemStep.Progression ip in IProgressions)
                ICanvas.Children.Remove (ip);

            IProgressions.Clear ();

            foreach (ItemStep istep in ISteps) {
                istep.Progressions.Clear ();

                foreach (Scenario.Step.Progression prog in istep.Step.Progressions) {
                    ItemStep? itemTo = ISteps.Find (s => s.UUID == prog.ToStepUUID);

                    if (itemTo == null)
                        throw new IndexOutOfRangeException ();

                    ItemStep.Progression p = new (istep, itemTo, istep.IStepEnd, prog);
                    istep.Progressions.Add (p);
                    IProgressions.Add (p);
                    ICanvas.Children.Add (p);
                }
            }
        }

        private async Task UpdateStepIndices () {
            // Set all Steps' indices for their Labels
            // Start at 1! This is an end-user convenience; these aren't used for data indexing, just aesthetics
            for (int i = 1; i < ISteps.Count; i++)
                ISteps [i].SetNumber (i);
        }

        private async Task SelectIStep (ItemStep item, Avalonia.Input.Pointer? capture = null) {
            ISelectedStep = item;
            IsSelectedStepEnd = false;

            if (capture != null) {
                PointerCaptured = capture;
                PointerCaptured.Capture (item);
            }
        }

        private async Task SelectEndStep (ItemStepEnd end, Avalonia.Input.Pointer? capture = null) {
            ISelectedStep = end.Step;
            IsSelectedStepEnd = true;

            if (capture != null) {
                PointerCaptured = capture;
                PointerCaptured.Capture (end.Step);
            }
        }

        private async Task DeselectAll () {
            ISelectedStep = null;
            IsSelectedStepEnd = false;

            PointerPosition = null;

            if (PointerCaptured != null) {
                PointerCaptured.Capture (null);
                PointerCaptured = null;
            }
        }

        private void Action_AddStep ()
            => _ = AddStep ();

        private void Action_DuplicateStep () {
            if (ISelectedStep == null)
                return;

            _ = AddStep (ISelectedStep);
        }

        private void Action_DeleteStep () {
            if (ISelectedStep != null)
                _ = DeleteStep (ISelectedStep);
        }

        private async Task Action_CopyPatient () {
            if (ISelectedStep == null)
                return;

            CopiedPatient = new Patient ();
            await CopiedPatient.Load_Process (ISelectedStep.Patient.Save ());
        }

        private async Task Action_PastePatient () {
            if (ISelectedStep == null)
                return;

            if (CopiedPatient != null) {
                await ISelectedStep.Patient.Load_Process (CopiedPatient.Save ());
            }

            await UpdateProgressionViewModel ();
        }

        /* Generic Menu Items (across all Panels) */

        private void MenuFileNew_Click (object sender, RoutedEventArgs e)
            => IMain.MenuFileNew_Click (sender, e);

        private void MenuFileLoad_Click (object sender, RoutedEventArgs e)
            => IMain.MenuFileLoad_Click (sender, e);

        private void MenuFileSave_Click (object sender, RoutedEventArgs e)
            => IMain.MenuFileSave_Click (sender, e);

        private void MenuFileExit_Click (object sender, RoutedEventArgs e)
            => IMain.MenuFileExit_Click (sender, e);

        private void MenuHelpAbout_Click (object sender, RoutedEventArgs e)
            => IMain.MenuHelpAbout_Click (sender, e);

        /* Menu Items specific to this Panel */

        private void MenuEditAddStep_Click (object sender, RoutedEventArgs e)
            => Action_AddStep ();

        private void MenuEditDuplicateStep_Click (object sender, RoutedEventArgs e)
            => Action_DuplicateStep ();

        private void MenuEditDeleteStep_Click (object sender, RoutedEventArgs e)
            => Action_DeleteStep ();

        private void MenuEditCopyPatient_Click (object sender, RoutedEventArgs e)
            => _ = Action_CopyPatient ();

        private void MenuEditPastePatient_Click (object sender, RoutedEventArgs e)
            => _ = Action_PastePatient ();

        private void MenuEditIndexSteps_Click (object sender, RoutedEventArgs e)
            => _ = UpdateStepIndices ();

        /* Any other Routed events for this Panel */

        private void ButtonAddStep_Click (object sender, RoutedEventArgs e)
            => Action_AddStep ();

        private void ButtonDuplicateStep_Click (object sender, RoutedEventArgs e)
            => Action_DuplicateStep ();

        private void BtnDeleteStep_Click (object sender, RoutedEventArgs e)
            => Action_DeleteStep ();

        private void BtnCopyPatient_Click (object sender, RoutedEventArgs e)
            => _ = Action_CopyPatient ();

        private void BtnPastePatient_Click (object sender, RoutedEventArgs e)
            => _ = Action_PastePatient ();

        private void BtnDeleteDefaultProgression_Click (object sender, RoutedEventArgs e) {
            throw new NotImplementedException ();
        }

        private void Item_PointerPressed (object? sender, PointerPressedEventArgs e) {
            if (sender is ItemStep item) {
                e.Handled = true;

                if (ISelectedStep == null || !IsSelectedStepEnd) {
                    _ = SelectIStep (item);
                } else if (ISelectedStep != null && IsSelectedStepEnd) {
                    if (ISelectedStep == item) {
                        _ = SelectIStep (item);
                    } else {
                        _ = AddProgression (ISelectedStep, item);
                        _ = DeselectAll ();
                    }
                }
            } else if (sender is ItemStepEnd end) {
                e.Handled = true;

                _ = SelectEndStep (end);
            } else if (sender is Canvas cnv) {
                e.Handled = true;

                _ = DeselectAll ();
            } else {
                return;
            }

            PointerPosition = e.GetPosition (null);

            _ = DrawISteps ();
            _ = DrawIProgressions ();
        }

        private void Item_PointerReleased (object? sender, PointerReleasedEventArgs e) {
            e.Handled = true;

            e.Pointer.Capture (null);
            PointerPosition = null;

            _ = DrawISteps ();
            _ = DrawIProgressions ();
        }

        private void Item_PointerMoved (object? sender, PointerEventArgs e) {
            if (PointerPosition != null && sender != null && sender is ItemStep item && ISelectedStep == item) {
                e.Handled = true;

                Point p = e.GetPosition (null);
                Point deltaPosition = p - (Point)PointerPosition;
                PointerPosition = p;

                double left = ISelectedStep.Bounds.Left + deltaPosition.X;
                double top = ISelectedStep.Bounds.Top + deltaPosition.Y;

                Canvas.SetLeft (ISelectedStep, left);
                Canvas.SetTop (ISelectedStep, top);

                _ = DrawIProgressions ();
            }
        }
    }
}