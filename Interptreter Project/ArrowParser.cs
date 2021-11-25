using System;
using System.Linq;
using System.Collections.Generic;
using InterpreterProject.ArrowTypes;
using InterpreterProject.ArrowExceptions;
using InterpreterProject.ArrowExpressions;

namespace InterpreterProject
{
    public class ArrowParser
    {
        private readonly List<Token> tokens = new();
        private int currentPos;
        private bool checkEOL = true;
        public ArrowParser(List<Token> tokens) { this.tokens = tokens; }

        public IExprTree Parse()
        {
            currentPos = 0;
            IExprTree tree = ArrowAppllication(0);

            return tree;
        }
        
        public void Consume(TokenType type)
        {
            
            if (CurrentToken().Type != type)
                throw new UnexpectedTokenException(CurrentToken().Type, type, CurrentToken().Line);
            //Console.WriteLine()
            currentPos++;
        }

        private IExprTree ArrowAppllication(int indentLevel)
        {
            return new CodeBlock(CodeBlock(indentLevel));
        }

        private List<IExprTree> CodeBlock(int indentLevel) 
        {
            List<IExprTree> statements = new();

            int indLvl;
            while (CurrentToken().Type != TokenType.EOF)
            {
                indLvl = 0;
                int ind = currentPos;
                while (tokens[ind].Type == TokenType.Tab)
                {
                    indLvl++;
                    ind++;
                }

                //Console.WriteLine(indentLevel + " " + indLvl);

                if (indLvl < indentLevel)
                {
                    checkEOL = false;
                    break;
                }
                else if (indLvl > indentLevel)
                    throw new UnexpectedIndentLevelException(PreviousToken().Line);

                for (int i = 0; i < indLvl; i++)
                    Consume(TokenType.Tab);

                if (CurrentToken().Type == TokenType.EOL || CurrentToken().Type == TokenType.Comment)
                {
                    Consume(TokenType.EOL);
                    continue;
                }

                statements.Add(Statement(indentLevel));

                if (CurrentToken().Type != TokenType.EOF && checkEOL)
                {
                    Consume(TokenType.EOL);
                }
                else
                    checkEOL = true;

            }
            return statements;
        }

        public IExprTree Statement(int indentLevel)
        {
            IExprTree cond;
            bool assignTokens = ContainsAssignTokens();
            bool typeTokens = ContainsTypeTokens();
            bool keywords = ContainsKeywords();
            if ((assignTokens || typeTokens) && !keywords)
                return Assignment(typeTokens);
            else if (CurrentToken().Type == TokenType.If)
            {
                Consume(TokenType.If);
                cond = assignTokens ? Assignment(typeTokens) : Expr();

                Consume(TokenType.EOL);
                List<IExprTree> block = CodeBlock(indentLevel + 1);

                return new IfStatement(cond, new CodeBlock(block));
            }
            else if (CurrentToken().Type == TokenType.While) 
            {
                Consume(TokenType.While);
                cond = assignTokens ? Assignment(typeTokens) : Expr();
                Consume(TokenType.EOL);
                List<IExprTree> block = CodeBlock(indentLevel + 1);

                return new WhileStatement(cond, new CodeBlock(block));
            }

            return Expr();
        }

        public Token PreviousToken() => tokens[currentPos - 1];

        private Token CurrentToken() => tokens[currentPos];

        private IExprTree Expr() => Or();

        private IExprTree Or() => BinaryOperation(Xor, TokenType.Or);

        private IExprTree Xor() => BinaryOperation(And, TokenType.Xor);

        private IExprTree And() => BinaryOperation(Shift, TokenType.And);

        private IExprTree Shift() => BinaryOperation(Sum, TokenType.LeftShift, TokenType.RightShift);

        private IExprTree Sum() => BinaryOperation(Term, TokenType.Plus, TokenType.Minus);

        private IExprTree Term() => BinaryOperation(Factor, TokenType.Times, TokenType.Divide, TokenType.Modulo);

        private IExprTree Factor()
        {
            while (CurrentToken().Type == TokenType.Invert || CurrentToken().Type == TokenType.Not)
            {
                Consume(CurrentToken().Type);
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
                        indexable = new Indexer(indexable, Expr(), PreviousToken().Line);
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
                    indexable = new Indexer(indexable, Expr(), PreviousToken().Line);
                    Consume(TokenType.RightBracket);
                }

                return indexable;
            }
            else if (CurrentToken().Type == TokenType.RightParenthesy)
                throw new UnmatchedBracketException(CurrentToken().Line);

            if (CurrentToken().Type == TokenType.LeftBracket)
            {
                List<IExprTree> elements;
                Consume(TokenType.LeftBracket);
                elements = Array();
                Consume(TokenType.RightBracket);

                IExprTree indexable = new ArrayExpression(elements);

                while (CurrentToken().Type == TokenType.LeftBracket)
                {
                    Consume(TokenType.LeftBracket);
                    indexable = new Indexer(indexable, Expr(), PreviousToken().Line);
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
            ArrowType type;
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
                        List<ArrowType> parameters = new();
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

        private IExprTree Assignment(bool typeTokens)
        {
            ArrowType var = new();
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
                    if (expr == null) throw new ExpectedExpressionException(PreviousToken().Line);

                    if (typeTokens)
                        return new Assignment(var, expr, PreviousToken().Line);
                    else
                        return new Reassignment(id, expr, TokenType.Assign, PreviousToken().Line);
                case TokenType.OrEquals:
                case TokenType.AndEquals:
                case TokenType.XorEquals:
                case TokenType.PlusEquals:
                case TokenType.MinusEquals:
                case TokenType.ModuloEquals:
                case TokenType.DivideEquals:
                case TokenType.TimesEquals:
                case TokenType.LeftShiftEquals:
                case TokenType.RightShiftEquals:
                    Consume(CurrentToken().Type);
                    TokenType type = PreviousToken().Type;
                    expr = Expr();
                    if (expr == null) throw new ExpectedExpressionException(PreviousToken().Line);

                    if (typeTokens)
                        throw new UninitiallizedVariableException(PreviousToken().Type, PreviousToken().Line);
                    else
                        return new Reassignment(id, expr, type, PreviousToken().Line);
                default:
                    return new Assignment(var, null, PreviousToken().Line);
            }
        }

        private List<IExprTree> Array()
        {
            List<IExprTree> elements = new();
                
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
                    TokenType.ModuloEquals, TokenType.Assign, TokenType.LeftShiftEquals, TokenType.RightShiftEquals }.Contains(tokens[i].Type))
                    return true;
                i++;
            }
            return false;
        }

        private bool ContainsKeywords() 
        {
            int i = currentPos;
            while (tokens[i].Type != TokenType.EOL && tokens[i].Type != TokenType.EOF)
            {
                if (new[] { TokenType.If, TokenType.While}.Contains(tokens[i].Type))
                    return true;
                i++;
            }
            return false;
        }
    }
}
