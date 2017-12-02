using System;
using System.Collections.Generic;
using System.Text;

namespace Common
{
    public interface ICloneable<out T>
    {
        T Clone();
    }
}
