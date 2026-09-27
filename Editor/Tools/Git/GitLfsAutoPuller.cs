using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

public class GitLfsAutoPuller
{
    public static async Task RunGitLfsPull(string packagePath)
    {
        if(!Directory.Exists(packagePath))
        {
            UnityEngine.Debug.LogError($"[Git LFS] Path not found: {packagePath}");
            return;
        }
        ProcessStartInfo processInfo = new ProcessStartInfo(GetGitPath(), "lfs pull")
        {
            WorkingDirectory = packagePath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
#if UNITY_EDITOR_OSX
        processInfo.EnvironmentVariables["PATH"] = "/opt/homebrew/bin:" + Environment.GetEnvironmentVariable("PATH");
        processInfo.EnvironmentVariables["HOME"] = Environment.GetEnvironmentVariable("HOME");
#endif
        using(Process process = Process.Start(processInfo))
        {
            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            await WaitForExitAsync_Unity(process);

            string output = await outputTask;
            string error = await errorTask;

            if(process.ExitCode == 0)
            {
                if(!string.IsNullOrEmpty(output) && !output.Contains("0 B / 0 B"))
                {
                    Unity.Core.Logging.AppLogger.Log($"[Git LFS] Successfully ran 'git lfs pull' in {packagePath}\nOutput:\n{output}");
                }
                else
                {
                    Unity.Core.Logging.AppLogger.Log($"[Git LFS] Package '{Path.GetFileName(packagePath)}' is up to date.");
                }
            }
            else
            {
                Unity.Core.Logging.AppLogger.LogError($"[Git LFS] Error running 'git lfs pull' in {packagePath}\nError:\n{error}");
            }
        }
    }
    private static string GetGitPath()
    {
#if UNITY_EDITOR_OSX
        string brewGit = "/opt/homebrew/bin/git";
        return brewGit;
        //if(File.Exists(brewGit))
        //{
        //    UnityEngine.Debug.Log($"[Git LFS] Using Homebrew git: {brewGit}");
        //    return brewGit;
        //}

        //try
        //{
        //    var psi = new ProcessStartInfo
        //    {
        //        FileName = "/bin/bash",
        //        Arguments = "-c \"which git\"",
        //        RedirectStandardOutput = true,
        //        UseShellExecute = false,
        //        CreateNoWindow = true
        //    };

        //    using(var process = Process.Start(psi))
        //    {
        //        process.WaitForExit();
        //        string result = process.StandardOutput.ReadToEnd().Trim();

        //        if(!string.IsNullOrEmpty(result))
        //        {
        //            UnityEngine.Debug.Log($"[Git LFS] Detected git path: {result}");
        //            return result;
        //        }
        //    }
        //}
        //catch(Exception ex)
        //{
        //    UnityEngine.Debug.LogError($"[Git LFS] Error detecting git path: {ex.Message}");
        //}
#endif

        // fallback cuối cùng
        return "git";
    }
    private static Task WaitForExitAsync_Unity(Process process)
    {
        var tcs = new TaskCompletionSource<bool>();

        process.EnableRaisingEvents = true;

        process.Exited += (sender, args) =>
        {
            tcs.TrySetResult(true);
        };

        return tcs.Task;
    }
}
