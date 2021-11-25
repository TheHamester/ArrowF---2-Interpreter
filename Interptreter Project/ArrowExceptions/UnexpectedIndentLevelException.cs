using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject.ArrowExceptions
{
    public class UnexpectedIndentLevelException : ArrowException
    {
        public UnexpectedIndentLevelException(int line) : base(line, "Unexpected indent level") { }
    }
}
