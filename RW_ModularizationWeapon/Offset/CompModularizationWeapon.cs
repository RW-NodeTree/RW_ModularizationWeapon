using RimWorld;
using RW_ModularizationWeapon.Tools;
using RW_NodeTree;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RW_ModularizationWeapon
{
    public partial class CompModularizationWeapon
    {
        public FieldReaderDgitList<VerbProperties> VerbPropertiesOffseter(string childNodeIdForVerbProperties)
        {
            FieldReaderDgitList<VerbProperties> results = new FieldReaderDgitList<VerbProperties>();
            WeaponAttachmentProperties current = Props.WeaponAttachmentPropertiesById(childNodeIdForVerbProperties);
            CompModularizationWeapon currentComp = ChildNodes[childNodeIdForVerbProperties];
            NodeContainer container = ChildNodes;
            results.DefaultValue = 0;
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                CompModularizationWeapon comp = container[i];
                WeaponAttachmentProperties properties = Props.WeaponAttachmentPropertiesById(id);
                if (comp != null && comp.Validity && id != childNodeIdForVerbProperties)
                {
                    FieldReaderDgitList<VerbProperties> cache = properties.verbPropertiesOffseterAffectHorizon;

                    if (current != null)
                    {
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
                        cache.DefaultValue = properties.verbPropertiesOffseterAffectHorizon.DefaultValue * current.verbPropertiesOtherPartOffseterAffectHorizonDefaultValue;
                        if (currentComp != null && comp != currentComp)
                        {
                            cache *= currentComp.Props.verbPropertiesOtherPartOffseterAffectHorizon;
                            cache.DefaultValue *= currentComp.Props.verbPropertiesOtherPartOffseterAffectHorizon;
                        }
                    }

                    cache = comp.Props.verbPropertiesOffseter * cache;
                    cache.DefaultValue = 0;
                    results += cache;
                    results.DefaultValue = 0;
                }
            }
            //Log.Message($"{this}.VerbPropertiesOffseter({childNodeIdForVerbProperties}) :\nresults : {results}");
            return results;
        }


        public FieldReaderDgitList<Tool> ToolsOffseter(string childNodeIdForTool)
        {
            FieldReaderDgitList<Tool> results = new FieldReaderDgitList<Tool>();
            WeaponAttachmentProperties current = Props.WeaponAttachmentPropertiesById(childNodeIdForTool);
            CompModularizationWeapon currentComp = ChildNodes[childNodeIdForTool];
            NodeContainer container = ChildNodes;
            results.DefaultValue = 0;
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                CompModularizationWeapon comp = container[i];
                WeaponAttachmentProperties properties = Props.WeaponAttachmentPropertiesById(id);
                if (comp != null && comp.Validity && id != childNodeIdForTool)
                {
                    FieldReaderDgitList<Tool> cache = properties.toolsOffseterAffectHorizon;

                    if (current != null)
                    {
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
                        cache.DefaultValue = properties.toolsOffseterAffectHorizon.DefaultValue * current.toolsOtherPartOffseterAffectHorizonDefaultValue;
                        if (currentComp != null && comp != currentComp)
                        {
                            cache *= currentComp.Props.toolsOtherPartOffseterAffectHorizon;
                            cache.DefaultValue *= currentComp.Props.toolsOtherPartOffseterAffectHorizon;
                        }
                    }

                    cache = comp.Props.toolsOffseter * cache;
                    cache.DefaultValue = 0;
                    results += cache;
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
                WeaponAttachmentProperties properties = Props.WeaponAttachmentPropertiesById(id);
                if (comp != null && comp.Validity)
                {
                    FieldReaderDgitList<CompProperties> cache = properties.compPropertiesOffseterAffectHorizon * comp.Props.compPropertiesOffseter;
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
            NodeContainer container = ChildNodes;
            float result = 0;
            if (!statOffsetCache.TryGetValue((statDef, part), out result))
            {
                result = (container.IsChild(part) || part == parent) ? 0 : Props.statOffset.GetStatOffsetFromList(statDef);
                WeaponAttachmentProperties current = null;
                CompModularizationWeapon currentComp = null;
                for (int i = 0; i < container.Count; i++)
                {
                    string id = container[(uint)i];
                    CompModularizationWeapon comp = container[i];
                    if (comp != null && comp.Validity && (comp.ChildNodes.IsChild(part) || part == comp.parent))
                    {
                        current = Props.WeaponAttachmentPropertiesById(id);
                        currentComp = comp;
                        break;
                    }
                }

                for (int i = 0; i < container.Count; i++)
                {
                    string id = container[(uint)i];
                    CompModularizationWeapon comp = container[i];
                    WeaponAttachmentProperties properties = Props.WeaponAttachmentPropertiesById(id);
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
                                * (currentComp != comp ? (currentComp?.Props.statOtherPartOffseterAffectHorizon ?? 1) : 1))
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
