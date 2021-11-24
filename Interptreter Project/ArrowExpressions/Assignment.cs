using InterpreterProject.ArrowTypes;

namespace InterpreterProject.ArrowExpressions
{
    public record Assignment(ArrowType Type, IExprTree Right, int Line) : IExprTree
    {
        public object Accept(IExprVisitor visitor) => visitor.Visit(this);
    }
}
