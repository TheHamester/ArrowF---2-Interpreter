using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject.ArrowExceptions
{
    public class UnexpectedTokenException : ArrowException
    {
        public readonly TokenType Token1;
        public readonly TokenType Token2;

        public UnexpectedTokenException(TokenType token1, TokenType token2, int line) : base(line, $"Unexpected token {token1}, expected {token2}") 
        {
            Token1 = token1;
            Token2 = token2;
        }
    }
}
