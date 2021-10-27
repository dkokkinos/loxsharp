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
        public readonly LoxClass superclass;
        private readonly Dictionary<string, LoxFunction> methods;

        public LoxClass(string name, LoxClass superclass, Dictionary<string, LoxFunction> methods)
        {
            this.Name = name;
            this.superclass = superclass;
            this.methods = methods;
        }

        public int arity()
        {
            LoxFunction initializer = findMethod("init");
            if (initializer == null) return 0;
            return initializer.arity();
        }

        public object call(Interpreter interpreter, List<object> arguments)
        {
            LoxInstance instance = new LoxInstance(this);
            LoxFunction initializer = findMethod("init");
            if (initializer != null)
                initializer.bind(instance).call(interpreter, arguments);
            return instance;
        }

        public LoxFunction findMethod(string name)
        {
            if (methods.TryGetValue(name, out LoxFunction method))
                return method;

            if (superclass != null)
                return superclass.findMethod(name);

            return null;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
