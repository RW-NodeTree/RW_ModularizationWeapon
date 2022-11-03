using HarmonyLib;
using RimWorld;
using RW_NodeTree.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;
using Verse;

namespace RW_ModularizationWeapon
{
    public class FieldReaderDgit<T> : IEnumerable<KeyValuePair<FieldInfo, double>> where T : new()
    {
        private Type type = typeof(T);
        private readonly Dictionary<FieldInfo, double> datas = new Dictionary<FieldInfo, double>();

        public FieldReaderDgit() { }

        public FieldReaderDgit(FieldReaderDgit<T> other)
        {
            if(other != null)
            {
                datas.AddRange(other.datas);
                type = other.type;
            }
        }


        public Type UsedType
        {
            get => type;
            set
            {
                if (value != null && typeof(T).IsAssignableFrom(value))
                {
                    type = value;
                    datas.RemoveAll(x => type.IsAssignableFrom(x.Key.DeclaringType));
                }
            }
        }


        public int Count => datas.Count;


        public bool TryGetValue(string name, out double value)
        {
            value = 0;
            FieldInfo fieldInfo = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (fieldInfo != null) return datas.TryGetValue(fieldInfo, out value);
            return false;
        }


        public void SetOrAdd(string name, double value)
        {
            FieldInfo fieldInfo = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (fieldInfo != null)
            {
                Type vt = fieldInfo.FieldType;
                if (vt == typeof(int) || vt == typeof(float) ||
                   vt == typeof(long) || vt == typeof(sbyte) ||
                   vt == typeof(double))
                    datas.SetOrAdd(fieldInfo, value);
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
                SetOrAdd(
                    node.Name,
                    (double)DirectXmlToObject.GetObjectFromXmlMethod(typeof(double))(node, true)
                    );
            }
            //Log.Message(ToString());
        }


        public override string ToString()
        {
            string result = $"{GetType()}\nHash={base.GetHashCode()}\ndata : \n";
            foreach ((FieldInfo field, double value) in datas)
            {
                result += $" {field.FieldType} {field.DeclaringType}.{field.Name} : {value}\n";
            }
            return result;
        }

        public IEnumerator<KeyValuePair<FieldInfo, double>> GetEnumerator()
        {
            return datas.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return datas.GetEnumerator();
        }

        public static FieldReaderDgit<T> operator +(FieldReaderDgit<T> a, double b)
        {
            a = new FieldReaderDgit<T>(a);
            List<FieldInfo> fieldInfos = new List<FieldInfo>(a.datas.Count);
            fieldInfos.AddRange(a.datas.Keys);
            foreach (FieldInfo field in fieldInfos)
            {
                a.datas[field] += b;
            }
            return a;
        }

        public static FieldReaderDgit<T> operator -(FieldReaderDgit<T> a, double b)
        {
            a = new FieldReaderDgit<T>(a);
            List<FieldInfo> fieldInfos = new List<FieldInfo>(a.datas.Count);
            fieldInfos.AddRange(a.datas.Keys);
            foreach (FieldInfo field in fieldInfos)
            {
                a.datas[field] -= b;
            }
            return a;
        }

        public static FieldReaderDgit<T> operator *(FieldReaderDgit<T> a, double b)
        {
            a = new FieldReaderDgit<T>(a);
            List<FieldInfo> fieldInfos = new List<FieldInfo>(a.datas.Count);
            fieldInfos.AddRange(a.datas.Keys);
            foreach (FieldInfo field in fieldInfos)
            {
                a.datas[field] *= b;
            }
            return a;
        }

        public static FieldReaderDgit<T> operator /(FieldReaderDgit<T> a, double b)
        {
            List<FieldInfo> fieldInfos = new List<FieldInfo>(a.datas.Count);
            fieldInfos.AddRange(a.datas.Keys);
            foreach (FieldInfo field in fieldInfos)
            {
                a.datas[field] /= b;
            }
            return a;
        }

