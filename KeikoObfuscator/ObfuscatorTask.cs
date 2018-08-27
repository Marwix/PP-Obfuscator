using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeikoObfuscator
{
    public abstract class ObfuscatorTask
    {
        protected ObfuscatorTask()
        {
            Phases = new List<ObfuscatorTaskPhase>();
        }

        public abstract string Name { get; }

        public abstract string Description { get; }

        public List<ObfuscatorTaskPhase> Phases { get; private set; }

        public ObfuscatorTaskPhase CurrentPhase { get; protected set; }
        
        public abstract void Initialize(IObfuscationContext context);
        
        public abstract void Finalize(IObfuscationContext context);

        public void ApplyAll(IObfuscationContext context)
        {
            for (int index = 0; index < Phases.Count; index++)
            {
                var phase = Phases[index];
                context.LogOutput.WriteMessage(string.Format("Performing phase {0} ({1})...", index + 1, phase.Name));
                CurrentPhase = phase;
                phase.Apply(context);
            }
        }
    }

    public abstract class ObfuscatorTaskPhase
    {
        public abstract string Name { get; }

        public abstract string Description { get; }

        public abstract void Apply(IObfuscationContext context);
    }
}
