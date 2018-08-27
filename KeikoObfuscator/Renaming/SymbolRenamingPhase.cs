using System;
using System.Collections.Generic;

namespace KeikoObfuscator.Renaming
{
    public class SymbolRenamingPhase : ObfuscatorTaskPhase
    {
        public SymbolRenamingPhase(SymbolRenamingTask task)
        {
            Task = task;
        }

        public SymbolRenamingTask Task { get; private set; }

        public SymbolAnalysisReport Report
        {
            get { return Task.AnalysisReport; }
        }

        public override string Name
        {
            get { return "Symbol Renaming Phase"; }
        }

        public override string Description
        {
            get { return "Applies the renaming procedure on all symbols marked in the analysis phase."; }
        }

        public override void Apply(IObfuscationContext context)
        {
            var typeNameGenerator = context.GetSymbolNameGenerator();
            foreach (var symbolType in Report.TypesToRename)
            {
                symbolType.NewName = typeNameGenerator.Next();

                var memberNameGenerators = new Dictionary<Type, SymbolNameGenerator>();

                foreach (var overload in symbolType.MemberOverloads)
                {
                    SymbolNameGenerator memberNameGenerator;
                    var memberType = overload.Symbols[0].Member.GetType();
                    if (!memberNameGenerators.TryGetValue(memberType, out memberNameGenerator))
                        memberNameGenerators.Add(memberType, memberNameGenerator = context.GetSymbolNameGenerator());

                    overload.NewName = memberNameGenerator.Next();
                }

                symbolType.Apply(context);
            }
        }
    }
}