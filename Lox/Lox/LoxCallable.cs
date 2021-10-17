using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lox
{
    public interface LoxCallable
    {
        int arity();
        object call(Interpreter interpreter, List<object> arguments);
    }
}
