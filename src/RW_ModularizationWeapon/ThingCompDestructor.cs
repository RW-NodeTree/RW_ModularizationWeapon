using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using UnityEngine;
using Verse;


namespace RW_ModularizationWeapon
{
    public interface IThingCompDestructor
    {
        void DestroyComp(ModularizationWeapon modularizationWeapon, ThingComp comp);
    }
}