using System.Collections.Generic;


namespace InterpreterProject.Expressions
{
    public class ArrayExpression : IExprTree
    {
        public List<IExprTree> Elements { get; set; }

        public object Accept(IExprVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public ArrayExpression(List<IExprTree> elements)
        {
            Elements = elements;
        }
    }
}
