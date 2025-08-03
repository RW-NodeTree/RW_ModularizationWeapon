using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Xml;
using Verse;
using HarmonyLib;
using RW_ModularizationWeapon.Tools;
using System.Collections.Concurrent;

namespace RW_ModularizationWeapon
{
    /// <summary>
    /// abstract type for instance calculation
    /// </summary>
    /// <typeparam name="T">instance base type for calculation</typeparam>
    /// <typeparam name="TV">instance properties base type for calculation</typeparam>
    public abstract class FieldReader<T, TV> : IDictionary<RuntimeFieldHandle, TV>
    {
        private Type type = typeof(T);
        /// <summary>
        /// current using type
        /// </summary>
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
        /// <summary>
        /// default properties value, if this calculation instance containe field
        /// </summary>
        public abstract TV DefaultValue { get; set; }
        /// <summary>
        /// check default value of this instance
        /// </summary>
        public abstract bool HasDefaultValue { get; }
        /// <summary>
        /// value of instance propertie on specific field for calculation
        /// </summary>
        /// <param name="key">specific field</param>
        /// <returns>specific field for calculation</returns>
        public abstract TV this[RuntimeFieldHandle key] { get; set; }
        /// <summary>
        /// field container, defined what field will calculation
        /// </summary>
        public abstract ICollection<RuntimeFieldHandle> Keys { get; }
        /// <summary>
        /// value container, defined what values of each `Keys`
        /// </summary>
        public abstract ICollection<TV> Values { get; }
        /// <summary>
        /// defined values count
        /// </summary>
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

        public bool Contains(KeyValuePair<RuntimeFieldHandle, TV> item) => TryGetValue(item.Key, out TV? value) && (object?)value == (object?)item.Value;

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
        /// <summary>
        /// universal calculation, `FieldReader` with single value
        /// </summary>
        /// <param name="calc">calculation function</param>
        /// <param name="value">single value for calculate</param>
        /// <returns>after calculation `FieldReader`</returns>
        public FieldReader<T, TV> CalcValue(Func<TV, TV, RuntimeFieldHandle, TV> calc, TV value)
        {
            FieldReader<T, TV> result = this.Clone();
            if(calc != null && value != null)
            {
                foreach (RuntimeFieldHandle field in this.Keys)
                {
                    TV origin = result[field];
                    result[field] = calc(origin, value, field);
                }
            }
            return result;
        }
        /// <summary>
        /// universal calculation, `FieldReader` with instance
        /// </summary>
        /// <param name="calc">calculation function</param>
        /// <param name="orginal">instance for calculate</param>
        /// <returns>instance after calculation</returns>
        public T? CalcValue(Func<TV, TV, RuntimeFieldHandle, TV> calc, T? orginal)
        {
            if (orginal != null)
            {
                T result = Gen.MemberwiseClone(orginal);
                if (calc != null)
                {
                    foreach (FieldInfo field in orginal.GetType().GetCachedInstanceFields())
                    {
                        if (UsedType.IsAssignableFrom(field.DeclaringType) && typeof(TV).IsAssignableFrom(field.FieldType))
                        {
                            RuntimeFieldHandle handle = field.FieldHandle;
                            if (!TryGetValue(handle, out TV value)) value = DefaultValue;
                            ref TV valRef = ref GetCachedFieldRef(handle)(result);
                            valRef = calc(valRef, value, handle);
                        }
                    }
                }
                return result;
            }
            return orginal;
        }
        /// <summary>
        /// universal calculation, `FieldReader` with `FieldReader`
        /// </summary>
        /// <typeparam name="TFR">result type</typeparam>
        /// <param name="calc">calculation function</param>
        /// <param name="a">`FieldReader` value a</param>
        /// <param name="b">`FieldReader` value b</param>
        /// <returns>after calculate `FieldReader`</returns>
        public static TFR? CalcValue<TFR>(Func<TV, TV, RuntimeFieldHandle, TV> calc, TFR? a, TFR? b) where TFR : FieldReader<T, TV>, new()
        {

            TFR result = new TFR();

            if (a == null && b == null) return null;

            a = a ?? new TFR();
            b = b ?? new TFR();

            if (a.UsedType.IsAssignableFrom(b.UsedType)) result.UsedType = b.UsedType;
            else if (b.UsedType.IsAssignableFrom(a.UsedType)) result.UsedType = a.UsedType;

            foreach (RuntimeFieldHandle field in a.Keys)
            {
                FieldInfo info = FieldInfo.GetFieldFromHandle(field);
                if (result.UsedType.IsAssignableFrom(info.DeclaringType))
                {
                    if (b.ContainsKey(field)) result.Add(field, calc(a[field], b[field], field));
                    else result.Add(field, calc(a[field], b.DefaultValue, field));
                }
            }

            foreach (RuntimeFieldHandle field in b.Keys)
            {
                FieldInfo info = FieldInfo.GetFieldFromHandle(field);
                if (result.UsedType.IsAssignableFrom(info.DeclaringType) && !result.ContainsKey(field))
                {
                    if (a.ContainsKey(field)) result.Add(field, calc(a[field], b[field], field));
                    else result.Add(field, calc(a.DefaultValue, b[field], field));
                }
            }
            return result;
        }


