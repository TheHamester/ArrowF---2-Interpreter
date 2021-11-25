using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject.ArrowExceptions
{
    public class ArrowException : Exception
    {
        public int Line { get; }

        public ArrowException(int line, string message) : base($"{message} on line {line}") 
        {
            Line = line;
        }
    }
}
