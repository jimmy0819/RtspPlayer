using System.Windows.Controls;

namespace RtspPlayer
{
    public partial class LoggerControl : UserControl
    {
        public LoggerControl()
        {
            InitializeComponent();
        }

        public void AppendLog(string message)
        {
            LogTextBlock.Text += message + "\n";
            ScrollViewer.ScrollToEnd();
        }
    }
}