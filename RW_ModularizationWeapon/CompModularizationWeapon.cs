using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using RimWorld.QuestGen;
using RW_ModularizationWeapon.Tools;
using RW_NodeTree;
using RW_NodeTree.Rendering;
using RW_NodeTree.Tools;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Noise;

namespace RW_ModularizationWeapon
{
    [StaticConstructorOnStartup]
    public partial class CompModularizationWeapon : CompBasicNodeComp, IEnumerable<(string,Thing,WeaponAttachmentProperties)>
    {
        public CompProperties_ModularizationWeapon Props => (CompProperties_ModularizationWeapon)props;

        public CompModularizationWeapon ParentPart => ParentProccesser?.parent;

        public CompModularizationWeapon RootPart
        {
            get
            {
                CompModularizationWeapon result = null;
                CompModularizationWeapon current = this;
                while (current != null)
                {
                    result = current;
                    current = current.ParentPart;
                }
                return result;
            }
        }


        static CompModularizationWeapon()
        {
            if(CombatExtended_CompAmmoUser != null)
            {
                CombatExtended_CompAmmoUser_currentAmmoInt = AccessTools.FieldRefAccess<ThingDef>(CombatExtended_CompAmmoUser, "currentAmmoInt");
                CombatExtended_CompAmmoUser_CurMagCount_get = CombatExtended_CompAmmoUser.GetMethod("get_CurMagCount", BindingFlags.Instance | BindingFlags.Public);
                CombatExtended_CompAmmoUser_CurMagCount_set = CombatExtended_CompAmmoUser.GetMethod("set_CurMagCount", BindingFlags.Instance | BindingFlags.Public);
            }
        }

        public override void PostPostMake()
        {
            if (Props.setRandomPartWhenCreate) LongEventHandler.ExecuteWhenFinished(SetThingToRandom);
            else LongEventHandler.ExecuteWhenFinished(SetThingToDefault);
            NodeProccesser.UpdateNode();
        }


        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref showTargetPart, "showTargetPart");
            Scribe_Values.Look(ref usingTargetPart, "usingTargetPart");
            Scribe_Collections.Look(ref targetPartsWithId, "targetPartsWithId", LookMode.Value, LookMode.LocalTargetInfo);
            if(Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
            {
                foreach(Thing thing in ChildNodes.Values)
                {
                    CompModularizationWeapon comp = thing;
                    if(comp != null)
                    {
                        comp.targetModeParent = NodeProccesser;
                        comp.UsingTargetPart = comp.ShowTargetPart;
                    }
                }
                foreach (ThingComp comp in parent.AllComps)
                {
                    if (comp == this) continue;
                    CompProperties properties = null;
                    foreach (CompProperties def in parent.def.comps)
                    {
                        if (def.compClass == comp.GetType())
                        {
                            properties = def;
                            break;
                        }
                    }
                    if (properties != null)
                    {
                        comp.props = CompPropertiesAfterAffect(properties);
                    }
                }
                NodeProccesser.UpdateNode();
            }
        }


