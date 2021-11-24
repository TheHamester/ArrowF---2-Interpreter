using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using InterpreterProject.ArrowExpressions;

namespace InterpreterProject
{
    class Program
    {
        static void Main()
        {
            string program = "";
            List<string> lines = File.ReadAllLines("input.txt").ToList();

            foreach (string str in lines)
                program += str + "\n";

            ArrowLexer lexer = new(program);
            
            List<Token> tokens = lexer.CollectTokens();

            Console.WriteLine(lexer.Input);
            ArrowParser p = new(tokens);
            IExprTree tree = p.Parse();


            ArrowInterpreter ai = new();
            ai.Interpret(tree);

        }
    }
}
