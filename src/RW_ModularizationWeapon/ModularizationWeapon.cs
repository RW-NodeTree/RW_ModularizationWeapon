using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using RW_ModularizationWeapon.Tools;
using RW_NodeTree;
using RW_NodeTree.Patch;
using RW_NodeTree.Rendering;
using RW_NodeTree.Tools;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using Verse;

namespace RW_ModularizationWeapon
{
    [StaticConstructorOnStartup]
    public partial class ModularizationWeapon : ThingWithComps, INodeProcesser, IRecipePatcher, IEnumerable<(string,Thing?,WeaponAttachmentProperties)>
    {

        private NodeContainer childNodes;

        static ModularizationWeapon()
        {
            foreach(ThingDef def in DefDatabase<ThingDef>.AllDefs)
            {
                def.GetModExtension<ModularizationWeaponExtension>()?.ResolveReferences(def);
            }
        }
        
        public ModularizationWeapon()
        {
            childNodes = new NodeContainer(this);
        }

        public ModularizationWeaponExtension Props
        {
            get
            {
                if(cachedProps != null) return cachedProps;
                cachedProps = def.GetModExtension<ModularizationWeaponExtension>();
                if (cachedProps == null)
                {
                    cachedProps = new ModularizationWeaponExtension();
                    def.modExtensions.Add(cachedProps);
                    cachedProps.drawChildPartWhenOnGround = false;
                    cachedProps.ResolveReferences(def);
                }
                return cachedProps;
            }
        }

        public ModularizationWeapon? ParentPart => ChildNodes.ParentProcesser as ModularizationWeapon;

        public ModularizationWeapon RootPart
        {
            get
            {
                ModularizationWeapon result = this;
                ModularizationWeapon? current = ParentPart;
                while (current != null)
                {
                    result = current;
                    current = current.ParentPart;
                }
                return result;
            }
        }

        public ModularizationWeapon RootOccupierPart
        {
            get
            {
                ModularizationWeapon result = this;
                ModularizationWeapon? current = occupiers ?? ParentPart;
                while (current != null)
                {
                    result = current;
                    current = current.occupiers ?? current.ParentPart;
                }
                return result;
            }
        }


        public HashSet<string> PartIDs
        {
            get
            {
                lock (partIDs)
                {
                    if (partIDs.Count == 0)
                    {
                        foreach (WeaponAttachmentProperties properties in Props.attachmentProperties)
                        {
                            if(properties.id != null)
                                partIDs.Add(properties.id);
                        }
                    }
                    return partIDs;
                }
            }
        }

        public NodeContainer ChildNodes => childNodes;

        public override Graphic Graphic
        {
            get
            {
                if (Props.drawChildPartWhenOnGround)
                {
                    if (cachedGraphic == null)
                    {
                        ModularizationWeaponExtension props = Props;
                        cachedGraphic = new Graphic_ChildNode(
                            this,
                            props.TextureSizeFactor,
                            props.ExceedanceFactor,
                            props.ExceedanceOffset,
                            GraphicsFormat.R8G8B8A8_SRGB,
                            props.TextureFilterMode,
                            props.outlineWidth > 0 ? PostFX : null
                        );
                    }
                    return cachedGraphic;
                }
                else
                {
                    return base.Graphic;
                }
            }
        }

        public ReadOnlyDictionary<string, WeaponAttachmentProperties> GetOrGenCurrentPartAttachmentProperties()
        {
            lock (this)
            {
                if (currentPartVNode == null) UpdateCurrentPartVNode();
                if (this.partAttachmentPropertiesCache == null)
                {
                    Dictionary<string, WeaponAttachmentProperties> partAttachmentPropertiesCache = new Dictionary<string, WeaponAttachmentProperties>(Props.attachmentProperties.Count);
                    List<Task<WeaponAttachmentProperties>> genTasks = new List<Task<WeaponAttachmentProperties>>(Props.attachmentProperties.Count);
                    foreach (WeaponAttachmentProperties properties in Props.attachmentProperties)
                    {
                        if (properties.id.NullOrEmpty()) continue;
                        // Thing thing = ChildNodes[properties.id];
                        // Log.Message($"{parent} Miss {properties.id} in CurrentPartAttachmentProperties for {thing}, generating");
                        // Log.Message($"{parent} Miss {properties.id} in CurrentPartAttachmentProperties : Props.attachmentPropertiesWithQuery.Count = {Props.attachmentPropertiesWithQuery.Count}");
                        genTasks.Add(Task.Run(delegate()
                        {
                            List<(WeaponAttachmentProperties, uint)> matched = new List<(WeaponAttachmentProperties, uint)>(Props.attachmentPropertiesWithQuery.Count);
                            foreach ((QueryGroup, WeaponAttachmentProperties) record in Props.attachmentPropertiesWithQuery)
                            {
                                if (record.Item1 != null && record.Item2 != null)
                                {
                                    uint currentMatch = record.Item1.Match(currentPartVNode![properties.id!]!);
                                    if (currentMatch > 0)
                                    {
                                        matched.Add((record.Item2, currentMatch));
                                    }
                                    // Log.Message($"{record.Item2.id} : {currentMatch}");
                                }
                            }

                            for (int i = 0; i < matched.Count; i++)
                            {
                                var a = matched[i];
                                for (int j = i + 1; j < matched.Count; j++)
                                {
                                    var b = matched[j];
                                    if (a.Item2 > b.Item2)
                                    {
                                        matched[j] = a;
                                        matched[i] = b;
                                        a = b;
                                    }
                                }
                            }

                            WeaponAttachmentProperties replaced = Gen.MemberwiseClone(properties);
                            for (int i = 0; i < matched.Count; i++)
                            {
                                WeaponAttachmentProperties attachmentProperties = matched[i].Item1;
                                if (attachmentProperties is OptionalWeaponAttachmentProperties optional)
                                {
                                    foreach (FieldInfo fieldInfo in optional.UsedFields)
                                    {
                                        fieldInfo.SetValue(replaced, fieldInfo.GetValue(optional));
                                    }
                                }
                                else replaced = Gen.MemberwiseClone(attachmentProperties);
                            }
                            replaced.id = properties.id;
                            return replaced;
                        }));
                    }
                    foreach(Task<WeaponAttachmentProperties> task in genTasks)
                        partAttachmentPropertiesCache.Add(task.Result.id!, task.Result);
                    this.partAttachmentPropertiesCache = new ReadOnlyDictionary<string, WeaponAttachmentProperties>(partAttachmentPropertiesCache);
                }
                return this.partAttachmentPropertiesCache;
            }
        }

