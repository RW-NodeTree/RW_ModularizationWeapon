using HarmonyLib;
using RimWorld;
using RW_ModularizationWeapon.Tools;
using RW_NodeTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Verse;
using static UnityEngine.Random;

namespace RW_ModularizationWeapon
{
    public partial class CompModularizationWeapon
    {
        private StatRequest RedirectoryReq(StatWorker statWorker, StatRequest req)
        {
            StatDef statDef = StatWorker_stat(statWorker);
            if ((statDef.category == StatCategoryDefOf.Weapon || statDef.category == StatCategoryDefOf.Weapon_Ranged) && statWorker.GetType() != CombatExtended_StatWorker_Magazine)
            {
                CompEquippable eq = req.Thing.TryGetComp<CompEquippable>();
                CompChildNodeProccesser proccesser = req.Thing;
                if (eq != null && proccesser != null)
                {
                    StatRequest cache = req;
                    Verb verb = CompChildNodeProccesser.GetOriginalPrimaryVerbs(eq.VerbTracker);
                    Thing thing = proccesser.GetBeforeConvertVerbCorrespondingThing(typeof(CompEquippable), verb).Item1;
                    //Log.Message($"{req.Thing} -> {verb} -> {thing}");
                    if (thing != null)
                    {
                        req = StatRequest.For(thing);
                        if (statWorker.IsDisabledFor(req.Thing) || !req.HasThing)
                        {
                            return cache;
                        }
                    }
                }
            }
            return req;
        }


        private void StatWorkerPerfix(Dictionary<string, object> forPostRead)
        {
            CompEquippable eq = parent.GetComp<CompEquippable>();
            if (eq != null)
            {
                //if (Prefs.DevMode) Log.Message(" prefix before clear: parent.def.Verbs0=" + parent.def.Verbs.Count + "; parent.def.tools0=" + parent.def.tools.Count + ";\n");
                List<Verb> verbs = eq.AllVerbs;
                List<VerbProperties> cachedVerbs = (from x in verbs where x.tool == null && !parent.def.Verbs.Contains(x.verbProps) select x.verbProps).ToList();
                List<Tool> cachedTools = (from x in verbs where x.tool != null && !parent.def.tools.Contains(x.tool) select x.tool).ToList();

                ThingDef_verbs(parent.def) = ThingDef_verbs(parent.def) ?? new List<VerbProperties>();
                Stack<List<VerbProperties>> stackVerb = (Stack<List<VerbProperties>>)forPostRead.GetOrNewWhenNull("CompModularizationWeapon_verbs", () => new Stack<List<VerbProperties>>());
                stackVerb.Push(ThingDef_verbs(parent.def));
                ThingDef_verbs(parent.def) = cachedVerbs;

                Stack<List<Tool>> stackTool = (Stack<List<Tool>>)forPostRead.GetOrNewWhenNull("CompModularizationWeapon_tools", () => new Stack<List<Tool>>());
                stackTool.Push(parent.def.tools);
                parent.def.tools = cachedTools;
                //if (Prefs.DevMode) Log.Message(" prefix after change: parent.def.Verbs.Count=" + parent.def.Verbs.Count + "; parent.def.tools.Count=" + parent.def.tools.Count + ";\n");
            }
            Stack<List<CompProperties>> stackComp = (Stack<List<CompProperties>>)forPostRead.GetOrNewWhenNull("CompModularizationWeapon_comps", () => new Stack<List<CompProperties>>());
            stackComp.Push(parent.def.comps);
            parent.def.comps = (from x in parent.AllComps select x.props).ToList();
        }


        private void StatWorkerPostfix(Dictionary<string, object> forPostRead)
        {
            object obj;
            CompEquippable eq = parent.GetComp<CompEquippable>();
            if (eq != null)
            {
                obj = forPostRead.TryGetValue("CompModularizationWeapon_verbs");
                if (obj != null)
                {
                    ThingDef_verbs(parent.def) = ((Stack<List<VerbProperties>>)obj).Pop();
                }
                obj = forPostRead.TryGetValue("CompModularizationWeapon_tools");
                if (obj != null)
                {
                    parent.def.tools = ((Stack<List<Tool>>)obj).Pop();
                }
            }
            obj = forPostRead.TryGetValue("CompModularizationWeapon_comps");
            if (obj != null)
            {
                parent.def.comps = ((Stack<List<CompProperties>>)obj).Pop();
            }
        }


