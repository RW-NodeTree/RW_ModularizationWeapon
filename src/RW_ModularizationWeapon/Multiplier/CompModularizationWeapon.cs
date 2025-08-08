using RimWorld;
using RW_ModularizationWeapon.Tools;
using RW_NodeTree;
using System;
using System.Collections.Generic;
using Verse;

namespace RW_ModularizationWeapon
{
    public partial class CompModularizationWeapon
    {


        #region Multiplier
        public FieldReaderDgitList<VerbProperties> VerbPropertiesMultiplier(string? childNodeIdForVerbProperties)
        {
            NodeContainer? container = ChildNodes;
            if (container == null) throw new NullReferenceException(nameof(ChildNodes));
            FieldReaderDgitList<VerbProperties> results = new FieldReaderDgitList<VerbProperties>();
            WeaponAttachmentProperties? current = CurrentPartWeaponAttachmentPropertiesById(childNodeIdForVerbProperties);
            CompModularizationWeapon? currentComp = childNodeIdForVerbProperties != null ? container[childNodeIdForVerbProperties] : null;
            results.DefaultValue = 1;
            for (int i = 0; i < container.Count; i++)
            {
                string id = ((IList<string?>)container)[i]!;
                CompModularizationWeapon? comp = container[i];
                WeaponAttachmentProperties? properties = CurrentPartWeaponAttachmentPropertiesById(id);
                if (comp != null && properties != null && comp.Validity && id != childNodeIdForVerbProperties)
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
                        if (currentComp != null && comp != currentComp)
                        {
                            cache *= currentComp.Props.verbPropertiesOtherPartMultiplierAffectHorizon;
                            cache.DefaultValue = defaultValue * currentComp.Props.verbPropertiesOtherPartMultiplierAffectHorizon.DefaultValue;
                        }
                    }

                    FieldReaderDgitList<VerbProperties> mul = comp.Props.verbPropertiesMultiplier - 1f;
                    if (mul.HasDefaultValue) mul.DefaultValue--;
                    mul = (mul * cache + 1) ?? mul;
                    mul.DefaultValue = 1;
                    results *= mul;
                    results.DefaultValue = 1;

                    mul = comp.VerbPropertiesMultiplier(null) - 1;
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
            CompModularizationWeapon? currentComp = childNodeIdForTool != null ? container[childNodeIdForTool] : null;
            results.DefaultValue = 1;
            for (int i = 0; i < container.Count; i++)
            {
                string id = ((IList<string?>)container)[i]!;
                CompModularizationWeapon? comp = container[i];
                WeaponAttachmentProperties? properties = CurrentPartWeaponAttachmentPropertiesById(id);
                if (comp != null && properties != null && comp.Validity && id != childNodeIdForTool)
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
                        if (currentComp != null && comp != currentComp)
                        {
                            cache *= currentComp.Props.toolsOtherPartMultiplierAffectHorizon;
                            cache.DefaultValue = defaultValue * currentComp.Props.toolsOtherPartMultiplierAffectHorizon.DefaultValue;
                        }
                    }

                    FieldReaderDgitList<Tool> mul = comp.Props.toolsMultiplier - 1f;
                    if (mul.HasDefaultValue) mul.DefaultValue--;
                    mul = (mul * cache + 1) ?? mul;
                    mul.DefaultValue = 1;
                    results *= mul;
                    results.DefaultValue = 1;

                    mul = comp.ToolsMultiplier(null) - 1;
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


        public FieldReaderDgitList<CompProperties> CompPropertiesMultiplier()
        {
            NodeContainer? container = ChildNodes;
            if (container == null) throw new NullReferenceException(nameof(ChildNodes));
            FieldReaderDgitList<CompProperties> results = new FieldReaderDgitList<CompProperties>();
            results.DefaultValue = 1;
            for (int i = 0; i < container.Count; i++)
            {
                string id = ((IList<string?>)container)[i]!;
                CompModularizationWeapon? comp = container[i];
                WeaponAttachmentProperties? properties = CurrentPartWeaponAttachmentPropertiesById(id);
                if (comp != null && properties != null && comp.Validity)
                {
                    FieldReaderDgitList<CompProperties> cache = comp.Props.compPropertiesMultiplier - 1;
                    if (cache.HasDefaultValue) cache.DefaultValue--;
                    cache = (cache * properties.compPropertiesMultiplierAffectHorizon + 1) ?? cache;
                    cache.DefaultValue = 1;
                    results *= cache;
                    results.DefaultValue = 1;

                    cache = comp.CompPropertiesMultiplier() - 1;
                    if (cache.HasDefaultValue) cache.DefaultValue--;
                    cache = (cache * properties.compPropertiesMultiplierAffectHorizon + 1) ?? cache;
                    cache.DefaultValue = 1;
                    results *= cache;
                    results.DefaultValue = 1;
                }
            }
            //Log.Message($"{this}.ToolsMultiplier({childNodeIdForTool}) :\nresults : {results}");
            return results;
        }

        public float GetStatMultiplier(StatDef statDef, string? childNodeIdForState)
        {
            lock (statMultiplierCache)
            {
                NodeContainer? container = ChildNodes;
                if (container == null) throw new NullReferenceException(nameof(ChildNodes));
                WeaponAttachmentProperties? current = CurrentPartWeaponAttachmentPropertiesById(childNodeIdForState);
                CompModularizationWeapon? currentComp = childNodeIdForState != null ? container[childNodeIdForState] : null;
                if (!statMultiplierCache.TryGetValue((statDef, childNodeIdForState), out float result))
                {
                    result = 1;
                    for (int i = 0; i < container.Count; i++)
                    {
                        string id = ((IList<string?>)container)[i]!;
                        CompModularizationWeapon? comp = container[i];
                        WeaponAttachmentProperties? properties = CurrentPartWeaponAttachmentPropertiesById(id);
                        if (comp != null && properties != null && comp.Validity && id != childNodeIdForState)
                        {
                            float cache = properties.statMultiplierAffectHorizon.GetStatValueFromList(statDef, properties.statMultiplierAffectHorizonDefaultValue);

                            if (current != null)
                            {
                                cache *= current.statOtherPartMultiplierAffectHorizon
                                .GetOrNewWhenNull(
                                    id,
                                    () => new List<StatModifier>()
                                ).GetStatValueFromList(statDef,current.statOtherPartMultiplierAffectHorizonDefaultValue);
                                if (currentComp != null && comp != currentComp)
                                {
                                    cache *= currentComp.Props.statOtherPartMultiplierAffectHorizon.GetStatValueFromList(statDef, currentComp.Props.statOtherPartMultiplierAffectHorizonDefaultValue);
                                }
                            }
                            result *= (comp.Props.statMultiplier.GetStatValueFromList(statDef, comp.Props.statMultiplierDefaultValue) - 1) * cache + 1;
                            result *= (comp.GetStatMultiplier(statDef, null) - 1) * cache + 1;
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
