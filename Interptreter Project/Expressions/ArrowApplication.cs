using System.Collections.Generic;

namespace InterpreterProject.Expressions
{
    public class ArrowApplication : IExprTree
    {
        public List<IExprTree> Statements;

        public object Accept(IExprVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public ArrowApplication(List<IExprTree> statements)
        {
            Statements = statements;
        }
    }
}
