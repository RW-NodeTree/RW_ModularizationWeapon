using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RW_ModularizationWeapon.Tools
{
    public static class Tools
    {
        public static TV GetOrNewWhenNull<TK,TV>(this Dictionary<TK,TV> dictonary,TK key) where TV : new ()
        {
            TV result = default(TV);
            if (dictonary != null && !dictonary.TryGetValue(key, out result))
            {
                result = new TV();
                dictonary.Add(key, result);
            }
            return result;
        }
    }
}