        public virtual void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            string? typename = xmlRoot.Attributes["Reader-Class"]?.Value;
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

        private static readonly ConcurrentDictionary<RuntimeFieldHandle, AccessTools.FieldRef<T, TV>> cachedFieldRef = new ConcurrentDictionary<RuntimeFieldHandle, AccessTools.FieldRef<T, TV>>();
        public static AccessTools.FieldRef<T, TV> GetCachedFieldRef(RuntimeFieldHandle runtimeField)
            => cachedFieldRef.GetOrAdd(runtimeField, (x) => AccessTools.FieldRefAccess<T, TV>(FieldInfo.GetFieldFromHandle(x)));
    }
    /// <summary>
    /// digit only calculater
    /// </summary>
    /// <typeparam name="T">instance base type for calculation</typeparam>
    public class FieldReaderDigit<T> : FieldReader<T, IConvertible>
    {
        private double? defaultValue;
        private readonly Dictionary<RuntimeFieldHandle, double> datas = new Dictionary<RuntimeFieldHandle, double>();

        public FieldReaderDigit() { }

        public FieldReaderDigit(FieldReaderDigit<T> other)
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

        /// <summary>
        /// Xml Usage Example
        /// ```xml
        ///<xx Reader-Class="c# type" Default="default_value">
        ///   <member_name_of_type>value</member_name_of_type>
        ///</xx>
        /// ```
        /// </summary>
        /// <param name="xmlRoot"></param>
        public override void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            base.LoadDataFromXmlCustom(xmlRoot);
            try
            {
                string? defaultValue = xmlRoot.Attributes["Default"]?.Value;
                if (defaultValue != null) this.defaultValue = ParseHelper.FromString<double>(defaultValue);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
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

        public override FieldReader<T, IConvertible> Clone() => new FieldReaderDigit<T>(this);

        public override void UsedTypeUpdate() { }

        public static FieldReaderDigit<T>? operator +(FieldReaderDigit<T>? a, double b)
            => (a?.CalcValue((av, bv, field) => av.ToDouble(null) + bv.ToDouble(null), b) as FieldReaderDigit<T>) ?? a;

        public static FieldReaderDigit<T>? operator -(FieldReaderDigit<T>? a, double b)
            => (a?.CalcValue((av, bv, field) => av.ToDouble(null) - bv.ToDouble(null), b) as FieldReaderDigit<T>) ?? a;

        public static FieldReaderDigit<T>? operator *(FieldReaderDigit<T>? a, double b)
            => (a?.CalcValue((av, bv, field) => av.ToDouble(null) * bv.ToDouble(null), b) as FieldReaderDigit<T>) ?? a;

        public static FieldReaderDigit<T>? operator /(FieldReaderDigit<T>? a, double b)
            => (a?.CalcValue((av, bv, field) => av.ToDouble(null) / bv.ToDouble(null), b) as FieldReaderDigit<T>) ?? a;

        public static FieldReaderDigit<T>? operator %(FieldReaderDigit<T>? a, double b)
            => (a?.CalcValue((av, bv, field) => av.ToDouble(null) % bv.ToDouble(null), b) as FieldReaderDigit<T>) ?? a;


        public static T? operator +(T? a, FieldReaderDigit<T>? b)
        {
            if (b != null)
            {
                a = b.CalcValue((va, vb, field) =>
                {
                    if (va != null)
                    {
                        if (va is int) return (int)(va.ToDouble(null) + vb.ToDouble(null));
                        else if (va is float) return (float)(va.ToDouble(null) + vb.ToDouble(null));
                        else if (va is long) return (long)(va.ToDouble(null) + vb.ToDouble(null));
                        else if (va is sbyte) return (sbyte)(va.ToDouble(null) + vb.ToDouble(null));
                        else if (va is double) return va.ToDouble(null) + vb.ToDouble(null);
                    }
                    return va ?? 0;
                }, a);
            }
            return a;
        }

        public static T? operator -(T? a, FieldReaderDigit<T>? b)
        {
            if (b != null)
            {
                a = b.CalcValue((va, vb, field) =>
                {
                    if (va != null)
                    {
                        if (va is int) return (int)(va.ToDouble(null) - vb.ToDouble(null));
                        else if (va is float) return (float)(va.ToDouble(null) - vb.ToDouble(null));
                        else if (va is long) return (long)(va.ToDouble(null) - vb.ToDouble(null));
                        else if (va is sbyte) return (sbyte)(va.ToDouble(null) - vb.ToDouble(null));
                        else if (va is double) return va.ToDouble(null) - vb.ToDouble(null);
                    }
                    return va ?? 0;
                }, a);
            }
            return a;
        }

        public static T? operator *(T? a, FieldReaderDigit<T>? b)
        {
            if (b != null)
            {
                a = b.CalcValue((va, vb, field) =>
                {
                    if (va != null)
                    {
                        if (va is int) return (int)(va.ToDouble(null) * vb.ToDouble(null));
                        else if (va is float) return (float)(va.ToDouble(null) * vb.ToDouble(null));
                        else if (va is long) return (long)(va.ToDouble(null) * vb.ToDouble(null));
                        else if (va is sbyte) return (sbyte)(va.ToDouble(null) * vb.ToDouble(null));
                        else if (va is double) return va.ToDouble(null) * vb.ToDouble(null);
                    }
                    return va ?? 0;
                }, a);
            }
            return a;
        }

        public static T? operator /(T? a, FieldReaderDigit<T>? b)
        {
            if (b != null)
            {
                a = b.CalcValue((va, vb, field) =>
                {
                    if (va != null)
                    {
                        if (va is int) return (int)(va.ToDouble(null) / vb.ToDouble(null));
                        else if (va is float) return (float)(va.ToDouble(null) / vb.ToDouble(null));
                        else if (va is long) return (long)(va.ToDouble(null) / vb.ToDouble(null));
                        else if (va is sbyte) return (sbyte)(va.ToDouble(null) / vb.ToDouble(null));
                        else if (va is double) return va.ToDouble(null) / vb.ToDouble(null);
                    }
                    return va ?? 0;
                }, a);
            }
            return a;
        }

        public static T? operator %(T? a, FieldReaderDigit<T>? b)
        {
            if (b != null)
            {
                a = b.CalcValue((va, vb, field) =>
                {
                    if (va != null)
                    {
                        if (va is int) return (int)(va.ToDouble(null) % vb.ToDouble(null));
                        else if (va is float) return (float)(va.ToDouble(null) % vb.ToDouble(null));
                        else if (va is long) return (long)(va.ToDouble(null) % vb.ToDouble(null));
                        else if (va is sbyte) return (sbyte)(va.ToDouble(null) % vb.ToDouble(null));
                        else if (va is double) return va.ToDouble(null) % vb.ToDouble(null);
                    }
                    return va ?? 0;
                }, a);
            }
            return a;
        }

        public static FieldReaderDigit<T>? operator +(FieldReaderDigit<T>? a, FieldReaderDigit<T>? b)
            => CalcValue((va, vb, field) => va.ToDouble(null) + vb.ToDouble(null), a, b);

        public static FieldReaderDigit<T>? operator -(FieldReaderDigit<T>? a, FieldReaderDigit<T>? b)
            => CalcValue((va, vb, field) => va.ToDouble(null) - vb.ToDouble(null), a, b);

        public static FieldReaderDigit<T>? operator *(FieldReaderDigit<T>? a, FieldReaderDigit<T>? b)
            => CalcValue((va, vb, field) => va.ToDouble(null) * vb.ToDouble(null), a, b);

        public static FieldReaderDigit<T>? operator /(FieldReaderDigit<T>? a, FieldReaderDigit<T>? b)
            => CalcValue((va, vb, field) => va.ToDouble(null) / vb.ToDouble(null), a, b);

        public static FieldReaderDigit<T>? operator %(FieldReaderDigit<T>? a, FieldReaderDigit<T>? b)
            => CalcValue((va, vb, field) => va.ToDouble(null) % vb.ToDouble(null), a, b);
    }
    /// <summary>
    /// boolean only calculater
    /// </summary>
    /// <typeparam name="T">instance base type for calculation</typeparam>
    public class FieldReaderBoolean<T> : FieldReader<T, bool>
    {
        private bool? defaultValue;
        private readonly Dictionary<RuntimeFieldHandle, bool> datas = new Dictionary<RuntimeFieldHandle, bool>();

