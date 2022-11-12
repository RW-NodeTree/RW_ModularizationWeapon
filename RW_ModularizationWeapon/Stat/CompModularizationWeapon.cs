using RimWorld;
using RW_NodeTree;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Verse;

namespace RW_ModularizationWeapon
{
    public partial class CompModularizationWeapon
    {
        private StatRequest RedirectoryReq(StatWorker statWorker, StatRequest req)
        {
            StatDef statDef = StatWorker_stat(statWorker);
            if ((statDef.category == StatCategoryDefOf.Weapon || statDef.category == StatCategoryDefOf.Weapon_Ranged))
            {
                CompEquippable eq = req.Thing.TryGetComp<CompEquippable>();
                CompChildNodeProccesser proccesser = req.Thing;
                if (eq != null && proccesser != null)
                {
                    StatRequest cache = req;
                    req = StatRequest.For(proccesser.GetBeforeConvertVerbCorrespondingThing(typeof(CompEquippable), eq.PrimaryVerb).Item1);
                    if(statWorker.IsDisabledFor(req.Thing))
                    {
                        return cache;
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
                if (cachedVerbs.Count > 0)
                {
                    ThingDef_verbs(parent.def) = ThingDef_verbs(parent.def) ?? new List<VerbProperties>();
                    forPostRead.Add("CompModularizationWeapon_verbs", ThingDef_verbs(parent.def));
                    ThingDef_verbs(parent.def) = cachedVerbs;
                }
                if (cachedTools.Count > 0)
                {
                    forPostRead.Add("CompModularizationWeapon_tools", parent.def.tools);
                    parent.def.tools = cachedTools;
                }
                //if (Prefs.DevMode) Log.Message(" prefix after change: parent.def.Verbs.Count=" + parent.def.Verbs.Count + "; parent.def.tools.Count=" + parent.def.tools.Count + ";\n");
            }
            forPostRead.Add("CompModularizationWeapon_comps", parent.def.comps);
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
                    ThingDef_verbs(parent.def) = (List<VerbProperties>)obj;
                }
                obj = forPostRead.TryGetValue("CompModularizationWeapon_tools");
                if (obj != null)
                {
                    parent.def.tools = (List<Tool>)obj;
                }
            }
            obj = forPostRead.TryGetValue("CompModularizationWeapon_comps");
            if (obj != null)
            {
                parent.def.comps = (List<CompProperties>)obj;
            }
        }


        protected override void PreStatWorker_GetValueUnfinalized(StatWorker statWorker, StatRequest req, bool applyPostProcess, Dictionary<string, object> forPostRead)
        {
            //Log.Message($"{statWorker} : PreStatWorker_GetValueUnfinalized");
            StatWorkerPerfix(forPostRead);
        }


        protected override float PostStatWorker_GetValueUnfinalized(StatWorker statWorker, StatRequest req, bool applyPostProcess, float result, Dictionary<string, object> forPostRead)
        {
            //Log.Message($"{statWorker} : PostStatWorker_GetValueUnfinalized");
            StatWorkerPostfix(forPostRead);
            StatRequest before = req;
            req = RedirectoryReq(statWorker, req);
            if (before.Thing != req.Thing && req.Thing != parent)
            {
                return statWorker.GetValueUnfinalized(req, applyPostProcess);
            }
            return result;
        }


        protected override float PreStatWorker_FinalizeValue(StatWorker statWorker, StatRequest req, bool applyPostProcess, float result, Dictionary<string, object> forPostRead)
        {
            //Log.Message($"{statWorker} : PreStatWorker_FinalizeValue");
            StatWorkerPerfix(forPostRead);
            StatRequest before = req;
            req = RedirectoryReq(statWorker, req);
            if (before.Thing != req.Thing && req.Thing != parent)
            {
                float cache = result;
                statWorker.FinalizeValue(req, ref cache, applyPostProcess);
                forPostRead.Add("afterRedirectoryReq", cache);
                //Log.Message($"{StatWorker_stat(statWorker)}.FinalizeValue({req})  afterRedirectoryReq : {result}");
            }
            return result;
        }