        public ReadOnlyDictionary<string, WeaponAttachmentProperties> GetOrGenTargetPartAttachmentProperties()
        {
            lock (this)
            {
                if (targetPartVNode == null) UpdateTargetPartVNode();
                if (this.partAttachmentPropertiesCache_TargetPart == null)
                {
                    Dictionary<string, WeaponAttachmentProperties> partAttachmentPropertiesCache_TargetPart = new Dictionary<string, WeaponAttachmentProperties>(Props.attachmentProperties.Count);
                    List<Task<WeaponAttachmentProperties>> genTasks = new List<Task<WeaponAttachmentProperties>>(Props.attachmentProperties.Count);
                    foreach (WeaponAttachmentProperties properties in Props.attachmentProperties)
                    {
                        if (properties.id.NullOrEmpty()) continue;
                        if (partAttachmentPropertiesCache_TargetPart.ContainsKey(properties.id!)) continue;
                        // Thing thing = GetTargetPart(properties.id).Thing;
                        // Log.Message($"{parent} Miss {properties.id} in TargetPartAttachmentProperties for {thing}, generating..");
                        // Log.Message($"{parent} Miss {properties.id} in TargetPartAttachmentProperties : Props.attachmentPropertiesWithQuery.Count = {Props.attachmentPropertiesWithQuery.Count}");
                        genTasks.Add(Task.Run(delegate ()
                        {
                            List<(WeaponAttachmentProperties, uint)> matched = new List<(WeaponAttachmentProperties, uint)>(Props.attachmentPropertiesWithQuery.Count);
                            foreach ((QueryGroup, WeaponAttachmentProperties) record in Props.attachmentPropertiesWithQuery)
                            {
                                if (record.Item1 != null && record.Item2 != null)
                                {
                                    uint currentMatch = record.Item1.Match(targetPartVNode![properties.id!]!);
                                    if (currentMatch > 0)
                                    {
                                        matched.Add((record.Item2, currentMatch));
                                    }
                                    // Log.Message($"{record.Item2.id} : {currentMatch}");
                                }
                            }

                            for (int i = 0; i < matched.Count; i++)
                            {
                                var a = matched[i];
                                for (int j = i + 1; j < matched.Count; j++)
                                {
                                    var b = matched[j];
                                    if (a.Item2 > b.Item2)
                                    {
                                        matched[j] = a;
                                        matched[i] = b;
                                        a = b;
                                    }
                                }
                            }

                            WeaponAttachmentProperties replaced = Gen.MemberwiseClone(properties);
                            for (int i = 0; i < matched.Count; i++)
                            {
                                WeaponAttachmentProperties attachmentProperties = matched[i].Item1;
                                if (attachmentProperties is OptionalWeaponAttachmentProperties optional)
                                {
                                    foreach (FieldInfo fieldInfo in optional.UsedFields)
                                    {
                                        fieldInfo.SetValue(replaced, fieldInfo.GetValue(optional));
                                    }
                                }
                                else replaced = Gen.MemberwiseClone(attachmentProperties);
                            }
                            replaced.id = properties.id;
                            return replaced;
                        }));
                    }
                    foreach (Task<WeaponAttachmentProperties> task in genTasks)
                        partAttachmentPropertiesCache_TargetPart.Add(task.Result.id!, task.Result);
                    this.partAttachmentPropertiesCache_TargetPart = new ReadOnlyDictionary<string, WeaponAttachmentProperties>(partAttachmentPropertiesCache_TargetPart);
                }
                return this.partAttachmentPropertiesCache_TargetPart;
            }
        }

        public override void PostMake()
        {
            if (Props.setRandomPartWhenCreate) SetPartToRandom();
            else SetPartToDefault();
            base.PostMake();
        }


