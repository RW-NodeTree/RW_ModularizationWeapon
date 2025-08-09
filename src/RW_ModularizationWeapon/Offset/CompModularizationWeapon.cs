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
        public FieldReaderDgitList<VerbProperties> VerbPropertiesOffseter(string? childNodeIdForVerbProperties) //null -> self
        {
            NodeContainer? container = ChildNodes;
            if (container == null) throw new NullReferenceException(nameof(ChildNodes));
            FieldReaderDgitList<VerbProperties> results = new FieldReaderDgitList<VerbProperties>();
            WeaponAttachmentProperties? current = CurrentPartWeaponAttachmentPropertiesById(childNodeIdForVerbProperties);
            CompModularizationWeapon? currentComp = childNodeIdForVerbProperties != null ? container[childNodeIdForVerbProperties] : null;
            results.DefaultValue = 0;
            for (int i = 0; i < container.Count; i++)
            {
                string id = ((IList<string>)container)[i];
                CompModularizationWeapon? comp = container[i];
                WeaponAttachmentProperties? properties = CurrentPartWeaponAttachmentPropertiesById(id);
                if (comp != null && properties != null && comp.Validity && id != childNodeIdForVerbProperties)
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
                        if (currentComp != null && comp != currentComp)
                        {
                            cache *= currentComp.Props.verbPropertiesOtherPartOffseterAffectHorizon;
                            cache.DefaultValue = defaultValue * currentComp.Props.verbPropertiesOtherPartOffseterAffectHorizon.DefaultValue;
                        }
                    }


                    FieldReaderDgitList<VerbProperties>
                    offset = comp.Props.verbPropertiesOffseter * cache;
                    offset.DefaultValue = 0;
                    results += offset;
                    results.DefaultValue = 0;

                    offset = comp.VerbPropertiesOffseter(null) * cache;
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
            CompModularizationWeapon? currentComp = childNodeIdForTool != null ? container[childNodeIdForTool] : null;
            results.DefaultValue = 0;
            for (int i = 0; i < container.Count; i++)
            {
                string id = ((IList<string>)container)[i];
                CompModularizationWeapon? comp = container[i];
                WeaponAttachmentProperties? properties = CurrentPartWeaponAttachmentPropertiesById(id);
                if (comp != null && properties != null && comp.Validity && id != childNodeIdForTool)
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
                        if (currentComp != null && comp != currentComp)
                        {
                            cache *= currentComp.Props.toolsOtherPartOffseterAffectHorizon;
                            cache.DefaultValue = defaultValue * currentComp.Props.toolsOtherPartOffseterAffectHorizon.DefaultValue;
                        }
                    }

                    FieldReaderDgitList<Tool>
                    offset = comp.Props.toolsOffseter * cache;
                    offset.DefaultValue = 0;
                    results += offset;
                    results.DefaultValue = 0;

                    offset = comp.ToolsOffseter(null) * cache;
                    offset.DefaultValue = 0;
                    results += offset;
                    results.DefaultValue = 0;
                }
            }
            //Log.Message($"{this}.ToolsOffseter({childNodeIdForTool}) :\nresults : {results}");
            return results;
        }


        public FieldReaderDgitList<CompProperties> CompPropertiesOffseter()
        {
            NodeContainer? container = ChildNodes;
            if (container == null) throw new NullReferenceException(nameof(ChildNodes));
            FieldReaderDgitList<CompProperties> results = new FieldReaderDgitList<CompProperties>();
            results.DefaultValue = 0;
            for (int i = 0; i < container.Count; i++)
            {
                string id = ((IList<string>)container)[i];
                CompModularizationWeapon? comp = container[i];
                WeaponAttachmentProperties? properties = CurrentPartWeaponAttachmentPropertiesById(id);
                if (comp != null && properties != null && comp.Validity)
                {
                    FieldReaderDgitList<CompProperties> cache = properties.compPropertiesOffseterAffectHorizon * comp.Props.compPropertiesOffseter;
                    cache.DefaultValue = 0;
                    results += cache;
                    results.DefaultValue = 0;

                    cache = properties.compPropertiesOffseterAffectHorizon * comp.CompPropertiesOffseter();
                    cache.DefaultValue = 0;
                    results += cache;
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
            lock (statOffsetCache)
            {
                WeaponAttachmentProperties? current = CurrentPartWeaponAttachmentPropertiesById(childNodeIdForState);
                CompModularizationWeapon? currentComp = childNodeIdForState != null ? container[childNodeIdForState] : null;
                if (!statOffsetCache.TryGetValue((statDef, childNodeIdForState), out float result))
                {
                    result = 0;
                    for (int i = 0; i < container.Count; i++)
                    {
                        string id = ((IList<string>)container)[i];
                        CompModularizationWeapon? comp = container[i];
                        WeaponAttachmentProperties? properties = CurrentPartWeaponAttachmentPropertiesById(id);
                        if (comp != null && properties != null && comp.Validity && id != childNodeIdForState)
                        {
                            float cache = properties.statOffsetAffectHorizon.GetStatValueFromList(statDef, properties.statOffsetAffectHorizonDefaultValue);

                            if (current != null)
                            {
                                cache *= current.statOtherPartOffseterAffectHorizon
                                .GetOrNewWhenNull(
                                    id,
                                    () => new List<StatModifier>()
                                ).GetStatValueFromList(statDef,current.statOtherPartOffseterAffectHorizonDefaultValue);
                                if (currentComp != null && comp != currentComp)
                                {
                                    cache *= currentComp.Props.statOtherPartOffseterAffectHorizon.GetStatValueFromList(statDef, currentComp.Props.statOtherPartOffseterAffectHorizonDefaultValue);
                                }
                            }
                            result += comp.Props.statOffset.GetStatValueFromList(statDef, comp.Props.statOffsetDefaultValue) * cache;
                            result += comp.GetStatOffset(statDef, null) * cache;
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
