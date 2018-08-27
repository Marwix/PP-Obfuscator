using System;
using KeikoObfuscator.JunkGeneration;
using KeikoObfuscator.Renaming;
using Mono.Cecil;
namespace KeikoObfuscator
{
    public static class Obfuscator
    {
        public static void Obfuscate(AssemblyDefinition assembly, ILogOutput logOutput)
        {
            logOutput.WriteMessage("Obfuscator module written by Marwix (2016).");
            logOutput.WriteMessage("---------------------------------------------------");

            var startTime = DateTime.Now;
            logOutput.WriteMessage("Started obfuscation process at " + startTime);

            var context = new ObfuscationContext(assembly, logOutput);

            var tasks = new ObfuscatorTask[]
            {
                new JunkCodeGenerationTask(),
                new SymbolRenamingTask(),
            };

            foreach (var task in tasks)
            {
                logOutput.WriteMessage("Initializing " + task.Name + "...");
                task.Initialize(context);
            }

            foreach (var task in tasks)
            {
                logOutput.WriteMessage("Applying " + task.Name + "...");
                task.ApplyAll(context);
            }

            foreach (var task in tasks)
            {
                logOutput.WriteMessage("Finalizing " + task.Name + "...");
                task.Finalize(context);
            }

            var endTime = DateTime.Now;

            logOutput.WriteMessage("---------------------------------------------------");
            logOutput.WriteMessage("Finished obfuscation process at " + endTime);
            logOutput.WriteMessage("Duration: " + (endTime - startTime));


        }
    }
}
