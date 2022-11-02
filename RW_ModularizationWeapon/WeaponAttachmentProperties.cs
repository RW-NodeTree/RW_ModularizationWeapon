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
        public float armorPenetrationOffsetAffectHorizon = 1;

        public float meleeCooldownTimeOffsetAffectHorizon = 1;

        public float meleeDamageOffsetAffectHorizon = 1;

        public float burstShotCountOffsetAffectHorizon = 1;

        public float ticksBetweenBurstShotsOffsetAffectHorizon = 1;

        public float muzzleFlashScaleOffsetAffectHorizon = 1;

        public float rangeOffsetAffectHorizon = 1;

        public float warmupTimeOffsetAffectHorizon = 1;

        public List<StatModifier> statOffsetAffectHorizon = new List<StatModifier>();
        #endregion


        #region Multiplier
        public float armorPenetrationMultiplierAffectHorizon = 1;

        public float meleeCooldownTimeMultiplierAffectHorizon = 1;

        public float meleeDamageMultiplierAffectHorizon = 1;

        public float burstShotCountMultiplierAffectHorizon = 1;

        public float ticksBetweenBurstShotsMultiplierAffectHorizon = 1;

        public float muzzleFlashScaleMultiplierAffectHorizon = 1;

        public float rangeMultiplierAffectHorizon = 1;

        public float warmupTimeMultiplierAffectHorizon = 1;

        public List<StatModifier> statMultiplierAffectHorizon = new List<StatModifier>();
        #endregion


        private Material materialCache;
    }
}
