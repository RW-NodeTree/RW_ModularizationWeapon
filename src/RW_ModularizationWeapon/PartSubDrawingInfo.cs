using UnityEngine;
using Verse;

namespace RW_ModularizationWeapon
{
    public class PartSubDrawingInfo
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
                    if (!PartTexPath.NullOrEmpty())
                    {
                        partTexCache = ContentFinder<Texture2D>.Get(PartTexPath);
                    }
                    partTexCache ??= BaseContent.BadTex;
                }
                return partTexCache;
            }
        }

        public Matrix4x4 Transfrom => Matrix4x4.TRS(postion, Quaternion.Euler(rotation), scale);


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
        /// special drawing texture when it attach on a part
        /// </summary>
        public string? PartTexPath = null;
        /// <summary>
        /// material cache of `PartTexPath`
        /// </summary>
        private Texture2D? partTexCache;
    }
}
