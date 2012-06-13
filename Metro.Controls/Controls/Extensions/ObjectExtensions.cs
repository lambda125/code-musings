using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metro.Controls.Extensions
{
    public static class ObjectExtensions
    {
        public static bool IsNotNull(this object thisObject)
        {
            return (thisObject != null);
        }

        public static bool IsNull(this object thisObject)
        {
            return (thisObject == null);
        }
    }
}
