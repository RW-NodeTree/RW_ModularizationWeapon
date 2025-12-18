using RimWorld;
using RW_ModularizationWeapon.Tools;
using RW_NodeTree;
using RW_NodeTree.Patch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Verse;

namespace RW_ModularizationWeapon
{
    public partial class ModularizationWeapon : IStatValuePatcher, IStatExplanationPatcher, IStatHyperlinksPatcher, IStatsToDrawPatcher, IStatGearAffectPatcher, IThingDefStatsPatcher
    {


        private void StatWorkerPerfix(Dictionary<string, object?> stats)
        {

            stats["verbs"] = ThingDef_verbs(def);
            ThingDef_verbs(def) = VerbPropertiesFromThing(this);
            stats["tools"] = def.tools;
            def.tools = ToolsFromThing(this);
            stats["comps"] = def.comps;
            def.comps = CompPropertiesFromThing(this);
        }


        private void StatWorkerFinalfix(Dictionary<string, object?> stats)
        {
            List<VerbProperties>? verbProperties = stats.TryGetValue("verbs") as List<VerbProperties>;
            if (verbProperties != null)
            {
                ThingDef_verbs(def) = verbProperties;
            }
            List<Tool>? tools = stats.TryGetValue("tools") as List<Tool>;
            if (tools != null)
            {
                def.tools = tools;
            }
            List<CompProperties>? compProperties = stats.TryGetValue("comps") as List<CompProperties>;
            if (compProperties != null)
            {
                def.comps = compProperties;
            }
        }



        public bool PreStatWorker_GetValueUnfinalized(StatWorker statWorker, StatDef statDef, bool applyFinalProcess, Dictionary<string, object?> stats)
        {
            //Log.Message($"{StatWorker_stat(statWorker)} : PreStatWorker_GetValueUnfinalized");
            StatWorkerPerfix(stats);
            return true;
        }

        public float PostStatWorker_GetValueUnfinalized(StatWorker statWorker, StatDef stateDef, bool applyPostProcess, float result, Dictionary<string, object?> stats) => result;

        public float FinalStatWorker_GetValueUnfinalized(StatWorker statWorker, StatDef statDef, bool applyFinalProcess, float result, Dictionary<string, object?> stats, Exception exception)
        {
            //Log.Message($"{StatWorker_stat(statWorker)} : FinalStatWorker_GetValueUnfinalized -> {result}");
            StatWorkerFinalfix(stats);
            return result;
        }



        public bool PreStatWorker_FinalizeValue(StatWorker statWorker, StatDef statDef, bool applyFinalProcess, ref float result, Dictionary<string, object?> stats)
        {
            //Log.Message($"{StatWorker_stat(statWorker)} : PreStatWorker_FinalizeValue -> {result}");
            StatWorkerPerfix(stats);
            return true;
        }

        public float PostStatWorker_FinalizeValue(StatWorker statWorker, StatDef stateDef, bool applyPostProcess, float result, Dictionary<string, object?> stats) => result;

        public float FinalStatWorker_FinalizeValue(StatWorker statWorker, StatDef statDef, bool applyFinalProcess, float result, Dictionary<string, object?> stats, Exception exception)
        {
            //Log.Message($"{StatWorker_stat(statWorker)} : FinalStatWorker_FinalizeValue -> {result}");
            NodeContainer container = ChildNodes;
            if (exception != null)
            {
                Log.Error(exception.ToString());
            }
            if (statWorker is StatWorker_MarketValue || statWorker == StatDefOf.Mass.Worker)
            {
                foreach (Thing? thing in container.Values)
                {
                    try
                    {
                        result += statWorker.GetValue(thing);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.ToString());
                    }
                }
            }
            else if (!(statWorker is StatWorker_MeleeAverageArmorPenetration || statWorker is StatWorker_MeleeAverageDPS))
            {
                if (statDef.defName == "BurstShotCount")
                {
                    CompEquippable? equippable = GetComp<CompEquippable>();
                    result = equippable?.PrimaryVerb?.verbProps.burstShotCount ?? result;
                }
                else
                {
                    result *= GetStatMultiplier(statDef, null);
                    result += GetStatOffset(statDef, null);
                }
            }
            StatWorkerFinalfix(stats);
            return result;
        }



        public bool PreStatWorker_GetExplanationUnfinalized(StatWorker statWorker, StatDef statDef, ToStringNumberSense numberSense, Dictionary<string, object?> stats) => true;

        public string PostStatWorker_GetExplanationUnfinalized(StatWorker statWorker, StatDef stateDef, ToStringNumberSense numberSense, string result, Dictionary<string, object?> stats) => result;

