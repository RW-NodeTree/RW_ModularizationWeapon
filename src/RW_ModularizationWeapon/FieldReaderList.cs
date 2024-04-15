using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Verse;

namespace RW_ModularizationWeapon
{
    public class FieldReaderDgitList<T> : List<FieldReaderDigit<T>>
    {
        private double? defaultValue;

        public FieldReaderDgitList() : base() { }

        public FieldReaderDgitList(FieldReaderDgitList<T> other) : base(other)
        {
            defaultValue = other.defaultValue;
        }

        public double DefaultValue
        {
            get => defaultValue ?? 0;
            set => defaultValue = value;
        }

        public bool HasDefaultValue => defaultValue.HasValue;

        /// <summary>
        /// Xml Usage Example
        /// ```xml
        ///<xx Default="default_value">
        ///    <li Reader-Class="c# type1" Default="default_value1">
        ///        <member_name_of_type>value</member_name_of_type>
        ///    </li>
        ///    <li Reader-Class="c# type2" Default="default_value2">
        ///        <member_name_of_type>value</member_name_of_type>
        ///    </li>
        ///</xx>
        /// ```
        /// </summary>
        /// <param name="xmlRoot"></param>
        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {

            try
            {
                string defaultValue = xmlRoot.Attributes["Default"]?.Value;
                if (defaultValue != null) this.defaultValue = ParseHelper.FromString<double>(defaultValue);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
            try
            {
                AddRange((List<FieldReaderDigit<T>>)DirectXmlToObject.GetObjectFromXmlMethod(typeof(List<FieldReaderDigit<T>>))(xmlRoot, true));
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }


        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"defaultValue : {defaultValue}");
            for (int i = 0; i < this.Count; i++)
            {
                FieldReaderDigit<T> field = this[i];
                builder.AppendLine($"{i} : {field}");
            }
            return builder.ToString();
        }


        #region FieldReaderDgitList_double
        public static FieldReaderDgitList<T> operator +(FieldReaderDgitList<T> a, double b)
        {
            FieldReaderDgitList<T> list = new FieldReaderDgitList<T>(a);
            list.RemoveAll(x => x == null);
            for (int i = 0; i < list.Count; i++)
            {
                list[i] += b;
            }
            return list;
        }

        public static FieldReaderDgitList<T> operator -(FieldReaderDgitList<T> a, double b)
        {
            FieldReaderDgitList<T> list = new FieldReaderDgitList<T>(a);
            list.RemoveAll(x => x == null);
            for (int i = 0; i < list.Count; i++)
            {
                list[i] -= b;
            }
            return list;
        }

        public static FieldReaderDgitList<T> operator *(FieldReaderDgitList<T> a, double b)
        {
            FieldReaderDgitList<T> list = new FieldReaderDgitList<T>(a);
            list.RemoveAll(x => x == null);
            for (int i = 0; i < list.Count; i++)
            {
                list[i] *= b;
            }
            return list;
        }

        public static FieldReaderDgitList<T> operator /(FieldReaderDgitList<T> a, double b)
        {
            FieldReaderDgitList<T> list = new FieldReaderDgitList<T>(a);
            list.RemoveAll(x => x == null);
            for (int i = 0; i < list.Count; i++)
            {
                list[i] /= b;
            }
            return list;
        }

        public static FieldReaderDgitList<T> operator %(FieldReaderDgitList<T> a, double b)
        {
            FieldReaderDgitList<T> list = new FieldReaderDgitList<T>(a);
            list.RemoveAll(x => x == null);
            for (int i = 0; i < list.Count; i++)
            {
                list[i] %= b;
            }
            return list;
        }
        #endregion

        #region T_FieldReaderDgitList
        public static T operator +(T a, FieldReaderDgitList<T> b)
        {
            if(a != null)
            {
                T result = Gen.MemberwiseClone(a);
                if(b != null)
                {
                    for (int i = 0; i < b.Count; i++)
                    {
                        FieldReaderDigit<T> v = b[i];
                        if (v != null)
                        {
                            if(!v.HasDefaultValue)
                            {
                                v = new FieldReaderDigit<T>(v);
                                v.DefaultValue = b.DefaultValue;
                            }
                            result += v;
                        }
                    }
                }
                return result;
            }
            return default(T);
        }

        public static T operator -(T a, FieldReaderDgitList<T> b)
        {
            if (a != null)
            {
                T result = Gen.MemberwiseClone(a);
                if (b != null)
                {
                    for (int i = 0; i < b.Count; i++)
                    {
                        FieldReaderDigit<T> v = b[i];
                        if (v != null)
                        {
                            if (!v.HasDefaultValue)
                            {
                                v = new FieldReaderDigit<T>(v);
                                v.DefaultValue = b.DefaultValue;
                            }
                            result -= v;
                        }
                    }
                }
                return result;
            }
            return default(T);
        }

        public static T operator *(T a, FieldReaderDgitList<T> b)
        {
            if (a != null)
            {
                T result = Gen.MemberwiseClone(a);
                if (b != null)
                {
                    for (int i = 0; i < b.Count; i++)
                    {
                        FieldReaderDigit<T> v = b[i];
                        if (v != null)
                        {
                            if (!v.HasDefaultValue)
                            {
                                v = new FieldReaderDigit<T>(v);
                                v.DefaultValue = b.DefaultValue;
                            }
                            result *= v;
                        }
                    }
                }
                return result;
            }
            return default(T);
        }

