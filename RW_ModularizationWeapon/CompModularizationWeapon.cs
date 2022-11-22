using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
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
                //CombatExtended_CompFireModes_availableAimModes = AccessTools.FieldRefAccess<IList>(CombatExtended_CompFireModes, "availableAimModes");
                CombatExtended_CompAmmoUser_CurMagCount_get = CombatExtended_CompAmmoUser.GetMethod("get_CurMagCount", BindingFlags.Instance | BindingFlags.Public);
                CombatExtended_CompAmmoUser_CurMagCount_set = CombatExtended_CompAmmoUser.GetMethod("set_CurMagCount", BindingFlags.Instance | BindingFlags.Public);
            }
        }

        public override void PostPostMake()
        {
            if (Props.setRandomPartWhenCreate) LongEventHandler.ExecuteWhenFinished(SetPartToRandom);
            else LongEventHandler.ExecuteWhenFinished(SetPartToDefault);
            NodeProccesser.UpdateNode();
        }


        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref targetPartsWithId, "targetPartsWithId", LookMode.Value, LookMode.LocalTargetInfo);
            if(Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
            {
                NodeContainer container = ChildNodes;
                foreach(Thing thing in container.Values)
                {
                    CompModularizationWeapon comp = thing;
                    if(comp != null)
                    {
                        comp.targetParentPart = container;
                    }
                }

                for (int i = 0; i < parent.AllComps.Count; i++)
                {
                    ThingComp comp = parent.AllComps[i];
                    Type type = comp.GetType();
                    if (type == typeof(CompChildNodeProccesser) || type == typeof(CompModularizationWeapon)) continue;
                    CompProperties properties = parent.def.comps.FirstOrDefault(x => x.compClass == type);
                    if (properties != null)
                    {
                        try
                        {
                            if (Props.compPropertiesCreateInstanceCompType.Contains(type)) comp = (ThingComp)Activator.CreateInstance(type);
                            comp.parent = parent;
                            comp.Initialize(CompPropertiesAfterAffect(properties));
                            parent.AllComps[i] = comp;
                        }
                        catch (Exception ex)
                        {
                            Log.Error("Could not instantiate or initialize a ThingComp: " + ex);
                        }
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
                                Command command = gizmo as Command;
                                if(command != null && type != typeof(CompModularizationWeapon))
                                {
                                    command.defaultLabel = part.LabelCap + " : " + command.defaultLabel;
                                }
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
                else if (!internal_NotDraw(part, properties) && (ParentProccesser != null || Props.drawChildPartWhenOnGround))
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
                            info.mesh = MeshReindexed.GetOrNewWhenNull(info.mesh, () =>
                            {
                                if (MeshReindexed.ContainsValue(info.mesh))
                                {
                                    return info.mesh;
                                }
                                Mesh mesh = new Mesh();
                                mesh.name = info.mesh.name + " Reindexed";

                                List<Vector3> vert = new List<Vector3>(info.mesh.vertices);
                                vert.AddRange(vert);
                                mesh.vertices = vert.ToArray();

                                List<Vector2> uv = new List<Vector2>(info.mesh.uv);
                                uv.AddRange(uv);
                                mesh.uv = uv.ToArray();

                                List<int> trangles = new List<int>(info.mesh.GetTriangles(0));
                                trangles.Capacity = 2 * trangles.Count;
                                for (int k = trangles.Count - 1; k >= 0; k--)
                                {
                                    trangles.Add(trangles[k] + 4);
                                }
                                mesh.SetTriangles(trangles, 0);
                                mesh.RecalculateNormals();
                                mesh.RecalculateBounds();
                                return mesh;
                            });

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
                    properties.filter.Allows(node) &&
                    !internal_Unchangeable(ChildNodes[id], properties);
            }
            return false;
        }


        public void SetPartToDefault()
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
                else ChildNodes[properties.id] = null;
            }
            foreach (Thing thing in ChildNodes.Values)
            {
                ((CompModularizationWeapon)thing)?.SetPartToDefault();
            }
        }


        public void SetPartToRandom()
        {
            foreach (WeaponAttachmentProperties properties in Props.attachmentProperties)
            {
                if(properties.randomThingDefWeights.NullOrEmpty())
                {
                    for (int i = 0; i < 3; i++)
                    {
                        int j = Rand.Range(0, properties.allowEmpty ? (properties.filter.AllowedDefCount + 1) : properties.filter.AllowedDefCount);
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
                else
                {
                    float count = 0;
                    properties.randomThingDefWeights.ForEach(x => count += x.count);
                    for (int i = 0; i < 3; i++)
                    {
                        float j = Rand.Range(0, count);
                        float k = 0;
                        ThingDef def = null;
                        foreach(ThingDefCountClass weight in properties.randomThingDefWeights)
                        {
                            float next = k + weight.count;
                            if (k <= j && next >= j) def = weight.thingDef;
                            k = next;
                        }
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
        }


        protected override IEnumerable<Thing> PostGenRecipe_MakeRecipeProducts(RecipeDef recipeDef, Pawn worker, List<Thing> ingredients, Thing dominantIngredient1, IBillGiver billGiver, Precept_ThingStyle precept, RecipeInvokeSource invokeSource, IEnumerable<Thing> result)
        {
            if(invokeSource == RecipeInvokeSource.products)
            {
                LongEventHandler.ExecuteWhenFinished(SetPartToDefault);
            }
            else if(invokeSource == RecipeInvokeSource.ingredients)
            {
                IEnumerable<Thing> Ingredients(IEnumerable<Thing> org)
                {
                    foreach(Thing ingredient in org) yield return ingredient;
                    foreach (string id in ChildNodes.Keys)
                    {
                        Thing part = ChildNodes[id];
                        ChildNodes[id] = null;
                        yield return part;
                    }
                }
                return Ingredients(result);
            }
            return result;
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

            for (int i = 0; i < parent.AllComps.Count; i++)
            {
                ThingComp comp = parent.AllComps[i];
                Type type = comp.GetType();
                if (type == typeof(CompChildNodeProccesser) || type == typeof(CompModularizationWeapon)) continue;
                CompProperties properties = parent.def.comps.FirstOrDefault(x => x.compClass == type);
                if(properties != null)
                {
                    try
                    {
                        if (Props.compPropertiesCreateInstanceCompType.Contains(type)) comp = (ThingComp)Activator.CreateInstance(type);
                        comp.parent = parent;
                        comp.Initialize(CompPropertiesAfterAffect(properties));
                        parent.AllComps[i] = comp;
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Could not instantiate or initialize a ThingComp: " + ex);
                    }
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
            UsingTargetPart = true;
            if (parent.holdingOwner != targetParentPart) parent.holdingOwner?.Remove(parent);
            parent.holdingOwner = container;
            UsingTargetPart = ShowTargetPart;
            NodeProccesser.NeedUpdate = true;
            NodeProccesser.UpdateNode();
        }


        protected override void Removed(NodeContainer container, string id)
        {
            //Log.Message($"container remove {container.Comp}");
            UsingTargetPart = false;
            targetParentPart = null;
            UsingTargetPart = ShowTargetPart;
            NodeProccesser.NeedUpdate = true;
            NodeProccesser.UpdateNode();
        }


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


        private readonly Dictionary<string, bool> childTreeViewOpend = new Dictionary<string, bool>();
        private readonly Dictionary<(StatDef, Thing), float> statOffsetCache = new Dictionary<(StatDef, Thing), float>();
        private readonly Dictionary<(StatDef, Thing), float> statMultiplierCache = new Dictionary<(StatDef, Thing), float>();
        private Dictionary<string, LocalTargetInfo> targetPartsWithId = new Dictionary<string, LocalTargetInfo>();
        private ThingOwner targetParentPart = null;
        private bool showTargetPart = false;
        private bool usingTargetPart = false;

        private static Type CombatExtended_CompAmmoUser = GenTypes.GetTypeInAnyAssembly("CombatExtended.CompAmmoUser");
        private static Type CombatExtended_CompFireModes = GenTypes.GetTypeInAnyAssembly("CombatExtended.CompFireModes");
        private static Type CombatExtended_StatWorker_Magazine = GenTypes.GetTypeInAnyAssembly("CombatExtended.StatWorker_Magazine");
        private static AccessTools.FieldRef<StatWorker, StatDef> StatWorker_stat = AccessTools.FieldRefAccess<StatWorker, StatDef>("stat");
        private static AccessTools.FieldRef<ThingDef, List<VerbProperties>> ThingDef_verbs = AccessTools.FieldRefAccess<ThingDef, List<VerbProperties>>("verbs");
        private static AccessTools.FieldRef<object, ThingDef> CombatExtended_CompAmmoUser_currentAmmoInt = null;
        //private static AccessTools.FieldRef<object, IList> CombatExtended_CompFireModes_availableAimModes = null;
        private static MethodInfo CombatExtended_CompAmmoUser_CurMagCount_get = null;
        private static MethodInfo CombatExtended_CompAmmoUser_CurMagCount_set = null;

        private static readonly Dictionary<Mesh, Mesh> MeshReindexed = new Dictionary<Mesh, Mesh>();
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



            void CheckAndSetDgitList<T>(ref FieldReaderDgitList<T> list, float defaultValue)
            {
                list = list ?? new FieldReaderDgitList<T>();
                list.RemoveAll(f => f == null);
                if (!list.HasDefaultValue) list.DefaultValue = defaultValue;
            }

            void CheckAndSetBoolList<T>(ref FieldReaderBoolList<T> list, bool defaultValue)
            {
                list = list ?? new FieldReaderBoolList<T>();
                list.RemoveAll(f => f == null);
                if (!list.HasDefaultValue) list.DefaultValue = defaultValue;
            }

            void CheckAndSetFiltList<T>(ref FieldReaderFiltList<T> list, bool defaultValue)
            {
                list = list ?? new FieldReaderFiltList<T>();
                list.RemoveAll(f => f == null);
                if (!list.HasDefaultValue) list.DefaultValue = defaultValue;
            }

            void CheckAndSetInstList<T>(ref FieldReaderInstList<T> list)
            {
                list = list ?? new FieldReaderInstList<T>();
                list.RemoveAll(f => f == null);
            }

            void CheckAndSetStatList(ref List<StatModifier> list)
            {
                list = list ?? new List<StatModifier>();
                list.RemoveAll(f => f == null);
            }

            CheckAndSetDgitList(ref verbPropertiesOffseter, 0);
            CheckAndSetDgitList(ref toolsOffseter, 0);
            CheckAndSetDgitList(ref compPropertiesOffseter, 0);
            CheckAndSetDgitList(ref verbPropertiesOtherPartOffseterAffectHorizon, 1);
            CheckAndSetDgitList(ref toolsOtherPartOffseterAffectHorizon, 1);

            CheckAndSetDgitList(ref verbPropertiesMultiplier, 1);
            CheckAndSetDgitList(ref toolsMultiplier, 1);
            CheckAndSetDgitList(ref compPropertiesMultiplier, 1);
            CheckAndSetDgitList(ref verbPropertiesOtherPartMultiplierAffectHorizon, 1);
            CheckAndSetDgitList(ref toolsOtherPartMultiplierAffectHorizon, 1);

            CheckAndSetBoolList(ref verbPropertiesBoolAndPatch, true);
            CheckAndSetBoolList(ref toolsBoolAndPatch, true);
            CheckAndSetBoolList(ref compPropertiesBoolAndPatch, true);
            CheckAndSetBoolList(ref verbPropertiesBoolAndPatchByOtherPart, true);
            CheckAndSetBoolList(ref toolsBoolAndPatchByOtherPart, true);

            CheckAndSetBoolList(ref verbPropertiesBoolOrPatch, false);
            CheckAndSetBoolList(ref toolsBoolOrPatch, false);
            CheckAndSetBoolList(ref compPropertiesBoolOrPatch, false);
            CheckAndSetBoolList(ref verbPropertiesBoolOrPatchByOtherPart, true);
            CheckAndSetBoolList(ref toolsBoolOrPatchByOtherPart, true);

            CheckAndSetInstList(ref verbPropertiesObjectPatch);
            CheckAndSetInstList(ref toolsObjectPatch);
            CheckAndSetInstList(ref compPropertiesObjectPatch);
            CheckAndSetFiltList(ref verbPropertiesObjectPatchByOtherPart, true);
            CheckAndSetFiltList(ref toolsObjectPatchByOtherPart, true);

            CheckAndSetStatList(ref statOffset);
            CheckAndSetStatList(ref statMultiplier);
            CheckAndSetStatList(ref statOtherPartOffseterAffectHorizon);
            CheckAndSetStatList(ref statOtherPartMultiplierAffectHorizon);

            parentDef.weaponTags = parentDef.weaponTags ?? new List<string>();

            disallowedOtherPart = disallowedOtherPart ?? new ThingFilter();
            disallowedOtherPart.ResolveReferences();

            //compPropertiesAffectCompType = compPropertiesAffectCompType ?? new List<Type>();
            //compPropertiesAffectCompType.RemoveAll(f => f == null || !typeof(ThingComp).IsAssignableFrom(f));

            compPropertiesCreateInstanceCompType = compPropertiesCreateInstanceCompType ?? new List<Type>();
            compPropertiesCreateInstanceCompType.RemoveAll(f => f == null || !typeof(ThingComp).IsAssignableFrom(f));
        }


        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
        {
            CompModularizationWeapon comp = req.Thing;
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
                    foreach (KeyValuePair<RuntimeFieldHandle, IConvertible> data in list[i])
                    {
                        FieldInfo field = FieldInfo.GetFieldFromHandle(data.Key);
                        if (snap) stringBuilder.Append("  ");
                        stringBuilder.AppendLine($"    {field.Name.Translate()} : {perfix}{data.Value}{postfix}");
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
                    foreach (KeyValuePair<RuntimeFieldHandle, object> data in list[i])
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
            VerbPropertiesOffseter = verbPropertiesMultiplier;
            if (comp != null) VerbPropertiesOffseter *= comp.VerbPropertiesMultiplier(null);
            count += listAllDgit(VerbPropertiesOffseter, "x", "");

            stringBuilder.AppendLine("toolsMultiplier".Translate().RawText + " :");
            ToolsOffseter = toolsMultiplier;
            if (comp != null) ToolsOffseter *= comp.ToolsMultiplier(null);
            count += listAllDgit(ToolsOffseter, "x", "");

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
            List<FieldReaderInst<VerbProperties>> VerbPropertiesObjectPatch = comp?.VerbPropertiesObjectPatch(null);
            if(VerbPropertiesObjectPatch != null)
            {
                foreach(FieldReaderInst<VerbProperties> fieldReader in verbPropertiesObjectPatch)
                {
                    int index = VerbPropertiesObjectPatch.FindIndex(x => x.UsedType == fieldReader.UsedType);
                    if (index < 0) VerbPropertiesObjectPatch.Add(fieldReader);
                    else VerbPropertiesObjectPatch[index] |= fieldReader;
                }
                count += listAllInst(VerbPropertiesObjectPatch, "", "");
            }

            stringBuilder.AppendLine("toolsPatch".Translate().RawText + " :");
            List<FieldReaderInst<Tool>> ToolsObjectPatch = comp?.ToolsObjectPatch(null);
            if (ToolsObjectPatch != null)
            {
                foreach (FieldReaderInst<Tool> fieldReader in toolsObjectPatch)
                {
                    int index = ToolsObjectPatch.FindIndex(x => x.UsedType == fieldReader.UsedType);
                    if (index < 0) ToolsObjectPatch.Add(fieldReader);
                    else ToolsObjectPatch[index] |= fieldReader;
                }
                count += listAllInst(ToolsObjectPatch, "", "");
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
                    listAllDgit((childComp.VerbPropertiesOffseter(null) + childComp.Props.verbPropertiesOffseter) * properties.verbPropertiesOffseterAffectHorizon, "+", "", true);

                    stringBuilder.AppendLine("  " + "toolsOffseter".Translate() + " :");
                    listAllDgit((childComp.ToolsOffseter(null) + childComp.Props.toolsOffseter) * properties.toolsOffseterAffectHorizon, "+", "", true);

                    stringBuilder.AppendLine("  " + "statOffseter".Translate() + " :");
                    foreach (StatModifier stat in childComp.Props.statOffset)
                    {
                        stringBuilder.AppendLine($"    {stat.stat.LabelCap} : +{properties.statOffsetAffectHorizon.GetStatValueFromList(stat.stat, properties.statOffsetAffectHorizonDefaultValue) * childComp.GetStatOffset(stat.stat, req.Thing)}");
                    }


                    stringBuilder.AppendLine("Multiplier".Translate() + " :");
                    stringBuilder.AppendLine("  " + "verbPropertiesMultiplier".Translate() + " :");
                    FieldReaderDgitList<VerbProperties> cacheVerbProperties = childComp.VerbPropertiesMultiplier(null);
                    cacheVerbProperties *= childComp.Props.verbPropertiesMultiplier;
                    cacheVerbProperties -= 1;
                    cacheVerbProperties.DefaultValue = 0;
                    listAllDgit(cacheVerbProperties * properties.verbPropertiesMultiplierAffectHorizon + 1, "x", "", true);

                    stringBuilder.AppendLine("  " + "toolsMultiplier".Translate() + " :");
                    FieldReaderDgitList<Tool> cacheTools = childComp.ToolsMultiplier(null);
                    cacheTools *= childComp.Props.toolsMultiplier;
                    cacheTools -= 1;
                    cacheTools.DefaultValue = 0;
                    listAllDgit(cacheTools * properties.toolsMultiplierAffectHorizon + 1, "x", "", true);

                    stringBuilder.AppendLine("  " + "statMultiplier".Translate() + " :");
                    foreach (StatModifier stat in childComp.Props.statMultiplier)
                    {
                        stringBuilder.AppendLine($"    {stat.stat.LabelCap} : x{properties.statMultiplierAffectHorizon.GetStatValueFromList(stat.stat, properties.statMultiplierAffectHorizonDefaultValue) * (childComp.GetStatMultiplier(stat.stat, req.Thing) - 1f) + 1f}");
                    }

                    stringBuilder.AppendLine("verbPropertiesPatch".Translate().RawText + " :");
                    VerbPropertiesObjectPatch = childComp.VerbPropertiesObjectPatch(null);
                    if (comp != null)
                    {
                        foreach (FieldReaderInst<VerbProperties> fieldReader in childComp.Props.verbPropertiesObjectPatch)
                        {
                            int index = VerbPropertiesObjectPatch.FindIndex(x => x.UsedType == fieldReader.UsedType);
                            if (index < 0) VerbPropertiesObjectPatch.Add(fieldReader);
                            else VerbPropertiesObjectPatch[index] |= fieldReader;
                        }
                    }
                    listAllInst(VerbPropertiesObjectPatch, "", "", true);

                    stringBuilder.AppendLine("toolsPatch".Translate().RawText + " :");
                    ToolsObjectPatch = childComp.ToolsObjectPatch(null);
                    if (comp != null)
                    {
                        foreach (FieldReaderInst<Tool> fieldReader in childComp.Props.toolsObjectPatch)
                        {
                            int index = ToolsObjectPatch.FindIndex(x => x.UsedType == fieldReader.UsedType);
                            if (index < 0) ToolsObjectPatch.Add(fieldReader);
                            else ToolsObjectPatch[index] |= fieldReader;
                        }
                    }
                    listAllInst(ToolsObjectPatch, "", "", true);
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

                    //stringBuilder.AppendLine(CheckAndMark(verbPropertiesObjectPatchByChildPart && properties.verbPropertiesObjectPatchByChildPart, "verbPropertiesObjectPatchByChildPart".Translate()));
                    //stringBuilder.AppendLine(CheckAndMark(toolsObjectPatchByChildPart && properties.toolsObjectPatchByChildPart, "toolsObjectPatchByChildPart".Translate()));
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


        public bool drawChildPartWhenOnGround = true;
        #endregion




        #region Offset
        #region Parent
        public FieldReaderDgitList<VerbProperties> verbPropertiesOffseter = new FieldReaderDgitList<VerbProperties>();


        public FieldReaderDgitList<Tool> toolsOffseter = new FieldReaderDgitList<Tool>();


        public FieldReaderDgitList<CompProperties> compPropertiesOffseter = new FieldReaderDgitList<CompProperties>();


        public List<StatModifier> statOffset = new List<StatModifier>();


        public float statOffsetDefaultValue = 1;
        #endregion


        #region OtherPart
        public FieldReaderDgitList<VerbProperties> verbPropertiesOtherPartOffseterAffectHorizon = new FieldReaderDgitList<VerbProperties>();

        public FieldReaderDgitList<Tool> toolsOtherPartOffseterAffectHorizon = new FieldReaderDgitList<Tool>();

        public List<StatModifier> statOtherPartOffseterAffectHorizon = new List<StatModifier>();

        public float statOtherPartOffseterAffectHorizonDefaultValue = 1;
        #endregion
        #endregion




        #region Multiplier
        #region Parent
        public FieldReaderDgitList<VerbProperties> verbPropertiesMultiplier = new FieldReaderDgitList<VerbProperties>();


        public FieldReaderDgitList<Tool> toolsMultiplier = new FieldReaderDgitList<Tool>();


        public FieldReaderDgitList<CompProperties> compPropertiesMultiplier = new FieldReaderDgitList<CompProperties>();


        public List<StatModifier> statMultiplier = new List<StatModifier>();


        public float statMultiplierDefaultValue = 1;
        #endregion


        #region OtherPart
        public FieldReaderDgitList<VerbProperties> verbPropertiesOtherPartMultiplierAffectHorizon = new FieldReaderDgitList<VerbProperties>();

        public FieldReaderDgitList<Tool> toolsOtherPartMultiplierAffectHorizon = new FieldReaderDgitList<Tool>();

        public List<StatModifier> statOtherPartMultiplierAffectHorizon = new List<StatModifier>();

        public float statOtherPartMultiplierAffectHorizonDefaultValue = 1;
        #endregion
        #endregion




        #region AndPatchs
        #region Parent
        public FieldReaderBoolList<VerbProperties> verbPropertiesBoolAndPatch = new FieldReaderBoolList<VerbProperties>();


        public FieldReaderBoolList<Tool> toolsBoolAndPatch = new FieldReaderBoolList<Tool>();


        public FieldReaderBoolList<CompProperties> compPropertiesBoolAndPatch = new FieldReaderBoolList<CompProperties>();
        #endregion


        #region OtherPart
        public FieldReaderBoolList<VerbProperties> verbPropertiesBoolAndPatchByOtherPart = new FieldReaderBoolList<VerbProperties>();

        public FieldReaderBoolList<Tool> toolsBoolAndPatchByOtherPart = new FieldReaderBoolList<Tool>();
        #endregion
        #endregion




        #region OrPatchs
        #region Parent
        public FieldReaderBoolList<VerbProperties> verbPropertiesBoolOrPatch = new FieldReaderBoolList<VerbProperties>();


        public FieldReaderBoolList<Tool> toolsBoolOrPatch = new FieldReaderBoolList<Tool>();


        public FieldReaderBoolList<CompProperties> compPropertiesBoolOrPatch = new FieldReaderBoolList<CompProperties>();
        #endregion


        #region OtherPart
        public FieldReaderBoolList<VerbProperties> verbPropertiesBoolOrPatchByOtherPart = new FieldReaderBoolList<VerbProperties>();

        public FieldReaderBoolList<Tool> toolsBoolOrPatchByOtherPart = new FieldReaderBoolList<Tool>();
        #endregion
        #endregion




        #region InstPatchs
        #region Parent
        public FieldReaderInstList<VerbProperties> verbPropertiesObjectPatch = new FieldReaderInstList<VerbProperties>();


        public FieldReaderInstList<Tool> toolsObjectPatch = new FieldReaderInstList<Tool>();


        public FieldReaderInstList<CompProperties> compPropertiesObjectPatch = new FieldReaderInstList<CompProperties>();
        #endregion


        #region OtherPart
        public FieldReaderFiltList<VerbProperties> verbPropertiesObjectPatchByOtherPart = new FieldReaderFiltList<VerbProperties>();

        public FieldReaderFiltList<Tool> toolsObjectPatchByOtherPart = new FieldReaderFiltList<Tool>();
        #endregion
        #endregion




        public List<WeaponAttachmentProperties> attachmentProperties = new List<WeaponAttachmentProperties>();


        public List<Type> compPropertiesCreateInstanceCompType = new List<Type>();


        public ThingFilter disallowedOtherPart = new ThingFilter();


        public string PartTexPath = null;


        private Material materialCache;
    }
}
