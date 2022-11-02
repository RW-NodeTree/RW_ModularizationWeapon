using HarmonyLib;
using RW_NodeTree.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;
using Verse;

namespace RW_ModularizationWeapon
{
    public class FieldReaderDgit<T> where T : new()
    {
        public Type type = typeof(T);
        private readonly Dictionary<string, FieldInfo> caches = new Dictionary<string, FieldInfo>();
        private readonly Dictionary<string, double> datas = new Dictionary<string, double>();

        public FieldReaderDgit() { }

        public FieldReaderDgit(FieldReaderDgit<T> other)
        {
            if(other != null)
            {
                caches.AddRange(other.caches);
                datas.AddRange(other.datas);
                type = other.type;
            }
        }

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            string typename = xmlRoot.Attributes["Class"]?.Value;
            try
            {
                type = typename != null ? GenTypes.GetTypeInAnyAssembly(typename) : type;
                if (!typeof(T).IsAssignableFrom(type)) type = typeof(T);
            }
            catch(Exception ex)
            {
                Log.Error(ex.ToString());
            }
            foreach(XmlNode node in xmlRoot.ChildNodes)
            {
                FieldInfo fieldInfo = type.GetField(node.Name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if(fieldInfo != null)
                {
                    Type vt = fieldInfo.FieldType;
                    if(vt == typeof(int) || vt == typeof(float) ||
                       vt == typeof(long) || vt == typeof(sbyte) ||
                       vt == typeof(double))
                    {
                        caches.Add(node.Name, fieldInfo);
                        double data = (double)DirectXmlToObject.GetObjectFromXmlMethod(typeof(double))(node, true);
                        datas.Add(node.Name, data);
                    }
                    
                }
            }
            Log.Message(ToString());
        }


        public override string ToString()
        {
            string result = $"FieldReaderDgit<{typeof(T)}>\nHash={base.GetHashCode()}\ndata : \n";
            foreach ((string name, double value) in datas)
            {
                result += $" {caches[name].FieldType} {caches[name].DeclaringType}.{name} : {value}\n";
            }
            return result;
        }



        public static FieldReaderDgit<T> operator +(FieldReaderDgit<T> a, double b)
        {
            a = new FieldReaderDgit<T>(a);
            foreach ((string name, double value) in a.datas)
            {
                a.datas[name] = value + b;
            }
            return a;
        }

        public static FieldReaderDgit<T> operator -(FieldReaderDgit<T> a, double b)
        {
            a = new FieldReaderDgit<T>(a);
            foreach ((string name, double value) in a.datas)
            {
                a.datas[name] = value - b;
            }
            return a;
        }

        public static FieldReaderDgit<T> operator *(FieldReaderDgit<T> a, double b)
        {
            a = new FieldReaderDgit<T>(a);
            foreach ((string name, double value) in a.datas)
            {
                a.datas[name] = value * b;
            }
            return a;
        }

        public static FieldReaderDgit<T> operator / (FieldReaderDgit<T> a, double b)
        {
            a = new FieldReaderDgit<T>(a);
            foreach ((string name, double value) in a.datas)
            {
                a.datas[name] = value / b;
            }
            return a;
        }


        public static T operator +(T a, FieldReaderDgit<T> b)
        {
            if (b != null)
            {
                a = (T)a.SimpleCopy();
                if(a != null)
                {
                    foreach ((string name, double value) in b.datas)
                    {
                        FieldInfo cache = b.caches[name];
                        if (cache != null && cache.DeclaringType.IsAssignableFrom(a.GetType()))
                        {
                            if (cache.FieldType == typeof(int)) cache.SetValue(a, (int)((int)cache.GetValue(a) + value));
                            else if (cache.FieldType == typeof(float)) cache.SetValue(a, (float)((float)cache.GetValue(a) + value));
                            else if (cache.FieldType == typeof(long)) cache.SetValue(a, (long)((long)cache.GetValue(a) + value));
                            else if (cache.FieldType == typeof(sbyte)) cache.SetValue(a, (sbyte)((sbyte)cache.GetValue(a) + value));
                            else cache.SetValue(a, (double)cache.GetValue(a) + value);
                        }
                    }
                }
            }
            return a;
        }

