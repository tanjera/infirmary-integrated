using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
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
using System.Windows.Threading;

namespace II_Windows {

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
