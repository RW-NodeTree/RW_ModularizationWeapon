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


        #region Multiplier
        public FieldReaderDgitList<VerbProperties> VerbPropertiesMultiplier(string childNodeIdForVerbProperties)
        {
            FieldReaderDgitList<VerbProperties> results = new FieldReaderDgitList<VerbProperties>();
            WeaponAttachmentProperties current = Props.WeaponAttachmentPropertiesById(childNodeIdForVerbProperties);
            CompModularizationWeapon currentComp = ChildNodes[childNodeIdForVerbProperties];
            NodeContainer container = ChildNodes;
            results.DefaultValue = 1;
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                CompModularizationWeapon comp = container[i];
                WeaponAttachmentProperties properties = Props.WeaponAttachmentPropertiesById(id);
                if (comp != null && comp.Validity && id != childNodeIdForVerbProperties)
                {
                    FieldReaderDgitList<VerbProperties> cache = properties.verbPropertiesMultiplierAffectHorizon;

                    if (current != null)
                    {
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
                        cache.DefaultValue = properties.verbPropertiesMultiplierAffectHorizon.DefaultValue * current.verbPropertiesOtherPartMultiplierAffectHorizonDefaultValue;
                        if (currentComp != null && comp != currentComp)
                        {
                            cache *= currentComp.Props.verbPropertiesOtherPartMultiplierAffectHorizon;
                            cache.DefaultValue *= currentComp.Props.verbPropertiesOtherPartMultiplierAffectHorizon;
                        }
                    }

                    FieldReaderDgitList<VerbProperties> mul = comp.Props.verbPropertiesMultiplier - 1f;
                    if (mul.HasDefaultValue) mul.DefaultValue--;
                    cache = mul * cache + 1;
                    cache.DefaultValue = 1;
                    results *= cache;
                    results.DefaultValue = 1;
                    //result *= (comp.Props.verbPropertiesMultiplier - 1f) * properties.verbPropertiesMultiplierAffectHorizon + 1f;
                }
            }
            //Log.Message($" Final {this}.VerbPropertiesMultiplier({childNodeIdForVerbProperties}) :\nresults : {results}");
            return results;
        }


        public FieldReaderDgitList<Tool> ToolsMultiplier(string childNodeIdForTool)
        {
            FieldReaderDgitList<Tool> results = new FieldReaderDgitList<Tool>();
            WeaponAttachmentProperties current = Props.WeaponAttachmentPropertiesById(childNodeIdForTool);
            CompModularizationWeapon currentComp = ChildNodes[childNodeIdForTool];
            NodeContainer container = ChildNodes;
            results.DefaultValue = 1;
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                CompModularizationWeapon comp = container[i];
                WeaponAttachmentProperties properties = Props.WeaponAttachmentPropertiesById(id);
                if (comp != null && comp.Validity && id != childNodeIdForTool)
                {
                    FieldReaderDgitList<Tool> cache = properties.toolsMultiplierAffectHorizon;

                    if (current != null)
                    {
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
                        cache.DefaultValue = properties.toolsMultiplierAffectHorizon.DefaultValue * current.toolsOtherPartMultiplierAffectHorizonDefaultValue;
                        if (currentComp != null && comp != currentComp)
                        {
                            cache *= currentComp.Props.toolsOtherPartMultiplierAffectHorizon;
                            cache.DefaultValue *= currentComp.Props.toolsOtherPartMultiplierAffectHorizon;
                        }
                    }

                    FieldReaderDgitList<Tool> mul = comp.Props.toolsMultiplier - 1f;
                    if (mul.HasDefaultValue) mul.DefaultValue--;
                    cache = mul * cache + 1;
                    cache.DefaultValue = 1;
                    results *= cache;
                    results.DefaultValue = 1;
                }
            }
            //Log.Message($"{this}.ToolsMultiplier({childNodeIdForTool}) :\nresults : {results}");
            return results;
        }


        public FieldReaderDgitList<CompProperties> CompPropertiesMultiplier()
        {
            FieldReaderDgitList<CompProperties> results = new FieldReaderDgitList<CompProperties>();
            NodeContainer container = ChildNodes;
            results.DefaultValue = 1;
            for (int i = 0; i < container.Count; i++)
            {
                string id = container[(uint)i];
                CompModularizationWeapon comp = container[i];
                WeaponAttachmentProperties properties = Props.WeaponAttachmentPropertiesById(id);
                if (comp != null && comp.Validity)
                {
                    FieldReaderDgitList<CompProperties> cache = comp.Props.compPropertiesMultiplier - 1;
                    if (cache.HasDefaultValue) cache.DefaultValue--;
                    cache = cache * properties.compPropertiesMultiplierAffectHorizon + 1;
                    cache.DefaultValue = 1;
                    results *= cache;
                    results.DefaultValue = 1;
                }
            }
            //Log.Message($"{this}.ToolsOffseter({childNodeIdForTool}) :\nresults : {results}");
            return results;
        }

        public float GetStatMultiplier(StatDef statDef, Thing part)
        {
            NodeContainer container = ChildNodes;
            float result = 1;
            if (!statMultiplierCache.TryGetValue((statDef, part), out result))
            {
                result = (container.IsChild(part) || part == parent) ? 1 : Props.statMultiplier.GetStatFactorFromList(statDef);
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
                        result *= 1f + (comp.GetStatMultiplier(statDef, part) - 1f)
                            * (comp.ChildNodes.IsChild(part) ? 1f : (properties.statMultiplierAffectHorizon
                                .GetStatValueFromList(
                                    statDef,
                                    properties.statMultiplierAffectHorizonDefaultValue
                                )
                                * (current?.statOtherPartMultiplierAffectHorizon
                                .GetOrNewWhenNull(id, () => new List<StatModifier>())
                                .GetStatValueFromList(
                                    statDef,
                                    current.statOtherPartMultiplierAffectHorizonDefaultValue
                                ) ?? 1f)
                                * (currentComp != comp ? (currentComp?.Props.statOtherPartMultiplierAffectHorizon ?? 1f) : 1f))
                            );
                    }
                }
                statMultiplierCache.Add((statDef, part), result);
                //Log.Message($"{this}.GetStatMultiplier({statDef},{part})=>{result} \ncurrent.statOtherPartMultiplierAffectHorizonDefaultValue : {current?.statOtherPartMultiplierAffectHorizonDefaultValue}");
            }
            return result;
        }
        #endregion
    }
}
