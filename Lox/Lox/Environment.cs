using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lox
{
    public class Environment
    {
        public Environment Enclosing { get; }
        private readonly Dictionary<string, object> values = new Dictionary<string, object>();

        public Environment()
        {
            Enclosing = null;
        }

        public Environment(Environment environment)
        {
            this.Enclosing = environment;
        }

        public void Define(string name, object value)
        {
            if (this.values.ContainsKey(name))
                this.values[name] = value;
            else
                this.values.Add(name, value);
        }

        public object Get(Token name)
        {
            if (this.values.ContainsKey(name.lexeme))
                return values[name.lexeme];

            if (Enclosing != null) return Enclosing.Get(name);

            throw new RuntimeError(name, $"Undefined variable '{name.lexeme}'.");
        }

        public void Assign(Token name, object value)
        {
            if (values.ContainsKey(name.lexeme))
            {
                values[name.lexeme] = value;
                return;
            }

            if(Enclosing != null)
            {
                Enclosing.Assign(name, value);
                return;
            }

            throw new RuntimeError(name, $"Undefined variable '{name.lexeme}'.");
        }

        public object GetAt(int distance, string lexeme)
        {
            return ancestor(distance).values[lexeme];
        }

        Environment ancestor(int distance)
        {
            Environment environment = this;
            for(int i = 0; i < distance; i++)
            {
                environment = environment.Enclosing;
            }
            return environment;
        }

        public void AssignAt(int distance, Token name, object value)
        {
            ancestor(distance).Assign(name, value);
        }
    }
}
