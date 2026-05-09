using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Kolera_Mtsk.Services
{
    /// <summary>
    /// FastReport Designer (Community veya kurulu sürüm) yolunu bulup .frx dosyasını açar.
    /// </summary>
    public static class FastReportDesignerLauncher
    {
        public static bool TryOpenFrx(string frxYolu)
        {
            if (string.IsNullOrWhiteSpace(frxYolu) || !File.Exists(frxYolu))
                return false;

            try
            {
                foreach (string exe in EnumerateDesignerCandidates())
                {
                    if (string.IsNullOrWhiteSpace(exe) || !File.Exists(exe))
                        continue;

                    Process.Start(new ProcessStartInfo
                    {
                        FileName = exe,
                        Arguments = "\"" + frxYolu + "\"",
                        UseShellExecute = true
                    });
                    return true;
                }
            }
            catch
            {
            }

            return false;
        }

        private static IEnumerable<string> EnumerateDesignerCandidates()
        {
            string ayar = (ConfigurationManager.AppSettings["FastReportDesignerPath"] ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(ayar))
                yield return ayar;

            string baseDir = NormalizeDir(AppDomain.CurrentDomain.BaseDirectory);
            string asmDir = NormalizeDir(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

            foreach (string root in DistinctRoots(baseDir, asmDir))
            {
                yield return Path.Combine(root, "FastReport", "Designer.exe");
                yield return Path.Combine(root, "Designer.exe");
                yield return Path.Combine(root, "FastReport.Designer.exe");
            }

            yield return @"C:\Program Files\FastReports\FastReport.Net\FastReport.Designer.exe";
            yield return @"C:\Program Files (x86)\FastReports\FastReport.Net\FastReport.Designer.exe";
            yield return @"C:\Program Files\FastReport\FastReport Designer\FastReport.Designer.exe";
            yield return @"C:\Program Files (x86)\FastReport\FastReport Designer\FastReport.Designer.exe";
        }

        private static IEnumerable<string> DistinctRoots(string a, string b)
        {
            if (!string.IsNullOrEmpty(a))
                yield return a;
            if (!string.IsNullOrEmpty(b) &&
                !string.Equals(a, b, StringComparison.OrdinalIgnoreCase))
                yield return b;
        }

        private static string NormalizeDir(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return string.Empty;
            return path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
    }
}
