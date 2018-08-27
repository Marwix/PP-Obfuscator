using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MethodBody = Mono.Cecil.Cil.MethodBody;

namespace KeikoObfuscator.JunkGeneration
{
    public class MsilBodyGenerator
    {
        private static readonly Random Random = new Random();

        private readonly ModuleDefinition _module;
        private readonly MethodDefinition _method;
        private readonly MethodBody _body;
        private readonly ILProcessor _processor;
        private readonly TypeDefinition[] _elementTypes;

        private const int MaxBlockDepth = 4;
        private int _blockDepth = 0;

        public static void AssignRandomMethodBody(MethodDefinition method)
        {
            var generator = new MsilBodyGenerator(method);

            var blockCount = Random.Next(1, 6);
            for (int i = 0; i < blockCount; i++)
            {
                generator.AppendBlock();
            }

            generator.FinalizeMethod();
        }

        private MsilBodyGenerator(MethodDefinition method)
        {
            _module = method.Module;
            _method = method;
            _body = method.Body = new MethodBody(method);
            _processor = _body.GetILProcessor();

            _elementTypes = new[]
            {
                _module.TypeSystem.Void.Resolve(),
                _module.TypeSystem.Boolean.Resolve(),
                _module.TypeSystem.Char.Resolve(),
                _module.TypeSystem.String.Resolve(),
                _module.TypeSystem.Byte.Resolve(),
                _module.TypeSystem.SByte.Resolve(),
                _module.TypeSystem.UInt16.Resolve(),
                _module.TypeSystem.Int16.Resolve(),
                _module.TypeSystem.UInt32.Resolve(),
                _module.TypeSystem.Int32.Resolve(),
                _module.TypeSystem.UInt64.Resolve(),
                _module.TypeSystem.Int64.Resolve(),
            };
        }

        private void FinalizeMethod()
        {
            _processor.Emit(OpCodes.Ret);
        }

        private void AppendCall(MethodDefinition method)
        {
            var opcode = method.IsStatic ? OpCodes.Call : OpCodes.Callvirt;
            var methodReference = _module.Import(method);

            if (!method.IsStatic)
                AppendLoadValue(methodReference.DeclaringType);

            foreach (var parameter in methodReference.Parameters)
                AppendLoadValue(parameter.ParameterType);

            _processor.Emit(opcode, methodReference);
        }

        private void AppendLocalAssignment(TypeReference valueType)
        {
            _processor.Emit(OpCodes.Stloc, GetOrCreateVariable(valueType));
        }

        private void AppendLoadValue(TypeReference valueType)
        {
            switch (Random.Next(4))
            {
                case 0:
                    AppendLiteral(valueType);
                    break;
                case 1:
                    AppendLoadLocal(valueType);
                    break;
                case 2:
                    if (valueType.IsPrimitive)
                        AppendBinaryOperation(valueType);
                    else
                        AppendLoadLocal(valueType);
                    break;
                case 3:
                    AppendRandomCall(valueType);
                    break;
            }
        }

        private void AppendRandomCall(TypeReference returnType)
        {
            MethodDefinition[] returningMethods;

            do
            {
                var randomType = GetRandomType();
                returningMethods = randomType.Methods.Where(x => x.IsPublic && x.ReturnType.FullName != "System.Void").ToArray();
            } while (returningMethods.Length == 0);

            var method = returningMethods[Random.Next(returningMethods.Length)];
            AppendCall(method);

            if (method.ReturnType.FullName != returnType.FullName)
            {
                _processor.Emit(method.ReturnType.IsValueType ? OpCodes.Isinst : OpCodes.Castclass, _module.Import(returnType));
            }
        }

        private void AppendBinaryOperation(TypeReference valueType)
        {
            AppendLoadValue(valueType);
            AppendLoadValue(valueType);

            OpCode opcode;
            switch (Random.Next(4))
            {
                case 0:
                    opcode = OpCodes.Add_Ovf_Un;
                    break;
                case 1:
                    opcode = OpCodes.Sub_Ovf_Un;
                    break;
                case 2:
                    opcode = OpCodes.Mul_Ovf_Un;
                    break;
                case 3:
                    opcode = OpCodes.Div_Un;
                    break;
                case 4:
                    opcode = OpCodes.And;
                    break;
                case 5:
                    opcode = OpCodes.Or;
                    break;
                case 6:
                    opcode = OpCodes.Xor;
                    break;
                default:
                    opcode = OpCodes.Pop;
                    break;

            }

            _processor.Emit(opcode);

        }
        
