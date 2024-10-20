using RimWorld;
using RW_ModularizationWeapon.Tools;
using RW_NodeTree;
using System.Collections.Generic;
using Verse;

namespace RW_ModularizationWeapon
{
    public partial class CompModularizationWeapon
    {
        public FieldReaderDgitList<VerbProperties> VerbPropertiesOffseter(string childNodeIdForVerbProperties)
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
                        cache.DefaultValue = defaultValue * current.verbPropertiesOtherPartOffseterAffectHorizonDefaultValue;
                        defaultValue = cache.DefaultValue;
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
                        cache.DefaultValue = defaultValue * current.toolsOtherPartOffseterAffectHorizonDefaultValue;
                        defaultValue = cache.DefaultValue;
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

        public float GetStatOffset(StatDef statDef, Thing part)
        {
            lock (statOffsetCache)
            {
                NodeContainer container = ChildNodes;
                float result = 0;
                if (!statOffsetCache.TryGetValue((statDef, part), out result))
                {
                    result = (container.IsChild(part) || part == parent) ? 0 : Props.statOffset.GetStatValueFromList(
                        statDef,
                        Props.statOffsetDefaultValue
                    );
                    WeaponAttachmentProperties current = null;
                    CompModularizationWeapon currentComp = null;
                    for (int i = 0; i < container.Count; i++)
                    {
                        string id = container[(uint)i];
                        CompModularizationWeapon comp = container[i];
                        if (comp != null && comp.Validity && (comp.ChildNodes.IsChild(part) || part == comp.parent))
                        {
                            current = CurrentPartWeaponAttachmentPropertiesById(id);
                            currentComp = comp;
                            break;
                        }
                    }

                    for (int i = 0; i < container.Count; i++)
                    {
                        string id = container[(uint)i];
                        CompModularizationWeapon comp = container[i];
                        WeaponAttachmentProperties properties = CurrentPartWeaponAttachmentPropertiesById(id);
                        if (comp != null && comp.Validity)
                        {
                            result += comp.GetStatOffset(statDef, part)
                                * (comp.ChildNodes.IsChild(part) ? 1f : (properties.statOffsetAffectHorizon.GetStatValueFromList(
                                        statDef,
                                        properties.statOffsetAffectHorizonDefaultValue
                                    )
                                    * (current?.statOtherPartOffseterAffectHorizon
                                    .GetOrNewWhenNull(id, () => new List<StatModifier>())
                                    .GetStatValueFromList(
                                        statDef,
                                        current.statOtherPartOffseterAffectHorizonDefaultValue
                                    ) ?? 1)
                                    * (currentComp != comp ? (currentComp?.Props.statOtherPartOffseterAffectHorizon.GetStatValueFromList(
                                            statDef,
                                            currentComp.Props.statOtherPartOffseterAffectHorizonDefaultValue
                                        ) ?? 1
                                    ) : 1))
                                );
                        }
                    }
                    statOffsetCache.Add((statDef, part), result);
                    //Log.Message($"{this}.GetStatOffset({statDef},{part})=>{result}\ncurrent.statOtherPartOffseterAffectHorizonDefaultValue : {current?.statOtherPartOffseterAffectHorizonDefaultValue}");
                }
                return result;
            }
        }
    }
}
