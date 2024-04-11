using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Verse;

namespace RW_ModularizationWeapon.Tools
{
    public static class ToolsFunction
    {
        public static TV GetOrNewWhenNull<TK,TV>(this Dictionary<TK,TV> dictonary,TK key, Func<TV> funcCreate)
        {
            TV result = default(TV);
            lock (dictonary)
            {
                if (dictonary != null && !dictonary.TryGetValue(key, out result))
                {
                    result = funcCreate();
                    dictonary.Add(key, result);
                }
            }
            return result;
        }
        public static void LogAllField(this object obj)
        {
            StringBuilder result = new StringBuilder();
            result.AppendLine($"{obj} :");
            if (obj != null)
            {
                result.AppendLine($"Hash : {obj.GetHashCode()}");
                foreach (FieldInfo field in obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                    result.AppendLine($"{field.Name} : {field.GetValue(obj)}");
            }
            Log.Message(result.ToString());
        }
    }
}
