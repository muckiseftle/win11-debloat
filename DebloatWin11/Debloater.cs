using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;

namespace DebloatWin11
{
    public class Debloater
    {
        private readonly Action<string> _log;
        private readonly string _logPath;
        private readonly List<string> _hpKeywords = new() { "hp" };
        private readonly List<string> _msKeywords = new()
        {
            "microsoft 365", "onenote", "to do", "teams", "outlook",
            "clipchamp", "bing", "news", "solitaire", "xbox", "poly lens",
            "start experiences", "copilot"
        };

        public Debloater(Action<string> log)
        {
            _log = log;
            _logPath = Path.Combine(Path.GetTempPath(), "debloat.log");
            File.WriteAllText(_logPath, $"Log gestartet {DateTime.Now}\n");
        }

        public void RunAll()
        {
            _log("Suche nach Programmen...");
            var programs = FindInstalledPrograms();
            RemoveHP(programs);
            RemoveMicrosoft(programs);
            RemoveAppx();
            InstallApplications();
            _log($"Fertig. Log: {_logPath}");
        }

        private class ProgramEntry
        {
            public string DisplayName { get; set; } = string.Empty;
            public string? UninstallString { get; set; }
        }

        private IEnumerable<ProgramEntry> FindInstalledPrograms()
        {
            var keys = new[]
            {
                Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"),
                Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall")
            };
            foreach (var key in keys)
            {
                if (key == null) continue;
                foreach (var subName in key.GetSubKeyNames())
                {
                    using var sub = key.OpenSubKey(subName);
                    var name = sub?.GetValue("DisplayName") as string;
                    if (string.IsNullOrWhiteSpace(name)) continue;
                    yield return new ProgramEntry
                    {
                        DisplayName = name,
                        UninstallString = sub?.GetValue("UninstallString") as string
                    };
                }
            }
        }

        private void RemoveHP(IEnumerable<ProgramEntry> programs)
        {
            foreach (var p in programs.Where(p => _hpKeywords.Any(k => p.DisplayName.ToLower().StartsWith(k))))
            {
                UninstallProgram(p);
            }
        }

        private void RemoveMicrosoft(IEnumerable<ProgramEntry> programs)
        {
            foreach (var p in programs.Where(p => _msKeywords.Any(k => p.DisplayName.ToLower().Contains(k))))
            {
                UninstallProgram(p);
            }
        }

        private void UninstallProgram(ProgramEntry entry)
        {
            if (string.IsNullOrWhiteSpace(entry.UninstallString)) return;
            _log($"Deinstalliere {entry.DisplayName}");
            LogFile($"Deinstalliere {entry.DisplayName}");
            try
            {
                var cmd = entry.UninstallString;
                if (!cmd.Contains("/quiet")) cmd += " /quiet";
                RunCommand(cmd);
            }
            catch (Exception ex)
            {
                _log($"Fehler bei {entry.DisplayName}: {ex.Message}");
            }
        }

        private void RemoveAppx()
        {
            var keywords = string.Join("|", _msKeywords.Concat(_hpKeywords));
            var script = $"Get-AppxPackage -AllUsers | Where-Object {{$_.Name -match '{keywords}'}} | Remove-AppxPackage -AllUsers";
            RunPowerShell(script);
            var prov = $"Get-AppxProvisionedPackage -Online | Where-Object {{$_.DisplayName -match '{keywords}'}} | Remove-AppxProvisionedPackage -Online";
            RunPowerShell(prov);
        }

        private void InstallApplications()
        {
            InstallIfMissing("Mozilla Firefox", "https://download.mozilla.org/?product=firefox-latest&os=win64&lang=de", "/S");
            InstallIfMissing("Microsoft Edge", "https://msedge.sf.dl.delivery.mp.microsoft.com/", "/silent /install");
            InstallIfMissing("7-Zip", "https://www.7-zip.org/a/7z2201-x64.exe", "/S");
            InstallIfMissing("Adobe Acrobat", "https://acrobat.adobe.com/link/acrobat/download", "/sAll");
            InstallIfMissing("TeamViewer Host", "https://cloud.itm-technologies.de/s/n2YzC6dWgSNkmMK/download/TeamViewer_Host_Setup.exe", "/S");
        }

        private void InstallIfMissing(string name, string url, string arguments)
        {
            if (IsProgramInstalled(name))
            {
                _log($"{name} bereits installiert.");
                return;
            }
            _log($"Installiere {name}");
            LogFile($"Installiere {name} von {url}");
            var file = Path.Combine(Path.GetTempPath(), Path.GetFileName(new Uri(url).LocalPath));
            using (var client = new WebClient())
            {
                client.DownloadFile(url, file);
            }
            RunCommand($"\"{file}\" {arguments}");
        }

        private bool IsProgramInstalled(string name)
        {
            var programs = FindInstalledPrograms();
            return programs.Any(p => p.DisplayName.ToLower().Contains(name.ToLower()));
        }

        private void RunCommand(string command)
        {
            var psi = new ProcessStartInfo("cmd.exe", $"/c {command}") { CreateNoWindow = true, UseShellExecute = false };
            var process = Process.Start(psi);
            process.WaitForExit();
        }

        private void RunPowerShell(string script)
        {
            var psi = new ProcessStartInfo("powershell", $"-NoProfile -ExecutionPolicy Bypass -Command \"{script}\"")
            {
                CreateNoWindow = true,
                UseShellExecute = false
            };
            var p = Process.Start(psi);
            p.WaitForExit();
        }

        private void LogFile(string message)
        {
            File.AppendAllText(_logPath, message + Environment.NewLine);
        }
    }
}
