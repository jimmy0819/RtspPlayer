using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using GLib;
using Gst;
using Gst.Video;
using Gst.App;
using System.Windows.Media.Imaging;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Windows.Media;


namespace RtspPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string pipelineString11 = "rtspsrc location=rtsp://admin:123456@172.17.30.240/stream1 " +
                        "latency=300 protocols=GST_RTSP_LOWER_TRANS_UDP drop-on-latency=1 ! " +
                        "queue max-size-buffers=10 leaky=downstream ! " +
                        "rtph265depay ! avdec_h265 ! videoconvert ! video/x-raw,format=RGB ! appsink name=outsink sync=false max-buffers=3 drop=true";
        string pipelineString12 = "rtspsrc location=rtsp://172.17.30.100:554/chID=3&streamType=main " +
                        "latency=300 protocols=GST_RTSP_LOWER_TRANS_UDP drop-on-latency=1 ! " +
                        "queue max-size-buffers=20 leaky=downstream ! " +
                        "rtph265depay ! avdec_h265 ! videoconvert ! video/x-raw,format=RGB ! appsink name=outsink sync=false max-buffers=20 drop=true";
        string pipelineString13 = "rtspsrc location=rtsp://admin:123456@172.17.30.241/stream1 " +
                        "latency=300 protocols=GST_RTSP_LOWER_TRANS_UDP drop-on-latency=1 ! " +
                        "queue max-size-buffers=20 leaky=downstream ! " +
                        "rtph265depay ! avdec_h265 ! videoconvert ! video/x-raw,format=RGB ! appsink name=outsink sync=false max-buffers=20 drop=true";
        public MainWindow()
        {
            InitializeComponent();
            Screen1.PipelineText = pipelineString11;
            Screen1.StartGst();
            GstSet1.AssignGstPlayer(Screen1, "1");
            GstSet1.ParsePipelineString(pipelineString11);
            Screen2.PipelineText = pipelineString12;
            Screen2.StartGst();
            GstSet2.AssignGstPlayer(Screen2, "2");
            GstSet2.ParsePipelineString(pipelineString12);
            Screen3.PipelineText = pipelineString13;
            Screen3.StartGst();
            GstSet3.AssignGstPlayer(Screen3, "3");
            GstSet3.ParsePipelineString(pipelineString13);
        }

    }
}