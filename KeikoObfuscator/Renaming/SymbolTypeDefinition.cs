using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;

namespace KeikoObfuscator.Renaming
{
    public class SymbolTypeDefinition
    {
        public SymbolTypeDefinition(TypeDefinition type)
        {
            Type = type;
            MemberOverloads = new List<SymbolOverload>();
        }

        public string NewNamespace
        {
            get;
            set;
        }

        public string NewName
        {
            get;
            set;
        }

        public TypeDefinition Type
        {
            get;
            private set;
        }

        public List<SymbolOverload> MemberOverloads
        {
            get;
            private set;
        }

        public void Apply(IObfuscationContext context)
        {
            Type.Namespace = NewNamespace;
            Type.Name = NewName;

            foreach (var overload in MemberOverloads)
                overload.Apply(context);
        }

    }
}
