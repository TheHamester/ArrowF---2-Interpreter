
namespace InterpreterProject.ArrowExpressions
{
    public interface IExprTree
    {
        object Accept(IExprVisitor visitor);
    }
}
