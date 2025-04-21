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
using System.Numerics;


namespace RtspPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string pipelineString11 = "rtspsrc location=rtsp://admin:123456@172.17.30.240/stream1 " +
                        "latency=200 protocols=GST_RTSP_LOWER_TRANS_UDP drop-on-latency=1 ! " +
                        "queue max-size-buffers=10 leaky=downstream ! " +
                        "rtph265depay ! h265parse ! avdec_h265  ! videoconvert ! videorate skip-to-first=true ! " +
                        "video/x-raw,format=RGB ! appsink name=outsink sync=false max-buffers=20 drop=true";
        string pipelineString12 = "rtspsrc location=rtsp://admin:123456@172.17.30.241/stream1 " +
                        "latency=200 protocols=GST_RTSP_LOWER_TRANS_TCP drop-on-latency=1 ! " +
                        "queue max-size-buffers=10 leaky=downstream ! " +
                        "rtph265depay ! h265parse ! avdec_h265 ! videoconvert ! videorate skip-to-first=true ! video/x-raw,format=RGB ! appsink name=outsink sync=false max-buffers=20 drop=true";
        string pipelineString13 = "rtspsrc location=rtsp://admin:123456@172.17.30.242/stream1 " +
                        "latency=200 protocols=GST_RTSP_LOWER_TRANS_UDP drop-on-latency=1 ! " +
                        "queue max-size-buffers=10 leaky=downstream ! " +
                        "rtph265depay ! h265parse ! avdec_h265 ! videoconvert ! videorate skip-to-first=true ! video/x-raw,format=RGB ! appsink name=outsink sync=false max-buffers=20 drop=true";
        string pipelineString14 = "rtspsrc location=rtsp://admin:123456@172.17.30.243/stream1 " +
                        "latency=200 protocols=GST_RTSP_LOWER_TRANS_TCP drop-on-latency=1 ! " +
                        "queue max-size-buffers=10 leaky=downstream ! " +
                        "rtph265depay ! h265parse ! avdec_h265 ! videoconvert ! videorate skip-to-first=true ! video/x-raw,format=RGB ! appsink name=outsink sync=false max-buffers=20 drop=true";
        string pipelineString15 = "rtspsrc location=rtsp://admin:123456@172.17.30.244/stream1 " +
                        "latency=200 protocols=GST_RTSP_LOWER_TRANS_UDP drop-on-latency=1 ! " +
                        "queue max-size-buffers=10 leaky=downstream ! " +
                        "rtph265depay ! h265parse ! avdec_h265 ! videoconvert ! videorate skip-to-first=true ! video/x-raw,format=RGB ! appsink name=outsink sync=false max-buffers=20 drop=true";
        string pipelineString16 = "rtspsrc location=rtsp://admin:123456@172.17.30.245/stream1 " +
                        "latency=200 protocols=GST_RTSP_LOWER_TRANS_TCP drop-on-latency=1 ! " +
                        "queue max-size-buffers=10 leaky=downstream ! " +
                        "rtph265depay ! h265parse ! avdec_h265 ! videoconvert ! videorate skip-to-first=true ! video/x-raw,format=RGB ! appsink name=outsink sync=false max-buffers=20 drop=true";
        string pipelineString17 = "rtspsrc location=rtsp://admin:123456@172.17.30.246/stream1 " +
                        "latency=200 protocols=GST_RTSP_LOWER_TRANS_UDP drop-on-latency=1 ! " +
                        "queue max-size-buffers=10 leaky=downstream ! " +
                        "rtph265depay ! h265parse ! avdec_h265 ! videoconvert ! videorate skip-to-first=true ! video/x-raw,format=RGB ! appsink name=outsink sync=false max-buffers=20 drop=true";
        string pipelineString18 = "rtspsrc location=rtsp://admin:123456@172.17.30.247/stream1 " +
                        "latency=200 protocols=GST_RTSP_LOWER_TRANS_TCP drop-on-latency=1 ! " +
                        "queue max-size-buffers=10 leaky=downstream ! " +
                        "rtph265depay ! h265parse ! avdec_h265 ! videoconvert ! videorate skip-to-first=true ! video/x-raw,format=RGB ! appsink name=outsink sync=false max-buffers=20 drop=true";


        string pipelineString_notUse = "rtspsrc location=rtsp://172.17.30.100:554/chID=3&streamType=main " +
                        "latency=300 protocols=GST_RTSP_LOWER_TRANS_UDP drop-on-latency=1 ! " +
                        "queue max-size-buffers=20 leaky=downstream ! " +
                        "rtph265depay ! avdec_h265 ! videoconvert ! video/x-raw,format=RGB ! appsink name=outsink sync=false max-buffers=20 drop=true";

        GstreamerPlayer _mainStreamNow;
        public GstreamerPlayer MainStreamNow
        {
            get => _mainStreamNow;
            set
            {
                if (_mainStreamNow != value)
                {
                    _mainStreamNow = value;
                    OnMainStreamNowChanged();
                }
            }
        }

        private void OnMainStreamNowChanged()
        {
            // Add the logic to be invoked when MainStreamNow is set  
            //MessageBox.Show("MainStreamNow has been updated!");
        }
        private string ChangeToMain(string pipelineString)
        {
            if (string.IsNullOrEmpty(pipelineString))
                throw new ArgumentException("Pipeline string cannot be null or empty.", nameof(pipelineString));



            return pipelineString.Replace("/stream1", "/stream0");
        }

        private string ChangeToMinor(string pipelineString)
        {
            if (string.IsNullOrEmpty(pipelineString))
                throw new ArgumentException("Pipeline string cannot be null or empty.", nameof(pipelineString));

            return pipelineString.Replace("/stream0", "/stream1");
        }

        public MainWindow()
        {
            InitializeComponent();

            //set ini mainstream
            MainStreamNow = Screen1;
            pipelineString11 = ChangeToMain(pipelineString11);
            Screen1.PipelineText = pipelineString11;
            Screen1.StartGst();
            Screen1.SetTag("1");
            GstSet1.AssignGstPlayer(Screen1, "1");
            GstSet1.ParsePipelineString(pipelineString11);

            Screen2.PipelineText = pipelineString12;
            Screen2.StartGst();
            Screen2.SetTag("2");
            GstSet2.AssignGstPlayer(Screen2, "2");
            GstSet2.ParsePipelineString(pipelineString12);

            Screen3.PipelineText = pipelineString13;
            Screen3.StartGst();
            Screen3.SetTag("3");
            GstSet3.AssignGstPlayer(Screen3, "3");
            GstSet3.ParsePipelineString(pipelineString13);

            Screen4.PipelineText = pipelineString14;
            Screen4.StartGst();
            Screen4.SetTag("4");
            GstSet4.AssignGstPlayer(Screen4, "4");
            GstSet4.ParsePipelineString(pipelineString14);

            Screen5.PipelineText = pipelineString15;
            Screen5.StartGst();
            Screen5.SetTag("5");
            GstSet5.AssignGstPlayer(Screen5, "5");
            GstSet5.ParsePipelineString(pipelineString15);

            Screen6.PipelineText = pipelineString16;
            Screen6.StartGst();
            Screen6.SetTag("6");
            GstSet6.AssignGstPlayer(Screen6, "6");
            GstSet6.ParsePipelineString(pipelineString16);


            Screen7.PipelineText = pipelineString17;
            Screen7.StartGst();
            Screen7.SetTag("7");
            GstSet7.AssignGstPlayer(Screen7, "7");
            GstSet7.ParsePipelineString(pipelineString17);

            Screen8.PipelineText = pipelineString18;
            Screen8.StartGst();
            Screen8.SetTag("8");
            GstSet8.AssignGstPlayer(Screen8, "8");
            GstSet8.ParsePipelineString(pipelineString18);


        }

        private void btn_RefeshAll_Click(object sender, RoutedEventArgs e)
        {
            Screen1.Refresh();
            Screen2.Refresh();
            Screen3.Refresh();
            Screen4.Refresh();
            Screen5.Refresh();
            Screen6.Refresh();
            Screen7.Refresh();
            Screen8.Refresh();
        }

        private void Screen_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if(sender != MainStreamNow)
            {
                //doswap
                // Get the column and row of the clicked player
                int column = Grid.GetColumn(MainStreamNow);
                int row = Grid.GetRow(MainStreamNow);
                int columnSpan =Grid.GetColumnSpan(MainStreamNow);
                int rowSpan = Grid.GetRowSpan(MainStreamNow);

                Grid.SetColumn(MainStreamNow, Grid.GetColumn((GstreamerPlayer)sender));
                Grid.SetRow(MainStreamNow, Grid.GetRow((GstreamerPlayer)sender));
                Grid.SetColumnSpan(MainStreamNow, Grid.GetColumnSpan((GstreamerPlayer)sender));
                Grid.SetRowSpan(MainStreamNow, Grid.GetRowSpan((GstreamerPlayer)sender));

                Grid.SetColumn((GstreamerPlayer)sender, column);
                Grid.SetRow((GstreamerPlayer)sender, row);
                Grid.SetColumnSpan((GstreamerPlayer)sender, columnSpan);
                Grid.SetRowSpan((GstreamerPlayer)sender, rowSpan);

                //set origin tominor

                MainStreamNow.PipelineText = ChangeToMinor(MainStreamNow.PipelineText);
                MainStreamNow.Refresh();

                MainStreamNow = (GstreamerPlayer)sender;
                //set mainsteam
                string mainSting = ChangeToMain(MainStreamNow.PipelineText);
                MainStreamNow.PipelineText = mainSting;
                MainStreamNow.Refresh();
            }
        }
    }
}