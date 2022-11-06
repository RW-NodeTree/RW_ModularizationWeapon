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

            void CheckAndSetList<T>(ref List<FieldReaderDgit<T>> list,float defaultValue)
            {
                list = list ?? new List<FieldReaderDgit<T>>();
                list.RemoveAll(f => f == null);
                list.ForEach(f =>
                {
                    if (!f.HasDefaultValue) f.DefaultValue = defaultValue;
                });
            }


            void CheckAndSetDictionary<T>(ref Dictionary<string, List<FieldReaderDgit<T>>> list, float defaultValue)
            {
                list = list ?? new Dictionary<string, List<FieldReaderDgit<T>>>();
                list.RemoveAll(f => f.Key == null || f.Value == null);
                foreach((string id, List<FieldReaderDgit<T>> innerList) in list)
                {
                    innerList.RemoveAll(f => f == null);
                    innerList.ForEach(f =>
                    {
                        if (!f.HasDefaultValue) f.DefaultValue = defaultValue;
                    });
                }
            }

            #region Offseter
            #region Child
            CheckAndSetList(ref verbPropertiesOffseterAffectHorizon, verbPropertiesOffseterAffectHorizonDefaultValue);
            CheckAndSetList(ref toolsOffseterAffectHorizon, toolsOffseterAffectHorizonDefaultValue);
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
            CheckAndSetList(ref verbPropertiesMultiplierAffectHorizon, verbPropertiesMultiplierAffectHorizonDefaultValue);
            CheckAndSetList(ref toolsMultiplierAffectHorizon, toolsMultiplierAffectHorizonDefaultValue);
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
        public List<FieldReaderDgit<VerbProperties>> verbPropertiesOffseterAffectHorizon = new List<FieldReaderDgit<VerbProperties>>();

        public List<FieldReaderDgit<Tool>> toolsOffseterAffectHorizon = new List<FieldReaderDgit<Tool>>();

        public List<StatModifier> statOffsetAffectHorizon = new List<StatModifier>();

        public float verbPropertiesOffseterAffectHorizonDefaultValue = 1;

        public float toolsOffseterAffectHorizonDefaultValue = 1;

        public float statOffsetAffectHorizonDefaultValue = 1;
        #endregion

        #region OtherPart
        public Dictionary<string, List<FieldReaderDgit<VerbProperties>>> verbPropertiesOtherPartOffseterAffectHorizon = new Dictionary<string, List<FieldReaderDgit<VerbProperties>>>();

        public Dictionary<string, List<FieldReaderDgit<Tool>>> toolsOtherPartOffseterAffectHorizon = new Dictionary<string, List<FieldReaderDgit<Tool>>>();

        public Dictionary<string, List<StatModifier>> statOtherPartOffseterAffectHorizon = new Dictionary<string, List<StatModifier>>();

        public float verbPropertiesOtherPartOffseterAffectHorizonDefaultValue = 1;

        public float toolsOtherPartOffseterAffectHorizonDefaultValue = 1;

        public float statOtherPartOffseterAffectHorizonDefaultValue = 1;
        #endregion
        #endregion


        #region Multiplier
        #region Child
        public List<FieldReaderDgit<VerbProperties>> verbPropertiesMultiplierAffectHorizon = new List<FieldReaderDgit<VerbProperties>>();

        public List<FieldReaderDgit<Tool>> toolsMultiplierAffectHorizon = new List<FieldReaderDgit<Tool>>();

        public List<StatModifier> statMultiplierAffectHorizon = new List<StatModifier>();

        public float verbPropertiesMultiplierAffectHorizonDefaultValue = 1;

        public float toolsMultiplierAffectHorizonDefaultValue = 1;

        public float statMultiplierAffectHorizonDefaultValue = 1;
        #endregion

        #region OtherPart
        public Dictionary<string, List<FieldReaderDgit<VerbProperties>>> verbPropertiesOtherPartMultiplierAffectHorizon = new Dictionary<string, List<FieldReaderDgit<VerbProperties>>>();

        public Dictionary<string, List<FieldReaderDgit<Tool>>> toolsOtherPartMultiplierAffectHorizon = new Dictionary<string, List<FieldReaderDgit<Tool>>>();

        public Dictionary<string, List<StatModifier>> statOtherPartMultiplierAffectHorizon = new Dictionary<string, List<StatModifier>>();

        public float verbPropertiesOtherPartMultiplierAffectHorizonDefaultValue = 1;

        public float toolsOtherPartMultiplierAffectHorizonDefaultValue = 1;

        public float statOtherPartMultiplierAffectHorizonDefaultValue = 1;
        #endregion
        #endregion


        private Material materialCache;
    }
}
