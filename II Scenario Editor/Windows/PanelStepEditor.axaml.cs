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
using IISE.Controls;

namespace IISE.Windows {

    public partial class PanelStepEditor : UserControl {
        /* Pointer to main data structure for the scenario, patient, devices, etc. */
        private Scenario? Scenario;
        private WindowMain? IMain;

        /* Interface items */
        public Canvas ICanvas;
        public ItemStep? ISelectedStep;
        public bool IsSelectedStepEnd = false;
        public List<ItemStep> ISteps = new List<ItemStep> ();
        public List<ItemStep.Progression> IProgressions = new List<ItemStep.Progression> ();

        // For copy/pasting Patient parameters
        private Scenario.Step? CopiedStep;

        // Variables for capturing mouse and dragging UI elements
        private Point? PointerPosition = null;

        private Avalonia.Input.Pointer? PointerCaptured = null;

        public PanelStepEditor () {
            InitializeComponent ();

            DataContext = this;

            ICanvas = this.FindControl<Canvas> ("cnvsDesigner");
            ICanvas.PointerPressed += Item_PointerPressed;

            _ = InitView ();

            _ = DrawISteps ();
            _ = UpdateIProgressions ();
            _ = DrawIProgressions ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        public Task InitReferences (WindowMain main) {
            IMain = main;

            return Task.CompletedTask;
        }

        private async Task InitInterface () {
            // Reset buffer parameters
            ISelectedStep = null;
            IsSelectedStepEnd = false;
            await (IMain?.SetStep (null) ?? Task.CompletedTask);

            PointerPosition = null;

            // Clear master lists and UI elements
            ICanvas.Children.Clear ();
            ISteps.Clear ();
        }

        public async Task SetScenario (Scenario s) {
            Scenario = s;

            await InitInterface ();
            await ImportSteps ();

            // Refresh the Properties View and draw Progression elements/colors
            await UpdateViewModel ();
            await DrawISteps ();
            await UpdateIProgressions ();
            await DrawIProgressions ();
        }

        private async Task InitView () {
            PropertyCombo pcmbProgressFrom = this.FindControl<PropertyCombo> ("pcmbProgressFrom");
            PropertyCombo pcmbProgressTo = this.FindControl<PropertyCombo> ("pcmbProgressTo");
            PropertyString pstrStepName = this.FindControl<PropertyString> ("pstrStepName");
            PropertyString pstrStepDescription = this.FindControl<PropertyString> ("pstrStepDescription");
            PropertyInt pintProgressTimer = this.FindControl<PropertyInt> ("pintProgressTimer");

            // Initiate controls for editing Scenario properties

            await pcmbProgressFrom.Init (PropertyCombo.Keys.DefaultSource, Array.Empty<string> (), new List<string> ());
            await pcmbProgressTo.Init (PropertyCombo.Keys.DefaultProgression, Array.Empty<string> (), new List<string> ());
            await pstrStepName.Init (PropertyString.Keys.StepName);
            await pstrStepDescription.Init (PropertyString.Keys.StepDescription);
            await pintProgressTimer.Init (PropertyInt.Keys.ProgressTimer, 1, -1, 1000);

            pcmbProgressFrom.PropertyChanged += UpdateScenario;
            pcmbProgressTo.PropertyChanged += UpdateScenario;
            pstrStepName.PropertyChanged += UpdateScenario;
            pstrStepDescription.PropertyChanged += UpdateScenario;
            pintProgressTimer.PropertyChanged += UpdateScenario;
        }

        private void UpdateScenario (object? sender, PropertyCombo.PropertyComboEventArgs e) {
            if (ISelectedStep != null) {
                switch (e.Key) {
                    default: break;
                    case PropertyCombo.Keys.DefaultSource: ISelectedStep.Step.DefaultSource = e.Value; break;

                    case PropertyCombo.Keys.DefaultProgression:
                        ISelectedStep.Step.DefaultProgression = ISelectedStep.Step.Progressions.Find (p => p.UUID == e.Value);
                        break;
                }

                // Propogate changes to the Step to other panels as needed
                _ = IMain?.SetStep (ISelectedStep.Step);
            }
        }

        private void UpdateScenario (object? sender, PropertyString.PropertyStringEventArgs e) {
            if (ISelectedStep != null) {
                switch (e.Key) {
                    default: break;
                    case PropertyString.Keys.StepName: ISelectedStep.Name = e.Value ?? ""; break;
                    case PropertyString.Keys.StepDescription: ISelectedStep.Description = e.Value ?? ""; break;
                }

                // Propogate changes to the Step to other panels as needed
                _ = IMain?.SetStep (ISelectedStep.Step);
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

        private void UpdateScenario (object? sender, PropertyProgression.PropertyProgressionEventArgs e) {
            if (ISelectedStep == null)
                return;

            Scenario.Step.Progression? prog = ISelectedStep?.Step.Progressions.Find (p => p.UUID == e.UUID);

            if (prog == null)
                return;

            prog.DestinationUUID = e.StepToUUID;
            prog.Description = e.Description ?? "";

            // Deletes a progression via this route
            if (e.ToDelete) {
                ISelectedStep?.Step.Progressions.Remove (prog);
                _ = UpdateProgressionViewModel ();

                _ = UpdateIProgressions ();
                _ = DrawIProgressions ();
            }
        }

        private async Task UpdateViewModel () {
            await UpdateStepViewModel ();
            await UpdateProgressionViewModel ();
        }

        private Task UpdateProgressionViewModel () {
            StackPanel stackProgressions = this.FindControl<StackPanel> ("stackProgressions");
            stackProgressions.Children.Clear ();

            if (ISelectedStep != null) {
                for (int i = 0; i < ISelectedStep.Step.Progressions.Count; i++) {
                    Scenario.Step.Progression prog = ISelectedStep.Step.Progressions [i];
                    Scenario.Step? dest = Scenario?.Steps.Find (s => s.UUID == prog.DestinationUUID);

                    if (dest != null) {
                        PropertyProgression pProg = new PropertyProgression ();
                        pProg.Init (prog.UUID, prog.DestinationUUID, dest.Name, prog.Description);
                        pProg.PropertyChanged += UpdateScenario;
                        stackProgressions.Children.Add (pProg);
                    }
                }
            }

            return Task.CompletedTask;
        }

        private async Task UpdateStepViewModel () {
            PropertyInt pintProgressTimer = this.FindControl<PropertyInt> ("pintProgressTimer");
            PropertyCombo pcmbProgressFrom = this.FindControl<PropertyCombo> ("pcmbProgressFrom");
            PropertyCombo pcmbProgressTo = this.FindControl<PropertyCombo> ("pcmbProgressTo");
            PropertyString pstrStepName = this.FindControl<PropertyString> ("pstrStepName");
            PropertyString pstrStepDescription = this.FindControl<PropertyString> ("pstrStepDescription");

            pintProgressTimer.IsEnabled = (ISelectedStep != null);
            pcmbProgressFrom.IsEnabled = (ISelectedStep != null);
            pcmbProgressTo.IsEnabled = (ISelectedStep != null);
            pstrStepName.IsEnabled = (ISelectedStep != null);
            pstrStepDescription.IsEnabled = (ISelectedStep != null);

            if (ISelectedStep != null) {
                /* This next chunk of code populates the "Progress To" and "Progress From" portion of the Step ViewModel*/

                // Should only be able to select default source and destination progression based on existing progressions!
                List<string> srcUUIDs = new () { "" }, srcNames = new () { "None" };
                List<string> destUUIDs = new () { "" }, destNames = new () { "None" };

                List<Scenario.Step> srcSteps = new (
                    Scenario?.Steps.FindAll (s => s.Progressions.Any (p => p.DestinationUUID == ISelectedStep.UUID))
                    ?? new List<Scenario.Step> ());
                List<Scenario.Step> destSteps = new (
                    Scenario?.Steps.FindAll (s => ISelectedStep.Step.Progressions.Any (p => p.DestinationUUID == s.UUID))
                    ?? new List<Scenario.Step> ());

                srcUUIDs.AddRange (srcSteps.Select (s => s.UUID ?? ""));
                srcNames.AddRange (srcSteps.Select (s => s.Name ?? ""));
                destUUIDs.AddRange (destSteps.Select (s => s.UUID ?? ""));
                destNames.AddRange (destSteps.Select (s => s.Name ?? ""));

                // Populate the actual ViewModel UserControls with the appropriate lists
                await pcmbProgressFrom.Update (srcUUIDs, srcNames,
                    ISelectedStep.Step.DefaultSource == null
                        ? 0 : srcUUIDs.FindIndex (ds => ds == ISelectedStep.Step.DefaultSource));

                await pcmbProgressTo.Update (destUUIDs, destNames,
                    ISelectedStep.Step.DefaultProgression == null
                        ? 0 : destUUIDs.FindIndex (ds => ds == ISelectedStep.Step.DefaultProgression.DestinationUUID));

                await pintProgressTimer.Set (ISelectedStep.Step.ProgressTimer);
                await pstrStepName.Set (ISelectedStep?.Step?.Name ?? "");
                await pstrStepDescription.Set (ISelectedStep?.Step?.Description ?? "");
            }
        }

        private Task ImportSteps () {
            /* For use when loading a saved file- Steps are already embedded in the Scenario,
             * the Scenario should already be set, and now we just need to process the Steps and
             * Progressions into the interface items (IStep) */

            foreach (Scenario.Step step in Scenario?.Steps ?? new List<Scenario.Step> ()) {
                ItemStep item = new ();
                item.Step = step;
                ISteps.Add (item);
                ICanvas.Children.Add (item);

                item.ZIndex = 1;
                item.PointerPressed += Item_PointerPressed;
                item.PointerReleased += Item_PointerReleased;
                item.PointerMoved += Item_PointerMoved;
                item.IStepEnd.PointerPressed += Item_PointerPressed;

                Canvas.SetLeft (item, item.Step.IISEPositionX);
                Canvas.SetTop (item, item.Step.IISEPositionY);
            }

            return Task.CompletedTask;
        }

        private async Task AddStep (ItemStep? incItem = null) {
            /* For use when adding a step via the UI designer- creates and instantiates the Step */

            if (Scenario is null)
                return;

            // Create data structures
            ItemStep item = new ();
            Scenario.Step step = new ();

            // Reference all relevant and interwoven data structures
            item.Step = step;
            Scenario.Steps.Add (step);
            ISteps.Add (item);
            ICanvas.Children.Add (item);

            if (incItem != null) {  // Duplicate
                                    // Copy interface properties and interface item properties
                item.Name = incItem.Name;
                item.Step.Name = incItem.Step.Name;

                item.Description = incItem.Description;
                item.Step.Description = incItem.Step.Description;

                await item.Step.Records.Load (incItem.Records.Save ());
                await item.Step.Physiology.Load (incItem.Physiology.Save ());
            } else {
                item.Name = $"Step #{ISteps.Count}";
            }

            // If this is the 1st step to be created, make it the Scenario's starting step
            if (item == ISteps.First ())
                Scenario.BeginStep = item.UUID;

            item.ZIndex = 1;
            item.PointerPressed += Item_PointerPressed;
            item.PointerReleased += Item_PointerReleased;
            item.PointerMoved += Item_PointerMoved;
            item.IStepEnd.PointerPressed += Item_PointerPressed;

            // Refresh the Properties View and draw Progression elements/colors
            await UpdateViewModel ();
            await DrawISteps ();
            await DrawIProgressions ();
        }

        private async Task DeleteStep (ItemStep item) {
            if (Scenario is null)
                return;

            // Remove the selected Step from the stack and visual
            ISteps.Remove (item);
            ICanvas.Children.Remove (item);

            foreach (ItemStep.Progression line in item.Progressions) {
                IProgressions.Remove (line);
                ICanvas.Children.Remove (line);
            }

            foreach (ItemStep s in ISteps) {
                // Nullify any default progression pointers that target the Step being deleted
                if (s.Step?.DefaultProgression?.UUID == item.UUID && s.Step is not null)
                    s.Step.DefaultProgression = null;

                // Remove all progressions targeting the Step being removed
                for (int i = s.Step.Progressions.Count - 1; i >= 0; i--) {
                    if (s.Step?.Progressions [i].DestinationUUID == item.UUID)
                        s.Step?.Progressions.RemoveAt (i);
                }
            }

            // Remove the Step from the Scenario model
            Scenario.Steps.RemoveAll (s => s.UUID == item.UUID);

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
            await UpdateViewModel ();

            Debug.WriteLine ($"Progression created from {itemFrom.UUID} -> {itemTo.UUID}");
        }

        private async Task DrawISteps () {
            foreach (ItemStep item in ISteps) {
                await item.UpdateViewModel ();
                await item.SetStep_Border (item == ISelectedStep);
                await item.SetEndStep_Border (IsSelectedStepEnd && item == ISelectedStep);

                // Color Step's Background depending
                if (Scenario?.BeginStep == item.UUID)
                    await item.SetStep_Fill (ItemStep.Fill_FirstStep);
                else {
                    bool isLinked = false;
                    foreach (Scenario.Step s in Scenario?.Steps ?? new List<Scenario.Step> ()) {
                        foreach (Scenario.Step.Progression p in s.Progressions) {
                            if (p.DestinationUUID == item.UUID) {
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

        private Task DrawIProgressions () {
            /* Recalculate the positions of Progression Lines on the Canvas
             * To be called when visual elements have moved on the Canvas
             */

            foreach (ItemStep.Progression ip in IProgressions) {
                ip.StartPoint = new Point (ip.Step.Bounds.Left + ip.StepEnd.Bounds.Left + ip.StepEnd.Width, ip.Step.Bounds.Center.Y);
                ip.EndPoint = new Point (ip.StepTo.Bounds.Left, ip.StepTo.Bounds.Center.Y);
                ip.InvalidateVisual ();
            }

            return Task.CompletedTask;
        }

        private Task UpdateIProgressions () {
            /* Re-iterate all Progressions and re-populate the list of Lines accordingly
             * To be called when the list of Progressions has changed
             */

            foreach (ItemStep.Progression ip in IProgressions)
                ICanvas.Children.Remove (ip);

            IProgressions.Clear ();

            foreach (ItemStep istep in ISteps) {
                istep.Progressions.Clear ();

                foreach (Scenario.Step.Progression prog in istep.Step.Progressions) {
                    ItemStep? itemTo = ISteps.Find (s => s.UUID == prog.DestinationUUID);

                    if (itemTo == null)
                        throw new IndexOutOfRangeException ();

                    ItemStep.Progression p = new (istep, itemTo, istep.IStepEnd, prog);
                    istep.Progressions.Add (p);
                    IProgressions.Add (p);
                    ICanvas.Children.Add (p);
                }
            }

            return Task.CompletedTask;
        }

        private async Task SelectIStep (ItemStep item, Avalonia.Input.Pointer? capture = null) {
            if (IMain is null)
                return;

            ISelectedStep = item;
            IsSelectedStepEnd = false;
            await IMain.SetStep (ISelectedStep.Step);

            if (capture != null) {
                PointerCaptured = capture;
                PointerCaptured.Capture (item);
            }

            await UpdateViewModel ();
        }

        private async Task SelectEndStep (ItemStepEnd end, Avalonia.Input.Pointer? capture = null) {
            if (IMain is null)
                return;

            ISelectedStep = end.Step;
            IsSelectedStepEnd = true;
            await IMain.SetStep (ISelectedStep.Step);

            if (capture != null) {
                PointerCaptured = capture;
                PointerCaptured.Capture (end.Step);
            }

            await UpdateViewModel ();
        }

        private async Task DeselectAll () {
            if (IMain is null)
                return;

            ISelectedStep = null;
            IsSelectedStepEnd = false;
            await IMain.SetStep (null);

            PointerPosition = null;

            if (PointerCaptured != null) {
                PointerCaptured.Capture (null);
                PointerCaptured = null;
            }

            await UpdateViewModel ();
        }

        private void Action_AddStep ()
            => _ = AddStep ();

        private void Action_DuplicateStep () {
            if (ISelectedStep == null)
                return;

            _ = AddStep (ISelectedStep);
        }

        private void Action_DeleteStep () {
            if (ISelectedStep != null) {
                _ = DeleteStep (ISelectedStep);
                _ = DeselectAll ();
            }
        }

        private async Task Action_RepositionSteps () {
            _ = DeselectAll ();

            int minorOffset = 25;
            int majorOffset = 200;
            int majorStack = 10;

            for (int i = 0; i < ISteps.Count; i++) {
                Canvas.SetLeft (ISteps [i],
                    (minorOffset * i)                                                                   // Cascading "minor" offset: x axis
                    - (System.Math.Floor ((double)(i / majorStack)) * majorStack * minorOffset));       // Reverts the "minor" offset every "major" offset to the stack's starting point

                Canvas.SetTop (ISteps [i],
                    (minorOffset * i)                                                                   // Cascading "minor" offset: y axis
                    + (System.Math.Floor ((double)(i / majorStack)) * majorOffset)                      // Adds a new "row" of cascading w/ a "major" offset
                    - (System.Math.Floor ((double)(i / majorStack)) * majorStack * minorOffset));       // But reverts the "minor" offset to the stack's starting point

                /* Set Step's metadata for saving/loading positioning data */
                ISteps [i].Step.IISEPositionX = (int)ISteps [i].Bounds.Left;
                ISteps [i].Step.IISEPositionY = (int)ISteps [i].Bounds.Top;
            }

            await UpdateIProgressions ();
            await DrawIProgressions ();
        }

        private async Task Action_CopyPhysiology () {
            if (ISelectedStep == null)
                return;

            if (CopiedStep == null)
                CopiedStep = new ();

            await CopiedStep.Physiology.Load (ISelectedStep.Physiology.Save ());
        }

        private async Task Action_CopyRecords () {
            if (ISelectedStep == null)
                return;

            if (CopiedStep == null)
                CopiedStep = new ();

            await CopiedStep.Records.Load (ISelectedStep.Records.Save ());
        }

        private async Task Action_PastePhysiology () {
            if (ISelectedStep == null)
                return;

            if (CopiedStep != null)
                await ISelectedStep.Physiology.Load (CopiedStep.Physiology.Save ());
        }

        private async Task Action_PasteRecords () {
            if (ISelectedStep == null)
                return;

            if (CopiedStep != null)
                await ISelectedStep.Records.Load (CopiedStep.Records.Save ());
        }

        /* Generic Menu Items (across all Panels) */

        private void MenuFileNew_Click (object sender, RoutedEventArgs e)
            => IMain?.MenuFileNew_Click (sender, e);

        private void MenuFileLoad_Click (object sender, RoutedEventArgs e)
            => IMain?.MenuFileLoad_Click (sender, e);

        private void MenuFileSave_Click (object sender, RoutedEventArgs e)
            => IMain?.MenuFileSave_Click (sender, e);

        private void MenuFileSaveAs_Click (object sender, RoutedEventArgs e)
            => IMain?.MenuFileSaveAs_Click (sender, e);

        private void MenuFileExit_Click (object sender, RoutedEventArgs e)
            => IMain?.MenuFileExit_Click (sender, e);

        private void MenuHelpAbout_Click (object sender, RoutedEventArgs e)
            => IMain?.MenuHelpAbout_Click (sender, e);

        /* Menu Items specific to this Panel */

        private void MenuEditAddStep_Click (object sender, RoutedEventArgs e)
            => Action_AddStep ();

        private void MenuEditDuplicateStep_Click (object sender, RoutedEventArgs e)
            => Action_DuplicateStep ();

        private void MenuEditDeleteStep_Click (object sender, RoutedEventArgs e)
            => Action_DeleteStep ();

        private void MenuEditRepositionSteps_Click (object sender, RoutedEventArgs e)
            => _ = Action_RepositionSteps ();

        private void MenuEditCopyPhysiology_Click (object sender, RoutedEventArgs e)
            => _ = Action_CopyPhysiology ();

        private void MenuEditPastePhysiology_Click (object sender, RoutedEventArgs e)
            => _ = Action_PastePhysiology ();

        private void MenuEditCopyRecords_Click (object sender, RoutedEventArgs e)
            => _ = Action_CopyRecords ();

        private void MenuEditPasteRecords_Click (object sender, RoutedEventArgs e)
            => _ = Action_PasteRecords ();

        /* Any other Routed events for this Panel */

        private void ButtonAddStep_Click (object sender, RoutedEventArgs e)
            => Action_AddStep ();

        private void ButtonDuplicateStep_Click (object sender, RoutedEventArgs e)
            => Action_DuplicateStep ();

        private void BtnDeleteStep_Click (object sender, RoutedEventArgs e)
            => Action_DeleteStep ();

        private void BtnCopyPhysiology_Click (object sender, RoutedEventArgs e)
            => _ = Action_CopyPhysiology ();

        private void BtnPastePhysiology_Click (object sender, RoutedEventArgs e)
            => _ = Action_PastePhysiology ();

        private void BtnCopyRecords_Click (object sender, RoutedEventArgs e)
            => _ = Action_CopyRecords ();

        private void BtnPasteRecords_Click (object sender, RoutedEventArgs e)
            => _ = Action_PasteRecords ();

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

                if (left > 0 && (left < ICanvas.Bounds.Width - ISelectedStep.Bounds.Width))
                    Canvas.SetLeft (ISelectedStep, left);

                if (top > 0 && (top < ICanvas.Bounds.Height - ISelectedStep.Bounds.Height))
                    Canvas.SetTop (ISelectedStep, top);

                /* Set Step's metadata for saving/loading positioning data */
                ISelectedStep.Step.IISEPositionX = (int)ISelectedStep.Bounds.Left;
                ISelectedStep.Step.IISEPositionY = (int)ISelectedStep.Bounds.Top;

                _ = DrawIProgressions ();
            }
        }
    }
}