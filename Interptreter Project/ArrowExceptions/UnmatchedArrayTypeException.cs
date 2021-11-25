using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject.ArrowExceptions
{
    internal class UnmatchedArrayTypeException : ArrowException
    {
        public string Type1 { get; }
        public string Type2 { get; }

        public UnmatchedArrayTypeException(string type1, string type2, int line) : base(line, $"Cannot have an element of type {type1} in an array of type {type2}") 
        {
            Type1 = type1;
            Type2 = type2;
        }
    }
}
