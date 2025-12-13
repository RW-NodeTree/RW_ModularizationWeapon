using RimWorld;
using RW_ModularizationWeapon.Tools;
using RW_NodeTree;
using System;
using System.Collections.Generic;
using Verse;

namespace RW_ModularizationWeapon
{
    public partial class ModularizationWeapon
    {


        #region Multiplier

        public FieldReaderDgitList<VerbProperties> VerbPropertiesMultiplier(string? childNodeIdForVerbProperties)
        {
            NodeContainer? container = ChildNodes;
            if (container == null) throw new NullReferenceException(nameof(ChildNodes));
            FieldReaderDgitList<VerbProperties> results = new FieldReaderDgitList<VerbProperties>();
            WeaponAttachmentProperties? current = CurrentPartWeaponAttachmentPropertiesById(childNodeIdForVerbProperties);
            ModularizationWeapon? currentWeapon = childNodeIdForVerbProperties != null ? container[childNodeIdForVerbProperties] as ModularizationWeapon : null;
            results.DefaultValue = 1;
            for (int i = 0; i < container.Count; i++)
            {
                string id = ((IList<string>)container)[i];
                ModularizationWeapon? weapon = container[i] as ModularizationWeapon;
                WeaponAttachmentProperties? properties = CurrentPartWeaponAttachmentPropertiesById(id);
                if (weapon != null && properties != null && id != childNodeIdForVerbProperties)
                {
                    FieldReaderDgitList<VerbProperties> cache = properties.verbPropertiesMultiplierAffectHorizon;

                    if (current != null)
                    {
                        double defaultValue = cache.DefaultValue;
                        cache *=
                            current.verbPropertiesOtherPartMultiplierAffectHorizon
                            .GetOrNewWhenNull(
                                id,
                                delegate ()
                                {
                                    FieldReaderDgitList<VerbProperties> result = new FieldReaderDgitList<VerbProperties>();
                                    result.DefaultValue = current.verbPropertiesOtherPartMultiplierAffectHorizonDefaultValue;
                                    return result;
                                }
                            );
                        defaultValue *= current.verbPropertiesOtherPartMultiplierAffectHorizonDefaultValue;
                        cache.DefaultValue = defaultValue;
                        if (currentWeapon != null && weapon != currentWeapon)
                        {
                            cache *= currentWeapon.Props.verbPropertiesOtherPartMultiplierAffectHorizon;
                            cache.DefaultValue = defaultValue * currentWeapon.Props.verbPropertiesOtherPartMultiplierAffectHorizon.DefaultValue;
                        }
                    }

                    FieldReaderDgitList<VerbProperties> mul = weapon.Props.verbPropertiesMultiplier - 1f;
                    if (mul.HasDefaultValue) mul.DefaultValue--;
                    mul = (mul * cache + 1) ?? mul;
                    mul.DefaultValue = 1;
                    results *= mul;
                    results.DefaultValue = 1;

                    mul = weapon.VerbPropertiesMultiplier(null) - 1;
                    if (mul.HasDefaultValue) mul.DefaultValue--;
                    mul = (mul * cache + 1) ?? mul;
                    mul.DefaultValue = 1;
                    results *= mul;
                    results.DefaultValue = 1;

                    //result *= (comp.Props.verbPropertiesMultiplier - 1f) * properties.verbPropertiesMultiplierAffectHorizon + 1f;
                }
            }
            //Log.Message($" Final {this}.VerbPropertiesMultiplier({childNodeIdForVerbProperties}) :\nresults : {results}");
            return results;
        }


