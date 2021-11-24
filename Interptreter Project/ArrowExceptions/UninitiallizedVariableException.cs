using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject.ArrowExceptions
{
    public class UninitiallizedVariableException : ArrowException
    {
        public readonly TokenType Type;

        public UninitiallizedVariableException(TokenType type, int line) : base(line, $"Cannot apply operation {type} to an unitialized variable") 
        {
            Type = type;
        }
    }
}
