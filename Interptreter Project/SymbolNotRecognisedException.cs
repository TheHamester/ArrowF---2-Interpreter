using System;

namespace InterpreterProject
{
    class SymbolNotRecognisedException : Exception
    {
        private char symbol;
        private int position;

        public SymbolNotRecognisedException(char symbol, int position) : base($"Symbol {symbol} at position {position} is not part of the language alphabet")
        {
            this.symbol = symbol;
            this.position = position;
        }
    }
}
