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

    public abstract class FieldReader<T, TV> : IDictionary<FieldInfo, TV>
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
                    List<FieldInfo> forRemove = (from x in Keys where !type.IsAssignableFrom(x.DeclaringType) select x).ToList();
                    foreach (FieldInfo f in forRemove)
                    {
                        Remove(f);
                    }
                    UsedTypeUpdate();
                }
            }
        }

        public abstract TV DefaultValue { get; set; }

        public abstract bool HasDefaultValue { get; }

        public abstract TV this[FieldInfo key] { get; set; }

        public abstract ICollection<FieldInfo> Keys { get; }

        public abstract ICollection<TV> Values { get; }

        public abstract int Count { get; }

        public bool IsReadOnly => true;

        public abstract void Add(FieldInfo key, TV value);

        public abstract void Clear();

        public abstract bool ContainsKey(FieldInfo key);

        public abstract bool Remove(FieldInfo key);

        public abstract bool TryGetValue(FieldInfo key, out TV value);

        public abstract FieldReader<T, TV> Clone();

        public abstract void UsedTypeUpdate();

        public void Add(KeyValuePair<FieldInfo, TV> item) => Add(item.Key, item.Value);

        public bool Contains(KeyValuePair<FieldInfo, TV> item) => TryGetValue(item.Key, out TV value) && (object)value == (object)item.Value;

        public void CopyTo(KeyValuePair<FieldInfo, TV>[] array, int arrayIndex)
        {
            foreach((FieldInfo field, TV value) in this)
            {
                array[arrayIndex] = new KeyValuePair<FieldInfo, TV>(field, value);
                arrayIndex++;
            }
        }

        public IEnumerator<KeyValuePair<FieldInfo, TV>> GetEnumerator()
        {
            foreach(FieldInfo key in Keys) yield return new KeyValuePair<FieldInfo, TV>(key, this[key]);
        }

        public bool Remove(KeyValuePair<FieldInfo, TV> item)
        {
            if(Contains(item)) return Remove(item.Key);
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public FieldReader<T, TV> ClacValue(Func<TV, TV, TV> calc, TV value)
        {
            FieldReader<T, TV> result = this.Clone();
            if(calc != null && value != null)
            {
                List<FieldInfo> fieldInfos = new List<FieldInfo>(result.Keys);
                foreach (FieldInfo field in fieldInfos)
                {
                    result[field] = calc(result[field], value);
                }
            }
            return result;
        }

        public T ClacValue(Func<TV, TV, TV> calc, T orginal)
        {
            if (orginal != null)
            {
                T result = Gen.MemberwiseClone(orginal);
                if (calc != null)
                {
                    foreach (FieldInfo field in result.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                    {
                        if (field.DeclaringType.IsAssignableFrom(UsedType) && typeof(TV).IsAssignableFrom(field.FieldType))
                        {
                            if (!TryGetValue(field, out TV value)) value = DefaultValue;
                            field.SetValue(result, calc((TV)field.GetValue(result), value));
                        }
                    }
                }
                return result;
            }
            return orginal;
        }

        public static TFR ClacValue<TFR>(Func<TV, TV, TV> calc, TFR a, TFR b) where TFR : FieldReader<T, TV>, new()
        {

            TFR result = new TFR();

            if (a == null && b == null) return null;

            a = a ?? new TFR();
            b = b ?? new TFR();

            if (a.UsedType.IsAssignableFrom(b.UsedType)) result.UsedType = b.UsedType;
            else if (b.UsedType.IsAssignableFrom(a.UsedType)) result.UsedType = a.UsedType;

            foreach (FieldInfo field in a.Keys)
            {
                if (result.UsedType.IsAssignableFrom(field.DeclaringType))
                {
                    if (b.ContainsKey(field)) result.Add(field, calc(a[field], b[field]));
                    else result.Add(field, calc(a[field], b.DefaultValue));
                }
            }

            foreach (FieldInfo field in b.Keys)
            {
                if (result.UsedType.IsAssignableFrom(field.DeclaringType) && !result.ContainsKey(field))
                {
                    if (a.ContainsKey(field)) result.Add(field, calc(a[field], b[field]));
                    else result.Add(field, calc(a.DefaultValue, b[field]));
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
        private readonly Dictionary<FieldInfo, double> datas = new Dictionary<FieldInfo, double>();

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

        public override ICollection<FieldInfo> Keys => datas.Keys;

        public override ICollection<IConvertible> Values => (from x in datas.Values select (IConvertible)x).ToArray();

        public override IConvertible this[FieldInfo key] 
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
                    this[field] = ParseHelper.FromString<double>(node.InnerText);
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

        public override bool ContainsKey(FieldInfo key) => datas.ContainsKey(key);

        public override void Add(FieldInfo key, IConvertible value)
        {
            if (key != null && key.DeclaringType.IsAssignableFrom(UsedType))
            {
                Type vt = key.FieldType;
                if (vt == typeof(int) || vt == typeof(float) ||
                   vt == typeof(long) || vt == typeof(sbyte) ||
                   vt == typeof(double))
                    datas.SetOrAdd(key, value.ToDouble(null));
                else throw new ArgumentException($"not support value(name={key.Name},type={vt})");
            }
        }

        public override bool Remove(FieldInfo key) => datas.Remove(key);

        public override bool TryGetValue(FieldInfo key, out IConvertible value)
        {
            value = default(double);
            bool result = datas.TryGetValue(key, out double outer);
            value = outer;
            return result;
        }

        public override void Clear() => datas.Clear();

        public override FieldReader<T, IConvertible> Clone() => new FieldReaderDgit<T>(this);

        public override void UsedTypeUpdate() { }

        public static FieldReaderDgit<T> operator +(FieldReaderDgit<T> a, double b)
            => (a?.ClacValue((av, bv) => av.ToDouble(null) + bv.ToDouble(null), b) as FieldReaderDgit<T>) ?? a;

        public static FieldReaderDgit<T> operator -(FieldReaderDgit<T> a, double b)
            => (a?.ClacValue((av, bv) => av.ToDouble(null) - bv.ToDouble(null), b) as FieldReaderDgit<T>) ?? a;

        public static FieldReaderDgit<T> operator *(FieldReaderDgit<T> a, double b)
            => (a?.ClacValue((av, bv) => av.ToDouble(null) * bv.ToDouble(null), b) as FieldReaderDgit<T>) ?? a;

        public static FieldReaderDgit<T> operator /(FieldReaderDgit<T> a, double b)
            => (a?.ClacValue((av, bv) => av.ToDouble(null) / bv.ToDouble(null), b) as FieldReaderDgit<T>) ?? a;

        public static FieldReaderDgit<T> operator %(FieldReaderDgit<T> a, double b)
            => (a?.ClacValue((av, bv) => av.ToDouble(null) % bv.ToDouble(null), b) as FieldReaderDgit<T>) ?? a;


        public static T operator +(T a, FieldReaderDgit<T> b)
        {
            if (b != null)
            {
                a = b.ClacValue((va, vb) =>
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
                a = b.ClacValue((va, vb) =>
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
                a = b.ClacValue((va, vb) =>
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
                a = b.ClacValue((va, vb) =>
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
                a = b.ClacValue((va, vb) =>
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
            => ClacValue((va, vb) => va.ToDouble(null) + vb.ToDouble(null), a, b);

        public static FieldReaderDgit<T> operator -(FieldReaderDgit<T> a, FieldReaderDgit<T> b)
            => ClacValue((va, vb) => va.ToDouble(null) - vb.ToDouble(null), a, b);

        public static FieldReaderDgit<T> operator *(FieldReaderDgit<T> a, FieldReaderDgit<T> b)
            => ClacValue((va, vb) => va.ToDouble(null) * vb.ToDouble(null), a, b);

        public static FieldReaderDgit<T> operator /(FieldReaderDgit<T> a, FieldReaderDgit<T> b)
            => ClacValue((va, vb) => va.ToDouble(null) / vb.ToDouble(null), a, b);

        public static FieldReaderDgit<T> operator %(FieldReaderDgit<T> a, FieldReaderDgit<T> b)
            => ClacValue((va, vb) => va.ToDouble(null) % vb.ToDouble(null), a, b);
    }

    public class FieldReaderBool<T> : FieldReader<T, bool>
    {
        private bool? defaultValue;
        private readonly Dictionary<FieldInfo, bool> datas = new Dictionary<FieldInfo, bool>();

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

        public override ICollection<FieldInfo> Keys => datas.Keys;

        public override ICollection<bool> Values => datas.Values;

        public override bool this[FieldInfo key]
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
                    this[field] = ParseHelper.FromString<bool>(node.InnerText);
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

        public override bool ContainsKey(FieldInfo key) => datas.ContainsKey(key);

        public override void Add(FieldInfo key, bool value)
        {
            if (key != null)
            {
                Type vt = key.FieldType;
                if (vt == typeof(bool))
                    datas.SetOrAdd(key, value);
                else throw new ArgumentException($"not support value(name={key.Name},type={vt})");
            }
        }

        public override bool Remove(FieldInfo key) => datas.Remove(key);

        public override bool TryGetValue(FieldInfo key, out bool value) => datas.TryGetValue(key, out value);

        public override void Clear() => datas.Clear();

        public override FieldReader<T, bool> Clone() => new FieldReaderBool<T>(this);

        public override void UsedTypeUpdate() { }

        public static FieldReaderBool<T> operator &(FieldReaderBool<T> a, bool b)
            => (a?.ClacValue((av, bv) => av && bv, b) as FieldReaderBool<T>) ?? a;

        public static FieldReaderBool<T> operator |(FieldReaderBool<T> a, bool b)
            => (a?.ClacValue((av, bv) => av || bv, b) as FieldReaderBool<T>) ?? a;

        public static FieldReaderBool<T> operator !(FieldReaderBool<T> a)
            => (a?.ClacValue((av, bv) => !av, false) as FieldReaderBool<T>) ?? a;


        public static T operator &(T a, FieldReaderBool<T> b)
        {
            if (b != null)
            {
                return b.ClacValue((va, vb) => va && vb, a);
            }
            return a;
        }

        public static T operator |(T a, FieldReaderBool<T> b)
        {
            if (b != null)
            {
                return b.ClacValue((va, vb) => va || vb, a);
            }
            return a;
        }

        public static FieldReaderBool<T> operator &(FieldReaderBool<T> a, FieldReaderBool<T> b)
            => ClacValue((va, vb) => va && vb, a, b);

        public static FieldReaderBool<T> operator |(FieldReaderBool<T> a, FieldReaderBool<T> b)
            => ClacValue((va, vb) => va || vb, a, b);
    }

    public class FieldReaderInst<T> : FieldReader<T, object>
    {
        private bool loading = false;
        private T datas = (T)Activator.CreateInstance(typeof(T));
        private readonly HashSet<FieldInfo> fields = new HashSet<FieldInfo>();

        public FieldReaderInst() { }

        public FieldReaderInst(FieldReaderInst<T> other)
        {
            if (other != null)
            {
                datas = Gen.MemberwiseClone(other.datas);
                UsedType = other.UsedType;
            }
        }

        public override int Count => fields.Count;

        public override ICollection<FieldInfo> Keys => new HashSet<FieldInfo>(fields);

        public override ICollection<object> Values => (from x in fields select x.GetValue(datas)).ToList();

        public override object DefaultValue { get => null; set => throw new NotImplementedException(); }

        public override bool HasDefaultValue => false;

        public override object this[FieldInfo key]
        {
            get => (key != null && fields.Contains(key)) ? key.GetValue(datas) : null;
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
                        fields.Add(fieldInfo);
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
            foreach (FieldInfo field in fields)
            {
                result += $" {field.FieldType} {field.DeclaringType}.{field.Name} : {field.GetValue(datas)}\n";
            }
            return result;
        }

        public override bool ContainsKey(FieldInfo key) => fields.Contains(key);

        public override void Add(FieldInfo key, object value)
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

        public override bool Remove(FieldInfo key) => fields.Remove(key);

        public override bool TryGetValue(FieldInfo key, out object value)
        {
            value = default(object);
            if (fields.Contains(key))
            {
                value = key.GetValue(datas);
                return true;
            }
            return false;
        }

        public override void Clear() => fields.Clear();

        public override FieldReader<T, object> Clone() => new FieldReaderInst<T>();

        public override void UsedTypeUpdate()
        {
            if(!loading)
            {
                T old = datas;
                datas = (T)Activator.CreateInstance(UsedType);
                foreach (FieldInfo field in fields) field.SetValue(datas, field.GetValue(old));
            }
        }

        public static T operator &(T a, FieldReaderInst<T> b)
        {
            if (b != null)
            {
                return b.ClacValue((va, vb) => (va != null) ? vb : va, a);
            }
            return a;
        }

        public static T operator |(T a, FieldReaderInst<T> b)
        {
            if (b != null)
            {
                return b.ClacValue((va, vb) => va ?? vb, a);
            }
            return a;
        }

        public static FieldReaderInst<T> operator &(FieldReaderInst<T> a, FieldReaderInst<T> b)
            => ClacValue((va, vb) => (va != null) ? vb : va, a, b);

        public static FieldReaderInst<T> operator |(FieldReaderInst<T> a, FieldReaderInst<T> b)
            => ClacValue((va, vb) => va ?? vb, a, b);
    }
}
