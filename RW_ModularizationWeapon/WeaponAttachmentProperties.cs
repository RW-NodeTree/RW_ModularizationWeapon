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
    public class WeaponAttachmentProperties
    {
        public Material UITexMaterial
        {
            get
            {
                if(materialCache == null)
                {
                    materialCache = new Material(ShaderDatabase.Cutout);
                    Texture2D texture = (!UITexPath.NullOrEmpty()) ? ContentFinder<Texture2D>.Get(UITexPath) : BaseContent.BadTex;
                    materialCache.mainTexture = texture;
                }
                return materialCache;
            }
        }

        public Texture2D UITexture => UITexMaterial.mainTexture as Texture2D;

        public Matrix4x4 Transfrom => Matrix4x4.TRS(postion, Quaternion.Euler(rotation), scale);

        public string Name => name ?? id;


        public void ResolveReferences()
        {
            filter = filter ?? new ThingFilter();
            filter.ResolveReferences();

            void CheckAndSetList<T>(ref FieldReaderDgitList<T> list, float defaultValue)
            {
                list = list ?? new FieldReaderDgitList<T>();
                list.RemoveAll(f => f == null);
                if (!list.HasDefaultValue) list.DefaultValue = defaultValue;
            }


            void CheckAndSetDictionary<T>(ref Dictionary<string, FieldReaderDgitList<T>> list, float defaultValue)
            {
                list = list ?? new Dictionary<string, FieldReaderDgitList<T>>();
                list.RemoveAll(f => f.Key == null || f.Value == null);
                foreach((string id, FieldReaderDgitList<T> innerList) in list)
                {
                    innerList.RemoveAll(f => f == null);
                    if (!innerList.HasDefaultValue) innerList.DefaultValue = defaultValue;
                }
            }

            #region Offseter
            #region Child
            CheckAndSetList(ref verbPropertiesOffseterAffectHorizon, 1);
            CheckAndSetList(ref toolsOffseterAffectHorizon, 1);
            #endregion

            #region OtherPart
            CheckAndSetDictionary(ref verbPropertiesOtherPartOffseterAffectHorizon, verbPropertiesOtherPartOffseterAffectHorizonDefaultValue);
            CheckAndSetDictionary(ref toolsOtherPartOffseterAffectHorizon, toolsOtherPartOffseterAffectHorizonDefaultValue);
            statOtherPartOffseterAffectHorizon = statOtherPartOffseterAffectHorizon ?? new Dictionary<string, List<StatModifier>>();
            statOtherPartOffseterAffectHorizon.RemoveAll(x => x.Key == null || x.Value == null);
            #endregion
            #endregion


            #region Offseter
            #region Child
            CheckAndSetList(ref verbPropertiesMultiplierAffectHorizon, 1);
            CheckAndSetList(ref toolsMultiplierAffectHorizon, 1);
            #endregion

            #region OtherPart
            CheckAndSetDictionary(ref verbPropertiesOtherPartMultiplierAffectHorizon, verbPropertiesOtherPartMultiplierAffectHorizonDefaultValue);
            CheckAndSetDictionary(ref toolsOtherPartMultiplierAffectHorizon, toolsOtherPartMultiplierAffectHorizonDefaultValue);
            statOtherPartMultiplierAffectHorizon = statOtherPartMultiplierAffectHorizon ?? new Dictionary<string, List<StatModifier>>();
            statOtherPartMultiplierAffectHorizon.RemoveAll(x => x.Key == null || x.Value == null);
            #endregion
            #endregion
        }


        public string id;

        public string name;

        public ThingFilter filter = new ThingFilter();

        public ThingDef defultThing = null;

        public Vector3 postion = Vector3.zero;
        
        public Vector3 rotation = Vector3.zero;

        public Vector3 scale = Vector3.one;

        public string UITexPath;

        public bool allowEmpty;

        public bool unchangeable;

        public bool notDraw;

        public bool notUseTools;

        public bool notUseVerbProperties;

        public int drawWeight;


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


        #region Patch
        #region Object
        public bool verbPropertiesObjectPatchByChildPart = true;

        public bool toolsObjectPatchByChildPart = true;

        public bool compPropertiesObjectPatchByChildPart = true;

        public bool verbPropertiesObjectPatchByOtherPart = false;

        public bool toolsObjectPatchByOtherPart = false;
        #endregion

        #region And
        public bool verbPropertiesBoolAndPatchByChildPart = true;

        public bool toolsBoolAndPatchByChildPart = true;

        public bool compPropertiesBoolAndPatchByChildPart = true;

        public bool verbPropertiesBoolAndPatchByOtherPart = false;

        public bool toolsBoolAndPatchByOtherPart = false;
        #endregion

        #region And
        public bool verbPropertiesBoolOrPatchByChildPart = true;

        public bool toolsBoolOrPatchByChildPart = true;

        public bool compPropertiesBoolOrPatchByChildPart = true;

        public bool verbPropertiesBoolOrPatchByOtherPart = false;

        public bool toolsBoolOrPatchByOtherPart = false;
        #endregion
        #endregion

        private Material materialCache;
    }
}
