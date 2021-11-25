using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject.ArrowExceptions
{
    internal class InvalidIndexException : ArrowException
    {
        public string Type { get; }

        public InvalidIndexException(string type, int line) : base(line, $"Unable to use {type} as index for an array") 
        {
            Type = type;
        }
    }
}
