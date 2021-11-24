using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Kuinox.SCharpScript
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            if(args.Length != 1)
            {
                Console.Error.WriteLine("One, and only one argument is needed, the script path.");
                return 42;
            }
            string scriptLocation = args[0];
            
            string tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempPath);
            string script = await File.ReadAllTextAsync(scriptLocation);
            Regex regex = new(@"<Project[^>]*>.*<\/Project>", RegexOptions.Compiled);
            Match match = regex.Match(script);
            string csproj = "";
            if (match.Success)
            {
                csproj = match.Value;
                script = script.Remove(match.Index, match.Length);
            }
            await File.WriteAllTextAsync(Path.Combine(tempPath, "Script.csproj"),
@"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net6.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
</PropertyGroup>
" + csproj + "\n</Project>");
            
            await File.WriteAllTextAsync(Path.Combine(tempPath, "Program.cs"), script);
            Process process = new();
            process.StartInfo.FileName = "dotnet";
            process.StartInfo.Arguments = $"run --project {tempPath}";
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardInput = true;
            process.ErrorDataReceived += (p, line) => Console.Error.WriteLine(line.Data);
            process.OutputDataReceived += (p, line) => Console.WriteLine(line.Data);
            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
            Task task = Console.OpenStandardInput().CopyToAsync(process.StandardInput.BaseStream);
            await process.WaitForExitAsync();
            
            Console.WriteLine(process.ExitCode);
            return process.ExitCode;
        }
    }
}