        public string FinalStatWorker_GetExplanationUnfinalized(StatWorker statWorker, StatDef statDef, ToStringNumberSense numberSense, string result, Dictionary<string, object?> stats, Exception exception)
        {
            NodeContainer container = ChildNodes;
            if (exception != null)
            {
                Log.Error(exception.ToString());
            }
            if (statWorker is StatWorker_MeleeAverageDPS ||
                statWorker is StatWorker_MeleeAverageArmorPenetration
            )
            {
                StringBuilder stringBuilder = new StringBuilder();
                foreach (var data in container)
                {
                    if (!NotUseTools(data.Item1))
                    {
                        stringBuilder.AppendLine("  " + data.Item2.Label + ":");
                        string exp = "\n" + statWorker.GetExplanationUnfinalized(StatRequest.For(data.Item2), numberSense);
                        exp = Regex.Replace(exp, "\n", "\n  ");
                        stringBuilder.AppendLine(exp);
                    }
                }
                result += "\n" + stringBuilder.ToString();
            }
            else if (
                statWorker is StatWorker_MarketValue ||
                statWorker == StatDefOf.Mass.Worker
            )
            {
                StringBuilder stringBuilder = new StringBuilder();
                foreach (var data in container)
                {
                    stringBuilder.AppendLine("  " + data.Item2.Label + ":");
                    string exp = "\n" + statWorker.GetExplanationUnfinalized(StatRequest.For(data.Item2), numberSense);
                    exp = Regex.Replace(exp, "\n", "\n  ");
                    stringBuilder.AppendLine(exp);
                }
                result += "\n" + stringBuilder.ToString();
            }
            //Log.Message($"{StatWorker_stat(statWorker)} : FinalStatWorker_GetExplanationUnfinalized; req : {req}; reqBefore : {before};parent : {parent}");
            return result;
        }



        public IEnumerable<Dialog_InfoCard.Hyperlink> PostStatWorker_GetInfoCardHyperlinks(StatWorker statWorker, StatDef statDef, IEnumerable<Dialog_InfoCard.Hyperlink> result)
        {
            NodeContainer? container = ChildNodes;
            if (container == null) throw new NullReferenceException(nameof(ChildNodes));
            foreach (Dialog_InfoCard.Hyperlink link in result)
            {
                yield return link;
            }
            if (statWorker is StatWorker_MeleeAverageDPS ||
                statWorker is StatWorker_MeleeAverageArmorPenetration ||
                statWorker is StatWorker_MarketValue ||
                statWorker == StatDefOf.Mass.Worker
            )
            {
                foreach (Thing? thing in container.Values)
                {
                    if (thing == null) continue;
                    yield return new Dialog_InfoCard.Hyperlink(thing);
                }
            }
        }


        public IEnumerable<StatDrawEntry> PostThingDef_SpecialDisplayStats(ThingDef def, IEnumerable<StatDrawEntry> result)
        {
            //Log.Message($"FinalThingDef_SpecialDisplayStats({def},{req},{result})");
            Dictionary<string, object?> stats = new Dictionary<string, object?>();
            StatWorkerPerfix(stats);

            List<StatDrawEntry> cache = new List<StatDrawEntry>();

            try
            {
                cache.AddRange(result);
            }
            catch(Exception ex)
            {
                Log.Error(ex.ToString());
            }

            StatWorkerFinalfix(stats);

            foreach (StatDrawEntry entry in cache) yield return entry;
        }


        public IEnumerable<StatDrawEntry> PostStatsReportUtility_StatsToDraw(IEnumerable<StatDrawEntry> result)
        {
            //Log.Message($"FinalStatsReportUtility_StatsToDraw({thing},{result})");
            Dictionary<string, object?> stats = new Dictionary<string, object?>();
            StatWorkerPerfix(stats);

            List<StatDrawEntry> cache = new List<StatDrawEntry>();

            try
            {
                cache.AddRange(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }

            StatWorkerFinalfix(stats);

            foreach (StatDrawEntry entry in cache) yield return entry;
        }

        public float FinalStatWorker_StatOffsetFromGear(StatDef stat, float result, Dictionary<string, object?> stats, Exception exception)
        {
            if (exception != null)
            {
                Log.Error(exception.ToString());
            }
            return result * GetStatMultiplier(stat, null) + GetStatOffset(stat, null);
        }

        public bool FinalStatWorker_GearHasCompsThatAffectStat(StatDef stat, bool result, Dictionary<string, object?> stats, Exception exception)
        {
            if (exception != null)
            {
                Log.Error(exception.ToString());
            }
            return result || StatWorker.StatOffsetFromGear(this, stat) != 0;
        }

        public bool PreStatWorker_GetExplanationFinalizePart(StatWorker statWorker, StatDef stateDef, ToStringNumberSense numberSense, float finalVal, Dictionary<string, object?> stats) => true;

        public string PostStatWorker_GetExplanationFinalizePart(StatWorker statWorker, StatDef stateDef, ToStringNumberSense numberSense, float finalVal, string result, Dictionary<string, object?> stats) => result;

        public string FinalStatWorker_GetExplanationFinalizePart(StatWorker statWorker, StatDef stateDef, ToStringNumberSense numberSense, float finalVal, string result, Dictionary<string, object?> stats, Exception exception) =>result;

        public bool PreStatWorker_GearHasCompsThatAffectStat(StatDef stat, Dictionary<string, object?> stats) => true;

        public bool PreStatWorker_StatOffsetFromGear(StatDef stat, Dictionary<string, object?> stats) => true;

        public bool PreStatWorker_InfoTextLineFromGear(StatDef stat, Dictionary<string, object?> stats) => true;

        public bool PostStatWorker_GearHasCompsThatAffectStat(StatDef stat, bool result, Dictionary<string, object?> stats) => result;

        public float PostStatWorker_StatOffsetFromGear(StatDef stat, float result, Dictionary<string, object?> stats) => result;

        public string PostStatWorker_InfoTextLineFromGear(StatDef stat, string result, Dictionary<string, object?> stats) => result;

        public IEnumerable<Thing> PostStatWorker_RelevantGear(Pawn pwan, StatDef stat, IEnumerable<Thing> result) => result;

        public string FinalStatWorker_InfoTextLineFromGear(StatDef stat, string result, Dictionary<string, object?> stats, Exception exception) => result;
    }
}
