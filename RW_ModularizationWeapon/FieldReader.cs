using HarmonyLib;
using RimWorld;
using RW_NodeTree.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;
using Verse;

namespace RW_ModularizationWeapon
{

    public abstract class FieldReader<T, TV> : IDictionary<RuntimeFieldHandle, TV>
    {
        private Type type = typeof(T);

        public Type UsedType
        {
            get => type;
            set
            {
                if (value != null && typeof(T).IsAssignableFrom(value))
                {
                    type = value;
                    List<RuntimeFieldHandle> forRemove = (from x in Keys where !type.IsAssignableFrom(FieldInfo.GetFieldFromHandle(x).DeclaringType) select x).ToList();
                    foreach (RuntimeFieldHandle f in forRemove)
                    {
                        Remove(f);
                    }
                    UsedTypeUpdate();
                }
            }
        }

        public abstract TV DefaultValue { get; set; }

        public abstract bool HasDefaultValue { get; }

        public abstract TV this[RuntimeFieldHandle key] { get; set; }

        public abstract ICollection<RuntimeFieldHandle> Keys { get; }

        public abstract ICollection<TV> Values { get; }

        public abstract int Count { get; }

        public bool IsReadOnly => true;

        public abstract void Add(RuntimeFieldHandle key, TV value);

        public abstract void Clear();

        public abstract bool ContainsKey(RuntimeFieldHandle key);

        public abstract bool Remove(RuntimeFieldHandle key);

        public abstract bool TryGetValue(RuntimeFieldHandle key, out TV value);

        public abstract FieldReader<T, TV> Clone();

        public abstract void UsedTypeUpdate();

        public void Add(KeyValuePair<RuntimeFieldHandle, TV> item) => Add(item.Key, item.Value);

        public bool Contains(KeyValuePair<RuntimeFieldHandle, TV> item) => TryGetValue(item.Key, out TV value) && (object)value == (object)item.Value;

        public void CopyTo(KeyValuePair<RuntimeFieldHandle, TV>[] array, int arrayIndex)
        {
            foreach(KeyValuePair<RuntimeFieldHandle, TV> data in this)
            {
                array[arrayIndex] = data;
                arrayIndex++;
            }
        }

        public IEnumerator<KeyValuePair<RuntimeFieldHandle, TV>> GetEnumerator()
        {
            foreach(RuntimeFieldHandle key in Keys) yield return new KeyValuePair<RuntimeFieldHandle, TV>(key, this[key]);
        }