        public static T operator /(T a, FieldReaderDgitList<T> b)
        {
            if (a != null)
            {
                T result = Gen.MemberwiseClone(a);
                if (b != null)
                {
                    for (int i = 0; i < b.Count; i++)
                    {
                        FieldReaderDigit<T> v = b[i];
                        if (v != null)
                        {
                            if (!v.HasDefaultValue)
                            {
                                v = new FieldReaderDigit<T>(v);
                                v.DefaultValue = b.DefaultValue;
                            }
                            result /= v;
                        }
                    }
                }
                return result;
            }
            return default(T);
        }

        public static T operator %(T a, FieldReaderDgitList<T> b)
        {
            if (a != null)
            {
                T result = Gen.MemberwiseClone(a);
                if (b != null)
                {
                    for (int i = 0; i < b.Count; i++)
                    {
                        FieldReaderDigit<T> v = b[i];
                        if (v != null)
                        {
                            if (!v.HasDefaultValue)
                            {
                                v = new FieldReaderDigit<T>(v);
                                v.DefaultValue = b.DefaultValue;
                            }
                            result %= v;
                        }
                    }
                }
                return result;
            }
            return default(T);
        }
        #endregion

        #region FieldReaderDgitList_FieldReaderDgitList
        public static FieldReaderDgitList<T> operator +(FieldReaderDgitList<T> a, FieldReaderDgitList<T> b)
        {
            FieldReaderDgitList<T> result = new FieldReaderDgitList<T>();

            if (a == null && b == null) return result;

            a = a ?? new FieldReaderDgitList<T>();
            b = b ?? new FieldReaderDgitList<T>();

            HashSet<Type> UsedTypes = new HashSet<Type>();

            if (a.HasDefaultValue) result.DefaultValue = a.DefaultValue;
            for (int i = 0; i < a.Count; i++)
            {
                FieldReaderDigit<T> child = new FieldReaderDigit<T>(a[i]);
                if (result.HasDefaultValue && !child.HasDefaultValue) child.DefaultValue = result.DefaultValue;
                int index = result.FindIndex(x => x.UsedType == child.UsedType);
                if (index < 0)
                {
                    index = result.Count;
                    result.Add(child);
                }
                else result[index] += child;
                if (result.HasDefaultValue && !result[index].HasDefaultValue) result[index].DefaultValue = result.DefaultValue;
                UsedTypes.Add(child.UsedType);
            }
            a = result;
            result = new FieldReaderDgitList<T>();

            if (b.HasDefaultValue) result.DefaultValue = b.DefaultValue;
            for (int i = 0; i < b.Count; i++)
            {
                FieldReaderDigit<T> child = new FieldReaderDigit<T>(b[i]);
                if (result.HasDefaultValue && !child.HasDefaultValue) child.DefaultValue = result.DefaultValue;
                int index = result.FindIndex(x => x.UsedType == child.UsedType);
                if (index < 0)
                {
                    index = result.Count;
                    result.Add(child);
                }
                else result[index] += child;
                if (result.HasDefaultValue && !result[index].HasDefaultValue) result[index].DefaultValue = result.DefaultValue;
                UsedTypes.Add(child.UsedType);
            }
            b = result;
            result = new FieldReaderDgitList<T>();

            foreach(Type type in UsedTypes)
            {
                FieldReaderDigit<T> va = a.Find(x => x.UsedType == type) ?? new FieldReaderDigit<T>();
                FieldReaderDigit<T> vb = b.Find(x => x.UsedType == type) ?? new FieldReaderDigit<T>();
                if (a.HasDefaultValue && !va.HasDefaultValue) va.DefaultValue = a.DefaultValue;
                if (b.HasDefaultValue && !vb.HasDefaultValue) vb.DefaultValue = b.DefaultValue;
                result.Add(va + vb);
            }
            return result;
        }

        public static FieldReaderDgitList<T> operator -(FieldReaderDgitList<T> a, FieldReaderDgitList<T> b)
        {
            FieldReaderDgitList<T> result = new FieldReaderDgitList<T>();

            if (a == null && b == null) return result;

            a = a ?? new FieldReaderDgitList<T>();
            b = b ?? new FieldReaderDgitList<T>();

            HashSet<Type> UsedTypes = new HashSet<Type>();

            if (a.HasDefaultValue) result.DefaultValue = a.DefaultValue;
            for (int i = 0; i < a.Count; i++)
            {
                FieldReaderDigit<T> child = new FieldReaderDigit<T>(a[i]);
                if (result.HasDefaultValue && !child.HasDefaultValue) child.DefaultValue = result.DefaultValue;
                int index = result.FindIndex(x => x.UsedType == child.UsedType);
                if (index < 0)
                {
                    index = result.Count;
                    result.Add(child);
                }
                else result[index] -= child;
                if (result.HasDefaultValue && !result[index].HasDefaultValue) result[index].DefaultValue = result.DefaultValue;
                UsedTypes.Add(child.UsedType);
            }
            a = result;
            result = new FieldReaderDgitList<T>();

            if (b.HasDefaultValue) result.DefaultValue = b.DefaultValue;
            for (int i = 0; i < b.Count; i++)
            {
                FieldReaderDigit<T> child = new FieldReaderDigit<T>(b[i]);
                if (result.HasDefaultValue && !child.HasDefaultValue) child.DefaultValue = result.DefaultValue;
                int index = result.FindIndex(x => x.UsedType == child.UsedType);
                if (index < 0)
                {
                    index = result.Count;
                    result.Add(child);
                }
                else result[index] -= child;
                if (result.HasDefaultValue && !result[index].HasDefaultValue) result[index].DefaultValue = result.DefaultValue;
                UsedTypes.Add(child.UsedType);
            }
            b = result;
            result = new FieldReaderDgitList<T>();

            foreach (Type type in UsedTypes)
            {
                FieldReaderDigit<T> va = a.Find(x => x.UsedType == type) ?? new FieldReaderDigit<T>();
                FieldReaderDigit<T> vb = b.Find(x => x.UsedType == type) ?? new FieldReaderDigit<T>();
                if (a.HasDefaultValue && !va.HasDefaultValue) va.DefaultValue = a.DefaultValue;
                if (b.HasDefaultValue && !vb.HasDefaultValue) vb.DefaultValue = b.DefaultValue;
                result.Add(va - vb);
            }
            return result;
        }


