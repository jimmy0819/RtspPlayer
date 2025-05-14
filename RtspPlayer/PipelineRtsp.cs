using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RtspPlayer
{
    internal class PipelineRtsp
    {
        //string pipelineString11 = "rtspsrc location=rtsp://admin:123456@172.17.30.240/stream1 " +
        //                "latency=1000 protocols=GST_RTSP_LOWER_TRANS_UDP drop-on-latency=1 ! " +
        //                "queue max-size-buffers=60 leaky=downstream ! " +
        //                "rtph265depay ! h265parse ! avdec_h265  ! videoconvert ! videorate skip-to-first=true ! " +
        //                "video/x-raw,format=RGB ! appsink name=outsink sync=false max-buffers=60 drop=true";
        public string Location { get; set; }
        public int Latency { get; set; }
        public int DropOnLatency { get; set; }
        public Protocols Protocols { get; set; }
        public int MaxSizeBuffers { get; set; }
        public bool SkipToFirst { get; set; } = true;
        public bool Sync { get; set; } = false;
        public int MaxBuffers { get; set; }
        public bool Drop { get; set; }
        public bool JitterBuffer { get; set; } = false;
        public bool IsMainStream { get; set; } = false;


        public bool IsInitailized { get; set; } = false;

        public PipelineRtsp(string location,int latency,int droponlatency,Protocols protocols,int maxSizeBuffers,bool skipToFirst,bool sync,int maxBuffers,bool drop,bool jitterBuffer) 
        {
            Location = location;
            Latency = latency;
            DropOnLatency = droponlatency;
            Protocols = protocols;
            MaxSizeBuffers = maxSizeBuffers;
            SkipToFirst = skipToFirst;
            Sync = sync;
            MaxBuffers = maxBuffers;
            Drop = drop;
            JitterBuffer = jitterBuffer;
            IsInitailized = true;
        }
        public PipelineRtsp() { }
        public string GetPipelineString()
        {
            return $"rtspsrc location={Location} " +
                   $"latency={Latency} " +
                   $"protocols={Protocols.ToString()} " +
                   $"drop-on-latency={DropOnLatency} ! " +
                   $"{(JitterBuffer == true ?
                   $"rtpjitterbuffer latency={Latency} drop-on-latency={DropOnLatency} do-lost=true do-retransmission=true ! " 
                   : "")}" +
                   $"queue max-size-buffers={MaxBuffers} leaky=downstream ! " +
                   $"rtph265depay ! h265parse ! avdec_h265 ! videoconvert ! videorate skip-to-first={(SkipToFirst == true ? "true":"false")} ! video/x-raw,format=RGB ! " +
                   $"appsink name=outsink sync=false max-buffers={MaxBuffers} drop={(Drop == true? "true" : "false")}";
        }
        public void ParsePipelineString(string pipeline)
        {
            var parameters = pipeline.Split(' ');
            foreach (var param in parameters)
            {
                var keyValue = param.Split('=');

                if (keyValue.Length == 1)
                {
                    switch (keyValue[0])
                    {
                        case "rtpjitterbuffer":
                            JitterBuffer = true;
                            break;
                    }
                }

                if (keyValue.Length == 2)
                {
                    switch (keyValue[0])
                    {
                        case "location":
                            Location = keyValue[1];
                            if (keyValue[1].Contains("/stream0"))
                            {
                                IsMainStream = true;
                            }
                            else if (keyValue[1].Contains("/stream1"))
                            {
                                IsMainStream = false;
                            }
                            break;
                        case "latency":
                            Latency = Int32.Parse (keyValue[1]);
                            break;
                        case "protocols":
                            Protocols =(Protocols) Enum.Parse(typeof(Protocols), keyValue[1]);
                            break;
                        case "drop-on-latency":
                            DropOnLatency = Int32.Parse( keyValue[1]);
                            break;
                        case "max-size-buffers":
                            MaxSizeBuffers =Int32.Parse( keyValue[1]);
                            break;
                        case "max-buffers":
                            MaxBuffers =Int32.Parse( keyValue[1]);
                            break;
                        case "rtpjitterbuffer":
                            JitterBuffer = true;
                            break;
                    }
                }
                else if (keyValue.Length > 2)
                {
                    for (int i = 1; i < keyValue.Length; i++)
                    {

                        Location += keyValue[i];
                        if (i != keyValue.Length - 1)
                        {
                            //not last
                            Location += "=";
                        }
                    }
                }
            }
            IsInitailized = true;
        }

        public override string ToString()
        {
            return GetPipelineString();
        }
    }
    enum Protocols
    {
        GST_RTSP_LOWER_TRANS_UDP = 0,
        GST_RTSP_LOWER_TRANS_TCP = 1,
    }
}
