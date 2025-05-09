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

        private NetworkInterface[] _previousInterfaces;

        private ManagementEventWatcher _watcher;

        

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
            Screenini();

            //ping ini

            NetworkInterfaceManager networkInterfaceManager = new NetworkInterfaceManager();

            _previousInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            NetworkChange.NetworkAddressChanged += OnNetworkAddressChanged;

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

        private bool Con1Ping;
        private bool Con2Ping;

        private void InterfaceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pinger != null) { 
                pinger.Dispose();
                pinger = null;
            }
            var comboBox = sender as ComboBox;
            if (comboBox?.SelectedItem is NetworkInterfaceInfo selectedInterface)
            {
                pinger = StartPinger("172.17.30.242",selectedInterface.IPAddress, PingOutput);
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
        private void InterfaceComboBox2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pinger2 != null) { 
                pinger2.Dispose();
                pinger2 = null;
            }
            var comboBox = sender as ComboBox;
            if (comboBox?.SelectedItem is NetworkInterfaceInfo selectedInterface)
            {
                pinger2 = StartPinger("172.17.30.242", selectedInterface.IPAddress, PingOutput2);
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
        private readonly TimeSpan _minimumInterval = TimeSpan.FromSeconds(5);
        private int sleepTime = 500;

        private bool Ismain = true;
        private void SwitchToMain(string Dest,string mask,string gate ,int matsmol,int matBig, NetworkInterfaceInfoForRoutes route1, NetworkInterfaceInfoForRoutes route2) 
        {
            if ((System.DateTime.Now - _lastSwitchTime) < _minimumInterval)
                return;

            _lastSwitchTime = System.DateTime.Now;

            Ismain = true;
            //failoverRouteManager.DeleteRoute(Dest);
            //failoverRouteManager.AddOrUpdateRoute(Dest, mask, gate, matsmol, route1.InterfaceIndex);
            //failoverRouteManager.AddOrUpdateRoute(Dest, mask, gate, matBig, route2.InterfaceIndex);
            while (!Con1Ping)
            {
                System.Threading.Thread.Sleep(sleepTime);
            }
            failoverRouteManager.DisableInterface(route2.InterfaceName);
            System.Threading.Thread.Sleep(sleepTime); // Blocks the thread for 200 milliseconds
            failoverRouteManager.EnableInterface(route2.InterfaceName);
            //failoverRouteManager.DeleteCache();
        }
        private void SwitchToSecond(string Dest, string mask, string gate, int matsmol, int matBig, NetworkInterfaceInfoForRoutes route1, NetworkInterfaceInfoForRoutes route2)
        {
            if ((System.DateTime.Now - _lastSwitchTime) < _minimumInterval)
                return;

            _lastSwitchTime = System.DateTime.Now;

            Ismain = false;
            while (!Con2Ping)
            {
                System.Threading.Thread.Sleep(sleepTime);
            }
            //failoverRouteManager.DeleteRoute(Dest);
            //failoverRouteManager.AddOrUpdateRoute(Dest, mask, "172.17.30.1", matsmol, route2.InterfaceIndex);
            //failoverRouteManager.AddOrUpdateRoute(Dest, mask, "172.17.30.1", matBig, route1.InterfaceIndex);
            failoverRouteManager.DisableInterface(route1.InterfaceName);
            System.Threading.Thread.Sleep(sleepTime); // Blocks the thread for 200 milliseconds
            failoverRouteManager.EnableInterface(route1.InterfaceName);
            //failoverRouteManager.DeleteCache();
        }

        private void CheckFailOver()
        {
            NetworkInterfaceManager manager = new NetworkInterfaceManager();
            NetworkInterfaceInfoForRoutes route1 = null;
            NetworkInterfaceInfoForRoutes route2 = null;

            string mask = "255.255.255.255";
            string Dest = "172.17.30.242";
            int matBig = 40;
            int matsmol = 1;
            if (con1 != null) 
            {
                route1 = manager.GetByIpAddress(con1.IPAddress);
            }
            if(con2 != null)
            {

                route2 = manager.GetByIpAddress(con2.IPAddress);
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
                                SwitchToMain(Dest, mask, "172.17.30.1", matsmol, matBig, route1, route2);
                        }
                        break;
                    case 2://10 to 11
                        //donothing
                        break;
                    case 1:// 01 to 11
                           //change to main
                        if(route1 != null && route2 != null)
                        {
                            SwitchToMain(Dest, mask, "172.17.30.1", matsmol, matBig, route1, route2);
                        }
                        break;
                    case 0:// 00 to 11
                        //change to main
                        if (route1 != null && route2 != null)
                        {
                            SwitchToMain(Dest, mask, "172.17.30.1", matsmol, matBig, route1, route2);
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
                                SwitchToMain(Dest, mask, "172.17.30.1", matsmol, matBig, route1, route2);
                        }
                        break;
                    case 1:// 01 to 10
                        //main
                        if (route1 != null && route2 != null)
                        {
                            SwitchToMain(Dest, mask, "172.17.30.1", matsmol, matBig, route1, route2);
                        }
                        break;
                    case 0:// 00 to 10
                        //main
                        if (route1 != null && route2 != null)
                        {
                            SwitchToMain(Dest, mask, "172.17.30.1", matsmol, matBig, route1, route2);
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
                            SwitchToSecond(Dest, mask, "172.17.30.1", matsmol, matBig, route1, route2);
                        }
                        break;
                    case 2://10 to 01
                        //sec
                        if (route1 != null && route2 != null)
                        {
                            SwitchToSecond(Dest, mask, "172.17.30.1", matsmol, matBig, route1, route2);
                        }
                        break;
                    case 1:// 01 to 01
                           //no
                        if (Ismain)
                        {
                            if (route1 != null && route2 != null)
                                SwitchToSecond(Dest, mask, "172.17.30.1", matsmol, matBig, route1, route2);
                        }
                        break;
                    case 0:// 00 to 01
                        //sec
                        if (route1 != null && route2 != null)
                        {
                            SwitchToSecond(Dest, mask, "172.17.30.1", matsmol, matBig, route1, route2);
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