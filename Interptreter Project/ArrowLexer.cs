using System;
using System.Collections.Generic;
using System.Linq;

namespace InterpreterProject
{
    public class ArrowLexer
    {
        public string Input { get; set; }

        private List<Token> tokens;
        private int currentPos;
        private int tokenStart;

        private const string Letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        private const string Digits = "0123456789";
        private Dictionary<string, TokenType> Keywords = new Dictionary<string, TokenType>()
                    {
                        { "break", TokenType.Break },
                        { "continue", TokenType.Continue },
                        { "if", TokenType.If },
                        { "while", TokenType.While },
                        { "random", TokenType.Random },
                        { "return", TokenType.Return },
                        { "N", TokenType.N },
                        { "B", TokenType.B },
                        { "true", TokenType.True },
                        { "false", TokenType.False }
                    };


        public ArrowLexer(string input)
        {
            Input = input;
        }

        public List<Token> CollectTokens()
        {
            tokens = new List<Token>();

            currentPos = 0;

            while (currentPos < Input.Length)
            {
                tokenStart = currentPos;

                if (Digits.Contains(Input[currentPos].ToString()))
                {
                    tokens.Add(new Token(TokenType.Note, CollectText(() => Digits.Contains(Input[currentPos].ToString())), tokenStart));
                    continue;
                }
                else if (Letters.Contains(Input[currentPos].ToString()))
                {
                    string text = CollectText(() => (Letters + Digits).Contains(Input[currentPos].ToString()));

                    if (Keywords.Keys.Contains(text))
                        tokens.Add(new Token(Keywords[text], text, tokenStart));
                    else
                        tokens.Add(new Token(TokenType.Identifier, text, tokenStart));

                    continue;
                }

                switch (Input[currentPos])
                {
                    case ' ':
                        currentPos++;
                        break;
                    case '[':
                        AddTokenAndAdvance(1, new Token(TokenType.LeftBracket, "[", tokenStart));
                        break;
                    case ']':
                        AddTokenAndAdvance(1, new Token(TokenType.RightBracket, "]", tokenStart));
                        break;
                    case '(':
                        AddTokenAndAdvance(1, new Token(TokenType.LeftParenthesy, "(", tokenStart));
                        break;
                    case ')':
                        AddTokenAndAdvance(1, new Token(TokenType.RightParenthesy, ")", tokenStart));
                        break;
                    case ',':
                        AddTokenAndAdvance(1, new Token(TokenType.Coma, ",", tokenStart));
                        break;
                    case '\t':
                        AddTokenAndAdvance(1, new Token(TokenType.Tab, "<TAB>", tokenStart));
                        break;
                    case '\n':
                        AddTokenAndAdvance(1, new Token(TokenType.EOL, "<EOL>", tokenStart));
                        break;
                    case '~':
                        AddTokenAndAdvance(1, new Token(TokenType.Invert, "~", tokenStart));
                        break;
                    case '+':
                        AddTokenAndAdvanceCombined(TokenType.Plus, TokenType.PlusEquals, '+');
                        break;
                    case '-':
                        AddTokenAndAdvanceCombined(TokenType.Minus, TokenType.MinusEquals, '-');
                        break;
                    case '*':
                        AddTokenAndAdvanceCombined(TokenType.Times, TokenType.TimesEquals, '*');
                        break;
                    case '%':
                        AddTokenAndAdvanceCombined(TokenType.Modulo, TokenType.ModuloEquals, '%');
                        break;
                    case '!':
                        AddTokenAndAdvanceCombined(TokenType.Not, TokenType.NotEquals, '!');
                        break;
                    case '&':
                        AddTokenAndAdvanceCombined(TokenType.And, TokenType.AndEquals, '&');
                        break;
                    case '|':
                        AddTokenAndAdvanceCombined(TokenType.Or, TokenType.OrEquals, '|');
                        break;
                    case '^':
                        AddTokenAndAdvanceCombined(TokenType.Xor, TokenType.XorEquals, '^');
                        break;
                    case '=':
                        AddTokenAndAdvanceCombined(TokenType.Assign, TokenType.Equals, '=');
                        break;
                    case '/':
                        if (currentPos + 1 < Input.Length && Input[currentPos + 1] == '=')
                        {
                            AddTokenAndAdvance(2, new Token(TokenType.DivideEquals, "/=", tokenStart));
                            continue;
                        }
                        else if (currentPos + 1 < Input.Length && Input[currentPos + 1] == '/')
                        {
                            currentPos += 2;
                            AddTokenAndAdvance(0, new Token(TokenType.Comment, $"//{ CollectText(() => Input[currentPos] != '\n') }", tokenStart));
                            continue;
                        }
                        AddTokenAndAdvance(1, new Token(TokenType.Divide, "/", tokenStart));
                        break;
                    case '<':
                        if (currentPos + 1 < Input.Length && Input[currentPos + 1] == '=')
                        {
                            AddTokenAndAdvance(2, new Token(TokenType.LessOrEqualTo, "<=", tokenStart));
                            continue;
                        }
                        else if (currentPos + 1 < Input.Length && Input[currentPos + 1] == '<')
                        {
                            AddTokenAndAdvance(2, new Token(TokenType.LeftShift, "<<", tokenStart));
                            continue;
                        }
                        AddTokenAndAdvance(1, new Token(TokenType.LessThan, "<", tokenStart));
                        break;
                    case '>':
                        if (currentPos + 1 < Input.Length && Input[currentPos + 1] == '=')
                        {
                            AddTokenAndAdvance(2, new Token(TokenType.GreaterOrEqualTo, ">=", tokenStart));
                            continue;
                        }
                        else if (currentPos + 1 < Input.Length && Input[currentPos + 1] == '>')
                        {
                            AddTokenAndAdvance(2, new Token(TokenType.RightShift, ">>", tokenStart));
                            continue;
                        }
                        AddTokenAndAdvance(1, new Token(TokenType.GreaterThan, ">", tokenStart));
                        break;
                    default:
                        throw new SymbolNotRecognisedException(Input[currentPos], currentPos);

             
                }
            }
            tokens.Add(new Token(TokenType.EOF, "<EOF>", currentPos));

            return tokens;
        }

        private void AddTokenAndAdvance(int amount, Token token)
        {
            tokens.Add(token);
            currentPos += amount;
        }

        private void AddTokenAndAdvanceCombined(TokenType type, TokenType combinedType, char symbol)
        {
            if (currentPos + 1 < Input.Length && Input[currentPos + 1] == '=')
            {
                AddTokenAndAdvance(2, new Token(combinedType, $"{symbol}=", tokenStart));
                return;
            }
            AddTokenAndAdvance(1, new Token(type, symbol.ToString(), tokenStart));
        }

        private string CollectText(Func<bool> check)
        {
            string text = "";
            while (currentPos < Input.Length && check())
            {
                text += Input[currentPos];
                currentPos++;
            }
            return text;
        }
    }
}
