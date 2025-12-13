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
        public FieldReaderDgitList<VerbProperties> VerbPropertiesOffseter(string? childNodeIdForVerbProperties) //null -> self
        {
            NodeContainer? container = ChildNodes;
            if (container == null) throw new NullReferenceException(nameof(ChildNodes));
            FieldReaderDgitList<VerbProperties> results = new FieldReaderDgitList<VerbProperties>();
            WeaponAttachmentProperties? current = CurrentPartWeaponAttachmentPropertiesById(childNodeIdForVerbProperties);
            ModularizationWeapon? currentWeapon = childNodeIdForVerbProperties != null ? container[childNodeIdForVerbProperties] as ModularizationWeapon : null;
            results.DefaultValue = 0;
            for (int i = 0; i < container.Count; i++)
            {
                string id = ((IList<string>)container)[i];
                ModularizationWeapon? weapon = container[i] as ModularizationWeapon;
                WeaponAttachmentProperties? properties = CurrentPartWeaponAttachmentPropertiesById(id);
                if (weapon != null && properties != null && id != childNodeIdForVerbProperties)
                {
                    FieldReaderDgitList<VerbProperties> cache = properties.verbPropertiesOffseterAffectHorizon;

                    if (current != null)
                    {
                        double defaultValue = cache.DefaultValue;
                        cache *=
                            current.verbPropertiesOtherPartOffseterAffectHorizon
                            .GetOrNewWhenNull(
                                id,
                                delegate ()
                                {
                                    FieldReaderDgitList<VerbProperties> result = new FieldReaderDgitList<VerbProperties>();
                                    result.DefaultValue = current.verbPropertiesOtherPartOffseterAffectHorizonDefaultValue;
                                    return result;
                                }
                            );
                        defaultValue *= current.verbPropertiesOtherPartOffseterAffectHorizonDefaultValue;
                        cache.DefaultValue = defaultValue;
                        if (currentWeapon != null && weapon != currentWeapon)
                        {
                            cache *= currentWeapon.Props.verbPropertiesOtherPartOffseterAffectHorizon;
                            cache.DefaultValue = defaultValue * currentWeapon.Props.verbPropertiesOtherPartOffseterAffectHorizon.DefaultValue;
                        }
                    }


                    FieldReaderDgitList<VerbProperties>
                    offset = weapon.Props.verbPropertiesOffseter * cache;
                    offset.DefaultValue = 0;
                    results += offset;
                    results.DefaultValue = 0;

                    offset = weapon.VerbPropertiesOffseter(null) * cache;
                    offset.DefaultValue = 0;
                    results += offset;
                    results.DefaultValue = 0;
                }
            }
            //Log.Message($"{this}.VerbPropertiesOffseter({childNodeIdForVerbProperties}) :\nresults : {results}");
            return results;
        }


        public FieldReaderDgitList<Tool> ToolsOffseter(string? childNodeIdForTool)
        {
            NodeContainer? container = ChildNodes;
            if (container == null) throw new NullReferenceException(nameof(ChildNodes));
            FieldReaderDgitList<Tool> results = new FieldReaderDgitList<Tool>();
            WeaponAttachmentProperties? current = CurrentPartWeaponAttachmentPropertiesById(childNodeIdForTool);
            ModularizationWeapon? currentWeapon = childNodeIdForTool != null ? container[childNodeIdForTool] as ModularizationWeapon : null;
            results.DefaultValue = 0;
            for (int i = 0; i < container.Count; i++)
            {
                string id = ((IList<string>)container)[i];
                ModularizationWeapon? weapon = container[i] as ModularizationWeapon;
                WeaponAttachmentProperties? properties = CurrentPartWeaponAttachmentPropertiesById(id);
                if (weapon != null && properties != null && id != childNodeIdForTool)
                {
                    FieldReaderDgitList<Tool> cache = properties.toolsOffseterAffectHorizon;

                    if (current != null)
                    {
                        double defaultValue = cache.DefaultValue;
                        cache *=
                            current.toolsOtherPartOffseterAffectHorizon
                            .GetOrNewWhenNull(
                                id,
                                delegate ()
                                {
                                    FieldReaderDgitList<Tool> result = new FieldReaderDgitList<Tool>();
                                    result.DefaultValue = current.toolsOtherPartOffseterAffectHorizonDefaultValue;
                                    return result;
                                }
                            );
                        defaultValue *= current.toolsOtherPartOffseterAffectHorizonDefaultValue;
                        cache.DefaultValue = defaultValue;
                        if (currentWeapon != null && weapon != currentWeapon)
                        {
                            cache *= currentWeapon.Props.toolsOtherPartOffseterAffectHorizon;
                            cache.DefaultValue = defaultValue * currentWeapon.Props.toolsOtherPartOffseterAffectHorizon.DefaultValue;
                        }
                    }

                    FieldReaderDgitList<Tool>
                    offset = weapon.Props.toolsOffseter * cache;
                    offset.DefaultValue = 0;
                    results += offset;
                    results.DefaultValue = 0;

                    offset = weapon.ToolsOffseter(null) * cache;
                    offset.DefaultValue = 0;
                    results += offset;
                    results.DefaultValue = 0;
                }
            }
            //Log.Message($"{this}.ToolsOffseter({childNodeIdForTool}) :\nresults : {results}");
            return results;
        }


        public FieldReaderDgitList<CompProperties> CompPropertiesOffseter(string? childNodeIdForCompProperties)
        {
            NodeContainer? container = ChildNodes;
            if (container == null) throw new NullReferenceException(nameof(ChildNodes));
            FieldReaderDgitList<CompProperties> results = new FieldReaderDgitList<CompProperties>();
            WeaponAttachmentProperties? current = CurrentPartWeaponAttachmentPropertiesById(childNodeIdForCompProperties);
            ModularizationWeapon? currentWeapon = childNodeIdForCompProperties != null ? container[childNodeIdForCompProperties] as ModularizationWeapon : null;
            results.DefaultValue = 0;
            for (int i = 0; i < container.Count; i++)
            {
                string id = ((IList<string>)container)[i];
                ModularizationWeapon? weapon = container[i] as ModularizationWeapon;
                WeaponAttachmentProperties? properties = CurrentPartWeaponAttachmentPropertiesById(id);
                if (weapon != null && properties != null && id != childNodeIdForCompProperties)
                {
                    FieldReaderDgitList<CompProperties> cache = properties.compPropertiesOffseterAffectHorizon;

                    if (current != null)
                    {
                        double defaultValue = cache.DefaultValue;
                        cache *=
                            current.compPropertiesOtherPartOffseterAffectHorizon
                            .GetOrNewWhenNull(
                                id,
                                delegate ()
                                {
                                    FieldReaderDgitList<CompProperties> result = new FieldReaderDgitList<CompProperties>();
                                    result.DefaultValue = current.compPropertiesOtherPartOffseterAffectHorizonDefaultValue;
                                    return result;
                                }
                            );
                        defaultValue *= current.compPropertiesOtherPartOffseterAffectHorizonDefaultValue;
                        cache.DefaultValue = defaultValue;
                        if (currentWeapon != null && weapon != currentWeapon)
                        {
                            cache *= currentWeapon.Props.compPropertiesOtherPartOffseterAffectHorizon;
                            cache.DefaultValue = defaultValue * currentWeapon.Props.compPropertiesOtherPartOffseterAffectHorizon.DefaultValue;
                        }
                    }

                    FieldReaderDgitList<CompProperties>
                    offset = weapon.Props.compPropertiesOffseter * cache;
                    offset.DefaultValue = 0;
                    results += offset;
                    results.DefaultValue = 0;

                    offset = weapon.CompPropertiesOffseter(null) * cache;
                    offset.DefaultValue = 0;
                    results += offset;
                    results.DefaultValue = 0;
                }
            }
            //Log.Message($"{this}.ToolsOffseter({childNodeIdForTool}) :\nresults : {results}");
            return results;
        }


        public float GetStatOffset(StatDef statDef, string? childNodeIdForState)
        {
            NodeContainer? container = ChildNodes;
            if (container == null) throw new NullReferenceException(nameof(ChildNodes));
            lock (this)
            {
                WeaponAttachmentProperties? current = CurrentPartWeaponAttachmentPropertiesById(childNodeIdForState);
                ModularizationWeapon? currentWeapon = childNodeIdForState != null ? container[childNodeIdForState] as ModularizationWeapon : null;
                statOffsetCache ??= new Dictionary<(StatDef, string?), float>();
                if (!statOffsetCache.TryGetValue((statDef, childNodeIdForState), out float result))
                {
                    result = 0;
                    for (int i = 0; i < container.Count; i++)
                    {
                        string id = ((IList<string>)container)[i];
                        ModularizationWeapon? weapon = container[i] as ModularizationWeapon;
                        WeaponAttachmentProperties? properties = CurrentPartWeaponAttachmentPropertiesById(id);
                        if (weapon != null && properties != null && id != childNodeIdForState)
                        {
                            float cache = properties.statOffsetAffectHorizon.GetStatValueFromList(statDef, properties.statOffsetAffectHorizonDefaultValue);

                            if (current != null)
                            {
                                cache *= current.statOtherPartOffseterAffectHorizon
                                .GetOrNewWhenNull(
                                    id,
                                    () => new List<StatModifier>()
                                ).GetStatValueFromList(statDef, current.statOtherPartOffseterAffectHorizonDefaultValue);
                                if (currentWeapon != null && weapon != currentWeapon)
                                {
                                    cache *= currentWeapon.Props.statOtherPartOffseterAffectHorizon.GetStatValueFromList(statDef, currentWeapon.Props.statOtherPartOffseterAffectHorizonDefaultValue);
                                }
                            }
                            result += weapon.Props.statOffset.GetStatValueFromList(statDef, weapon.Props.statOffsetDefaultValue) * cache;
                            result += weapon.GetStatOffset(statDef, null) * cache;
                        }
                    }
                    statOffsetCache.Add((statDef, childNodeIdForState), result);
                    //Log.Message($"{this}.GetStatOffset({statDef},{part})=>{result}\ncurrent.statOtherPartOffseterAffectHorizonDefaultValue : {current?.statOtherPartOffseterAffectHorizonDefaultValue}");
                }
                return result;
            }
        }
    }
}
