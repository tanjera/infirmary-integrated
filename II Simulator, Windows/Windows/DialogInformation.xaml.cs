using II;
using II.Localization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace IISIM.Windows {

    /// <summary>
    /// Interaction logic for DialogEULA.xaml
    /// </summary>
    public partial class DialogInformation : Window {
        public App? Instance;

        public string Message {
            set {
                txtMessage.Text = value;
            }
        }

        public string Button {
            set {
                btnContinue.Content = value;
            }
        }

        public DialogInformation () {
            InitializeComponent ();
        }

        public DialogInformation (App? app) {
            InitializeComponent ();

            DataContext = this;
            Instance = app;
        }

        private void ButtonOK_Click (object s, RoutedEventArgs e) => this.Close ();
    }
}