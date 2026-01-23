using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
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
                def.GetModExtension<ModularizationWeaponDefExtension>()?.ResolveReferences(def);
            }
        }
        
        public ModularizationWeapon()
        {
            childNodes = new NodeContainer(this);
        }

        public ModularizationWeaponDefExtension Props
        {
            get
            {
                if(cachedProps != null) return cachedProps;
                cachedProps = def.GetModExtension<ModularizationWeaponDefExtension>();
                if (cachedProps == null)
                {
                    cachedProps = new ModularizationWeaponDefExtension();
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
                lock (this)
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
                    if (graphicCache == null)
                    {
                        ModularizationWeaponDefExtension props = Props;
                        graphicCache = new Graphic_ChildNode(
                            this,
                            props.TextureSizeFactor,
                            props.ExceedanceFactor,
                            props.ExceedanceOffset,
                            GraphicsFormat.R8G8B8A8_UNorm,
                            props.TextureFilterMode,
                            props.outlineWidth > 0 ? PostFX : null
                        );
                    }
                    return graphicCache;
                }
                else
                {
                    return base.Graphic;
                }
            }
        }

        public ReadOnlyDictionary<string, WeaponAttachmentProperties> GetOrGenCurrentPartAttachmentProperties()
        {
            bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
            if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
            try
            {
                if (VNodeCache == null) UpdateCurrentPartVNode();
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
                                    uint currentMatch = record.Item1.Match(VNodeCache![properties.id!]!);
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
                        
                    bool isWriteLockHeld = readerWriterLockSlim.IsWriteLockHeld;
                    if (!isWriteLockHeld) readerWriterLockSlim.EnterWriteLock();
                    try
                    {
                        this.partAttachmentPropertiesCache = new ReadOnlyDictionary<string, WeaponAttachmentProperties>(partAttachmentPropertiesCache);
                    }
                    finally
                    {
                        if (!isWriteLockHeld) readerWriterLockSlim.ExitWriteLock();
                    }
                }
                return this.partAttachmentPropertiesCache;
            }
            finally
            {
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
            }
        }

        public ReadOnlyDictionary<string, WeaponAttachmentProperties> GetOrGenTargetPartAttachmentProperties()
        {
            bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
            if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
            try
            {
                if (VNodeCache_TargetPart == null) UpdateTargetPartVNode();
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
                                    uint currentMatch = record.Item1.Match(VNodeCache_TargetPart![properties.id!]!);
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
                        
                    bool isWriteLockHeld = readerWriterLockSlim.IsWriteLockHeld;
                    if (!isWriteLockHeld) readerWriterLockSlim.EnterWriteLock();
                    try
                    {
                        this.partAttachmentPropertiesCache_TargetPart = new ReadOnlyDictionary<string, WeaponAttachmentProperties>(partAttachmentPropertiesCache_TargetPart);
                    }
                    finally
                    {
                        if (!isWriteLockHeld) readerWriterLockSlim.ExitWriteLock();
                    }
                }
                return this.partAttachmentPropertiesCache_TargetPart;
            }
            finally
            {
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
            }
        }

        public override void PostMake()
        {
            if (Props.setRandomPartWhenCreate) SetPartToRandom();
            else SetPartToDefault();
            base.PostMake();
            PrivateProperties.MarkAllMaked();
            var props = InheritableProperties;
            if(Props.equippable && currentWeaponMode < props.Count)
            {
                props[(int)currentWeaponMode].MarkAllMaked();
            }
        }


        public override void ExposeData()
        {
            bool isWriteLockHeld = readerWriterLockSlim.IsWriteLockHeld;
            if (!isWriteLockHeld) readerWriterLockSlim.EnterWriteLock();
            try
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
                        targetPartsWithId[id] = targetInfo;
                        ModularizationWeapon? part = targetInfo.Thing as ModularizationWeapon;
                        if (part != null) part.occupiers = this;
                    }
                }
                Scribe_Values.Look(ref this.currentWeaponMode, "currentWeaponMode");
                base.ExposeData();
                PrivateProperties.MarkAllMaked();
                if(Props.equippable)
                {
                    ReadOnlyCollection<CompProperties_ModularizationWeaponEquippable> inheritableProperties = InheritableProperties;
                    if(currentWeaponMode < inheritableProperties.Count)
                    {
                        inheritableProperties[(int)currentWeaponMode].MarkAllMaked();
                    }
                    if (Scribe.EnterNode("SandboxDatas"))
                    {
                        try
                        {
                            uint original = currentWeaponMode;
                            for(uint i = 0; i < inheritableProperties.Count; i++)
                            {
                                if (i != original && Scribe.EnterNode("item_" + (i > original ? (i - 1) : i)))
                                {
                                    try
                                    {
                                        currentWeaponMode = i;
                                        if (Scribe.loader?.curXmlParent?.Attributes["IsNull"] == null) //Also pass on write mode
                                        {
                                            this.InitializeComps(); // Only create comps, not invoke PostMake
                                            CompProperties_ModularizationWeaponEquippable comps = inheritableProperties[(int)i];
                                            comps.ExposeData(); // 
                                            if(Scribe.mode != LoadSaveMode.Saving)
                                            {
                                                comps.MarkAllMaked();
                                            }
                                        }
                                    }
                                    catch(Exception ex)
                                    {
                                        const int key = ('M' << 24) | ('W' << 16) | ('E' << 8) | 'D';
                                        Log.ErrorOnce(ex.ToString(), key);
                                    }
                                    Scribe.ExitNode();
                                }
                            }
                            currentWeaponMode = original;
                            if (Scribe.mode == LoadSaveMode.LoadingVars)
                            {
                                this.InitializeComps();
                            }
                        }
                        catch(Exception ex)
                        {
                            const int key = ('M' << 24) | ('W' << 16) | ('E' << 8) | 'D';
                            Log.ErrorOnce(ex.ToString(), key);
                        }
                        Scribe.ExitNode();
                    }
                }
            }
            finally
            {
                if (!isWriteLockHeld) readerWriterLockSlim.ExitWriteLock();
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
                if (!NotDraw(part, properties))
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
            if (invokeSource != graphicCache && nodeRenderingInfos.TryGetValue("", out List<RenderInfo> renderInfos))
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
            ReadOnlyDictionary<string, WeaponAttachmentProperties> attachmentProperties = GetOrGenCurrentPartAttachmentProperties();
            foreach (var kv in nodeRenderingInfos)
            {
                string id = kv.Key;
                renderInfos = kv.Value;
                if (id.NullOrEmpty())
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
                else
                {
                    bool needTransToIdentity = ChildNodes[id] is ModularizationWeapon;
                    attachmentProperties.TryGetValue(id, out WeaponAttachmentProperties? properties);
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
            bool isUpgradeableReadLockHeld = readerWriterLockSlim.IsUpgradeableReadLockHeld || readerWriterLockSlim.IsWriteLockHeld;
            if (!isUpgradeableReadLockHeld) readerWriterLockSlim.EnterUpgradeableReadLock();
            try
            {
                ModularizationWeapon? comp = node as ModularizationWeapon;
                if (comp != null && (comp.ParentPart != null || (comp.occupiers != null && comp.occupiers != this))) return false;
                ReadOnlyDictionary<string, WeaponAttachmentProperties> currentAttachmentProperties = GetOrGenCurrentPartAttachmentProperties();
                ReadOnlyDictionary<string, WeaponAttachmentProperties> targetAttachmentProperties = GetOrGenTargetPartAttachmentProperties();
                currentAttachmentProperties.TryGetValue(id, out WeaponAttachmentProperties? currentPartProperties);
                targetAttachmentProperties.TryGetValue(id, out WeaponAttachmentProperties? targetPartProperties);
                //if (Prefs.DevMode) Log.Message($"properties : {properties}");
                if (currentPartProperties != null && targetPartProperties != null)
                {
                    if (node == null) return currentPartProperties.allowEmpty && targetPartProperties.allowEmpty;

                    return
                        currentPartProperties.filterWithWeights.Any(x => x.thingDef == node.def) &&
                        targetPartProperties.filterWithWeights.Any(x => x.thingDef == node.def) &&
                        !Unchangeable(childs[id!], currentPartProperties) &&
                        !Unchangeable(childs[id!], targetPartProperties);
                }
                return false;
            }
            finally
            {
                if (!isUpgradeableReadLockHeld) readerWriterLockSlim.ExitUpgradeableReadLock();
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
                List<Thing> prve = new List<Thing>(childs);
                SwapTargetPart();

                foreach (Thing? thing in targetPartsWithId.Values)
                {
                    thing?.Destroy();
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
                foreach (Thing? thing in targetPartsWithId.Values)
                {
                    thing?.Destroy();
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
            graphicCache_TargetPart?.ForceUpdateAll();
            statOffsetCache_TargetPart = null;
            statMultiplierCache_TargetPart = null;
            WeaponProperties? publicPropertiesCache_TargetPart = this.privatePropertiesCache_TargetPart;
            this.privatePropertiesCache_TargetPart = null;
            ReadOnlyCollection<CompProperties_ModularizationWeaponEquippable>? protectedPropertiesCache_TargetPart = this.inheritablePropertiesCache_TargetPart;
            this.inheritablePropertiesCache_TargetPart = null;
            
            publicPropertiesCache_TargetPart?.DestroyComps();
            if(protectedPropertiesCache_TargetPart != null)
            {
                foreach (var item in protectedPropertiesCache_TargetPart)
                {
                    item.DestroyComps();
                }
            }
        }


        private void SwapAttachmentPropertiesCacheAndVNode()
        {
            NodeContainer? childs = ChildNodes;
            if (childs == null) return;
            ReadOnlyDictionary<string, WeaponAttachmentProperties>? attachmentPropertiesCache = this.partAttachmentPropertiesCache;
            this.partAttachmentPropertiesCache = this.partAttachmentPropertiesCache_TargetPart;
            this.partAttachmentPropertiesCache_TargetPart = attachmentPropertiesCache;
            VNode? targetPartXmlNode = this.VNodeCache_TargetPart;
            this.VNodeCache_TargetPart = this.VNodeCache;
            this.VNodeCache = targetPartXmlNode;
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


        private void InitAttachmentProperties()
        {
            NodeContainer? childs = ChildNodes;
            if (childs == null) return;
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


        private void SwapCache()
        {
            NodeContainer? childs = ChildNodes;
            if (childs == null) return;
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

            Graphic_ChildNode? graphicCache = this.graphicCache;
            this.graphicCache = this.graphicCache_TargetPart;
            this.graphicCache_TargetPart = graphicCache;
            WeaponProperties? privatePropertiesCache = this.privatePropertiesCache;
            this.privatePropertiesCache = this.privatePropertiesCache_TargetPart;
            this.privatePropertiesCache_TargetPart = privatePropertiesCache;
            ReadOnlyCollection<CompProperties_ModularizationWeaponEquippable>? inheritablePropertiesCache = this.inheritablePropertiesCache;
            this.inheritablePropertiesCache = this.inheritablePropertiesCache_TargetPart;
            this.inheritablePropertiesCache_TargetPart = inheritablePropertiesCache;
            Dictionary<(StatDef, string?), float>? statOffsetCache = this.statOffsetCache;
            this.statOffsetCache = this.statOffsetCache_TargetPart;
            this.statOffsetCache_TargetPart = statOffsetCache;
            Dictionary<(StatDef, string?), float>? statMultiplierCache = this.statMultiplierCache;
            this.statMultiplierCache = this.statMultiplierCache_TargetPart;
            this.statMultiplierCache_TargetPart = statMultiplierCache;

            currentWeaponMode = uint.MaxValue;
            CurrentMode = 0;
        }


        private void UpdateCache()
        {
            NodeContainer? childs = ChildNodes;
            if (childs == null) return;
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

            this.graphicCache?.ForceUpdateAll();

            this.statOffsetCache = null;
            this.statMultiplierCache = null;

            WeaponProperties? privatePropertiesCache = this.privatePropertiesCache;
            this.privatePropertiesCache = null;
            ReadOnlyCollection<CompProperties_ModularizationWeaponEquippable>? inheritablePropertiesCache = this.inheritablePropertiesCache;
            this.inheritablePropertiesCache = null;
            
            currentWeaponMode = uint.MaxValue;
            CurrentMode = 0;
            privatePropertiesCache?.DestroyComps();
            if(inheritablePropertiesCache != null)
            {
                foreach (var item in inheritablePropertiesCache)
                {
                    item.DestroyComps();
                }
            }
        }


        public Dictionary<string, Thing>? GenChilds(INodeProcesser actionNode, Dictionary<string, object?> cachedDataToPostUpatde)
        {
            // if(stopWatch.IsRunning) stopWatch.Restart();
            // else stopWatch.Start();
            // long ct = 0;
            // long lt = 0;
            readerWriterLockSlim.EnterWriteLock();
            NodeContainer? childs = ChildNodes;
            if (childs == null) return null;
            Dictionary<string, Thing> nextChild = new Dictionary<string, Thing>(childs.Count);
            if (Props.attachmentProperties.Count <= 0)
            {
                return nextChild;
            }
            foreach (var keyValue in childs)
            {
                #if V13 || V14 || V15
                if (keyValue.Item2.Destroyed)
                #else
                if (keyValue.Item2.Destroyed && childs.removeContentsIfDestroyed)
                #endif
                {
                    targetPartChanged = true;
                    if (swap)
                    {
                        cachedDataToPostUpatde["cachedSwap"] = null;
                        swap = false;
                    }
                }
                else
                {
                    nextChild[keyValue.Item1] = keyValue.Item2;
                }
            }
            if (!swap)
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

        public void PreUpdateChilds(INodeProcesser actionNode, Dictionary<string, object?> cachedDataToPostUpatde, ReadOnlyDictionary<string, Thing> prveChilds)
        {
            Map? swapMap = MapHeld;
            NodeContainer? childs = ChildNodes;
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
                        needUpdateVNode = true;
                        if(swapMap != null && kv.Value.Thing != null && !kv.Value.Thing.Spawned)
                        {
                            kv.Value.Thing.SpawnSetup(swapMap, false);
                        }
                    }
                }
                if (needUpdateVNode)
                {
                    targetPartChanged = true;
                    UpdateTargetPartVNode();
                }
                #endregion

                #region Restore previous parts
                targetPartsWithId = new Dictionary<string, LocalTargetInfo>(this.targetPartsWithId);
                foreach (var kv in targetPartsWithId)
                {
                    prveChilds.TryGetValue(kv.Key, out Thing? thing);
                    SetTargetPartInternal(kv.Key, thing, out _);
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


        public void PostUpdateChilds(INodeProcesser actionNode, Dictionary<string, object?> cachedDataFromPerUpdate, ReadOnlyDictionary<string, Thing> prveChilds)
        {
            // if(stopWatch.IsRunning) stopWatch.Restart();
            // else stopWatch.Start();
            // long ct = 0;
            // long lt = 0;
            try
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
                    targetPartChanged = false;
                }
                if (Props.attachmentProperties.Count > 0 && actionNode == this)
                {
                    if (swap) SwapAttachmentPropertiesCacheAndVNode();
                    else UpdateCurrentPartVNode();
                    InitAttachmentProperties();
                    if (swap) SwapCache();
                    else UpdateCache();
                }
                if (cachedDataFromPerUpdate.ContainsKey("cachedSwap"))
                {
                    swap = true;
                    ChildNodes.NeedUpdate = true;
                }
                else
                {
                    swap = false;
                    ChildNodes.NeedUpdate = false;
                }
            }
            finally
            {
                readerWriterLockSlim.ExitWriteLock();
            }

        }


        public void SetChildPostion(IntVec3? pos = null)
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

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.ChildNodes);
        }


        public ThingOwner GetDirectlyHeldThings() => ChildNodes;


        private bool swap = false;
        private bool targetPartChanged = false;
        private uint currentWeaponMode = 0;
        private ModularizationWeaponDefExtension? cachedProps = null;
        private ModularizationWeapon? occupiers = null;
        private List<string> targetPartsWithId_IdWorkingList = new List<string>();
        private List<LocalTargetInfo> targetPartsWithId_TargetWorkingList = new List<LocalTargetInfo>();
        private Dictionary<string, LocalTargetInfo> targetPartsWithId = new Dictionary<string, LocalTargetInfo>(); //part difference table

#region Swapedable cache
        private VNode? VNodeCache = null;
        private VNode? VNodeCache_TargetPart = null;
        private WeaponProperties? privatePropertiesCache = null;
        private WeaponProperties? privatePropertiesCache_TargetPart = null;
        private Graphic_ChildNode? graphicCache = null;
        private Graphic_ChildNode? graphicCache_TargetPart = null;
        private ReadOnlyCollection<CompProperties_ModularizationWeaponEquippable>? inheritablePropertiesCache = null;
        private ReadOnlyCollection<CompProperties_ModularizationWeaponEquippable>? inheritablePropertiesCache_TargetPart = null;
        private Dictionary<(StatDef, string?), float>? statOffsetCache = null;
        private Dictionary<(StatDef, string?), float>? statOffsetCache_TargetPart = null;
        private Dictionary<(StatDef, string?), float>? statMultiplierCache = null;
        private Dictionary<(StatDef, string?), float>? statMultiplierCache_TargetPart = null;
        private ReadOnlyDictionary<string, WeaponAttachmentProperties>? partAttachmentPropertiesCache = null;
        private ReadOnlyDictionary<string, WeaponAttachmentProperties>? partAttachmentPropertiesCache_TargetPart = null;
#endregion

        private readonly ReaderWriterLockSlim readerWriterLockSlim = new ReaderWriterLockSlim();
        private readonly HashSet<string> partIDs = new HashSet<string>();
        private readonly Dictionary<string, bool> childTreeViewOpend = new Dictionary<string, bool>();
        private static Material? PostFXMat = null;
        private static CommandBuffer? PostFXCommandBuffer = null;
        private static readonly Dictionary<Mesh, Mesh> MeshReindexed = new Dictionary<Mesh, Mesh>();
        internal static readonly AccessTools.FieldRef<ThingDef, List<VerbProperties>?> ThingDef_verbs = AccessTools.FieldRefAccess<ThingDef, List<VerbProperties>?>("verbs");
        
    }
}
