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

        public bool HasDefaultValue => defaultValue != null;

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


        public static T operator +(T a, FieldReaderDgitList<T> b)
        {
            if(a != null)
            {
                T result = Gen.MemberwiseClone(a);
                if(b != null)
                {
                    foreach(var v in b)
                    {
                        if(v != null) result += v;
                    }
                }
                return result;
            }
            return default(T);
        }

        public static FieldReaderDgitList<T> operator +(FieldReaderDgitList<T> a, FieldReaderDgitList<T> b)
        {
            FieldReaderDgitList<T> result = new FieldReaderDgitList<T>();

            if (a == null && b == null) return result;

            a = a ?? new FieldReaderDgitList<T>();
            b = b ?? new FieldReaderDgitList<T>();


            foreach (FieldReaderDgit<T> child in a)
            {
                int index = result.FindIndex(x => x.UsedType == child.UsedType);
                if (index < 0) result.Add(child);
                else result[index] += child;
            }
            foreach (FieldReaderDgit<T> child in b)
            {
                int index = result.FindIndex(x => x.UsedType == child.UsedType);
                if (index < 0) result.Add(child + a.DefaultValue);
                else result[index] += child;
            }
            return result;
        }
    }
}