        public static FieldReaderDgitList<T> operator *(FieldReaderDgitList<T> a, FieldReaderDgitList<T> b)// a : mul, b : fordebug
        {
            FieldReaderDgitList<T> result = new FieldReaderDgitList<T>();

            if (a == null && b == null) return result;

            a = a ?? new FieldReaderDgitList<T>();
            b = b ?? new FieldReaderDgitList<T>();

            HashSet<Type> UsedTypes = new HashSet<Type>();

            if (a.HasDefaultValue) result.DefaultValue = a.DefaultValue;
            for (int i = 0; i < a.Count; i++)
            {
                FieldReaderDigit<T> child = new FieldReaderDigit<T>(a[i]);
                if (result.HasDefaultValue && !child.HasDefaultValue) child.DefaultValue = result.DefaultValue;
                int index = result.FindIndex(x => x.UsedType == child.UsedType);
                if (index < 0)
                {
                    index = result.Count;
                    result.Add(child);
                }
                else result[index] *= child;
                if (result.HasDefaultValue && !result[index].HasDefaultValue) result[index].DefaultValue = result.DefaultValue;
                UsedTypes.Add(child.UsedType);
            }
            a = result;
            result = new FieldReaderDgitList<T>();

            if (b.HasDefaultValue) result.DefaultValue = b.DefaultValue;
            for (int i = 0; i < b.Count; i++)
            {
                FieldReaderDigit<T> child = new FieldReaderDigit<T>(b[i]);
                if (result.HasDefaultValue && !child.HasDefaultValue) child.DefaultValue = result.DefaultValue;
                int index = result.FindIndex(x => x.UsedType == child.UsedType);
                if (index < 0)
                {
                    index = result.Count;
                    result.Add(child);
                }
                else result[index] *= child;
                if (result.HasDefaultValue && !result[index].HasDefaultValue) result[index].DefaultValue = result.DefaultValue;
                UsedTypes.Add(child.UsedType);
            }
            b = result;
            result = new FieldReaderDgitList<T>();

            foreach (Type type in UsedTypes)
            {
                FieldReaderDigit<T> va = a.Find(x => x.UsedType == type) ?? new FieldReaderDigit<T>();
                FieldReaderDigit<T> vb = b.Find(x => x.UsedType == type) ?? new FieldReaderDigit<T>();
                if (a.HasDefaultValue && !va.HasDefaultValue) va.DefaultValue = a.DefaultValue;
                if (b.HasDefaultValue && !vb.HasDefaultValue) vb.DefaultValue = b.DefaultValue;
                result.Add(va * vb);
            }
            return result;
        }
        

        public static FieldReaderDgitList<T> operator /(FieldReaderDgitList<T> a, FieldReaderDgitList<T> b)
        {
            FieldReaderDgitList<T> result = new FieldReaderDgitList<T>();

            if (a == null && b == null) return result;

            a = a ?? new FieldReaderDgitList<T>();
            b = b ?? new FieldReaderDgitList<T>();

            HashSet<Type> UsedTypes = new HashSet<Type>();

            if (a.HasDefaultValue) result.DefaultValue = a.DefaultValue;
            for (int i = 0; i < a.Count; i++)
            {
                FieldReaderDigit<T> child = new FieldReaderDigit<T>(a[i]);
                if (result.HasDefaultValue && !child.HasDefaultValue) child.DefaultValue = result.DefaultValue;
                int index = result.FindIndex(x => x.UsedType == child.UsedType);
                if (index < 0)
                {
                    index = result.Count;
                    result.Add(child);
                }
                else result[index] /= child;
                if (result.HasDefaultValue && !result[index].HasDefaultValue) result[index].DefaultValue = result.DefaultValue;
                UsedTypes.Add(child.UsedType);
            }
            a = result;
            result = new FieldReaderDgitList<T>();

            if (b.HasDefaultValue) result.DefaultValue = b.DefaultValue;
            for (int i = 0; i < b.Count; i++)
            {
                FieldReaderDigit<T> child = new FieldReaderDigit<T>(b[i]);
                if (result.HasDefaultValue && !child.HasDefaultValue) child.DefaultValue = result.DefaultValue;
                int index = result.FindIndex(x => x.UsedType == child.UsedType);
                if (index < 0)
                {
                    index = result.Count;
                    result.Add(child);
                }
                else result[index] /= child;
                if (result.HasDefaultValue && !result[index].HasDefaultValue) result[index].DefaultValue = result.DefaultValue;
                UsedTypes.Add(child.UsedType);
            }
            b = result;
            result = new FieldReaderDgitList<T>();

            foreach (Type type in UsedTypes)
            {
                FieldReaderDigit<T> va = a.Find(x => x.UsedType == type) ?? new FieldReaderDigit<T>();
                FieldReaderDigit<T> vb = b.Find(x => x.UsedType == type) ?? new FieldReaderDigit<T>();
                if (a.HasDefaultValue && !va.HasDefaultValue) va.DefaultValue = a.DefaultValue;
                if (b.HasDefaultValue && !vb.HasDefaultValue) vb.DefaultValue = b.DefaultValue;
                result.Add(va / vb);
            }
            return result;
        }

