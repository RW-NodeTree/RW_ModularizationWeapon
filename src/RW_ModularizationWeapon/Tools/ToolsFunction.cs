using HarmonyLib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Verse;

namespace RW_ModularizationWeapon.Tools
{
    public static class ToolsFunction
    {
        private static readonly ConcurrentDictionary<Type, FieldInfo[]> cachedFields = new ConcurrentDictionary<Type, FieldInfo[]>();
        public static TV GetOrNewWhenNull<TK, TV>(this Dictionary<TK, TV> dictonary, TK key, Func<TV> funcCreate)
        {
            lock (dictonary)
            {
                if (!dictonary.TryGetValue(key, out TV? result) || result == null)
                {
                    result = funcCreate();
                    dictonary[key] = result;
                    return result;
                }
                return result;
            }
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

        public static FieldInfo[] GetCachedInstanceFields(this Type type)
            => cachedFields.GetOrAdd(type, (x) => x.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public));
    }
}
