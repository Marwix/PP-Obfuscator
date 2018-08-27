using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace KeikoObfuscator.Renaming
{
    public class SymbolOverload
    {
        public SymbolOverload()
        {
            Symbols = new List<SymbolDefinition>();
        }

        public SymbolOverload(params SymbolDefinition[] members)
            : this()
        {
            Symbols.AddRange(members);
        }

        public SymbolOverload(params IMemberDefinition[] members)
            : this()
        {
            Symbols.AddRange(members.Select(x => new SymbolDefinition(x)));
        }

        public List<SymbolDefinition> Symbols { get; private set; }

        public string NewName { get; set; }

        public void Apply(IObfuscationContext context)
        {
            foreach (var symbol in Symbols)
            {
                symbol.Member.Name = NewName;

                var method = symbol.Member as MethodDefinition;
                if (method != null)
                {
                    var generator = context.GetSymbolNameGenerator();
                    foreach (var parameter in method.Parameters)
                        parameter.Name = generator.Next();
                }
            }
        }
    }
}