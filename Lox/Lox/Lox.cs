using System;
using System.Collections.Generic;
using System.IO;

namespace Lox
{
    class Lox
    {
        private static readonly Interpreter _interpreter = new Interpreter();
        static bool hadError = false;
        static bool hadRuntimeError = false;

        static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: jlox [script]");
            }
            else if (args.Length == 1)
            {
                runFile(args[0]);
            }
            else
            {
                runPrompt();
            }
        }

        private static void runFile(string path)
        {
            if (hadError) return;
            string script = File.ReadAllText(path);
            run(script);
        }

        private static void runPrompt()
        {
            for (; ; ) {
                Console.WriteLine("> ");
                String line = Console.ReadLine();
                if (line == null) break;
                hadError = false;
                hadRuntimeError = false;
                var path = Path.Combine(Directory.GetCurrentDirectory(), "../../..", line);
                var source = File.ReadAllText(path);
                run(source);
            }
        }

        private static int run(String source)
        {
            Scanner scanner = new Scanner(source);
            List<Token> tokens = scanner.scanTokens();

            Parser parser = new Parser(tokens);
            var statements = parser.parse();

            if (hadError) return 65;
            if (hadRuntimeError) return 70;

            _interpreter.Interpret(statements);
            return 0;
        }

        public static void error(int line, string message)
        {
            report(line, "", message);
        }

        private static void report(int line, String where,
                                   String message)
        {
            Console.WriteLine(
                "[line " + line + "] Error" + where + ": " + message);
            hadError = true;
        }

        public static void error(Token token, string message)
        {
            if (token.type == TokenType.EOF)
            {
                report(token.line, " at end", message);
            }
            else
            {
                report(token.line, " at '" + token.lexeme + "'", message);
            }
        }


        public static void RuntimeError(RuntimeError error)
        {
            Console.WriteLine(error.Message);
            Console.WriteLine($"[line {error.Token.line}]");
            hadRuntimeError = true;
        }

    }
}
