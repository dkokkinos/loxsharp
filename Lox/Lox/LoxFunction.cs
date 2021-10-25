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
        private readonly Environment closure;

        public LoxFunction(Stmt.Function declaration, Environment closure)
        {
            this.closure = closure;
            this.declaration = declaration;
        }

        public int arity() => declaration._params.Count;

        public object call(Interpreter interpreter, List<object> arguments)
        {
            Environment environment = new Environment(this.closure);
            for(int i = 0; i < declaration._params.Count; i++)
            {
                environment.Define(declaration._params[i].lexeme, arguments[i]);
            }
            try
            {
                interpreter.executeBlock(declaration.body, environment);
            }catch(Return returnValue)
            {
                return returnValue.Value;
            }
            return null;
        }

        public object bind(LoxInstance loxInstance)
        {
            Environment environment = new Environment(closure);
            environment.Define("this", loxInstance);
            return new LoxFunction(declaration, environment);
        }

        public override string ToString() => $"<fn {declaration.name.lexeme}>";
    }
}
