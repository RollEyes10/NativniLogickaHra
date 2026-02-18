using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace NativniLogickaHra.Utils;

public static class Logger
{
    public static void Log(string message)
    {
        try
        {
            var m = $"[{DateTime.UtcNow:O}] {message}{Environment.NewLine}";
            Task.Run(async () =>
            {
                try
                {
                    var dir = FileSystem.AppDataDirectory;
                    var path = Path.Combine(dir, "ai_calls.log");
                    await File.AppendAllTextAsync(path, m).ConfigureAwait(false);
                }
                catch { }
            });
        }
        catch { }
    }
}
