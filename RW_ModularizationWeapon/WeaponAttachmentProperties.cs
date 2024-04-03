using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RW_ModularizationWeapon
{
    /// <summary>
    /// Properties of attach point
    /// </summary>
    public class WeaponAttachmentProperties
    {

        public Texture2D UITexture
        {
            get
            {
                if(cachedUITex == null && !UITexPath.NullOrEmpty()) cachedUITex = ContentFinder<Texture2D>.Get(UITexPath) ?? BaseContent.BadTex;
                return cachedUITex;
            }
        }

        public Matrix4x4 Transfrom => Matrix4x4.TRS(postion, Quaternion.Euler(rotation), scale);

        public string Name => name ?? id;


        public void ResolveReferences()
        {
            filter = filter ?? new ThingFilter();
            filter.ResolveReferences();

            randomThingDefWeights = randomThingDefWeights ?? new List<ThingDefCountClass>();
            randomThingDefWeights.RemoveAll(x => x == null || x.thingDef == null || !filter.Allows(x.thingDef));

            allowedExtraCompType = allowedExtraCompType ?? new List<Type>();
            allowedExtraCompType.RemoveAll(x => x == null);


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


            void CheckAndSetDictionaryDgit<T>(ref Dictionary<string, FieldReaderDgitList<T>> list, float defaultValue)
            {
                list = list ?? new Dictionary<string, FieldReaderDgitList<T>>();
                list.RemoveAll(f => f.Key == null || f.Value == null);
                foreach(KeyValuePair<string, FieldReaderDgitList<T>> data in list)
                {
                    data.Value.RemoveAll(f => f == null);
                    if (!data.Value.HasDefaultValue) data.Value.DefaultValue = defaultValue;
                }
            }


            void CheckAndSetDictionaryBool<T>(ref Dictionary<string, FieldReaderBoolList<T>> list, bool defaultValue)
            {
                list = list ?? new Dictionary<string, FieldReaderBoolList<T>>();
                list.RemoveAll(f => f.Key == null || f.Value == null);
                foreach (KeyValuePair<string, FieldReaderBoolList<T>> data in list)
                {
                    data.Value.RemoveAll(f => f == null);
                    if (!data.Value.HasDefaultValue) data.Value.DefaultValue = defaultValue;
                }
            }


            void CheckAndSetDictionaryFilt<T>(ref Dictionary<string, FieldReaderFiltList<T>> list, bool defaultValue)
            {
                list = list ?? new Dictionary<string, FieldReaderFiltList<T>>();
                list.RemoveAll(f => f.Key == null || f.Value == null);
                foreach (KeyValuePair<string, FieldReaderFiltList<T>> data in list)
                {
                    data.Value.RemoveAll(f => f == null);
                    if (!data.Value.HasDefaultValue) data.Value.DefaultValue = defaultValue;
                }
            }
            #endregion


            #region Offseter
            #region Child
            CheckAndSetDgitList(ref verbPropertiesOffseterAffectHorizon, 1);
            CheckAndSetDgitList(ref toolsOffseterAffectHorizon, 1);
            CheckAndSetDgitList(ref compPropertiesOffseterAffectHorizon, 1);
            #endregion

            #region OtherPart
            CheckAndSetDictionaryDgit(ref verbPropertiesOtherPartOffseterAffectHorizon, verbPropertiesOtherPartOffseterAffectHorizonDefaultValue);
            CheckAndSetDictionaryDgit(ref toolsOtherPartOffseterAffectHorizon, toolsOtherPartOffseterAffectHorizonDefaultValue);
            statOtherPartOffseterAffectHorizon = statOtherPartOffseterAffectHorizon ?? new Dictionary<string, List<StatModifier>>();
            statOtherPartOffseterAffectHorizon.RemoveAll(x => x.Key == null || x.Value == null);
            #endregion
            #endregion


            #region Offseter
            #region Child
            CheckAndSetDgitList(ref verbPropertiesMultiplierAffectHorizon, 1);
            CheckAndSetDgitList(ref toolsMultiplierAffectHorizon, 1);
            CheckAndSetDgitList(ref compPropertiesMultiplierAffectHorizon, 1);
            #endregion

            #region OtherPart
            CheckAndSetDictionaryDgit(ref verbPropertiesOtherPartMultiplierAffectHorizon, verbPropertiesOtherPartMultiplierAffectHorizonDefaultValue);
            CheckAndSetDictionaryDgit(ref toolsOtherPartMultiplierAffectHorizon, toolsOtherPartMultiplierAffectHorizonDefaultValue);
            statOtherPartMultiplierAffectHorizon = statOtherPartMultiplierAffectHorizon ?? new Dictionary<string, List<StatModifier>>();
            statOtherPartMultiplierAffectHorizon.RemoveAll(x => x.Key == null || x.Value == null);
            #endregion
            #endregion


            #region AndPatch
            #region Child
            CheckAndSetBoolList(ref verbPropertiesBoolAndPatchByChildPart, true);
            CheckAndSetBoolList(ref toolsBoolAndPatchByChildPart, true);
            CheckAndSetBoolList(ref compPropertiesBoolAndPatchByChildPart, true);
            #endregion

            #region OtherPart
            CheckAndSetDictionaryBool(ref verbPropertiesBoolAndPatchByOtherPart, verbPropertiesBoolAndPatchByOtherPartDefaultValue);
            CheckAndSetDictionaryBool(ref toolsBoolAndPatchByOtherPart, toolsBoolAndPatchByOtherPartDefaultValue);
            #endregion
            #endregion


            #region OrPatch
            #region Child
            CheckAndSetBoolList(ref verbPropertiesBoolOrPatchByChildPart, false);
            CheckAndSetBoolList(ref toolsBoolOrPatchByChildPart, false);
            CheckAndSetBoolList(ref compPropertiesBoolOrPatchByChildPart, false);
            #endregion

            #region OtherPart
            CheckAndSetDictionaryBool(ref verbPropertiesBoolOrPatchByOtherPart, verbPropertiesBoolOrPatchByOtherPartDefaultValue);
            CheckAndSetDictionaryBool(ref toolsBoolOrPatchByOtherPart, toolsBoolOrPatchByOtherPartDefaultValue);
            #endregion
            #endregion


            #region ObjectPatch
            #region Child
            CheckAndSetFiltList(ref verbPropertiesObjectPatchByChildPart, true);
            CheckAndSetFiltList(ref toolsObjectPatchByChildPart, true);
            CheckAndSetFiltList(ref compPropertiesObjectPatchByChildPart, true);
            #endregion

            #region OtherPart
            CheckAndSetDictionaryFilt(ref verbPropertiesObjectPatchByOtherPart, verbPropertiesObjectPatchByOtherPartDefaultValue);
            CheckAndSetDictionaryFilt(ref toolsObjectPatchByOtherPart, toolsObjectPatchByOtherPartDefaultValue);
            #endregion
            #endregion
        }

        /// <summary>
        /// attach point unique id
        /// </summary>
        public string id;
        /// <summary>
        /// attach point name
        /// </summary>
        public string name;
        /// <summary>
        /// what thing can attach on this attach point
        /// </summary>
        public ThingFilter filter = new ThingFilter();
        /// <summary>
        /// defultThing when create part instance, if `CompProperties_ModularizationWeapon.setRandomPartWhenCreate` is `false` or create by crafting
        /// </summary>
        public ThingDef defultThing = null;
        /// <summary>
        /// random part weight of this attach point
        /// </summary>
        public List<ThingDefCountClass> randomThingDefWeights = new List<ThingDefCountClass>();
        /// <summary>
        /// attach point drawing postion
        /// </summary>
        public Vector3 postion = Vector3.zero;
        /// <summary>
        /// attach point drawing rotation(Euler)
        /// </summary>
        public Vector3 rotation = Vector3.zero;
        /// <summary>
        /// attach point drawing scale
        /// </summary>
        public Vector3 scale = Vector3.one;
        /// <summary>
        /// attach point ICON textrue path
        /// </summary>
        public string UITexPath;
        /// <summary>
        /// if it's **`true`**, it's able to set empty part on this attach point
        /// </summary>
        public bool allowEmpty;
        /// <summary>
        /// not allow this attach point set to other attachment
        /// </summary>
        public bool unchangeable;
        /// <summary>
        /// if it's **`true`**, this attach point and it's child attach point will not rendering
        /// </summary>
        public bool notDraw;
        /// <summary>
        /// if it's **`true`**,the parent part will not able to read `IVerbOwner.Tools` from this attach point
        /// </summary>
        public bool notUseTools;
        /// <summary>
        /// if it's **`true`**,the parent part will not able to read `IVerbOwner.VerbProperties` from this attach point
        /// </summary>
        public bool notUseVerbProperties;
        /// <summary>
        /// if is's **`true`**,the randering postion of this part will offset by the pixel scale
        /// </summary>
        public bool postionInPixelSize;
        /// <summary>
        /// drawing weight of this attach point
        /// </summary>
        public int drawWeight;
        /// <summary>
        /// allowed extra ThingComp type that will append on perent
        /// </summary>
        public List<Type> allowedExtraCompType = new List<Type>();


        #region Offset
        #region Child
        public FieldReaderDgitList<VerbProperties> verbPropertiesOffseterAffectHorizon = new FieldReaderDgitList<VerbProperties>();

        public FieldReaderDgitList<Tool> toolsOffseterAffectHorizon = new FieldReaderDgitList<Tool>();

        public FieldReaderDgitList<CompProperties> compPropertiesOffseterAffectHorizon = new FieldReaderDgitList<CompProperties>();

        public List<StatModifier> statOffsetAffectHorizon = new List<StatModifier>();

        public float statOffsetAffectHorizonDefaultValue = 1;
        #endregion

        #region OtherPart
        public Dictionary<string, FieldReaderDgitList<VerbProperties>> verbPropertiesOtherPartOffseterAffectHorizon = new Dictionary<string, FieldReaderDgitList<VerbProperties>>();

        public Dictionary<string, FieldReaderDgitList<Tool>> toolsOtherPartOffseterAffectHorizon = new Dictionary<string, FieldReaderDgitList<Tool>>();

        public Dictionary<string, List<StatModifier>> statOtherPartOffseterAffectHorizon = new Dictionary<string, List<StatModifier>>();

        public float verbPropertiesOtherPartOffseterAffectHorizonDefaultValue = 1;

        public float toolsOtherPartOffseterAffectHorizonDefaultValue = 1;

        public float statOtherPartOffseterAffectHorizonDefaultValue = 1;
        #endregion
        #endregion


        #region Multiplier
        #region Child
        public FieldReaderDgitList<VerbProperties> verbPropertiesMultiplierAffectHorizon = new FieldReaderDgitList<VerbProperties>();

        public FieldReaderDgitList<Tool> toolsMultiplierAffectHorizon = new FieldReaderDgitList<Tool>();

        public FieldReaderDgitList<CompProperties> compPropertiesMultiplierAffectHorizon = new FieldReaderDgitList<CompProperties>();

        public List<StatModifier> statMultiplierAffectHorizon = new List<StatModifier>();

        public float statMultiplierAffectHorizonDefaultValue = 1;
        #endregion

        #region OtherPart
        public Dictionary<string, FieldReaderDgitList<VerbProperties>> verbPropertiesOtherPartMultiplierAffectHorizon = new Dictionary<string, FieldReaderDgitList<VerbProperties>>();

        public Dictionary<string, FieldReaderDgitList<Tool>> toolsOtherPartMultiplierAffectHorizon = new Dictionary<string, FieldReaderDgitList<Tool>>();

        public Dictionary<string, List<StatModifier>> statOtherPartMultiplierAffectHorizon = new Dictionary<string, List<StatModifier>>();

        public float verbPropertiesOtherPartMultiplierAffectHorizonDefaultValue = 1;

        public float toolsOtherPartMultiplierAffectHorizonDefaultValue = 1;

        public float statOtherPartMultiplierAffectHorizonDefaultValue = 1;
        #endregion
        #endregion


        #region AndPatch
        //default true
        #region Child
        public FieldReaderBoolList<VerbProperties> verbPropertiesBoolAndPatchByChildPart = new FieldReaderBoolList<VerbProperties>();

        public FieldReaderBoolList<Tool> toolsBoolAndPatchByChildPart = new FieldReaderBoolList<Tool>();

        public FieldReaderBoolList<CompProperties> compPropertiesBoolAndPatchByChildPart = new FieldReaderBoolList<CompProperties>();
        #endregion

        #region OtherPart
        public Dictionary<string, FieldReaderBoolList<VerbProperties>> verbPropertiesBoolAndPatchByOtherPart = new Dictionary<string, FieldReaderBoolList<VerbProperties>>();

        public Dictionary<string, FieldReaderBoolList<Tool>> toolsBoolAndPatchByOtherPart = new Dictionary<string, FieldReaderBoolList<Tool>>();

        public bool verbPropertiesBoolAndPatchByOtherPartDefaultValue = true;

        public bool toolsBoolAndPatchByOtherPartDefaultValue = true;
        #endregion
        #endregion


        #region OrPatch
        //defaule false
        #region Child
        public FieldReaderBoolList<VerbProperties> verbPropertiesBoolOrPatchByChildPart = new FieldReaderBoolList<VerbProperties>();

        public FieldReaderBoolList<Tool> toolsBoolOrPatchByChildPart = new FieldReaderBoolList<Tool>();

        public FieldReaderBoolList<CompProperties> compPropertiesBoolOrPatchByChildPart = new FieldReaderBoolList<CompProperties>();
        #endregion

        #region OtherPart
        public Dictionary<string, FieldReaderBoolList<VerbProperties>> verbPropertiesBoolOrPatchByOtherPart = new Dictionary<string, FieldReaderBoolList<VerbProperties>>();

        public Dictionary<string, FieldReaderBoolList<Tool>> toolsBoolOrPatchByOtherPart = new Dictionary<string, FieldReaderBoolList<Tool>>();

        public bool verbPropertiesBoolOrPatchByOtherPartDefaultValue = false;

        public bool toolsBoolOrPatchByOtherPartDefaultValue = false;
        #endregion
        #endregion


        #region ObjectPatch
        #region Child
        public FieldReaderFiltList<VerbProperties> verbPropertiesObjectPatchByChildPart = new FieldReaderFiltList<VerbProperties>();

        public FieldReaderFiltList<Tool> toolsObjectPatchByChildPart = new FieldReaderFiltList<Tool>();

        public FieldReaderFiltList<CompProperties> compPropertiesObjectPatchByChildPart = new FieldReaderFiltList<CompProperties>();
        #endregion

        #region OtherPart
        public Dictionary<string, FieldReaderFiltList<VerbProperties>> verbPropertiesObjectPatchByOtherPart = new Dictionary<string, FieldReaderFiltList<VerbProperties>>();

        public Dictionary<string, FieldReaderFiltList<Tool>> toolsObjectPatchByOtherPart = new Dictionary<string, FieldReaderFiltList<Tool>>();

        public bool verbPropertiesObjectPatchByOtherPartDefaultValue = true;

        public bool toolsObjectPatchByOtherPartDefaultValue = true;
        #endregion
        #endregion


        private Texture2D cachedUITex = null;
    }
}
