using RW_NodeTree;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RW_ModularizationWeapon
{
    public partial class CompModularizationWeapon
    {
        public bool GetChildTreeViewOpend(string id)
        {
            bool result;
            if (!childTreeViewOpend.TryGetValue(id, out result))
            {
                result = false;
                childTreeViewOpend.Add(id, result);
            }
            return result;
        }


        public void SetChildTreeViewOpend(string id, bool value) => childTreeViewOpend.SetOrAdd(id, value);


        public Vector2 TreeViewDrawSize(Vector2 BlockSize)
        {
            Vector2 result = new Vector2(BlockSize.x, 0);
            foreach ((string id, Thing? thing, WeaponAttachmentProperties properties) in this)
            {
                result.y += BlockSize.y;
                if (id != null && GetChildTreeViewOpend(id))
                {
                    CompModularizationWeapon? comp = thing;
                    if (!(comp?.Props.attachmentProperties).NullOrEmpty())
                    {
                        Vector2 childSize = comp!.TreeViewDrawSize(BlockSize);
                        result.y += childSize.y;
                        result.x = Math.Max(childSize.x + BlockSize.y, result.x);
                    }
                }
            }
            return result;
        }


        public float DrawChildTreeView(
            Vector2 DrawPos,
            float ScrollPos,
            float BlockHeight,
            float ContainerWidth,
            float ContainerHeight,
            Action<string, Thing?, CompModularizationWeapon>? openEvent,
            Action<string, Thing?, CompModularizationWeapon>? closeEvent,
            Action<string, Thing?, CompModularizationWeapon>? iconEvent,
            HashSet<(string, CompModularizationWeapon)>? Selected
        )
        {
            Vector2 currentPos = DrawPos;
            bool cacheWordWrap = Text.WordWrap;
            GameFont cacheFont = Text.Font;
            TextAnchor cacheAnchor = Text.Anchor;
            Text.WordWrap = false;
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleLeft;
            bool isSub = DrawPos.x > 0;
            bool colorSelecter = false;
            float halfBlockHeight = BlockHeight / 2;
            float lastY = currentPos.y;
            foreach ((string id, Thing? thing, WeaponAttachmentProperties properties) in this)
            {
                if (id != null)
                {
                    bool opend = GetChildTreeViewOpend(id);
                    bool inRenderingRange = currentPos.y + BlockHeight > ScrollPos && currentPos.y < ScrollPos + ContainerHeight;
                    float next = currentPos.y;
                    CompModularizationWeapon? comp = thing;
                    CompChildNodeProccesser? proccesser = thing;
                    if (!(comp?.Props.attachmentProperties).NullOrEmpty())
                    {
                        if (opend)
                        {
                            next += comp!.DrawChildTreeView(
                                currentPos + Vector2.one * BlockHeight,
                                ScrollPos,
                                BlockHeight,
                                ContainerWidth - BlockHeight,
                                ContainerHeight,
                                openEvent,
                                closeEvent,
                                iconEvent,
                                Selected
                            );
                        }
                    }
                    else SetChildTreeViewOpend(id, false);
                    if (inRenderingRange)
                    {
                        Color color = GUI.color;
                        if (isSub)
                        {
                            GUI.color = Widgets.SeparatorLineColor;
                            Widgets.DrawLineHorizontal(currentPos.x - halfBlockHeight, currentPos.y + halfBlockHeight, halfBlockHeight);
                            if(currentPos == DrawPos)
                                Widgets.DrawLineVertical(currentPos.x - halfBlockHeight, lastY, halfBlockHeight);
                            else
                                Widgets.DrawLineVertical(currentPos.x - halfBlockHeight, lastY + halfBlockHeight, currentPos.y - lastY);
                        }
                        if (Selected?.Contains((id, this)) ?? false) Widgets.DrawBoxSolidWithOutline(new Rect(currentPos.x, currentPos.y, ContainerWidth, BlockHeight), new Color32(51, 153, 255, 64), new Color32(51, 153, 255, 96));
                        else if (opend) Widgets.DrawBoxSolidWithOutline(new Rect(currentPos.x, currentPos.y, ContainerWidth, BlockHeight), new Color32(255, 239, 127, 45), new Color32(255, 239, 127, 68));
                        else
                        {
                            if (colorSelecter) Widgets.DrawBoxSolid(new Rect(currentPos.x, currentPos.y, ContainerWidth, BlockHeight), Widgets.SeparatorLineColor * new Color(0.75f, 0.75f, 0.75f, 1f));
                            else Widgets.DrawBoxSolid(new Rect(currentPos.x, currentPos.y, ContainerWidth, BlockHeight), Widgets.SeparatorLineColor * new Color(0.5f, 0.5f, 0.5f, 1f));
                            if (isSub)
                            {
                                GUI.color = Widgets.SeparatorLineColor;
                                Widgets.DrawLineVertical(currentPos.x, currentPos.y, BlockHeight);
                            }
                        }
                        Widgets.DrawHighlightIfMouseover(new Rect(currentPos.x, currentPos.y, ContainerWidth, BlockHeight));//hover
                        GUI.color = color;

                        if (thing != null)
                        {
                            ThingStyleDef styleDef = thing.StyleDef;
                            CompChildNodeProccesser? comp_targetModeParent = (thing.def.graphicData != null && (styleDef == null || styleDef.UIIcon == null) && thing.def.uiIconPath.NullOrEmpty() && !(thing is Pawn || thing is Corpse)) ? comp?.ParentProccesser : null;
                            if (comp_targetModeParent != null)
                            {
                                thing.holdingOwner = null;
                                proccesser?.ResetRenderedTexture();
                            }
                            try
                            {
                                Widgets.ThingIcon(new Rect(currentPos.x + 1, currentPos.y + 1, BlockHeight - 1, BlockHeight - 2), thing);
                                Widgets.Label(new Rect(currentPos.x + BlockHeight, currentPos.y + 1, ContainerWidth - BlockHeight - 1, BlockHeight - 2), $"{properties.Name} : {thing.Label}");
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex.ToString());
                            }
                            if (comp_targetModeParent != null)
                            {
                                thing.holdingOwner = comp_targetModeParent.ChildNodes;
                                proccesser?.ResetRenderedTexture();
                            }
                        }
                        else
                        {
                            try
                            {
                                Widgets.DrawTextureFitted(new Rect(currentPos.x, currentPos.y, BlockHeight, BlockHeight), properties.UITexture, 1);
                                Widgets.Label(new Rect(currentPos.x + BlockHeight, currentPos.y, ContainerWidth - BlockHeight, BlockHeight), properties.Name);
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex.ToString());
                            }
                        }
                        try
                        {
                            if (Widgets.ButtonInvisible(new Rect(currentPos.x, currentPos.y, BlockHeight, BlockHeight))) iconEvent?.Invoke(id, thing, this);
                            if (Widgets.ButtonInvisible(new Rect(currentPos.x + BlockHeight, currentPos.y, ContainerWidth - BlockHeight, BlockHeight)))
                            {
                                opend = !opend;
                                if (opend) openEvent?.Invoke(id, thing, this);
                                else closeEvent?.Invoke(id, thing, this);
                                SetChildTreeViewOpend(id, opend);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex.ToString());
                        }
                    }
                    lastY = currentPos.y;
                    currentPos.y = next + BlockHeight;
                    colorSelecter = !colorSelecter;
                }
            }
            Text.WordWrap = cacheWordWrap;
            Text.Font = GameFont.Small;
            Text.Anchor = cacheAnchor;
            return currentPos.y - DrawPos.y;
        }
    }
}
