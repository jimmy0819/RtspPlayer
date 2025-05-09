using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RtspPlayer
{
    public class PingViaInterface : IDisposable
    {
        private readonly string targetIp;
        private readonly string interfaceIp;
        private readonly int intervalMs;
        private Timer timer;
        private bool disposed;

        public event EventHandler<string> PingResult;
        public event EventHandler<bool> PingStatusChanged;

        public PingViaInterface(string targetIp, string interfaceIp, int intervalMs = 500)
        {
            this.targetIp = targetIp;
            this.interfaceIp = interfaceIp;
            this.intervalMs = intervalMs;
        }

        public void Start()
        {
            if (timer == null)
            {
                timer = new Timer(PingCallback, null, 0, intervalMs);
            }
        }

        public void Stop()
        {
            timer?.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void PingCallback(object state)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "ping",
                    Arguments = $"-S {interfaceIp} {targetIp} -n 1",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    PingResult?.Invoke(this, output);
                    bool success = process.ExitCode == 0;
                    PingStatusChanged?.Invoke(this, success);
                }
            }
            catch (Exception ex)
            {
                PingResult?.Invoke(this, "Ping error: " + ex.Message);
                PingStatusChanged?.Invoke(this, false);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                Stop();
                timer?.Dispose();
                timer = null;

                // Optional: Clear subscribers if you want to help GC
                PingResult = null;
                PingStatusChanged = null;
            }

            disposed = true;
        }
    }

}
