using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Verse;

namespace RW_ModularizationWeapon
{
    public class FieldReaderDgitList<T> : List<FieldReaderDgit<T>>
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
                AddRange((List<FieldReaderDgit<T>>)DirectXmlToObject.GetObjectFromXmlMethod(typeof(List<FieldReaderDgit<T>>))(xmlRoot, true));
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
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
                        FieldReaderDgit<T> v = b[i];
                        if (v != null)
                        {
                            if(!v.HasDefaultValue)
                            {
                                v = new FieldReaderDgit<T>(v);
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
                        FieldReaderDgit<T> v = b[i];
                        if (v != null)
                        {
                            if (!v.HasDefaultValue)
                            {
                                v = new FieldReaderDgit<T>(v);
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
                        FieldReaderDgit<T> v = b[i];
                        if (v != null)
                        {
                            if (!v.HasDefaultValue)
                            {
                                v = new FieldReaderDgit<T>(v);
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
                        FieldReaderDgit<T> v = b[i];
                        if (v != null)
                        {
                            if (!v.HasDefaultValue)
                            {
                                v = new FieldReaderDgit<T>(v);
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
                        FieldReaderDgit<T> v = b[i];
                        if (v != null)
                        {
                            if (!v.HasDefaultValue)
                            {
                                v = new FieldReaderDgit<T>(v);
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


            foreach (FieldReaderDgit<T> child in a)
            {
                int index = result.FindIndex(x => x.UsedType == child.UsedType);
                FieldReaderDgit<T> value =
                    b.Find(
                        x => x.UsedType == child.UsedType
                    );
                if (value == null)
                {
                    value = new FieldReaderDgit<T>();
                    value.UsedType = child.UsedType;
                    if (b.HasDefaultValue) value.DefaultValue = b.DefaultValue;
                }
                else if (!value.HasDefaultValue)
                {
                    value = new FieldReaderDgit<T>(value);
                    value.DefaultValue = b.DefaultValue;
                }
                if (index < 0) result.Add(child + value);
                else result[index] += child + value;
            }

            foreach (FieldReaderDgit<T> child in b)
            {
                if(a.Find(x => x.UsedType == child.UsedType) == null)
                {
                    int index = result.FindIndex(x => x.UsedType == child.UsedType);
                    FieldReaderDgit<T> value = new FieldReaderDgit<T>();
                    value.UsedType = child.UsedType;
                    if (a.HasDefaultValue) value.DefaultValue = a.DefaultValue;
                    if (index < 0) result.Add(value + child);
                    else result[index] += value + child;
                }
            }
            return result;
        }

        public static FieldReaderDgitList<T> operator -(FieldReaderDgitList<T> a, FieldReaderDgitList<T> b)
        {
            FieldReaderDgitList<T> result = new FieldReaderDgitList<T>();

            if (a == null && b == null) return result;

            a = a ?? new FieldReaderDgitList<T>();
            b = b ?? new FieldReaderDgitList<T>();


            foreach (FieldReaderDgit<T> child in a)
            {
                int index = result.FindIndex(x => x.UsedType == child.UsedType);
                FieldReaderDgit<T> value =
                    b.Find(
                        x => x.UsedType == child.UsedType
                    );
                if (value == null)
                {
                    value = new FieldReaderDgit<T>();
                    value.UsedType = child.UsedType;
                    if (b.HasDefaultValue) value.DefaultValue = b.DefaultValue;
                    b.Add(value);
                }
                else if (!value.HasDefaultValue)
                {
                    value = new FieldReaderDgit<T>(value);
                    value.DefaultValue = b.DefaultValue;
                }
                if (index < 0) result.Add(child - value);
                else result[index] -= child - value;
            }

            foreach (FieldReaderDgit<T> child in b)
            {
                if (a.Find(x => x.UsedType == child.UsedType) == null)
                {
                    int index = result.FindIndex(x => x.UsedType == child.UsedType);
                    FieldReaderDgit<T> value = new FieldReaderDgit<T>();
                    value.UsedType = child.UsedType;
                    if (a.HasDefaultValue) value.DefaultValue = a.DefaultValue;
                    a.Add(value);
                    if (index < 0) result.Add(value - child);
                    else result[index] -= value - child;
                }
            }
            return result;
        }


        public static FieldReaderDgitList<T> operator *(FieldReaderDgitList<T> a, FieldReaderDgitList<T> b)
        {
            FieldReaderDgitList<T> result = new FieldReaderDgitList<T>();

            if (a == null && b == null) return result;

            a = a ?? new FieldReaderDgitList<T>();
            b = b ?? new FieldReaderDgitList<T>();


            foreach (FieldReaderDgit<T> child in a)
            {
                int index = result.FindIndex(x => x.UsedType == child.UsedType);
                FieldReaderDgit<T> value =
                    b.Find(
                        x => x.UsedType == child.UsedType
                    );
                if (value == null)
                {
                    value = new FieldReaderDgit<T>();
                    value.UsedType = child.UsedType;
                    if (b.HasDefaultValue) value.DefaultValue = b.DefaultValue;
                    b.Add(value);
                }
                else if (!value.HasDefaultValue)
                {
                    value = new FieldReaderDgit<T>(value);
                    value.DefaultValue = b.DefaultValue;
                }
                if (index < 0) result.Add(child * value);
                else result[index] *= child * value;
            }

            foreach (FieldReaderDgit<T> child in b)
            {
                if (a.Find(x => x.UsedType == child.UsedType) == null)
                {
                    int index = result.FindIndex(x => x.UsedType == child.UsedType);
                    FieldReaderDgit<T> value = new FieldReaderDgit<T>();
                    value.UsedType = child.UsedType;
                    if (a.HasDefaultValue) value.DefaultValue = a.DefaultValue;
                    a.Add(value);
                    if (index < 0) result.Add(value * child);
                    else result[index] *= value * child;
                }
            }
            return result;
        }
        

        public static FieldReaderDgitList<T> operator /(FieldReaderDgitList<T> a, FieldReaderDgitList<T> b)
        {
            FieldReaderDgitList<T> result = new FieldReaderDgitList<T>();

            if (a == null && b == null) return result;

            a = a ?? new FieldReaderDgitList<T>();
            b = b ?? new FieldReaderDgitList<T>();


            foreach (FieldReaderDgit<T> child in a)
            {
                int index = result.FindIndex(x => x.UsedType == child.UsedType);
                FieldReaderDgit<T> value =
                    b.Find(
                        x => x.UsedType == child.UsedType
                    );
                if (value == null)
                {
                    value = new FieldReaderDgit<T>();
                    value.UsedType = child.UsedType;
                    if (b.HasDefaultValue) value.DefaultValue = b.DefaultValue;
                    b.Add(value);
                }
                else if (!value.HasDefaultValue)
                {
                    value = new FieldReaderDgit<T>(value);
                    value.DefaultValue = b.DefaultValue;
                }
                if (index < 0) result.Add(child / value);
                else result[index] /= child / value;
            }

            foreach (FieldReaderDgit<T> child in b)
            {
                if (a.Find(x => x.UsedType == child.UsedType) == null)
                {
                    int index = result.FindIndex(x => x.UsedType == child.UsedType);
                    FieldReaderDgit<T> value = new FieldReaderDgit<T>();
                    value.UsedType = child.UsedType;
                    if (a.HasDefaultValue) value.DefaultValue = a.DefaultValue;
                    a.Add(value);
                    if (index < 0) result.Add(value / child);
                    else result[index] /= value / child;
                }
            }
            return result;
        }

        public static FieldReaderDgitList<T> operator %(FieldReaderDgitList<T> a, FieldReaderDgitList<T> b)
        {
            FieldReaderDgitList<T> result = new FieldReaderDgitList<T>();

            if (a == null && b == null) return result;

            a = a ?? new FieldReaderDgitList<T>();
            b = b ?? new FieldReaderDgitList<T>();


            foreach (FieldReaderDgit<T> child in a)
            {
                int index = result.FindIndex(x => x.UsedType == child.UsedType);
                FieldReaderDgit<T> value =
                    b.Find(
                        x => x.UsedType == child.UsedType
                    );
                if (value == null)
                {
                    value = new FieldReaderDgit<T>();
                    value.UsedType = child.UsedType;
                    if (b.HasDefaultValue) value.DefaultValue = b.DefaultValue;
                    b.Add(value);
                }
                else if (!value.HasDefaultValue)
                {
                    value = new FieldReaderDgit<T>(value);
                    value.DefaultValue = b.DefaultValue;
                }
                if (index < 0) result.Add(child % value);
                else result[index] %= child % value;
            }

            foreach (FieldReaderDgit<T> child in b)
            {
                if (a.Find(x => x.UsedType == child.UsedType) == null)
                {
                    int index = result.FindIndex(x => x.UsedType == child.UsedType);
                    FieldReaderDgit<T> value = new FieldReaderDgit<T>();
                    value.UsedType = child.UsedType;
                    if (a.HasDefaultValue) value.DefaultValue = a.DefaultValue;
                    a.Add(value);
                    if (index < 0) result.Add(value % child);
                    else result[index] %= value % child;
                }
            }
            return result;
        }
        #endregion
    }
    public class FieldReaderBoolList<T> : List<FieldReaderBool<T>>
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
                AddRange((List<FieldReaderBool<T>>)DirectXmlToObject.GetObjectFromXmlMethod(typeof(List<FieldReaderBool<T>>))(xmlRoot, true));
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

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

        #region FieldReaderBoolList_double
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
                        FieldReaderBool<T> v = b[i];
                        if (v != null)
                        {
                            if (!v.HasDefaultValue)
                            {
                                v = new FieldReaderBool<T>(v);
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
                        FieldReaderBool<T> v = b[i];
                        if (v != null)
                        {
                            if (!v.HasDefaultValue)
                            {
                                v = new FieldReaderBool<T>(v);
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

        #region FieldReaderDgitList_FieldReaderDgitList
        public static FieldReaderBoolList<T> operator &(FieldReaderBoolList<T> a, FieldReaderBoolList<T> b)
        {
            FieldReaderBoolList<T> result = new FieldReaderBoolList<T>();

            if (a == null && b == null) return result;

            a = a ?? new FieldReaderBoolList<T>();
            b = b ?? new FieldReaderBoolList<T>();


            foreach (FieldReaderBool<T> child in a)
            {
                int index = result.FindIndex(x => x.UsedType == child.UsedType);
                FieldReaderBool<T> value =
                    b.Find(
                        x => x.UsedType == child.UsedType
                    );
                if (value == null)
                {
                    value = new FieldReaderBool<T>();
                    value.UsedType = child.UsedType;
                    if (b.HasDefaultValue) value.DefaultValue = b.DefaultValue;
                }
                else if (!value.HasDefaultValue)
                {
                    value = new FieldReaderBool<T>(value);
                    value.DefaultValue = b.DefaultValue;
                }
                if (index < 0) result.Add(child & value);
                else result[index] &= child & value;
            }

            foreach (FieldReaderBool<T> child in b)
            {
                if (a.Find(x => x.UsedType == child.UsedType) == null)
                {
                    int index = result.FindIndex(x => x.UsedType == child.UsedType);
                    FieldReaderBool<T> value = new FieldReaderBool<T>();
                    value.UsedType = child.UsedType;
                    if (a.HasDefaultValue) value.DefaultValue = a.DefaultValue;
                    if (index < 0) result.Add(value & child);
                    else result[index] &= value & child;
                }
            }
            return result;
        }

        public static FieldReaderBoolList<T> operator |(FieldReaderBoolList<T> a, FieldReaderBoolList<T> b)
        {
            FieldReaderBoolList<T> result = new FieldReaderBoolList<T>();

            if (a == null && b == null) return result;

            a = a ?? new FieldReaderBoolList<T>();
            b = b ?? new FieldReaderBoolList<T>();


            foreach (FieldReaderBool<T> child in a)
            {
                int index = result.FindIndex(x => x.UsedType == child.UsedType);
                FieldReaderBool<T> value =
                    b.Find(
                        x => x.UsedType == child.UsedType
                    );
                if (value == null)
                {
                    value = new FieldReaderBool<T>();
                    value.UsedType = child.UsedType;
                    if (b.HasDefaultValue) value.DefaultValue = b.DefaultValue;
                }
                else if (!value.HasDefaultValue)
                {
                    value = new FieldReaderBool<T>(value);
                    value.DefaultValue = b.DefaultValue;
                }
                if (index < 0) result.Add(child | value);
                else result[index] |= child | value;
            }

            foreach (FieldReaderBool<T> child in b)
            {
                if (a.Find(x => x.UsedType == child.UsedType) == null)
                {
                    int index = result.FindIndex(x => x.UsedType == child.UsedType);
                    FieldReaderBool<T> value = new FieldReaderBool<T>();
                    value.UsedType = child.UsedType;
                    if (a.HasDefaultValue) value.DefaultValue = a.DefaultValue;
                    if (index < 0) result.Add(value | child);
                    else result[index] |= value | child;
                }
            }
            return result;
        }
        #endregion
    }
}
