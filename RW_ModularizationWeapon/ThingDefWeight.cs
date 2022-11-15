using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Verse;

namespace RW_ModularizationWeapon
{
    public class ThingDefWeight
    {
        public ThingDef thingDef;
        public float weight;
        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "thingDef", xmlRoot.Name);
            this.weight = ParseHelper.FromString<float>(xmlRoot.FirstChild.Value);
        }
    }
}
