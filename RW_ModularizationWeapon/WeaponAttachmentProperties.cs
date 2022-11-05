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

            #region Offseter
            #region Child
            verbPropertiesOffseterAffectHorizon = verbPropertiesOffseterAffectHorizon ?? new List<FieldReaderDgit<VerbProperties>>();
            verbPropertiesOffseterAffectHorizon.RemoveAll(f => f == null);
            verbPropertiesOffseterAffectHorizon.ForEach(f => f.defaultValue = verbPropertiesOffseterAffectHorizonDefaultValue);
            toolsOffseterAffectHorizon = toolsOffseterAffectHorizon ?? new List<FieldReaderDgit<Tool>>();
            toolsOffseterAffectHorizon.RemoveAll(f => f == null);
            toolsOffseterAffectHorizon.ForEach(f => f.defaultValue = toolsOffseterAffectHorizonDefaultValue);
            #endregion

            #region OtherPart

            #endregion
            #endregion


            #region Offseter
            #region Child
            verbPropertiesMultiplierAffectHorizon = verbPropertiesMultiplierAffectHorizon ?? new List<FieldReaderDgit<VerbProperties>>();
            verbPropertiesMultiplierAffectHorizon.RemoveAll(f => f == null);
            verbPropertiesMultiplierAffectHorizon.ForEach(f => f.defaultValue = verbPropertiesMultiplierAffectHorizonDefaultValue);
            toolsMultiplierAffectHorizon = toolsMultiplierAffectHorizon ?? new List<FieldReaderDgit<Tool>>();
            toolsMultiplierAffectHorizon.RemoveAll(f => f == null);
            toolsMultiplierAffectHorizon.ForEach(f => f.defaultValue = toolsMultiplierAffectHorizonDefaultValue);
            #endregion

            #region OtherPart

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

        public float verbPropertiesOffseterAffectHorizonDefaultValue = 1;

        public float toolsOffseterAffectHorizonDefaultValue = 1;

        public float verbPropertiesOtherPartOffseterAffectHorizonDefaultValue = 1;

        public float toolsOffseterOtherPartAffectHorizonDefaultValue = 1;

        public float statOffsetAffectHorizonDefaultValue = 1;

        public float verbPropertiesMultiplierAffectHorizonDefaultValue = 1;

        public float toolsMultiplierAffectHorizonDefaultValue = 1;

        public float verbPropertiesOtherPartMultiplierAffectHorizonDefaultValue = 1;

        public float toolsOtherPartMultiplierAffectHorizonDefaultValue = 1;

        public float statMultiplierAffectHorizonDefaultValue = 1;

        public bool verbPropertiesAffectByOtherPart;

        public bool toolsAffectByOtherPart;

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
        #endregion

        #region OtherPart
        public Dictionary<string, List<FieldReaderDgit<VerbProperties>>> verbPropertiesOtherPartOffseterAffectHorizon = new Dictionary<string, List<FieldReaderDgit<VerbProperties>>>();

        public Dictionary<string, List<FieldReaderDgit<Tool>>> toolsOffseterOtherPartAffectHorizon = new Dictionary<string, List<FieldReaderDgit<Tool>>>();
        #endregion
        #endregion


        #region Multiplier
        #region Child
        public List<FieldReaderDgit<VerbProperties>> verbPropertiesMultiplierAffectHorizon = new List<FieldReaderDgit<VerbProperties>>();

        public List<FieldReaderDgit<Tool>> toolsMultiplierAffectHorizon = new List<FieldReaderDgit<Tool>>();

        public List<StatModifier> statMultiplierAffectHorizon = new List<StatModifier>();
        #endregion

        #region OtherPart
        public Dictionary<string, List<FieldReaderDgit<VerbProperties>>> verbPropertiesOtherPartMultiplierAffectHorizon = new Dictionary<string, List<FieldReaderDgit<VerbProperties>>>();

        public Dictionary<string, List<FieldReaderDgit<Tool>>> toolsOtherPartMultiplierAffectHorizon = new Dictionary<string, List<FieldReaderDgit<Tool>>>();
        #endregion
        #endregion


        private Material materialCache;
    }
}
