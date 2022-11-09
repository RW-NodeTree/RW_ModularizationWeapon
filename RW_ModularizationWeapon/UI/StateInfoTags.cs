using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse.Noise;
using Verse;
using RimWorld.Planet;
using System.Reflection;
using static HarmonyLib.Code;
using static Verse.Dialog_InfoCard;

namespace RW_ModularizationWeapon.UI
{
    public class StateInfoTags
    {
        public StateInfoTags(float InitDrawSize, Thing thing, Precept_ThingStyle precept = null)
        {
            this.thing = thing;
            Dialog_InfoCard infoCard = new Dialog_InfoCard(thing, precept);
            Dialog_InfoCard_FillCard.Invoke(infoCard, new object[] { new Rect(0, 0, (InitDrawSize + 24) * 2, 0) });
            this.UpdateStatInfos();
        }


        public StateInfoTags(float InitDrawSize, Def onlyDef, Precept_ThingStyle precept = null)
        {
            Dialog_InfoCard infoCard = new Dialog_InfoCard(onlyDef, precept);
            Dialog_InfoCard_FillCard.Invoke(infoCard, new object[] { new Rect(0, 0, (InitDrawSize + 24) * 2, 0) });
            this.UpdateStatInfos();
        }


        public StateInfoTags(float InitDrawSize, ThingDef thingDef, ThingDef stuff, Precept_ThingStyle precept = null)
        {
            Dialog_InfoCard infoCard = new Dialog_InfoCard(thingDef, stuff, precept);
            Dialog_InfoCard_FillCard.Invoke(infoCard, new object[] { new Rect(0, 0, (InitDrawSize + 24) * 2, 0) });
            this.UpdateStatInfos();
        }


        public StateInfoTags(float InitDrawSize, RoyalTitleDef titleDef, Faction faction, Pawn pawn = null)
        {
            Dialog_InfoCard infoCard = new Dialog_InfoCard(titleDef, faction, pawn);
            Dialog_InfoCard_FillCard.Invoke(infoCard, new object[] { new Rect(0, 0, (InitDrawSize + 24) * 2, 0) });
            this.UpdateStatInfos();
        }


        public StateInfoTags(float InitDrawSize, Faction faction)
        {
            Dialog_InfoCard infoCard = new Dialog_InfoCard(faction);
            Dialog_InfoCard_FillCard.Invoke(infoCard, new object[] { new Rect(0, 0, (InitDrawSize + 24) * 2, 0) });
            this.UpdateStatInfos();
        }


        public StateInfoTags(float InitDrawSize, WorldObject worldObject)
        {
            Dialog_InfoCard infoCard = new Dialog_InfoCard(worldObject);
            Dialog_InfoCard_FillCard.Invoke(infoCard, new object[] { new Rect(0, 0, (InitDrawSize + 24) * 2, 0) });
            this.UpdateStatInfos();
        }

        public void UpdateStatInfos()
        {
            statInfos.Clear();
            statInfos.AddRange(from x in (List<StatDrawEntry>)StatsReportUtility_cachedDrawEntries.GetValue(null) select (false, x));
            infoCardMaxHeight = (float)StatsReportUtility_listHeight.GetValue(null);
            StatsReportUtility.Reset();
            if(statInfos.Count > 0) statInfos[0] = (true, statInfos[0].Item2);
        }

        public void Draw(Rect rect)
        {
            if(statInfos != null)
            {

                float infoCardWidth = rect.height > infoCardMaxHeight ? rect.width : rect.width - GUI.skin.verticalScrollbar.fixedWidth;
                Widgets.BeginScrollView(
                    rect,
                    ref scrollView,
                    new Rect(0, 0, infoCardWidth, infoCardMaxHeight)
                );
                Widgets.BeginGroup(new Rect(-8, 0, infoCardWidth + 8, infoCardMaxHeight));
                Text.Font = GameFont.Small;
                infoCardMaxHeight = 0;
                for (int i = 0; i < statInfos.Count; i++)
                {
                    (bool open, StatDrawEntry stat) = statInfos[i];
                    StatRequest statRequest = StatRequest.ForEmpty();
                    if (stat.hasOptionalReq) statRequest = stat.optionalReq;
                    else if (thing != null) statRequest = StatRequest.For(thing);
                    infoCardMaxHeight += stat.Draw(
                        8,
                        infoCardMaxHeight,
                        infoCardWidth,
                        open,
                        false,
                        false,
                        delegate ()
                        {
                            open = !open;
                        },
                        delegate () { },
                        scrollView,
                        new Rect(0, 0, 0, rect.height)
                    );
                    if (open)
                    {
                        string explanationText = stat.GetExplanationText(statRequest);
                        float num = Text.CalcHeight(explanationText, infoCardWidth) + 10f;
                        if (infoCardMaxHeight + 2 >= scrollView.y && infoCardMaxHeight <= scrollView.y + rect.height)
                            Widgets.DrawBoxSolid(new Rect(8, infoCardMaxHeight, infoCardWidth, 2), new Color32(51, 153, 255, 96));

                        infoCardMaxHeight += 2;

                        if (infoCardMaxHeight + num >= scrollView.y && infoCardMaxHeight <= scrollView.y + rect.height)
                        {
                            GUI.color = new Color32(51, 153, 255, 255);
                            Widgets.DrawHighlightSelected(new Rect(8, infoCardMaxHeight, infoCardWidth, num));
                            GUI.color = Color.white;
                            Widgets.Label(new Rect(8, infoCardMaxHeight, infoCardWidth, num), explanationText);
                        }
                        infoCardMaxHeight += num;

                        IEnumerable<Dialog_InfoCard.Hyperlink> hyperlinks = stat.GetHyperlinks(statRequest);
                        if(hyperlinks != null)
                        {
                            foreach (Dialog_InfoCard.Hyperlink hyperlink in stat.GetHyperlinks(statRequest))
                            {
                                num = Text.CalcHeight(hyperlink.Label, infoCardWidth);
                                if (infoCardMaxHeight + num >= scrollView.y && infoCardMaxHeight <= scrollView.y + rect.height)
                                {
                                    GUI.color = new Color32(51, 153, 255, 255);
                                    Widgets.DrawHighlightSelected(new Rect(8, infoCardMaxHeight, infoCardWidth, num));
                                    GUI.color = Color.white;
                                    Widgets.HyperlinkWithIcon(
                                        new Rect(8, infoCardMaxHeight, infoCardWidth, num),
                                        hyperlink, "ViewHyperlink".Translate(hyperlink.Label),
                                        2f,
                                        6f,
                                        null,
                                        false,
                                        null
                                    );
                                }
                                infoCardMaxHeight += num;
                            }
                        }
                    }
                    statInfos[i] = (open, stat);
                }
                Widgets.EndGroup();
                Widgets.EndScrollView();
            }
        }


        private static readonly FieldInfo StatsReportUtility_listHeight = typeof(StatsReportUtility).GetField("listHeight", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly FieldInfo StatsReportUtility_cachedDrawEntries = typeof(StatsReportUtility).GetField("cachedDrawEntries", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo Dialog_InfoCard_FillCard = typeof(Dialog_InfoCard).GetMethod("FillCard", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(Rect) }, null);
        private readonly List<(bool, StatDrawEntry)> statInfos = new List<(bool, StatDrawEntry)>();
        private Thing thing;
        private Vector2 scrollView;
        private float infoCardMaxHeight = 0;
    }
}
