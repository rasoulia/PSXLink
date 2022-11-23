using PSXLink.MVVM.Models;
using System.Linq;
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
            if (row is null)
            {
                return;
            }
            string links = string.Join("\n", row.Link!.Split("\n").Skip(1));
            Clipboard.SetText(links);
        }

        private int index = 0;
        private void LogList_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            if (LogList.Items.Count - 1 != index)
            {
                index = LogList.Items.Count - 1;
                LogList.ScrollIntoView(LogList.Items[^1]);
            }
        }
    }
}