        public static T operator -(T a, FieldReaderDgit<T> b)
        {
            if (b != null)
            {
                a = (T)a.SimpleCopy();
                if (a != null)
                {
                    foreach ((string name, double value) in b.datas)
                    {
                        FieldInfo cache = b.caches[name];
                        if (cache != null && cache.DeclaringType.IsAssignableFrom(a.GetType()))
                        {
                            if (cache.FieldType == typeof(int)) cache.SetValue(a, (int)((int)cache.GetValue(a) - value));
                            else if (cache.FieldType == typeof(float)) cache.SetValue(a, (float)((float)cache.GetValue(a) - value));
                            else if (cache.FieldType == typeof(long)) cache.SetValue(a, (long)((long)cache.GetValue(a) - value));
                            else if (cache.FieldType == typeof(sbyte)) cache.SetValue(a, (sbyte)((sbyte)cache.GetValue(a) - value));
                            else cache.SetValue(a, (double)cache.GetValue(a) - value);
                        }
                    }
                }
            }
            return a;
        }

        public static T operator *(T a, FieldReaderDgit<T> b)
        {
            if (b != null)
            {
                a = (T)a.SimpleCopy();
                if (a != null)
                {
                    foreach ((string name, double value) in b.datas)
                    {
                        FieldInfo cache = b.caches[name];
                        if (cache != null && cache.DeclaringType.IsAssignableFrom(a.GetType()))
                        {
                            if (cache.FieldType == typeof(int)) cache.SetValue(a, (int)((int)cache.GetValue(a) * value));
                            else if (cache.FieldType == typeof(float)) cache.SetValue(a, (float)((float)cache.GetValue(a) * value));
                            else if (cache.FieldType == typeof(long)) cache.SetValue(a, (long)((long)cache.GetValue(a) * value));
                            else if (cache.FieldType == typeof(sbyte)) cache.SetValue(a, (sbyte)((sbyte)cache.GetValue(a) * value));
                            else cache.SetValue(a, (double)cache.GetValue(a) * value);
                        }
                    }
                }
            }
            return a;
        }

        public static T operator /(T a, FieldReaderDgit<T> b)
        {
            if (b != null)
            {
                a = (T)a.SimpleCopy();
                if (a != null)
                {
                    foreach ((string name, double value) in b.datas)
                    {
                        FieldInfo cache = b.caches[name];
                        if (cache != null && cache.DeclaringType.IsAssignableFrom(a.GetType()))
                        {
                            if (cache.FieldType == typeof(int)) cache.SetValue(a, (int)((int)cache.GetValue(a) / value));
                            else if (cache.FieldType == typeof(float)) cache.SetValue(a, (float)((float)cache.GetValue(a) / value));
                            else if (cache.FieldType == typeof(long)) cache.SetValue(a, (long)((long)cache.GetValue(a) / value));
                            else if (cache.FieldType == typeof(sbyte)) cache.SetValue(a, (sbyte)((sbyte)cache.GetValue(a) / value));
                            else cache.SetValue(a, (double)cache.GetValue(a) / value);
                        }
                    }
                }
            }
            return a;
        }

        public static FieldReaderDgit<T> operator +(FieldReaderDgit<T> a, FieldReaderDgit<T> b)
        {
            FieldReaderDgit<T> result = new FieldReaderDgit<T>();

            if (a == null && b == null) return result;

            a = a ?? new FieldReaderDgit<T>();
            b = b ?? new FieldReaderDgit<T>();

            if (a.type.IsAssignableFrom(b.type)) result.type = b.type;
            else if (b.type.IsAssignableFrom(a.type)) result.type = a.type;

            foreach ((string name, double value) in a.datas)
            {
                FieldInfo cache = a.caches[name];
                if (result.type.IsAssignableFrom(cache.DeclaringType))
                {
                    result.caches.SetOrAdd(name, cache);
                    result.datas.SetOrAdd(name, value);
                }
            }
            foreach ((string name, double value) in b.datas)
            {
                FieldInfo cache = b.caches[name];
                if (result.type.IsAssignableFrom(cache.DeclaringType))
                {
                    result.caches.SetOrAdd(name, cache);
                    if (result.datas.ContainsKey(name)) result.datas[name] += b.datas[name];
                    else result.datas.Add(name, value);
                }
            }
            return result;
        }

