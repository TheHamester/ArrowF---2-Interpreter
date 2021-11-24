using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject.ArrowExceptions
{
    internal class UnmatchedArrayDepthException : ArrowException
    {
        public readonly int Depth;

        public UnmatchedArrayDepthException(int depth, int line) : base(line, $"Not all outer arrays have depth {depth}") 
        {
            Depth = depth;
        }

    }
}
