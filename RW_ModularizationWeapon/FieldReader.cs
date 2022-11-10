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
using System.Xml.Linq;
using UnityEngine;
using Verse;
using static HarmonyLib.Code;

namespace RW_ModularizationWeapon
{
    public class FieldReaderDgit<T> : IDictionary<FieldInfo, double>
    {
        private double? defaultValue;
        private Type type = typeof(T);
        private readonly Dictionary<FieldInfo, double> datas = new Dictionary<FieldInfo, double>();

        public FieldReaderDgit() { }

        public FieldReaderDgit(FieldReaderDgit<T> other)
        {
            if(other != null)
            {
                datas.AddRange(other.datas);
                type = other.type;
                defaultValue = other.defaultValue;
            }
        }


        public double DefaultValue
        {
            get => defaultValue ?? 0;
            set => defaultValue = value;
        }

        public bool HasDefaultValue => defaultValue.HasValue;


        public Type UsedType
        {
            get => type;
            set
            {
                if (value != null && typeof(T).IsAssignableFrom(value))
                {
                    type = value;
                    datas.RemoveAll(x => !type.IsAssignableFrom(x.Key.DeclaringType));
                }
            }
        }


        public int Count => datas.Count;

        public ICollection<FieldInfo> Keys => datas.Keys;

        public ICollection<double> Values => datas.Values;

        public bool IsReadOnly => ((ICollection<KeyValuePair<FieldInfo, double>>)datas).IsReadOnly;

        public double this[FieldInfo key] 
        {
            get => ((IDictionary<FieldInfo, double>)datas)[key];
            set => Add(key, value);
        }

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
            if (fieldInfo != null) Add(fieldInfo, value);
        }

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            string typename = xmlRoot.Attributes["Class"]?.Value;
            try
            {
                type = typename != null ? (GenTypes.GetTypeInAnyAssembly(typename) ?? type) : type;
                if (!typeof(T).IsAssignableFrom(type)) type = typeof(T);
            }
            catch(Exception ex)
            {
                Log.Error(ex.ToString());
            }

            try
            {
                string defaultValue = xmlRoot.Attributes["Default"]?.Value;
                if (defaultValue != null) this.defaultValue = ParseHelper.FromString<double>(defaultValue);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }

