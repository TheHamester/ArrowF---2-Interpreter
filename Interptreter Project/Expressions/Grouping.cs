
namespace InterpreterProject.Expressions
{
    public class Grouping : IExprTree
    {
        public IExprTree Expr;

        public object Accept(IExprVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public Grouping(IExprTree expr)
        {
            Expr = expr;
        }
    }
}
