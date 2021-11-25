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
        public static async Task<int> Main( string[] args )
        {
            if( args.Length != 1 )
            {
                return PrintWrongArgCount();
            }

            string scriptLocation = args[0];

            switch( scriptLocation )
            {
                case "-h":
                case "-H":
                case "/h":
                case "/H":
                case "-?":
                case "/?":
                case "-help":
                case "/help":
                case "--help":
                    return PrintUsage();
            }

            if( !File.Exists( scriptLocation ) )
            {
                return PrintFileNotFound( scriptLocation );
            }


            string tempPath = Path.Combine( Path.GetTempPath(), Path.GetRandomFileName() );
            Directory.CreateDirectory( tempPath );
            string script = await File.ReadAllTextAsync( scriptLocation );
            Regex regex = new( @"<Project[^>]*>(.*)</Project>", RegexOptions.Compiled | RegexOptions.Singleline );
            Match match = regex.Match( script );
            string csproj = "";
            if( match.Success )
            {
                var capture = match.Groups[1];
                csproj = capture.Value;
                int newLineCount = CountLines( csproj );
                script = new string( '\n', newLineCount ) + script.Remove( match.Index, match.Length );
            }
            await File.WriteAllTextAsync( Path.Combine( tempPath, "Script.csproj" ),
@"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net6.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
</PropertyGroup>
" + csproj + "\n</Project>" );

            await File.WriteAllTextAsync( Path.Combine( tempPath, "Program.cs" ), script );
            Process process = new();
            process.StartInfo.FileName = "dotnet";
            process.StartInfo.Arguments = $"run --project {tempPath}";
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardInput = true;
            process.ErrorDataReceived += ( p, line ) => Console.Error.WriteLine( line.Data );
            process.OutputDataReceived += ( p, line ) => Console.WriteLine( line.Data );
            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
            _ = Console.OpenStandardInput().CopyToAsync( process.StandardInput.BaseStream );
            await process.WaitForExitAsync();

            Console.WriteLine( process.ExitCode );
            return process.ExitCode;
        }

        static int CountLines( string str )
        {
            if( str == null )
                throw new ArgumentNullException( nameof( str ) );
            if( str == string.Empty )
                return 0;
            int index = -1;
            int count = 0;
            while( -1 != (index = str.IndexOf( Environment.NewLine, index + 1 )) )
                count++;

            return count + 1;
        }
        private static int PrintWrongArgCount()
        {
            Console.Error.WriteLine( "One, and only one, argument is needed, the script path." );
            return 42;
        }

        private static int PrintUsage()
        {
            Console.WriteLine();
            Console.WriteLine( "Usage:" );
            Console.WriteLine( "c# <ScriptPath>" );
            return 0;
        }

        private static int PrintFileNotFound( string scriptLocation )
        {
            Console.Error.WriteLine( $"The path '{scriptLocation}' does not exist." );
            return 2;
        }
    }
}
