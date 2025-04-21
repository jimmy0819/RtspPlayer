using Gst.App;
using Gst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
using System.Windows.Interop;

namespace RtspPlayer
{
    /// <summary>
    /// Interaction logic for GstreamerPlayer.xaml
    /// </summary>
    public partial class GstreamerPlayer : UserControl
    {
        private Pipeline _pipeline;
        private Bus _bus;
        private AppSink _videoSink;
        private int _renderTimerFrequency = 30;
        private int _messageTimerFrequency = 30;
        private Timer _renderTimer = null;
        private Timer _messageTimer = null;
        private Timer _restartTimer = null;
        private bool IsSynchronized = false;
        private long _sampleLock = 0;
        private Element appSink;

        static string pipelineString10 = "rtspsrc location= rtsp://admin:123456@172.17.30.240/stream1 " +
                        "latency=100 protocols=GST_RTSP_LOWER_TRANS_UDP drop-on-latency=1 ! " +
                        //"rtpjitterbuffer latency=100 drop-on-latency=0 ! " +
                        "queue max-size-buffers=15 leaky=downstream ! " +
                        "rtph265depay ! avdec_h265 ! videoconvert ! video/x-raw,format=RGB ! autovideosink sync=false";
        static string pipelineString11 = "rtspsrc location= rtsp://admin:123456@172.17.30.240/stream1 " +
                        "latency=100 protocols=GST_RTSP_LOWER_TRANS_UDP drop-on-latency=1 ! " +
                        //"rtpjitterbuffer latency=100 drop-on-latency=0 ! " +
                        "queue max-size-buffers=15 leaky=downstream ! " +
                        "rtph265depay ! avdec_h265 ! videoconvert ! video/x-raw,format=RGB ! appsink name=outsink sync=false max-buffers=5 drop=true";

        public GstreamerPlayer()
        {
            InitializeComponent();
        }

        public void SetTag(String word)
        {
            TagWord.Text = word;
        }

        public void SetTagShow(bool show)
        {
            TagShow.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        }

        public static readonly DependencyProperty PipelineTextProperty =
            DependencyProperty.Register(
                "DisplayText",
                typeof(string),
                typeof(GstreamerPlayer),
                new FrameworkPropertyMetadata(
                    pipelineString11, // Default Value
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    OnPipelineTextChanged) // OnChange Callback
                );

        public string PipelineText
        {
            get { return (string)GetValue(PipelineTextProperty); }
            set { SetValue(PipelineTextProperty, value); }
        }

        public event Action<string> PipelineTextChanged;

        private static void OnPipelineTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as GstreamerPlayer;
            if (control != null)
            {
                // Invoke the PipelineTextChanged event with the new PipelineText value
                control.PipelineTextChanged?.Invoke((string)e.NewValue);
            }
        }

        

        public void SetPipeLine(string pipeLine)
        {
            Dispatcher.Invoke(() =>
            {
                PipelineText = pipeLine;
            });
            ClosePipe();
            InitializeGStreamer();
        }

        public void Refresh()
        {
            ClosePipe();
            InitializeGStreamer();
        }

        public void StartGst()
        {
            InitializeGStreamer();
        }

        public void InitializeGStreamer()
        {
            // Set GST_DEBUG level before initializing GStreamer
            Environment.SetEnvironmentVariable("GST_DEBUG", "4"); // Set verbosity level

            // Set GStreamer Path before initialization
            GStreamerSetup.SetGSTPath(@"C:\Program Files\gstreamer\1.0\mingw_x86_64");
            //C:\gstreamerIns\bin
            //GStreamerSetup.SetGSTPath(@"C:\Program Files\gstreamer\1.0\msvc_x86_64");

            Gst.Application.Init(); // Initialize GStreamer
            string PText = "";
            Dispatcher.Invoke(() =>
            {
                PText = PipelineText; // Reset or update safely
            });

            try
            {
                _pipeline = (Pipeline)Parse.Launch(PText);
                _bus = _pipeline.Bus;

                // Start playing
                InitializeAppSink("outsink");
                _pipeline.SetState(State.Playing);
            }
            catch (Exception e)
            {
                MessageBox.Show("Error initializing GStreamer: " + e.Message, "GStreamer Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                                    buffer.Dispose(); // 🔹 Explicitly release buffer
                            }
                            sample.Dispose(); // 🔹 Explicitly release sample
                            sampleValue.Dispose();
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
                if (videoImage.Source != null)
                {
                    // Release the old bitmap to free memory
                    var oldBitmap = videoImage.Source as BitmapSource;
                    videoImage.Source = null;
                }
                try
                {
                    videoImage.Source = bitmap;
                }
                catch (Exception ex) { }
            });
        }

        public void EnsurePipelinePlaying()
        {
            if (_pipeline == null)
                return; // Pipeline doesn't exist, nothing to do

            State currentState, pendingState;
            _pipeline.GetState(out currentState, out pendingState, Gst.Constants.CLOCK_TIME_NONE);

            if (currentState == State.Paused || pendingState == State.Paused)
            {
                Console.WriteLine("Pipeline is paused. Resuming playback...");
                _pipeline.SetState(State.Playing);
            }
            else
            {
                Console.WriteLine("Pipeline is already playing.");
            }
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
            Console.WriteLine(message.ToString());
            switch (message.Type)
            {
                case MessageType.Error:
                    GLib.GException ex;
                    string debug;
                    message.ParseError(out ex, out debug);
                    if (_pipeline != null)
                        ClosePipe();
                        InitializeGStreamer();
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

        public void ClosePipe()
        {
            if (_pipeline != null)
            {
                _renderTimer?.Dispose();
                _renderTimer = null;
                _messageTimer?.Dispose();
                _messageTimer = null;

                // Properly shut down the pipeline
                _pipeline.SetState(State.Null);

                // Wait for the state change to complete
                State current, pending;
                _pipeline.GetState(out current, out pending, Gst.Constants.CLOCK_TIME_NONE);
                while (current != State.Null)
                {
                    _pipeline.GetState(out current, out pending, Gst.Constants.CLOCK_TIME_NONE);
                }

                // Dispose of the bus
                _bus?.Dispose();
                _bus = null;

                // Dispose of the app sink
                appSink?.Dispose();
                appSink = null;

                // Now release resources
                _pipeline.Dispose();
                _pipeline = null;
                
            }

        }

    }
}
