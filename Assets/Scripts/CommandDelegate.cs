using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts
{
    /// <summary>
    /// Delegate to pass methods to instance of the Command struct.
    /// </summary>
    /// <param name="parameter"></param>
    /// <returns></returns>
    public delegate CommandOutput CommandDelegate(string parameter);
}
