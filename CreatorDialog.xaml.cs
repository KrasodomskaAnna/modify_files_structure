using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace lab_2
{
    /// <summary>
    /// Logika interakcji dla klasy CreatorDialog.xaml
    /// </summary>
    public partial class CreatorDialog : Window
    {
		private string regPattern = @"^[a-zA-Z0-9_~-]{1,8}\.(txt|php|html)";

        public CreatorDialog()
        {
			InitializeComponent();
        }

        private void OnOkClick(object sender, RoutedEventArgs e) { 
			if (this.file_t.IsChecked == true) {
				if (!Regex.IsMatch(this.name.Text, regPattern))
				{
					infoErr.Text = "Nazwa pliku nie pasuje do patternu. Popraw go, aby był zgodny z reg: " + this.regPattern;
					return;
				}
			}

            DialogResult = true;		
		}
        private void OnCancelClick(object sender, RoutedEventArgs e)  =>
            DialogResult = false;
    }
}