        protected override float PostStatWorker_FinalizeValue(StatWorker statWorker, StatRequest req, bool applyPostProcess, float result, Dictionary<string, object> forPostRead)
        {
            //Log.Message($"{statWorker} : PostStatWorker_FinalizeValue");
            StatWorkerPostfix(forPostRead);
            if (forPostRead.TryGetValue("afterRedirectoryReq", out object cache))
            {
                //Log.Message($"{StatWorker_stat(statWorker)}.FinalizeValue({req})  afterRedirectoryReq : {result}");
                return (float)cache;
            }
            if (statWorker is StatWorker_MarketValue || statWorker == StatDefOf.Mass.Worker)
            {
                foreach (Thing thing in ChildNodes.Values)
                {
                    result += statWorker.GetValue(thing);
                }
            }
            else if (!(statWorker is StatWorker_MeleeAverageArmorPenetration || statWorker is StatWorker_MeleeAverageDPS))
            {
                StatDef statDef = StatWorker_stat(statWorker);
                result *= GetStatMultiplier(statDef, req.Thing);
                result += GetStatOffset(statDef, req.Thing);
            }
            return result;
        }


        protected override void PreStatWorker_GetStatDrawEntryLabel(StatWorker statWorker, StatDef stat, float value, ToStringNumberSense numberSense, StatRequest optionalReq, bool finalized, Dictionary<string, object> forPostRead)
        {
            //Log.Message($"{statWorker} : PreStatWorker_GetStatDrawEntryLabel");
            StatWorkerPerfix(forPostRead);
        }


        protected override string PostStatWorker_GetStatDrawEntryLabel(StatWorker statWorker, StatDef stat, float value, ToStringNumberSense numberSense, StatRequest optionalReq, bool finalized, string result, Dictionary<string, object> forPostRead)
        {
            //Log.Message($"{statWorker} : PostStatWorker_GetStatDrawEntryLabel");
            StatWorkerPostfix(forPostRead);
            StatRequest before = optionalReq;
            optionalReq = RedirectoryReq(statWorker, optionalReq);
            if (before.Thing != optionalReq.Thing && optionalReq.Thing != parent)
            {
                return statWorker.GetStatDrawEntryLabel(stat, value, numberSense, optionalReq, finalized);
            }
            return result;
        }


        protected override void PreStatWorker_GetExplanationUnfinalized(StatWorker statWorker, StatRequest req, ToStringNumberSense numberSense, Dictionary<string, object> forPostRead)
        {
            //Log.Message($"{statWorker} : PreStatWorker_GetExplanationUnfinalized");
            if (!(statWorker is StatWorker_MeleeAverageDPS ||
                statWorker is StatWorker_MeleeAverageArmorPenetration ||
                statWorker is StatWorker_MarketValue ||
                statWorker == StatDefOf.Mass.Worker)
            )
                StatWorkerPerfix(forPostRead);
        }


