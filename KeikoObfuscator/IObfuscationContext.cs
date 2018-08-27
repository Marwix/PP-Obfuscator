using System;
using System.Collections.Generic;
using KeikoObfuscator.Renaming;
using Mono.Cecil;

namespace KeikoObfuscator
{
    public interface IObfuscationContext
    {
        AssemblyDefinition Assembly { get; }

        IList<ObfuscatorTask> Tasks { get; }

        ILogOutput LogOutput { get; }

        SymbolNameGenerator GetSymbolNameGenerator();
    }

    public class ObfuscationContext : IObfuscationContext
    {
        public ObfuscationContext(AssemblyDefinition assembly, ILogOutput logOutput)
        {
            Assembly = assembly;
            LogOutput = logOutput;
            Tasks = new List<ObfuscatorTask>();
        }

        public AssemblyDefinition Assembly { get; private set; }

        public IList<ObfuscatorTask> Tasks { get; private set; }

        public ILogOutput LogOutput { get; private set; }

        public SymbolNameGenerator GetSymbolNameGenerator()
        {
            return new AlphabeticNameGenerator();
        }
    }
}