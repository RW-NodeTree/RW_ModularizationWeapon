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

            verbPropertiesOffseterAffectHorizon = verbPropertiesOffseterAffectHorizon ?? new FieldReaderDgit<VerbProperties>();
            verbPropertiesOffseterAffectHorizon.defaultValue = 1;
            toolsOffseterAffectHorizon = toolsOffseterAffectHorizon ?? new FieldReaderDgit<Tool>();
            toolsOffseterAffectHorizon.defaultValue = 1;

            verbPropertiesMultiplierAffectHorizon = verbPropertiesMultiplierAffectHorizon ?? new FieldReaderDgit<VerbProperties>();
            verbPropertiesMultiplierAffectHorizon.defaultValue = 1;
            toolsMultiplierAffectHorizon = toolsMultiplierAffectHorizon ?? new FieldReaderDgit<Tool>();
            toolsMultiplierAffectHorizon.defaultValue = 1;
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

        public bool verbPropertiesAffectByOtherPart;

        public bool toolsAffectByOtherPart;

        public int drawWeight;


        #region Offset
        public FieldReaderDgit<VerbProperties> verbPropertiesOffseterAffectHorizon = new FieldReaderDgit<VerbProperties>();

        public FieldReaderDgit<Tool> toolsOffseterAffectHorizon = new FieldReaderDgit<Tool>();

        public List<StatModifier> statOffsetAffectHorizon = new List<StatModifier>();
        #endregion


        #region Multiplier
        public FieldReaderDgit<VerbProperties> verbPropertiesMultiplierAffectHorizon = new FieldReaderDgit<VerbProperties>();

        public FieldReaderDgit<Tool> toolsMultiplierAffectHorizon = new FieldReaderDgit<Tool>();

        public List<StatModifier> statMultiplierAffectHorizon = new List<StatModifier>();
        #endregion


        private Material materialCache;
    }
}
