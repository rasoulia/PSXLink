using PSXLink.MVVM.Models;
using System.Windows;
using System.Windows.Controls;

namespace PSXLink.MVVM.Views
{
    /// <summary>
    /// Interaction logic for LogView.xaml
    /// </summary>
    public partial class LogView : UserControl
    {
        public LogView()
        {
            InitializeComponent();
        }

        private void BtnCopyLink_Click(object sender, RoutedEventArgs e)
        {
            UpdateLog? row = ((Button)e.Source).DataContext as UpdateLog;
            if (row == null)
            {
                return;
            }
            Clipboard.SetText(row.Link);
        }
    }
}
