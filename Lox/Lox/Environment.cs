using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lox
{
    public class Environment
    {
        private readonly Environment parent;
        private readonly Dictionary<string, object> values = new Dictionary<string, object>();

        public Environment()
        {
            parent = null;
        }

        public Environment(Environment environment)
        {
            this.parent = environment;
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

            if (parent != null) return parent.Get(name);

            throw new RuntimeError(name, $"Undefined variable '{name.lexeme}'.");
        }

        public void Assign(Token name, object value)
        {
            if (values.ContainsKey(name.lexeme))
            {
                values[name.lexeme] = value;
                return;
            }

            if(parent != null)
            {
                parent.Assign(name, value);
                return;
            }

            throw new RuntimeError(name, $"Undefined variable '{name.lexeme}'.");
        }
    }
}
