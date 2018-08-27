using Mono.Cecil;

namespace KeikoObfuscator.Renaming
{
    public class SymbolDefinition
    {
        public SymbolDefinition(IMemberDefinition member)
        {
            Member = member;
            OriginalName = Member.Name;
        }

        public IMemberDefinition Member { get; private set; }

        public string OriginalName { get; private set; }

        public override string ToString()
        {
            return Member.ToString();
        }
    }
}