        public FieldReaderBoolean() { }

        public FieldReaderBoolean(FieldReaderBoolean<T> other)
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


        /// <summary>
        /// Xml Usage Example
        /// ```xml
        ///<xx Reader-Class="c# type" Default="default_value">
        ///   <member_name_of_type>value</member_name_of_type>
        ///</xx>
        /// ```
        /// </summary>
        /// <param name="xmlRoot"></param>
        public override void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            base.LoadDataFromXmlCustom(xmlRoot);
            try
            {
                string? defaultValue = xmlRoot.Attributes["Default"]?.Value;
                if (defaultValue != null) this.defaultValue = ParseHelper.FromString<bool>(defaultValue);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
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

        public override FieldReader<T, bool> Clone() => new FieldReaderBoolean<T>(this);

        public override void UsedTypeUpdate() { }

        public static FieldReaderBoolean<T>? operator &(FieldReaderBoolean<T>? a, bool b)
            => (a?.CalcValue((av, bv, field) => av && bv, b) as FieldReaderBoolean<T>) ?? a;

        public static FieldReaderBoolean<T>? operator |(FieldReaderBoolean<T>? a, bool b)
            => (a?.CalcValue((av, bv, field) => av || bv, b) as FieldReaderBoolean<T>) ?? a;

        public static FieldReaderBoolean<T>? operator !(FieldReaderBoolean<T>? a)
            => (a?.CalcValue((av, bv, field) => !av, false) as FieldReaderBoolean<T>) ?? a;


        public static T? operator &(T? a, FieldReaderBoolean<T>? b)
        {
            if (b != null)
            {
                return b.CalcValue((va, vb, field) => va && vb, a);
            }
            return a;
        }

        public static T? operator |(T? a, FieldReaderBoolean<T>? b)
        {
            if (b != null)
            {
                return b.CalcValue((va, vb, field) => va || vb, a);
            }
            return a;
        }

        public static FieldReaderBoolean<T>? operator &(FieldReaderBoolean<T>? a, FieldReaderBoolean<T>? b)
            => CalcValue((va, vb, field) => va && vb, a, b);

        public static FieldReaderBoolean<T>? operator |(FieldReaderBoolean<T>? a, FieldReaderBoolean<T>? b)
            => CalcValue((va, vb, field) => va || vb, a, b);
    }
    /// <summary>
    /// instance only calculater
    /// </summary>
    /// <typeparam name="T">instance base type for calculation</typeparam>
    public class FieldReaderInstance<T> : FieldReader<T, object?>
    {
        private bool loading = false;
        private T datas = (T)Activator.CreateInstance(typeof(T));
        private readonly HashSet<RuntimeFieldHandle> fields = new HashSet<RuntimeFieldHandle>();