        public bool Remove(KeyValuePair<RuntimeFieldHandle, TV> item)
        {
            if(Contains(item)) return Remove(item.Key);
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public FieldReader<T, TV> ClacValue(Func<TV, TV, FieldInfo, TV> calc, TV value)
        {
            FieldReader<T, TV> result = this.Clone();
            if(calc != null && value != null)
            {
                List<RuntimeFieldHandle> fieldInfos = new List<RuntimeFieldHandle>(result.Keys);
                foreach (RuntimeFieldHandle field in fieldInfos)
                {
                    result[field] = calc(result[field], value, FieldInfo.GetFieldFromHandle(field));
                }
            }
            return result;
        }

        public T ClacValue(Func<TV, TV, FieldInfo, TV> calc, T orginal)
        {
            if (orginal != null)
            {
                T result = Gen.MemberwiseClone(orginal);
                if (calc != null)
                {
                    foreach (FieldInfo field in result.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                    {
                        if (UsedType.IsAssignableFrom(field.DeclaringType) && typeof(TV).IsAssignableFrom(field.FieldType))
                        {
                            if (!TryGetValue(field.FieldHandle, out TV value)) value = DefaultValue;
                            field.SetValue(result, calc((TV)field.GetValue(result), value, field));
                        }
                    }
                }
                return result;
            }
            return orginal;
        }

        public static TFR ClacValue<TFR>(Func<TV, TV, FieldInfo, TV> calc, TFR a, TFR b) where TFR : FieldReader<T, TV>, new()
        {

            TFR result = new TFR();

            if (a == null && b == null) return null;

            a = a ?? new TFR();
            b = b ?? new TFR();

            if (a.UsedType.IsAssignableFrom(b.UsedType)) result.UsedType = b.UsedType;
            else if (b.UsedType.IsAssignableFrom(a.UsedType)) result.UsedType = a.UsedType;

            foreach (RuntimeFieldHandle field in a.Keys)
            {
                if (result.UsedType.IsAssignableFrom(FieldInfo.GetFieldFromHandle(field).DeclaringType))
                {
                    if (b.ContainsKey(field)) result.Add(field, calc(a[field], b[field], FieldInfo.GetFieldFromHandle(field)));
                    else result.Add(field, calc(a[field], b.DefaultValue, FieldInfo.GetFieldFromHandle(field)));
                }
            }

            foreach (RuntimeFieldHandle field in b.Keys)
            {
                if (result.UsedType.IsAssignableFrom(FieldInfo.GetFieldFromHandle(field).DeclaringType) && !result.ContainsKey(field))
                {
                    if (a.ContainsKey(field)) result.Add(field, calc(a[field], b[field], FieldInfo.GetFieldFromHandle(field)));
                    else result.Add(field, calc(a.DefaultValue, b[field], FieldInfo.GetFieldFromHandle(field)));
                }
            }
            return result;
        }


        public virtual void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            string typename = xmlRoot.Attributes["Reader-Class"]?.Value;
            try
            {
                if (typename != null)
                {
                    UsedType = GenTypes.GetTypeInAnyAssembly(typename);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }
    }
    
    public class FieldReaderDgit<T> : FieldReader<T, IConvertible>
    {
        private double? defaultValue;
        private readonly Dictionary<RuntimeFieldHandle, double> datas = new Dictionary<RuntimeFieldHandle, double>();

        public FieldReaderDgit() { }

        public FieldReaderDgit(FieldReaderDgit<T> other)
        {
            if(other != null)
            {
                datas.AddRange(other.datas);
                UsedType = other.UsedType;
                defaultValue = other.defaultValue;
            }
        }


        public override IConvertible DefaultValue
        {
            get => defaultValue.GetValueOrDefault();
            set => defaultValue = value.ToDouble(null);
        }

        public override bool HasDefaultValue => defaultValue.HasValue;


        public override int Count => datas.Count;

        public override ICollection<RuntimeFieldHandle> Keys => datas.Keys;

        public override ICollection<IConvertible> Values => (from x in datas.Values select (IConvertible)x).ToArray();

        public override IConvertible this[RuntimeFieldHandle key] 
        {
            get => datas.TryGetValue(key);
            set => Add(key, value);
        }

        public override void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            base.LoadDataFromXmlCustom(xmlRoot);
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
            <xx Reader-Class="c# type" Default="default_value">
                <member_name_of_type>value</member_name_of_type>
            </xx>
            **/
            foreach (XmlNode node in xmlRoot.ChildNodes)
            {
                try
                {
                    FieldInfo field = UsedType.GetField(node.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    this[field.FieldHandle] = ParseHelper.FromString<double>(node.InnerText);
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
            foreach (KeyValuePair<RuntimeFieldHandle, double> data in datas)
            {
                FieldInfo field = FieldInfo.GetFieldFromHandle(data.Key);
                result += $" {field.FieldType} {field.DeclaringType}.{field.Name} : {data.Value}\n";
            }
            return result;
        }

        public override bool ContainsKey(RuntimeFieldHandle key) => datas.ContainsKey(key);

        public override void Add(RuntimeFieldHandle key, IConvertible value)
        {
            FieldInfo field = FieldInfo.GetFieldFromHandle(key);
            if (field != null && field.DeclaringType.IsAssignableFrom(UsedType))
            {
                Type vt = field.FieldType;
                if (vt == typeof(int) || vt == typeof(float) ||
                   vt == typeof(long) || vt == typeof(sbyte) ||
                   vt == typeof(double))
                    datas.SetOrAdd(key, value.ToDouble(null));
                else throw new ArgumentException($"not support value(name={field.Name},type={vt})");
            }
        }

        public override bool Remove(RuntimeFieldHandle key) => datas.Remove(key);

        public override bool TryGetValue(RuntimeFieldHandle key, out IConvertible value)
        {
            bool result = datas.TryGetValue(key, out double outer);
            value = outer;
            return result;
        }

        public override void Clear() => datas.Clear();

        public override FieldReader<T, IConvertible> Clone() => new FieldReaderDgit<T>(this);

        public override void UsedTypeUpdate() { }

        public static FieldReaderDgit<T> operator +(FieldReaderDgit<T> a, double b)
            => (a?.ClacValue((av, bv, field) => av.ToDouble(null) + bv.ToDouble(null), b) as FieldReaderDgit<T>) ?? a;

        public static FieldReaderDgit<T> operator -(FieldReaderDgit<T> a, double b)
            => (a?.ClacValue((av, bv, field) => av.ToDouble(null) - bv.ToDouble(null), b) as FieldReaderDgit<T>) ?? a;

        public static FieldReaderDgit<T> operator *(FieldReaderDgit<T> a, double b)
            => (a?.ClacValue((av, bv, field) => av.ToDouble(null) * bv.ToDouble(null), b) as FieldReaderDgit<T>) ?? a;

        public static FieldReaderDgit<T> operator /(FieldReaderDgit<T> a, double b)
            => (a?.ClacValue((av, bv, field) => av.ToDouble(null) / bv.ToDouble(null), b) as FieldReaderDgit<T>) ?? a;

        public static FieldReaderDgit<T> operator %(FieldReaderDgit<T> a, double b)
            => (a?.ClacValue((av, bv, field) => av.ToDouble(null) % bv.ToDouble(null), b) as FieldReaderDgit<T>) ?? a;


        public static T operator +(T a, FieldReaderDgit<T> b)
        {
            if (b != null)
            {
                a = b.ClacValue((va, vb, field) =>
                {
                    if (va != null)
                    {
                        if (va.GetType() == typeof(int)) return (int)(va.ToDouble(null) + vb.ToDouble(null));
                        else if (va.GetType() == typeof(float)) return (float)(va.ToDouble(null) + vb.ToDouble(null));
                        else if (va.GetType() == typeof(long)) return (long)(va.ToDouble(null) + vb.ToDouble(null));
                        else if (va.GetType() == typeof(sbyte)) return (sbyte)(va.ToDouble(null) + vb.ToDouble(null));
                        else if (va.GetType() == typeof(double)) return va.ToDouble(null) + vb.ToDouble(null);
                    }
                    return va;
                }, a);
            }
            return a;
        }

        public static T operator -(T a, FieldReaderDgit<T> b)
        {
            if (b != null)
            {
                a = b.ClacValue((va, vb, field) =>
                {
                    if (va != null)
                    {
                        if (va.GetType() == typeof(int)) return (int)(va.ToDouble(null) - vb.ToDouble(null));
                        else if (va.GetType() == typeof(float)) return (float)(va.ToDouble(null) - vb.ToDouble(null));
                        else if (va.GetType() == typeof(long)) return (long)(va.ToDouble(null) - vb.ToDouble(null));
                        else if (va.GetType() == typeof(sbyte)) return (sbyte)(va.ToDouble(null) - vb.ToDouble(null));
                        else if (va.GetType() == typeof(double)) return va.ToDouble(null) - vb.ToDouble(null);
                    }
                    return va;
                }, a);
            }
            return a;
        }

        public static T operator *(T a, FieldReaderDgit<T> b)
        {
            if (b != null)
            {
                a = b.ClacValue((va, vb, field) =>
                {
                    if (va != null)
                    {
                        if (va.GetType() == typeof(int)) return (int)(va.ToDouble(null) * vb.ToDouble(null));
                        else if (va.GetType() == typeof(float)) return (float)(va.ToDouble(null) * vb.ToDouble(null));
                        else if (va.GetType() == typeof(long)) return (long)(va.ToDouble(null) * vb.ToDouble(null));
                        else if (va.GetType() == typeof(sbyte)) return (sbyte)(va.ToDouble(null) * vb.ToDouble(null));
                        else if (va.GetType() == typeof(double)) return va.ToDouble(null) * vb.ToDouble(null);
                    }
                    return va;
                }, a);
            }
            return a;
        }

        public static T operator /(T a, FieldReaderDgit<T> b)
        {
            if (b != null)
            {
                a = b.ClacValue((va, vb, field) =>
                {
                    if (va != null)
                    {
                        if (va.GetType() == typeof(int)) return (int)(va.ToDouble(null) / vb.ToDouble(null));
                        else if (va.GetType() == typeof(float)) return (float)(va.ToDouble(null) / vb.ToDouble(null));
                        else if (va.GetType() == typeof(long)) return (long)(va.ToDouble(null) / vb.ToDouble(null));
                        else if (va.GetType() == typeof(sbyte)) return (sbyte)(va.ToDouble(null) / vb.ToDouble(null));
                        else if (va.GetType() == typeof(double)) return va.ToDouble(null) / vb.ToDouble(null);
                    }
                    return va;
                }, a);
            }
            return a;
        }

        public static T operator %(T a, FieldReaderDgit<T> b)
        {
            if (b != null)
            {
                a = b.ClacValue((va, vb, field) =>
                {
                    if (va != null)
                    {
                        if (va.GetType() == typeof(int)) return (int)(va.ToDouble(null) % vb.ToDouble(null));
                        else if (va.GetType() == typeof(float)) return (float)(va.ToDouble(null) % vb.ToDouble(null));
                        else if (va.GetType() == typeof(long)) return (long)(va.ToDouble(null) % vb.ToDouble(null));
                        else if (va.GetType() == typeof(sbyte)) return (sbyte)(va.ToDouble(null) % vb.ToDouble(null));
                        else if (va.GetType() == typeof(double)) return va.ToDouble(null) % vb.ToDouble(null);
                    }
                    return va;
                }, a);
            }
            return a;
        }

        public static FieldReaderDgit<T> operator +(FieldReaderDgit<T> a, FieldReaderDgit<T> b)
            => ClacValue((va, vb, field) => va.ToDouble(null) + vb.ToDouble(null), a, b);

        public static FieldReaderDgit<T> operator -(FieldReaderDgit<T> a, FieldReaderDgit<T> b)
            => ClacValue((va, vb, field) => va.ToDouble(null) - vb.ToDouble(null), a, b);

        public static FieldReaderDgit<T> operator *(FieldReaderDgit<T> a, FieldReaderDgit<T> b)
            => ClacValue((va, vb, field) => va.ToDouble(null) * vb.ToDouble(null), a, b);

        public static FieldReaderDgit<T> operator /(FieldReaderDgit<T> a, FieldReaderDgit<T> b)
            => ClacValue((va, vb, field) => va.ToDouble(null) / vb.ToDouble(null), a, b);

        public static FieldReaderDgit<T> operator %(FieldReaderDgit<T> a, FieldReaderDgit<T> b)
            => ClacValue((va, vb, field) => va.ToDouble(null) % vb.ToDouble(null), a, b);
    }

    public class FieldReaderBool<T> : FieldReader<T, bool>
    {
        private bool? defaultValue;
        private readonly Dictionary<RuntimeFieldHandle, bool> datas = new Dictionary<RuntimeFieldHandle, bool>();

        public FieldReaderBool() { }

        public FieldReaderBool(FieldReaderBool<T> other)
        {
            if (other != null)
            {
                datas.AddRange(other.datas);
                UsedType = other.UsedType;
                defaultValue = other.defaultValue;
            }
        }

        public override bool DefaultValue
        {
            get => defaultValue.GetValueOrDefault();
            set => defaultValue = value;
        }

        public override bool HasDefaultValue => defaultValue.HasValue;

        public override int Count => datas.Count;

        public override ICollection<RuntimeFieldHandle> Keys => datas.Keys;

        public override ICollection<bool> Values => datas.Values;

        public override bool this[RuntimeFieldHandle key]
        {
            get => datas[key];
            set => Add(key, value);
        }


        public override void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            base.LoadDataFromXmlCustom(xmlRoot);
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
            <xx Reader-Class="c# type" Default="default_value">
                <member_name_of_type>value</member_name_of_type>
            </xx>
            **/
            foreach (XmlNode node in xmlRoot.ChildNodes)
            {
                try
                {
                    FieldInfo field = UsedType.GetField(node.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    this[field.FieldHandle] = ParseHelper.FromString<bool>(node.InnerText);
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
            foreach (KeyValuePair<RuntimeFieldHandle, bool> data in datas)
            {
                FieldInfo field = FieldInfo.GetFieldFromHandle(data.Key);
                result += $" {field.FieldType} {field.DeclaringType}.{field.Name} : {data.Value}\n";
            }
            return result;
        }

        public override bool ContainsKey(RuntimeFieldHandle key) => datas.ContainsKey(key);

        public override void Add(RuntimeFieldHandle key, bool value)
        {
            FieldInfo field = FieldInfo.GetFieldFromHandle(key);
            if (field != null && field.DeclaringType.IsAssignableFrom(UsedType))
            {
                Type vt = field.FieldType;
                if (vt == typeof(bool))
                    datas.SetOrAdd(key, value);
                else throw new ArgumentException($"not support value(name={field.Name},type={vt})");
            }
        }

        public override bool Remove(RuntimeFieldHandle key) => datas.Remove(key);

        public override bool TryGetValue(RuntimeFieldHandle key, out bool value) => datas.TryGetValue(key, out value);

        public override void Clear() => datas.Clear();

        public override FieldReader<T, bool> Clone() => new FieldReaderBool<T>(this);

        public override void UsedTypeUpdate() { }

        public static FieldReaderBool<T> operator &(FieldReaderBool<T> a, bool b)
            => (a?.ClacValue((av, bv, field) => av && bv, b) as FieldReaderBool<T>) ?? a;

        public static FieldReaderBool<T> operator |(FieldReaderBool<T> a, bool b)
            => (a?.ClacValue((av, bv, field) => av || bv, b) as FieldReaderBool<T>) ?? a;

        public static FieldReaderBool<T> operator !(FieldReaderBool<T> a)
            => (a?.ClacValue((av, bv, field) => !av, false) as FieldReaderBool<T>) ?? a;


        public static T operator &(T a, FieldReaderBool<T> b)
        {
            if (b != null)
            {
                return b.ClacValue((va, vb, field) => va && vb, a);
            }
            return a;
        }

        public static T operator |(T a, FieldReaderBool<T> b)
        {
            if (b != null)
            {
                return b.ClacValue((va, vb, field) => va || vb, a);
            }
            return a;
        }

        public static FieldReaderBool<T> operator &(FieldReaderBool<T> a, FieldReaderBool<T> b)
            => ClacValue((va, vb, field) => va && vb, a, b);

        public static FieldReaderBool<T> operator |(FieldReaderBool<T> a, FieldReaderBool<T> b)
            => ClacValue((va, vb, field) => va || vb, a, b);
    }

    public class FieldReaderInst<T> : FieldReader<T, object>
    {
        private bool loading = false;
        private T datas = (T)Activator.CreateInstance(typeof(T));
        private readonly HashSet<RuntimeFieldHandle> fields = new HashSet<RuntimeFieldHandle>();

        public FieldReaderInst() { }

        public FieldReaderInst(FieldReaderInst<T> other)
        {
            if (other != null)
            {
                datas = Gen.MemberwiseClone(other.datas);
                fields.AddRange(other.fields);
                UsedType = other.UsedType;
            }
        }

        public override int Count => fields.Count;

        public override ICollection<RuntimeFieldHandle> Keys => new HashSet<RuntimeFieldHandle>(fields);

        public override ICollection<object> Values => (from x in fields select FieldInfo.GetFieldFromHandle(x).GetValue(datas)).ToList();

        public override object DefaultValue { get => null; set => throw new NotImplementedException(); }

        public override bool HasDefaultValue => false;

        public override object this[RuntimeFieldHandle key]
        {
            get => (key != null && fields.Contains(key)) ? FieldInfo.GetFieldFromHandle(key).GetValue(datas) : null;
            set => Add(key, value);
        }


        public override void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            loading = true;
            base.LoadDataFromXmlCustom(xmlRoot);
            /**
            <xx Reader-Class="c# type">
                <member_name_of_type>value</member_name_of_type>
            </xx>
            **/
            try
            {
                datas = (T)DirectXmlToObject.GetObjectFromXmlMethod(UsedType)(xmlRoot, true);
                UsedType = datas?.GetType() ?? typeof(T);
                foreach (XmlNode node in xmlRoot.ChildNodes)
                {
                    FieldInfo fieldInfo = UsedType.GetField(node.Name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    if (fieldInfo != null)
                    {
                        fields.Add(fieldInfo.FieldHandle);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
            loading = false;
            //Log.Message(ToString());
        }


        public override string ToString()
        {
            string result = $"{GetType()}\nHash={base.GetHashCode()}\ndata : \n";
            foreach (RuntimeFieldHandle key in fields)
            {
                FieldInfo field = FieldInfo.GetFieldFromHandle(key);
                result += $" {field.FieldType} {field.DeclaringType}.{field.Name} : {field.GetValue(datas)}\n";
            }
            return result;
        }

        public override bool ContainsKey(RuntimeFieldHandle key) => fields.Contains(key);

        public override void Add(RuntimeFieldHandle key, object value)
        {
            FieldInfo field = FieldInfo.GetFieldFromHandle(key);
            if (field != null && field.DeclaringType.IsAssignableFrom(UsedType))
            {
                if (value == null || (field.FieldType.IsAssignableFrom(value.GetType()) && field.DeclaringType.IsAssignableFrom(datas.GetType())))
                {
                    field.SetValue(datas, value);
                    fields.Add(key);
                }
            }
        }

        public override bool Remove(RuntimeFieldHandle key) => fields.Remove(key);

        public override bool TryGetValue(RuntimeFieldHandle key, out object value)
        {
            value = default(object);
            FieldInfo field = FieldInfo.GetFieldFromHandle(key);
            if (field != null && fields.Contains(key))
            {
                value = field.GetValue(datas);
                return true;
            }
            return false;
        }

        public override void Clear() => fields.Clear();

        public override FieldReader<T, object> Clone() => new FieldReaderInst<T>(this);

        public override void UsedTypeUpdate()
        {
            if(!loading)
            {
                T old = datas;
                datas = (T)Activator.CreateInstance(UsedType);
                foreach (RuntimeFieldHandle key in fields)
                {
                    FieldInfo field = FieldInfo.GetFieldFromHandle(key);
                    field.SetValue(datas, field.GetValue(old));
                }
            }
        }

        public static T operator &(T a, FieldReaderInst<T> b)
        {
            if (b != null)
            {
                return b.ClacValue((va, vb, field) => (va != null && b.ContainsKey(field.FieldHandle)) ? vb : va, a);
            }
            return a;
        }

        public static T operator |(T a, FieldReaderInst<T> b)
        {
            if (b != null)
            {
                return b.ClacValue((va, vb, field) => va ?? vb, a);
            }
            return a;
        }

        public static FieldReaderInst<T> operator &(FieldReaderInst<T> a, FieldReaderInst<T> b)
            => ClacValue((va, vb, field) => (va != null && b.ContainsKey(field.FieldHandle)) ? vb : va, a, b);

        public static FieldReaderInst<T> operator |(FieldReaderInst<T> a, FieldReaderInst<T> b)
            => ClacValue((va, vb, field) => va ?? vb, a, b);
    }

    public class FieldReaderFilt<T> : FieldReader<T, bool>
    {
        private bool? defaultValue;
        private readonly Dictionary<RuntimeFieldHandle, bool> datas = new Dictionary<RuntimeFieldHandle, bool>();

        public FieldReaderFilt() { }

        public FieldReaderFilt(FieldReaderFilt<T> other)
        {
            if (other != null)
            {
                datas.AddRange(other.datas);
                UsedType = other.UsedType;
                defaultValue = other.defaultValue;
            }
        }

        public override bool DefaultValue
        {
            get => defaultValue.GetValueOrDefault();
            set => defaultValue = value;
        }

        public override bool HasDefaultValue => defaultValue.HasValue;

        public override int Count => datas.Count;

        public override ICollection<RuntimeFieldHandle> Keys => datas.Keys;

        public override ICollection<bool> Values => datas.Values;

        public override bool this[RuntimeFieldHandle key]
        {
            get => datas[key];
            set => Add(key, value);
        }


        public override void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            base.LoadDataFromXmlCustom(xmlRoot);
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
            <xx Reader-Class="c# type" Default="default_value">
                <member_name_of_type>value</member_name_of_type>
            </xx>
            **/
            foreach (XmlNode node in xmlRoot.ChildNodes)
            {
                try
                {
                    FieldInfo field = UsedType.GetField(node.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    this[field.FieldHandle] = ParseHelper.FromString<bool>(node.InnerText);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
            //Log.Message(ToString());
        }


        public override string ToString()
        {
            string result = $"{GetType()}\nHash={base.GetHashCode()}\ndefaultValue={defaultValue}\ndata : \n";
            foreach (KeyValuePair<RuntimeFieldHandle, bool> data in datas)
            {
                FieldInfo field = FieldInfo.GetFieldFromHandle(data.Key);
                result += $" {field.FieldType} {field.DeclaringType}.{field.Name} : {data.Value}\n";
            }
            return result;
        }

        public override bool ContainsKey(RuntimeFieldHandle key) => datas.ContainsKey(key);

        public override void Add(RuntimeFieldHandle key, bool value)
        {
            FieldInfo field = FieldInfo.GetFieldFromHandle(key);
            if (field != null && field.DeclaringType.IsAssignableFrom(UsedType))
            {
                datas.SetOrAdd(key, value);
            }
        }

        public override bool Remove(RuntimeFieldHandle key) => datas.Remove(key);

        public override bool TryGetValue(RuntimeFieldHandle key, out bool value) => datas.TryGetValue(key, out value);

        public override void Clear() => datas.Clear();

        public override FieldReader<T, bool> Clone() => new FieldReaderFilt<T>(this);

        public override void UsedTypeUpdate() { }

        public static FieldReaderFilt<T> operator &(FieldReaderFilt<T> a, bool b)
            => (a?.ClacValue((av, bv, field) => av && bv, b) as FieldReaderFilt<T>) ?? a;

        public static FieldReaderFilt<T> operator |(FieldReaderFilt<T> a, bool b)
            => (a?.ClacValue((av, bv, field) => av || bv, b) as FieldReaderFilt<T>) ?? a;

        public static FieldReaderFilt<T> operator !(FieldReaderFilt<T> a)
            => (a?.ClacValue((av, bv, field) => !av, false) as FieldReaderFilt<T>) ?? a;

        public static FieldReaderFilt<T> operator &(FieldReaderFilt<T> a, FieldReaderFilt<T> b)
            => ClacValue((va, vb, field) => va && vb, a, b);

        public static FieldReaderFilt<T> operator |(FieldReaderFilt<T> a, FieldReaderFilt<T> b)
            => ClacValue((va, vb, field) => va || vb, a, b);

        public static FieldReaderDgit<T> operator &(FieldReaderDgit<T> a, FieldReaderFilt<T> b)
        {
            if(a != null && b != null)
            {
                FieldReaderDgit<T> org = a;
                a = (FieldReaderDgit<T>)a.Clone();
                foreach(RuntimeFieldHandle fieldHandle in org.Keys)
                {
                    if (b.ContainsKey(fieldHandle))
                    {
                        if (!b[fieldHandle] && a.ContainsKey(fieldHandle)) a.Remove(fieldHandle);
                    }
                    else if(!b.DefaultValue && a.ContainsKey(fieldHandle)) a.Remove(fieldHandle);
                }
            }
            return a;
        }

        public static FieldReaderBool<T> operator &(FieldReaderBool<T> a, FieldReaderFilt<T> b)
        {
            if (a != null && b != null)
            {
                FieldReaderBool<T> org = a;
                a = (FieldReaderBool<T>)a.Clone();
                foreach (RuntimeFieldHandle fieldHandle in org.Keys)
                {
                    if (b.ContainsKey(fieldHandle))
                    {
                        if (!b[fieldHandle] && a.ContainsKey(fieldHandle)) a.Remove(fieldHandle);
                    }
                    else if (!b.DefaultValue && a.ContainsKey(fieldHandle)) a.Remove(fieldHandle);
                }
            }
            return a;
        }

        public static FieldReaderInst<T> operator &(FieldReaderInst<T> a, FieldReaderFilt<T> b)
        {
            if (a != null && b != null)
            {
                FieldReaderInst<T> org = a;
                a = (FieldReaderInst<T>)a.Clone();
                foreach (RuntimeFieldHandle fieldHandle in org.Keys)
                {
                    if (b.ContainsKey(fieldHandle))
                    {
                        if (!b[fieldHandle] && a.ContainsKey(fieldHandle)) a.Remove(fieldHandle);
                    }
                    else if (!b.DefaultValue && a.ContainsKey(fieldHandle)) a.Remove(fieldHandle);
                }
            }
            return a;
        }
    }
}
