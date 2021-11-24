using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject.ArrowExceptions
{
    public class UnmatchedBracketException : ArrowException
    {
        public UnmatchedBracketException(int line) : base(line, $"Unmatched bracket") { }
    }
}