        private void AppendLiteral(TypeReference valueType)
        {
            switch (valueType.Name)
            {
                case "String":
                    _processor.Emit(OpCodes.Ldstr, GenerateRandomString());
                    break;
                case "SByte":
                    _processor.Emit(OpCodes.Ldc_I4, Random.Next(sbyte.MinValue, sbyte.MaxValue));
                    break;
                case "Int16":
                    _processor.Emit(OpCodes.Ldc_I4, Random.Next(short.MinValue, short.MaxValue));
                    break;
                case "Int32":
                    _processor.Emit(OpCodes.Ldc_I4, Random.Next(int.MinValue, int.MaxValue));
                    break;
                case "Int64":
                    _processor.Emit(OpCodes.Ldc_I8, (long)Random.Next(int.MinValue, int.MaxValue));
                    break;
                case "Byte":
                    _processor.Emit(OpCodes.Ldc_I4, Random.Next(0, byte.MaxValue));
                    break;
                case "Char":
                case "UInt16":
                    _processor.Emit(OpCodes.Ldc_I4, Random.Next(0, ushort.MaxValue));
                    break;
                case "UInt32":
                    _processor.Emit(OpCodes.Ldc_I4, Random.Next(0, int.MaxValue));
                    break;
                case "UInt64":
                    _processor.Emit(OpCodes.Ldc_I8, (long)Random.Next(0, int.MaxValue));
                    break;
                case "Single":
                    _processor.Emit(OpCodes.Ldc_R4, (float)Random.NextDouble());
                    break;
                case "Double":
                    _processor.Emit(OpCodes.Ldc_R8, Random.NextDouble());
                    break;
                case "Boolean":
                    _processor.Emit(Random.Next(100) > 50 ? OpCodes.Ldc_I4_0 : OpCodes.Ldc_I4_1);
                    break;
                default:
                    if (valueType.IsValueType)
                    {
                        var local = GetOrCreateVariable(valueType);
                        _processor.Emit(OpCodes.Ldloca, local);
                        _processor.Emit(OpCodes.Initobj, _module.Import(valueType));
                        _processor.Emit(OpCodes.Ldloc, local);
                    }
                    else
                    {
                        _processor.Emit(OpCodes.Ldnull);
                    }
                    break;
            }
        }

        private static string GenerateRandomString()
        {
            var characters = new char[Random.Next(1, 10)];
            for (int i = 0; i < characters.Length; i++)
                characters[i] = (char)Random.Next(65, 91);
            return new string(characters);
        }

        private void AppendLoadLocal(TypeReference valueType)
        {
            _processor.Emit(OpCodes.Ldloc, GetOrCreateVariable(valueType));
        }

        private VariableDefinition GetOrCreateVariable(TypeReference variableType)
        {
            var variable = _body.Variables.FirstOrDefault(x => x.VariableType.FullName == variableType.FullName);
            if (variable == null || Random.Next(100) > 75)
            {
                _body.Variables.Add(variable = new VariableDefinition(variableType));
                AppendLoadValue(variableType);
                AppendLocalAssignment(variableType);
            }

            return variable;
        }

        private void AppendBlock()
        {
            _blockDepth++;

            if (_blockDepth >= MaxBlockDepth)
            {
                AppendSimpleBlock();
                _blockDepth--;
                return;
            }

            switch (Random.Next(4))
            {
                case 0:
                    AppendSimpleBlock();
                    break;
                case 1:
                    AppendIfStatement();
                    break;
                case 2:
                    AppendLoopStatement();
                    break;
                case 3:
                    AppendTryStatement();
                    break;
            }

            _blockDepth--;
        }

        private void AppendSimpleBlock()
        {
            var statementCount = Random.Next(-10, 5);
            if (statementCount <= 0)
                statementCount = 1;

            for (int i = 0; i < statementCount; i++)
            {
                switch (Random.Next(2))
                {
                    case 0:
                        AppendAssignmentStatement();
                        break;
                    case 1:
                        AppendInvocationStatement();
                        break;
                }
            }
        }

        private void AppendAssignmentStatement()
        {
            var type = GetRandomType();
            AppendLoadValue(type);
            AppendLocalAssignment(type);
        }

        private void AppendInvocationStatement()
        {
            var type = GetRandomType();
            AppendRandomCall(type);
            _processor.Emit(OpCodes.Pop);
        }

        private void AppendIfStatement()
        {
            AppendLoadValue(_module.TypeSystem.Boolean);
            var appendElseBlock = Random.Next(100) > 50;
            var target1 = Instruction.Create(OpCodes.Nop);
            var target2 = Instruction.Create(OpCodes.Nop);

            _processor.Emit(OpCodes.Brtrue, target1);
            AppendBlock();

            if (appendElseBlock)
                _processor.Emit(OpCodes.Br, target2);

            _processor.Append(target1);

            if (appendElseBlock)
            {
                AppendBlock();
                _processor.Append(target2);
            }
        }

        private void AppendLoopStatement()
        {
            bool isDoLoop = Random.Next(100) > 50;

            var target1 = Instruction.Create(OpCodes.Nop);
            var target2 = Instruction.Create(OpCodes.Nop);

            if (!isDoLoop)
                _processor.Emit(OpCodes.Br, target2);

            _processor.Append(target1);

            AppendBlock();

            _processor.Append(target2);
            AppendLoadValue(_module.TypeSystem.Boolean);
            _processor.Emit(OpCodes.Brtrue, target1);

        }

        private void AppendTryStatement()
        {
            var handler = new ExceptionHandler(Random.Next(2) == 1 ? ExceptionHandlerType.Catch : ExceptionHandlerType.Finally);

            var target = Instruction.Create(OpCodes.Nop);

            _processor.Append(handler.TryStart = Instruction.Create(OpCodes.Nop));
            AppendBlock();
            _processor.Emit(OpCodes.Leave, target);

            if (handler.HandlerType == ExceptionHandlerType.Catch)
            {
                _processor.Append(handler.TryEnd = handler.HandlerStart = Instruction.Create(OpCodes.Pop));
                handler.CatchType = _module.TypeSystem.Object;
            }
            else
            {
                _processor.Append(handler.TryEnd = handler.HandlerStart = Instruction.Create(OpCodes.Nop));
            }

            AppendBlock();
            _processor.Emit(OpCodes.Leave_S, target);

            _processor.Append(handler.HandlerEnd = target);

            _body.ExceptionHandlers.Add(handler);
        }

        private TypeDefinition GetRandomType()
        {
            switch (Random.Next(2))
            {
                case 0:
                    return _module.Types[Random.Next(_module.Types.Count)];
                default:
                    return _elementTypes[Random.Next(_elementTypes.Length)];
            }
        }
    }
}