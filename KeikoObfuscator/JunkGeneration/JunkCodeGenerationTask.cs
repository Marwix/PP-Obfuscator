using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeikoObfuscator.JunkGeneration
{
    public class JunkCodeGenerationTask : ObfuscatorTask
    {
        public override string Name
        {
            get { return "Junk Code Generator"; }
        }

        public override string Description
        {
            get { return "Injects redundant code into the target assembly to mislead reverse engineers."; }
        }

        public override void Initialize(IObfuscationContext context)
        {
            Phases.AddRange(new ObfuscatorTaskPhase[]
            {
                new MemberGenerationPhase(this)
            });
        }

        public override void Finalize(IObfuscationContext context)
        {
            
        }
    }
}
