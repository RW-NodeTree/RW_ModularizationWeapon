using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Mono.Unix;
using RimWorld;
using RW_ModularizationWeapon.Tools;
using RW_NodeTree;
using RW_NodeTree.Rendering;
using RW_NodeTree.Tools;
using UnityEngine;
using UnityEngine.Rendering;
using Verse;

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
                CompModularizationWeapon result = this;
                CompModularizationWeapon current = ParentPart;
                while (current != null)
                {
                    result = current;
                    current = current.ParentPart;
                }
                return result;
            }
        }


        public HashSet<string> PartIDs
        {
            get
            {
                if(partIDs.Count == 0)
                {
                    foreach (WeaponAttachmentProperties properties in Props.attachmentProperties)
                    {
                        partIDs.Add(properties.id);
                    }
                }
                return partIDs;
            }
        }

        public Dictionary<string, WeaponAttachmentProperties> CurrentPartAttachmentProperties
        {
            get
            {
                if (currentPartVNode == null) UpdateCurrentPartVNode();
                foreach(WeaponAttachmentProperties properties in Props.attachmentProperties)
                {
                    if (currentPartAttachmentPropertiesCache.ContainsKey(properties.id)) continue;
                    // Log.Message($"{parent} Miss {properties.id} in CurrentPartAttachmentProperties, generating");
                    Thing thing = ChildNodes[properties.id];
                    Dictionary<WeaponAttachmentProperties, uint> mached = new Dictionary<WeaponAttachmentProperties, uint>();
                    if(thing != null)
                    {
                        foreach((QueryGroup, WeaponAttachmentProperties) record in Props.attachmentPropertiesWithQuery)
                        {
                            if (record.Item1 != null && record.Item2 != null)
                            {
                                uint currentMach = record.Item1.Mach(currentPartVNode[properties.id]);
                                if(currentMach > 0)
                                {
                                    mached.Add(record.Item2, currentMach);
                                }
                            }
                        }
                    }
                    WeaponAttachmentProperties replaced = Gen.MemberwiseClone(properties);
                    for (int i = mached.Count - 1; i >= 0; i--)
                    {
                        uint minMach = uint.MaxValue;
                        WeaponAttachmentProperties attachmentProperties = null;
                        foreach(KeyValuePair<WeaponAttachmentProperties, uint> record in mached)
                        {
                            if(minMach < record.Value || attachmentProperties == null)
                            {
                                minMach = record.Value;
                                attachmentProperties = record.Key;
                            }
                        }
                        if(attachmentProperties != null)
                        {
                            Log.Message($"{attachmentProperties.id} : {minMach}");
                            mached.Remove(attachmentProperties);
                            OptionalWeaponAttachmentProperties optional = attachmentProperties as OptionalWeaponAttachmentProperties;
                            if (optional != null)
                            {
                                foreach(FieldInfo fieldInfo in optional.UsedFields)
                                {
                                    fieldInfo.SetValue(replaced,fieldInfo.GetValue(optional));
                                }
                            }
                            else replaced = Gen.MemberwiseClone(attachmentProperties);
                        }
                    }
                    replaced.id = properties.id;
                    currentPartAttachmentPropertiesCache.Add(properties.id, replaced);
                }
                return currentPartAttachmentPropertiesCache;
            }
        }

        public Dictionary<string, WeaponAttachmentProperties> TargetPartAttachmentProperties
        {
            get
            {
                if (targetPartVNode == null) UpdateTargetPartVNode();
                foreach(WeaponAttachmentProperties properties in Props.attachmentProperties)
                {
                    if (targetPartAttachmentPropertiesCache.ContainsKey(properties.id)) continue;
                    // Log.Message($"{parent} Miss {properties.id} in TargetPartAttachmentProperties, generating");
                    Thing thing = ChildNodes[properties.id];
                    Dictionary<WeaponAttachmentProperties, uint> mached = new Dictionary<WeaponAttachmentProperties, uint>();
                    if(thing != null)
                    {
                        foreach((QueryGroup, WeaponAttachmentProperties) record in Props.attachmentPropertiesWithQuery)
                        {
                            if (record.Item1 != null && record.Item2 != null)
                            {
                                uint currentMach = record.Item1.Mach(targetPartVNode[properties.id]);
                                if(currentMach > 0)
                                {
                                    mached.Add(record.Item2, currentMach);
                                }
                            }
                        }
                    }
                    WeaponAttachmentProperties replaced = Gen.MemberwiseClone(properties);
                    for (int i = mached.Count - 1; i >= 0; i--)
                    {
                        uint minMach = uint.MaxValue;
                        WeaponAttachmentProperties attachmentProperties = null;
                        foreach(KeyValuePair<WeaponAttachmentProperties, uint> record in mached)
                        {
                            if(minMach < record.Value || attachmentProperties == null)
                            {
                                minMach = record.Value;
                                attachmentProperties = record.Key;
                            }
                        }
                        if(attachmentProperties != null)
                        {
                            Log.Message($"{attachmentProperties.id} : {minMach}");
                            mached.Remove(attachmentProperties);
                            OptionalWeaponAttachmentProperties optional = attachmentProperties as OptionalWeaponAttachmentProperties;
                            if (optional != null)
                            {
                                foreach(FieldInfo fieldInfo in optional.UsedFields)
                                {
                                    fieldInfo.SetValue(replaced,fieldInfo.GetValue(optional));
                                }
                            }
                            else replaced = Gen.MemberwiseClone(attachmentProperties);
                        }
                    }
                    replaced.id = properties.id;
                    targetPartAttachmentPropertiesCache.Add(properties.id, replaced);
                }
                return targetPartAttachmentPropertiesCache;
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
            if(PerformanceOptimizer_ComponentCache != null)
            {
                PerformanceOptimizer_ComponentCache_ResetCompCache = PerformanceOptimizer_ComponentCache.GetMethod("ResetCompCache", BindingFlags.Static | BindingFlags.Public);
            }
        }

        public override void PostPostMake()
        {
            if (Props.setRandomPartWhenCreate) SetPartToRandom();
            else SetPartToDefault();
        }


        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref targetPartsWithId, "targetPartsWithId", LookMode.Value, LookMode.LocalTargetInfo, ref targetPartsWithId_IdWorkingList, ref targetPartsWithId_TargetWorkingList);
            if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
            {
                for(int i = 0; i < Math.Min(targetPartsWithId_IdWorkingList.Count, targetPartsWithId_TargetWorkingList.Count); i ++)
                {
                    string id = targetPartsWithId_IdWorkingList[i];
                    LocalTargetInfo targetInfo = targetPartsWithId_TargetWorkingList[i];
                    targetPartsWithId.SetOrAdd(id, targetInfo);
                    CompModularizationWeapon part = targetInfo.Thing;
                    if (part != null) part.occupiers = this;
                }
            }
            //if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs) NodeProccesser.UpdateNode();
        }


        public override bool AllowStackWith(Thing other) => Props.attachmentProperties.Count == 0;

        public override bool HasPostFX(bool textureMode) => Props.drawOutlineOnRoot && (textureMode || ParentPart == null);


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
                        if(type == typeof(CompModularizationWeapon) || Props.compGetGizmosExtraAllowedCompType.Contains(type))
                        {
                            foreach(Gizmo gizmo in comp.CompGetGizmosExtra())
                            {
                                Command command = gizmo as Command;
                                if(command != null && type != typeof(CompModularizationWeapon))
                                {
                                    command.defaultLabel = part.LabelCap + " : " + command.defaultLabel;
                                    command.shrinkable = true;
                                }
                                yield return gizmo;
                            }
                        }
                    }
                }
            }
        }


        //protected override List<(string, Thing, List<RenderInfo>)> PreDrawSteep(List<(string, Thing, List<RenderInfo>)> nodeRenderingInfos, Rot4 rot, Graphic graphic, Dictionary<string, object> cachedDataToPostDrawSteep)
        //{
        //    return base.PreDrawSteep(nodeRenderingInfos, rot, graphic, cachedDataToPostDrawSteep);
        //}


        protected override List<(string, Thing, List<RenderInfo>)> PostDrawSteep(List<(string, Thing, List<RenderInfo>)> nodeRenderingInfos, Rot4 rot, Graphic graphic, Dictionary<string, object> cachedDataFromPerDrawSteep)
        {
            MaterialRequest req;
            Matrix4x4 scale = Matrix4x4.identity;
            uint texScale = NodeProccesser.Props.TextureSizeFactor;
            Material material = graphic?.MatAt(rot, this.parent) ?? BaseContent.BadMat;
            if (MaterialPool.TryGetRequestForMat(material, out req)) req.mainTex = (Props.PartTexture == BaseContent.BadTex) ? material.mainTexture : Props.PartTexture;
            else req = new MaterialRequest()
            {
                renderQueue = material.renderQueue,
                shader = material.shader,
                color = material.color,
                colorTwo = material.GetColor(ShaderPropertyIDs.ColorTwo),
                mainTex = (Props.PartTexture == BaseContent.BadTex) ? material.mainTexture : Props.PartTexture,
                maskTex = material.GetTexture(ShaderPropertyIDs.MaskTex) as Texture2D,
            };
            for (int i = 0; i < nodeRenderingInfos.Count; i++)
            {
                (string id, Thing part, List<RenderInfo> renderInfos) = nodeRenderingInfos[i];
                if (id.NullOrEmpty() && part == parent)
                {
                    //Log.Message($"ParentProccesser : {ParentProccesser}");
                    if (ParentProccesser != null)
                    {
                        for (int j = 0; j < renderInfos.Count; j++)
                        {
                            RenderInfo info = renderInfos[j];
                            if (info.material == material)
                            {
                                info.material = MaterialPool.MatFrom(req);
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
                (string id, Thing part, List<RenderInfo> renderInfos) = nodeRenderingInfos[i];
                WeaponAttachmentProperties properties = CurrentPartWeaponAttachmentPropertiesById(id);
                if (id.NullOrEmpty() && part == parent)
                {
                    if (ParentProccesser != null)
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

                                matrix[k] = scale * matrix[k];
                            }
                        }

                        renderInfos.Capacity += Props.subRenderinfInfos.Count;
                        foreach (PartSubDrawingInfo drawingInfo in Props.subRenderinfInfos)
                        {
                            req.mainTex = (drawingInfo.PartTexture == BaseContent.BadTex) ? material.mainTexture : drawingInfo.PartTexture;
                            renderInfos.Add(new RenderInfo(MeshPool.plane10, 0, scale * drawingInfo.Transfrom, MaterialPool.MatFrom(req), 0));
                        }
                    }
                }
                else if (!internal_NotDraw(part, properties) && (ParentProccesser != null || Props.drawChildPartWhenOnGround))
                {
                    if(properties != null)
                    {
                        Matrix4x4 transfrom = properties.Transfrom(texScale);
                        for (int j = 0; j < renderInfos.Count; j++)
                        {
                            bool needTransToIdentity = (CompChildNodeProccesser)part == null;
                            Matrix4x4[] matrix = renderInfos[j].matrices;
                            for (int k = 0; k < matrix.Length; k++)
                            {
                                if (needTransToIdentity)
                                {
                                    Vector4 cache = matrix[k].GetRow(0);
                                    matrix[k].SetRow(0, new Vector4(new Vector3(cache.x, cache.y, cache.z).magnitude, 0, 0, cache.w));

                                    cache = matrix[k].GetRow(1);
                                    matrix[k].SetRow(1, new Vector4(0, new Vector3(cache.x, cache.y, cache.z).magnitude, 0, cache.w));

                                    cache = matrix[k].GetRow(2);
                                    matrix[k].SetRow(2, new Vector4(0, 0, new Vector3(cache.x, cache.y, cache.z).magnitude, cache.w));
                                }
                                matrix[k] = transfrom * scale * matrix[k];
                                //matrix[k] = properties.Transfrom;
                            }
                        }
                    }
                }
                else
                {
                    renderInfos.Clear();
                }

                for (int j = 0; j < renderInfos.Count; j++)
                {
                    RenderInfo info = renderInfos[j];
                    info.mesh = ReindexedMesh(info.mesh);
                    info.CanUseFastDrawingMode = true;
                    renderInfos[j] = info;
                }
            }
            nodeRenderingInfos.SortBy(x =>
            {
                List<WeaponAttachmentProperties> props = CurrentPartAttachmentProperties.Values.ToList();
                for (int i = 0; i < props.Count; i++)
                {
                    WeaponAttachmentProperties properties = props[i];
                    if (properties.id == x.Item1) return i + properties.drawWeight * props.Count;
                }
                return -1;
            });
            return nodeRenderingInfos;
        }


        public override void PostFX(RenderTexture tar)
        {
            if(PostFXMat == null)
            {
                List<ModContentPack> runningModsListForReading = LoadedModManager.RunningModsListForReading;
                foreach (ModContentPack pack in runningModsListForReading)
                {
                    //Log.Message($"{pack.PackageId},{pack.assetBundles.loadedAssetBundles?.Count}");
                    if (pack.PackageId.Equals("rwnodetree.rwweaponmodularization") && !pack.assetBundles.loadedAssetBundles.NullOrEmpty())
                    {
                        //Log.Message($"{pack.PackageId} found, try to load shader");
                        foreach (AssetBundle assetBundle in pack.assetBundles.loadedAssetBundles)
                        {
                            //Log.Message("Loading shader");
                            Shader shader = assetBundle.LoadAsset<Shader>(@"Assets\Data\Materials\RWNodeTree.RWWeaponModularization\OutLine.shader");
                            if (shader != null)
                            {
                                PostFXMat = new Material(shader);
                                break;
                            }
                        }
                        break;
                    }
                }
            }

            if(PostFXCommandBuffer == null) PostFXCommandBuffer = new CommandBuffer();
            PostFXCommandBuffer.Clear();
            RenderTexture renderTexture = RenderTexture.GetTemporary(tar.width, tar.height, 0, tar.format);
            PostFXCommandBuffer.Blit(tar, renderTexture);
            PostFXMat.SetFloat("_EdgeSize", NodeProccesser.Props.TextureSizeFactor / 64f);
            PostFXCommandBuffer.Blit(renderTexture, tar, PostFXMat);
            Graphics.ExecuteCommandBuffer(PostFXCommandBuffer);
            RenderTexture.ReleaseTemporary(renderTexture);
        }


        public bool AllowPart(Thing part, string id = null, bool checkOccupy = true)
        {
            if (!PartIDs.Contains(id)) return false;
            if (part == ChildNodes[id]) return true;
            CompModularizationWeapon comp = part;
            if (checkOccupy && comp?.occupiers != null) return false; 
            if (allowedPartCache.TryGetValue((id,part), out bool result)) return result;
            WeaponAttachmentProperties currentPartProperties = CurrentPartWeaponAttachmentPropertiesById(id);
            WeaponAttachmentProperties targetPartProperties = TargetPartWeaponAttachmentPropertiesById(id);
            //if (Prefs.DevMode) Log.Message($"properties : {properties}");
            result = currentPartProperties != null && targetPartProperties != null;
            if (!result) return false;
            if (part == null) return currentPartProperties.allowEmpty && targetPartProperties.allowEmpty;

            result &=
                currentPartProperties.filter.Allows(part) &&
                targetPartProperties.filter.Allows(part) &&
                !internal_Unchangeable(ChildNodes[id], currentPartProperties) &&
                !internal_Unchangeable(ChildNodes[id], targetPartProperties);
            allowedPartCache.Add((id,part), result);
            return result;
        }


        protected override bool AllowNode(Thing node, string id = null) => AllowPart(node, id, false);


        public void SetPartToDefault()
        {
            if (AllowSwap)
            {
                List<WeaponAttachmentProperties> props = CurrentPartAttachmentProperties.Values.ToList();
                for (int i = 0; i < props.Count; i++)
                {
                    WeaponAttachmentProperties properties = props[i];
                    ThingDef def = properties.defultThing;
                    if (def != null)
                    {
                        Thing thing = ThingMaker.MakeThing(def, GenStuff.RandomStuffFor(def));
                        thing.TryGetComp<CompQuality>()?.SetQuality(QualityUtility.GenerateQualityRandomEqualChance(), ArtGenerationContext.Colony);
                        SetTargetPart(properties.id, thing);
                    }
                    else SetTargetPart(properties.id, null);
                    ChildNodes[properties.id]?.Destroy();
                }
                foreach (Thing thing in targetPartsWithId.Values)
                {
                    CompModularizationWeapon comp = thing;
                    if (comp?.Props.setRandomPartWhenCreate ?? false)
                    {
                        comp?.SetPartToDefault();
                    }
                }
                SwapTargetPart();
                ClearTargetPart();
            }
        }


        public void SetPartToRandom()
        {

            //Console.WriteLine($"====================================   {parent}.SetPartToRandom End   ====================================");
            if (AllowSwap)
            {
                List<WeaponAttachmentProperties> props = CurrentPartAttachmentProperties.Values.ToList();
                // Console.WriteLine($"==================================== {parent}.SetPartToRandom Start   ====================================");
                for (int i = 0; i < props.Count; i++)
                {
                    WeaponAttachmentProperties properties = props[i];
                    if (properties.randomThingDefWeights.NullOrEmpty())
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            int k = Rand.Range(0, properties.allowEmpty ? (properties.filter.AllowedDefCount + properties.randomToEmptyWeight) : properties.filter.AllowedDefCount);
                            ThingDef def = k < properties.filter.AllowedDefCount ? properties.filter.AllowedThingDefs.ToList()[k] : null;
                            if (def != null)
                            {
                                Thing thing = ThingMaker.MakeThing(def, GenStuff.RandomStuffFor(def));
                                thing.TryGetComp<CompQuality>()?.SetQuality(QualityUtility.GenerateQualityRandomEqualChance(), ArtGenerationContext.Outsider);
                                if (SetTargetPart(properties.id, thing))
                                {
                                    ChildNodes[properties.id]?.Destroy();
                                    break;
                                }
                            }
                            else if(SetTargetPart(properties.id, null))
                            {
                                ChildNodes[properties.id]?.Destroy();
                                break;
                            }
                        }
                    }
                    else
                    {
                        float count = properties.allowEmpty ? properties.randomToEmptyWeight : 0;
                        properties.randomThingDefWeights.ForEach(x => count += x.count);
                        for (int j = 0; j < 3; j++)
                        {
                            float k = Rand.Range(0, count);
                            float l = 0;
                            ThingDef def = null;
                            foreach (ThingDefCountClass weight in properties.randomThingDefWeights)
                            {
                                float next = l + weight.count;
                                if (l <= k && next >= k) def = weight.thingDef;
                                l = next;
                            }
                            if (def != null)
                            {
                                Thing thing = ThingMaker.MakeThing(def, GenStuff.RandomStuffFor(def));
                                thing.TryGetComp<CompQuality>()?.SetQuality(QualityUtility.GenerateQualityRandomEqualChance(), ArtGenerationContext.Outsider);
                                if (SetTargetPart(properties.id, thing))
                                {
                                    ChildNodes[properties.id]?.Destroy();
                                    break;
                                }
                            }
                            else if(SetTargetPart(properties.id, null))
                            {
                                ChildNodes[properties.id]?.Destroy();
                                break;
                            }
                        }
                    }
                    // Console.WriteLine($"{parent}[{properties.id}]:{ChildNodes[properties.id]},{GetTargetPart(properties.id)}");
                }
                foreach (Thing thing in targetPartsWithId.Values)
                {
                    CompModularizationWeapon comp = thing;
                    if (comp?.Props.setRandomPartWhenCreate ?? false)
                    {
                        comp?.SetPartToRandom();
                    }
                }
                SwapTargetPart();
                ClearTargetPart();
            }
        }


        protected override IEnumerable<Thing> PostGenRecipe_MakeRecipeProducts(RecipeDef recipeDef, Pawn worker, List<Thing> ingredients, Thing dominantIngredient1, IBillGiver billGiver, Precept_ThingStyle precept, RecipeInvokeSource invokeSource, IEnumerable<Thing> result)
        {
            if(invokeSource == RecipeInvokeSource.products)
            {
                SetPartToDefault();
            }
            else if(invokeSource == RecipeInvokeSource.ingredients && AllowSwap)
            {
                IEnumerable<Thing> Ingredients(IEnumerable<Thing> org)
                {

                    foreach (Thing ingredient in org) yield return ingredient;
                    foreach (string id in ChildNodes.Keys)
                    {
                        Thing part = ChildNodes[id];
                        SetTargetPart(id, null);
                        yield return part;
                    }
                    SwapTargetPart();
                    ClearTargetPart();
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

            if (Props.compPropertiesResoveCrosseReferenceType.Contains(compProperties.compClass)) compProperties.ResolveReferences(parent.def);
            return compProperties;
        }


        public List<CompProperties> AllExtraCompProperties()
        {
            List<CompProperties> result = new List<CompProperties>();
            NodeContainer container = ChildNodes;
            for (int i = 0; i < container.Count; i++)
            {
                CompModularizationWeapon comp = container[i];
                WeaponAttachmentProperties properties = CurrentPartWeaponAttachmentPropertiesById(container[(uint)i]);
                if(comp != null && properties != null)
                {
                    result.AddRange(
                        from x 
                        in comp.Props.extraComp 
                        where properties.allowedExtraCompType.Contains(x.compClass) 
                            && result.FirstOrDefault(c => c.compClass == x.compClass) == null 
                        select x
                    );
                    result.AddRange(
                        from x
                        in comp.AllExtraCompProperties()
                        where properties.allowedExtraCompType.Contains(x.compClass)
                            && result.FirstOrDefault(c => c.compClass == x.compClass) == null
                        select x
                    );
                }
            }
            return result;
        }

        private void MarkTargetPartChanged()
        {

            Map map = parent.MapHeld;
            if (map != null &&
                CombatExtended_CompAmmoUser != null &&
                CombatExtended_CompAmmoUser_currentAmmoInt != null &&
                CombatExtended_CompAmmoUser_CurMagCount_get != null &&
                CombatExtended_CompAmmoUser_CurMagCount_set != null
                )
            {
                //Console.WriteLine(parent.PositionHeld);
                ThingComp currentComp = cachedThingComps.Find(x => CombatExtended_CompAmmoUser.IsAssignableFrom(x.GetType()));
                if (currentComp != null)
                {
                    ThingDef def = CombatExtended_CompAmmoUser_currentAmmoInt(currentComp);
                    int count = (int)CombatExtended_CompAmmoUser_CurMagCount_get.Invoke(currentComp, null);
                    if (def != null && count > 0)
                    {
                        Thing thing = ThingMaker.MakeThing(def, null);
                        thing.stackCount = count;
                        CombatExtended_CompAmmoUser_CurMagCount_set.Invoke(currentComp, new object[] { 0 });
                        GenThing.TryDropAndSetForbidden(thing, parent.PositionHeld, map, ThingPlaceMode.Near, out _, false);
                    }
                }
            }
            cachedThingComps.Clear();
            cachedCompProperties.Clear();
            statOffsetCache_TargetPart.Clear();
            statMultiplierCache_TargetPart.Clear();
            toolsCache_TargetPart.Clear();
            verbPropertiesCache_TargetPart.Clear();
            if (AllowSwap) UpdateTargetPartVNode();
        }

        /// <summary>
        /// Active method for testing `targetPartChanged`
        /// </summary>
        /// <returns>`targetPartChanged` of target child</returns>
        private bool CheckAndSetTargetCache()
        {
            foreach (Thing thing in ChildNodes.Values)
                if (((CompModularizationWeapon)thing)?.CheckAndSetTargetCache() ?? false)
                    targetPartChanged = true;
            foreach (Thing thing in targetPartsWithId.Values)
                if (((CompModularizationWeapon)thing)?.CheckAndSetTargetCache() ?? false)
                    targetPartChanged = true;
            if (targetPartChanged) MarkTargetPartChanged();
            return targetPartChanged;
        }


        private bool CheckTargetVaild(bool deSpawn)
        {
            bool result = true;
            CompChildNodeProccesser proccesser = NodeProccesser;
            foreach (Thing thing in ChildNodes.Values)
                if (!(((CompModularizationWeapon)thing)?.CheckTargetVaild(deSpawn) ?? true))
                    result = false;
            foreach (string id in this.PartIDs)
            {
                if (targetPartsWithId.TryGetValue(id, out LocalTargetInfo target))
                {
                    if (target.HasThing && target.Thing.holdingOwner != null)
                    {
                        if (target.Thing.Spawned && parent.MapHeld != null && target.Thing.Map == parent.MapHeld)
                        {
                            if (deSpawn) target.Thing.DeSpawn();
                        }
                        else
                        {
                            SetTargetPart(id, ChildNodes[id]);
                            ((CompModularizationWeapon)target.Thing)?.UpdateTargetPartVNode();
                            result = false;
                            continue;
                        }
                    }
                    if (proccesser.AllowNode(target.Thing, id))
                    {
                        result = (((CompModularizationWeapon)target.Thing)?.CheckTargetVaild(deSpawn) ?? true) && result;
                    }
                    else
                    {
                        SetTargetPart(id, ChildNodes[id]);
                        ((CompModularizationWeapon)target.Thing)?.UpdateTargetPartVNode();
                        result = false;
                    }
                }
                else
                {
                    result = (((CompModularizationWeapon)ChildNodes[id])?.CheckTargetVaild(deSpawn) ?? true) && result;
                }
            }
            if (!result) UpdateTargetPartVNode();
            return result;
        }


        private void SwapAttachmentPropertiesCacheAndXmlNode()
        {
            Dictionary<string, WeaponAttachmentProperties> attachmentPropertiesCache = new Dictionary<string, WeaponAttachmentProperties>(this.currentPartAttachmentPropertiesCache);
            this.currentPartAttachmentPropertiesCache.Clear();
            this.currentPartAttachmentPropertiesCache.AddRange(this.targetPartAttachmentPropertiesCache);
            this.targetPartAttachmentPropertiesCache.Clear();
            this.targetPartAttachmentPropertiesCache.AddRange(attachmentPropertiesCache);
            VNode targetPartXmlNode = this.targetPartVNode;
            this.targetPartVNode = this.currentPartVNode;
            this.currentPartVNode = targetPartXmlNode;
            foreach (Thing thing in ChildNodes.Values)
            {
                CompModularizationWeapon comp = thing;
                comp?.SwapAttachmentPropertiesCacheAndXmlNode();
            }
            foreach (Thing thing in targetPartsWithId.Values)
            {
                CompModularizationWeapon comp = thing;
                comp?.SwapAttachmentPropertiesCacheAndXmlNode();
            }
        }

        protected override void PreUpdateNode(CompChildNodeProccesser actionNode, Dictionary<string, object> cachedDataToPostUpatde, Dictionary<string, Thing> prveChilds, out bool blockEvent, out bool notUpdateTexture)
        {
            // if(stopWatch.IsRunning) stopWatch.Restart();
            // else stopWatch.Start();
            // long ct = 0;
            // long lt = 0;
            blockEvent = false;
            notUpdateTexture = false;
            foreach (KeyValuePair<string, Thing> keyValue in prveChilds)
            {
                ChildNodes[keyValue.Key] = keyValue.Value;
            }
            CompModularizationWeapon root = RootPart;
            bool occupyed = root.occupiers != null;
            
            // ct = stopWatch.ElapsedTicks;
            // Log.Message($"{parent}.PreUpdate  swap: {swap}; occupiers: {occupiers?.parent}; pass cpu ticks 1 {ct}, dt = {ct - lt}");
            // lt = ct;

            if (root == this)
            {
                while (!CheckTargetVaild(!occupyed)) continue;
                CheckAndSetTargetCache();
            }

            // ct = stopWatch.ElapsedTicks;
            // Log.Message($"{parent}.PreUpdate  swap: {swap}; occupiers: {occupiers?.parent}; pass cpu ticks 2 {ct}, dt = {ct - lt}");
            // lt = ct;

            if (occupyed || !swap)
            {
                if (root == this) UpdateCurrentPartVNode();
                _ = CurrentPartAttachmentProperties;
                _ = TargetPartAttachmentProperties;
                
                // ct = stopWatch.ElapsedTicks;
                // Log.Message($"{parent}.PreUpdate  swap: {swap}; occupiers: {occupiers?.parent}; pass cpu ticks 3 {ct}, dt = {ct - lt}");
                // lt = ct;

                return;
            }
            
            // ct = stopWatch.ElapsedTicks;
            // Log.Message($"{parent}.PreUpdate  swap: {swap}; occupiers: {occupiers?.parent}; pass cpu ticks 3 {ct}, dt = {ct - lt}");
            // lt = ct;

            //Console.WriteLine($"==================================== {parent}.PreUpdateNode Start   ====================================");
            Map map = parent.MapHeld;
            foreach (string id in this.PartIDs)
            {
                //Console.WriteLine($"{parent}[{id}]:{ChildNodes[id]},{GetTargetPart(id)}");
                CompChildNodeProccesser proccesser;
                CompModularizationWeapon weaponComp;
                Thing prev = ChildNodes[id];
                if (!targetPartsWithId.TryGetValue(id, out LocalTargetInfo target))
                {

                    proccesser = prev;
                    if (proccesser != null) proccesser.NeedUpdate = true;

                    weaponComp = prev;
                    if (weaponComp != null) weaponComp.swap = true;
                    continue;
                }

                ChildNodes[id] = target.Thing;
                SetTargetPart(id, prev);

                //Sync child swap state

                proccesser = prev;
                if (proccesser != null) proccesser.NeedUpdate = true;

                weaponComp = prev;
                if (weaponComp != null) weaponComp.swap = true;

                proccesser = target.Thing;
                if (proccesser != null) proccesser.NeedUpdate = true;

                weaponComp = target.Thing;
                if (weaponComp != null) weaponComp.swap = true;

                if(map != null && prev != null)
                {
                    int index = map.cellIndices.CellToIndex(prev.Position);
                    if (index < map.cellIndices.NumGridCells && index >= 0)
                    {
                        prev.SpawnSetup(map, false);
                    }
                }
            }

            // ct = stopWatch.ElapsedTicks;
            // Log.Message($"{parent}.PreUpdate  swap: {swap}; occupiers: {occupiers?.parent}; pass cpu ticks 4 {ct}, dt = {ct - lt}");
            // lt = ct;

            if (AllowSwap) SwapAttachmentPropertiesCacheAndXmlNode();
            _ = CurrentPartAttachmentProperties;
            _ = TargetPartAttachmentProperties;
            //Console.WriteLine($"====================================   {parent}.PreUpdateNode End   ====================================");
            // ct = stopWatch.ElapsedTicks;
            // Log.Message($"{parent}.PreUpdate  swap: {swap}; occupiers: {occupiers?.parent}; pass cpu ticks 5 {ct}, dt = {ct - lt}");
            // lt = ct;

            return;
        }


        protected override void PostUpdateNode(CompChildNodeProccesser actionNode, Dictionary<string, object> cachedDataFromPerUpdate, Dictionary<string, Thing> prveChilds, out bool blockEvent, out bool notUpdateTexture)
        {
            // if(stopWatch.IsRunning) stopWatch.Restart();
            // else stopWatch.Start();
            // long ct = 0;
            // long lt = 0;

            blockEvent = false;
            notUpdateTexture = false;
            bool swaping = RootPart.occupiers == null && swap;

            if (swaping)
            {
                Dictionary<(StatDef, Thing), float> statOffsetCache = new Dictionary<(StatDef, Thing), float>(this.statOffsetCache);
                this.statOffsetCache.Clear();
                this.statOffsetCache.AddRange(this.statOffsetCache_TargetPart);
                this.statOffsetCache_TargetPart.Clear();
                this.statOffsetCache_TargetPart.AddRange(statOffsetCache);
                Dictionary<(StatDef, Thing), float> statMultiplierCache = new Dictionary<(StatDef, Thing), float>(this.statMultiplierCache);
                this.statMultiplierCache.Clear();
                this.statMultiplierCache.AddRange(this.statMultiplierCache_TargetPart);
                this.statMultiplierCache_TargetPart.Clear();
                this.statMultiplierCache_TargetPart.AddRange(statMultiplierCache);
                Dictionary<Type, List<Tool>> toolsCache = new Dictionary<Type, List<Tool>>(this.toolsCache);
                this.toolsCache.Clear();
                this.toolsCache.AddRange(this.toolsCache_TargetPart);
                this.toolsCache_TargetPart.Clear();
                this.toolsCache_TargetPart.AddRange(toolsCache);
                Dictionary<Type, List<VerbProperties>> verbPropertiesCache = new Dictionary<Type, List<VerbProperties>>(this.verbPropertiesCache);
                this.verbPropertiesCache.Clear();
                this.verbPropertiesCache.AddRange(this.verbPropertiesCache_TargetPart);
                this.verbPropertiesCache_TargetPart.Clear();
                this.verbPropertiesCache_TargetPart.AddRange(verbPropertiesCache);
                
                // notUpdateTexture = !targetPartChanged && this.cachedGraphic_ChildNode != null;
                // if (notUpdateTexture) NodeProccesser.ResetRenderedTexture();
                // Graphic_ChildNode cachedGraphic_ChildNode = this.cachedGraphic_ChildNode ?? new Graphic_ChildNode(NodeProccesser, parent.Graphic.GetGraphic_ChildNode().SubGraphic);
                // this.cachedGraphic_ChildNode = parent.Graphic.GetGraphic_ChildNode();
                // parent.Graphic.SetGraphic_ChildNode(cachedGraphic_ChildNode);
                // _ = cachedGraphic_ChildNode.MatSingle;

            }
            else
            {
                this.statOffsetCache.Clear();
                this.statMultiplierCache.Clear();
                this.toolsCache.Clear();
                this.verbPropertiesCache.Clear();
            }

            // ct = stopWatch.ElapsedTicks;
            // Log.Message($"{parent}.PostUpdate swap: {swap}; occupiers: {occupiers?.parent}; pass cpu ticks 1 {ct}, dt = {ct - lt}");
            // lt = ct;

            List<(Task<CompProperties>, ThingComp, bool)> cachedTask = new List<(Task<CompProperties>, ThingComp, bool)>(parent.def.comps.Count);
            
            List<CompProperties> cachedCompProperties = swaping ? new List<CompProperties>(this.cachedCompProperties) : new List<CompProperties>();
            List<ThingComp> cachedThingComps = swaping ? new List<ThingComp>(this.cachedThingComps) : new List<ThingComp>();
            List<ThingComp> allComps = ThingWithComps_comps(parent);
            List<ThingComp> allCompsCopy = new List<ThingComp>(allComps);
            if (swaping)
            {
                this.cachedThingComps.Clear();
                this.cachedCompProperties.Clear();
                Task allCompsTask = Task.Run(
                    ()=>
                    allComps.RemoveAll(x => parent.def.comps.FirstOrDefault(c => c.compClass == x.GetType()) == null)
                );
                Task cachedThingCompsTask = Task.Run(
                    ()=>
                    this.cachedThingComps.AddRange(from x in allCompsCopy where parent.def.comps.FirstOrDefault(c => c.compClass == x.GetType()) == null select x)
                );
                
                Task cachedCompPropertiesTask = Task.Run(
                    ()=>
                    this.cachedCompProperties.AddRange(from x in allCompsCopy where parent.def.comps.FirstOrDefault(c => c.compClass == x.GetType()) != null select x.props)
                );
                allCompsTask.Wait();
                cachedThingCompsTask.Wait();
                cachedCompPropertiesTask.Wait();
            }
            else allComps.RemoveAll(x => parent.def.comps.FirstOrDefault(c => c.compClass == x.GetType()) == null);
            
            // ct = stopWatch.ElapsedTicks;
            // Log.Message($"{parent}.PostUpdate swap: {swap}; occupiers: {occupiers?.parent}; pass cpu ticks 2 {ct}, dt = {ct - lt}");
            // lt = ct;

            for (int i = 0; i < allComps.Count; i++)
            {
                ThingComp comp = allComps[i];
                Type type = comp.GetType();
                if (type == typeof(CompChildNodeProccesser) || type == typeof(CompModularizationWeapon)) continue;

                CompProperties properties = cachedCompProperties.FirstOrDefault(x => x.compClass == type);
                bool useCache = properties != null;
                if (!useCache) properties = parent.def.comps.FirstOrDefault(x => x.compClass == type);

                if (properties != null)
                {
                    try
                    {
                        if (Props.compPropertiesCreateInstanceCompType.Contains(type))
                        {
                            if (swaping && this.cachedThingComps.Find(x => x.GetType() == type) == null)
                            {
                                this.cachedThingComps.Add(comp);
                                this.cachedCompProperties.RemoveAll(x => x.compClass == type);
                            }
                            comp = cachedThingComps.Find(x => x.GetType() == type);
                            if(comp != null)
                            {
                                allComps[i] = comp;
                                continue;
                            }
                            comp = (ThingComp)Activator.CreateInstance(type);
                            comp.parent = parent;
                        }
                        if (useCache)
                        {
                            if (Props.compPropertiesInitializeCompType.Contains(type) || Props.compPropertiesCreateInstanceCompType.Contains(type)) comp.Initialize(properties);
                            else comp.props = properties;
                        }
                        //else
                        //{
                        //    if (Props.compPropertiesInitializeCompType.Contains(type) || Props.compPropertiesCreateInstanceCompType.Contains(type)) comp.Initialize(CompPropertiesAfterAffect(properties));
                        //    else comp.props = CompPropertiesAfterAffect(properties);
                        //}
                        else cachedTask.Add((Task.Run(() => CompPropertiesAfterAffect(properties)), comp, Props.compPropertiesInitializeCompType.Contains(type) || Props.compPropertiesCreateInstanceCompType.Contains(type)));
                        allComps[i] = comp;
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Could not instantiate or initialize a ThingComp: " + ex);
                    }
                }
            }

            // ct = stopWatch.ElapsedTicks;
            // Log.Message($"{parent}.PostUpdate swap: {swap}; occupiers: {occupiers?.parent}; pass cpu ticks 3 {ct}, dt = {ct - lt}");
            // lt = ct;

            foreach (CompProperties prop in AllExtraCompProperties())
            {
                if (allComps.FirstOrDefault(x => x.GetType() == prop.compClass) == null)
                {
                    ThingComp comp = cachedThingComps.FirstOrDefault(x => x.GetType() == prop.compClass);
                    if (comp == null)
                    {
                        comp = (ThingComp)Activator.CreateInstance(prop.compClass);
                        cachedTask.Add((Task.Run(() => CompPropertiesAfterAffect(prop)), comp, true));
                        //comp.Initialize(CompPropertiesAfterAffect(prop));
                    }
                    comp.parent = parent;
                    allComps.Add(comp);
                    //comp.props.LogAllField();
                }
            }


            // ct = stopWatch.ElapsedTicks;
            // Log.Message($"{parent}.PostUpdate swap: {swap}; occupiers: {occupiers?.parent}; pass cpu ticks 4 {ct}, dt = {ct - lt}");
            // lt = ct;

            foreach ((Task<CompProperties>, ThingComp, bool) info in cachedTask)
            {
                if (info.Item2 != null)
                {
                    //info.Item1.Wait();
                    if (info.Item3) info.Item2.Initialize(info.Item1.Result);
                    else info.Item2.props = info.Item1.Result;
                }
            }


            if (PerformanceOptimizer_ComponentCache != null && PerformanceOptimizer_ComponentCache_ResetCompCache != null)
            {
                //Log.Message($"PerformanceOptimizer_ComponentCache_ResetCompCache : {PerformanceOptimizer_ComponentCache_ResetCompCache}");
                PerformanceOptimizer_ComponentCache_ResetCompCache.Invoke(null, new object[] { parent });
            }
            
            // ct = stopWatch.ElapsedTicks;
            // Log.Message($"{parent}.PostUpdate swap: {swap}; occupiers: {occupiers?.parent}; pass cpu ticks 5 {ct}, dt = {ct - lt}");
            // lt = ct;

            if (!swaping)
            {
                foreach (FieldInfo fieldInfo in Props.ThingCompCopiedMember)
                {
                    ThingComp src = allCompsCopy.Find(x => fieldInfo.DeclaringType.IsAssignableFrom(x.GetType()));
                    ThingComp tar = allComps.Find(x => fieldInfo.DeclaringType.IsAssignableFrom(x.GetType()));
                    if (src != null && tar != null) fieldInfo.SetValue(tar, fieldInfo.GetValue(src));
                }
                return;
            }
            
            // ct = stopWatch.ElapsedTicks;
            // Log.Message($"{parent}.PostUpdate swap: {swap}; occupiers: {occupiers?.parent}; pass cpu ticks 6 {ct}, dt = {ct - lt}");
            // lt = ct;

            swap = false;
            NeedUpdate = false;
            targetPartChanged = false;
            // if (RootPart == this && occupiers == null) UpdateTargetPartXmlTree();
            return;
        }


        private void Private_SetChildPostion(IntVec3? pos = null)
        {
            if (pos == null) pos = parent.PositionHeld;
            foreach (Thing thing in ChildNodes.Values)
            {
                if(thing != null)
                {
                    thing.Position = pos.Value;
                    ((CompModularizationWeapon)thing)?.Private_SetChildPostion(pos);
                }
            }
            foreach (Thing thing in targetPartsWithId.Values)
            {
                if(thing != null)
                {
                    thing.Position = pos.Value;
                    ((CompModularizationWeapon)thing)?.Private_SetChildPostion(pos);
                }
            }
        }


        public void SetChildPostion() => RootPart.Private_SetChildPostion();


        private void Private_SetChildPostionInvalid(IntVec3? pos = null)
        {
            if (pos == null) pos = parent.PositionHeld;
            foreach (Thing thing in ChildNodes.Values)
            {
                if(thing != null)
                {
                    thing.Position = pos.Value;
                    ((CompModularizationWeapon)thing)?.Private_SetChildPostionInvalid(pos);
                }
            }
            foreach (Thing thing in targetPartsWithId.Values)
            {
                if(thing != null)
                {
                    thing.Position = pos.Value;
                    ((CompModularizationWeapon)thing)?.Private_SetChildPostionInvalid(pos);
                }
            }
        }


        public void SetChildPostionInvalid() => RootPart.Private_SetChildPostionInvalid();


        internal static Mesh ReindexedMesh(Mesh meshin)
        {
            return MeshReindexed.GetOrNewWhenNull(meshin, () =>
            {
                if (MeshReindexed.ContainsValue(meshin))
                {
                    return meshin;
                }
                Mesh mesh = new Mesh();
                mesh.name = meshin.name + " Reindexed";

                List<Vector3> vert = new List<Vector3>(meshin.vertices);
                vert.AddRange(vert);
                mesh.vertices = vert.ToArray();

                List<Vector2> uv = new List<Vector2>(meshin.uv);
                uv.AddRange(uv);
                mesh.uv = uv.ToArray();

                List<int> trangles = new List<int>(meshin.GetTriangles(0));
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
        }

        public string UniqueVerbOwnerID()
        {
            return $"CompModularizationWeapon_VerbTracker_{parent}";
        }

        public bool VerbsStillUsableBy(Pawn p)
        {
            return true;
        }
        
        public WeaponAttachmentProperties CurrentPartWeaponAttachmentPropertiesById(string id)
        {
            if(!id.NullOrEmpty()) return CurrentPartAttachmentProperties.TryGetValue(id);
            return null;
        }
        
        public WeaponAttachmentProperties TargetPartWeaponAttachmentPropertiesById(string id)
        {
            if(!id.NullOrEmpty()) return TargetPartAttachmentProperties.TryGetValue(id);
            return null;
        }

        #region operator
        public static implicit operator Thing(CompModularizationWeapon node)
        {
            return node?.parent;
        }

        public static implicit operator CompModularizationWeapon(Thing thing)
        {
            List<ThingComp> comps = (thing as ThingWithComps)?.AllComps;
            if (comps == null || comps.Count < 2) return null;
            CompModularizationWeapon result = comps[1] as CompModularizationWeapon;
            if(result != null) return result;
            retry:;
            if (!compLoadingCache.TryGetValue(thing.def, out int index))
            {
                int i = 0;
                for (; i < comps.Count; i++)
                {
                    result = comps[i] as CompModularizationWeapon;
                    if (result != null) break;
                }
                if (result != null)
                {
                    index = i;
                }
                else
                {
                    index = -1;
                }
                compLoadingCache.Add(thing.def,index);
            }
            else if(index >= 0)
            {
                if(index >= comps.Count)
                {
                    compLoadingCache.Remove(thing.def);
                    goto retry;
                }
                result = comps[index] as CompModularizationWeapon;
                if(result == null)
                {
                    compLoadingCache.Remove(thing.def);
                    goto retry;
                }
            }
            return result;
        }
        #endregion


        internal CompModularizationWeapon occupiers = null;
        private bool swap = false;
        private bool targetPartChanged = false;
        private VNode targetPartVNode = null;
        private VNode currentPartVNode = null;
        // private Graphic_ChildNode cachedGraphic_ChildNode = null;
        private List<string> targetPartsWithId_IdWorkingList = new List<string>();
        private List<LocalTargetInfo> targetPartsWithId_TargetWorkingList = new List<LocalTargetInfo>();
        private Dictionary<string, LocalTargetInfo> targetPartsWithId = new Dictionary<string, LocalTargetInfo>(); //part difference table
        private readonly HashSet<string> partIDs = new HashSet<string>();
        private readonly List<ThingComp> cachedThingComps = new List<ThingComp>();
        private readonly List<CompProperties> cachedCompProperties = new List<CompProperties>();
        private readonly Dictionary<string, bool> childTreeViewOpend = new Dictionary<string, bool>();
        private readonly Dictionary<(string, Thing), bool> allowedPartCache = new Dictionary<(string, Thing), bool>();
        private readonly Dictionary<string, WeaponAttachmentProperties> currentPartAttachmentPropertiesCache = new Dictionary<string, WeaponAttachmentProperties>();
        private readonly Dictionary<string, WeaponAttachmentProperties> targetPartAttachmentPropertiesCache = new Dictionary<string, WeaponAttachmentProperties>();
        private readonly Dictionary<(StatDef, Thing), float> statOffsetCache = new Dictionary<(StatDef, Thing), float>();
        private readonly Dictionary<(StatDef, Thing), float> statMultiplierCache = new Dictionary<(StatDef, Thing), float>();
        private readonly Dictionary<(StatDef, Thing), float> statOffsetCache_TargetPart = new Dictionary<(StatDef, Thing), float>();
        private readonly Dictionary<(StatDef, Thing), float> statMultiplierCache_TargetPart = new Dictionary<(StatDef, Thing), float>();
        private readonly Dictionary<Type, List<Tool>> toolsCache = new Dictionary<Type, List<Tool>>();
        private readonly Dictionary<Type, List<VerbProperties>> verbPropertiesCache = new Dictionary<Type, List<VerbProperties>>();
        private readonly Dictionary<Type, List<Tool>> toolsCache_TargetPart = new Dictionary<Type, List<Tool>>();
        private readonly Dictionary<Type, List<VerbProperties>> verbPropertiesCache_TargetPart = new Dictionary<Type, List<VerbProperties>>();

        private static Type CombatExtended_CompAmmoUser = GenTypes.GetTypeInAnyAssembly("CombatExtended.CompAmmoUser");
        private static Type CombatExtended_StatWorker_Magazine = GenTypes.GetTypeInAnyAssembly("CombatExtended.StatWorker_Magazine");
        private static Type PerformanceOptimizer_ComponentCache = GenTypes.GetTypeInAnyAssembly("PerformanceOptimizer.ComponentCache");
        private static AccessTools.FieldRef<StatWorker, StatDef> StatWorker_stat = AccessTools.FieldRefAccess<StatWorker, StatDef>("stat");
        private static AccessTools.FieldRef<ThingDef, List<VerbProperties>> ThingDef_verbs = AccessTools.FieldRefAccess<ThingDef, List<VerbProperties>>("verbs");
        private static AccessTools.FieldRef<ThingWithComps, List<ThingComp>> ThingWithComps_comps = AccessTools.FieldRefAccess<ThingWithComps, List<ThingComp>>("comps");
        private static AccessTools.FieldRef<object, ThingDef> CombatExtended_CompAmmoUser_currentAmmoInt = null;
        //private static AccessTools.FieldRef<object, IList> CombatExtended_CompFireModes_availableAimModes = null;
        private static MethodInfo CombatExtended_CompAmmoUser_CurMagCount_get = null;
        private static MethodInfo CombatExtended_CompAmmoUser_CurMagCount_set = null;
        private static MethodInfo PerformanceOptimizer_ComponentCache_ResetCompCache = null;
        private static Material PostFXMat = null;
        private static CommandBuffer PostFXCommandBuffer = null;


        private static readonly Dictionary<Mesh, Mesh> MeshReindexed = new Dictionary<Mesh, Mesh>();
        private static readonly Dictionary<ThingDef,int> compLoadingCache = new Dictionary<ThingDef,int>();
        
        internal static readonly Stopwatch stopWatch = new Stopwatch();
    }

    /// <summary>
    /// this type is parmerters holder of the type `CompModularizationWeapon`, it define all parmerters that can write in XML.
    /// </summary>
    [StaticConstructorOnStartup]
    public class CompProperties_ModularizationWeapon : CompProperties
    {

        /// <summary>
        /// Texture of `PartTexMaterial`
        /// </summary>
        public Texture2D PartTexture
        {
            get
            {
                if(partTexCache == null && !PartTexPath.NullOrEmpty()) partTexCache = ContentFinder<Texture2D>.Get(PartTexPath) ?? BaseContent.BadTex;
                return partTexCache;
            }
        }


        public List<FieldInfo> ThingCompCopiedMember
        {
            get
            {
                if(fieldInfosOfThingCompCopiedMember.NullOrEmpty())
                {
                    fieldInfosOfThingCompCopiedMember = new List<FieldInfo>(thingCompCopiedMember.Count);
                    foreach(string str in thingCompCopiedMember)
                    {
                        if(str.NullOrEmpty()) continue;
                        string[] splited = str.Split('.');
                        string typeName = splited[0];
                        for(int i = 1; i < splited.Length - 1; i++)
                        {
                            typeName += "."+splited[i];
                        }
                        try{
                            FieldInfo fieldInfo = GenTypes.GetTypeInAnyAssembly(typeName)?.GetField(splited[splited.Length - 1],BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                            if(fieldInfo != null) fieldInfosOfThingCompCopiedMember.Add(fieldInfo);
                        }
                        catch
                        {
                            Log.Error($"Invaild field name : {str}, correct structor is (typeNameWithNameSpace).(FieldName)");
                        }
                    }
                }
                return fieldInfosOfThingCompCopiedMember;
            }
        }

        static CompProperties_ModularizationWeapon()
        {
            foreach(ThingDef def in DefDatabase<ThingDef>.AllDefs)
            {
                if (def.comps.Count < 2) continue;
                for(int i = 0; i < def.comps.Count; i++)
                {
                    CompProperties properties = def.comps[i];
                    if(properties.compClass == typeof(CompModularizationWeapon))
                    {
                        def.comps.RemoveAt(i);
                        def.comps.Insert(1,properties);
                        break;
                    }
                }
            }
        }

        public CompProperties_ModularizationWeapon()
        {
            compClass = typeof(CompModularizationWeapon);
        }

        /// <summary>
        /// Config checking
        /// </summary>
        /// <param name="parentDef"></param>
        /// <returns></returns>
        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            attachmentPropertiesWithQuery = new List<(QueryGroup,WeaponAttachmentProperties)>();
            foreach(string error in base.ConfigErrors(parentDef))
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
                        QueryGroup query = new QueryGroup(properties.id);
                        attachmentPropertiesWithQuery.Add((query,properties));
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
                    if(!(propertiesForCompare?.id).NullOrEmpty() && propertiesForCompare.id == properties.id)
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
        public WeaponAttachmentProperties WeaponAttachmentPropertiesById(string id)
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
        public override void ResolveReferences(ThingDef parentDef)
        {
            fieldInfosOfThingCompCopiedMember = null;
            if (CombatExtended_CompAmmoUser != null)
            {
                if (!compPropertiesCreateInstanceCompType.Contains(CombatExtended_CompAmmoUser)) compPropertiesCreateInstanceCompType.Add(CombatExtended_CompAmmoUser);
                if (!thingCompCopiedMember.Contains("CombatExtended.CompAmmoUser.curMagCountInt")) thingCompCopiedMember.Add("CombatExtended.CompAmmoUser.curMagCountInt");
                if (!thingCompCopiedMember.Contains("CombatExtended.CompAmmoUser.currentAmmoInt")) thingCompCopiedMember.Add("CombatExtended.CompAmmoUser.currentAmmoInt");
                if (!thingCompCopiedMember.Contains("CombatExtended.CompAmmoUser.selectedAmmo")) thingCompCopiedMember.Add("CombatExtended.CompAmmoUser.selectedAmmo");
            }
            if (CombatExtended_CompFireModes != null)
            {
                if (!compPropertiesCreateInstanceCompType.Contains(CombatExtended_CompFireModes)) compPropertiesCreateInstanceCompType.Add(CombatExtended_CompFireModes);
                if (!thingCompCopiedMember.Contains("CombatExtended.CompFireModes.currentFireModeInt")) thingCompCopiedMember.Add("CombatExtended.CompFireModes.currentFireModeInt");
                if (!thingCompCopiedMember.Contains("CombatExtended.CompFireModes.currentAimModeInt")) thingCompCopiedMember.Add("CombatExtended.CompFireModes.currentAimModeInt");
                if (!thingCompCopiedMember.Contains("CombatExtended.CompFireModes.targetMode")) thingCompCopiedMember.Add("CombatExtended.CompFireModes.targetMode");
                if (!thingCompCopiedMember.Contains("CombatExtended.CompFireModes.newComp")) thingCompCopiedMember.Add("CombatExtended.CompFireModes.newComp");
            }

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
            
            if (attachmentPropertiesWithQuery.NullOrEmpty())
            {
                attachmentPropertiesWithQuery = new List<(QueryGroup, WeaponAttachmentProperties)>();
                for (int i = attachmentProperties.Count - 1; i >= 0; i--)
                {
                    WeaponAttachmentProperties properties = attachmentProperties[i];
                    if (!properties.id.IsVaildityKeyFormat())
                    {
                        try{
                            attachmentProperties.RemoveAt(i);
                            QueryGroup query = new QueryGroup(properties.id);
                            attachmentPropertiesWithQuery.Add((query,properties));
                        }
                        catch{
                            Log.Error($"attachmentProperties[{i}].id is invaild key format : Not XML allowed node name");
                        }
                    }
                }
            }
            if (attachmentProperties.Count > 0) parentDef.stackLimit = 1;


            #region innerMethod
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
            #endregion

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

            compPropertiesResoveCrosseReferenceType = compPropertiesResoveCrosseReferenceType ?? new List<Type>();
            compPropertiesResoveCrosseReferenceType.RemoveAll(f => f == null || !typeof(ThingComp).IsAssignableFrom(f));

            compPropertiesCreateInstanceCompType = compPropertiesCreateInstanceCompType ?? new List<Type>();
            compPropertiesCreateInstanceCompType.RemoveAll(f => f == null || !typeof(ThingComp).IsAssignableFrom(f));

            compPropertiesInitializeCompType = compPropertiesInitializeCompType ?? new List<Type>();
            compPropertiesInitializeCompType.RemoveAll(f => f == null || !typeof(ThingComp).IsAssignableFrom(f));

            compGetGizmosExtraAllowedCompType = compGetGizmosExtraAllowedCompType ?? new List<Type>();
            compGetGizmosExtraAllowedCompType.RemoveAll(f => f == null || !typeof(ThingComp).IsAssignableFrom(f));
        }

        /// <summary>
        /// extra stat draw entry on info card
        /// </summary>
        /// <param name="req">request/param>
        /// <returns>extra stat draw entry</returns>
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
            List<FieldReaderInstance<VerbProperties>> VerbPropertiesObjectPatch = comp?.VerbPropertiesObjectPatch(null);
            if(VerbPropertiesObjectPatch != null)
            {
                foreach(FieldReaderInstance<VerbProperties> fieldReader in verbPropertiesObjectPatch)
                {
                    int index = VerbPropertiesObjectPatch.FindIndex(x => x.UsedType == fieldReader.UsedType);
                    if (index < 0) VerbPropertiesObjectPatch.Add(fieldReader);
                    else VerbPropertiesObjectPatch[index] |= fieldReader;
                }
                count += listAllInst(VerbPropertiesObjectPatch, "", "");
            }

            stringBuilder.AppendLine("toolsPatch".Translate().RawText + " :");
            List<FieldReaderInstance<Tool>> ToolsObjectPatch = comp?.ToolsObjectPatch(null);
            if (ToolsObjectPatch != null)
            {
                foreach (FieldReaderInstance<Tool> fieldReader in toolsObjectPatch)
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
            foreach (WeaponAttachmentProperties properties in comp?.CurrentPartAttachmentProperties.Values.ToList() ?? attachmentProperties)
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
                        foreach (FieldReaderInstance<VerbProperties> fieldReader in childComp.Props.verbPropertiesObjectPatch)
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
                        foreach (FieldReaderInstance<Tool> fieldReader in childComp.Props.toolsObjectPatch)
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
        /// if it's **`true`**, it will not draw attachment when it not attach on other part
        /// </summary>
        public bool drawChildPartWhenOnGround = true;

        /// <summary>
        /// if it's **`true`**, it will draw outline if it's root part
        /// </summary>
        public bool drawOutlineOnRoot = true;
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
        #endregion
        #endregion



        /// <summary>
        /// attach points defintion
        /// </summary>
        public List<WeaponAttachmentProperties> attachmentProperties = new List<WeaponAttachmentProperties>();

        /// <summary>
        /// extra drawing info when it attach on a part
        /// </summary>
        public List<PartSubDrawingInfo> subRenderinfInfos = new List<PartSubDrawingInfo>();

        /// <summary>
        /// extra comp that will add to parent comps
        /// </summary>
        public List<CompProperties> extraComp = new List<CompProperties>();

        /// <summary>
        /// the ThingComp type that will invoke `ResoveCrosseReference` method from `extraComp`
        /// </summary>
        public List<Type> compPropertiesResoveCrosseReferenceType = new List<Type>();

        /// <summary>
        /// the ThingComp type that will invoke `CreateInstance` method from `extraComp`
        /// </summary>
        public List<Type> compPropertiesCreateInstanceCompType = new List<Type>();

        /// <summary>
        /// the ThingComp type that will invoke `InitializeComp` method from `extraComp`
        /// </summary>
        public List<Type> compPropertiesInitializeCompType = new List<Type>();

        /// <summary>
        /// the ThingComp type that will invoke `GetGizmosExtra` method after this comp invoke `GetGizmosExtra`
        /// </summary>
        public List<Type> compGetGizmosExtraAllowedCompType = new List<Type>();

        /// <summary>
        /// When node update without swaping. the member who will be copying decided by this field list.
        /// <br/>
        /// Menber of this list should writen in (Type).(FieldName)
        /// </summary>
        public List<string> thingCompCopiedMember = new List<string>();

        /// <summary>
        /// special drawing texture when it attach on a part
        /// </summary>
        public string PartTexPath = null;

        /// <summary>
        /// material cache of `PartTexPath`
        /// </summary>
        private Texture2D partTexCache;
        private List<FieldInfo> fieldInfosOfThingCompCopiedMember;
        internal List<(QueryGroup,WeaponAttachmentProperties)> attachmentPropertiesWithQuery;

        private static Type CombatExtended_CompAmmoUser = GenTypes.GetTypeInAnyAssembly("CombatExtended.CompAmmoUser");
        private static Type CombatExtended_CompFireModes = GenTypes.GetTypeInAnyAssembly("CombatExtended.CompFireModes");
    }
}
