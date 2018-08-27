using System;
using Mono.Cecil;

namespace KeikoObfuscator.JunkGeneration
{
    public class MemberGenerationPhase : ObfuscatorTaskPhase
    {
        private static readonly Random Random = new Random();

        public MemberGenerationPhase(JunkCodeGenerationTask task)
        {
            Task = task;
        }

        public JunkCodeGenerationTask Task
        {
            get;
            private set;
        }

        public override string Name
        {
            get { return "Nember Generation Phase"; }
        }

        public override string Description
        {
            get { return "Generates and injects redudant members."; }
        }

        public override void Apply(IObfuscationContext context)
        {
            var module = context.Assembly.MainModule;
            for (int i = 0; i < Random.Next(3, 30); i ++)
            {
                TypeDefinition targetType;
                do
                {
                    targetType = module.Types[Random.Next(module.Types.Count)];
                } while (targetType.IsInterface);

                var junkMethod = new MethodDefinition("junkMethod" + i, MethodAttributes.Static, module.TypeSystem.Void);

                var parameterCount = Random.Next(-4, 4);
                if (parameterCount < 0)
                    parameterCount = 0;
                for (int j = 0; j < parameterCount; j++)
                {
                    junkMethod.Parameters.Add(new ParameterDefinition(module.TypeSystem.Object));
                }

                targetType.Methods.Add(junkMethod);
                MsilBodyGenerator.AssignRandomMethodBody(junkMethod);
            }
        }
    }
}