
namespace InterpreterProject.Expressions
{
    public class Assignment : IExprTree
    {
        public ArrowType Type;
        public IExprTree Right;

        public object Accept(IExprVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public Assignment(ArrowType type, IExprTree right)
        {
            Type = type;
            Right = right;
        }
    }
}
