
namespace InterpreterProject.Expressions
{
    public class Statement : IExprTree
    {
        public object Accept(IExprVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}