        public FieldReaderDgitList<Tool> ToolsMultiplier(string? childNodeIdForTool)
        {
            NodeContainer? container = ChildNodes;
            if (container == null) throw new NullReferenceException(nameof(ChildNodes));
            FieldReaderDgitList<Tool> results = new FieldReaderDgitList<Tool>();
            WeaponAttachmentProperties? current = CurrentPartWeaponAttachmentPropertiesById(childNodeIdForTool);
            ModularizationWeapon? currentWeapon = childNodeIdForTool != null ? container[childNodeIdForTool] as ModularizationWeapon : null;
            results.DefaultValue = 1;
            for (int i = 0; i < container.Count; i++)
            {
                string id = ((IList<string>)container)[i];
                ModularizationWeapon? weapon = container[i] as ModularizationWeapon;
                WeaponAttachmentProperties? properties = CurrentPartWeaponAttachmentPropertiesById(id);
                if (weapon != null && properties != null && id != childNodeIdForTool)
                {
                    FieldReaderDgitList<Tool> cache = properties.toolsMultiplierAffectHorizon;

                    if (current != null)
                    {
                        double defaultValue = cache.DefaultValue;
                        cache *=
                            current.toolsOtherPartMultiplierAffectHorizon
                            .GetOrNewWhenNull(
                                id,
                                delegate ()
                                {
                                    FieldReaderDgitList<Tool> result = new FieldReaderDgitList<Tool>();
                                    result.DefaultValue = current.toolsOtherPartMultiplierAffectHorizonDefaultValue;
                                    return result;
                                }
                            );
                        defaultValue *= current.toolsOtherPartMultiplierAffectHorizonDefaultValue;
                        cache.DefaultValue = defaultValue;
                        if (currentWeapon != null && weapon != currentWeapon)
                        {
                            cache *= currentWeapon.Props.toolsOtherPartMultiplierAffectHorizon;
                            cache.DefaultValue = defaultValue * currentWeapon.Props.toolsOtherPartMultiplierAffectHorizon.DefaultValue;
                        }
                    }

                    FieldReaderDgitList<Tool> mul = weapon.Props.toolsMultiplier - 1f;
                    if (mul.HasDefaultValue) mul.DefaultValue--;
                    mul = (mul * cache + 1) ?? mul;
                    mul.DefaultValue = 1;
                    results *= mul;
                    results.DefaultValue = 1;

                    mul = weapon.ToolsMultiplier(null) - 1;
                    if (mul.HasDefaultValue) mul.DefaultValue--;
                    mul = (mul * cache + 1) ?? mul;
                    mul.DefaultValue = 1;
                    results *= mul;
                    results.DefaultValue = 1;
                }
            }
            //Log.Message($"{this}.ToolsMultiplier({childNodeIdForTool}) :\nresults : {results}");
            return results;
        }


        public FieldReaderDgitList<CompProperties> CompPropertiesMultiplier(string? childNodeIdForCompProperties)
        {
            NodeContainer? container = ChildNodes;
            if (container == null) throw new NullReferenceException(nameof(ChildNodes));
            FieldReaderDgitList<CompProperties> results = new FieldReaderDgitList<CompProperties>();
            WeaponAttachmentProperties? current = CurrentPartWeaponAttachmentPropertiesById(childNodeIdForCompProperties);
            ModularizationWeapon? currentWeapon = childNodeIdForCompProperties != null ? container[childNodeIdForCompProperties] as ModularizationWeapon : null;
            results.DefaultValue = 1;
            for (int i = 0; i < container.Count; i++)
            {
                string id = ((IList<string>)container)[i];
                ModularizationWeapon? weapon = container[i] as ModularizationWeapon;
                WeaponAttachmentProperties? properties = CurrentPartWeaponAttachmentPropertiesById(id);
                if (weapon != null && properties != null && id != childNodeIdForCompProperties)
                {
                    FieldReaderDgitList<CompProperties> cache = properties.compPropertiesMultiplierAffectHorizon;

                    if (current != null)
                    {
                        double defaultValue = cache.DefaultValue;
                        cache *=
                            current.compPropertiesOtherPartMultiplierAffectHorizon
                            .GetOrNewWhenNull(
                                id,
                                delegate ()
                                {
                                    FieldReaderDgitList<CompProperties> result = new FieldReaderDgitList<CompProperties>();
                                    result.DefaultValue = current.compPropertiesOtherPartMultiplierAffectHorizonDefaultValue;
                                    return result;
                                }
                            );
                        defaultValue *= current.compPropertiesOtherPartMultiplierAffectHorizonDefaultValue;
                        cache.DefaultValue = defaultValue;
                        if (currentWeapon != null && weapon != currentWeapon)
                        {
                            cache *= currentWeapon.Props.compPropertiesOtherPartMultiplierAffectHorizon;
                            cache.DefaultValue = defaultValue * currentWeapon.Props.compPropertiesOtherPartMultiplierAffectHorizon.DefaultValue;
                        }
                    }

                    FieldReaderDgitList<CompProperties> mul = weapon.Props.compPropertiesMultiplier - 1f;
                    if (mul.HasDefaultValue) mul.DefaultValue--;
                    mul = (mul * cache + 1) ?? mul;
                    mul.DefaultValue = 1;
                    results *= mul;
                    results.DefaultValue = 1;

                    mul = weapon.CompPropertiesMultiplier(null) - 1;
                    if (mul.HasDefaultValue) mul.DefaultValue--;
                    mul = (mul * cache + 1) ?? mul;
                    mul.DefaultValue = 1;
                    results *= mul;
                    results.DefaultValue = 1;
                }
            }
            //Log.Message($"{this}.ToolsMultiplier({childNodeIdForTool}) :\nresults : {results}");
            return results;
        }