        protected override string PostStatWorker_GetExplanationUnfinalized(StatWorker statWorker, StatRequest req, ToStringNumberSense numberSense, string result, Dictionary<string, object> forPostRead)
        {
            //Log.Message($"{statWorker} : PostStatWorker_GetExplanationUnfinalized");
            if (!(statWorker is StatWorker_MeleeAverageDPS ||
                statWorker is StatWorker_MeleeAverageArmorPenetration ||
                statWorker is StatWorker_MarketValue ||
                statWorker == StatDefOf.Mass.Worker)
            )
                StatWorkerPostfix(forPostRead);
            StatRequest before = req;
            req = RedirectoryReq(statWorker, req);
            if (before.Thing != req.Thing && req.Thing != parent)
            {
                return statWorker.GetExplanationUnfinalized(req, numberSense);
            }
            if (statWorker is StatWorker_MeleeAverageDPS ||
                statWorker is StatWorker_MeleeAverageArmorPenetration ||
                statWorker is StatWorker_MarketValue ||
                statWorker == StatDefOf.Mass.Worker
            )
            {
                StringBuilder stringBuilder = new StringBuilder();
                foreach ((string id, Thing thing) in ChildNodes)
                {
                    if (!NotUseTools(id))
                    {
                        stringBuilder.AppendLine("  " + thing.Label + ":");
                        string exp = "\n" + statWorker.GetExplanationUnfinalized(StatRequest.For(thing), numberSense);
                        exp = Regex.Replace(exp, "\n", "\n  ");
                        stringBuilder.AppendLine(exp);
                    }
                }
                result += "\n" + stringBuilder.ToString();
            }
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
                List<VerbProperties> verbProperties = null;
                List<Tool> tools = null;
                CompEquippable eq = parent.GetComp<CompEquippable>();
                if (eq != null)
                {
                    //if (Prefs.DevMode) Log.Message(" prefix before clear: parent.def.Verbs0=" + parent.def.Verbs.Count + "; parent.def.tools0=" + parent.def.tools.Count + ";\n");
                    List<Verb> verbs = eq.AllVerbs;
                    List<VerbProperties> cachedVerbs = new List<VerbProperties>();
                    List<Tool> cachedTools = new List<Tool>();
                    foreach (Verb verb in verbs)
                    {
                        if (verb.tool != null && !parent.def.tools.Contains(verb.tool)) cachedTools.Add(verb.tool);
                        else if (!parent.def.Verbs.Contains(verb.verbProps)) cachedVerbs.Add(verb.verbProps);
                    }
                    if (cachedVerbs.Count > 0)
                    {
                        ThingDef_verbs(parent.def) = ThingDef_verbs(parent.def) ?? new List<VerbProperties>();
                        verbProperties = ThingDef_verbs(parent.def);
                        ThingDef_verbs(parent.def) = cachedVerbs;
                    }
                    if (cachedTools.Count > 0)
                    {
                        tools = parent.def.tools;
                        parent.def.tools = cachedTools;
                    }
                }
                List<CompProperties> compProperties = def.comps;
                def.comps = (from x in parent.AllComps select x.props).ToList();

                foreach (StatDrawEntry entry in result)
                {
                    yield return entry;
                }
                if (verbProperties != null)
                {
                    ThingDef_verbs(parent.def) = verbProperties;
                }
                if (tools != null)
                {
                    parent.def.tools = tools;
                }

                def.comps = compProperties;

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
                List<VerbProperties> verbProperties = null;
                List<Tool> tools = null;
                CompEquippable eq = parent.GetComp<CompEquippable>();
                if (eq != null)
                {
                    //if (Prefs.DevMode) Log.Message(" prefix before clear: parent.def.Verbs0=" + parent.def.Verbs.Count + "; parent.def.tools0=" + parent.def.tools.Count + ";\n");
                    List<Verb> verbs = eq.AllVerbs;
                    List<VerbProperties> cachedVerbs = new List<VerbProperties>();
                    List<Tool> cachedTools = new List<Tool>();
                    foreach (Verb verb in verbs)
                    {
                        if (verb.tool != null && !parent.def.tools.Contains(verb.tool)) cachedTools.Add(verb.tool);
                        else if (!parent.def.Verbs.Contains(verb.verbProps)) cachedVerbs.Add(verb.verbProps);
                    }
                    if (cachedVerbs.Count > 0)
                    {
                        ThingDef_verbs(parent.def) = ThingDef_verbs(parent.def) ?? new List<VerbProperties>();
                        verbProperties = ThingDef_verbs(parent.def);
                        ThingDef_verbs(parent.def) = cachedVerbs;
                    }
                    if (cachedTools.Count > 0)
                    {
                        tools = parent.def.tools;
                        parent.def.tools = cachedTools;
                    }
                }

                List<CompProperties> compProperties = parent.def.comps;
                parent.def.comps = (from x in parent.AllComps select x.props).ToList();

                foreach (StatDrawEntry entry in result)
                {
                    yield return entry;
                }
                if (verbProperties != null)
                {
                    ThingDef_verbs(parent.def) = verbProperties;
                }
                if (tools != null)
                {
                    parent.def.tools = tools;
                }

                parent.def.comps = compProperties;
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
    }
}
