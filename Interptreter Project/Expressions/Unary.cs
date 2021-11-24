
namespace InterpreterProject.Expressions
{
    public class Unary : IExprTree
    {
        public Token Token;
        public IExprTree Right;

        public object Accept(IExprVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public Unary(Token token, IExprTree right)
        {
            Token = token;
            Right = right;
        }
    }
}
