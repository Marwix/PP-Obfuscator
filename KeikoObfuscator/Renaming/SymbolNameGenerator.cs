using System;
using System.Collections.Generic;

namespace KeikoObfuscator.Renaming
{
    public abstract class SymbolNameGenerator
    {
        public abstract string Next();
    }

    public class AlphabeticNameGenerator : SymbolNameGenerator
    {
        private const int MinimumIndex = 64;
        private const int MaximumIndex = 90;
        private readonly List<int> _counter = new List<int>();

        public AlphabeticNameGenerator()
        {
            Reset();
        }

        public void Reset()
        {
            _counter.Clear();
            _counter.Add(MinimumIndex);
        }

        public override string Next()
        {
            int index = _counter.Count - 1;
            while (index >= 0)
            {
                if (_counter[index] >= MaximumIndex)
                {
                    _counter[index] = MinimumIndex + 1;
                    if (index == 0)
                    {
                        _counter.Add(MinimumIndex + 1);
                        break;
                    }
                    index--;
                }
                else
                {
                    break;
                }
            }

            _counter[index]++;

            var characters = new char[_counter.Count];
            for (int i = 0; i < _counter.Count; i++)
                characters[i] = (char)_counter[i];
            return new string(characters);
        }
    }
    
    public class RangeNameGenerator : SymbolNameGenerator
    {
        private static readonly Random Random = new Random();

        public static readonly RangeNameGenerator BrailleNameGenerator = new RangeNameGenerator(0x2800, 0x2900);
        public static readonly RangeNameGenerator ChineseNameGenerator = new RangeNameGenerator(0x33E0, 0x8000);

        public RangeNameGenerator(int minimumIndex, int maximumIndex)
        {
            MinimumIndex = minimumIndex;
            MaximumIndex = maximumIndex;
        }

        public int MinimumIndex
        {
            get;
            private set;
        }

        public int MaximumIndex
        {
            get;
            private set;
        }
        
        public override string Next()
        {
            var characters = new char[Random.Next(0, 20)];
            for (int i = 0; i < characters.Length; i++)
                characters[i] = (char)Random.Next(MinimumIndex, MaximumIndex);
            return new string(characters);
        }
    }
}