        public static FieldReaderDgit<T> operator -(FieldReaderDgit<T> a, FieldReaderDgit<T> b)
        {
            FieldReaderDgit<T> result = new FieldReaderDgit<T>();

            if (a == null && b == null) return result;

            a = a ?? new FieldReaderDgit<T>();
            b = b ?? new FieldReaderDgit<T>();

            if (a.type.IsAssignableFrom(b.type)) result.type = b.type;
            else if (b.type.IsAssignableFrom(a.type)) result.type = a.type;

            foreach ((string name, double value) in a.datas)
            {
                FieldInfo cache = a.caches[name];
                if (result.type.IsAssignableFrom(cache.DeclaringType))
                {
                    result.caches.SetOrAdd(name, cache);
                    result.datas.SetOrAdd(name, value);
                }
            }
            foreach ((string name, double value) in b.datas)
            {
                FieldInfo cache = b.caches[name];
                if (result.type.IsAssignableFrom(cache.DeclaringType))
                {
                    result.caches.SetOrAdd(name, cache);
                    if (result.datas.ContainsKey(name)) result.datas[name] -= b.datas[name];
                    else result.datas.Add(name, value);
                }
            }
            return result;
        }

        public static FieldReaderDgit<T> operator *(FieldReaderDgit<T> a, FieldReaderDgit<T> b)
        {
            FieldReaderDgit<T> result = new FieldReaderDgit<T>();

            if (a == null && b == null) return result;

            a = a ?? new FieldReaderDgit<T>();
            b = b ?? new FieldReaderDgit<T>();

            if (a.type.IsAssignableFrom(b.type)) result.type = b.type;
            else if (b.type.IsAssignableFrom(a.type)) result.type = a.type;

            foreach ((string name, double value) in a.datas)
            {
                FieldInfo cache = a.caches[name];
                if (result.type.IsAssignableFrom(cache.DeclaringType))
                {
                    result.caches.SetOrAdd(name, cache);
                    result.datas.SetOrAdd(name, value);
                }
            }
            foreach ((string name, double value) in b.datas)
            {
                FieldInfo cache = b.caches[name];
                if (result.type.IsAssignableFrom(cache.DeclaringType))
                {
                    result.caches.SetOrAdd(name, cache);
                    if (result.datas.ContainsKey(name)) result.datas[name] *= b.datas[name];
                    else result.datas.Add(name, value);
                }
            }
            return result;
        }

        public static FieldReaderDgit<T> operator /(FieldReaderDgit<T> a, FieldReaderDgit<T> b)
        {
            FieldReaderDgit<T> result = new FieldReaderDgit<T>();

            if (a == null && b == null) return result;

            a = a ?? new FieldReaderDgit<T>();
            b = b ?? new FieldReaderDgit<T>();

            if (a.type.IsAssignableFrom(b.type)) result.type = b.type;
            else if (b.type.IsAssignableFrom(a.type)) result.type = a.type;

            foreach ((string name, double value) in a.datas)
            {
                FieldInfo cache = a.caches[name];
                if (result.type.IsAssignableFrom(cache.DeclaringType))
                {
                    result.caches.SetOrAdd(name, cache);
                    result.datas.SetOrAdd(name, value);
                }
            }
            foreach ((string name, double value) in b.datas)
            {
                FieldInfo cache = b.caches[name];
                if (result.type.IsAssignableFrom(cache.DeclaringType))
                {
                    result.caches.SetOrAdd(name, cache);
                    if (result.datas.ContainsKey(name)) result.datas[name] /= b.datas[name];
                    else result.datas.Add(name, value);
                }
            }
            return result;
        }
    }
}
