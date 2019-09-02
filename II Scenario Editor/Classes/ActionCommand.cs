using System;
using System.Windows.Input;

namespace II.Scenario_Editor {

    public class ActionCommand : ICommand {
        private readonly Action _action;

        public ActionCommand (Action action) {
            _action = action;
        }

        public void Execute (object parameter) {
            _action ();
        }

        public bool CanExecute (object parameter) {
            return true;
        }

#pragma warning disable CS0067

        public event EventHandler CanExecuteChanged;

#pragma warning restore CS0067
    }
}