        public static FieldReaderDgit<T> operator %(FieldReaderDgit<T> a, double b)
        {
            List<FieldInfo> fieldInfos = new List<FieldInfo>(a.datas.Count);
            fieldInfos.AddRange(a.datas.Keys);
            foreach (FieldInfo field in fieldInfos)
            {
                a.datas[field] %= b;
            }
            return a;
        }


        public static T operator +(T a, FieldReaderDgit<T> b)
        {
            if (b != null)
            {
                a = Gen.MemberwiseClone(a);
                if(a != null)
                {
                    List<FieldInfo> fieldInfos = new List<FieldInfo>(b.datas.Count);
                    fieldInfos.AddRange(b.datas.Keys);
                    foreach (FieldInfo field in fieldInfos)
                    {
                        if (field != null && field.DeclaringType.IsAssignableFrom(a.GetType()))
                        {
                            if (field.FieldType == typeof(int)) field.SetValue(a, (int)((int)field.GetValue(a) + b.datas[field]));
                            else if (field.FieldType == typeof(float)) field.SetValue(a, (float)((float)field.GetValue(a) + b.datas[field]));
                            else if (field.FieldType == typeof(long)) field.SetValue(a, (long)((long)field.GetValue(a) + b.datas[field]));
                            else if (field.FieldType == typeof(sbyte)) field.SetValue(a, (sbyte)((sbyte)field.GetValue(a) + b.datas[field]));
                            else if (field.FieldType == typeof(double)) field.SetValue(a, (double)field.GetValue(a) + b.datas[field]);
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
                a = Gen.MemberwiseClone(a);
                if (a != null)
                {
                    List<FieldInfo> fieldInfos = new List<FieldInfo>(b.datas.Count);
                    fieldInfos.AddRange(b.datas.Keys);
                    foreach (FieldInfo field in fieldInfos)
                    {
                        if (field != null && field.DeclaringType.IsAssignableFrom(a.GetType()))
                        {
                            if (field.FieldType == typeof(int)) field.SetValue(a, (int)((int)field.GetValue(a) - b.datas[field]));
                            else if (field.FieldType == typeof(float)) field.SetValue(a, (float)((float)field.GetValue(a) - b.datas[field]));
                            else if (field.FieldType == typeof(long)) field.SetValue(a, (long)((long)field.GetValue(a) - b.datas[field]));
                            else if (field.FieldType == typeof(sbyte)) field.SetValue(a, (sbyte)((sbyte)field.GetValue(a) - b.datas[field]));
                            else if (field.FieldType == typeof(double)) field.SetValue(a, (double)field.GetValue(a) - b.datas[field]);
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
                a = Gen.MemberwiseClone(a);
                if (a != null)
                {
                    List<FieldInfo> fieldInfos = new List<FieldInfo>(b.datas.Count);
                    fieldInfos.AddRange(b.datas.Keys);
                    foreach (FieldInfo field in fieldInfos)
                    {
                        if (field != null && field.DeclaringType.IsAssignableFrom(a.GetType()))
                        {
                            if (field.FieldType == typeof(int)) field.SetValue(a, (int)((int)field.GetValue(a) * b.datas[field]));
                            else if (field.FieldType == typeof(float)) field.SetValue(a, (float)((float)field.GetValue(a) * b.datas[field]));
                            else if (field.FieldType == typeof(long)) field.SetValue(a, (long)((long)field.GetValue(a) * b.datas[field]));
                            else if (field.FieldType == typeof(sbyte)) field.SetValue(a, (sbyte)((sbyte)field.GetValue(a) * b.datas[field]));
                            else if (field.FieldType == typeof(double)) field.SetValue(a, (double)field.GetValue(a) * b.datas[field]);
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
                a = Gen.MemberwiseClone(a);
                if (a != null)
                {
                    List<FieldInfo> fieldInfos = new List<FieldInfo>(b.datas.Count);
                    fieldInfos.AddRange(b.datas.Keys);
                    foreach (FieldInfo field in fieldInfos)
                    {
                        if (field != null && field.DeclaringType.IsAssignableFrom(a.GetType()))
                        {
                            if (field.FieldType == typeof(int)) field.SetValue(a, (int)((int)field.GetValue(a) / b.datas[field]));
                            else if (field.FieldType == typeof(float)) field.SetValue(a, (float)((float)field.GetValue(a) / b.datas[field]));
                            else if (field.FieldType == typeof(long)) field.SetValue(a, (long)((long)field.GetValue(a) / b.datas[field]));
                            else if (field.FieldType == typeof(sbyte)) field.SetValue(a, (sbyte)((sbyte)field.GetValue(a) / b.datas[field]));
                            else if (field.FieldType == typeof(double)) field.SetValue(a, (double)field.GetValue(a) / b.datas[field]);
                        }
                    }
                }
            }
            return a;
        }

        public static T operator %(T a, FieldReaderDgit<T> b)
        {
            if (b != null)
            {
                a = Gen.MemberwiseClone(a);
                if (a != null)
                {
                    List<FieldInfo> fieldInfos = new List<FieldInfo>(b.datas.Count);
                    fieldInfos.AddRange(b.datas.Keys);
                    foreach (FieldInfo field in fieldInfos)
                    {
                        if (field != null && field.DeclaringType.IsAssignableFrom(a.GetType()))
                        {
                            if (field.FieldType == typeof(int)) field.SetValue(a, (int)((int)field.GetValue(a) % b.datas[field]));
                            else if (field.FieldType == typeof(float)) field.SetValue(a, (float)((float)field.GetValue(a) % b.datas[field]));
                            else if (field.FieldType == typeof(long)) field.SetValue(a, (long)((long)field.GetValue(a) % b.datas[field]));
                            else if (field.FieldType == typeof(sbyte)) field.SetValue(a, (sbyte)((sbyte)field.GetValue(a) % b.datas[field]));
                            else if (field.FieldType == typeof(double)) field.SetValue(a, (double)field.GetValue(a) % b.datas[field]);
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

            List<FieldInfo> fieldInfos = new List<FieldInfo>(a.datas.Count);
            fieldInfos.AddRange(a.datas.Keys);
            foreach (FieldInfo field in fieldInfos)
            {
                if (result.type.IsAssignableFrom(field.DeclaringType))
                {
                    result.datas.Add(field, a.datas[field]);
                }
            }
            fieldInfos = new List<FieldInfo>(b.datas.Count);
            fieldInfos.AddRange(b.datas.Keys);
            foreach (FieldInfo field in fieldInfos)
            {
                if (result.type.IsAssignableFrom(field.DeclaringType))
                {
                    if (result.datas.ContainsKey(field)) result.datas[field] += b.datas[field];
                    else result.datas.Add(field, b.datas[field]);
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

            List<FieldInfo> fieldInfos = new List<FieldInfo>(a.datas.Count);
            fieldInfos.AddRange(a.datas.Keys);
            foreach (FieldInfo field in fieldInfos)
            {
                if (result.type.IsAssignableFrom(field.DeclaringType))
                {
                    result.datas.Add(field, a.datas[field]);
                }
            }
            fieldInfos = new List<FieldInfo>(b.datas.Count);
            fieldInfos.AddRange(b.datas.Keys);
            foreach (FieldInfo field in fieldInfos)
            {
                if (result.type.IsAssignableFrom(field.DeclaringType))
                {
                    if (result.datas.ContainsKey(field)) result.datas[field] -= b.datas[field];
                    else result.datas.Add(field, b.datas[field]);
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

            List<FieldInfo> fieldInfos = new List<FieldInfo>(a.datas.Count);
            fieldInfos.AddRange(a.datas.Keys);
            foreach (FieldInfo field in fieldInfos)
            {
                if (result.type.IsAssignableFrom(field.DeclaringType))
                {
                    result.datas.Add(field, a.datas[field]);
                }
            }
            fieldInfos = new List<FieldInfo>(b.datas.Count);
            fieldInfos.AddRange(b.datas.Keys);
            foreach (FieldInfo field in fieldInfos)
            {
                if (result.type.IsAssignableFrom(field.DeclaringType))
                {
                    if (result.datas.ContainsKey(field)) result.datas[field] *= b.datas[field];
                    else result.datas.Add(field, b.datas[field]);
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

            List<FieldInfo> fieldInfos = new List<FieldInfo>(a.datas.Count);
            fieldInfos.AddRange(a.datas.Keys);
            foreach (FieldInfo field in fieldInfos)
            {
                if (result.type.IsAssignableFrom(field.DeclaringType))
                {
                    result.datas.Add(field, a.datas[field]);
                }
            }
            fieldInfos = new List<FieldInfo>(b.datas.Count);
            fieldInfos.AddRange(b.datas.Keys);
            foreach (FieldInfo field in fieldInfos)
            {
                if (result.type.IsAssignableFrom(field.DeclaringType))
                {
                    if (result.datas.ContainsKey(field)) result.datas[field] /= b.datas[field];
                    else result.datas.Add(field, b.datas[field]);
                }
            }
            return result;
        }

        public static FieldReaderDgit<T> operator %(FieldReaderDgit<T> a, FieldReaderDgit<T> b)
        {
            FieldReaderDgit<T> result = new FieldReaderDgit<T>();

            if (a == null && b == null) return result;

            a = a ?? new FieldReaderDgit<T>();
            b = b ?? new FieldReaderDgit<T>();

            if (a.type.IsAssignableFrom(b.type)) result.type = b.type;
            else if (b.type.IsAssignableFrom(a.type)) result.type = a.type;

            List<FieldInfo> fieldInfos = new List<FieldInfo>(a.datas.Count);
            fieldInfos.AddRange(a.datas.Keys);
            foreach (FieldInfo field in fieldInfos)
            {
                if (result.type.IsAssignableFrom(field.DeclaringType))
                {
                    result.datas.Add(field, a.datas[field]);
                }
            }
            fieldInfos = new List<FieldInfo>(b.datas.Count);
            fieldInfos.AddRange(b.datas.Keys);
            foreach (FieldInfo field in fieldInfos)
            {
                if (result.type.IsAssignableFrom(field.DeclaringType))
                {
                    if (result.datas.ContainsKey(field)) result.datas[field] %= b.datas[field];
                    else result.datas.Add(field, b.datas[field]);
                }
            }
            return result;
        }
    }

    public class FieldReaderBool<T> : IEnumerable<KeyValuePair<FieldInfo, bool>> where T : new()
    {
        private Type type = typeof(T);
        private readonly Dictionary<FieldInfo, bool> datas = new Dictionary<FieldInfo, bool>();

        public FieldReaderBool() { }

        public FieldReaderBool(FieldReaderBool<T> other)
        {
            if (other != null)
            {
                datas.AddRange(other.datas);
                type = other.type;
            }
        }

        public Type UsedType
        {
            get => type;
            set
            {
                if (value != null && typeof(T).IsAssignableFrom(value))
                {
                    type = value;
                    datas.RemoveAll(x => type.IsAssignableFrom(x.Key.DeclaringType));
                }
            }
        }


        public int Count => datas.Count;


        public bool TryGetValue(string name, out bool value)
        {
            value = false;
            FieldInfo fieldInfo = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (fieldInfo != null) return datas.TryGetValue(fieldInfo, out value);
            return false;
        }


        public void SetOrAdd(string name, bool value)
        {
            FieldInfo fieldInfo = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (fieldInfo != null)
            {
                Type vt = fieldInfo.FieldType;
                if (vt == typeof(bool))
                    datas.SetOrAdd(fieldInfo, value);
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
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
            foreach (XmlNode node in xmlRoot.ChildNodes)
            {
                SetOrAdd(
                    node.Name,
                    (bool)DirectXmlToObject.GetObjectFromXmlMethod(typeof(bool))(node, true)
                    );
            }
            //Log.Message(ToString());
        }


        public override string ToString()
        {
            string result = $"{GetType()}\nHash={base.GetHashCode()}\ndata : \n";
            foreach ((FieldInfo field, bool value) in datas)
            {
                result += $" {field.FieldType} {field.DeclaringType}.{field.Name} : {value}\n";
            }
            return result;
        }

        public IEnumerator<KeyValuePair<FieldInfo, bool>> GetEnumerator()
        {
            return datas.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return datas.GetEnumerator();
        }

        public static FieldReaderBool<T> operator &(FieldReaderBool<T> a, bool b)
        {
            a = new FieldReaderBool<T>(a);
            List<FieldInfo> fieldInfos = new List<FieldInfo>(a.datas.Count);
            fieldInfos.AddRange(a.datas.Keys);
            foreach (FieldInfo field in fieldInfos)
            {
                a.datas[field] = a.datas[field] && b;
            }
            return a;
        }

        public static FieldReaderBool<T> operator |(FieldReaderBool<T> a, bool b)
        {
            a = new FieldReaderBool<T>(a);
            List<FieldInfo> fieldInfos = new List<FieldInfo>(a.datas.Count);
            fieldInfos.AddRange(a.datas.Keys);
            foreach (FieldInfo field in fieldInfos)
            {
                a.datas[field] = a.datas[field] || b;
            }
            return a;
        }

        public static FieldReaderBool<T> operator ~(FieldReaderBool<T> a)
        {
            a = new FieldReaderBool<T>(a);
            List<FieldInfo> fieldInfos = new List<FieldInfo>(a.datas.Count);
            fieldInfos.AddRange(a.datas.Keys);
            foreach (FieldInfo field in fieldInfos)
            {
                a.datas[field] = !a.datas[field];
            }
            return a;
        }


        public static T operator &(T a, FieldReaderBool<T> b)
        {
            if (b != null)
            {
                a = Gen.MemberwiseClone(a);
                if (a != null)
                {
                    List<FieldInfo> fieldInfos = new List<FieldInfo>(b.datas.Count);
                    fieldInfos.AddRange(b.datas.Keys);
                    foreach (FieldInfo field in fieldInfos)
                    {
                        if (field != null && field.DeclaringType.IsAssignableFrom(a.GetType()))
                        {
                            field.SetValue(a, (bool)field.GetValue(a) && b.datas[field]);
                        }
                    }
                }
            }
            return a;
        }

        public static T operator |(T a, FieldReaderBool<T> b)
        {
            if (b != null)
            {
                a = Gen.MemberwiseClone(a);
                if (a != null)
                {
                    List<FieldInfo> fieldInfos = new List<FieldInfo>(b.datas.Count);
                    fieldInfos.AddRange(b.datas.Keys);
                    foreach (FieldInfo field in fieldInfos)
                    {
                        if (field != null && field.DeclaringType.IsAssignableFrom(a.GetType()))
                        {
                            field.SetValue(a, (bool)field.GetValue(a) || b.datas[field]);
                        }
                    }
                }
            }
            return a;
        }

        public static FieldReaderBool<T> operator &(FieldReaderBool<T> a, FieldReaderBool<T> b)
        {
            FieldReaderBool<T> result = new FieldReaderBool<T>();

            if (a == null && b == null) return result;

            a = a ?? new FieldReaderBool<T>();
            b = b ?? new FieldReaderBool<T>();

            if (a.type.IsAssignableFrom(b.type)) result.type = b.type;
            else if (b.type.IsAssignableFrom(a.type)) result.type = a.type;

            List<FieldInfo> fieldInfos = new List<FieldInfo>(a.datas.Count);
            fieldInfos.AddRange(a.datas.Keys);
            foreach (FieldInfo field in fieldInfos)
            {
                if (result.type.IsAssignableFrom(field.DeclaringType))
                {
                    result.datas.Add(field, a.datas[field]);
                }
            }
            fieldInfos = new List<FieldInfo>(b.datas.Count);
            fieldInfos.AddRange(b.datas.Keys);
            foreach (FieldInfo field in fieldInfos)
            {
                if (result.type.IsAssignableFrom(field.DeclaringType))
                {
                    if (result.datas.ContainsKey(field)) result.datas[field] = result.datas[field] && b.datas[field];
                    else result.datas.Add(field, b.datas[field]);
                }
            }
            return result;
        }

        public static FieldReaderBool<T> operator |(FieldReaderBool<T> a, FieldReaderBool<T> b)
        {
            FieldReaderBool<T> result = new FieldReaderBool<T>();

            if (a == null && b == null) return result;

            a = a ?? new FieldReaderBool<T>();
            b = b ?? new FieldReaderBool<T>();

            if (a.type.IsAssignableFrom(b.type)) result.type = b.type;
            else if (b.type.IsAssignableFrom(a.type)) result.type = a.type;

            List<FieldInfo> fieldInfos = new List<FieldInfo>(a.datas.Count);
            fieldInfos.AddRange(a.datas.Keys);
            foreach (FieldInfo field in fieldInfos)
            {
                if (result.type.IsAssignableFrom(field.DeclaringType))
                {
                    result.datas.SetOrAdd(field, a.datas[field]);
                }
            }
            fieldInfos = new List<FieldInfo>(b.datas.Count);
            fieldInfos.AddRange(b.datas.Keys);
            foreach (FieldInfo field in fieldInfos)
            {
                if (result.type.IsAssignableFrom(field.DeclaringType))
                {
                    if (result.datas.ContainsKey(field)) result.datas[field] = result.datas[field] || b.datas[field];
                    else result.datas.Add(field, b.datas[field]);
                }
            }
            return result;
        }
    }

    public class FieldReaderInst<T> : IEnumerable<KeyValuePair<FieldInfo, object>> where T : new()
    {
        private Type type = typeof(T);
        private readonly Dictionary<FieldInfo, object> datas = new Dictionary<FieldInfo, object>();

        public FieldReaderInst() { }

        public FieldReaderInst(FieldReaderInst<T> other)
        {
            if (other != null)
            {
                datas.AddRange(other.datas);
                type = other.type;
            }
        }


        public Type UsedType
        {
            get=> type;
            set
            {
                if(value != null && typeof(T).IsAssignableFrom(value))
                {
                    type = value;
                    datas.RemoveAll(x => type.IsAssignableFrom(x.Key.DeclaringType));
                }
            }
        }


        public int Count => datas.Count;


        public bool TryGetValue(string name, out object value)
        {
            value = null;
            FieldInfo fieldInfo = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if(fieldInfo != null) return datas.TryGetValue(fieldInfo, out value);
            return false;
        }


        public void SetOrAdd(string name, object value)
        {
            FieldInfo fieldInfo = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if(fieldInfo != null)
            {
                Type vt = fieldInfo.FieldType;
                if (vt != typeof(bool) && vt != typeof(int) &&
                    vt != typeof(float) && vt != typeof(long) &&
                    vt != typeof(sbyte) && vt != typeof(double) && 
                    vt.IsAssignableFrom(value.GetType()))
                    datas.SetOrAdd(fieldInfo, value);
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
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
            foreach (XmlNode node in xmlRoot.ChildNodes)
            {
                FieldInfo fieldInfo = type.GetField(node.Name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (fieldInfo != null)
                {
                    Type vt = fieldInfo.FieldType;
                    if (vt != typeof(bool) && vt != typeof(int) &&
                        vt != typeof(float)&& vt != typeof(long)&&
                        vt != typeof(sbyte)&& vt != typeof(double))
                    {
                        object data = DirectXmlToObject.GetObjectFromXmlMethod(vt)(node, true);
                        datas.Add(fieldInfo, data);
                    }

                }
            }
            //Log.Message(ToString());
        }


        public override string ToString()
        {
            string result = $"{GetType()}\nHash={base.GetHashCode()}\ndata : \n";
            foreach ((FieldInfo field, object value) in datas)
            {
                result += $" {field.FieldType} {field.DeclaringType}.{field.Name} : {value}\n";
            }
            return result;
        }

        public IEnumerator<KeyValuePair<FieldInfo, object>> GetEnumerator()
        {
            return datas.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return datas.GetEnumerator();
        }

        public static T operator &(T a, FieldReaderInst<T> b)
        {
            if (b != null)
            {
                a = Gen.MemberwiseClone(a);
                if (a != null)
                {
                    List<FieldInfo> fieldInfos = new List<FieldInfo>(b.datas.Count);
                    fieldInfos.AddRange(b.datas.Keys);
                    foreach (FieldInfo field in fieldInfos)
                    {
                        if (field != null && field.DeclaringType.IsAssignableFrom(a.GetType()))
                        {
                            object data = field.GetValue(a);
                            if(data != null) field.SetValue(a, b.datas[field] ?? data);
                        }
                    }
                }
            }
            return a;
        }

        public static T operator |(T a, FieldReaderInst<T> b)
        {
            if (b != null)
            {
                a = Gen.MemberwiseClone(a);
                if (a != null)
                {
                    List<FieldInfo> fieldInfos = new List<FieldInfo>(b.datas.Count);
                    fieldInfos.AddRange(b.datas.Keys);
                    foreach (FieldInfo field in fieldInfos)
                    {
                        if (field != null && field.DeclaringType.IsAssignableFrom(a.GetType()))
                        {
                            object data = field.GetValue(a);
                            field.SetValue(a, data ?? b.datas[field]);
                        }
                    }
                }
            }
            return a;
        }

        public static FieldReaderInst<T> operator &(FieldReaderInst<T> a, FieldReaderInst<T> b)
        {
            FieldReaderInst<T> result = new FieldReaderInst<T>();

            if (a == null && b == null) return result;

            a = a ?? new FieldReaderInst<T>();
            b = b ?? new FieldReaderInst<T>();

            if (a.type.IsAssignableFrom(b.type)) result.type = b.type;
            else if (b.type.IsAssignableFrom(a.type)) result.type = a.type;

            List<FieldInfo> fieldInfos = new List<FieldInfo>(a.datas.Count);
            fieldInfos.AddRange(a.datas.Keys);
            foreach (FieldInfo field in fieldInfos)
            {
                if (result.type.IsAssignableFrom(field.DeclaringType))
                {
                    result.datas.Add(field, a.datas[field]);
                }
            }
            fieldInfos = new List<FieldInfo>(b.datas.Count);
            fieldInfos.AddRange(b.datas.Keys);
            foreach (FieldInfo field in fieldInfos)
            {
                if (result.type.IsAssignableFrom(field.DeclaringType))
                {
                    if (result.datas.ContainsKey(field))
                    {
                        object data = result.datas[field];
                        if (data != null) result.datas[field] = b.datas[field] ?? data;
                    }
                    else result.datas.Add(field, b.datas[field]);
                }
            }
            return result;
        }

        public static FieldReaderInst<T> operator |(FieldReaderInst<T> a, FieldReaderInst<T> b)
        {
            FieldReaderInst<T> result = new FieldReaderInst<T>();

            if (a == null && b == null) return result;

            a = a ?? new FieldReaderInst<T>();
            b = b ?? new FieldReaderInst<T>();

            if (a.type.IsAssignableFrom(b.type)) result.type = b.type;
            else if (b.type.IsAssignableFrom(a.type)) result.type = a.type;

            List<FieldInfo> fieldInfos = new List<FieldInfo>(a.datas.Count);
            fieldInfos.AddRange(a.datas.Keys);
            foreach (FieldInfo field in fieldInfos)
            {
                if (result.type.IsAssignableFrom(field.DeclaringType))
                {
                    result.datas.Add(field, a.datas[field]);
                }
            }
            fieldInfos = new List<FieldInfo>(b.datas.Count);
            fieldInfos.AddRange(b.datas.Keys);
            foreach (FieldInfo field in fieldInfos)
            {
                if (result.type.IsAssignableFrom(field.DeclaringType))
                {
                    if (result.datas.ContainsKey(field))
                    {
                        object data = result.datas[field];
                        result.datas[field] = data ?? b.datas[field];
                    }
                    else result.datas.Add(field, b.datas[field]);
                }
            }
            return result;
        }
    }

}
