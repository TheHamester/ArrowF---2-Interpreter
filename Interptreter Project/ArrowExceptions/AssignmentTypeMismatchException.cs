using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject.ArrowExceptions
{
    internal class AssignmentTypeMismatchException : ArrowException
    {
        public readonly string Id;
        public readonly string Type1;
        public readonly string Type2;

        public AssignmentTypeMismatchException(string id, string type1, string type2, int line) : base(line, $"Cannot assign value of type {type1} to variable \"{id}\" of type {type2}") 
        {
            Id = id;
            Type1 = type1;
            Type2 = type2;
        }
    }
}