        public override void ExposeData()
        {
            lock (this)
            {
                Scribe_Deep.Look(ref this.childNodes, "innerContainer", this);
                if (childNodes == null)
                {
                    childNodes = new NodeContainer(this);
                }
                Scribe_Collections.Look(ref targetPartsWithId, "targetPartsWithId", LookMode.Value, LookMode.LocalTargetInfo, ref targetPartsWithId_IdWorkingList, ref targetPartsWithId_TargetWorkingList);
                if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
                {
                    for(int i = 0; i < Math.Min(targetPartsWithId_IdWorkingList.Count, targetPartsWithId_TargetWorkingList.Count); i ++)
                    {
                        string id = targetPartsWithId_IdWorkingList[i];
                        LocalTargetInfo targetInfo = targetPartsWithId_TargetWorkingList[i];
                        targetPartsWithId.SetOrAdd(id, targetInfo);
                        ModularizationWeapon? part = targetInfo.Thing as ModularizationWeapon;
                        if (part != null) part.occupiers = this;
                    }
                }
                base.ExposeData();
            }
            //if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs) NodeProccesser.UpdateNode();
        }


        public override bool CanStackWith(Thing other) => Props.attachmentProperties.Count == 0;


        public Dictionary<string, Rot4> PreGenRenderInfos(Rot4 rot, Graphic_ChildNode invokeSource, Dictionary<string, object?> cachedDataToPostDrawStep)
        {
            base.Graphic?.Draw(Vector3.zero, rot, this);
            List<WeaponAttachmentProperties> props = [.. GetOrGenCurrentPartAttachmentProperties().Values];
            props.SortBy(x =>-x.drawWeight);
            Dictionary<string, Rot4> renderInfos = new Dictionary<string, Rot4>(props.Count);
            for (int i = 0; i < props.Count; i++)
            {
                WeaponAttachmentProperties properties = props[i];
                Thing? part = ChildNodes[properties.id!];
                if (!internal_NotDraw(part, properties))
                {
                    renderInfos.Add(properties.id!, rot);
                }
            }
            return renderInfos;
        }


        public List<RenderInfo> PostGenRenderInfos(Rot4 rot, Graphic_ChildNode invokeSource, Dictionary<string, List<RenderInfo>> nodeRenderingInfos, Dictionary<string, object?> cachedDataFromPerDrawStep)
        {
            Matrix4x4 scale = Matrix4x4.identity;
            uint texScale = Props.TextureSizeFactor;
            Material material = base.Graphic?.MatAt(rot, this) ?? BaseContent.BadMat;
            // WTF? It didn't check anything, but it still able to replace material at right time..
            if (MaterialPool.TryGetRequestForMat(material, out MaterialRequest req)) req.mainTex = (Props.PartTexture == BaseContent.BadTex) ? material.mainTexture : Props.PartTexture;
            else req = new MaterialRequest()
            {
                renderQueue = material.renderQueue,
                shader = material.shader,
                color = material.color,
                colorTwo = material.GetColor(ShaderPropertyIDs.ColorTwo),
                mainTex = (Props.PartTexture == BaseContent.BadTex) ? material.mainTexture : Props.PartTexture,
                maskTex = material.GetTexture(ShaderPropertyIDs.MaskTex) as Texture2D,
            };
            if (invokeSource != cachedGraphic && nodeRenderingInfos.TryGetValue("", out List<RenderInfo> renderInfos))
            {
                for (int i = 0; i < renderInfos.Count; i++)
                {
                    RenderInfo info = renderInfos[i];
                    if (info.material == material)
                    {
                        info.material = MaterialPool.MatFrom(req);
                        scale.m00 = Props.DrawSizeWhenAttach.x / info.mesh.bounds.size.x;
                        scale.m22 = Props.DrawSizeWhenAttach.y / info.mesh.bounds.size.z;
                        renderInfos[i] = info;
                        break;
                    }
                }
            }
            
            List<RenderInfo> result = new List<RenderInfo>();
            foreach (var kv in nodeRenderingInfos)
            {
                string id = kv.Key;
                renderInfos = kv.Value;
                if (id.NullOrEmpty())
                {
                    if (invokeSource != cachedGraphic)
                    {
                        for (int i = 0; i < renderInfos.Count; i++)
                        {
                            RenderInfo info = renderInfos[i];
                            Matrix4x4[] matrix = info.matrices;

                            for (int j = 0; j < matrix.Length; j++)
                            {
                                Vector4 cache = matrix[j].GetRow(0);
                                matrix[j].SetRow(0, new Vector4(new Vector3(cache.x, cache.y, cache.z).magnitude, 0, 0, cache.w));

                                cache = matrix[j].GetRow(1);
                                matrix[j].SetRow(1, new Vector4(0, new Vector3(cache.x, cache.y, cache.z).magnitude, 0, cache.w));

                                cache = matrix[j].GetRow(2);
                                matrix[j].SetRow(2, new Vector4(0, 0, new Vector3(cache.x, cache.y, cache.z).magnitude, cache.w));

                                matrix[j] = scale * matrix[j];
                            }
                        }

                        renderInfos.Capacity += Props.subRenderingInfos.Count;
                        foreach (PartSubDrawingInfo drawingInfo in Props.subRenderingInfos)
                        {
                            req.mainTex = drawingInfo.PartTexture;
                            renderInfos.Add(new RenderInfo(MeshPool.plane10, 0, scale * drawingInfo.Transfrom, MaterialPool.MatFrom(req), 0));
                        }
                    }
                }
                else
                {
                    bool needTransToIdentity = ChildNodes[id] is ModularizationWeapon;
                    WeaponAttachmentProperties? properties = CurrentPartWeaponAttachmentPropertiesById(id);
                    if (properties != null)
                    {
                        Matrix4x4 transfrom = properties.Transfrom(texScale);
                        for (int i = 0; i < renderInfos.Count; i++)
                        {
                            Matrix4x4[] matrix = renderInfos[i].matrices;
                            for (int j = 0; j < matrix.Length; j++)
                            {
                                if (needTransToIdentity)
                                {
                                    Vector4 cache = matrix[j].GetRow(0);
                                    matrix[j].SetRow(0, new Vector4(new Vector3(cache.x, cache.y, cache.z).magnitude, 0, 0, cache.w));

                                    cache = matrix[j].GetRow(1);
                                    matrix[j].SetRow(1, new Vector4(0, new Vector3(cache.x, cache.y, cache.z).magnitude, 0, cache.w));

                                    cache = matrix[j].GetRow(2);
                                    matrix[j].SetRow(2, new Vector4(0, 0, new Vector3(cache.x, cache.y, cache.z).magnitude, cache.w));
                                }
                                matrix[j] = transfrom * scale * matrix[j];
                                //matrix[k] = properties.Transfrom;
                            }
                        }
                    }
                }

                for (int j = 0; j < renderInfos.Count; j++)
                {
                    RenderInfo info = renderInfos[j];
                    info.mesh = ReindexedMesh(info.mesh);
                    info.CanUseFastDrawingMode = true;
                    renderInfos[j] = info;
                }
                result.AddRange(renderInfos);
            }
            return result;
        }


