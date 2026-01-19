using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using RimWorld;
using RW_ModularizationWeapon.Tools;
using RW_NodeTree.Rendering;
using RW_NodeTree.Tools;
using UnityEngine;
using Verse;


namespace RW_ModularizationWeapon
{
    /// <summary>
    /// this type is parmerters holder of the type `CompModularizationWeapon`, it define all parmerters that can write in XML.
    /// </summary>
    public class ModularizationWeaponDefExtension : DefModExtension
    {

        /// <summary>
        /// Texture of `PartTexMaterial`
        /// </summary>
        public Texture2D PartTexture
        {
            get
            {
                if (partTexCache == null)
                {
                    if(!PartTexPath.NullOrEmpty())
                    {
                        partTexCache = ContentFinder<Texture2D>.Get(PartTexPath);
                    }
                    partTexCache ??= BaseContent.BadTex;
                }
                return partTexCache;
            }
        }

        /// <summary>
        /// Config checking
        /// </summary>
        /// <param name="parentDef"></param>
        /// <returns></returns>
        public override IEnumerable<string> ConfigErrors()
        {
            attachmentPropertiesWithQuery ??= new List<(QueryGroup,WeaponAttachmentProperties)>();
            attachmentPropertiesWithQuery.Clear();
            foreach(string error in base.ConfigErrors())
            {
                yield return error;
            }
            for (int i = attachmentProperties.Count - 1; i >= 0; i--)
            {
                WeaponAttachmentProperties properties = attachmentProperties[i];
                if (properties == null)
                {
                    attachmentProperties.RemoveAt(i);
                    yield return $"attachmentProperties[{i}] is null";
                    continue;
                }
                else if (!properties.id.IsVaildityKeyFormat())
                {
                    bool faild = false;
                    try
                    {
                        attachmentProperties.RemoveAt(i);
                        QueryGroup query = new QueryGroup(properties.id!);
                        attachmentPropertiesWithQuery!.Add((query,properties));
                    }
                    catch
                    {
                        faild = true;
                    }
                    if (faild)
                    {
                        yield return $"attachmentProperties[{i}].id is invaild key format : Not XML allowed node name";
                        continue;
                    }
                }
                for (int j = 0; j < i; j++)
                {
                    WeaponAttachmentProperties propertiesForCompare = attachmentProperties[j];
                    if(!(propertiesForCompare?.id).NullOrEmpty() && propertiesForCompare!.id == properties.id)
                    {
                        attachmentProperties.RemoveAt(i);
                        yield return $"attachmentProperties[{i}].id should be unique, but now repeat with attachmentProperties[{j}].id";
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// get attachment properties by attachment id
        /// </summary>
        /// <param name="id">attachment id</param>
        /// <returns>attachment properties</returns>
        public WeaponAttachmentProperties? WeaponAttachmentPropertiesById(string id)
        {
            if(!id.NullOrEmpty())
            {
                foreach (WeaponAttachmentProperties properties in attachmentProperties)
                {
                    if(properties.id == id) return properties;
                }
            }
            return null;
        }

        /// <summary>
        /// apply and preprocessing all properties of this comp
        /// </summary>
        /// <param name="parentDef">parent Def</param>
        public void ResolveReferences(ThingDef parentDef)
        {
            #if DEBUG
            Log.Message($"ModularizationWeaponExtension.ResolveReferences : {parentDef}");
            #endif

            foreach (WeaponAttachmentProperties properties in attachmentProperties)
            {
                properties.ResolveReferences();
            }
            foreach (WeaponAttachmentProperties properties in attachmentProperties)
            {
                properties.verbPropertiesOtherPartOffseterAffectHorizon.RemoveAll(x => WeaponAttachmentPropertiesById(x.Key) == null);
                properties.toolsOtherPartOffseterAffectHorizon.RemoveAll(x => WeaponAttachmentPropertiesById(x.Key) == null);
                properties.verbPropertiesOtherPartMultiplierAffectHorizon.RemoveAll(x => WeaponAttachmentPropertiesById(x.Key) == null);
                properties.toolsOtherPartMultiplierAffectHorizon.RemoveAll(x => WeaponAttachmentPropertiesById(x.Key) == null);
                properties.verbPropertiesBoolAndPatchByOtherPart.RemoveAll(x => WeaponAttachmentPropertiesById(x.Key) == null);
                properties.toolsBoolAndPatchByOtherPart.RemoveAll(x => WeaponAttachmentPropertiesById(x.Key) == null);
                properties.verbPropertiesBoolOrPatchByOtherPart.RemoveAll(x => WeaponAttachmentPropertiesById(x.Key) == null);
                properties.toolsBoolOrPatchByOtherPart.RemoveAll(x => WeaponAttachmentPropertiesById(x.Key) == null);
                properties.verbPropertiesObjectPatchByOtherPart.RemoveAll(x => WeaponAttachmentPropertiesById(x.Key) == null);
                properties.toolsObjectPatchByOtherPart.RemoveAll(x => WeaponAttachmentPropertiesById(x.Key) == null);
            }

            foreach ((QueryGroup, WeaponAttachmentProperties) properties in attachmentPropertiesWithQuery!)
            {
                properties.Item2.ResolveReferences();
            }
            foreach ((QueryGroup, WeaponAttachmentProperties) properties in attachmentPropertiesWithQuery)
            {
                properties.Item2.verbPropertiesOtherPartOffseterAffectHorizon.RemoveAll(x => WeaponAttachmentPropertiesById(x.Key) == null);
                properties.Item2.toolsOtherPartOffseterAffectHorizon.RemoveAll(x => WeaponAttachmentPropertiesById(x.Key) == null);
                properties.Item2.verbPropertiesOtherPartMultiplierAffectHorizon.RemoveAll(x => WeaponAttachmentPropertiesById(x.Key) == null);
                properties.Item2.toolsOtherPartMultiplierAffectHorizon.RemoveAll(x => WeaponAttachmentPropertiesById(x.Key) == null);
                properties.Item2.verbPropertiesBoolAndPatchByOtherPart.RemoveAll(x => WeaponAttachmentPropertiesById(x.Key) == null);
                properties.Item2.toolsBoolAndPatchByOtherPart.RemoveAll(x => WeaponAttachmentPropertiesById(x.Key) == null);
                properties.Item2.verbPropertiesBoolOrPatchByOtherPart.RemoveAll(x => WeaponAttachmentPropertiesById(x.Key) == null);
                properties.Item2.toolsBoolOrPatchByOtherPart.RemoveAll(x => WeaponAttachmentPropertiesById(x.Key) == null);
                properties.Item2.verbPropertiesObjectPatchByOtherPart.RemoveAll(x => WeaponAttachmentPropertiesById(x.Key) == null);
                properties.Item2.toolsObjectPatchByOtherPart.RemoveAll(x => WeaponAttachmentPropertiesById(x.Key) == null);
            }
            // foreach ((QueryGroup, WeaponAttachmentProperties) properties in attachmentPropertiesWithQuery)
            // {
            //     Log.Message($"{parentDef} : ({properties.Item1}, {properties.Item2})");
            // }
            if (attachmentProperties.Count > 0) parentDef.stackLimit = 1;


            #region innerMethod
            void CheckAndSetDgitList<T>(ref FieldReaderDgitList<T> list, float defaultValue)
            {
                list ??= new FieldReaderDgitList<T>();
                list.RemoveAll(f => f == null);
                if (!list.HasDefaultValue) list.DefaultValue = defaultValue;
            }

            void CheckAndSetBoolList<T>(ref FieldReaderBoolList<T> list, bool defaultValue)
            {
                list ??= new FieldReaderBoolList<T>();
                list.RemoveAll(f => f == null);
                if (!list.HasDefaultValue) list.DefaultValue = defaultValue;
            }

            void CheckAndSetFiltList<T>(ref FieldReaderFiltList<T> list, bool defaultValue)
            {
                list ??= [];
                list.RemoveAll(f => f == null);
                if (!list.HasDefaultValue) list.DefaultValue = defaultValue;
            }

            void CheckAndSetInstList<T>(ref FieldReaderInstList<T> list)
            {
                list ??= [];
                list.RemoveAll(f => f == null);
            }

            void CheckAndSetStatList(ref List<StatModifier> list)
            {
                list ??= [];
                list.RemoveAll(f => f == null);
            }
            #endregion

            CheckAndSetDgitList(ref verbPropertiesOffseter, 0);
            CheckAndSetDgitList(ref toolsOffseter, 0);
            CheckAndSetDgitList(ref compPropertiesOffseter, 0);
            CheckAndSetDgitList(ref verbPropertiesOtherPartOffseterAffectHorizon, 1);
            CheckAndSetDgitList(ref toolsOtherPartOffseterAffectHorizon, 1);
            CheckAndSetDgitList(ref compPropertiesOtherPartOffseterAffectHorizon, 1);

            CheckAndSetDgitList(ref verbPropertiesMultiplier, 1);
            CheckAndSetDgitList(ref toolsMultiplier, 1);
            CheckAndSetDgitList(ref compPropertiesMultiplier, 1);
            CheckAndSetDgitList(ref verbPropertiesOtherPartMultiplierAffectHorizon, 1);
            CheckAndSetDgitList(ref toolsOtherPartMultiplierAffectHorizon, 1);
            CheckAndSetDgitList(ref compPropertiesOtherPartMultiplierAffectHorizon, 1);

            CheckAndSetBoolList(ref verbPropertiesBoolAndPatch, true);
            CheckAndSetBoolList(ref toolsBoolAndPatch, true);
            CheckAndSetBoolList(ref compPropertiesBoolAndPatch, true);
            CheckAndSetBoolList(ref verbPropertiesBoolAndPatchByOtherPart, true);
            CheckAndSetBoolList(ref toolsBoolAndPatchByOtherPart, true);
            CheckAndSetBoolList(ref compPropertiesBoolAndPatchByOtherPart, true);

            CheckAndSetBoolList(ref verbPropertiesBoolOrPatch, false);
            CheckAndSetBoolList(ref toolsBoolOrPatch, false);
            CheckAndSetBoolList(ref compPropertiesBoolOrPatch, false);
            CheckAndSetBoolList(ref verbPropertiesBoolOrPatchByOtherPart, true);
            CheckAndSetBoolList(ref toolsBoolOrPatchByOtherPart, true);
            CheckAndSetBoolList(ref compPropertiesBoolOrPatchByOtherPart, true);

            CheckAndSetInstList(ref verbPropertiesObjectPatch);
            CheckAndSetInstList(ref toolsObjectPatch);
            CheckAndSetInstList(ref compPropertiesObjectPatch);
            CheckAndSetFiltList(ref verbPropertiesObjectPatchByOtherPart, true);
            CheckAndSetFiltList(ref toolsObjectPatchByOtherPart, true);
            CheckAndSetFiltList(ref compPropertiesObjectPatchByOtherPart, true);

            CheckAndSetStatList(ref statOffset);
            CheckAndSetStatList(ref statMultiplier);
            CheckAndSetStatList(ref statOtherPartOffseterAffectHorizon);
            CheckAndSetStatList(ref statOtherPartMultiplierAffectHorizon);

            parentDef.weaponTags ??= [];

            ModularizationWeapon.ThingDef_verbs(parentDef) ??= [];
            parentDef.tools ??= [];
            parentDef.comps ??= [];
            notAllowedCompTypes ??= [];
            
        }

        /// <summary>
        /// extra stat draw entry on info card
        /// </summary>
        /// <param name="req">request/param>
        /// <returns>extra stat draw entry</returns>
        public IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
        {
            ModularizationWeapon? comp = req.Thing as ModularizationWeapon;
            StringBuilder stringBuilder = new StringBuilder();

            int listAllDgit<T>(
                FieldReaderDgitList<T> list,
                string perfix,
                string postfix,
                bool snap = false
                )
            {
                int result = 0;
                for (int i = 0; i < list.Count; i++)
                {
                    if (snap) stringBuilder.Append("  ");
                    stringBuilder.AppendLine($"  NO.{i+1} :");
                    foreach (KeyValuePair<RuntimeFieldHandle, IConvertible?> data in list[i])
                    {
                        if (data.Value != null) continue;
                        FieldInfo field = FieldInfo.GetFieldFromHandle(data.Key);
                        if (snap) stringBuilder.Append("  ");
                        stringBuilder.AppendLine($"    {field.Name.Translate()} : {perfix}{data.Value}{postfix}");
                    }
                    result += list[i].Count;
                }
                return result;
            }

            int listAllInst<T>(
                List<FieldReaderInstance<T>> list,
                string perfix,
                string postfix,
                bool snap = false
                )
            {
                int result = 0;
                for (int i = 0; i < list.Count; i++)
                {
                    if (snap) stringBuilder.Append("  ");
                    stringBuilder.AppendLine($"  NO.{i + 1} :");
                    foreach (KeyValuePair<RuntimeFieldHandle, object?> data in list[i])
                    {
                        FieldInfo field = FieldInfo.GetFieldFromHandle(data.Key);
                        if (snap) stringBuilder.Append("  ");
                        stringBuilder.AppendLine($"    {field.Name.Translate()} : {perfix}{data.Value}{postfix}");
                    }
                    result += list[i].Count;
                }
                return result;
            }
            int count = statOffset.Count;
            stringBuilder.AppendLine("verbPropertiesOffseter".Translate().RawText + " :");
            FieldReaderDgitList<VerbProperties> VerbPropertiesOffseter = verbPropertiesOffseter;
            if (comp != null) VerbPropertiesOffseter += comp.VerbPropertiesOffseter(null);
            count += listAllDgit(VerbPropertiesOffseter, "+", "");

            stringBuilder.AppendLine("toolsOffseter".Translate().RawText + " :");
            FieldReaderDgitList<Tool> ToolsOffseter = toolsOffseter;
            if (comp != null) ToolsOffseter += comp.ToolsOffseter(null);
            count += listAllDgit(ToolsOffseter, "+", "");

            stringBuilder.AppendLine("compPropertiesOffseter".Translate().RawText + " :");
            FieldReaderDgitList<CompProperties> CompPropertiesOffseter = compPropertiesOffseter;
            if (comp != null) CompPropertiesOffseter += comp.CompPropertiesOffseter(null);
            count += listAllDgit(CompPropertiesOffseter, "+", "");

            stringBuilder.AppendLine("statOffseter".Translate().RawText + " :");
            foreach (StatModifier stat in statOffset)
            {
                float value = (comp?.GetStatOffset(stat.stat, null) ?? 0) + stat.value;
                string text = value < 0 ? "" : "+";
                stringBuilder.AppendLine($"  {stat.stat.LabelCap} : {text}{value}");
            }
            yield return new StatDrawEntry(
                StatCategoryDefOf.Weapon,
                "Offset".Translate(),
                count + " " + "Offseter".Translate(),
                stringBuilder.ToString(),
                1000
                );

            stringBuilder.Clear();
            count = statMultiplier.Count;
            stringBuilder.AppendLine("verbPropertiesMultiplier".Translate().RawText + " :");
            VerbPropertiesOffseter = verbPropertiesMultiplier;
            if (comp != null) VerbPropertiesOffseter *= comp.VerbPropertiesMultiplier(null);
            count += listAllDgit(VerbPropertiesOffseter, "x", "");

            stringBuilder.AppendLine("toolsMultiplier".Translate().RawText + " :");
            ToolsOffseter = toolsMultiplier;
            if (comp != null) ToolsOffseter *= comp.ToolsMultiplier(null);
            count += listAllDgit(ToolsOffseter, "x", "");

            stringBuilder.AppendLine("compPropertiesMultiplier".Translate().RawText + " :");
            CompPropertiesOffseter = compPropertiesMultiplier;
            if (comp != null) CompPropertiesOffseter *= comp.CompPropertiesMultiplier(null);
            count += listAllDgit(CompPropertiesOffseter, "x", "");

            stringBuilder.AppendLine("statMultiplier".Translate().RawText + " :");
            foreach (StatModifier stat in statMultiplier)
            {
                float value = (comp?.GetStatMultiplier(stat.stat, null) ?? 1) * stat.value;
                string text = value < 0 ? "" : " ";
                stringBuilder.AppendLine($"  {stat.stat.LabelCap} : x{text}{value}");
            }
            yield return new StatDrawEntry(
                StatCategoryDefOf.Weapon, 
                "Multiplier".Translate(),
                count + " " + "Multiplier".Translate(), 
                stringBuilder.ToString(), 
                1000
                );

            stringBuilder.Clear();
            count = 0;
            stringBuilder.AppendLine("verbPropertiesPatch".Translate().RawText + " :");
            List<FieldReaderInstance<VerbProperties>>? VerbPropertiesObjectPatch = comp?.VerbPropertiesObjectPatch(null);
            if(VerbPropertiesObjectPatch != null)
            {
                foreach(FieldReaderInstance<VerbProperties> fieldReader in verbPropertiesObjectPatch)
                {
                    int index = VerbPropertiesObjectPatch.FindIndex(x => x.UsedType == fieldReader.UsedType);
                    if (index < 0) VerbPropertiesObjectPatch.Add(fieldReader);
                    else VerbPropertiesObjectPatch[index] = (VerbPropertiesObjectPatch[index] | fieldReader) ?? VerbPropertiesObjectPatch[index];
                }
                count += listAllInst(VerbPropertiesObjectPatch, "", "");
            }

            stringBuilder.AppendLine("toolsPatch".Translate().RawText + " :");
            List<FieldReaderInstance<Tool>>? ToolsObjectPatch = comp?.ToolsObjectPatch(null);
            if (ToolsObjectPatch != null)
            {
                foreach (FieldReaderInstance<Tool> fieldReader in toolsObjectPatch)
                {
                    int index = ToolsObjectPatch.FindIndex(x => x.UsedType == fieldReader.UsedType);
                    if (index < 0) ToolsObjectPatch.Add(fieldReader);
                    else ToolsObjectPatch[index] = (ToolsObjectPatch[index] | fieldReader) ?? ToolsObjectPatch[index];
                }
                count += listAllInst(ToolsObjectPatch, "", "");
            }

            stringBuilder.AppendLine("compPropertiesPatch".Translate().RawText + " :");
            List<FieldReaderInstance<CompProperties>>? CompPropertiesObjectPatch = comp?.CompPropertiesObjectPatch(null);
            if(CompPropertiesObjectPatch != null)
            {
                foreach(FieldReaderInstance<CompProperties> fieldReader in compPropertiesObjectPatch)
                {
                    int index = CompPropertiesObjectPatch.FindIndex(x => x.UsedType == fieldReader.UsedType);
                    if (index < 0) CompPropertiesObjectPatch.Add(fieldReader);
                    else CompPropertiesObjectPatch[index] = (CompPropertiesObjectPatch[index] | fieldReader) ?? CompPropertiesObjectPatch[index];
                }
                count += listAllInst(CompPropertiesObjectPatch, "", "");
            }

            yield return new StatDrawEntry(
                StatCategoryDefOf.Weapon,
                "Patcher".Translate(),
                count + " " + "Patcher".Translate(),
                stringBuilder.ToString(),
                1000
                );

            stringBuilder.Clear();

            string CheckAndMark(bool flag, string name)
            {
                string result = "<color=" + (flag ? "#d9ead3><b>" : "grey>");
                result += name + " : " + (flag ? ("Yes".Translate().RawText + "</b>") : "No".Translate().RawText);
                return result += "</color>";
            }

            //UnityEngine.GUIUtility.systemCopyBuffer = "<color=" + (unchangeable ? "green" : "red") + ">" + "unchangeable".Translate() + " : " + (unchangeable ? "Yes".Translate() : "No".Translate()) + "</color>";
            //stringBuilder.AppendLine("<color=" + (unchangeable ? "green" : "red") + ">" + "unchangeable" + " : " + (unchangeable ? "Yes" : "No") + "</color>");
            stringBuilder.AppendLine(CheckAndMark(unchangeable, "unchangeable".Translate()));
            stringBuilder.AppendLine(CheckAndMark(notAllowParentUseTools, "notAllowParentUseTools".Translate()));
            stringBuilder.AppendLine(CheckAndMark(allowCreateOnCraftingPort, "allowCreateOnCraftingPort".Translate()));
            stringBuilder.AppendLine(CheckAndMark(notAllowParentUseVerbProperties, "notAllowParentUseVerbProperties".Translate()));
            //stringBuilder.AppendLine("<color=" + (notAllowParentUseTools ? "green" : "red") + ">" + "notAllowParentUseTools".Translate() + " : " + (notAllowParentUseTools ? "Yes".Translate() : "No".Translate()) + "</color>");
            //stringBuilder.AppendLine("<color=" + (notAllowParentUseVerbProperties ? "green" : "red") + ">" + "notAllowParentUseVerbProperties".Translate() + " : " + (notAllowParentUseVerbProperties ? "Yes".Translate() : "No".Translate()) + "</color>");
            //stringBuilder.AppendLine("useOriginalCraftMethod".Translate() + " : <color=" + (useOriginalCraftMethod ? "green" : "red") + ">" + (useOriginalCraftMethod ? "Yes".Translate() : "No".Translate()) + "</color>");
            //stringBuilder.AppendLine("<color=" + (verbPropertiesAffectByOtherPart ? "green" : "red") + ">" + "verbPropertiesAffectByOtherPart".Translate() + " : " + (verbPropertiesAffectByOtherPart ? "Yes".Translate() : "No".Translate()) + "</color>");
            //stringBuilder.AppendLine("<color=" + (toolsAffectByOtherPart ? "green" : "red") + ">" + "toolsAffectByOtherPart".Translate() + " : " + (toolsAffectByOtherPart ? "Yes".Translate() : "No".Translate()) + "</color>");
            //stringBuilder.AppendLine("<color=" + (verbPropertiesAffectByChildPart ? "green" : "red") + ">" + "verbPropertiesAffectByChildPart".Translate() + " : " + (verbPropertiesAffectByChildPart ? "Yes".Translate() : "No".Translate()) + "</color>");
            //stringBuilder.AppendLine("<color=" + (toolsAffectByChildPart ? "green" : "red") + ">" + "toolsAffectByChildPart".Translate() + " : " + (toolsAffectByChildPart ? "Yes".Translate() : "No".Translate()) + "</color>");
            yield return new StatDrawEntry(
                category: StatCategoryDefOf.Weapon,
                "Condation".Translate(),
                "",
                stringBuilder.ToString(),
                1000
                );
            //UnityEngine.GUIUtility.systemCopyBuffer = stringBuilder.ToString();
            #region child
            foreach (WeaponAttachmentProperties properties in comp?.GetOrGenCurrentPartAttachmentProperties().Values.ToList() ?? attachmentProperties)
            {

                Thing? child = properties.id != null ? comp?.ChildNodes?[properties.id] : null;
                ModularizationWeapon? childComp = child as ModularizationWeapon;
                stringBuilder.Clear();

                count = 0;
                int Offseter = 0;
                int Multiplier = 0;
                if (childComp != null)
                {
                    Offseter = childComp.Props.statOffset.Count;
                    Multiplier = childComp.Props.statMultiplier.Count;

                    stringBuilder.AppendLine("Offseter".Translate() + " :");
                    stringBuilder.AppendLine("  " + "verbPropertiesOffseter".Translate() + " :");
                    listAllDgit((childComp.VerbPropertiesOffseter(null) + childComp.Props.verbPropertiesOffseter) * properties.verbPropertiesOffseterAffectHorizon, "+", "", true);

                    stringBuilder.AppendLine("  " + "toolsOffseter".Translate() + " :");
                    listAllDgit((childComp.ToolsOffseter(null) + childComp.Props.toolsOffseter) * properties.toolsOffseterAffectHorizon, "+", "", true);

                    stringBuilder.AppendLine("  " + "compPropertiesOffseter".Translate() + " :");
                    listAllDgit((childComp.CompPropertiesOffseter(null) + childComp.Props.compPropertiesOffseter) * properties.compPropertiesOffseterAffectHorizon, "+", "", true);

                    stringBuilder.AppendLine("  " + "statOffseter".Translate() + " :");
                    foreach (StatModifier stat in childComp.Props.statOffset)
                    {
                        float value = properties.statOffsetAffectHorizon.GetStatValueFromList(stat.stat, properties.statOffsetAffectHorizonDefaultValue) * (childComp.GetStatOffset(stat.stat, null) + stat.value);
                        string text = value < 0 ? "" : "+";
                        stringBuilder.AppendLine($"    {stat.stat.LabelCap} : {text}{value}");
                    }


                    stringBuilder.AppendLine("Multiplier".Translate() + " :");
                    stringBuilder.AppendLine("  " + "verbPropertiesMultiplier".Translate() + " :");
                    FieldReaderDgitList<VerbProperties> cacheVerbProperties = childComp.VerbPropertiesMultiplier(null);
                    cacheVerbProperties *= childComp.Props.verbPropertiesMultiplier;
                    cacheVerbProperties -= 1;
                    cacheVerbProperties.DefaultValue = 0;
                    listAllDgit((cacheVerbProperties * properties.verbPropertiesMultiplierAffectHorizon + 1) ?? cacheVerbProperties, "x", "", true);

                    stringBuilder.AppendLine("  " + "toolsMultiplier".Translate() + " :");
                    FieldReaderDgitList<Tool> cacheTools = childComp.ToolsMultiplier(null);
                    cacheTools *= childComp.Props.toolsMultiplier;
                    cacheTools -= 1;
                    cacheTools.DefaultValue = 0;
                    listAllDgit((cacheTools * properties.toolsMultiplierAffectHorizon + 1) ?? cacheTools, "x", "", true);

                    stringBuilder.AppendLine("  " + "compPropertiesMultiplier".Translate() + " :");
                    FieldReaderDgitList<CompProperties> cacheCompProperties = childComp.CompPropertiesMultiplier(null);
                    cacheCompProperties *= childComp.Props.compPropertiesMultiplier;
                    cacheCompProperties -= 1;
                    cacheCompProperties.DefaultValue = 0;
                    listAllDgit((cacheCompProperties * properties.compPropertiesMultiplierAffectHorizon + 1) ?? cacheCompProperties, "x", "", true);

                    stringBuilder.AppendLine("  " + "statMultiplier".Translate() + " :");
                    foreach (StatModifier stat in childComp.Props.statMultiplier)
                    {
                        float value = properties.statMultiplierAffectHorizon.GetStatValueFromList(stat.stat, properties.statMultiplierAffectHorizonDefaultValue) * (childComp.GetStatMultiplier(stat.stat, null) * stat.value - 1f) + 1f;
                        string text = value < 0 ? "" : " ";
                        stringBuilder.AppendLine($"    {stat.stat.LabelCap} : x{text}{value}");
                    }

                    stringBuilder.AppendLine("verbPropertiesPatch".Translate().RawText + " :");
                    VerbPropertiesObjectPatch = childComp.VerbPropertiesObjectPatch(null);
                    if (comp != null)
                    {
                        foreach (FieldReaderInstance<VerbProperties> fieldReader in childComp.Props.verbPropertiesObjectPatch)
                        {
                            int index = VerbPropertiesObjectPatch.FindIndex(x => x.UsedType == fieldReader.UsedType);
                            if (index < 0) VerbPropertiesObjectPatch.Add(fieldReader);
                            else VerbPropertiesObjectPatch[index] = (VerbPropertiesObjectPatch[index] | fieldReader) ?? VerbPropertiesObjectPatch[index];
                        }
                    }
                    listAllInst(VerbPropertiesObjectPatch, "", "", true);

                    stringBuilder.AppendLine("toolsPatch".Translate().RawText + " :");
                    ToolsObjectPatch = childComp.ToolsObjectPatch(null);
                    if (comp != null)
                    {
                        foreach (FieldReaderInstance<Tool> fieldReader in childComp.Props.toolsObjectPatch)
                        {
                            int index = ToolsObjectPatch.FindIndex(x => x.UsedType == fieldReader.UsedType);
                            if (index < 0) ToolsObjectPatch.Add(fieldReader);
                            else ToolsObjectPatch[index] = (ToolsObjectPatch[index] | fieldReader) ?? ToolsObjectPatch[index];
                        }
                    }
                    listAllInst(ToolsObjectPatch, "", "", true);

                    stringBuilder.AppendLine("compPropertiesPatch".Translate().RawText + " :");
                    CompPropertiesObjectPatch = childComp.CompPropertiesObjectPatch(null);
                    if (comp != null)
                    {
                        foreach (FieldReaderInstance<CompProperties> fieldReader in childComp.Props.compPropertiesObjectPatch)
                        {
                            int index = CompPropertiesObjectPatch.FindIndex(x => x.UsedType == fieldReader.UsedType);
                            if (index < 0) CompPropertiesObjectPatch.Add(fieldReader);
                            else CompPropertiesObjectPatch[index] = (CompPropertiesObjectPatch[index] | fieldReader) ?? CompPropertiesObjectPatch[index];
                        }
                    }
                    listAllInst(CompPropertiesObjectPatch, "", "", true);
                }
                else
                {
                    Offseter = properties.statOffsetAffectHorizon.Count;
                    Multiplier = properties.statMultiplierAffectHorizon.Count;

                    stringBuilder.AppendLine("OffseterAffectHorizon".Translate() + " :");
                    stringBuilder.AppendLine("  " + "verbPropertiesOffseterAffectHorizon".Translate() + " :");
                    Offseter += listAllDgit(properties.verbPropertiesOffseterAffectHorizon, "x", "", true);

                    stringBuilder.AppendLine("  " + "toolsOffseterAffectHorizon".Translate() + " :");
                    Offseter += listAllDgit(properties.toolsOffseterAffectHorizon, "x", "", true);

                    stringBuilder.AppendLine("  " + "compPropertiesOffseterAffectHorizon".Translate() + " :");
                    Offseter += listAllDgit(properties.compPropertiesOffseterAffectHorizon, "x", "", true);

                    stringBuilder.AppendLine("  " + "statOffsetAffectHorizon".Translate() + " :");
                    foreach (StatModifier stat in properties.statOffsetAffectHorizon)
                    {
                        stringBuilder.AppendLine($"    {stat.stat.LabelCap} : x{stat.value}");
                    }

                    stringBuilder.AppendLine("MultiplierAffectHorizon".Translate() + " :");
                    stringBuilder.AppendLine("  " + "verbPropertiesMultiplierAffectHorizon".Translate() + " :");
                    Multiplier += listAllDgit(properties.verbPropertiesMultiplierAffectHorizon, "(k-1)x", "+1", true);

                    stringBuilder.AppendLine("  " + "toolsMultiplierAffectHorizon".Translate() + " :");
                    Multiplier += listAllDgit(properties.toolsMultiplierAffectHorizon, "(k-1)x", "+1", true);

                    stringBuilder.AppendLine("  " + "compPropertiesMultiplierAffectHorizon".Translate() + " :");
                    Multiplier += listAllDgit(properties.compPropertiesMultiplierAffectHorizon, "(k-1)x", "+1", true);

                    stringBuilder.AppendLine("  " + "statMultiplierAffectHorizon".Translate() + " :");
                    foreach (StatModifier stat in properties.statMultiplierAffectHorizon)
                    {
                        stringBuilder.AppendLine($"    {stat.stat.LabelCap} : (k-1)x{stat.value}+1");
                    }

                    //stringBuilder.AppendLine(CheckAndMark(verbPropertiesObjectPatchByChildPart && properties.verbPropertiesObjectPatchByChildPart, "verbPropertiesObjectPatchByChildPart".Translate()));
                    //stringBuilder.AppendLine(CheckAndMark(toolsObjectPatchByChildPart && properties.toolsObjectPatchByChildPart, "toolsObjectPatchByChildPart".Translate()));
                }

                List<Dialog_InfoCard.Hyperlink> hyperlinks = new List<Dialog_InfoCard.Hyperlink>(properties.filterWithWeights.Count);
                hyperlinks.AddRange(from x in properties.filterWithWeights select new Dialog_InfoCard.Hyperlink(x.thingDef));
                if (child != null) hyperlinks.Insert(0, new Dialog_InfoCard.Hyperlink(child));
                yield return new StatDrawEntry(
                    StatCategoryDefOf.Weapon,
                    "AttachmentPoint".Translate() + " : " + properties.Name,
                    childComp?.Label ?? (Offseter
                    + " " + "Offseter".Translate() + "; " +
                    Multiplier
                    + " " + "Multiplier".Translate() + ";"),
                    stringBuilder.ToString(),
                    900,null,
                    hyperlinks
                    );
            }
            #endregion
            
            if (Prefs.DevMode)
            {
                CompEquippable? equippable = req.Thing?.TryGetComp<CompEquippable>();
                if(equippable != null)
                {
                    stringBuilder.Clear();
                    foreach (Verb verb in equippable.AllVerbs)
                    {
                        stringBuilder.AppendLine(verb.ToString());
                        if(verb.verbProps != null)
                        {
                            stringBuilder.AppendLine(" +verbProps:");
                            foreach(FieldInfo field in verb.verbProps.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                            {
                                stringBuilder.AppendLine($" | +{field.Name} : {field.GetValue(verb.verbProps)}");
                            }
                        }
                        if(verb.tool != null)
                        {
                            stringBuilder.AppendLine(" +tool:");
                            foreach(FieldInfo field in verb.tool.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                            {
                                stringBuilder.AppendLine($" | +{field.Name} : {field.GetValue(verb.tool)}");
                            }
                        }
                        stringBuilder.AppendLine("--------------------------------");
                    }
                    yield return new StatDrawEntry(
                        category: StatCategoryDefOf.Weapon,
                        "DEV: Verb Info",
                        "",
                        stringBuilder.ToString(),
                        1000
                    );
                }
            }
        }

        public float ExceedanceFactor = 1f;
        public float ExceedanceOffset = 1f;
        public uint TextureSizeFactor = RenderingTools.DefaultTextureSizeFactor;
        public FilterMode TextureFilterMode = FilterMode.Bilinear;
        /// <summary>
        /// If the part attach on other part, it will use this draw size
        /// </summary>
        public Vector2 DrawSizeWhenAttach = Vector2.one;


        #region Condation
        /// <summary>
        /// if parent part install this part it will checking this value when switch to other part
        /// </summary>
        public bool unchangeable = false;

        /// <summary>
        /// if it's **`true`**, this part and it's child part will not rendering when attach on a part
        /// </summary>
        public bool notDrawInParent = false;

        /// <summary>
        /// if it's **`true`**, it will able to create on `ModifyWeapon` window
        /// </summary>
        public bool allowCreateOnCraftingPort = false;

        /// <summary>
        /// if it's **`true`**, it will set random part by `WeaponAttachmentProperties.randomThingDefWeights` if it's not empty or by `WeaponAttachmentProperties.filter.AllowedThingDefs`
        /// </summary>
        public bool setRandomPartWhenCreate = false;

        /// <summary>
        /// if it's **`true`**,the parent part will not able to read `IVerbOwner.VerbProperties` from this node
        /// </summary>
        public bool notAllowParentUseVerbProperties = false;

        /// <summary>
        /// if it's **`true`**,the parent part will not able to read `IVerbOwner.Tools` from this node
        /// </summary>
        public bool notAllowParentUseTools = false;

        /// <summary>
        /// if it's **`true`**,the parent part will not able to read `extraComp` from this node
        /// </summary>
        public bool notAllowParentUseCompProperties = false;

        /// <summary>
        /// if it's **`true`**, it will not draw attachment when it not attach on other part
        /// </summary>
        public bool drawChildPartWhenOnGround = true;

        /// <summary>
        /// if it's **`true`**, the outline width scale of the root part will use pixel size
        /// </summary>
        public bool outlineWidthInPixelSize = false;

        /// <summary>
        /// if it's **`not zero value`**, the outline of the root part will drawing by this width
        /// </summary>
        public float outlineWidth = 0.015625f;
        #endregion




        #region Offset
        #region Parent
        /// <summary>
        /// this value will doing some addition calcation with `IVerbOwner.VerbProperties` value of parent
        /// </summary>
        public FieldReaderDgitList<VerbProperties> verbPropertiesOffseter = new FieldReaderDgitList<VerbProperties>();

        /// <summary>
        /// this value will doing some addition calcation with `IVerbOwner.Tools` value of parent
        /// </summary>
        public FieldReaderDgitList<Tool> toolsOffseter = new FieldReaderDgitList<Tool>();

        /// <summary>
        /// this value will doing some addition calcation with `ThingDef.Comps` value of parent
        /// </summary>
        public FieldReaderDgitList<CompProperties> compPropertiesOffseter = new FieldReaderDgitList<CompProperties>();

        /// <summary>
        /// this value will doing some addition calcation after `StatWorker.GetValue` returned of parent
        /// </summary>
        public List<StatModifier> statOffset = new List<StatModifier>();

        /// <summary>
        /// the default value when `statOffset` not contains stat
        /// </summary>
        public float statOffsetDefaultValue = 0;
        #endregion


        #region OtherPart
        /// <summary>
        /// this value define how strong will other part `verbPropertiesOffseter` affect `IVerbOwner.VerbProperties` value of this
        /// </summary>
        public FieldReaderDgitList<VerbProperties> verbPropertiesOtherPartOffseterAffectHorizon = new FieldReaderDgitList<VerbProperties>();
        /// <summary>
        /// this value define how strong will other part `toolsOffseter` affect `IVerbOwner.Tools` value of this
        /// </summary>
        public FieldReaderDgitList<Tool> toolsOtherPartOffseterAffectHorizon = new FieldReaderDgitList<Tool>();
        /// <summary>
        /// this value define how strong will other part `compPropertiesOffseter` affect `CompProperties` value of this
        /// </summary>
        public FieldReaderDgitList<CompProperties> compPropertiesOtherPartOffseterAffectHorizon = new FieldReaderDgitList<CompProperties>();
        /// <summary>
        /// this value define how strong will other part `statOffset` affect `StatWorker.GetValue` value of this
        /// </summary>
        public List<StatModifier> statOtherPartOffseterAffectHorizon = new List<StatModifier>();
        /// <summary>
        /// the default value when `statOtherPartOffseterAffectHorizon` not contains stat
        /// </summary>
        public float statOtherPartOffseterAffectHorizonDefaultValue = 1;
        #endregion
        #endregion




        #region Multiplier
        #region Parent
        /// <summary>
        /// this value will doing some multiplier calcation with `IVerbOwner.VerbProperties` value of parent
        /// </summary>
        public FieldReaderDgitList<VerbProperties> verbPropertiesMultiplier = new FieldReaderDgitList<VerbProperties>();

        /// <summary>
        /// this value will doing some multiplier calcation with `IVerbOwner.Tools` value of parent
        /// </summary>
        public FieldReaderDgitList<Tool> toolsMultiplier = new FieldReaderDgitList<Tool>();

        /// <summary>
        /// this value will doing some multiplier calcation with `ThingDef.Comps` value of parent
        /// </summary>
        public FieldReaderDgitList<CompProperties> compPropertiesMultiplier = new FieldReaderDgitList<CompProperties>();

        /// <summary>
        /// this value will doing some multiplier calcation with `StatWorker.GetValue` value of parent
        /// </summary>
        public List<StatModifier> statMultiplier = new List<StatModifier>();

        /// <summary>
        /// the default value when `statMultiplier` not contains stat
        /// </summary>
        public float statMultiplierDefaultValue = 1;
        #endregion


        #region OtherPart
        /// <summary>
        /// this value define how strong will other part `verbPropertiesMultiplier` affect `IVerbOwner.VerbProperties` value of this
        /// </summary>
        public FieldReaderDgitList<VerbProperties> verbPropertiesOtherPartMultiplierAffectHorizon = new FieldReaderDgitList<VerbProperties>();
        /// <summary>
        /// this value define how strong will other part `toolsMultiplier` affect `IVerbOwner.Tools` value of this
        /// </summary>
        public FieldReaderDgitList<Tool> toolsOtherPartMultiplierAffectHorizon = new FieldReaderDgitList<Tool>();
        /// <summary>
        /// this value define how strong will other part `compProperties` affect `ThingDef.Comps` value of this
        /// </summary>
        public FieldReaderDgitList<CompProperties> compPropertiesOtherPartMultiplierAffectHorizon = new FieldReaderDgitList<CompProperties>();
        /// <summary>
        /// this value define how strong will other part `statMultiplier` affect `StatWorker.GetValue` value of this
        /// </summary>
        public List<StatModifier> statOtherPartMultiplierAffectHorizon = new List<StatModifier>();
        /// <summary>
        /// the default value when `statOtherPartMultiplierAffectHorizon` not contains stat
        /// </summary>
        public float statOtherPartMultiplierAffectHorizonDefaultValue = 1;
        #endregion
        #endregion




        #region AndPatchs
        #region Parent
        /// <summary>
        /// this value will doing some boolean and calcation with `IVerbOwner.VerbProperties` value of parent
        /// </summary>
        public FieldReaderBoolList<VerbProperties> verbPropertiesBoolAndPatch = new FieldReaderBoolList<VerbProperties>();

        /// <summary>
        /// this value will doing some boolean and calcation with `IVerbOwner.Tools` value of parent
        /// </summary>
        public FieldReaderBoolList<Tool> toolsBoolAndPatch = new FieldReaderBoolList<Tool>();

        /// <summary>
        /// this value will doing some boolean and calcation with `ThingDef.Comps` value of parent
        /// </summary>
        public FieldReaderBoolList<CompProperties> compPropertiesBoolAndPatch = new FieldReaderBoolList<CompProperties>();
        #endregion


        #region OtherPart
        /// <summary>
        /// this value will doing some boolean and calcation with `verbPropertiesBoolAndPatch` value of this
        /// </summary>
        public FieldReaderBoolList<VerbProperties> verbPropertiesBoolAndPatchByOtherPart = new FieldReaderBoolList<VerbProperties>();
        /// <summary>
        /// this value will doing some boolean and calcation with `toolsBoolAndPatch` value of this
        /// </summary>
        public FieldReaderBoolList<Tool> toolsBoolAndPatchByOtherPart = new FieldReaderBoolList<Tool>();
        /// <summary>
        /// this value will doing some boolean and calcation with `compPropertiesBoolAndPatch` value of this
        /// </summary>
        public FieldReaderBoolList<CompProperties> compPropertiesBoolAndPatchByOtherPart = new FieldReaderBoolList<CompProperties>();
        #endregion
        #endregion




        #region OrPatchs
        #region Parent
        /// <summary>
        /// this value will doing some boolean or calcation with `IVerbOwner.VerbProperties` value of parent
        /// </summary>
        public FieldReaderBoolList<VerbProperties> verbPropertiesBoolOrPatch = new FieldReaderBoolList<VerbProperties>();

        /// <summary>
        /// this value will doing some boolean or calcation with `IVerbOwner.Tools` value of parent
        /// </summary>
        public FieldReaderBoolList<Tool> toolsBoolOrPatch = new FieldReaderBoolList<Tool>();

        /// <summary>
        /// this value will doing some boolean or calcation with `ThingDef.Comps` value of parent
        /// </summary>
        public FieldReaderBoolList<CompProperties> compPropertiesBoolOrPatch = new FieldReaderBoolList<CompProperties>();
        #endregion


        #region OtherPart
        /// <summary>
        /// this value will doing some boolean and calcation with `verbPropertiesBoolOrPatch` value of this
        /// </summary>
        public FieldReaderBoolList<VerbProperties> verbPropertiesBoolOrPatchByOtherPart = new FieldReaderBoolList<VerbProperties>();
        /// <summary>
        /// this value will doing some boolean and calcation with `toolsBoolOrPatchByOtherPart` value of this
        /// </summary>
        public FieldReaderBoolList<Tool> toolsBoolOrPatchByOtherPart = new FieldReaderBoolList<Tool>();
        /// <summary>
        /// this value will doing some boolean and calcation with `compPropertiesBoolOrPatchByOtherPart` value of this
        /// </summary>
        public FieldReaderBoolList<CompProperties> compPropertiesBoolOrPatchByOtherPart = new FieldReaderBoolList<CompProperties>();
        #endregion
        #endregion




        #region InstPatchs
        #region Parent
        /// <summary>
        /// this value will replace some parmerters in `IVerbOwner.VerbProperties` value of parent
        /// </summary>
        public FieldReaderInstList<VerbProperties> verbPropertiesObjectPatch = new FieldReaderInstList<VerbProperties>();

        /// <summary>
        /// this value will replace some parmerters in `IVerbOwner.Tools` value of parent
        /// </summary>
        public FieldReaderInstList<Tool> toolsObjectPatch = new FieldReaderInstList<Tool>();

        /// <summary>
        /// this value will replace some parmerters in `ThingDef.Comps` value of parent
        /// </summary>
        public FieldReaderInstList<CompProperties> compPropertiesObjectPatch = new FieldReaderInstList<CompProperties>();
        #endregion


        #region OtherPart
        /// <summary>
        /// this value define witch parmerters is able to replace on `IVerbOwner.VerbProperties` of this
        /// </summary>
        public FieldReaderFiltList<VerbProperties> verbPropertiesObjectPatchByOtherPart = new FieldReaderFiltList<VerbProperties>();
        /// <summary>
        /// this value define witch parmerters is able to replace on `IVerbOwner.Tools` of this
        /// </summary>
        public FieldReaderFiltList<Tool> toolsObjectPatchByOtherPart = new FieldReaderFiltList<Tool>();
        /// <summary>
        /// this value define witch parmerters is able to replace on `ThingDef.Comps` of this
        /// </summary>
        public FieldReaderFiltList<CompProperties> compPropertiesObjectPatchByOtherPart = new FieldReaderFiltList<CompProperties>();
        #endregion
        #endregion



        /// <summary>
        /// attach points defintion
        /// </summary>
        public List<WeaponAttachmentProperties> attachmentProperties = new List<WeaponAttachmentProperties>();

        /// <summary>
        /// extra drawing info when it attach on a part
        /// </summary>
        public List<PartSubDrawingInfo> subRenderingInfos = new List<PartSubDrawingInfo>();

        /// <summary>
        /// List of destructors for ThingComps associated with this modularization weapon.
        /// </summary>
        public List<IThingCompDestructor> thingCompDestructors = new List<IThingCompDestructor>();

        /// <summary>
        /// List of weaponPropertiesInfos
        /// </summary>
        public List<WeaponPropertiesInfo> weaponPropertiesInfos = new List<WeaponPropertiesInfo>();

        /// <summary>
        /// the ThingComp type that not allow create from child comps
        /// </summary>
        public List<Type> notAllowedCompTypes = new List<Type>();

        /// <summary>
        /// special drawing texture when it attach on a part
        /// </summary>
        public string? PartTexPath = null;

        /// <summary>
        /// material cache of `PartTexPath`
        /// </summary>
        private Texture2D? partTexCache;
        internal List<(QueryGroup,WeaponAttachmentProperties)> attachmentPropertiesWithQuery = new List<(QueryGroup, WeaponAttachmentProperties)>();
    }
}