        public FieldReaderInstance() { }

        public FieldReaderInstance(FieldReaderInstance<T> other)
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

        public override ICollection<object?> Values => (from x in fields select FieldInfo.GetFieldFromHandle(x).GetValue(datas)).ToList();

        public override object? DefaultValue { get => null; set => throw new NotImplementedException(); }

        public override bool HasDefaultValue => false;

        public override object? this[RuntimeFieldHandle key]
        {
            get => (key != null && fields.Contains(key)) ? FieldInfo.GetFieldFromHandle(key).GetValue(datas) : null;
            set => Add(key, value);
        }


        /// <summary>
        /// Xml Usage Example
        /// ```xml
        ///<xx Reader-Class="c# type">
        ///   <member_name_of_type>value</member_name_of_type>
        ///</xx>
        /// ```
        /// </summary>
        /// <param name="xmlRoot"></param>
        public override void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            loading = true;
            base.LoadDataFromXmlCustom(xmlRoot);
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

        public override void Add(RuntimeFieldHandle key, object? value)
        {
            FieldInfo field = FieldInfo.GetFieldFromHandle(key);
            if (field != null && field.DeclaringType.IsAssignableFrom(UsedType))
            {
                if (value == null || (field.FieldType.IsAssignableFrom(value.GetType()) && field.DeclaringType.IsAssignableFrom(datas?.GetType() ?? UsedType)))
                {
                    field.SetValue(datas, value);
                    fields.Add(key);
                }
            }
        }

