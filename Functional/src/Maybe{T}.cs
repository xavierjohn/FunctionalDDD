using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Functional;
public struct Maybe<T> : IEquatable<Maybe<T>>, IEquatable<object>
{
    public bool Equals(Maybe<T> other)
    {
        throw new NotImplementedException();
    }
}
