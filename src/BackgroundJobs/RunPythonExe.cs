using GateEntryExit.Domain;
using Newtonsoft.Json;
using System.Diagnostics;


namespace GateEntryExit.BackgroundJobs
{
    public class RunPythonExe
    {
        public async Task<(bool isRunSuccessfully, bool hasAnyError)> ExecuteExe(bool gateOnly = false)
        {
            // Assume python exe run return an object like gate as of now
            var gate = new Gate();

            // Assume this script is in another project which is referenced in main project. Right click exe and set "CopyToOutputDirectory" as "CopyAlways".
            // If you have the exe in this folder(/script/dist/python.exe) in another project then when main project build this .exe will be copied in mainProject/bin/debug/net8.0/script/dist/python.exe.
            // mainProject/bin/debug/net8.0 is the path where mainProject.dll will be there

            string path = Path.Combine("script", "dist", "python.exe");
            string argument = $"--gateOnly {( gateOnly ? "false" : "true" )} ";
            var startInfo = CreateProcessStartInfo(path, argument);

            using (Process process = new Process { StartInfo = startInfo })
            {
                var newProcess = process.Start();
                var standardOutput = string.Empty;
                using (var reader = process.StandardOutput)
                {
                    string? line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        standardOutput = line.Trim();
                    }
                }
                string standardError = process.StandardError.ReadToEnd();

                process.WaitForExit();

                var outputs = new
                {
                    id = process.Id,
                    exitcode = process.ExitCode,
                    standardError,
                    standardOutput,
                    name = process.ToString(),
                    start = process.StartTime,
                    exit = process.ExitTime
                };

                if(string.IsNullOrWhiteSpace(standardError))
                    // do some logging of error

                try
                {
                    if (string.IsNullOrWhiteSpace(standardOutput) || standardOutput.Equals("null"))
                        throw new Exception($"Error. StandardOutput empty. {JsonConvert.SerializeObject(outputs)}");

                    gate = JsonConvert.DeserializeObject<Gate>(standardOutput) ?? throw new Exception($"Error, Gate desrialization returned null {JsonConvert.SerializeObject(outputs)}");
                }
                catch(Exception ex)
                {

                }
            }
            return (true, false);
        }

        private ProcessStartInfo CreateProcessStartInfo(string path, string argument)
        {
            path = Path.Combine(AppContext.BaseDirectory, path);

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = path,
                Arguments = argument,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            return startInfo;
        }
    }
}
