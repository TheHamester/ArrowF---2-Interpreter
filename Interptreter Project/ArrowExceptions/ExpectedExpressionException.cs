using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject.ArrowExceptions
{
    public class ExpectedExpressionException : ArrowException
    {
        public ExpectedExpressionException(int line) : base(line, "Exprected expression") { }
    }

}