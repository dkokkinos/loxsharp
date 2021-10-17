using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lox
{
    public class LoxFunction : LoxCallable
    {
        private readonly Stmt.Function declaration;

        public LoxFunction(Stmt.Function declaration)
        {
            this.declaration = declaration;
        }

        public int arity() => declaration._params.Count;

        public object call(Interpreter interpreter, List<object> arguments)
        {
            Environment environment = new Environment(interpreter.globals);
            for(int i = 0; i < declaration._params.Count; i++)
            {
                environment.Define(declaration._params[i].lexeme, arguments[i]);
            }

            interpreter.executeBlock(declaration.body, environment);
            return null;
        }

        public override string ToString() => $"<fn {declaration.name.lexeme}>";
    }
}
