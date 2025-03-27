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
        
        private Pipeline _pipeline;
        private Bus _bus;
        private D3DImage _d3dImage;
        private AppSink _videoSink;
        private int _renderTimerFrequency=60;
        private int _messageTimerFrequency =30;
        private Timer _renderTimer = null;
        private Timer _messageTimer = null;
        private bool IsSynchronized = false;
        private long _sampleLock = 0;
        private Element appSink;
        
   

        public MainWindow()
        {
            InitializeComponent();
         

            InitializeGStreamer();
        }

        private void InitializeGStreamer()
        {
            // Set GST_DEBUG level before initializing GStreamer
            Environment.SetEnvironmentVariable("GST_DEBUG", "4"); // Set verbosity level

            // Set GStreamer Path before initialization
            GStreamerSetup.SetGSTPath(@"C:\Program Files\gstreamer\1.0\mingw_x86_64");

            Gst.Application.Init(); // Initialize GStreamer

            // Simple test video pipeline
            string pipelineString =
                " compositor name=mix sink_0::xpos=0 sink_0::ypos=0 sink_1::xpos=640 sink_1::ypos=0 sink_2::xpos=1280 sink_2::ypos=0 " +
                " sink_3::xpos=0 sink_3::ypos=360 sink_4::xpos=640 sink_4::ypos=360 sink_5::xpos=1280 sink_5::ypos=360 " +
                " sink_6::xpos=0 sink_6::ypos=720 sink_7::xpos=640 sink_7::ypos=720 sink_8::xpos=1280 sink_8::ypos=720 ! " +
                " videoconvert ! autovideosink " +
                " rtspsrc location=rtsp://172.17.30.100:554/chID=3&streamType=main retry=5 ! queue ! decodebin ! videoconvert ! videoscale ! video/x-raw,width=640,height=360 ! mix. " +
                " rtspsrc location=rtsp://IP2 retry=5 ! queue ! decodebin ! videoconvert ! videoscale ! video/x-raw,width=640,height=360 ! mix. " +
                " rtspsrc location=rtsp://IP3 retry=5 ! queue ! decodebin ! videoconvert ! videoscale ! video/x-raw,width=640,height=360 ! mix. " +
                " rtspsrc location=rtsp://IP4 retry=5 ! queue ! decodebin ! videoconvert ! videoscale ! video/x-raw,width=640,height=360 ! mix. " +
                " rtspsrc location=rtsp://IP5 retry=5 ! queue ! decodebin ! videoconvert ! videoscale ! video/x-raw,width=640,height=360 ! mix. " +
                " rtspsrc location=rtsp://IP6 retry=5 ! queue ! decodebin ! videoconvert ! videoscale ! video/x-raw,width=640,height=360 ! mix. " +
                " rtspsrc location=rtsp://IP7 retry=5 ! queue ! decodebin ! videoconvert ! videoscale ! video/x-raw,width=640,height=360 ! mix. " +
                " rtspsrc location=rtsp://IP8 retry=5 ! queue ! decodebin ! videoconvert ! videoscale ! video/x-raw,width=640,height=360 ! mix. " +
                " rtspsrc location=rtsp://IP9 retry=5 ! queue ! decodebin ! videoconvert ! videoscale ! video/x-raw,width=640,height=360 ! mix. ";
            string pipelineString2 = "rtspsrc location=rtsp://172.17.30.100:554/chID=3&streamType=main retry=5 ! queue ! decodebin ! videoconvert ! autovideosink ";

            string pipelineString3 = "rtspsrc location=rtsp://172.17.30.100:554/chID=3&streamType=main " +
                        "latency=0 protocols=GST_RTSP_LOWER_TRANS_UDP drop-on-latency=1 ! " +
                        "queue max-size-buffers=1 leaky=downstream ! " +
                        "decodebin ! videoconvert ! autovideosink sync=false";

            string pipelineString4 = "rtspsrc location=rtsp://172.17.30.100:554/chID=3&streamType=main " +
                        "latency=50 protocols=GST_RTSP_LOWER_TRANS_UDP drop-on-latency=10 ! " +
                        "queue max-size-buffers=1 leaky=downstream ! " +
                        "rtph265depay ! avdec_h265 ! videoconvert ! video/x-raw,format=RGB ! autovideosink sync=false";

            string pipelineString5 = "rtspsrc location=rtsp://172.17.30.100:554/chID=3&streamType=main " +
                        "latency=200 protocols=GST_RTSP_LOWER_TRANS_UDP drop-on-latency=0 ! " +
                        "rtpjitterbuffer latency=200 drop-on-latency=0 ! " +
                        "queue max-size-buffers=25 leaky=downstream ! " +
                        "rtph265depay ! avdec_h265 ! videoconvert ! video/x-raw,format=RGB ! autovideosink sync=false";
            //appsink name=outsink
            string pipelineString6 = "rtspsrc location=rtsp://172.17.30.100:554/chID=3&streamType=main " +
                        "latency=400 protocols=GST_RTSP_LOWER_TRANS_UDP drop-on-latency=1 ! " +
                        "rtpjitterbuffer latency=400 drop-on-latency=1 ! " +
                        "queue max-size-buffers=60 leaky=downstream ! " +
                        "rtph265depay ! avdec_h265 ! videoconvert ! video/x-raw,format=RGB ! " +
                        "tee name=t " +  // Duplicates video stream
                        "t. ! queue ! appsink name=outsink sync=false " + // Sends frames to WPF
                        "t. ! queue ! autovideosink sync=false";    // Original Video Display

            string pipelineString7 = "rtspsrc location=rtsp://172.17.30.100:554/chID=3&streamType=main " +
                        "latency=400 protocols=GST_RTSP_LOWER_TRANS_UDP drop-on-latency=0 ! " +
                        "rtpjitterbuffer latency=400 drop-on-latency=0 ! " +
                        "queue max-size-buffers=60 leaky=downstream ! " +
                        "rtph265depay ! avdec_h265 ! videoconvert ! video/x-raw,format=RGB ! appsink name=outsink sync=false";

            string pipelineString8 = "rtspsrc location= rtsp://admin:123456@192.168.1.240/stream0 " +
                        "latency=800 protocols=GST_RTSP_LOWER_TRANS_UDP drop-on-latency=0 ! " +
                        "rtpjitterbuffer latency=800 drop-on-latency=0 ! " +
                        "queue max-size-buffers=120 leaky=downstream ! " +
                        "rtph265depay ! avdec_h265 ! videoconvert ! video/x-raw,format=RGB ! autovideosink sync=false";

            string pipelineString9 = "rtspsrc location= rtsp://admin:123456@172.17.30.240/stream1 " +
                        "latency=200 protocols=GST_RTSP_LOWER_TRANS_UDP drop-on-latency=0 ! " +
                        "rtpjitterbuffer latency=200 drop-on-latency=0 ! " +
                        "queue max-size-buffers=30 leaky=downstream ! " +
                        "rtph265depay ! avdec_h265 ! videoconvert ! video/x-raw,format=RGB ! autovideosink sync=false";

            string pipelineString10 = "rtspsrc location= rtsp://admin:123456@172.17.30.240/stream1 " +
                        "latency=200 protocols=GST_RTSP_LOWER_TRANS_UDP drop-on-latency=0 ! " +
                        "rtpjitterbuffer latency=200 drop-on-latency=0 ! " +
                        "queue max-size-buffers=30 leaky=downstream ! " +
                        "rtph265depay ! avdec_h265 ! videoconvert ! video/x-raw,format=RGB ! appsink name=outsink sync=false";

            string pipelineString11 = "rtspsrc location= rtsp://admin:123456@172.17.30.240/stream1 " +
                        "latency=200 protocols=GST_RTSP_LOWER_TRANS_UDP drop-on-latency=0 ! " +
                        "rtpjitterbuffer latency=200 drop-on-latency=0 ! " +
                        "queue max-size-buffers=30 leaky=downstream ! " +
                        "rtph265depay ! avdec_h265 ! videoconvert ! video/x-raw,format=RGB ! autovideosink sync=false";

            try
            {
                _pipeline = (Pipeline)Parse.Launch(pipelineString10);
                _bus = _pipeline.Bus;


                // Start playing
                InitializeAppSink("outsink");
                _pipeline.SetState(State.Playing);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error initializing GStreamer: " + e.Message);
            }
        }

        

        private void InitializeAppSink(string _videoSinkName)
        {
            appSink = _pipeline.GetChildByName(_videoSinkName) as Element;

            _renderTimer = new Timer(RenderTimerProc, this, 0, 1000 / _renderTimerFrequency);
            _messageTimer = new Timer(MessageTimerProc, this, 0, 1000 / _messageTimerFrequency);
        }

        private void MessageTimerProc(object _)
        {
            if (_pipeline != null)
            {
                using (var bus = _pipeline.Bus)
                {
                    var message = bus.Poll(MessageType.Any, 0);
                    if (message != null)
                    {
                        OnNewMessage(message);
                        message.Dispose();
                    }
                }
            }
        }

        private void RenderTimerProc(object _)
        {
            if (System.Threading.Interlocked.CompareExchange(ref _sampleLock, 1, 0) == 0)
            {

                try
                {
                    if (appSink != null)
                    {
                        Console.WriteLine("Appsink found!");

                        // Get last sample manually
                        GLib.Value sampleValue = appSink.GetProperty("last-sample");
                        Sample sample = sampleValue.Val as Sample;

                        if (sample != null)
                        {
                            using (sample)
                            {
                                Gst.Buffer buffer = sample.Buffer;
                                using (buffer)
                                {
                                    Gst.MapInfo map;
                                    if (buffer.Map(out map, MapFlags.Read))
                                    {
                                        // Get the Caps (capabilities) from the sample
                                        var caps = sample.Caps;
                                        var structure = caps.GetStructure(0);  // Assuming one structure for video

                                        // Extract width and height from the structure
                                        int width = 0, height = 0;
                                        structure.GetInt("width", out width);
                                        structure.GetInt("height", out height);

                                        Console.WriteLine($"Frame Resolution: {width}x{height}");

                                        
                                        // Since map.Data is already a byte[], we can use it directly
                                        byte[] frameData = map.Data;

                                        
                                        // Update the WPF Image control with the new frame
                                        UpdateFrame(frameData, width, height);

                                        // Unmap the buffer after processing
                                        buffer.Unmap(map);
                                    }
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("No sample available.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Failed to find appsink element.");
                    }
                }
                finally
                {
                    System.Threading.Interlocked.Decrement(ref _sampleLock);
                }
            }
        }

        public void UpdateFrame(byte[] frameData, int width, int height)
        {
            var bitmap = CreateRGBBitmapSource(frameData, width, height);
            Dispatcher.Invoke(() =>
            {
                videoImage.Source = bitmap;
            });
        }

        private WriteableBitmap CreateWriteableBitmap(byte[] frameData, int width, int height)
        {
            // Create a WriteableBitmap with the correct pixel format and dimensions
            var writeableBitmap = new WriteableBitmap(width, height, 96, 96, System.Windows.Media.PixelFormats.Bgra32, null);

            // Copy the byte data into the WriteableBitmap
            writeableBitmap.Lock();
            Marshal.Copy(frameData, 0, writeableBitmap.BackBuffer, frameData.Length);
            writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
            writeableBitmap.Unlock();
            writeableBitmap.Freeze();
            //File.WriteAllBytes("frameData.raw", frameData);

            return writeableBitmap;
        }

        private BitmapSource CreateRGBBitmapSource(byte[] frameData, int width, int height)
        {
            // Create a BitmapSource from raw RGB data (3 bytes per pixel)
            BitmapSource bitmap = BitmapSource.Create(
                width, height, 96, 96,
                PixelFormats.Rgb24, // Supports 3 bytes per pixel
                null,
                frameData,
                width * 3 // Stride = width * bytesPerPixel (3 for RGB)
            );
            bitmap.Freeze();
            return bitmap;
        }

        private void OnNewMessage(Gst.Message message)
        {
            switch (message.Type)
            {
                case MessageType.Error:
                    GLib.GException ex;
                    string debug;
                    message.ParseError(out ex, out debug);
                    //Error?.Invoke(this, ex, debug);
                    break;
                case MessageType.Eos:
                    //EndOfStream?.Invoke(this);
                    break;
                case MessageType.StateChanged:
                    State oldState, newState, pendingState;
                    message.ParseStateChanged(out oldState, out newState, out pendingState);
                    //StateChanged?.Invoke(this, oldState, newState, pendingState);
                    break;
            }

            //Message?.Invoke(this, message);
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            _pipeline.SetState(State.Null);
        }
    }
}