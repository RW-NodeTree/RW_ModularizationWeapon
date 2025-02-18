using RimWorld;
using RW_ModularizationWeapon.Tools;
using RW_NodeTree;
using System.Collections.Generic;
using Verse;

namespace RW_ModularizationWeapon
{
    public partial class CompModularizationWeapon
    {
        public FieldReaderDgitList<VerbProperties> VerbPropertiesOffseter(string childNodeIdForVerbProperties) //null -> self
        {
            FieldReaderDgitList<VerbProperties> results = new FieldReaderDgitList<VerbProperties>();
            WeaponAttachmentProperties current = CurrentPartWeaponAttachmentPropertiesById(childNodeIdForVerbProperties);
            CompModularizationWeapon currentComp = ChildNodes[childNodeIdForVerbProperties];
            NodeContainer container = ChildNodes;
            results.DefaultValue = 0;
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                CompModularizationWeapon comp = container[i];
                WeaponAttachmentProperties properties = CurrentPartWeaponAttachmentPropertiesById(id);
                if (comp != null && comp.Validity && id != childNodeIdForVerbProperties)
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


        public FieldReaderDgitList<Tool> ToolsOffseter(string childNodeIdForTool)
        {
            FieldReaderDgitList<Tool> results = new FieldReaderDgitList<Tool>();
            WeaponAttachmentProperties current = CurrentPartWeaponAttachmentPropertiesById(childNodeIdForTool);
            CompModularizationWeapon currentComp = ChildNodes[childNodeIdForTool];
            NodeContainer container = ChildNodes;
            results.DefaultValue = 0;
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                CompModularizationWeapon comp = container[i];
                WeaponAttachmentProperties properties = CurrentPartWeaponAttachmentPropertiesById(id);
                if (comp != null && comp.Validity && id != childNodeIdForTool)
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
            FieldReaderDgitList<CompProperties> results = new FieldReaderDgitList<CompProperties>();
            NodeContainer container = ChildNodes;
            results.DefaultValue = 0;
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                CompModularizationWeapon comp = container[i];
                WeaponAttachmentProperties properties = CurrentPartWeaponAttachmentPropertiesById(id);
                if (comp != null && comp.Validity)
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

        public float GetStatOffset(StatDef statDef, string childNodeIdForState)
        {
            lock (statOffsetCache)
            {
                WeaponAttachmentProperties current = CurrentPartWeaponAttachmentPropertiesById(childNodeIdForState);
                CompModularizationWeapon currentComp = ChildNodes[childNodeIdForState];
                NodeContainer container = ChildNodes;
                if (!statOffsetCache.TryGetValue((statDef, childNodeIdForState), out float result))
                {
                    result = 0;
                    for (int i = 0; i < container.Count; i++)
                    {
                        string id = container[(uint)i];
                        CompModularizationWeapon comp = container[i];
                        WeaponAttachmentProperties properties = CurrentPartWeaponAttachmentPropertiesById(id);
                        if (comp != null && comp.Validity && id != childNodeIdForState)
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