        public float GetStatMultiplier(StatDef statDef, string? childNodeIdForState)
        {
            lock (this)
            {
                NodeContainer? container = ChildNodes;
                if (container == null) throw new NullReferenceException(nameof(ChildNodes));
                WeaponAttachmentProperties? current = CurrentPartWeaponAttachmentPropertiesById(childNodeIdForState);
                ModularizationWeapon? currentWeapon = childNodeIdForState != null ? container[childNodeIdForState] as ModularizationWeapon : null;
                statMultiplierCache ??= new Dictionary<(StatDef, string?), float>();
                if (!statMultiplierCache.TryGetValue((statDef, childNodeIdForState), out float result))
                {
                    result = 1;
                    for (int i = 0; i < container.Count; i++)
                    {
                        string id = ((IList<string>)container)[i];
                        ModularizationWeapon? weapon = container[i] as ModularizationWeapon;
                        WeaponAttachmentProperties? properties = CurrentPartWeaponAttachmentPropertiesById(id);
                        if (weapon != null && properties != null && id != childNodeIdForState)
                        {
                            float cache = properties.statMultiplierAffectHorizon.GetStatValueFromList(statDef, properties.statMultiplierAffectHorizonDefaultValue);

                            if (current != null)
                            {
                                cache *= current.statOtherPartMultiplierAffectHorizon
                                .GetOrNewWhenNull(
                                    id,
                                    () => new List<StatModifier>()
                                ).GetStatValueFromList(statDef, current.statOtherPartMultiplierAffectHorizonDefaultValue);
                                if (currentWeapon != null && weapon != currentWeapon)
                                {
                                    cache *= currentWeapon.Props.statOtherPartMultiplierAffectHorizon.GetStatValueFromList(statDef, currentWeapon.Props.statOtherPartMultiplierAffectHorizonDefaultValue);
                                }
                            }
                            result *= (weapon.Props.statMultiplier.GetStatValueFromList(statDef, weapon.Props.statMultiplierDefaultValue) - 1) * cache + 1;
                            result *= (weapon.GetStatMultiplier(statDef, null) - 1) * cache + 1;
                        }
                    }
                    statMultiplierCache.Add((statDef, childNodeIdForState), result);
                    //Log.Message($"{this}.GetStatMultiplier({statDef},{part})=>{result}\ncurrent.statOtherPartMultiplierAffectHorizonDefaultValue : {current?.statOtherPartMultiplierAffectHorizonDefaultValue}");
                }
                return result;
            }
        }
        #endregion
    }
}