        public static FieldReaderDgitList<T> operator %(FieldReaderDgitList<T> a, FieldReaderDgitList<T> b)
        {
            FieldReaderDgitList<T> result = new FieldReaderDgitList<T>();

            if (a == null && b == null) return result;

            a = a ?? new FieldReaderDgitList<T>();
            b = b ?? new FieldReaderDgitList<T>();

            HashSet<Type> UsedTypes = new HashSet<Type>();

            if (a.HasDefaultValue) result.DefaultValue = a.DefaultValue;
            for (int i = 0; i < a.Count; i++)
            {
                FieldReaderDigit<T> child = new FieldReaderDigit<T>(a[i]);
                if (result.HasDefaultValue && !child.HasDefaultValue) child.DefaultValue = result.DefaultValue;
                int index = result.FindIndex(x => x.UsedType == child.UsedType);
                if (index < 0)
                {
                    index = result.Count;
                    result.Add(child);
                }
                else result[index] %= child;
                if (result.HasDefaultValue && !result[index].HasDefaultValue) result[index].DefaultValue = result.DefaultValue;
                UsedTypes.Add(child.UsedType);
            }
            a = result;
            result = new FieldReaderDgitList<T>();

            if (b.HasDefaultValue) result.DefaultValue = b.DefaultValue;
            for (int i = 0; i < b.Count; i++)
            {
                FieldReaderDigit<T> child = new FieldReaderDigit<T>(b[i]);
                if (result.HasDefaultValue && !child.HasDefaultValue) child.DefaultValue = result.DefaultValue;
                int index = result.FindIndex(x => x.UsedType == child.UsedType);
                if (index < 0)
                {
                    index = result.Count;
                    result.Add(child);
                }
                else result[index] %= child;
                if (result.HasDefaultValue && !result[index].HasDefaultValue) result[index].DefaultValue = result.DefaultValue;
                UsedTypes.Add(child.UsedType);
            }
            b = result;
            result = new FieldReaderDgitList<T>();

            foreach (Type type in UsedTypes)
            {
                FieldReaderDigit<T> va = a.Find(x => x.UsedType == type) ?? new FieldReaderDigit<T>();
                FieldReaderDigit<T> vb = b.Find(x => x.UsedType == type) ?? new FieldReaderDigit<T>();
                if (a.HasDefaultValue && !va.HasDefaultValue) va.DefaultValue = a.DefaultValue;
                if (b.HasDefaultValue && !vb.HasDefaultValue) vb.DefaultValue = b.DefaultValue;
                result.Add(va % vb);
            }
            return result;
        }
        #endregion
    }

    public class FieldReaderBoolList<T> : List<FieldReaderBoolean<T>>
    {
        private bool? defaultValue;

        public FieldReaderBoolList() : base() { }

        public FieldReaderBoolList(FieldReaderBoolList<T> other) : base(other)
        {
            defaultValue = other.defaultValue;
        }

        public bool DefaultValue
        {
            get => defaultValue ?? false;
            set => defaultValue = value;
        }

        public bool HasDefaultValue => defaultValue.HasValue;

        /// <summary>
        /// Xml Usage Example
        /// ```xml
        ///<xx Default="default_value">
        ///    <li Reader-Class="c# type1" Default="default_value1">
        ///        <member_name_of_type>value</member_name_of_type>
        ///    </li>
        ///    <li Reader-Class="c# type2" Default="default_value2">
        ///        <member_name_of_type>value</member_name_of_type>
        ///    </li>
        ///</xx>
        /// ```
        /// </summary>
        /// <param name="xmlRoot"></param>
        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {

            try
            {
                string defaultValue = xmlRoot.Attributes["Default"]?.Value;
                if (defaultValue != null) this.defaultValue = ParseHelper.FromString<bool>(defaultValue);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
            try
            {
                AddRange((List<FieldReaderBoolean<T>>)DirectXmlToObject.GetObjectFromXmlMethod(typeof(List<FieldReaderBoolean<T>>))(xmlRoot, true));
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }


        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"defaultValue : {defaultValue}");
            for (int i = 0; i < this.Count; i++)
            {
                FieldReaderBoolean<T> field = this[i];
                builder.AppendLine($"{i} : {field}");
            }
            return builder.ToString();
        }


        #region FieldReaderBoolList_bool
        public static FieldReaderBoolList<T> operator !(FieldReaderBoolList<T> a)
        {
            FieldReaderBoolList<T> list = new FieldReaderBoolList<T>(a);
            list.RemoveAll(x => x == null);
            for (int i = 0; i < list.Count; i++)
            {
                list[i] = !list[i];
            }
            return list;
        }

        public static FieldReaderBoolList<T> operator &(FieldReaderBoolList<T> a, bool b)
        {
            FieldReaderBoolList<T> list = new FieldReaderBoolList<T>(a);
            list.RemoveAll(x => x == null);
            for (int i = 0; i < list.Count; i++)
            {
                list[i] &= b;
            }
            return list;
        }

        public static FieldReaderBoolList<T> operator |(FieldReaderBoolList<T> a, bool b)
        {
            FieldReaderBoolList<T> list = new FieldReaderBoolList<T>(a);
            list.RemoveAll(x => x == null);
            for (int i = 0; i < list.Count; i++)
            {
                list[i] |= b;
            }
            return list;
        }
        #endregion

