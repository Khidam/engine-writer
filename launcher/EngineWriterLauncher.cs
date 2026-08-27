using System;
using System.Diagnostics;
using System.IO;

internal static class EngineWriterLauncher
{
    [STAThread]
    private static void Main()
    {
        string root = AppDomain.CurrentDomain.BaseDirectory;
        string html = Path.Combine(root, "EngineWriter.html");
        if (!File.Exists(html))
        {
            System.Windows.Forms.MessageBox.Show(
                "EngineWriter.html não foi encontrado ao lado do executável.",
                "Engine Writer",
                System.Windows.Forms.MessageBoxButtons.OK,
                System.Windows.Forms.MessageBoxIcon.Error);
            return;
        }

        string edge = FindEdge();
        if (!String.IsNullOrEmpty(edge))
        {
            string url = new Uri(html).AbsoluteUri;
            Process.Start(new ProcessStartInfo
            {
                FileName = edge,
                Arguments = "--app=\"" + url + "\" --start-maximized",
                UseShellExecute = false
            });
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = html,
            UseShellExecute = true
        });
    }

    private static string FindEdge()
    {
        string[] candidates = new string[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Microsoft", "Edge", "Application", "msedge.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Microsoft", "Edge", "Application", "msedge.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Edge", "Application", "msedge.exe")
        };

        for (int i = 0; i < candidates.Length; i++)
        {
            if (File.Exists(candidates[i])) return candidates[i];
        }
        return null;
    }
}
