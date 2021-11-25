using System;
using System.Linq;
using System.Collections.Generic;
using InterpreterProject.ArrowExceptions;

namespace InterpreterProject
{
    public class ArrowLexer
    {
        public string Input { get; set; }

        private List<Token> tokens;
        private int currentPos;
        private int currentLine;

        private const string Letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        private const string Digits = "0123456789";
        private readonly Dictionary<string, TokenType> Keywords = new()
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
            currentLine = 1;

            while (currentPos < Input.Length)
            {

                if (Digits.Contains(Input[currentPos].ToString()))
                {
                    tokens.Add(new Token(TokenType.Note, CollectText(() => Digits.Contains(Input[currentPos].ToString())), currentLine));
                    continue;
                }
                else if (Letters.Contains(Input[currentPos].ToString()))
                {
                    string text = CollectText(() => (Letters + Digits).Contains(Input[currentPos].ToString()));

                    if (Keywords.Keys.Contains(text))
                        tokens.Add(new Token(Keywords[text], text, currentLine));
                    else
                        tokens.Add(new Token(TokenType.Identifier, text, currentLine));

                    continue;
                }

                switch (Input[currentPos])
                {
                    case ' ':
                        currentPos++;
                        break;
                    case '[':
                        AddTokenAndAdvance(1, new Token(TokenType.LeftBracket, "[", currentLine));
                        break;
                    case ']':
                        AddTokenAndAdvance(1, new Token(TokenType.RightBracket, "]", currentLine));
                        break;
                    case '(':
                        AddTokenAndAdvance(1, new Token(TokenType.LeftParenthesy, "(", currentLine));
                        break;
                    case ')':
                        AddTokenAndAdvance(1, new Token(TokenType.RightParenthesy, ")", currentLine));
                        break;
                    case ',':
                        AddTokenAndAdvance(1, new Token(TokenType.Coma, ",", currentLine));
                        break;
                    case '\t':
                        AddTokenAndAdvance(1, new Token(TokenType.Tab, "<TAB>", currentLine));
                        break;
                    case '\n':
                        AddTokenAndAdvance(1, new Token(TokenType.EOL, "<EOL>", currentLine));
                        currentLine++;
                        break;
                    case '~':
                        AddTokenAndAdvance(1, new Token(TokenType.Invert, "~", currentLine));
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
                            AddTokenAndAdvance(2, new Token(TokenType.DivideEquals, "/=", currentLine));
                            continue;
                        }
                        else if (currentPos + 1 < Input.Length && Input[currentPos + 1] == '/')
                        {
                            currentPos += 2;
                            AddTokenAndAdvance(0, new Token(TokenType.Comment, $"//{ CollectText(() => Input[currentPos] != '\n') }", currentLine));
                            continue;
                        }
                        AddTokenAndAdvance(1, new Token(TokenType.Divide, "/", currentLine));
                        break;
                    case '<':
                        if (currentPos + 1 < Input.Length && Input[currentPos + 1] == '=')
                        {
                            AddTokenAndAdvance(2, new Token(TokenType.LessOrEqualTo, "<=", currentLine));
                            continue;
                        }
                        else if (currentPos + 1 < Input.Length && Input[currentPos + 1] == '<')
                        {
                            if (currentPos + 2 < Input.Length && Input[currentPos + 2] == '=')
                            {
                                AddTokenAndAdvance(3, new Token(TokenType.LeftShiftEquals, "<<=", currentLine));
                                continue;
                            }
                            AddTokenAndAdvance(2, new Token(TokenType.LeftShift, "<<", currentLine));
                            continue;
                        }
                        AddTokenAndAdvance(1, new Token(TokenType.LessThan, "<", currentLine));
                        break;
                    case '>':
                        if (currentPos + 1 < Input.Length && Input[currentPos + 1] == '=')
                        {
                            AddTokenAndAdvance(2, new Token(TokenType.GreaterOrEqualTo, ">=", currentLine));
                            continue;
                        }
                        else if (currentPos + 1 < Input.Length && Input[currentPos + 1] == '>')
                        {
                            if (currentPos + 2 < Input.Length && Input[currentPos + 2] == '=')
                            {
                                AddTokenAndAdvance(3, new Token(TokenType.RightShiftEquals, ">>=", currentLine));
                                continue;
                            }
                            AddTokenAndAdvance(2, new Token(TokenType.RightShift, ">>", currentLine));
                            continue;
                        }
                        AddTokenAndAdvance(1, new Token(TokenType.GreaterThan, ">", currentLine));
                        break;
                    default:
                        throw new SymbolNotRecognisedException(Input[currentPos], currentPos, currentLine);      
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
                AddTokenAndAdvance(2, new Token(combinedType, $"{symbol}=", currentLine));
                return;
            }
            AddTokenAndAdvance(1, new Token(type, symbol.ToString(), currentLine));
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