        public void PostFX(RenderTexture tar)
        {
            if (PostFXMat == null)
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
                            // Log.Message($"Loading shader in {assetBundle.name}");
                            Shader shader = assetBundle.LoadAsset<Shader>(@"Assets\Data\RWNodeTree.RWWeaponModularization\OutLine.shader");
                            if (shader != null && shader.isSupported)
                            {
                                // Log.Message($"pass {assetBundle.name}.{shader.name}");
                                PostFXMat = new Material(shader);
                                break;
                            }
                        }
                        break;
                    }
                }
            }
            if(PostFXMat == null) return;

            PostFXCommandBuffer ??= new CommandBuffer();
            PostFXCommandBuffer.Clear();
            RenderTexture renderTexture = RenderTexture.GetTemporary(tar.width, tar.height, 0, tar.format);
            PostFXCommandBuffer.Blit(tar, renderTexture);
            PostFXMat.SetFloat("_EdgeSize", Props.outlineWidthInPixelSize ? Props.outlineWidth : Props.TextureSizeFactor * Props.outlineWidth);
            PostFXCommandBuffer.Blit(renderTexture, tar, PostFXMat);
            Graphics.ExecuteCommandBuffer(PostFXCommandBuffer);
            RenderTexture.ReleaseTemporary(renderTexture);
        }


        public bool AllowNode(Thing? node, string? id = null)
        {
            NodeContainer? childs = ChildNodes;
            if (id == null) return false;
            if (childs == null) return false;
            if (!PartIDs.Contains(id)) return false;
            if (node == childs[id]) return true;
            lock (this)
            {
                ModularizationWeapon? comp = node as ModularizationWeapon;
                if (comp != null && (comp.ParentPart != null || (comp.occupiers != null && comp.occupiers != this))) return false;
                WeaponAttachmentProperties? currentPartProperties = CurrentPartWeaponAttachmentPropertiesById(id!);
                WeaponAttachmentProperties? targetPartProperties = TargetPartWeaponAttachmentPropertiesById(id!);
                //if (Prefs.DevMode) Log.Message($"properties : {properties}");
                if (currentPartProperties != null && targetPartProperties != null)
                {
                    if (node == null) return currentPartProperties.allowEmpty && targetPartProperties.allowEmpty;

                    return
                        currentPartProperties.filterWithWeights.Any(x => x.thingDef == node.def) &&
                        targetPartProperties.filterWithWeights.Any(x => x.thingDef == node.def) &&
                        !internal_Unchangeable(childs[id!], currentPartProperties) &&
                        !internal_Unchangeable(childs[id!], targetPartProperties);
                }
                return false;
            }
        }


        public void SetPartToDefault()
        {
            NodeContainer? childs = ChildNodes;
            if (IsSwapRoot && childs != null)
            {
                ReadOnlyDictionary<string, WeaponAttachmentProperties> props = GetOrGenCurrentPartAttachmentProperties();
                foreach (var properties in props)
                {
                    ThingDef? def = properties.Value.defultThing;
                    if (def != null)
                    {
                        Thing thing = ThingMaker.MakeThing(def, GenStuff.RandomStuffFor(def));
                        thing.TryGetComp<CompQuality>()?.SetQuality(QualityUtility.GenerateQualityRandomEqualChance(), ArtGenerationContext.Colony);
                        if (SetTargetPart(properties.Key, thing))
                        {
                            (childs[properties.Key] as ModularizationWeapon)?.SetPartToRandom();
                        }
                        else
                        {
                            thing.Destroy();
                        }
                    }
                    else if(SetTargetPart(properties.Key, null))
                    {
                        (childs[properties.Key] as ModularizationWeapon)?.SetPartToRandom();
                    }
                }
                SwapTargetPart();
                foreach (var properties in props)
                {
                    GetTargetPart(properties.Key).Thing?.Destroy();
                }
                ClearTargetPart();
            }
        }


        public void SetPartToRandom()
        {

            NodeContainer? childs = ChildNodes;
            //Console.WriteLine($"====================================   {parent}.SetPartToRandom End   ====================================");
            if (IsSwapRoot && childs != null)
            {
                ReadOnlyDictionary<string, WeaponAttachmentProperties> props = GetOrGenCurrentPartAttachmentProperties();
                // Console.WriteLine($"==================================== {parent}.SetPartToRandom Start   ====================================");
                foreach (var properties in props)
                {
                    bool insertFlag = false;
                    if (!properties.Value.filterWithWeights.NullOrEmpty())
                    {
                        float count = properties.Value.allowEmpty ? properties.Value.randomToEmptyWeight : 0;
                        properties.Value.filterWithWeights.ForEach(x => count += x.count);
                        for (int j = 0; j < 3; j++)
                        {
                            float k = Rand.Range(0, count);
                            float l = 0;
                            ThingDef? def = null;
                            foreach (ThingDefCountClass weight in properties.Value.filterWithWeights)
                            {
                                float next = l + weight.count;
                                if (l <= k && next >= k) def = weight.thingDef;
                                l = next;
                            }
                            if (def != null)
                            {
                                Thing thing = ThingMaker.MakeThing(def, GenStuff.RandomStuffFor(def));
                                thing.TryGetComp<CompQuality>()?.SetQuality(QualityUtility.GenerateQualityRandomEqualChance(), ArtGenerationContext.Outsider);
                                if (SetTargetPart(properties.Key, thing))
                                {
                                    insertFlag = true;
                                    break;
                                }
                                thing.Destroy();
                            }
                            else if(SetTargetPart(properties.Key, null))
                            {
                                insertFlag = true;
                                break;
                            }
                        }
                    }
                    if(!insertFlag)
                    {
                        (childs[properties.Key] as ModularizationWeapon)?.SetPartToRandom();
                    }
                    // Console.WriteLine($"{parent}[{properties.id}]:{ChildNodes[properties.id]},{GetTargetPart(properties.id)}");
                }
                SwapTargetPart();
                foreach (var properties in props)
                {
                    GetTargetPart(properties.Key).Thing?.Destroy();
                }
                ClearTargetPart();
            }
        }


        public IEnumerable<Thing> PostGenRecipe_MakeRecipeProducts(RecipeDef recipeDef, Pawn worker, List<Thing> ingredients, Thing? dominantIngredient, IBillGiver billGiver, Precept_ThingStyle? precept, RecipeInvokeSource invokeSource, IEnumerable<Thing> result)
        {
            if(invokeSource == RecipeInvokeSource.products)
            {
                SetPartToDefault();
            }
            else if(invokeSource == RecipeInvokeSource.ingredients && IsSwapRoot)
            {
                IEnumerable<Thing> Ingredients(IEnumerable<Thing> org)
                {
                    foreach (Thing ingredient in org) yield return ingredient;
                    NodeContainer? childs = ChildNodes;
                    if (childs != null)
                    {
                        foreach (var par in childs)
                        {
                            SetTargetPart(par.Item1, null);
                            if (par.Item2 != null) yield return par.Item2;
                        }
                        SwapTargetPart();
                        ClearTargetPart();
                    }
                }
                return Ingredients(result);
            }
            return result;
        }


        public override void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            base.PreApplyDamage(ref dinfo, out absorbed);
            if (absorbed) return;
            NodeContainer? childs = ChildNodes;
            if (childs != null)
            {
                int count = childs.Count + 1;
                dinfo.SetAmount(dinfo.Amount / count);
                foreach (Thing? thing in childs.Values)
                {
                    thing?.TakeDamage(dinfo);
                }
            }
        }

        private void MarkTargetPartChanged()
        {
            lock (this)
            {
                cachedGraphic_TargetPart?.ForceUpdateAll();
                statOffsetCache_TargetPart = null;
                statMultiplierCache_TargetPart = null;
                toolsCache_TargetPart = null;
                verbPropertiesCache_TargetPart = null;
                childVariantVerbsOfVerbProp_TargetPart = null;
                childVariantVerbsOfTool_TargetPart = null;
                compPropertiesCache_TargetPart = null;

                if(comps_TargetPart != null)
                {
                    foreach (var destructor in Props.thingCompDestructors)
                    {
                        foreach (var comp in comps_TargetPart)
                        {
                            destructor.DestroyComp(this, comp);
                        }
                    }
                    comps_TargetPart = null;
                }
            }
        }


        private void SwapAttachmentPropertiesCacheAndVNode()
        {
            NodeContainer? childs = ChildNodes;
            if (childs == null) return;
            lock (this)
            {
                ReadOnlyDictionary<string, WeaponAttachmentProperties>? attachmentPropertiesCache = this.partAttachmentPropertiesCache;
                this.partAttachmentPropertiesCache = this.partAttachmentPropertiesCache_TargetPart;
                this.partAttachmentPropertiesCache_TargetPart = attachmentPropertiesCache;
                VNode? targetPartXmlNode = this.targetPartVNode;
                this.targetPartVNode = this.currentPartVNode;
                this.currentPartVNode = targetPartXmlNode;
                foreach (Thing? thing in childs.Values)
                {
                    ModularizationWeapon? weapon = thing as ModularizationWeapon;
                    weapon?.SwapAttachmentPropertiesCacheAndVNode();
                }
                foreach (Thing? thing in targetPartsWithId.Values)
                {
                    ModularizationWeapon? weapon = thing as ModularizationWeapon;
                    weapon?.SwapAttachmentPropertiesCacheAndVNode();
                }
            }
        }


        private void InitAttachmentProperties()
        {
            NodeContainer? childs = ChildNodes;
            if (childs == null) return;
            lock (this)
            {
                if (Props.attachmentProperties.Count <= 0) return;
                foreach (Thing? thing in childs.Values)
                {
                    ModularizationWeapon? comp = thing as ModularizationWeapon;
                    comp?.InitAttachmentProperties();
                }
                foreach (Thing? thing in targetPartsWithId.Values)
                {
                    ModularizationWeapon? comp = thing as ModularizationWeapon;
                    comp?.InitAttachmentProperties();
                }
                GetOrGenCurrentPartAttachmentProperties();
                GetOrGenTargetPartAttachmentProperties();
            }
        }


        private void SwapCache()
        {
            NodeContainer? childs = ChildNodes;
            if (childs == null) return;
            lock (this)
            {
                if (Props.attachmentProperties.Count <= 0) return;
                foreach (Thing? thing in childs.Values)
                {
                    ModularizationWeapon? comp = thing as ModularizationWeapon;
                    comp?.SwapCache();
                }
                foreach (Thing? thing in targetPartsWithId.Values)
                {
                    ModularizationWeapon? comp = thing as ModularizationWeapon;
                    comp?.SwapCache();
                }

                Graphic_ChildNode? cachedGraphic = this.cachedGraphic;
                this.cachedGraphic = this.cachedGraphic_TargetPart;
                this.cachedGraphic_TargetPart = cachedGraphic;
                Dictionary<(StatDef, string?), float>? statOffsetCache = this.statOffsetCache;
                this.statOffsetCache = this.statOffsetCache_TargetPart;
                this.statOffsetCache_TargetPart = statOffsetCache;
                Dictionary<(StatDef, string?), float>? statMultiplierCache = this.statMultiplierCache;
                this.statMultiplierCache = this.statMultiplierCache_TargetPart;
                this.statMultiplierCache_TargetPart = statMultiplierCache;
                ReadOnlyCollection<(string? id, int index, Tool afterConvert)>? toolsCache = this.toolsCache;
                this.toolsCache = this.toolsCache_TargetPart;
                this.toolsCache_TargetPart = toolsCache;
                ReadOnlyCollection<(string? id, int index, VerbProperties afterConvert)>? verbPropertiesCache = this.verbPropertiesCache;
                this.verbPropertiesCache = this.verbPropertiesCache_TargetPart;
                this.verbPropertiesCache_TargetPart = verbPropertiesCache;
                Dictionary<int, ReadOnlyCollection<(CompEquippable, Verb)>>? childVariantVerbsOfVerbProp = this.childVariantVerbsOfVerbProp;
                this.childVariantVerbsOfVerbProp = this.childVariantVerbsOfVerbProp_TargetPart;
                this.childVariantVerbsOfVerbProp_TargetPart = childVariantVerbsOfVerbProp;
                Dictionary<(int, VerbProperties?), ReadOnlyCollection<(CompEquippable, Verb)>>? childVariantVerbsOfTool = this.childVariantVerbsOfTool;
                this.childVariantVerbsOfTool_TargetPart = childVariantVerbsOfTool;
                this.childVariantVerbsOfTool = this.childVariantVerbsOfTool_TargetPart;
                ReadOnlyCollection<(string? id, int index, CompProperties afterConvert)>? compPropertiesCache = this.compPropertiesCache;
                this.compPropertiesCache = this.compPropertiesCache_TargetPart;
                this.compPropertiesCache_TargetPart = compPropertiesCache;
                InitializeComps();
            }
            
        }


        private void UpdateCache()
        {
            NodeContainer? childs = ChildNodes;
            if (childs == null) return;
            lock (this)
            {
                if (Props.attachmentProperties.Count <= 0) return;
                foreach (Thing? thing in childs.Values)
                {
                    ModularizationWeapon? comp = thing as ModularizationWeapon;
                    comp?.UpdateCache();
                }
                foreach (Thing? thing in targetPartsWithId.Values)
                {
                    ModularizationWeapon? comp = thing as ModularizationWeapon;
                    comp?.UpdateCache();
                }

                this.cachedGraphic?.ForceUpdateAll();

                this.statOffsetCache = null;
                this.statMultiplierCache = null;
                this.childVariantVerbsOfVerbProp = null;
                this.childVariantVerbsOfTool = null;
                this.toolsCache = null;
                this.verbPropertiesCache = null;
                this.compPropertiesCache = null;


                foreach (var destructor in Props.thingCompDestructors)
                {
                    foreach (var comp in AllComps)
                    {
                        destructor.DestroyComp(this, comp);
                    }
                }
                InitializeComps();
            }
        }


        public Dictionary<string, Thing>? GenChilds(INodeProcesser actionNode, Dictionary<string, object?> cachedDataToPostUpatde)
        {
            // if(stopWatch.IsRunning) stopWatch.Restart();
            // else stopWatch.Start();
            // long ct = 0;
            // long lt = 0;
            NodeContainer? childs = ChildNodes;
            if (childs == null) return null;
            lock (this)
            {
                Dictionary<string, Thing> nextChild = new Dictionary<string, Thing>(childs.Count);
                if (Props.attachmentProperties.Count <= 0)
                {
                    return nextChild;
                }
                foreach (var keyValue in childs)
                {
                    nextChild[keyValue.Item1] = keyValue.Item2;
                }
                if (swap)
                {
                    return nextChild;
                }
                Map? swapMap = MapHeld;
                foreach (var kv in targetPartsWithId)
                {
                    string id = kv.Key;
                    LocalTargetInfo target = kv.Value;
                    if(target.Thing != null)
                    {
                        nextChild[id] = target.Thing;
                        if(swapMap != null && target.Thing.Map == swapMap)
                            target.Thing.DeSpawn();
                    }
                    else
                    {
                        nextChild.Remove(id);
                    }
                }
                return nextChild;
            }
        }

        public void PreUpdateChilds(INodeProcesser actionNode, Dictionary<string, object?> cachedDataToPostUpatde, ReadOnlyDictionary<string, Thing> prveChilds)
        {
            Map? swapMap = MapHeld;
            NodeContainer? childs = ChildNodes;
            lock (this)
            {
                if (swap)
                {
                    #region Remove not successed swap parts
                    bool needUpdateVNode = false;
                    Dictionary<string, LocalTargetInfo> targetPartsWithId = new Dictionary<string, LocalTargetInfo>(this.targetPartsWithId);
                    foreach (var kv in targetPartsWithId)
                    {
                        if (childs[kv.Key] != kv.Value)
                        {
                            RemoveTargetPartInternal(kv.Key, out _);
                            ModularizationWeapon? part = kv.Value.Thing as ModularizationWeapon;
                            part?.UpdateTargetPartVNode();
                            targetPartChanged = true;
                            needUpdateVNode = true;
                            if(swapMap != null && kv.Value.Thing != null && !kv.Value.Thing.Spawned)
                            {
                                kv.Value.Thing.SpawnSetup(swapMap, false);
                            }
                        }
                    }
                    if (needUpdateVNode)
                    {
                        UpdateTargetPartVNode();
                    }
                    #endregion

                    #region Restore previous parts
                    targetPartsWithId = new Dictionary<string, LocalTargetInfo>(this.targetPartsWithId);
                    foreach (var kv in targetPartsWithId)
                    {
                        if (prveChilds.ContainsKey(kv.Key))
                            SetTargetPartInternal(kv.Key, prveChilds[kv.Key], out _);
                        else
                            RemoveTargetPartInternal(kv.Key, out _);
                    }
                    
                    #endregion
                }

                
                #region Mark childs and targetPartsWithId to update
                foreach (Thing? node in childs.Values)
                {
                    ModularizationWeapon? part = node as ModularizationWeapon;
                    if(part != null)
                    {
                        part.ChildNodes.NeedUpdate = true;
                        part.swap = swap;
                    }
                }

                foreach (Thing? node in targetPartsWithId.Values)
                {
                    ModularizationWeapon? part = node as ModularizationWeapon;
                    if(part != null)
                    {
                        part.ChildNodes.NeedUpdate = true;
                        part.swap = swap;
                    }
                    if(swap && swapMap != null && node != null && !node.Spawned)
                    {
                        int index = swapMap.cellIndices.CellToIndex(node.Position);
                        if (index < swapMap.cellIndices.NumGridCells && index >= 0)
                        {
                            node.SpawnSetup(swapMap, false);
                        }
                    }
                }
                #endregion
            }
        }


        public void PostUpdateChilds(INodeProcesser actionNode, Dictionary<string, object?> cachedDataFromPerUpdate, ReadOnlyDictionary<string, Thing> prveChilds)
        {
            // if(stopWatch.IsRunning) stopWatch.Restart();
            // else stopWatch.Start();
            // long ct = 0;
            // long lt = 0;

            lock (this)
            {
                if (targetPartChanged)
                {

                    if(occupiers != null)
                    {
                        occupiers.targetPartChanged = true;
                    }
                    else if(ParentPart != null)
                    {
                        ParentPart.targetPartChanged = true;
                    }
                    MarkTargetPartChanged();
                }
                if (Props.attachmentProperties.Count > 0 && actionNode == this)
                {
                    if (swap) SwapAttachmentPropertiesCacheAndVNode();
                    else UpdateCurrentPartVNode();
                    InitAttachmentProperties();
                    if (swap) SwapCache();
                    else UpdateCache();
                }
                swap = false;
                ChildNodes.NeedUpdate = false;
                targetPartChanged = false;
            }
        }


        public void SetChildPostion(IntVec3? pos = null)
        {
            lock (this)
            {
                NodeContainer? childs = ChildNodes;
                if (childs == null) return;
                IntVec3 handlePos = pos ?? PositionHeld;
                foreach (Thing? thing in childs.Values)
                {
                    if(thing != null)
                    {
                        thing.Position = handlePos;
                        (thing as ModularizationWeapon)?.SetChildPostion(handlePos);
                    }
                }
            }
        }


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

                List<Vector3> vert = [.. meshin.vertices];
                vert.AddRange(vert);
                mesh.vertices = [.. vert];

                List<Vector2> uv = [.. meshin.uv];
                uv.AddRange(uv);
                mesh.uv = [.. uv];

                List<int> trangles = [.. meshin.GetTriangles(0)];
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
        
        public WeaponAttachmentProperties? CurrentPartWeaponAttachmentPropertiesById(string? id)
        {
            if(!id.NullOrEmpty()) return GetOrGenCurrentPartAttachmentProperties()!.TryGetValue(id!);
            return null;
        }
        
        public WeaponAttachmentProperties? TargetPartWeaponAttachmentPropertiesById(string? id)
        {
            if(!id.NullOrEmpty()) return GetOrGenTargetPartAttachmentProperties()!.TryGetValue(id!);
            return null;
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.ChildNodes);
        }


        public ThingOwner GetDirectlyHeldThings() => ChildNodes;


        private bool swap = false;
        private bool targetPartChanged = false;
        private VNode? targetPartVNode = null;
        private VNode? currentPartVNode = null;
        private ModularizationWeaponExtension? cachedProps = null;
        private ModularizationWeapon? occupiers = null;
        private Graphic_ChildNode? cachedGraphic = null;
        private Graphic_ChildNode? cachedGraphic_TargetPart = null;
        private List<string> targetPartsWithId_IdWorkingList = new List<string>();
        private List<LocalTargetInfo> targetPartsWithId_TargetWorkingList = new List<LocalTargetInfo>();
        private Dictionary<string, LocalTargetInfo> targetPartsWithId = new Dictionary<string, LocalTargetInfo>(); //part difference table
        private Dictionary<(StatDef, string?), float>? statOffsetCache = null;
        private Dictionary<(StatDef, string?), float>? statMultiplierCache = null;
        private Dictionary<(StatDef, string?), float>? statOffsetCache_TargetPart = null;
        private Dictionary<(StatDef, string?), float>? statMultiplierCache_TargetPart = null;
        private ReadOnlyDictionary<string, WeaponAttachmentProperties>? partAttachmentPropertiesCache = null;
        private ReadOnlyDictionary<string, WeaponAttachmentProperties>? partAttachmentPropertiesCache_TargetPart = null;
        private readonly HashSet<string> partIDs = new HashSet<string>();
        private readonly Dictionary<string, bool> childTreeViewOpend = new Dictionary<string, bool>();
        private static Material? PostFXMat = null;
        private static CommandBuffer? PostFXCommandBuffer = null;
        private static readonly AccessTools.FieldRef<ThingDef, List<VerbProperties>?> ThingDef_verbs = AccessTools.FieldRefAccess<ThingDef, List<VerbProperties>?>("verbs");


        private static readonly Dictionary<Mesh, Mesh> MeshReindexed = new Dictionary<Mesh, Mesh>();
        
        internal static readonly Stopwatch stopWatch = new Stopwatch();
    }

    /// <summary>
    /// this type is parmerters holder of the type `CompModularizationWeapon`, it define all parmerters that can write in XML.
    /// </summary>
    public class ModularizationWeaponExtension : DefModExtension
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
            bool needGennerateAttachmentPropertiesWithQuery = this.attachmentPropertiesWithQuery.NullOrEmpty();
            if(needGennerateAttachmentPropertiesWithQuery) attachmentPropertiesWithQuery = new List<(QueryGroup,WeaponAttachmentProperties)>();
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
                        if(needGennerateAttachmentPropertiesWithQuery)
                        {
                            attachmentProperties.RemoveAt(i);
                            QueryGroup query = new QueryGroup(properties.id!);
                            attachmentPropertiesWithQuery!.Add((query,properties));
                        }
                        else faild = true;
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
                            QueryGroup query = new QueryGroup(properties.id!);
                            attachmentPropertiesWithQuery.Add((query,properties));
                        }
                        catch{
                            Log.Error($"attachmentProperties[{i}].id is invaild key format : Not XML allowed node name");
                        }
                    }
                }
            }
            else
            {
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

            parentDef.weaponTags ??= [];
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
