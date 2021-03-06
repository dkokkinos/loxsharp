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
        private readonly bool isInitializer;

        public LoxFunction(Stmt.Function declaration, Environment closure, bool isInitializer)
        {
            this.closure = closure;
            this.declaration = declaration;
            this.isInitializer = isInitializer;
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
                if (isInitializer) return closure.GetAt(0, "this");
                return returnValue.Value;
            }
            if (isInitializer) return closure.GetAt(0, "this");
            return null;
        }

        public LoxFunction bind(LoxInstance loxInstance)
        {
            Environment environment = new Environment(closure);
            environment.Define("this", loxInstance);
            return new LoxFunction(declaration, environment, isInitializer);
        }

        public override string ToString() => $"<fn {declaration.name.lexeme}>";
    }
}
