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
using System.Windows.Shapes;

using II.Localization;

namespace II_Windows {
    /// <summary>
    /// Interaction logic for DialogLanguage.xaml
    /// </summary>
    public partial class DialogLanguage : Window {
        public DialogLanguage () {
            InitializeComponent ();

            // Populate UI strings per language selection
            dlgLanguage.Title = App.Language.Dictionary["LANG:LanguageSelection"];
            lblChooseLanguage.Content = App.Language.Dictionary["LANG:ChooseLanguage"];
            btnContinue.Content = App.Language.Dictionary["BUTTON:Continue"];

            cmbLanguages.ItemsSource = Languages.Descriptions;
            cmbLanguages.SelectedIndex = (int)Languages.Values.ENU;
        }

        private void OnClick_Continue (object sender, RoutedEventArgs e) {
            App.Language.Value = (Languages.Values)Enum.GetValues(typeof(Languages.Values)).GetValue(cmbLanguages.SelectedIndex);

            Properties.Settings.Default.Language = App.Language.Value.ToString();
            Properties.Settings.Default.Save ();
            this.Close ();
        }
    }
}
