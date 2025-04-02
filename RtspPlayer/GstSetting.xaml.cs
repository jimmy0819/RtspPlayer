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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RtspPlayer
{
    /// <summary>
    /// Interaction logic for GstSetting.xaml
    /// </summary>
    public partial class GstSetting : UserControl
    {
        GstreamerPlayer gstP;
        public GstSetting()
        {
            InitializeComponent();
        }
        public void AssignGstPlayer(GstreamerPlayer gstreamerPlayer, String name)
        {
            gstP = gstreamerPlayer;
            btnSettings.Content = "⚙"+name;
        }
        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            settingsPopup.IsOpen = true;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            settingsPopup.IsOpen = false;
            if (gstP != null)
            {
                gstP.SetPipeLine(GetPipelineString());
            }
        }
        
        public string GetPipelineString()
        {
            return $"rtspsrc location={txtRtspUrl.Text} " +
                   $"latency={txtLatency.Text} " +
                   $"protocols={txtProtocols.Text} " +
                   $"drop-on-latency={(chkDropOnLatency.IsChecked == true ? 1 : 0)} ! " +
                   "queue max-size-buffers=" + txtMaxBuffers.Text + " leaky=downstream ! " +
                   $"rtph265depay ! avdec_h265 ! videoconvert ! video/x-raw,format=RGB ! appsink name=outsink sync=false max-buffers={txtBinBuffer.Text} drop=true";
        }   
        public void ParsePipelineString(string pipeline)
        {
            var parameters = pipeline.Split(' ');
            foreach (var param in parameters)
            {
                var keyValue = param.Split('=');
                if (keyValue.Length == 2)
                {
                    switch (keyValue[0])
                    {
                        case "location":
                            txtRtspUrl.Text = keyValue[1];
                            break;
                        case "latency":
                            txtLatency.Text = keyValue[1];
                            break;
                        case "protocols":
                            txtProtocols.Text = keyValue[1];
                            break;
                        case "drop-on-latency":
                            chkDropOnLatency.IsChecked = keyValue[1] == "1";
                            break;
                        case "max-size-buffers":
                            txtMaxBuffers.Text = keyValue[1];
                            break;
                        case "max-buffers":
                            txtBinBuffer.Text = keyValue[1];
                            break;
                    }
                }
                else if(keyValue.Length > 2)
                {
                    for(int i = 1; i< keyValue.Length; i++) {

                        txtRtspUrl.Text += keyValue[i];
                        if(i != keyValue.Length - 1)
                        {
                            //not last
                            txtRtspUrl.Text += "=";
                        }
                    }
                }
            }
        }
    }
}
