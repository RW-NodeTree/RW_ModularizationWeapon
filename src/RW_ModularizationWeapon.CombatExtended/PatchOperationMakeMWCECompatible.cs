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

        private bool GetOrCreateNode(XmlNode xmlNode, string name, out XmlElement? output)
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
                output = comps_nodes[0] as XmlElement;
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
        }
        
        public override bool ApplyWorker(XmlDocument xml)
        {
            XmlContainer? ammoUser = AmmoUser;
            XmlContainer? fireModes = FireModes;
            AmmoUser = null;
            FireModes = null;
            bool result = base.ApplyWorker(xml);
            foreach (XmlNode current in xml.SelectNodes("Defs/ThingDef[defName=\"" + defName + "\"]/modExtensions/li[@Class=\"RW_ModularizationWeapon.ModularizationWeaponExtension\"]"))
            {
                GetOrCreateNode(current, "protectedCompProperties", out XmlElement? comps);
                if (comps != null)
                {

                    // add CompProperties_AmmoUser
                    if (ammoUser != null)
                    {
                        AppendAllChild(comps, ammoUser.node, "li", "CombatExtended.CompProperties_AmmoUser");
                    }

                    // add CompProperties_FireModes
                    if (fireModes != null)
                    {
                        AppendAllChild(comps, fireModes.node, "li", "CombatExtended.CompProperties_FireModes");
                    }
                }
            }
            AmmoUser = ammoUser;
            FireModes = fireModes;
            return result;
        }
    }
}