        #region T_FieldReaderBoolListList
        public static T operator &(T a, FieldReaderBoolList<T> b)
        {
            if (a != null)
            {
                T result = Gen.MemberwiseClone(a);
                if (b != null)
                {
                    for (int i = 0; i < b.Count; i++)
                    {
                        FieldReaderBoolean<T> v = b[i];
                        if (v != null)
                        {
                            if (!v.HasDefaultValue)
                            {
                                v = new FieldReaderBoolean<T>(v);
                                v.DefaultValue = b.DefaultValue;
                            }
                            result &= v;
                        }
                    }
                }
                return result;
            }
            return default(T);
        }

        public static T operator |(T a, FieldReaderBoolList<T> b)
        {
            if (a != null)
            {
                T result = Gen.MemberwiseClone(a);
                if (b != null)
                {
                    for (int i = 0; i < b.Count; i++)
                    {
                        FieldReaderBoolean<T> v = b[i];
                        if (v != null)
                        {
                            if (!v.HasDefaultValue)
                            {
                                v = new FieldReaderBoolean<T>(v);
                                v.DefaultValue = b.DefaultValue;
                            }
                            result |= v;
                        }
                    }
                }
                return result;
            }
            return default(T);
        }
        #endregion

        #region FieldReaderBoolList_FieldReaderBoolList
        public static FieldReaderBoolList<T> operator &(FieldReaderBoolList<T> a, FieldReaderBoolList<T> b)
        {
            FieldReaderBoolList<T> result = new FieldReaderBoolList<T>();

            if (a == null && b == null) return result;

            a = a ?? new FieldReaderBoolList<T>();
            b = b ?? new FieldReaderBoolList<T>();

            HashSet<Type> UsedTypes = new HashSet<Type>();

            if (a.HasDefaultValue) result.DefaultValue = a.DefaultValue;
            for (int i = 0; i < a.Count; i++)
            {
                FieldReaderBoolean<T> child = new FieldReaderBoolean<T>(a[i]);
                if (result.HasDefaultValue && !child.HasDefaultValue) child.DefaultValue = result.DefaultValue;
                int index = result.FindIndex(x => x.UsedType == child.UsedType);
                if (index < 0)
                {
                    index = result.Count;
                    result.Add(child);
                }
                else result[index] &= child;
                if (result.HasDefaultValue && !result[index].HasDefaultValue) result[index].DefaultValue = result.DefaultValue;
                UsedTypes.Add(child.UsedType);
            }
            a = result;
            result = new FieldReaderBoolList<T>();

            if (b.HasDefaultValue) result.DefaultValue = b.DefaultValue;
            for (int i = 0; i < b.Count; i++)
            {
                FieldReaderBoolean<T> child = new FieldReaderBoolean<T>(b[i]);
                if (result.HasDefaultValue && !child.HasDefaultValue) child.DefaultValue = result.DefaultValue;
                int index = result.FindIndex(x => x.UsedType == child.UsedType);
                if (index < 0)
                {
                    index = result.Count;
                    result.Add(child);
                }
                else result[index] &= child;
                if (result.HasDefaultValue && !result[index].HasDefaultValue) result[index].DefaultValue = result.DefaultValue;
                UsedTypes.Add(child.UsedType);
            }
            b = result;
            result = new FieldReaderBoolList<T>();

            foreach (Type type in UsedTypes)
            {
                FieldReaderBoolean<T> va = a.Find(x => x.UsedType == type) ?? new FieldReaderBoolean<T>();
                FieldReaderBoolean<T> vb = b.Find(x => x.UsedType == type) ?? new FieldReaderBoolean<T>();
                if (a.HasDefaultValue && !va.HasDefaultValue) va.DefaultValue = a.DefaultValue;
                if (b.HasDefaultValue && !vb.HasDefaultValue) vb.DefaultValue = b.DefaultValue;
                result.Add(va & vb);
            }
            return result;
        }

        public static FieldReaderBoolList<T> operator |(FieldReaderBoolList<T> a, FieldReaderBoolList<T> b)
        {
            FieldReaderBoolList<T> result = new FieldReaderBoolList<T>();

            if (a == null && b == null) return result;

            a = a ?? new FieldReaderBoolList<T>();
            b = b ?? new FieldReaderBoolList<T>();

            HashSet<Type> UsedTypes = new HashSet<Type>();

            if (a.HasDefaultValue) result.DefaultValue = a.DefaultValue;
            for (int i = 0; i < a.Count; i++)
            {
                FieldReaderBoolean<T> child = new FieldReaderBoolean<T>(a[i]);
                if (result.HasDefaultValue && !child.HasDefaultValue) child.DefaultValue = result.DefaultValue;
                int index = result.FindIndex(x => x.UsedType == child.UsedType);
                if (index < 0)
                {
                    index = result.Count;
                    result.Add(child);
                }
                else result[index] |= child;
                if (result.HasDefaultValue && !result[index].HasDefaultValue) result[index].DefaultValue = result.DefaultValue;
                UsedTypes.Add(child.UsedType);
            }
            a = result;
            result = new FieldReaderBoolList<T>();

            if (b.HasDefaultValue) result.DefaultValue = b.DefaultValue;
            for (int i = 0; i < b.Count; i++)
            {
                FieldReaderBoolean<T> child = new FieldReaderBoolean<T>(b[i]);
                if (result.HasDefaultValue && !child.HasDefaultValue) child.DefaultValue = result.DefaultValue;
                int index = result.FindIndex(x => x.UsedType == child.UsedType);
                if (index < 0)
                {
                    index = result.Count;
                    result.Add(child);
                }
                else result[index] |= child;
                if (result.HasDefaultValue && !result[index].HasDefaultValue) result[index].DefaultValue = result.DefaultValue;
                UsedTypes.Add(child.UsedType);
            }
            b = result;
            result = new FieldReaderBoolList<T>();

            foreach (Type type in UsedTypes)
            {
                FieldReaderBoolean<T> va = a.Find(x => x.UsedType == type) ?? new FieldReaderBoolean<T>();
                FieldReaderBoolean<T> vb = b.Find(x => x.UsedType == type) ?? new FieldReaderBoolean<T>();
                if (a.HasDefaultValue && !va.HasDefaultValue) va.DefaultValue = a.DefaultValue;
                if (b.HasDefaultValue && !vb.HasDefaultValue) vb.DefaultValue = b.DefaultValue;
                result.Add(va | vb);
            }
            return result;
        }
        #endregion
    }
    
