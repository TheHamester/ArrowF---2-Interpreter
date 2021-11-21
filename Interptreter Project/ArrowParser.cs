using System;
using System.Collections.Generic;
using System.Linq;
using InterpreterProject.Expressions;


namespace InterpreterProject
{

    public class ArrowParser
    {
        private List<Token> tokens = new List<Token>();
        private int currentPos;

        public ArrowParser(List<Token> tokens) { this.tokens = tokens; }

        public IExprTree Parse()
        {
            currentPos = 0;
            IExprTree tree = ArrowAppllication();

            return tree;
        }
        
        public void Consume(TokenType type)
        {
            if (CurrentToken().Type != type)
                throw new Exception($"{CurrentToken().Type} but expected {type}");

            currentPos++;
        }

        private IExprTree ArrowAppllication()
        {
            List<IExprTree> statements = new List<IExprTree>();
            while (CurrentToken().Type != TokenType.EOF)
            {
                statements.Add(Statement());
                if(CurrentToken().Type != TokenType.EOF)
                    Consume(TokenType.EOL);
            }
            return new ArrowApplication(statements);
        }

        public IExprTree Statement()
        {
            bool assignTokens = ContainsAssignTokens();
            bool typeTokens = ContainsTypeTokens();
            if (assignTokens || typeTokens)
                return Assignment(assignTokens, typeTokens);

            return Expr();
        }

        public Token PreviousToken() => tokens[currentPos - 1];

        private Token CurrentToken() => tokens[currentPos];

        private IExprTree Expr()
        {
            IExprTree indexable = Or();

            return indexable;
        }



        private IExprTree Or() => BinaryOperation(Xor, TokenType.Or);

        private IExprTree Xor() => BinaryOperation(And, TokenType.Xor);

        private IExprTree And() => BinaryOperation(Shift, TokenType.And);

        private IExprTree Shift() => BinaryOperation(Sum, TokenType.LeftShift, TokenType.RightShift);

        private IExprTree Sum() => BinaryOperation(Term, TokenType.Plus, TokenType.Minus);

        private IExprTree Term() => BinaryOperation(Factor, TokenType.Times, TokenType.Divide, TokenType.Modulo);

        private IExprTree Factor()
        {
            while (CurrentToken().Type == TokenType.Invert)
            {
                Consume(TokenType.Invert);
                return new Unary(PreviousToken(), Factor());
            }

            return Primary();
        }

        private IExprTree Primary()
        {
            if (new[] { TokenType.Identifier, TokenType.Note, TokenType.True, TokenType.False }.Contains(CurrentToken().Type))
            {
                Consume(CurrentToken().Type);
                IExprTree indexable = new Literal(PreviousToken());
                if (PreviousToken().Type == TokenType.Identifier)
                    while (CurrentToken().Type == TokenType.LeftBracket)
                    {
                        Consume(TokenType.LeftBracket);
                        indexable = new Indexer(indexable, Expr());
                        Consume(TokenType.RightBracket);

                    }
                return indexable;
            }

            if (CurrentToken().Type == TokenType.LeftParenthesy)
            {
                Consume(TokenType.LeftParenthesy);
                IExprTree tree = Expr();
                Consume(TokenType.RightParenthesy);

                IExprTree indexable = new Grouping(tree);

                while (CurrentToken().Type == TokenType.LeftBracket)
                {
                    Consume(TokenType.LeftBracket);
                    indexable = new Indexer(indexable, Expr());
                    Consume(TokenType.RightBracket);
                }

                return indexable;
            }
            else if (CurrentToken().Type == TokenType.RightParenthesy)
                throw new Exception();

            if (CurrentToken().Type == TokenType.LeftBracket)
            {
                List<IExprTree> elements = new List<IExprTree>();
                Consume(TokenType.LeftBracket);
                elements = Array();
                Consume(TokenType.RightBracket);

                IExprTree indexable = new ArrayExpression(elements);

                while (CurrentToken().Type == TokenType.LeftBracket)
                {
                    Consume(TokenType.LeftBracket);
                    indexable = new Indexer(indexable, Expr());
                    Consume(TokenType.RightBracket);
                }

                return indexable;
            }

            return null;
        }



        private IExprTree BinaryOperation(Func<IExprTree> ex, params TokenType[] args) 
        {
            IExprTree tree = ex();

            while (args.Contains(CurrentToken().Type))
                for (int i = 0; i < args.Length; i++)
                    if (CurrentToken().Type == args[i])
                    {
                        Consume(args[i]);
                        tree = new Binary(tree, PreviousToken(), ex());
                        break;
                    }
            
            return tree;
        }

