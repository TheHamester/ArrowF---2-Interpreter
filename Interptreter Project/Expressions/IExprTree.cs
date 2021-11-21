
namespace InterpreterProject.Expressions
{
    public interface IExprTree
    {
        object Accept(IExprVisitor visitor);
    }
}
