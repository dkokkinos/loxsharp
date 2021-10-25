using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lox
{
    public class LoxInstance
    {
        private LoxClass klass;
        private readonly Dictionary<string, object> fields = new Dictionary<string, object>();

        public LoxInstance(LoxClass klass)
        {
            this.klass = klass;
        }

        public object Get(Token name)
        {
            if (this.fields.TryGetValue(name.lexeme, out object value))
                return value;

            throw new RuntimeError(name, $"Undefined property '{name.lexeme}'.");
        }

        public override string ToString()
        {
            return klass.Name + " instance";
        }

        public void Set(Token name, object value)
        {
            if (fields.ContainsKey(name.lexeme))
                fields[name.lexeme] = value;
            else
                fields.Add(name.lexeme, value);
        }
    }
}
