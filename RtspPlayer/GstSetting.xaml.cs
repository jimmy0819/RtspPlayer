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
            gstreamerPlayer.PipelineTextChanged += parsePipelineString;
        }
        private void parsePipelineString(String pipeline)
        {
            ParsePipelineString(pipeline);
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
            PipelineRtsp newpipe =
                new PipelineRtsp()
                {
                    Location = txtRtspUrl.Text,
                    Latency = Int32.Parse(txtLatency.Text),
                    DropOnLatency = (chkDropOnLatency.IsChecked == true ? 1 : 0),
                    Protocols = (Protocols)Enum.Parse(typeof(Protocols), txtProtocols.Text),
                    MaxSizeBuffers = Int32.Parse(txtMaxBuffers.Text),
                    SkipToFirst = (chkSkipToFirst.IsChecked == true ? true : false),
                    Sync = true,
                    MaxBuffers = Int32.Parse(txtBinBuffer.Text),
                    Drop = (chkDropOnLatency.IsChecked == true),
                    JitterBuffer = (chkJitterBuffer.IsChecked == true),
                    IsInitailized = true
                };
            //string auto = "rtspsrc location=rtsp://admin:123456@172.17.30.242/stream0 latency=4000 protocols=GST_RTSP_LOWER_TRANS_UDP ! decodebin name=decoder ! videoconvert ! videorate skip-to-first=false ! video/x-raw,format=RGB ! autovideosink sync=false";
            return newpipe.ToString();
                //auto;
            //$"rtspsrc location={txtRtspUrl.Text} " +
            //   $"latency={txtLatency.Text} " +
            //   $"protocols={txtProtocols.Text} " +
            //   $"drop-on-latency={(chkDropOnLatency.IsChecked == true ? 1 : 0)} ! " +
            //   $"{(chkJitterBuffer.IsChecked == true ? $"rtpjitterbuffer latency={txtLatency.Text} drop-on-latency=0 ! " : "")}" +
            //   "queue max-size-buffers=" + txtMaxBuffers.Text + " leaky=downstream ! " +
            //   $"rtph265depay ! h265parse ! avdec_h265 ! videoconvert ! videorate skip-to-first=true ! video/x-raw,format=RGB ! appsink name=outsink sync=false max-buffers={txtBinBuffer.Text} drop=true";
        }
        public void ParsePipelineString(string pipeline)
        {
            PipelineRtsp newpipe = new PipelineRtsp();
            newpipe.ParsePipelineString(pipeline);
            var parameters = pipeline.Split(' ');
            
            chkJitterBuffer.IsChecked = newpipe.JitterBuffer;
                            
            txtRtspUrl.Text = newpipe.Location;
                            
            chkMainStream.IsChecked = newpipe.IsMainStream;
                            
            txtLatency.Text = newpipe.Latency.ToString();
                           
            txtProtocols.Text = newpipe.Protocols.ToString();
                            
            chkDropOnLatency.IsChecked =(newpipe.DropOnLatency == 1);
                            
            txtMaxBuffers.Text = newpipe.MaxBuffers.ToString();
                            
            txtBinBuffer.Text = newpipe.MaxSizeBuffers.ToString();
                            
        }

        private void chkMainStream_Checked(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtRtspUrl.Text))
                throw new ArgumentException("Pipeline string cannot be null or empty.", nameof(txtRtspUrl.Text));
            if(sender is CheckBox checkBox)
            {
                if (checkBox.IsChecked == true)
                {
                    txtRtspUrl.Text = txtRtspUrl.Text.Replace("/stream1", "/stream0");
                    
                }
                else
                {
                    txtRtspUrl.Text = txtRtspUrl.Text.Replace("/stream0", "/stream1");
                    
                }
            }
        }

        private void chkMainStream_Unchecked(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtRtspUrl.Text))
                throw new ArgumentException("Pipeline string cannot be null or empty.", nameof(txtRtspUrl.Text));
            if (sender is CheckBox checkBox)
            {
                if (checkBox.IsChecked == true)
                {
                    txtRtspUrl.Text = txtRtspUrl.Text.Replace("/stream1", "/stream0");

                }
                else
                {
                    txtRtspUrl.Text = txtRtspUrl.Text.Replace("/stream0", "/stream1");

                }
            }
        }
    }
}