        protected override void PreStatWorker_GetValueUnfinalized(StatWorker statWorker, StatRequest req, bool applyPostProcess, Dictionary<string, object> forPostRead)
        {
            //Log.Message($"{StatWorker_stat(statWorker)} : PreStatWorker_GetValueUnfinalized");
            if (!(statWorker is StatWorker_MeleeAverageDPS ||
                statWorker is StatWorker_MeleeAverageArmorPenetration ||
                statWorker is StatWorker_MarketValue ||
                statWorker == StatDefOf.Mass.Worker)
            )
                StatWorkerPerfix(forPostRead);
            StatRequest before = req;
            req = RedirectoryReq(statWorker, req);
            if (before.Thing == req.Thing && req.Thing != parent)
            {
                ((CompChildNodeProccesser)req.Thing)?.PreStatWorker_GetValueUnfinalized(statWorker,req,applyPostProcess,forPostRead);
            }
        }


        protected override float PostStatWorker_GetValueUnfinalized(StatWorker statWorker, StatRequest req, bool applyPostProcess, float result, Dictionary<string, object> forPostRead)
        {
            //Log.Message($"{StatWorker_stat(statWorker)} : PostStatWorker_GetValueUnfinalized -> {result}");
            StatRequest before = req;
            req = RedirectoryReq(statWorker, req);
            if (req.Thing != parent)
            {
                if (before.Thing == req.Thing)
                {
                    result = ((CompChildNodeProccesser)req.Thing)?.PostStatWorker_GetValueUnfinalized(statWorker, req, applyPostProcess, result, forPostRead) ?? result;
                }
                else
                {
                    try
                    {
                        result = statWorker.GetValueUnfinalized(req, applyPostProcess);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.ToString());
                    }
                }
            }
            if (!(statWorker is StatWorker_MeleeAverageDPS ||
                statWorker is StatWorker_MeleeAverageArmorPenetration ||
                statWorker is StatWorker_MarketValue ||
                statWorker == StatDefOf.Mass.Worker)
            )
                StatWorkerPostfix(forPostRead);
            return result;
        }


