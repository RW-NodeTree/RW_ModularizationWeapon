using CombatExtended;
using HarmonyLib;
using System;
using System.Reflection;
using System.Xml;
using Verse;

namespace RW_ModularizationWeapon.CombatExtended
{
    public class PatchOperationMakeMWCECompatible : PatchOperationMakeGunCECompatible
    {


        private bool GetOrCreateNode(XmlNode xmlNode, string name, out XmlElement output)
        {
            var comps_nodes = xmlNode.SelectNodes(name);
            if (comps_nodes.Count == 0)
            {
                output = xmlNode.OwnerDocument.CreateElement(name);
                xmlNode.AppendChild(output);
                return false;
            }
            else
            {
                output = (XmlElement)comps_nodes[0];
                return true;
            }
        }

        private void AppendAllChild(XmlNode target, XmlNode xmlNode, string name, string className)
        {
            XmlElement element = target.OwnerDocument.CreateElement(name);
            element.SetAttribute("Class", className);
            foreach (XmlNode current in xmlNode.ChildNodes)
            {
                element.AppendChild(element.OwnerDocument.ImportNode(current, true));
            }
            target.AppendChild(element);
        }
        
        public override bool ApplyWorker(XmlDocument xml)
        {
            XmlContainer? ammoUser = AmmoUser;
            XmlContainer? fireModes = FireModes;
            XmlContainer? properties = Properties;
            AmmoUser = null;
            FireModes = null;
            Properties = null;
            bool result = base.ApplyWorker(xml);
            foreach (XmlNode current in xml.SelectNodes("Defs/ThingDef[defName=\"" + defName + "\"]/modExtensions/li[@Class=\"RW_ModularizationWeapon.ModularizationWeaponDefExtension\"]"))
            {
                GetOrCreateNode(current, "equippableModeInfos", out XmlElement infos);
                GetOrCreateNode(infos, "li", out infos);
                GetOrCreateNode(infos, "inheritableCompProperties", out XmlElement props);
                // add CompProperties_AmmoUser
                if (ammoUser != null)
                {
                    AppendAllChild(props, ammoUser.node, "li", "CombatExtended.CompProperties_AmmoUser");
                    XmlElement element = xml.CreateElement("li");
                    element.SetAttribute("Class", "RW_ModularizationWeapon.CombatExtended.CompAmmoUserDestructor");
                    GetOrCreateNode(current, "thingCompDestructors", out XmlElement destructors);
                    destructors.AppendChild(element);
                }

                // add CompProperties_FireModes
                if (fireModes != null)
                {
                    AppendAllChild(props, fireModes.node, "li", "CombatExtended.CompProperties_FireModes");
                }


                if (properties != null)
                {
                    GetOrCreateNode(infos, "inheritableVerbProperties", out props);
                    
                    XmlNodeList xmlNodeList = props.SelectNodes("li[verbClass=\"Verb_Shoot\" or verbClass=\"Verb_ShootOneUse\" or verbClass=\"Verb_LaunchProjectile\"]");
                    foreach (object item in xmlNodeList)
                    {
                        if (item is XmlNode oldChild)
                        {
                            props.RemoveChild(oldChild);
                        }
                    }
                    AppendAllChild(props, properties.node, "li", "CombatExtended.VerbPropertiesCE");
                }
            }
            AmmoUser = ammoUser;
            FireModes = fireModes;
            Properties = properties;
            return result;
        }
    }
}
