using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lox
{
    public class Return : Exception
    {
        public object Value { get; }

        public Return(object value)
        {
            Value = value;
        }
    }
}
