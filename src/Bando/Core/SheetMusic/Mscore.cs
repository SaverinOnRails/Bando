using System.Diagnostics;
using System;
using System.Text.RegularExpressions;
using System.IO;
namespace Bando.Core.SheetMusic;

public static class MuseScoreManager
{
    public static string Version()
    {
        try
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "mscore",
                Arguments = "--version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using (var process = Process.Start(processStartInfo))
            {
                if (process == null)
                {
                    throw new InvalidOperationException("Failed to start MuseScore process.");
                }
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                string fullOutput = output + error;
                var match = Regex.Match(fullOutput, @"MuseScore\d*\s+(\d+\.\d+(?:\.\d+)?)", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
                throw new InvalidOperationException("Could not parse MuseScore version from output.");
            }
        }
        catch
        {
            throw new FileNotFoundException("MuseScore is not installed or not found in PATH.");
        }
    }

    public static string FromMidiToMei(string midiFilePath)
    {
        if (!File.Exists(midiFilePath))
        {
            throw new FileNotFoundException($"MIDI file not found: {midiFilePath}");
        }
        string tempMeiFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.mei");
        try
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "mscore",
                Arguments = $"\"{midiFilePath}\" -o \"{tempMeiFile}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using (var process = Process.Start(processStartInfo))
            {
                if (process == null)
                {
                    throw new InvalidOperationException("Failed to start MuseScore process.");
                }

                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    string error = process.StandardError.ReadToEnd();
                    throw new InvalidOperationException($"MuseScore conversion failed with exit code {process.ExitCode}: {error}");
                }
            }
            System.Threading.Thread.Sleep(100);
            if (!File.Exists(tempMeiFile))
            {
                throw new InvalidOperationException("MuseScore failed to create the MEI file.");
            }
            string meiContent = File.ReadAllText(tempMeiFile);
            return meiContent;
        }
        catch
        {
            throw new FileNotFoundException("MuseScore is not installed or not found in PATH.");
        }
        finally
        {
            if (File.Exists(tempMeiFile))
            {
                try
                {
                    File.Delete(tempMeiFile);
                }
                catch
                {
                }
            }
        }
    }
}
