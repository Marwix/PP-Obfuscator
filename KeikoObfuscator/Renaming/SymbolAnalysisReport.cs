using System.Collections.Generic;

namespace KeikoObfuscator.Renaming
{
    public class SymbolAnalysisReport
    {
        public SymbolAnalysisReport()
        {
            TypesToRename = new List<SymbolTypeDefinition>();
        }
        
        public List<SymbolTypeDefinition> TypesToRename { get; private set; }
    }
}