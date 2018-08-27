using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace KeikoObfuscator.Renaming
{
    public class SymbolAnalysisPhase : ObfuscatorTaskPhase
    {
        public SymbolAnalysisPhase(SymbolRenamingTask task)
        {
            Task = task;
        }

        public SymbolRenamingTask Task { get; private set; }

        public SymbolAnalysisReport Report { get { return Task.AnalysisReport; } }

        public override string Name
        {
            get { return "Symbol Analysis Phase"; }
        }

        public override string Description
        {
            get { return "Analyses and marks all possible symbols to be renamed."; }
        }
        
        public override void Apply(IObfuscationContext context)
        {
            foreach (var module in context.Assembly.Modules)
            {
                AnalyseModule(module);
            }
        }

        private void AnalyseModule(ModuleDefinition module)
        {
            foreach (var type in module.Types)
            {
                AnalyseType(type);
            }
        }

        private void AnalyseType(TypeDefinition type)
        {
            var symbolType = new SymbolTypeDefinition(type);
            Report.TypesToRename.Add(symbolType);

            foreach (var nestedType in type.NestedTypes)
                AnalyseType(nestedType);

            //if (type.IsEnum || type.IsValueType)
            //    return;
            //
            //if (!symbolType.Type.IsNested)
            AnalyseFieldsInType(symbolType);

            AnalyseMethodsInType(symbolType);
            AnalysePropertiesInType(symbolType);
        }

        private static void AnalyseFieldsInType(SymbolTypeDefinition symbolType)
        {
            var overloads = new List<SymbolOverload>();
            
            foreach (var field in symbolType.Type.Fields)
            {
                if (field.IsSpecialName || field.IsRuntimeSpecialName)
                    continue;
                
                var fieldOverload = overloads.FirstOrDefault(
                    overload => overload.Symbols.All(
                        x => ((FieldDefinition)x.Member).FieldType != field.FieldType));

                if (fieldOverload == null)
                {
                    fieldOverload = new SymbolOverload();
                    overloads.Add(fieldOverload);
                    symbolType.MemberOverloads.Add(fieldOverload);
                }

                fieldOverload.Symbols.Add(new SymbolDefinition(field));
            }
        }

        private static void AnalyseMethodsInType(SymbolTypeDefinition symbolType)
        {
            var overloads = new List<SymbolOverload>();
            foreach (var method in symbolType.Type.Methods)
            {
                if (method.IsSpecialName || method.IsRuntimeSpecialName || method.IsVirtual)
                    continue;
                
                var methodOverload = overloads.FirstOrDefault(
                    overload => overload.Symbols.All(
                        x =>
                        {
                            var method2 = (MethodDefinition) x.Member;
                            return method2.ReturnType != method.ReturnType ||
                                   !method2.Parameters.SequenceEqual(method.Parameters, ParameterComparer.Instance);
                        }));

                if (methodOverload == null)
                {
                    methodOverload = new SymbolOverload();
                    overloads.Add(methodOverload);
                    symbolType.MemberOverloads.Add(methodOverload);
                }

                methodOverload.Symbols.Add(new SymbolDefinition(method));
            }
        }
        
        private class ParameterComparer : IEqualityComparer<ParameterDefinition>
        {
            public static ParameterComparer Instance { get; private set; }

            static ParameterComparer()
            {
                Instance = new ParameterComparer();
            }

            private ParameterComparer()
            {
            }
            
            public bool Equals(ParameterDefinition x, ParameterDefinition y)
            {
                return x.ParameterType == y.ParameterType;
            }

            public int GetHashCode(ParameterDefinition obj)
            {
                return obj.ParameterType.GetHashCode();
            }
        }

        private void AnalysePropertiesInType(SymbolTypeDefinition symbolType)
        {
            var overloads = new List<SymbolOverload>();

            foreach (var property in symbolType.Type.Properties)
            {
                if (property.IsSpecialName || property.IsRuntimeSpecialName)
                    continue;

                var propertyOverload = overloads.FirstOrDefault(
                    overload => overload.Symbols.All(
                        x => ((PropertyDefinition)x.Member).PropertyType != property.PropertyType));

                if (propertyOverload == null)
                {
                    propertyOverload = new SymbolOverload();
                    overloads.Add(propertyOverload);
                    symbolType.MemberOverloads.Add(propertyOverload);
                }

                propertyOverload.Symbols.Add(new SymbolDefinition(property));
            }
        }
    }
}