        protected override float PreStatWorker_FinalizeValue(StatWorker statWorker, StatRequest req, bool applyPostProcess, float result, Dictionary<string, object> forPostRead)
        {
            //Log.Message($"{StatWorker_stat(statWorker)} : PreStatWorker_FinalizeValue -> {result}");
            if (!(statWorker is StatWorker_MeleeAverageDPS ||
                statWorker is StatWorker_MeleeAverageArmorPenetration ||
                statWorker is StatWorker_MarketValue ||
                statWorker == StatDefOf.Mass.Worker)
            )
                StatWorkerPerfix(forPostRead);
            StatRequest before = req;
            req = RedirectoryReq(statWorker, req);
            if (req.Thing != parent)
            {
                if (before.Thing == req.Thing)
                {
                    //Log.Message($"{req.Thing} -> {req.Thing?.Spawned} -> {req.Thing?.Map} -> {req.Thing?.MapHeld} -> {req.Thing?.ParentHolder}");
                    return ((CompChildNodeProccesser)req.Thing)?.PreStatWorker_FinalizeValue(statWorker, req, applyPostProcess, result, forPostRead) ?? result;
                    //Log.Message($"{StatWorker_stat(statWorker)}.FinalizeValue({req})  afterRedirectoryReq : {result}");
                }
                else
                {
                    try
                    {
                        float cache = result;
                        statWorker.FinalizeValue(req, ref cache, applyPostProcess);
                        forPostRead.Add("afterRedirectoryReq", cache);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.ToString());
                    }
                }
            }
            return result;
        }


        protected override float PostStatWorker_FinalizeValue(StatWorker statWorker, StatRequest req, bool applyPostProcess, float result, Dictionary<string, object> forPostRead)
        {
            //Log.Message($"{StatWorker_stat(statWorker)} : PostStatWorker_FinalizeValue -> {result}");
            StatRequest before = req;
            req = RedirectoryReq(statWorker, req);
            if (req.Thing == parent)
            {
                StatDef statDef = StatWorker_stat(statWorker);
                if (statWorker is StatWorker_MarketValue || statWorker == StatDefOf.Mass.Worker)
                {
                    foreach (Thing thing in ChildNodes.Values)
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
                        VerbTracker verbTracker = parent.TryGetComp<CompEquippable>()?.VerbTracker;
                        if(verbTracker != null)
                        {
                            List<Verb> verbList = CompChildNodeProccesser.GetOriginalAllVerbs(verbTracker);
                            foreach(Verb verb in verbList)
                            {
                                if(verb.verbProps.isPrimary && NodeProccesser.GetBeforeConvertVerbCorrespondingThing(typeof(CompEquippable),verb).Item1 == parent)
                                {
                                    result = verb.verbProps.burstShotCount;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        result *= GetStatMultiplier(statDef, req.Thing);
                        result += GetStatOffset(statDef, req.Thing);
                    }
                }
            }
            else if (before.Thing == req.Thing)
            {
                result = ((CompChildNodeProccesser)req.Thing)?.PostStatWorker_FinalizeValue(statWorker, req, applyPostProcess, result, forPostRead) ?? result;
            }
            if (forPostRead.TryGetValue("afterRedirectoryReq", out object cache))
            {
                //Log.Message($"{StatWorker_stat(statWorker)}.FinalizeValue({req})  afterRedirectoryReq : {result}");
                result = (float)cache;
            }
            if (!(statWorker is StatWorker_MeleeAverageDPS ||
                statWorker is StatWorker_MeleeAverageArmorPenetration ||
                statWorker is StatWorker_MarketValue ||
                statWorker == StatDefOf.Mass.Worker)
            )
                StatWorkerPostfix(forPostRead);
            return result;
        }


        protected override void PreStatWorker_GetStatDrawEntryLabel(StatWorker statWorker, StatDef stat, float value, ToStringNumberSense numberSense, StatRequest optionalReq, bool finalized, Dictionary<string, object> forPostRead)
        {
            //Log.Message($"{StatWorker_stat(statWorker)} : PerStatWorker_GetStatDrawEntryLabel({optionalReq})");
            StatRequest before = optionalReq;
            optionalReq = RedirectoryReq(statWorker, optionalReq);
            if (!(statWorker is StatWorker_MeleeAverageDPS ||
                statWorker is StatWorker_MeleeAverageArmorPenetration ||
                statWorker is StatWorker_MarketValue ||
                statWorker == StatDefOf.Mass.Worker)
            )
                StatWorkerPerfix(forPostRead);
            if (before.Thing == optionalReq.Thing && optionalReq.Thing != parent)
            {
                ((CompChildNodeProccesser)optionalReq.Thing)?.PreStatWorker_GetStatDrawEntryLabel(statWorker, stat, value, numberSense, optionalReq, finalized, forPostRead);
            }
        }


        protected override string PostStatWorker_GetStatDrawEntryLabel(StatWorker statWorker, StatDef stat, float value, ToStringNumberSense numberSense, StatRequest optionalReq, bool finalized, string result, Dictionary<string, object> forPostRead)
        {
            //Log.Message($"{StatWorker_stat(statWorker)} : PostStatWorker_GetStatDrawEntryLabel({optionalReq})");
            StatRequest before = optionalReq;
            optionalReq = RedirectoryReq(statWorker, optionalReq);
            if (optionalReq.Thing != parent)
            {
                if (before.Thing == optionalReq.Thing)
                {
                    result = ((CompChildNodeProccesser)optionalReq.Thing)?.PostStatWorker_GetStatDrawEntryLabel(statWorker, stat, value, numberSense, optionalReq, finalized, result, forPostRead) ?? result;
                }
                else
                {
                    try
                    {
                        result = statWorker.GetStatDrawEntryLabel(stat, value, numberSense, optionalReq, finalized);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.ToString());
                    }
                }
            }
            if (!(statWorker is StatWorker_MeleeAverageDPS ||
                statWorker is StatWorker_MeleeAverageArmorPenetration ||
                statWorker is StatWorker_MarketValue ||
                statWorker == StatDefOf.Mass.Worker)
            )
                StatWorkerPostfix(forPostRead);
            return result;
        }


        protected override void PreStatWorker_GetExplanationUnfinalized(StatWorker statWorker, StatRequest req, ToStringNumberSense numberSense, Dictionary<string, object> forPostRead)
        {
            StatRequest before = req;
            req = RedirectoryReq(statWorker, req);
            //Log.Message($"{StatWorker_stat(statWorker)} : PreStatWorker_GetExplanationUnfinalized; req : {req}; reqBefore : {before};parent : {parent}");
            if (!(statWorker is StatWorker_MeleeAverageDPS ||
                statWorker is StatWorker_MeleeAverageArmorPenetration ||
                statWorker is StatWorker_MarketValue ||
                statWorker == StatDefOf.Mass.Worker)
            )
                StatWorkerPerfix(forPostRead);
            if (before.Thing == req.Thing && req.Thing != parent)
            {
                ((CompChildNodeProccesser)req.Thing)?.PreStatWorker_GetExplanationUnfinalized(statWorker, req, numberSense, forPostRead);
            }
        }


        protected override string PostStatWorker_GetExplanationUnfinalized(StatWorker statWorker, StatRequest req, ToStringNumberSense numberSense, string result, Dictionary<string, object> forPostRead)
        {
            StatRequest before = req;
            req = RedirectoryReq(statWorker, req);
            if (req.Thing == parent)
            {
                if (statWorker is StatWorker_MeleeAverageDPS ||
                    statWorker is StatWorker_MeleeAverageArmorPenetration ||
                    statWorker is StatWorker_MarketValue ||
                    statWorker == StatDefOf.Mass.Worker
                )
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    foreach (KeyValuePair<string,Thing> data in ChildNodes)
                    {
                        if (!NotUseTools(data.Key))
                        {
                            stringBuilder.AppendLine("  " + data.Value.Label + ":");
                            string exp = "\n" + statWorker.GetExplanationUnfinalized(StatRequest.For(data.Value), numberSense);
                            exp = Regex.Replace(exp, "\n", "\n  ");
                            stringBuilder.AppendLine(exp);
                        }
                    }
                    result += "\n" + stringBuilder.ToString();
                }
            }
            else if (before.Thing == req.Thing)
            {
                result = ((CompChildNodeProccesser)req.Thing)?.PostStatWorker_GetExplanationUnfinalized(statWorker, req, numberSense, result, forPostRead) ?? result;
            }
            else
            {
                try
                {
                    result = statWorker.GetExplanationUnfinalized(req, numberSense);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
            if (!(statWorker is StatWorker_MeleeAverageDPS ||
                statWorker is StatWorker_MeleeAverageArmorPenetration ||
                statWorker is StatWorker_MarketValue ||
                statWorker == StatDefOf.Mass.Worker)
            )
                StatWorkerPostfix(forPostRead);
            //Log.Message($"{StatWorker_stat(statWorker)} : PostStatWorker_GetExplanationUnfinalized; req : {req}; reqBefore : {before};parent : {parent}");
            return result;
        }


        protected override IEnumerable<Dialog_InfoCard.Hyperlink> PostStatWorker_GetInfoCardHyperlinks(StatWorker statWorker, StatRequest statRequest, IEnumerable<Dialog_InfoCard.Hyperlink> result)
        {
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
                foreach (Thing thing in ChildNodes.Values)
                {
                    yield return new Dialog_InfoCard.Hyperlink(thing);
                }
            }
        }


        protected override IEnumerable<StatDrawEntry> PostThingDef_SpecialDisplayStats(ThingDef def, StatRequest req, IEnumerable<StatDrawEntry> result)
        {
            //Log.Message($"PostThingDef_SpecialDisplayStats({def},{req},{result})");
            if (req.Thing == parent)
            {
                Dictionary<string, object> forPostRead = new Dictionary<string, object>();
                StatWorkerPerfix(forPostRead);

                List<StatDrawEntry> cache = new List<StatDrawEntry>();

                try
                {
                    cache.AddRange(result);
                }
                catch(Exception ex)
                {
                    Log.Error(ex.ToString());
                }

                foreach (StatDrawEntry entry in cache) yield return entry;

                StatWorkerPostfix(forPostRead);
            }
            else
            {
                result = ((CompChildNodeProccesser)req.Thing)?.PostThingDef_SpecialDisplayStats(def, req, result) ?? result;
                foreach (StatDrawEntry entry in result)
                {
                    yield return entry;
                }
            }
        }


        protected override IEnumerable<StatDrawEntry> PostStatsReportUtility_StatsToDraw(Thing thing, IEnumerable<StatDrawEntry> result)
        {
            //Log.Message($"PostStatsReportUtility_StatsToDraw({thing},{result})");
            if (thing == parent)
            {
                Dictionary<string, object> forPostRead = new Dictionary<string, object>();
                StatWorkerPerfix(forPostRead);

                List<StatDrawEntry> cache = new List<StatDrawEntry>();

                try
                {
                    cache.AddRange(result);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }

                foreach (StatDrawEntry entry in cache) yield return entry;

                StatWorkerPostfix(forPostRead);
            }
            else
            {
                result = ((CompChildNodeProccesser)thing)?.PostStatsReportUtility_StatsToDraw(thing, result) ?? result;
                foreach (StatDrawEntry entry in result)
                {
                    yield return entry;
                }
            }
        }

        protected override float PostStatWorker_StatOffsetFromGear(Thing gear, StatDef stat, float result, Dictionary<string, object> forPostRead)
        {
            return result * GetStatMultiplier(stat, gear) + GetStatOffset(stat, gear);
        }

        protected override bool PostStatWorker_GearHasCompsThatAffectStat(Thing gear, StatDef stat, bool result, Dictionary<string, object> forPostRead)
        {
            return result || StatWorker.StatOffsetFromGear(gear, stat) != 0;
        }
    }
}