        private ArrowType Var(bool expectIdentifier)
        {
            ArrowType type = null;
            if (new[] { TokenType.N, TokenType.B, TokenType.LeftBracket, TokenType.LeftParenthesy }.Contains(CurrentToken().Type))
            {
                switch (CurrentToken().Type)
                {
                    case TokenType.N:
                        Consume(TokenType.N);
                        return new ArrowNote("", 0, false);
                    case TokenType.B:
                        Consume(TokenType.B);
                        return new ArrowBoolean("", false, false);
                    case TokenType.LeftBracket:
                        Consume(TokenType.LeftBracket);
                        type = new ArrowArray("", Var(false), new List<object>(), false);
                        Consume(TokenType.RightBracket);
                        break;
                    case TokenType.LeftParenthesy:
                        List<ArrowType> parameters = new List<ArrowType>();
                        Consume(TokenType.LeftParenthesy);
            
                        parameters.Add(Var(true));

                        if (expectIdentifier)
                        {
                            Consume(TokenType.Identifier);
                            parameters.Last().Id = PreviousToken().Text;
                        }

                        while (CurrentToken().Type == TokenType.Coma)
                        {
                            Consume(TokenType.Coma);
                            parameters.Add(Var(false));

                            if (expectIdentifier)
                            {
                                Consume(TokenType.Identifier);
                                parameters.Last().Id = PreviousToken().Text;
                            }
                        }

                        Consume(TokenType.RightParenthesy);
                        type = new ArrowFunction("", parameters, null, true);
                        break;
                    default:
                        throw new Exception(); 
                }
                return type;

            }
            else throw new Exception();    
        }

        private IExprTree Assignment(bool assignTokens, bool typeTokens)
        {
            ArrowType var = new ArrowType();
            if (typeTokens)
                var = Var(true);

            Consume(TokenType.Identifier);
            string id = PreviousToken().Text;
            if(typeTokens)
                var.Id = id;

            IExprTree expr;
            switch (CurrentToken().Type)
            {
                case TokenType.Assign:
                    Consume(TokenType.Assign);
                    
                    expr = Expr();
                    if (expr == null) throw new Exception("Expected expression");


                    if (typeTokens)
                        return new Assignment(var, expr);
                    else
                        return new Reassignment(id, expr, TokenType.Assign);
                case TokenType.OrEquals:
                case TokenType.AndEquals:
                case TokenType.XorEquals:
                case TokenType.PlusEquals:
                case TokenType.MinusEquals:
                case TokenType.ModuloEquals:
                case TokenType.DivideEquals:
                case TokenType.TimesEquals:
                    Consume(CurrentToken().Type);
                    TokenType type = PreviousToken().Type;
                    expr = Expr();
                    if (expr == null) throw new Exception("Expected expression");

                    if (typeTokens)
                        throw new Exception($"Cannot apply {type} to a variable with no value");
                    else
                        return new Reassignment(id, expr, type);
                default:
                    return new Assignment(var, null);
            }
        }

        private List<IExprTree> Array()
        {
            List<IExprTree> elements = new List<IExprTree>();
                
            IExprTree tree = Expr();
            if (tree == null)
                return new List<IExprTree>();
            else
                elements.Add(tree);

            while (CurrentToken().Type == TokenType.Coma)
            {
                Consume(TokenType.Coma);
                elements.Add(Expr());
            }

            return elements;
        }

        private bool ContainsTypeTokens()
        {
            int i = currentPos;
            while (tokens[i].Type != TokenType.EOL && tokens[i].Type != TokenType.EOF)
            {
                if (new[] { TokenType.N, TokenType.B }.Contains(tokens[i].Type)) return true;
                i++;
            }
            return false;
        }

        private bool ContainsAssignTokens()
        {
            int i = currentPos;
            while (tokens[i].Type != TokenType.EOL && tokens[i].Type != TokenType.EOF)
            {
                if (new[] { TokenType.OrEquals,
                    TokenType.AndEquals, TokenType.XorEquals, TokenType.TimesEquals,
                    TokenType.DivideEquals, TokenType.PlusEquals, TokenType.MinusEquals,
                    TokenType.ModuloEquals, TokenType.Assign}.Contains(tokens[i].Type))
                    return true;
                i++;
            }
            return false;
        }

        private void OutputList<T>(List<T> list)
        {
            foreach (T t in list)
                Console.WriteLine(t);
        }
    }
}
