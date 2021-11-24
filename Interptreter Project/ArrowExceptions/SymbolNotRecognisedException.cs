using System;

namespace InterpreterProject.ArrowExceptions
{
    public class SymbolNotRecognisedException : ArrowException
    {
        public readonly char Symbol;
        public readonly int Position;

        public SymbolNotRecognisedException(char symbol, int position, int line) : base(line, $"Symbol {symbol} at position {position} is not part of the language alphabet")
        {
            Symbol = symbol;
            Position = position;
        }
    }
}
