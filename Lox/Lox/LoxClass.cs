using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lox
{
    public class LoxClass : LoxCallable
    {
        public string Name { get; }

        public LoxClass(string name)
        {
            this.Name = name;
        }

        public int arity() => 0;

        public object call(Interpreter interpreter, List<object> arguments)
        {
            LoxInstance instance = new LoxInstance(this);
            return instance;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
