using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Managed.UI
{
    public enum UIMessage
    {
        /// <summary>
        /// Touch point has started 
        /// </summary>
        TouchStart,

        /// <summary>
        /// Touch point has moved
        /// </summary>
        TouchMove,

        /// <summary>
        /// Touch point has ended
        /// </summary>
        TouchEnd,
    }
}
