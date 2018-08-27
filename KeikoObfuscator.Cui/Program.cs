using System;
using System.IO;
using System.Windows.Forms;
using Mono.Cecil;

namespace KeikoObfuscator.Cui
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.Title = "Marwix - Obfuscator";
                // Get or ask for path to assembly.
                string path;
                if (args.Length > 0)
                {
                    path = args[0];
                }
                else
                {
                    Console.Write("Path: ");
                    path = Console.ReadLine();
                }

                using (var inputStream = File.OpenRead(path = path.Replace("\"", "")))
                {
                    // Read assembly.
                    var assembly = AssemblyDefinition.ReadAssembly(inputStream);
                    var logOutput = new ConsoleLogOutput();

                    // Obfuscate assembly.
                    Obfuscator.Obfuscate(assembly, logOutput);

                    // Write obfuscated assembly to disk.
                    var outputDirectory = Path.Combine(Path.GetDirectoryName(path), "Obfuscated");
                    if (!Directory.Exists(outputDirectory))
                        Directory.CreateDirectory(outputDirectory);

                    using (var outputStream = File.Create(Path.Combine(outputDirectory, Path.GetFileName(path))))
                        assembly.Write(outputStream);
                    Console.WriteLine("Obfuscated.");
                }

                Console.ReadKey();
            }
            catch (Exception er) { MessageBox.Show(er.ToString()); }
        }
    }

    class ConsoleLogOutput : ILogOutput
    {
        public void ReportError(Exception exception)
        {
            Console.WriteLine(exception.ToString());
        }

        public void WriteMessage(string message)
        {
            Console.WriteLine(message);
        }
    }
}
