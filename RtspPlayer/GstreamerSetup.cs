using System;
using System.IO;
using System.Runtime.InteropServices;

public static class GStreamerSetup
{
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetDllDirectory(string lpPathName);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool AddDllDirectory(string lpPathName);

    public static void SetGSTPath(string gstdir)
    {
        if (string.IsNullOrEmpty(gstdir) || !Directory.Exists(gstdir))
            return;

        string gstBin = Path.Combine(gstdir, "bin");
        string gstLib = Path.Combine(gstdir, "lib");

        // Ensure the paths exist
        if (!Directory.Exists(gstBin) || !Directory.Exists(gstLib))
            return;

        // Add paths to environment variables
        var path = Environment.GetEnvironmentVariable("PATH") ?? "";
        if (!path.Contains(gstBin))
        {
            path = $"{gstBin};{gstLib};{path}";
            Environment.SetEnvironmentVariable("PATH", path);
        }

        Environment.SetEnvironmentVariable("GSTREAMER_ROOT", gstdir);
        Environment.SetEnvironmentVariable("GSTREAMER_1_0_ROOT_X86_64", gstdir);
        Environment.SetEnvironmentVariable("GST_PLUGIN_PATH", gstLib);
        Environment.SetEnvironmentVariable("GST_DEBUG_DUMP_DOT_DIR", Path.GetTempPath());

        // Load libraries explicitly
        try
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                SetDllDirectory(gstBin);
                AddDllDirectory(gstBin);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error setting DLL path: {ex.Message}");
        }
    }
}