            /**
            <xx Class="c# type" Default="default_value">
                <member_name_of_type>value</member_name_of_type>
            </xx>
            **/
            foreach (XmlNode node in xmlRoot.ChildNodes)
            {
                try
                {
                    SetOrAdd(
                        node.Name,
                        ParseHelper.FromString<double>(node.InnerText)
                        );
                }
                catch(Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
            //Log.Message(ToString());
        }


        public override string ToString()
        {
            string result = $"{GetType()}\nHash={base.GetHashCode()}\ndefaultValue={defaultValue}\ndata : \n";
            foreach ((FieldInfo field, double value) in datas)
            {
                result += $" {field.FieldType} {field.DeclaringType}.{field.Name} : {value}\n";
            }
            return result;
        }

        public bool ContainsKey(FieldInfo key) => datas.ContainsKey(key);

        public void Add(FieldInfo key, double value)
        {
            if (key != null)
            {
                Type vt = key.FieldType;
                if (vt == typeof(int) || vt == typeof(float) ||
                   vt == typeof(long) || vt == typeof(sbyte) ||
                   vt == typeof(double))
                    datas.SetOrAdd(key, value);
                else throw new ArgumentException($"not support value(name={key.Name},type={vt})");
            }
        }

        public bool Remove(FieldInfo key) => datas.Remove(key);

        public bool TryGetValue(FieldInfo key, out double value) => datas.TryGetValue(key, out value);

        public void Add(KeyValuePair<FieldInfo, double> item) => Add(item.Key, item.Value);

        public void Clear() => datas.Clear();

        public bool Contains(KeyValuePair<FieldInfo, double> item)=> datas.Contains(item);

        public void CopyTo(KeyValuePair<FieldInfo, double>[] array, int arrayIndex) => ((ICollection<KeyValuePair<FieldInfo, double>>)datas).CopyTo(array, arrayIndex);

        public bool Remove(KeyValuePair<FieldInfo, double> item) => ((ICollection<KeyValuePair<FieldInfo, double>>)datas).Remove(item);

        public IEnumerator<KeyValuePair<FieldInfo, double>> GetEnumerator() => datas.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => datas.GetEnumerator();

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
                    foreach (FieldInfo field in a.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                    {
                        if (field.DeclaringType.IsAssignableFrom(b.type))
                        {
                            double value;
                            if (!b.datas.TryGetValue(field, out value)) value = b.DefaultValue;
                            if (field != null && b.datas.ContainsKey(field))
                            {
                                if (field.FieldType == typeof(int)) field.SetValue(a, (int)((int)field.GetValue(a) + value));
                                else if (field.FieldType == typeof(float)) field.SetValue(a, (float)((float)field.GetValue(a) + value));
                                else if (field.FieldType == typeof(long)) field.SetValue(a, (long)((long)field.GetValue(a) + value));
                                else if (field.FieldType == typeof(sbyte)) field.SetValue(a, (sbyte)((sbyte)field.GetValue(a) + value));
                                else if (field.FieldType == typeof(double)) field.SetValue(a, (double)field.GetValue(a) + value);
                            }
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
                    foreach (FieldInfo field in a.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                    {
                        if (field.DeclaringType.IsAssignableFrom(b.type))
                        {
                            double value;
                            if (!b.datas.TryGetValue(field, out value)) value = b.DefaultValue;
                            if (field != null && b.datas.ContainsKey(field))
                            {
                                if (field.FieldType == typeof(int)) field.SetValue(a, (int)((int)field.GetValue(a) - value));
                                else if (field.FieldType == typeof(float)) field.SetValue(a, (float)((float)field.GetValue(a) - value));
                                else if (field.FieldType == typeof(long)) field.SetValue(a, (long)((long)field.GetValue(a) - value));
                                else if (field.FieldType == typeof(sbyte)) field.SetValue(a, (sbyte)((sbyte)field.GetValue(a) - value));
                                else if (field.FieldType == typeof(double)) field.SetValue(a, (double)field.GetValue(a) - value);
                            }
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
                    foreach (FieldInfo field in a.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                    {
                        if (field.DeclaringType.IsAssignableFrom(b.type))
                        {
                            double value;
                            if (!b.datas.TryGetValue(field, out value)) value = b.DefaultValue;
                            if (field != null && b.datas.ContainsKey(field))
                            {
                                if (field.FieldType == typeof(int)) field.SetValue(a, (int)((int)field.GetValue(a) * value));
                                else if (field.FieldType == typeof(float)) field.SetValue(a, (float)((float)field.GetValue(a) * value));
                                else if (field.FieldType == typeof(long)) field.SetValue(a, (long)((long)field.GetValue(a) * value));
                                else if (field.FieldType == typeof(sbyte)) field.SetValue(a, (sbyte)((sbyte)field.GetValue(a) * value));
                                else if (field.FieldType == typeof(double)) field.SetValue(a, (double)field.GetValue(a) * value);
                            }
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
                    foreach (FieldInfo field in a.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                    {
                        if (field.DeclaringType.IsAssignableFrom(b.type))
                        {
                            double value;
                            if (!b.datas.TryGetValue(field, out value)) value = b.DefaultValue;
                            if (field != null && b.datas.ContainsKey(field))
                            {
                                if (field.FieldType == typeof(int)) field.SetValue(a, (int)((int)field.GetValue(a) / value));
                                else if (field.FieldType == typeof(float)) field.SetValue(a, (float)((float)field.GetValue(a) / value));
                                else if (field.FieldType == typeof(long)) field.SetValue(a, (long)((long)field.GetValue(a) / value));
                                else if (field.FieldType == typeof(sbyte)) field.SetValue(a, (sbyte)((sbyte)field.GetValue(a) / value));
                                else if (field.FieldType == typeof(double)) field.SetValue(a, (double)field.GetValue(a) / value);
                            }
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
                    foreach (FieldInfo field in a.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                    {
                        if(field.DeclaringType.IsAssignableFrom(b.type))
                        {
                            double value;
                            if (!b.datas.TryGetValue(field, out value)) value = b.DefaultValue;
                            if (field != null && b.datas.ContainsKey(field))
                            {
                                if (field.FieldType == typeof(int)) field.SetValue(a, (int)((int)field.GetValue(a) % value));
                                else if (field.FieldType == typeof(float)) field.SetValue(a, (float)((float)field.GetValue(a) % value));
                                else if (field.FieldType == typeof(long)) field.SetValue(a, (long)((long)field.GetValue(a) % value));
                                else if (field.FieldType == typeof(sbyte)) field.SetValue(a, (sbyte)((sbyte)field.GetValue(a) % value));
                                else if (field.FieldType == typeof(double)) field.SetValue(a, (double)field.GetValue(a) % value);
                            }
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

            foreach (FieldInfo field in a.datas.Keys)
            {
                if (result.type.IsAssignableFrom(field.DeclaringType))
                {
                    if (b.datas.ContainsKey(field)) result.datas.Add(field, a.datas[field] + b.datas[field]);
                    else result.datas.Add(field, a.datas[field] + b.DefaultValue);
                }
            }

            foreach (FieldInfo field in b.datas.Keys)
            {
                if (result.type.IsAssignableFrom(field.DeclaringType) && !result.datas.ContainsKey(field))
                {
                    if (a.datas.ContainsKey(field)) result.datas.Add(field, a.datas[field] + b.datas[field]);
                    else result.datas.Add(field, a.DefaultValue + b.datas[field]);
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

            foreach (FieldInfo field in a.datas.Keys)
            {
                if (result.type.IsAssignableFrom(field.DeclaringType))
                {
                    if (b.datas.ContainsKey(field)) result.datas.Add(field, a.datas[field] - b.datas[field]);
                    else result.datas.Add(field, a.datas[field] - b.DefaultValue);
                }
            }

            foreach (FieldInfo field in b.datas.Keys)
            {
                if (result.type.IsAssignableFrom(field.DeclaringType) && !result.datas.ContainsKey(field))
                {
                    if (a.datas.ContainsKey(field)) result.datas.Add(field, a.datas[field] - b.datas[field]);
                    else result.datas.Add(field, a.DefaultValue - b.datas[field]);
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

            foreach (FieldInfo field in a.datas.Keys)
            {
                if (result.type.IsAssignableFrom(field.DeclaringType))
                {
                    if(b.datas.ContainsKey(field)) result.datas.Add(field, a.datas[field] * b.datas[field]);
                    else result.datas.Add(field, a.datas[field] * b.DefaultValue);
                }
            }

            foreach (FieldInfo field in b.datas.Keys)
            {
                if (result.type.IsAssignableFrom(field.DeclaringType) && !result.datas.ContainsKey(field))
                {
                    if (a.datas.ContainsKey(field)) result.datas.Add(field, a.datas[field] * b.datas[field]);
                    else result.datas.Add(field, a.DefaultValue * b.datas[field]);
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

            foreach (FieldInfo field in a.datas.Keys)
            {
                if (result.type.IsAssignableFrom(field.DeclaringType))
                {
                    if (b.datas.ContainsKey(field)) result.datas.Add(field, a.datas[field] / b.datas[field]);
                    else result.datas.Add(field, a.datas[field] / b.DefaultValue);
                }
            }

            foreach (FieldInfo field in b.datas.Keys)
            {
                if (result.type.IsAssignableFrom(field.DeclaringType) && !result.datas.ContainsKey(field))
                {
                    if (a.datas.ContainsKey(field)) result.datas.Add(field, a.datas[field] / b.datas[field]);
                    else result.datas.Add(field, a.DefaultValue / b.datas[field]);
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

            foreach (FieldInfo field in a.datas.Keys)
            {
                if (result.type.IsAssignableFrom(field.DeclaringType))
                {
                    if (b.datas.ContainsKey(field)) result.datas.Add(field, a.datas[field] % b.datas[field]);
                    else result.datas.Add(field, a.datas[field] % b.DefaultValue);
                }
            }

            foreach (FieldInfo field in b.datas.Keys)
            {
                if (result.type.IsAssignableFrom(field.DeclaringType) && !result.datas.ContainsKey(field))
                {
                    if (a.datas.ContainsKey(field)) result.datas.Add(field, a.datas[field] % b.datas[field]);
                    else result.datas.Add(field, a.DefaultValue % b.datas[field]);
                }
            }
            return result;
        }
    }

    public class FieldReaderBool<T> : IDictionary<FieldInfo, bool>
    {
        private bool? defaultValue;
        private Type type = typeof(T);
        private readonly Dictionary<FieldInfo, bool> datas = new Dictionary<FieldInfo, bool>();

        public FieldReaderBool() { }

        public FieldReaderBool(FieldReaderBool<T> other)
        {
            if (other != null)
            {
                datas.AddRange(other.datas);
                type = other.type;
                defaultValue = other.defaultValue;
            }
        }

        public bool DefaultValue
        {
            get => defaultValue ?? false;
            set => defaultValue = value;
        }


        public bool HasDefaultValue => defaultValue.HasValue;


        public Type UsedType
        {
            get => type;
            set
            {
                if (value != null && typeof(T).IsAssignableFrom(value))
                {
                    type = value;
                    datas.RemoveAll(x => !type.IsAssignableFrom(x.Key.DeclaringType));
                }
            }
        }


        public int Count => datas.Count;

        public ICollection<FieldInfo> Keys => datas.Keys;

        public ICollection<bool> Values => datas.Values;

        public bool IsReadOnly => ((ICollection<KeyValuePair<FieldInfo, bool>>)datas).IsReadOnly;

        public bool this[FieldInfo key]
        {
            get => datas[key];
            set => Add(key, value);
        }


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
            if (fieldInfo != null) Add(fieldInfo, value);
        }


        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            string typename = xmlRoot.Attributes["Class"]?.Value;
            try
            {
                type = typename != null ? (GenTypes.GetTypeInAnyAssembly(typename) ?? type) : type;
                if (!typeof(T).IsAssignableFrom(type)) type = typeof(T);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }

            try
            {
                string defaultValue = xmlRoot.Attributes["Default"]?.Value;
                if (defaultValue != null) this.defaultValue = ParseHelper.FromString<bool>(defaultValue);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }

            /**
            <xx Class="c# type" Default="default_value">
                <member_name_of_type>value</member_name_of_type>
            </xx>
            **/
            foreach (XmlNode node in xmlRoot.ChildNodes)
            {
                try
                {
                    SetOrAdd(
                        node.Name,
                        ParseHelper.FromString<bool>(node.InnerText)
                        );
                }
                catch(Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
            //Log.Message(ToString());
        }


        public override string ToString()
        {
            string result = $"{GetType()}\nHash={base.GetHashCode()}\ndefaultValue={defaultValue}\ndata : \n";
            foreach ((FieldInfo field, bool value) in datas)
            {
                result += $" {field.FieldType} {field.DeclaringType}.{field.Name} : {value}\n";
            }
            return result;
        }

        public bool ContainsKey(FieldInfo key) => datas.ContainsKey(key);

        public void Add(FieldInfo key, bool value)
        {
            if (key != null)
            {
                Type vt = key.FieldType;
                if (vt == typeof(bool))
                    datas.SetOrAdd(key, value);
                else throw new ArgumentException($"not support value(name={key.Name},type={vt})");
            }
        }

        public bool Remove(FieldInfo key) => datas.Remove(key);

        public bool TryGetValue(FieldInfo key, out bool value) => datas.TryGetValue(key, out value);

        public void Add(KeyValuePair<FieldInfo, bool> item) => Add(item.Key, item.Value);

        public void Clear() => datas.Clear();

        public bool Contains(KeyValuePair<FieldInfo, bool> item) => datas.Contains(item);

        public void CopyTo(KeyValuePair<FieldInfo, bool>[] array, int arrayIndex) => ((ICollection<KeyValuePair<FieldInfo, bool>>)datas).CopyTo(array, arrayIndex);

        public bool Remove(KeyValuePair<FieldInfo, bool> item) => ((ICollection<KeyValuePair<FieldInfo, bool>>)datas).Remove(item);

        public IEnumerator<KeyValuePair<FieldInfo, bool>> GetEnumerator() => datas.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => datas.GetEnumerator();

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

        public static FieldReaderBool<T> operator !(FieldReaderBool<T> a)
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
                    foreach (FieldInfo field in a.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                    {
                        if (field.DeclaringType.IsAssignableFrom(b.type))
                        {
                            bool value;
                            if (!b.datas.TryGetValue(field, out value)) value = b.DefaultValue;
                            if (field != null && b.datas.ContainsKey(field))
                            {
                               field.SetValue(a, (bool)field.GetValue(a) && value);
                            }
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
                    foreach (FieldInfo field in a.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                    {
                        if (field.DeclaringType.IsAssignableFrom(b.type))
                        {
                            bool value;
                            if (!b.datas.TryGetValue(field, out value)) value = b.DefaultValue;
                            if (field != null && b.datas.ContainsKey(field))
                            {
                                field.SetValue(a, (bool)field.GetValue(a) || value);
                            }
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

            foreach (FieldInfo field in a.datas.Keys)
            {
                if (result.type.IsAssignableFrom(field.DeclaringType))
                {
                    if (b.datas.ContainsKey(field)) result.datas.Add(field, a.datas[field] && b.datas[field]);
                    else result.datas.Add(field, a.datas[field] && b.DefaultValue);
                }
            }

            foreach (FieldInfo field in b.datas.Keys)
            {
                if (result.type.IsAssignableFrom(field.DeclaringType) && !result.datas.ContainsKey(field))
                {
                    if (a.datas.ContainsKey(field)) result.datas.Add(field, a.datas[field] && b.datas[field]);
                    else result.datas.Add(field, a.DefaultValue && b.datas[field]);
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

            foreach (FieldInfo field in a.datas.Keys)
            {
                if (result.type.IsAssignableFrom(field.DeclaringType))
                {
                    if (b.datas.ContainsKey(field)) result.datas.Add(field, a.datas[field] || b.datas[field]);
                    else result.datas.Add(field, a.datas[field] || b.DefaultValue);
                }
            }

            foreach (FieldInfo field in b.datas.Keys)
            {
                if (result.type.IsAssignableFrom(field.DeclaringType) && !result.datas.ContainsKey(field))
                {
                    if (a.datas.ContainsKey(field)) result.datas.Add(field, a.datas[field] || b.datas[field]);
                    else result.datas.Add(field, a.DefaultValue || b.datas[field]);
                }
            }
            return result;
        }
    }

    public class FieldReaderInst<T> : IDictionary<FieldInfo, object>
    {
        private Type type = typeof(T);
        private T datas = (T)Activator.CreateInstance(typeof(T));
        private readonly HashSet<FieldInfo> fields = new HashSet<FieldInfo>();

        public FieldReaderInst() { }

        public FieldReaderInst(FieldReaderInst<T> other)
        {
            if (other != null)
            {
                datas = Gen.MemberwiseClone(other.datas);
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
                    fields.RemoveWhere(x => !type.IsAssignableFrom(x.DeclaringType));
                    T org = datas;
                    datas = (T)Activator.CreateInstance(type);
                    foreach(FieldInfo field in fields)
                    {
                        field.SetValue(datas, field.GetValue(org));
                    }
                }
            }
        }


        public int Count => fields.Count;

        public ICollection<FieldInfo> Keys => new HashSet<FieldInfo>(fields);

        public ICollection<object> Values => (from x in fields select x.GetValue(datas)).ToList();

        public bool IsReadOnly => true;

        public object this[FieldInfo key]
        {
            get => (key != null && fields.Contains(key)) ? key.GetValue(datas) : null;
            set => Add(key, value);
        }


        public bool TryGetValue(string name, out object value)
        {
            value = null;
            FieldInfo fieldInfo = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (fieldInfo != null)
            {
                return TryGetValue(fieldInfo, out value);
            }
            return false;
        }


        public void SetOrAdd(string name, object value)
        {
            FieldInfo fieldInfo = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (fieldInfo != null) Add(fieldInfo, value);
        }


        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            /**
            <xx Class="c# type">
                <member_name_of_type>value</member_name_of_type>
            </xx>
            **/
            try
            {
                datas = (T)DirectXmlToObject.GetObjectFromXmlMethod(typeof(T))(xmlRoot, true);
                type = datas?.GetType() ?? typeof(T);
                foreach (XmlNode node in xmlRoot.ChildNodes)
                {
                    FieldInfo fieldInfo = type.GetField(node.Name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    if (fieldInfo != null)
                    {
                        fields.Add(fieldInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
            //Log.Message(ToString());
        }


        public override string ToString()
        {
            string result = $"{GetType()}\nHash={base.GetHashCode()}\ndata : \n";
            foreach (FieldInfo field in fields)
            {
                result += $" {field.FieldType} {field.DeclaringType}.{field.Name} : {field.GetValue(datas)}\n";
            }
            return result;
        }

        public bool ContainsKey(FieldInfo key) => fields.Contains(key);

        public void Add(FieldInfo key, object value)
        {
            if (key != null)
            {
                if (key.FieldType.IsAssignableFrom(value.GetType()) && key.DeclaringType.IsAssignableFrom(datas.GetType()))
                {
                    key.SetValue(datas, value);
                    fields.Add(key);
                }
            }
        }

        public bool Remove(FieldInfo key) => fields.Remove(key);

        public bool TryGetValue(FieldInfo key, out object value)
        {
            value = default(object);
            if (fields.Contains(key))
            {
                value = key.GetValue(datas);
                return true;
            }
            return false;
        }

        public void Add(KeyValuePair<FieldInfo, object> item) => Add(item.Key, item.Value);

        public void Clear() => fields.Clear();

        public bool Contains(KeyValuePair<FieldInfo, object> item) => fields.Contains(item.Key) && item.Key.GetValue(datas) == item.Value;

        public void CopyTo(KeyValuePair<FieldInfo, object>[] array, int arrayIndex)
        {
            int i = arrayIndex;
            foreach(FieldInfo field in fields)
            {
                array[i] = new KeyValuePair<FieldInfo, object>(field,field.GetValue(datas));
                i++;
            }
        }

        public bool Remove(KeyValuePair<FieldInfo, object> item) => Remove(item.Key);

        public IEnumerator<KeyValuePair<FieldInfo, object>> GetEnumerator()
        {
            foreach (FieldInfo field in fields)
            {
                yield return new KeyValuePair<FieldInfo, object>(field, field.GetValue(datas));
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static T operator &(T a, FieldReaderInst<T> b)
        {
            if (b != null)
            {
                a = Gen.MemberwiseClone(a);
                if (a != null)
                {
                    foreach ((FieldInfo field, object obj) in b)
                    {
                        if (field != null && field.DeclaringType.IsAssignableFrom(a.GetType()))
                        {
                            object data = field.GetValue(a);
                            if (data != null && obj != null) field.SetValue(a, obj);
                            else if (!field.FieldType.IsValueType) field.SetValue(a, null);
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
                    foreach ((FieldInfo field, object obj) in b)
                    {
                        if (field != null && field.DeclaringType.IsAssignableFrom(a.GetType()))
                        {
                            field.SetValue(a, field.GetValue(a) ?? obj);
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

            if (a.type.IsAssignableFrom(b.type)) result.UsedType = b.type;
            else if (b.type.IsAssignableFrom(a.type)) result.UsedType = a.type;

            foreach ((FieldInfo field, object obj) in a)
            {
                if (result.type.IsAssignableFrom(field.DeclaringType) && b.fields.Contains(field))
                {
                    object data = field.GetValue(b.datas);
                    if (data != null && obj != null)
                    {
                        field.SetValue(result.datas, data);
                        result.fields.Add(field);
                    }
                    else if(!field.FieldType.IsValueType)
                    {
                        field.SetValue(result.datas, null);
                        result.fields.Add(field);
                    }
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

            if (a.type.IsAssignableFrom(b.type)) result.UsedType = b.type;
            else if (b.type.IsAssignableFrom(a.type)) result.UsedType = a.type;

            foreach ((FieldInfo field, object obja) in a)
            {
                if (result.type.IsAssignableFrom(field.DeclaringType))
                {
                    field.SetValue(result.datas, obja ?? field.GetValue(b.datas));
                }
            }

            foreach ((FieldInfo field, object objb) in b)
            {
                if (result.type.IsAssignableFrom(field.DeclaringType) && !result.fields.Contains(field))
                {
                    field.SetValue(result.datas, field.GetValue(a.datas) ?? objb);
                }
            }
            return result;
        }
    }
}
