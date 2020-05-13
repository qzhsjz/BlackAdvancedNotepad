using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MiniNotepad
{
    /// <summary>
    /// InputDialog.xaml 的交互逻辑
    /// </summary>
    public partial class InputDialog : Window
    {
        public string Result => tbContent.Text;
        public InputDialog(string title="", string prompt="", string initval="")
        {
            InitializeComponent();
            lbPrompt.Content = prompt;
            Title = title;
            tbContent.Text = initval;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
