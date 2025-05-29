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
using System.Management;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.DirectoryServices.ActiveDirectory;


namespace RtspPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string pipelineString11 = "rtspsrc location=rtsp://admin:123456@172.17.30.240/stream1 " +
                        "latency=1000 protocols=GST_RTSP_LOWER_TRANS_UDP drop-on-latency=1 ! " +
                        "queue max-size-buffers=60 leaky=downstream ! " +
                        "rtph265depay ! h265parse ! avdec_h265  ! videoconvert ! videorate skip-to-first=true ! " +
                        "video/x-raw,format=RGB ! appsink name=outsink sync=false max-buffers=60 drop=true";
        string pipelineString12 = "rtspsrc location=rtsp://admin:123456@172.17.30.241/stream1 " +
                        "latency=1000 protocols=GST_RTSP_LOWER_TRANS_TCP drop-on-latency=1 ! " +
                        "queue max-size-buffers=60 leaky=downstream ! " +
                        "rtph265depay ! h265parse ! avdec_h265 ! videoconvert ! videorate skip-to-first=true ! video/x-raw,format=RGB ! appsink name=outsink sync=false max-buffers=60 drop=true";
        string pipelineString13 = "rtspsrc location=rtsp://admin:123456@172.17.30.242/stream1 " +
                        "latency=1000 protocols=GST_RTSP_LOWER_TRANS_UDP drop-on-latency=1 ! " +
                        "queue max-size-buffers=60 leaky=downstream ! " +
                        "rtph265depay ! h265parse ! avdec_h265 ! videoconvert ! videorate skip-to-first=true ! video/x-raw,format=RGB ! appsink name=outsink sync=false max-buffers=60 drop=true";
        string pipelineString14 = "rtspsrc location=rtsp://admin:123456@172.17.30.243/stream1 " +
                        "latency=1000 protocols=GST_RTSP_LOWER_TRANS_TCP drop-on-latency=1 ! " +
                        "queue max-size-buffers=60 leaky=downstream ! " +
                        "rtph265depay ! h265parse ! avdec_h265 ! videoconvert ! videorate skip-to-first=true ! video/x-raw,format=RGB ! appsink name=outsink sync=false max-buffers=60 drop=true";
        string pipelineString15 = "rtspsrc location=rtsp://admin:123456@172.17.30.244/stream1 " +
                        "latency=1000 protocols=GST_RTSP_LOWER_TRANS_UDP drop-on-latency=1 ! " +
                        "queue max-size-buffers=60 leaky=downstream ! " +
                        "rtph265depay ! h265parse ! avdec_h265 ! videoconvert ! videorate skip-to-first=true ! video/x-raw,format=RGB ! appsink name=outsink sync=false max-buffers=60 drop=true";
        string pipelineString16 = "rtspsrc location=rtsp://admin:123456@172.17.30.245/stream1 " +
                        "latency=1000 protocols=GST_RTSP_LOWER_TRANS_TCP drop-on-latency=1 ! " +
                        "queue max-size-buffers=60 leaky=downstream ! " +
                        "rtph265depay ! h265parse ! avdec_h265 ! videoconvert ! videorate skip-to-first=true ! video/x-raw,format=RGB ! appsink name=outsink sync=false max-buffers=60 drop=true";
        string pipelineString17 = "rtspsrc location=rtsp://admin:123456@172.17.30.246/stream1 " +
                        "latency=1000 protocols=GST_RTSP_LOWER_TRANS_UDP drop-on-latency=1 ! " +
                        "queue max-size-buffers=60 leaky=downstream ! " +
                        "rtph265depay ! h265parse ! avdec_h265 ! videoconvert ! videorate skip-to-first=true ! video/x-raw,format=RGB ! appsink name=outsink sync=false max-buffers=60 drop=true";
        string pipelineString18 = "rtspsrc location=rtsp://admin:123456@172.17.30.247/stream1 " +
                        "latency=1000 protocols=GST_RTSP_LOWER_TRANS_TCP drop-on-latency=1 ! " +
                        "queue max-size-buffers=60 leaky=downstream ! " +
                        "rtph265depay ! h265parse ! avdec_h265 ! videoconvert ! videorate skip-to-first=true ! video/x-raw,format=RGB ! appsink name=outsink sync=false max-buffers=60 drop=true";


        string pipelineString_notUse = "rtspsrc location=rtsp://172.17.30.100:554/chID=3&streamType=main " +
                        "latency=300 protocols=GST_RTSP_LOWER_TRANS_UDP drop-on-latency=1 ! " +
                        "queue max-size-buffers=20 leaky=downstream ! " +
                        "rtph265depay ! avdec_h265 ! videoconvert ! video/x-raw,format=RGB ! appsink name=outsink sync=false max-buffers=20 drop=true";

        PipelineRtsp pipelineRtsp1 = new PipelineRtsp();
        PipelineRtsp pipelineRtsp2 = new PipelineRtsp();
        PipelineRtsp pipelineRtsp3 = new PipelineRtsp();
        PipelineRtsp pipelineRtsp4 = new PipelineRtsp();
        PipelineRtsp pipelineRtsp5 = new PipelineRtsp();
        PipelineRtsp pipelineRtsp6 = new PipelineRtsp();
        PipelineRtsp pipelineRtsp7 = new PipelineRtsp();
        PipelineRtsp pipelineRtsp8 = new PipelineRtsp();

        GstreamerPlayer _mainStreamNow;

        private NetworkInterface[] _previousInterfaces;

        private ManagementEventWatcher _watcher;

        private string[] TopingIps = { "172.17.30.240", "172.17.30.241", "172.17.30.242", "172.17.30.243",
            "172.17.30.244", "172.17.30.245", "172.17.30.246", "172.17.30.247", "172.17.30.100" };

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

        private PingViaInterface pinger;
        private PingViaInterface pinger2;
        private FailoverRouteManager failoverRouteManager = new FailoverRouteManager();

        public MainWindow()
        {
            InitializeComponent();
            iniPipelineString();
            Screenini();

            //ping ini

            NetworkInterfaceManager networkInterfaceManager = new NetworkInterfaceManager();

            _previousInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            ToPingComboBox.ItemsSource = TopingIps;
            CbbToOff.ItemsSource = allToOff;
            NetworkChange.NetworkAddressChanged += OnNetworkAddressChanged;

        }
        private void iniPipelineString()
        {
            if(!Properties.Settings.Default.Setting1.Equals(""))
                pipelineString11 = Properties.Settings.Default.Setting1;
            if (!Properties.Settings.Default.Setting2.Equals(""))
                pipelineString12 = Properties.Settings.Default.Setting2;
            if (!Properties.Settings.Default.Setting3.Equals(""))
                pipelineString13 = Properties.Settings.Default.Setting3;
            if (!Properties.Settings.Default.Setting4.Equals(""))
                pipelineString14 = Properties.Settings.Default.Setting4;
            if (!Properties.Settings.Default.Setting5.Equals(""))
                pipelineString15 = Properties.Settings.Default.Setting5;
            if (!Properties.Settings.Default.Setting6.Equals(""))
                pipelineString16 = Properties.Settings.Default.Setting6;
            if (!Properties.Settings.Default.Setting7.Equals(""))
                pipelineString17 = Properties.Settings.Default.Setting7;
            if (!Properties.Settings.Default.Setting8.Equals(""))
                pipelineString18 = Properties.Settings.Default.Setting8;

            if (!Properties.Settings.Default.Setting1.Equals(""))
                pipelineRtsp1.ParsePipelineString(Properties.Settings.Default.Setting1);
            if (!Properties.Settings.Default.Setting2.Equals(""))
                pipelineRtsp2.ParsePipelineString(Properties.Settings.Default.Setting2);
            if (!Properties.Settings.Default.Setting3.Equals(""))
                pipelineRtsp3.ParsePipelineString(Properties.Settings.Default.Setting3);
            if (!Properties.Settings.Default.Setting4.Equals(""))
                pipelineRtsp4.ParsePipelineString(Properties.Settings.Default.Setting4);
            if (!Properties.Settings.Default.Setting5.Equals(""))
                pipelineRtsp5.ParsePipelineString(Properties.Settings.Default.Setting5);
            if (!Properties.Settings.Default.Setting6.Equals(""))
                pipelineRtsp6.ParsePipelineString(Properties.Settings.Default.Setting6);
            if (!Properties.Settings.Default.Setting7.Equals(""))
                pipelineRtsp7.ParsePipelineString(Properties.Settings.Default.Setting7);
            if (!Properties.Settings.Default.Setting8.Equals(""))
                pipelineRtsp8.ParsePipelineString(Properties.Settings.Default.Setting8);
        }

        private PingViaInterface StartPinger(string dest,string face,TextBlock _PingOutput)
        {
            PingViaInterface _pinger;
            
            _pinger = new PingViaInterface(dest, face, 500);
            _pinger.PingStatusChanged += (s, success) =>
            {
                Dispatcher.Invoke(() =>
                {
                    _PingOutput.Text = success ? "Ping Success" : "Ping Failed";
                    _PingOutput.Background = success ? Brushes.Green : Brushes.Red;
                });
            };

            _pinger.Start();
            return _pinger;
        }



        private void LoadNetworkInterfaces(string num)
        {
            var interfaces = new List<NetworkInterfaceInfo>();

            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up)
                    continue;

                var ipProps = ni.GetIPProperties();
                var ipv4 = ipProps.UnicastAddresses
                                  .FirstOrDefault(ip => ip.Address.AddressFamily == AddressFamily.InterNetwork);

                if (ipv4 != null)
                {
                    interfaces.Add(new NetworkInterfaceInfo
                    {
                        Name = ni.Name,
                        IPAddress = ipv4.Address.ToString()
                    });
                }
            }
            if (num.Equals("1"))
            {

                InterfaceComboBox.ItemsSource = interfaces;
            }else if (num.Equals("2"))
            {

                InterfaceComboBox2.ItemsSource = interfaces;
            }
            //if (interfaces.Count > 0)
            //InterfaceComboBox.SelectedIndex = 0;
        }

        private void Screenini()
        {
            // Set main stream
            MainStreamNow = Screen1;
            pipelineString11 = ChangeToMain(pipelineString11);

            if (!string.IsNullOrEmpty(Properties.Settings.Default.Setting1))
            {
                Screen1.PipelineText = Properties.Settings.Default.Setting1;
            }
            else
            {
                Screen1.PipelineText = pipelineString11;
            }
            Screen1.PipelineText = "{rtspsrc location=rtsp://admin:123456@172.17.30.240/stream0 latency=10 protocols=GST_RTSP_LOWER_TRANS_UDP drop-on-latency=1 " +
                "! queue max-size-buffers=10 leaky=downstream " +
                "! rtph264depay ! h264parse ! avdec_h264 ! videoconvert " +
                "! videorate skip-to-first=true ! video/x-raw,format=RGB " +
                "! appsink name=outsink sync=false max-buffers=10 drop=true}";
            Screen1.StartGst();
            Screen1.SetTag("1");
            GstSet1.AssignGstPlayer(Screen1, "1");
            GstSet1.ParsePipelineString(Screen1.PipelineText);

            if (!string.IsNullOrEmpty(Properties.Settings.Default.Setting2))
            {
                Screen2.PipelineText = Properties.Settings.Default.Setting2;
            }
            else
            {
                Screen2.PipelineText = pipelineString12;
            }
            Screen2.StartGst();
            Screen2.SetTag("2");
            GstSet2.AssignGstPlayer(Screen2, "2");
            GstSet2.ParsePipelineString(Screen2.PipelineText);

            if (!string.IsNullOrEmpty(Properties.Settings.Default.Setting3))
            {
                Screen3.PipelineText = Properties.Settings.Default.Setting3;
            }
            else
            {
                Screen3.PipelineText = pipelineString13;
            }
            Screen3.StartGst();
            Screen3.SetTag("3");
            GstSet3.AssignGstPlayer(Screen3, "3");
            GstSet3.ParsePipelineString(Screen3.PipelineText);

            if (!string.IsNullOrEmpty(Properties.Settings.Default.Setting4))
            {
                Screen4.PipelineText = Properties.Settings.Default.Setting4;
            }
            else
            {
                Screen4.PipelineText = pipelineString14;
            }
            Screen4.StartGst();
            Screen4.SetTag("4");
            GstSet4.AssignGstPlayer(Screen4, "4");
            GstSet4.ParsePipelineString(Screen4.PipelineText);

            if (!string.IsNullOrEmpty(Properties.Settings.Default.Setting5))
            {
                Screen5.PipelineText = Properties.Settings.Default.Setting5;
            }
            else
            {
                Screen5.PipelineText = pipelineString15;
            }
            Screen5.StartGst();
            Screen5.SetTag("5");
            GstSet5.AssignGstPlayer(Screen5, "5");
            GstSet5.ParsePipelineString(Screen5.PipelineText);

            if (!string.IsNullOrEmpty(Properties.Settings.Default.Setting6))
            {
                Screen6.PipelineText = Properties.Settings.Default.Setting6;
            }
            else
            {
                Screen6.PipelineText = pipelineString16;
            }
            Screen6.StartGst();
            Screen6.SetTag("6");
            GstSet6.AssignGstPlayer(Screen6, "6");
            GstSet6.ParsePipelineString(Screen6.PipelineText);

            if (!string.IsNullOrEmpty(Properties.Settings.Default.Setting7))
            {
                Screen7.PipelineText = Properties.Settings.Default.Setting7;
            }
            else
            {
                Screen7.PipelineText = pipelineString17;
            }
            Screen7.StartGst();
            Screen7.SetTag("7");
            GstSet7.AssignGstPlayer(Screen7, "7");
            GstSet7.ParsePipelineString(Screen7.PipelineText);

            if (!string.IsNullOrEmpty(Properties.Settings.Default.Setting8))
            {
                Screen8.PipelineText = Properties.Settings.Default.Setting8;
            }
            else
            {
                Screen8.PipelineText = pipelineString18;
            }
            Screen8.StartGst();
            Screen8.SetTag("8");
            GstSet8.AssignGstPlayer(Screen8, "8");
            GstSet8.ParsePipelineString(Screen8.PipelineText);
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

        int TagStatus = 1;
        private void btn_HideTag_Click(object sender, RoutedEventArgs e)
        {
            TagStatus += 1;
            TagStatus = TagStatus % 2;
            bool Tagboo = TagStatus == 1;
            Screen1.SetTagShow(Tagboo);
            Screen2.SetTagShow(Tagboo);
            Screen3.SetTagShow(Tagboo);
            Screen4.SetTagShow(Tagboo);
            Screen5.SetTagShow(Tagboo);
            Screen6.SetTagShow(Tagboo);
            Screen7.SetTagShow(Tagboo);
            Screen8.SetTagShow(Tagboo);
        }

        private void OnNetworkAddressChanged(object sender, EventArgs e)
        {
            var currentInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (var nic in currentInterfaces)
            {
                var previous = _previousInterfaces.FirstOrDefault(p => p.Id == nic.Id);

                // Check for status change (e.g., Up <-> Down)
                if (previous != null && previous.OperationalStatus != nic.OperationalStatus)
                {
                    Dispatcher.Invoke(() =>
                    {
                        OnInterfaceConnectionChanged(nic, previous.OperationalStatus, nic.OperationalStatus);
                    });
                }
            }

            _previousInterfaces = currentInterfaces;
        }

        private void OnInterfaceConnectionChanged(NetworkInterface nic, OperationalStatus oldStatus, OperationalStatus newStatus)
        {
            string message = $"Interface '{nic.Name}' changed from {oldStatus} to {newStatus}";
            //MessageBox.Show(message);
            // Optionally log or react here
            Screen1.Refresh();
            Screen2.Refresh();
            Screen3.Refresh();
            Screen4.Refresh();
            Screen5.Refresh();
            Screen6.Refresh();
            Screen7.Refresh();
            Screen8.Refresh();
        }
        

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _watcher?.Stop();
            _watcher?.Dispose();
            Properties.Settings.Default.Setting1 = Screen1.PipelineText;
            Properties.Settings.Default.Setting2 = Screen2.PipelineText;
            Properties.Settings.Default.Setting3 = Screen3.PipelineText;
            Properties.Settings.Default.Setting4 = Screen4.PipelineText;
            Properties.Settings.Default.Setting5 = Screen5.PipelineText;
            Properties.Settings.Default.Setting6 = Screen6.PipelineText;
            Properties.Settings.Default.Setting7 = Screen7.PipelineText;
            Properties.Settings.Default.Setting8 = Screen8.PipelineText;
            Properties.Settings.Default.Save();
        }

        private void InterfaceComboBox_Selected(object sender, EventArgs e)
        {
            LoadNetworkInterfaces("1");
        }

        private bool Con1Ping = false;
        private bool Con2Ping = false;

        private void InterfaceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pinger != null) { 
                pinger.Dispose();
                pinger = null;
            }
            var comboBox = sender as ComboBox;
            if (comboBox?.SelectedItem is NetworkInterfaceInfo selectedInterface)
            {
                pinger = StartPinger(ipToping, selectedInterface.IPAddress, PingOutput);
                pinger.PingStatusChanged += (s, success) =>
                { 
                    Con1Ping = success;
                    con1 = selectedInterface;
                    CheckFailOver();
                };
            }
        }
        private NetworkInterfaceInfo con1;
        private NetworkInterfaceInfo con2;
        private string ipToping = "172.17.30.242";
        private void InterfaceComboBox2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pinger2 != null) { 
                pinger2.Dispose();
                pinger2 = null;
            }
            var comboBox = sender as ComboBox;
            if (comboBox?.SelectedItem is NetworkInterfaceInfo selectedInterface)
            {
                pinger2 = StartPinger(ipToping, selectedInterface.IPAddress, PingOutput2);
                pinger2.PingStatusChanged += (s, success) =>
                {
                    Con2Ping = success;
                    con2 = selectedInterface;
                    CheckFailOver();
                };
            }

        }




        private int prestate = 0;

        private System.DateTime _lastSwitchTime = System.DateTime.MinValue;
        private readonly TimeSpan _minimumInterval = TimeSpan.FromSeconds(2);
        private int sleepTime = 500;

        private bool Ismain = true;

        private void SwitchToMainManual(string Dest, string mask, string gate, int matsmol, int matBig, NetworkInterfaceInfoForRoutes route1, NetworkInterfaceInfoForRoutes route2)
        {
            if(route1 != null && route2 != null)
            {

            }
            else
            {
                return;
            }

                failoverRouteManager.DeleteRoute(Dest);
            failoverRouteManager.AddOrUpdateRoute(Dest, mask, gate, matsmol, route1.InterfaceIndex);
            failoverRouteManager.AddOrUpdateRoute(Dest, mask, gate, matBig, route2.InterfaceIndex);
           
            failoverRouteManager.DisableInterface(route2.InterfaceName);
            System.Threading.Thread.Sleep(sleepTime); // Blocks the thread for 200 milliseconds
            failoverRouteManager.EnableInterface(route2.InterfaceName);
            failoverRouteManager.AddOrUpdateRoute(Dest, mask, gate, matsmol, route1.InterfaceIndex);
            failoverRouteManager.AddOrUpdateRoute(Dest, mask, gate, matBig, route2.InterfaceIndex);
            failoverRouteManager.DeleteCache();

            System.Threading.Thread.Sleep(sleepTime);
            Screen1.Refresh();
            Screen2.Refresh();
            Screen3.Refresh();
            Screen4.Refresh();
            Screen5.Refresh();
            Screen6.Refresh();
            Screen7.Refresh();
            Screen8.Refresh();
        }


        private void SwitchToMain(string Dest,string mask,string gate ,int matsmol,int matBig, NetworkInterfaceInfoForRoutes route1, NetworkInterfaceInfoForRoutes route2) 
        {
            if ((System.DateTime.Now - _lastSwitchTime) < _minimumInterval)
                return;

            _lastSwitchTime = System.DateTime.Now;

            Ismain = true;
            SwitchToMainManual(Dest, mask, "0.0.0.0", matsmol, matBig, route1, route2);
        }

        private void SwitchToSecondManual(string Dest, string mask, string gate, int matsmol, int matBig, NetworkInterfaceInfoForRoutes route1, NetworkInterfaceInfoForRoutes route2)
        {
            if (route1 != null && route2 != null)
            {

            }
            else
            {
                return;
            }

            failoverRouteManager.DeleteRoute(Dest);
            failoverRouteManager.AddOrUpdateRoute(Dest, mask, gate, matsmol, route2.InterfaceIndex);
            failoverRouteManager.AddOrUpdateRoute(Dest, mask, gate, matBig, route1.InterfaceIndex);
            failoverRouteManager.DisableInterface(route1.InterfaceName);
            System.Threading.Thread.Sleep(sleepTime); // Blocks the thread for 200 milliseconds
            failoverRouteManager.EnableInterface(route1.InterfaceName);
            failoverRouteManager.AddOrUpdateRoute(Dest, mask, gate, matsmol, route2.InterfaceIndex);
            failoverRouteManager.AddOrUpdateRoute(Dest, mask, gate, matBig, route1.InterfaceIndex);
            failoverRouteManager.DeleteCache();

            System.Threading.Thread.Sleep(sleepTime);
            Screen1.Refresh();
            Screen2.Refresh();
            Screen3.Refresh();
            Screen4.Refresh();
            Screen5.Refresh();
            Screen6.Refresh();
            Screen7.Refresh();
            Screen8.Refresh();
        }


        private void SwitchToSecond(string Dest, string mask, string gate, int matsmol, int matBig, NetworkInterfaceInfoForRoutes route1, NetworkInterfaceInfoForRoutes route2)
        {
            if ((System.DateTime.Now - _lastSwitchTime) < _minimumInterval)
                return;

            _lastSwitchTime = System.DateTime.Now;

            Ismain = false;
            SwitchToSecondManual(Dest, mask, "0.0.0.0", matsmol, matBig, route1, route2);
        }

        private void CheckFailOver()
        {
            if (!DoAutoSwitch) { return; }
            NetworkInterfaceManager manager = new NetworkInterfaceManager();
            NetworkInterfaceInfoForRoutes route1 = null;
            NetworkInterfaceInfoForRoutes route2 = null;

            string mask = "255.255.255.0";
            string Dest = "172.17.30.0";
            string gate = "0.0.0.0";
            int matBig = 40;
            int matsmol = 1;
            if (con1 != null) 
            {
                route1 = manager.GetByIpAddress(con1.IPAddress);
            }
            else
            {
                return;
            }
            if (con2 != null)
            {

                route2 = manager.GetByIpAddress(con2.IPAddress);
            }
            else
            {
                return;
            }
            if (Con1Ping == true && Con2Ping == true)//11
            {
                switch (prestate)
                {
                    case 3://11 to 11
                        //donothing
                        if (!Ismain)
                        {
                            if (route1 != null && route2 != null)
                                SwitchToMain(Dest, mask, gate, matsmol, matBig, route1, route2);
                        }
                        break;
                    case 2://10 to 11
                        //donothing
                        break;
                    case 1:// 01 to 11
                           //change to main
                        if (route1 != null && route2 != null)
                        {
                            SwitchToMain(Dest, mask, gate, matsmol, matBig, route1, route2);
                        }
                        break;
                    case 0:// 00 to 11
                        //change to main
                        if (route1 != null && route2 != null)
                        {
                            SwitchToMain(Dest, mask, gate, matsmol, matBig, route1, route2);
                        }
                        break;
                }
                prestate = 3;
            }
            if (Con1Ping == true && Con2Ping != true)//10
            {
                switch (prestate)
                {
                    case 3://11 to 10
                        //no
                        break;
                    case 2://10 to 10
                           //no
                        if (!Ismain)
                        {
                            if (route1 != null && route2 != null)
                                SwitchToMain(Dest, mask, gate, matsmol, matBig, route1, route2);
                        }
                        break;
                    case 1:// 01 to 10
                        //main
                        if (route1 != null && route2 != null)
                        {
                            SwitchToMain(Dest, mask, gate, matsmol, matBig, route1, route2);
                        }
                        break;
                    case 0:// 00 to 10
                        //main
                        if (route1 != null && route2 != null)
                        {
                            SwitchToMain(Dest, mask, gate, matsmol, matBig, route1, route2);
                        }
                        break;
                }
                prestate = 2;
            }
            if (Con1Ping != true && Con2Ping == true)//01
            {
                switch (prestate)
                {
                    case 3://11 to 01
                        //sec
                        if (route1 != null && route2 != null)
                        {
                            SwitchToSecond(Dest, mask, gate, matsmol, matBig, route1, route2);
                        }
                        break;
                    case 2://10 to 01
                        //sec
                        if (route1 != null && route2 != null)
                        {
                            SwitchToSecond(Dest, mask, gate, matsmol, matBig, route1, route2);
                        }
                        break;
                    case 1:// 01 to 01
                           //no
                        if (Ismain)
                        {
                            if (route1 != null && route2 != null)
                                SwitchToSecond(Dest, mask, gate, matsmol, matBig, route1, route2);
                        }
                        break;
                    case 0:// 00 to 01
                        //sec
                        if (route1 != null && route2 != null)
                        {
                            SwitchToSecond(Dest, mask, gate, matsmol, matBig, route1, route2);
                        }
                        break;
                }
                prestate = 1;
            }
            if (Con1Ping != true && Con2Ping != true)//00
            {
                switch (prestate)
                {
                    case 3://11 to 00
                        //no
                        break;
                    case 2://10 to 00
                        //no
                        break;
                    case 1:// 01 to 00
                        //no
                        break;
                    case 0:// 00 to 00
                        //no
                        break;
                }
                prestate = 0;
            }
        }

        private void InterfaceComboBox2_Selected(object sender, EventArgs e)
        {
            LoadNetworkInterfaces("2");
        }

        private void ToPingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox?.SelectedItem is string selectedIP)
            {
                ipToping = (string)comboBox.SelectedItem;
                if (pinger != null)
                {
                    pinger.Dispose();
                    pinger = null;
                }
                
                if (con1 is NetworkInterfaceInfo selectedInterface)
                {
                    pinger = StartPinger(selectedIP, selectedInterface.IPAddress, PingOutput);
                    pinger.PingStatusChanged += (s, success) =>
                    {
                        Con1Ping = success;
                        con1 = selectedInterface;
                        CheckFailOver();
                    };
                }

                if (pinger2 != null)
                {
                    pinger2.Dispose();
                    pinger2 = null;
                }
                
                if (con2 is NetworkInterfaceInfo selectedInterface2)
                {
                    pinger2 = StartPinger(selectedIP, selectedInterface2.IPAddress, PingOutput2);
                    pinger2.PingStatusChanged += (s, success) =>
                    {
                        Con2Ping = success;
                        con2 = selectedInterface2;
                        CheckFailOver();
                    };
                }
            }
        }

        private bool DoAutoSwitch;

        private void ChkAutoSwitch_Checked(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() => {
                if (sender is CheckBox checkBox)
                {
                    DoAutoSwitch = checkBox.IsChecked == true;
                }
            });
        }
        private void ChkAutoSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() => {
                if (sender is CheckBox checkBox)
                {
                    DoAutoSwitch = checkBox.IsChecked == true;
                }
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NetworkInterfaceManager manager = new NetworkInterfaceManager();
            NetworkInterfaceInfoForRoutes route1 = null;
            NetworkInterfaceInfoForRoutes route2 = null;

            string mask = "255.255.255.0";
            string Dest = "172.17.30.0";
            int matBig = 40;
            int matsmol = 1;
            //change to ch1
            if (con1 != null)
            {
                route1 = manager.GetByIpAddress(con1.IPAddress);
            }
            else
            {
                return;
            }
            if (con2 != null)
            {

                route2 = manager.GetByIpAddress(con2.IPAddress);
            }
            else
            {
                return;
            }
            SwitchToMainManual(Dest, mask, "0.0.0.0", matsmol, matBig, route1, route2);

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            NetworkInterfaceManager manager = new NetworkInterfaceManager();
            NetworkInterfaceInfoForRoutes route1 = null;
            NetworkInterfaceInfoForRoutes route2 = null;

            string mask = "255.255.255.0";
            string Dest = "172.17.30.0";
            int matBig = 40;
            int matsmol = 1;
            //change to ch1
            if (con1 != null)
            {
                route1 = manager.GetByIpAddress(con1.IPAddress);
            }
            else
            {
                return;
            }
            if (con2 != null)
            {

                route2 = manager.GetByIpAddress(con2.IPAddress);
            }
            else
            {
                return;
            }
            SwitchToSecondManual(Dest, mask, "0.0.0.0", matsmol, matBig, route1, route2);
        }
        string ToOff = "";
        string[] allToOff = new string[] { "1","2","3","4","5","6","7","8"};
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            //off
            switch (ToOff)
            {
                case "1":
                Screen1.Off();
                break;
                case "2":
                Screen2.Off();
                break;
                case "3":
                Screen3.Off();
                break;
                case "4":
                Screen4.Off();
                break;
                case "5":
                Screen5.Off();
                break;
                case "6":
                Screen6.Off();
                break;
                case "7":
                Screen7.Off();
                break;
                case "8":
                Screen8.Off();
                break;
                    
            }
        }

        private void CbbToOff_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox?.SelectedItem is string To_Off) 
            {
                ToOff = To_Off;
            }
        }
    }

    public class NetworkInterfaceInfo
    {
        public string Name { get; set; }
        public string IPAddress { get; set; }

        public override string ToString()
        {
            return $"{Name} - {IPAddress}";
        }
    }
}