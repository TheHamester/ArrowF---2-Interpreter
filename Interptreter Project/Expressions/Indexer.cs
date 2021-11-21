using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject.Expressions
{
    public class Indexer : IExprTree
    {
        public IExprTree Indexable;

        public IExprTree Expr;

        public object Accept(IExprVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public Indexer(IExprTree indexable, IExprTree expr)
        {
            Indexable = indexable;
            Expr = expr;
        }
    }
}