        public override bool AllowStackWith(Thing other)
        {
            return Props.attachmentProperties.Count == 0;
        }


        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            for (int i = 0; i < ChildNodes.Count; i++)
            {
                ThingWithComps part = ChildNodes[i] as ThingWithComps;
                if(part != null)
                {
                    foreach (ThingComp comp in part.AllComps)
                    {
                        Type type = comp.GetType();
                        if(type == typeof(CompModularizationWeapon) || type == CombatExtended_CompAmmoUser || type == CombatExtended_CompFireModes)
                        {
                            foreach(Gizmo gizmo in comp.CompGetGizmosExtra())
                            {
                                yield return gizmo;
                            }
                        }
                    }
                }
            }
        }


        protected override List<(Thing, string, List<RenderInfo>)> OverrideDrawSteep(List<(Thing, string, List<RenderInfo>)> nodeRenderingInfos, Rot4 rot, Graphic graphic)
        {
            Matrix4x4 scale = Matrix4x4.identity;
            for (int i = 0; i < nodeRenderingInfos.Count; i++)
            {
                (Thing part, string id, List<RenderInfo> renderInfos) = nodeRenderingInfos[i];
                WeaponAttachmentProperties properties = Props.WeaponAttachmentPropertiesById(id);
                if (id.NullOrEmpty() && part == parent)
                {
                    //Log.Message($"ParentProccesser : {ParentProccesser}");
                    if (ParentProccesser != null)
                    {
                        Material material = graphic?.MatAt(rot, this.parent);
                        for (int j = 0; j < renderInfos.Count; j++)
                        {
                            RenderInfo info = renderInfos[j];
                            if (info.material == material)
                            {
                                info.material = Props.PartTexMaterial ?? info.material;
                                scale.m00 = Props.DrawSizeWhenAttach.x / info.mesh.bounds.size.x;
                                scale.m22 = Props.DrawSizeWhenAttach.y / info.mesh.bounds.size.z;
                                renderInfos[j] = info;
                                break;
                            }
                        }
                    }
                    break;
                }
            }
            for (int i = 0; i < nodeRenderingInfos.Count; i++)
            {
                (Thing part, string id, List<RenderInfo> renderInfos) = nodeRenderingInfos[i];
                WeaponAttachmentProperties properties = Props.WeaponAttachmentPropertiesById(id);
                if(id.NullOrEmpty() && part == parent)
                {
                    if (ParentProccesser != null)
                    {
                        for (int j = 0; j < renderInfos.Count; j++)
                        {
                            RenderInfo info = renderInfos[j];
                            Matrix4x4[] matrix = info.matrices;

                            for (int k = 0; k < matrix.Length; k++)
                            {
                                matrix[k] = scale * matrix[k];
                            }
                        }
                    }
                }
                else if (!internal_NotDraw(part, properties))
                {
                    if (properties != null)
                    {
                        for (int j = 0; j < renderInfos.Count; j++)
                        {
                            RenderInfo info = renderInfos[j];
                            Matrix4x4[] matrix = info.matrices;
                            for (int k = 0; k < matrix.Length; k++)
                            {
                                Vector4 cache = matrix[k].GetRow(0);
                                matrix[k].SetRow(0, new Vector4(new Vector3(cache.x, cache.y, cache.z).magnitude, 0, 0, cache.w));

                                cache = matrix[k].GetRow(1);
                                matrix[k].SetRow(1, new Vector4(0, new Vector3(cache.x, cache.y, cache.z).magnitude, 0, cache.w));

                                cache = matrix[k].GetRow(2);
                                matrix[k].SetRow(2, new Vector4(0, 0, new Vector3(cache.x, cache.y, cache.z).magnitude, cache.w));

                                matrix[k] = properties.Transfrom * scale * matrix[k];
                                //matrix[k] = properties.Transfrom;
                            }
                            renderInfos[j] = info;
                        }
                    }
                }
                else
                {
                    renderInfos.Clear();
                }
            }
            nodeRenderingInfos.SortBy(x =>
            {
                for (int i = 0; i < Props.attachmentProperties.Count; i++)
                {
                    WeaponAttachmentProperties properties = Props.attachmentProperties[i];
                    if (properties.id == x.Item2) return i + properties.drawWeight * Props.attachmentProperties.Count;
                }
                return -1;
            });
            return nodeRenderingInfos;
        }


        protected override bool AllowNode(Thing node, string id = null)
        {
            WeaponAttachmentProperties properties = Props.WeaponAttachmentPropertiesById(id);
            //if (Prefs.DevMode) Log.Message($"properties : {properties}");
            if (properties != null)
            {
                if(node == null) return properties.allowEmpty;
                CompModularizationWeapon comp = node;
                if(comp != null)
                {
                    for (int i = 0; i < ChildNodes.Count; i++)
                    {
                        if (comp.Props.disallowedOtherPart.Allows(ChildNodes[i]))
                        {
                            return false;
                        }
                    }
                }
                for (int i = 0; i < ChildNodes.Count; i++)
                {
                    comp = ChildNodes[i];
                    if(comp != null && comp.Props.disallowedOtherPart.Allows(node))
                    {
                        return false;
                    }
                }
                //if (Prefs.DevMode) Log.Message($"properties.filter.AllowedDefCount : {properties.filter.AllowedDefCount}");
                return
                    ((CompModularizationWeapon)node)?.targetModeParent == null &&
                    properties.filter.Allows(node) &&
                    !internal_Unchangeable(ChildNodes[id], properties);
            }
            return false;
        }


        public void SetThingToDefault()
        {
            foreach (WeaponAttachmentProperties properties in Props.attachmentProperties)
            {
                ThingDef def = properties.defultThing;
                if (def != null)
                {
                    Thing thing = ThingMaker.MakeThing(def, GenStuff.RandomStuffFor(def));
                    thing.TryGetComp<CompQuality>()?.SetQuality(QualityUtility.GenerateQualityRandomEqualChance(), ArtGenerationContext.Colony);
                    ChildNodes[properties.id] = thing;
                }
            }
            foreach (Thing thing in ChildNodes.Values)
            {
                ((CompModularizationWeapon)thing)?.SetThingToDefault();
            }
        }


        public void SetThingToRandom()
        {
            System.Random random = new System.Random();
            foreach (WeaponAttachmentProperties properties in Props.attachmentProperties)
            {
                for(int i = 0; i < 3; i++)
                {
                    int j = random.Next(properties.allowEmpty ? (properties.filter.AllowedDefCount + 1) : properties.filter.AllowedDefCount);
                    ThingDef def = j < properties.filter.AllowedDefCount ? properties.filter.AllowedThingDefs.ToList()[j] : null;
                    if (def != null)
                    {
                        Thing thing = ThingMaker.MakeThing(def, GenStuff.RandomStuffFor(def));
                        thing.TryGetComp<CompQuality>()?.SetQuality(QualityUtility.GenerateQualityRandomEqualChance(), ArtGenerationContext.Colony);
                        ChildNodes[properties.id] = thing;
                        if (ChildNodes[properties.id] == thing) break;
                    }
                    else break;
                }
            }
        }


        protected override IEnumerable<Thing> PostGenRecipe_MakeRecipeProducts(RecipeDef recipeDef, Pawn worker, List<Thing> ingredients, Thing dominantIngredient1, IBillGiver billGiver, Precept_ThingStyle precept, RecipeInvokeSource invokeSource, IEnumerable<Thing> result)
        {
            List<Thing> things = result.ToList();
            if(invokeSource == RecipeInvokeSource.products)
            {
                LongEventHandler.ExecuteWhenFinished(SetThingToDefault);
            }
            return things;
        }


        protected override bool PostThingWithComps_PreApplyDamage(ref DamageInfo dinfo, bool absorbed)
        {
            int count = ChildNodes.Count + 1;
            dinfo.SetAmount(dinfo.Amount / count);
            foreach (Thing thing in ChildNodes.Values)
            {
                thing.TakeDamage(dinfo);
            }
            return absorbed;
        }


        internal CompProperties CompPropertiesAfterAffect(CompProperties compProperties)
        {
            //tool = (Tool)tool.SimpleCopy();
            compProperties *= CompPropertiesMultiplier();
            compProperties += CompPropertiesOffseter();
            compProperties &= CompPropertiesBoolAndPatch();
            compProperties |= CompPropertiesBoolOrPatch();
            CompPropertiesObjectPatch()
            .ForEach(x =>
            {
                //Log.Message(x.ToString());
                compProperties &= x;
                compProperties |= x;
            });
            return compProperties;
        }


        protected override bool UpdateNode(CompChildNodeProccesser actionNode)
        {
            statOffsetCache.Clear();
            statMultiplierCache.Clear();

            if (!UsingTargetPart && CombatExtended_CompAmmoUser != null)
            {
                foreach (ThingComp comp in parent.AllComps)
                {
                    Type type = comp.GetType();
                    if (type == CombatExtended_CompAmmoUser)
                    {
                        Thing thing = ThingMaker.MakeThing(CombatExtended_CompAmmoUser_currentAmmoInt(comp), null);
                        thing.stackCount = (int)CombatExtended_CompAmmoUser_CurMagCount_get.Invoke(comp,null);
                        CombatExtended_CompAmmoUser_CurMagCount_set.Invoke(comp, new object[] { 0 });
                        GenThing.TryDropAndSetForbidden(thing, parent.PositionHeld, parent.MapHeld, ThingPlaceMode.Near, out _, false);
                    }
                }
            }

            foreach (ThingComp comp in parent.AllComps)
            {
                if (comp == this) continue;
                CompProperties properties = null;
                foreach(CompProperties prop in parent.def.comps)
                {
                    if(prop.compClass == comp.GetType())
                    {
                        properties = prop;
                        break;
                    }
                }
                if(properties != null)
                {
                    comp.Initialize(CompPropertiesAfterAffect(properties));
                }
            }

            return false;
        }


        protected override HashSet<string> RegiestedNodeId(HashSet<string> regiestedNodeId)
        {
            foreach(WeaponAttachmentProperties properties in Props.attachmentProperties) regiestedNodeId.Add(properties.id);
            return regiestedNodeId;
        }


        protected override void Added(NodeContainer container, string id)
        {
            //Log.Message($"container add {container.Comp}");
            targetModeParent = container.Comp;
            UsingTargetPart = ShowTargetPart;
            NodeProccesser.NeedUpdate = true;
        }


        protected override void Removed(NodeContainer container, string id)
        {
            //Log.Message($"container remove {container.Comp}");
            targetModeParent = null;
            UsingTargetPart = ShowTargetPart;
            NodeProccesser.NeedUpdate = true;
            NodeProccesser.UpdateNode();
        }


        protected override CompChildNodeProccesser OverrideParentProccesser(CompChildNodeProccesser orginal) => UsingTargetPart ? targetModeParent : orginal;


        #region operator
        public static implicit operator Thing(CompModularizationWeapon node)
        {
            return node?.parent;
        }

        public static implicit operator CompModularizationWeapon(Thing thing)
        {
            return thing?.TryGetComp<CompModularizationWeapon>();
        }
        #endregion


        internal CompChildNodeProccesser targetModeParent;
        private readonly Dictionary<string, bool> childTreeViewOpend = new Dictionary<string, bool>();
        private readonly Dictionary<(StatDef, Thing), float> statOffsetCache = new Dictionary<(StatDef, Thing), float>();
        private readonly Dictionary<(StatDef, Thing), float> statMultiplierCache = new Dictionary<(StatDef, Thing), float>();
        private Dictionary<string, LocalTargetInfo> targetPartsWithId = new Dictionary<string, LocalTargetInfo>();
        private bool showTargetPart = false;
        private bool usingTargetPart = false;

        private static Type CombatExtended_CompAmmoUser = GenTypes.GetTypeInAnyAssembly("CombatExtended.CompAmmoUser");
        private static Type CombatExtended_CompFireModes = GenTypes.GetTypeInAnyAssembly("CombatExtended.CompFireModes");
        private static AccessTools.FieldRef<StatWorker, StatDef> StatWorker_stat = AccessTools.FieldRefAccess<StatWorker, StatDef>("stat");
        private static AccessTools.FieldRef<ThingDef, List<VerbProperties>> ThingDef_verbs = AccessTools.FieldRefAccess<ThingDef, List<VerbProperties>>("verbs");
        private static AccessTools.FieldRef<object, ThingDef> CombatExtended_CompAmmoUser_currentAmmoInt = null;
        private static MethodInfo CombatExtended_CompAmmoUser_CurMagCount_get = null;
        private static MethodInfo CombatExtended_CompAmmoUser_CurMagCount_set = null;
    }


    public class CompProperties_ModularizationWeapon : CompProperties
    {
        public Material PartTexMaterial
        {
            get
            {
                if (materialCache == null)
                {
                    Texture2D texture = (!PartTexPath.NullOrEmpty()) ? ContentFinder<Texture2D>.Get(PartTexPath) : null;
                    if(texture != null)
                    {
                        materialCache = new Material(ShaderDatabase.Cutout);
                        materialCache.mainTexture = texture;
                    }
                }
                return materialCache;
            }
        }


        public Texture2D PartTexture
        {
            get
            {
                return PartTexMaterial?.mainTexture as Texture2D;
            }
        }


        public CompProperties_ModularizationWeapon()
        {
            compClass = typeof(CompModularizationWeapon);
        }


        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach(string error in base.ConfigErrors(parentDef))
            {
                yield return error;
            }
            for (int i = attachmentProperties.Count - 1; i >= 0; i--)
            {
                WeaponAttachmentProperties properties = attachmentProperties[i];
                if(properties == null)
                {
                    attachmentProperties.RemoveAt(i);
                    yield return $"attachmentProperties[{i}] is null";
                    continue;
                }
                else if(!properties.id.IsVaildityKeyFormat())
                {
                    attachmentProperties.RemoveAt(i);
                    yield return $"attachmentProperties[{i}].id is invaild key format";
                    continue;
                }
                for (int j = 0; j < i; j++)
                {
                    WeaponAttachmentProperties propertiesForCompare = attachmentProperties[j];
                    if(!(propertiesForCompare?.id).NullOrEmpty() && propertiesForCompare.id == properties.id)
                    {
                        attachmentProperties.RemoveAt(i);
                        yield return $"attachmentProperties[{i}].id should be unique, but now repeat with attachmentProperties[{j}].id";
                        break;
                    }
                }
            }
        }


        public WeaponAttachmentProperties WeaponAttachmentPropertiesById(string id)
        {
            if(!id.NullOrEmpty())
            {
                foreach(WeaponAttachmentProperties properties in attachmentProperties)
                {
                    if(properties.id == id) return properties;
                }
            }
            return null;
        }


        public override void ResolveReferences(ThingDef parentDef)
        {
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
            }
            if (attachmentProperties.Count > 0) parentDef.stackLimit = 1;


            void CheckAndSetListDgit<T>(ref FieldReaderDgitList<T> list, float defaultValue)
            {
                list = list ?? new FieldReaderDgitList<T>();
                list.RemoveAll(f => f == null);
                list.DefaultValue = defaultValue;
            }

            void CheckAndSetListBool<T>(ref FieldReaderBoolList<T> list, bool defaultValue)
            {
                list = list ?? new FieldReaderBoolList<T>();
                list.RemoveAll(f => f == null);
                list.DefaultValue = defaultValue;
            }

            CheckAndSetListDgit(ref verbPropertiesOffseter, 0);
            CheckAndSetListDgit(ref toolsOffseter, 0);

            CheckAndSetListDgit(ref verbPropertiesMultiplier, 1);
            CheckAndSetListDgit(ref toolsMultiplier, 1);

            verbPropertiesObjectPatch = verbPropertiesObjectPatch ?? new List<FieldReaderInst<VerbProperties>>();
            verbPropertiesObjectPatch.RemoveAll(f => f == null);
            toolsObjectPatch = toolsObjectPatch ?? new List<FieldReaderInst<Tool>>();
            toolsObjectPatch.RemoveAll(f => f == null);

            CheckAndSetListBool(ref verbPropertiesBoolAndPatch, true);
            CheckAndSetListBool(ref toolsBoolAndPatch, true);

            CheckAndSetListBool(ref verbPropertiesBoolOrPatch, false);
            CheckAndSetListBool(ref toolsBoolOrPatch, false);

            parentDef.weaponTags = parentDef.weaponTags ?? new List<string>();

            disallowedOtherPart = disallowedOtherPart ?? new ThingFilter();
            disallowedOtherPart.ResolveReferences();
        }


        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
        {
            CompModularizationWeapon comp = req.Thing;
            StringBuilder stringBuilder = new StringBuilder();

            int listAllDgit<T>(
                List<FieldReaderDgit<T>> list,
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
                    foreach ((FieldInfo field, IConvertible value) in list[i])
                    {
                        if (snap) stringBuilder.Append("  ");
                        stringBuilder.AppendLine($"    {field.Name.Translate()} : {perfix}{value}{postfix}");
                    }
                    result += list[i].Count;
                }
                return result;
            }

            int listAllInst<T>(
                List<FieldReaderInst<T>> list,
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
                    foreach ((FieldInfo field, object value) in list[i])
                    {
                        if (snap) stringBuilder.Append("  ");
                        stringBuilder.AppendLine($"    {field.Name.Translate()} : {perfix}{value}{postfix}");
                    }
                    result += list[i].Count;
                }
                return result;
            }
            int count = statOffset.Count;
            stringBuilder.AppendLine("verbPropertiesOffseter".Translate().RawText + " :");
            count += listAllDgit(comp?.VerbPropertiesOffseter(null) ?? verbPropertiesOffseter, "+", "");

            stringBuilder.AppendLine("toolsOffseter".Translate().RawText + " :");
            count += listAllDgit(comp?.ToolsOffseter(null) ?? toolsOffseter, "+", "");

            stringBuilder.AppendLine("statOffseter".Translate().RawText + " :");
            foreach (StatModifier stat in statOffset)
            {
                stringBuilder.AppendLine($"  {stat.stat.LabelCap} : +{comp?.GetStatOffset(stat.stat, null) ?? stat.value}");
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
            count += listAllDgit(comp?.VerbPropertiesMultiplier(null) ?? verbPropertiesMultiplier, "x", "");

            stringBuilder.AppendLine("toolsMultiplier".Translate().RawText + " :");
            count += listAllDgit(comp?.ToolsMultiplier(null) ?? toolsMultiplier, "x", "");

            stringBuilder.AppendLine("statMultiplier".Translate().RawText + " :");
            foreach (StatModifier stat in statMultiplier)
            {
                stringBuilder.AppendLine($"  {stat.stat.LabelCap} : x{comp?.GetStatMultiplier(stat.stat, null) ?? stat.value}");
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
            count += listAllInst(comp?.VerbPropertiesObjectPatch(null) ?? verbPropertiesObjectPatch, "", "");

            stringBuilder.AppendLine("toolsPatch".Translate().RawText + " :");
            count += listAllInst(comp?.ToolsObjectPatch(null) ?? toolsObjectPatch, "", "");

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
            foreach (WeaponAttachmentProperties properties in attachmentProperties)
            {

                Thing child = comp?.ChildNodes[properties.id];
                CompModularizationWeapon childComp = child;
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
                    listAllDgit(childComp.VerbPropertiesOffseter(null) * properties.verbPropertiesOffseterAffectHorizon, "+", "", true);

                    stringBuilder.AppendLine("  " + "toolsOffseter".Translate() + " :");
                    listAllDgit(childComp.ToolsOffseter(null) * properties.toolsOffseterAffectHorizon, "+", "", true);

                    stringBuilder.AppendLine("  " + "statOffseter".Translate() + " :");
                    foreach (StatModifier stat in childComp.Props.statOffset)
                    {
                        stringBuilder.AppendLine($"    {stat.stat.LabelCap} : +{properties.statOffsetAffectHorizon.GetStatValueFromList(stat.stat, properties.statOffsetAffectHorizonDefaultValue) * childComp.GetStatOffset(stat.stat, req.Thing)}");
                    }


                    stringBuilder.AppendLine("Multiplier".Translate() + " :");
                    stringBuilder.AppendLine("  " + "verbPropertiesMultiplier".Translate() + " :");
                    FieldReaderDgitList<VerbProperties> cacheVerbProperties = childComp.VerbPropertiesMultiplier(null) - 1;
                    if (cacheVerbProperties.HasDefaultValue) cacheVerbProperties.DefaultValue--;
                    listAllDgit(cacheVerbProperties * properties.verbPropertiesMultiplierAffectHorizon + 1, "x", "", true);

                    stringBuilder.AppendLine("  " + "toolsMultiplier".Translate() + " :");
                    FieldReaderDgitList<Tool> cacheTools = childComp.ToolsMultiplier(null) - 1;
                    if (cacheTools.HasDefaultValue) cacheTools.DefaultValue--;
                    listAllDgit(cacheTools * properties.toolsMultiplierAffectHorizon + 1, "x", "", true);

                    stringBuilder.AppendLine("  " + "statMultiplier".Translate() + " :");
                    foreach (StatModifier stat in childComp.Props.statMultiplier)
                    {
                        stringBuilder.AppendLine($"    {stat.stat.LabelCap} : x{properties.statMultiplierAffectHorizon.GetStatValueFromList(stat.stat, properties.statMultiplierAffectHorizonDefaultValue) * (childComp.GetStatMultiplier(stat.stat, req.Thing) - 1f) + 1f}");
                    }

                    stringBuilder.AppendLine("verbPropertiesPatch".Translate().RawText + " :");
                    if(verbPropertiesObjectPatchByChildPart && properties.verbPropertiesObjectPatchByChildPart) listAllInst(childComp.VerbPropertiesObjectPatch(null), "", "", true);

                    stringBuilder.AppendLine("toolsPatch".Translate().RawText + " :");
                    if (toolsObjectPatchByChildPart && properties.toolsObjectPatchByChildPart) listAllInst(childComp.ToolsObjectPatch(null), "", "", true);
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

                    stringBuilder.AppendLine("  " + "statMultiplierAffectHorizon".Translate() + " :");
                    foreach (StatModifier stat in properties.statMultiplierAffectHorizon)
                    {
                        stringBuilder.AppendLine($"    {stat.stat.LabelCap} : (k-1)x{stat.value}+1");
                    }

                    stringBuilder.AppendLine(CheckAndMark(verbPropertiesObjectPatchByChildPart && properties.verbPropertiesObjectPatchByChildPart, "verbPropertiesObjectPatchByChildPart".Translate()));
                    stringBuilder.AppendLine(CheckAndMark(toolsObjectPatchByChildPart && properties.toolsObjectPatchByChildPart, "toolsObjectPatchByChildPart".Translate()));
                }

                List<Dialog_InfoCard.Hyperlink> hyperlinks = new List<Dialog_InfoCard.Hyperlink>(properties.filter.AllowedDefCount);
                hyperlinks.AddRange(from x in properties.filter.AllowedThingDefs select new Dialog_InfoCard.Hyperlink(x));
                if (child != null) hyperlinks.Insert(0, new Dialog_InfoCard.Hyperlink(child));
                yield return new StatDrawEntry(
                    StatCategoryDefOf.Weapon,
                    "AttachmentPoint".Translate() + " : " + properties.Name,
                    childComp?.parent.Label ?? (Offseter
                    + " " + "Offseter".Translate() + "; " +
                    Multiplier
                    + " " + "Multiplier".Translate() + ";"),
                    stringBuilder.ToString(),
                    900,null,
                    hyperlinks
                    );
            }
            #endregion
        }

        public Vector2 DrawSizeWhenAttach = Vector2.one;


        #region Condation
        public bool unchangeable = false;


        public bool notDrawInParent = false;


        public bool allowCreateOnCraftingPort = false;


        public bool setRandomPartWhenCreate = false;


        public bool notAllowParentUseVerbProperties = false;


        public bool notAllowParentUseTools = false;
        #endregion


        #region Offset
        public FieldReaderDgitList<VerbProperties> verbPropertiesOffseter = new FieldReaderDgitList<VerbProperties>();


        public FieldReaderDgitList<Tool> toolsOffseter = new FieldReaderDgitList<Tool>();


        public List<StatModifier> statOffset = new List<StatModifier>();


        public FieldReaderDgitList<CompProperties> compPropertiesOffseter = new FieldReaderDgitList<CompProperties>();


        //public FieldReaderDgitList<CompProperties>> ThingCompOffseter = new FieldReaderDgitList<CompProperties>>();

        public float verbPropertiesOtherPartOffseterAffectHorizon = 1;

        public float toolsOtherPartOffseterAffectHorizon = 1;

        public float statOtherPartOffseterAffectHorizon = 1;
        #endregion


        #region Multiplier
        public FieldReaderDgitList<VerbProperties> verbPropertiesMultiplier = new FieldReaderDgitList<VerbProperties>();


        public FieldReaderDgitList<Tool> toolsMultiplier = new FieldReaderDgitList<Tool>();


        public List<StatModifier> statMultiplier = new List<StatModifier>();


        public FieldReaderDgitList<CompProperties> compPropertiesMultiplier = new FieldReaderDgitList<CompProperties>();


        //public FieldReaderDgitList<CompProperties>> ThingCompMultiplier = new FieldReaderDgitList<CompProperties>>();

        public float verbPropertiesOtherPartMultiplierAffectHorizon = 1;

        public float toolsOtherPartMultiplierAffectHorizon = 1;

        public float statOtherPartMultiplierAffectHorizon = 1;
        #endregion


        #region Patchs
        public List<FieldReaderInst<VerbProperties>> verbPropertiesObjectPatch = new List<FieldReaderInst<VerbProperties>>();


        public List<FieldReaderInst<Tool>> toolsObjectPatch = new List<FieldReaderInst<Tool>>();


        public List<FieldReaderInst<CompProperties>> compPropertiesObjectPatch = new List<FieldReaderInst<CompProperties>>();


        public bool verbPropertiesObjectPatchByChildPart = true;


        public bool toolsObjectPatchByChildPart = true;


        public bool verbPropertiesObjectPatchByOtherPart = false;


        public bool toolsObjectPatchByOtherPart = false;




        public FieldReaderBoolList<VerbProperties> verbPropertiesBoolAndPatch = new FieldReaderBoolList<VerbProperties>();


        public FieldReaderBoolList<Tool> toolsBoolAndPatch = new FieldReaderBoolList<Tool>();


        public FieldReaderBoolList<CompProperties> compPropertiesBoolAndPatch = new FieldReaderBoolList<CompProperties>();


        public bool verbPropertiesBoolAndPatchByChildPart = true;


        public bool toolsBoolAndPatchByChildPart = true;


        public bool verbPropertiesBoolAndPatchByOtherPart = false;


        public bool toolsBoolAndPatchByOtherPart = false;




        public FieldReaderBoolList<VerbProperties> verbPropertiesBoolOrPatch = new FieldReaderBoolList<VerbProperties>();


        public FieldReaderBoolList<Tool> toolsBoolOrPatch = new FieldReaderBoolList<Tool>();


        public FieldReaderBoolList<CompProperties> compPropertiesBoolOrPatch = new FieldReaderBoolList<CompProperties>();


        public bool verbPropertiesBoolOrPatchByChildPart = true;


        public bool toolsBoolOrPatchByChildPart = true;


        public bool verbPropertiesBoolOrPatchByOtherPart = false;


        public bool toolsBoolOrPatchByOtherPart = false;
        #endregion


        public List<WeaponAttachmentProperties> attachmentProperties = new List<WeaponAttachmentProperties>();


        public ThingFilter disallowedOtherPart = new ThingFilter();


        public string PartTexPath = null;


        private Material materialCache;
    }
}