        public override bool Remove(RuntimeFieldHandle key) => fields.Remove(key);

        public override bool TryGetValue(RuntimeFieldHandle key, out object? value)
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

        public override FieldReader<T, object?> Clone() => new FieldReaderInstance<T>(this);

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

        public static T? operator &(T? a, FieldReaderInstance<T>? b)
        {
            if (b != null)
            {
                return b.CalcValue((va, vb, field) => (va != null && b.ContainsKey(field)) ? vb : va, a);
            }
            return a;
        }

        public static T? operator |(T? a, FieldReaderInstance<T>? b)
        {
            if (b != null)
            {
                return b.CalcValue((va, vb, field) => va ?? vb, a);
            }
            return a;
        }

        public static FieldReaderInstance<T>? operator &(FieldReaderInstance<T>? a, FieldReaderInstance<T>? b)
            => CalcValue((va, vb, field) => (va != null && (b?.ContainsKey(field) ?? false)) ? vb : va, a, b);

        public static FieldReaderInstance<T>? operator |(FieldReaderInstance<T>? a, FieldReaderInstance<T>? b)
            => CalcValue((va, vb, field) => va ?? vb, a, b);
    }
    /// <summary>
    /// key filter for `FieldReader`
    /// </summary>
    /// <typeparam name="T">instance base type for calculation</typeparam>
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



        /// <summary>
        /// Xml Usage Example
        /// ```xml
        ///<xx Reader-Class="c# type" Default="default_value">
        ///   <member_name_of_type>value</member_name_of_type>
        ///</xx>
        /// ```
        /// </summary>
        /// <param name="xmlRoot"></param>
        public override void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            base.LoadDataFromXmlCustom(xmlRoot);
            try
            {
                string? defaultValue = xmlRoot.Attributes["Default"]?.Value;
                if (defaultValue != null) this.defaultValue = ParseHelper.FromString<bool>(defaultValue);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
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

        public static FieldReaderFilt<T>? operator &(FieldReaderFilt<T>? a, bool b)
            => (a?.CalcValue((av, bv, field) => av && bv, b) as FieldReaderFilt<T>) ?? a;

        public static FieldReaderFilt<T>? operator |(FieldReaderFilt<T>? a, bool b)
            => (a?.CalcValue((av, bv, field) => av || bv, b) as FieldReaderFilt<T>) ?? a;

        public static FieldReaderFilt<T>? operator !(FieldReaderFilt<T>? a)
            => (a?.CalcValue((av, bv, field) => !av, false) as FieldReaderFilt<T>) ?? a;

        public static FieldReaderFilt<T>? operator &(FieldReaderFilt<T>? a, FieldReaderFilt<T>? b)
            => CalcValue((va, vb, field) => va && vb, a, b);

        public static FieldReaderFilt<T>? operator |(FieldReaderFilt<T>? a, FieldReaderFilt<T>? b)
            => CalcValue((va, vb, field) => va || vb, a, b);

        public static FieldReaderDigit<T>? operator &(FieldReaderDigit<T>? a, FieldReaderFilt<T>? b)
        {
            if(a != null && b != null)
            {
                FieldReaderDigit<T> org = a;
                a = (FieldReaderDigit<T>)a.Clone();
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

        public static FieldReaderBoolean<T>? operator &(FieldReaderBoolean<T>? a, FieldReaderFilt<T>? b)
        {
            if (a != null && b != null)
            {
                FieldReaderBoolean<T> org = a;
                a = (FieldReaderBoolean<T>)a.Clone();
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

        public static FieldReaderInstance<T>? operator &(FieldReaderInstance<T>? a, FieldReaderFilt<T>? b)
        {
            if (a != null && b != null)
            {
                FieldReaderInstance<T> org = a;
                a = (FieldReaderInstance<T>)a.Clone();
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
