namespace KeikoObfuscator.Renaming
{
    public class SymbolRenamingTask : ObfuscatorTask
    {
        public override string Name
        {
            get { return "Symbol Renamer"; }
        }

        public override string Description
        {
            get { return "Renames all possible symbols to an inreversible state to confuse reverse engineers."; }
        }

        public SymbolAnalysisReport AnalysisReport { get; private set; }

        public override void Initialize(IObfuscationContext context)
        {
            AnalysisReport = new SymbolAnalysisReport();
            Phases.AddRange(new ObfuscatorTaskPhase[]
            {
                new SymbolAnalysisPhase(this),
                new SymbolRenamingPhase(this)
            });
        }
        
        public override void Finalize(IObfuscationContext context)
        {
        }
    }
}
