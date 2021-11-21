
namespace InterpreterProject.Expressions
{
    public class Binary : IExprTree
    {
        public IExprTree Left;
        public Token Token;
        public IExprTree Right;

        public object Accept(IExprVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public Binary(IExprTree left, Token token, IExprTree right)
        {
            Left = left;
            Token = token;
            Right = right;
        }
    }
}