    public class FieldReaderInstList<T> : List<FieldReaderInstance<T>>
    {

        public FieldReaderInstList() : base() { }

        public FieldReaderInstList(FieldReaderInstList<T> other) : base(other) { }

        /// <summary>
        /// Xml Usage Example
        /// ```xml
        ///<xx>
        ///    <li Class="c# type1">
        ///        <member_name_of_type>value</member_name_of_type>
        ///    </li>
        ///    <li Class="c# type2">
        ///        <member_name_of_type>value</member_name_of_type>
        ///    </li>
        ///</xx>
        /// ```
        /// </summary>
        /// <param name="xmlRoot"></param>
        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            try
            {
                AddRange((List<FieldReaderInstance<T>>)DirectXmlToObject.GetObjectFromXmlMethod(typeof(List<FieldReaderInstance<T>>))(xmlRoot, true));
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }


        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < this.Count; i++)
            {
                FieldReaderInstance<T> field = this[i];
                builder.AppendLine($"{i} : {field}");
            }
            return builder.ToString();
        }

        #region T_FieldReaderInstListList
        public static T operator &(T a, FieldReaderInstList<T> b)
        {
            if (a != null)
            {
                T result = Gen.MemberwiseClone(a);
                if (b != null)
                {
                    for (int i = 0; i < b.Count; i++)
                    {
                        FieldReaderInstance<T> v = b[i];
                        if (v != null)
                        {
                            result &= v;
                        }
                    }
                }
                return result;
            }
            return default(T);
        }

        public static T operator |(T a, FieldReaderInstList<T> b)
        {
            if (a != null)
            {
                T result = Gen.MemberwiseClone(a);
                if (b != null)
                {
                    for (int i = 0; i < b.Count; i++)
                    {
                        FieldReaderInstance<T> v = b[i];
                        if (v != null)
                        {
                            result |= v;
                        }
                    }
                }
                return result;
            }
            return default(T);
        }
        #endregion

        #region FieldReaderInstList_FieldReaderInstList
        public static FieldReaderInstList<T> operator &(FieldReaderInstList<T> a, FieldReaderInstList<T> b)
        {
            FieldReaderInstList<T> result = new FieldReaderInstList<T>();

            if (a == null && b == null) return result;

            a = a ?? new FieldReaderInstList<T>();
            b = b ?? new FieldReaderInstList<T>();

            HashSet<Type> UsedTypes = new HashSet<Type>();

            for (int i = 0; i < a.Count; i++)
            {
                FieldReaderInstance<T> child = new FieldReaderInstance<T>(a[i]);
                int index = result.FindIndex(x => x.UsedType == child.UsedType);
                if (index < 0)
                {
                    index = result.Count;
                    result.Add(child);
                }
                else result[index] &= child;
                UsedTypes.Add(child.UsedType);
            }
            a = result;
            result = new FieldReaderInstList<T>();

            for (int i = 0; i < b.Count; i++)
            {
                FieldReaderInstance<T> child = new FieldReaderInstance<T>(b[i]);
                int index = result.FindIndex(x => x.UsedType == child.UsedType);
                if (index < 0)
                {
                    index = result.Count;
                    result.Add(child);
                }
                else result[index] &= child;
                UsedTypes.Add(child.UsedType);
            }
            b = result;
            result = new FieldReaderInstList<T>();

            foreach (Type type in UsedTypes)
            {
                FieldReaderInstance<T> va = a.Find(x => x.UsedType == type) ?? new FieldReaderInstance<T>();
                FieldReaderInstance<T> vb = b.Find(x => x.UsedType == type) ?? new FieldReaderInstance<T>();
                result.Add(va & vb);
            }
            return result;
        }

        public static FieldReaderInstList<T> operator |(FieldReaderInstList<T> a, FieldReaderInstList<T> b)
        {
            FieldReaderInstList<T> result = new FieldReaderInstList<T>();

            if (a == null && b == null) return result;

            a = a ?? new FieldReaderInstList<T>();
            b = b ?? new FieldReaderInstList<T>();

            HashSet<Type> UsedTypes = new HashSet<Type>();

            for (int i = 0; i < a.Count; i++)
            {
                FieldReaderInstance<T> child = new FieldReaderInstance<T>(a[i]);
                int index = result.FindIndex(x => x.UsedType == child.UsedType);
                if (index < 0)
                {
                    index = result.Count;
                    result.Add(child);
                }
                else result[index] |= child;
                UsedTypes.Add(child.UsedType);
            }
            a = result;
            result = new FieldReaderInstList<T>();

            for (int i = 0; i < b.Count; i++)
            {
                FieldReaderInstance<T> child = new FieldReaderInstance<T>(b[i]);
                int index = result.FindIndex(x => x.UsedType == child.UsedType);
                if (index < 0)
                {
                    index = result.Count;
                    result.Add(child);
                }
                else result[index] |= child;
                UsedTypes.Add(child.UsedType);
            }
            b = result;
            result = new FieldReaderInstList<T>();

            foreach (Type type in UsedTypes)
            {
                FieldReaderInstance<T> va = a.Find(x => x.UsedType == type) ?? new FieldReaderInstance<T>();
                FieldReaderInstance<T> vb = b.Find(x => x.UsedType == type) ?? new FieldReaderInstance<T>();
                result.Add(va | vb);
            }
            return result;
        }
        #endregion
    }

    public class FieldReaderFiltList<T> : List<FieldReaderFilt<T>>
    {
        private bool? defaultValue;

        public FieldReaderFiltList() : base() { }

        public FieldReaderFiltList(FieldReaderFiltList<T> other) : base(other)
        {
            defaultValue = other.defaultValue;
        }

        public bool DefaultValue
        {
            get => defaultValue ?? false;
            set => defaultValue = value;
        }

        public bool HasDefaultValue => defaultValue.HasValue;

        /// <summary>
        /// Xml Usage Example
        /// ```xml
        ///<xx Default="default_value">
        ///    <li Reader-Class="c# type1" Default="default_value1">
        ///        <member_name_of_type>value</member_name_of_type>
        ///    </li>
        ///    <li Reader-Class="c# type2" Default="default_value2">
        ///        <member_name_of_type>value</member_name_of_type>
        ///    </li>
        ///</xx>
        /// ```
        /// </summary>
        /// <param name="xmlRoot"></param>
        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {

            try
            {
                string defaultValue = xmlRoot.Attributes["Default"]?.Value;
                if (defaultValue != null) this.defaultValue = ParseHelper.FromString<bool>(defaultValue);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
            try
            {
                AddRange((List<FieldReaderFilt<T>>)DirectXmlToObject.GetObjectFromXmlMethod(typeof(List<FieldReaderFilt<T>>))(xmlRoot, true));
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }


        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"defaultValue : {defaultValue}");
            for (int i = 0; i < this.Count; i++)
            {
                FieldReaderFilt<T> field = this[i];
                builder.AppendLine($"{i} : {field}");
            }
            return builder.ToString();
        }


        #region FieldReaderFiltList_bool
        public static FieldReaderFiltList<T> operator !(FieldReaderFiltList<T> a)
        {
            FieldReaderFiltList<T> list = new FieldReaderFiltList<T>(a);
            list.RemoveAll(x => x == null);
            for (int i = 0; i < list.Count; i++)
            {
                list[i] = !list[i];
            }
            return list;
        }

        public static FieldReaderFiltList<T> operator &(FieldReaderFiltList<T> a, bool b)
        {
            FieldReaderFiltList<T> list = new FieldReaderFiltList<T>(a);
            list.RemoveAll(x => x == null);
            for (int i = 0; i < list.Count; i++)
            {
                list[i] &= b;
            }
            return list;
        }

        public static FieldReaderFiltList<T> operator |(FieldReaderFiltList<T> a, bool b)
        {
            FieldReaderFiltList<T> list = new FieldReaderFiltList<T>(a);
            list.RemoveAll(x => x == null);
            for (int i = 0; i < list.Count; i++)
            {
                list[i] |= b;
            }
            return list;
        }
        #endregion

        #region FieldReaderFiltList_FieldReaderFiltList
        public static FieldReaderFiltList<T> operator &(FieldReaderFiltList<T> a, FieldReaderFiltList<T> b)
        {
            FieldReaderFiltList<T> result = new FieldReaderFiltList<T>();

            if (a == null && b == null) return result;

            a = a ?? new FieldReaderFiltList<T>();
            b = b ?? new FieldReaderFiltList<T>();

            HashSet<Type> UsedTypes = new HashSet<Type>();

            if (a.HasDefaultValue) result.DefaultValue = a.DefaultValue;
            for (int i = 0; i < a.Count; i++)
            {
                FieldReaderFilt<T> child = new FieldReaderFilt<T>(a[i]);
                if (result.HasDefaultValue && !child.HasDefaultValue) child.DefaultValue = result.DefaultValue;
                int index = result.FindIndex(x => x.UsedType == child.UsedType);
                if (index < 0)
                {
                    index = result.Count;
                    result.Add(child);
                }
                else result[index] &= child;
                if (result.HasDefaultValue && !result[index].HasDefaultValue) result[index].DefaultValue = result.DefaultValue;
                UsedTypes.Add(child.UsedType);
            }
            a = result;
            result = new FieldReaderFiltList<T>();

            if (b.HasDefaultValue) result.DefaultValue = b.DefaultValue;
            for (int i = 0; i < b.Count; i++)
            {
                FieldReaderFilt<T> child = new FieldReaderFilt<T>(b[i]);
                if (result.HasDefaultValue && !child.HasDefaultValue) child.DefaultValue = result.DefaultValue;
                int index = result.FindIndex(x => x.UsedType == child.UsedType);
                if (index < 0)
                {
                    index = result.Count;
                    result.Add(child);
                }
                else result[index] &= child;
                if (result.HasDefaultValue && !result[index].HasDefaultValue) result[index].DefaultValue = result.DefaultValue;
                UsedTypes.Add(child.UsedType);
            }
            b = result;
            result = new FieldReaderFiltList<T>();

            foreach (Type type in UsedTypes)
            {
                FieldReaderFilt<T> va = a.Find(x => x.UsedType == type) ?? new FieldReaderFilt<T>();
                FieldReaderFilt<T> vb = b.Find(x => x.UsedType == type) ?? new FieldReaderFilt<T>();
                if (a.HasDefaultValue && !va.HasDefaultValue) va.DefaultValue = a.DefaultValue;
                if (b.HasDefaultValue && !vb.HasDefaultValue) vb.DefaultValue = b.DefaultValue;
                result.Add(va & vb);
            }
            return result;
        }

        public static FieldReaderFiltList<T> operator |(FieldReaderFiltList<T> a, FieldReaderFiltList<T> b)
        {
            FieldReaderFiltList<T> result = new FieldReaderFiltList<T>();

            if (a == null && b == null) return result;

            a = a ?? new FieldReaderFiltList<T>();
            b = b ?? new FieldReaderFiltList<T>();

            HashSet<Type> UsedTypes = new HashSet<Type>();

            if (a.HasDefaultValue) result.DefaultValue = a.DefaultValue;
            for (int i = 0; i < a.Count; i++)
            {
                FieldReaderFilt<T> child = new FieldReaderFilt<T>(a[i]);
                if (result.HasDefaultValue && !child.HasDefaultValue) child.DefaultValue = result.DefaultValue;
                int index = result.FindIndex(x => x.UsedType == child.UsedType);
                if (index < 0)
                {
                    index = result.Count;
                    result.Add(child);
                }
                else result[index] |= child;
                if (result.HasDefaultValue && !result[index].HasDefaultValue) result[index].DefaultValue = result.DefaultValue;
                UsedTypes.Add(child.UsedType);
            }
            a = result;
            result = new FieldReaderFiltList<T>();

            if (b.HasDefaultValue) result.DefaultValue = b.DefaultValue;
            for (int i = 0; i < b.Count; i++)
            {
                FieldReaderFilt<T> child = new FieldReaderFilt<T>(b[i]);
                if (result.HasDefaultValue && !child.HasDefaultValue) child.DefaultValue = result.DefaultValue;
                int index = result.FindIndex(x => x.UsedType == child.UsedType);
                if (index < 0)
                {
                    index = result.Count;
                    result.Add(child);
                }
                else result[index] |= child;
                if (result.HasDefaultValue && !result[index].HasDefaultValue) result[index].DefaultValue = result.DefaultValue;
                UsedTypes.Add(child.UsedType);
            }
            b = result;
            result = new FieldReaderFiltList<T>();

            foreach (Type type in UsedTypes)
            {
                FieldReaderFilt<T> va = a.Find(x => x.UsedType == type) ?? new FieldReaderFilt<T>();
                FieldReaderFilt<T> vb = b.Find(x => x.UsedType == type) ?? new FieldReaderFilt<T>();
                if (a.HasDefaultValue && !va.HasDefaultValue) va.DefaultValue = a.DefaultValue;
                if (b.HasDefaultValue && !vb.HasDefaultValue) vb.DefaultValue = b.DefaultValue;
                result.Add(va | vb);
            }
            return result;
        }

        public static FieldReaderDgitList<T> operator &(FieldReaderDgitList<T> a, FieldReaderFiltList<T> b)
        {
            FieldReaderDgitList<T> result = new FieldReaderDgitList<T>();

            if (a != null)
            {
                result = new FieldReaderDgitList<T>(a);
                b = b ?? new FieldReaderFiltList<T>();

                if(b.Count > 0)
                {
                    foreach (FieldReaderFilt<T> filt in b)
                    {
                        FieldReaderFilt<T> copy = new FieldReaderFilt<T>(filt);
                        if (!copy.HasDefaultValue && b.HasDefaultValue) copy.DefaultValue = b.DefaultValue;
                        for (int i = 0; i < result.Count; i++)
                        {
                            result[i] &= copy;
                        }
                    }
                }
                else if(!b.DefaultValue)
                {
                    result.Clear();
                }
            }
            return result;
        }

        public static FieldReaderBoolList<T> operator &(FieldReaderBoolList<T> a, FieldReaderFiltList<T> b)
        {
            FieldReaderBoolList<T> result = new FieldReaderBoolList<T>();

            if (a != null)
            {
                result = new FieldReaderBoolList<T>(a);
                b = b ?? new FieldReaderFiltList<T>();

                if (b.Count > 0)
                {
                    foreach (FieldReaderFilt<T> filt in b)
                    {
                        FieldReaderFilt<T> copy = new FieldReaderFilt<T>(filt);
                        if (!copy.HasDefaultValue && b.HasDefaultValue) copy.DefaultValue = b.DefaultValue;
                        for (int i = 0; i < result.Count; i++)
                        {
                            result[i] &= copy;
                        }
                    }
                }
                else if (!b.DefaultValue)
                {
                    result.Clear();
                }
            }
            return result;
        }

        public static FieldReaderInstList<T> operator &(FieldReaderInstList<T> a, FieldReaderFiltList<T> b)
        {
            FieldReaderInstList<T> result = new FieldReaderInstList<T>();

            if (a != null)
            {
                result = new FieldReaderInstList<T>(a);
                b = b ?? new FieldReaderFiltList<T>();

                if (b.Count > 0)
                {
                    foreach (FieldReaderFilt<T> filt in b)
                    {
                        FieldReaderFilt<T> copy = new FieldReaderFilt<T>(filt);
                        if (!copy.HasDefaultValue && b.HasDefaultValue) copy.DefaultValue = b.DefaultValue;
                        for (int i = 0; i < result.Count; i++)
                        {
                            result[i] &= copy;
                        }
                    }
                }
                else if (!b.DefaultValue)
                {
                    result.Clear();
                }
            }
            return result;
        }
        #endregion
    }
}
