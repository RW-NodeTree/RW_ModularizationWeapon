using RimWorld;
using RW_NodeTree;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            foreach ((string id, Thing thing, WeaponAttachmentProperties properties) in this)
            {
                result.y += BlockSize.y;
                if (id != null && GetChildTreeViewOpend(id))
                {
                    CompModularizationWeapon comp = thing;
                    if (comp != null)
                    {
                        Vector2 childSize = comp.TreeViewDrawSize(BlockSize);
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
            Action<string, Thing, CompModularizationWeapon> openEvent,
            Action<string, Thing, CompModularizationWeapon> closeEvent,
            Action<string, Thing, CompModularizationWeapon> iconEvent,
            HashSet<(string, CompModularizationWeapon)> Selected
        )
        {
            Vector2 currentPos = DrawPos;
            bool cacheWordWrap = Text.WordWrap;
            GameFont cacheFont = Text.Font;
            TextAnchor cacheAnchor = Text.Anchor;
            Text.WordWrap = false;
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleLeft;
            foreach ((string id, Thing thing, WeaponAttachmentProperties properties) in this)
            {
                if (id != null)
                {
                    if (currentPos.y + BlockHeight > ScrollPos && currentPos.y < ScrollPos + ContainerHeight)
                    {
                        if (Selected?.Contains((id, this)) ?? false) Widgets.DrawBoxSolidWithOutline(new Rect(currentPos.x, currentPos.y, ContainerWidth, BlockHeight), new Color32(51, 153, 255, 64), new Color32(51, 153, 255, 96));
                        else if (GetChildTreeViewOpend(id)) Widgets.DrawHighlightSelected(new Rect(currentPos.x, currentPos.y, ContainerWidth, BlockHeight));
                        Widgets.DrawHighlightIfMouseover(new Rect(currentPos.x, currentPos.y, ContainerWidth, BlockHeight));//hover
                    }

                    bool opend = GetChildTreeViewOpend(id);

                    if (currentPos.y + BlockHeight > ScrollPos && currentPos.y < ScrollPos + ContainerHeight)
                    {
                        if (Widgets.ButtonInvisible(new Rect(currentPos.x + BlockHeight, currentPos.y, ContainerWidth - BlockHeight, BlockHeight)))
                        {
                            opend = !opend;
                            if (opend) openEvent?.Invoke(id, thing, this);
                            else closeEvent?.Invoke(id, thing, this);
                            SetChildTreeViewOpend(id, opend);
                        }
                    }
                    if (thing != null)
                    {
                        CompModularizationWeapon comp = thing;
                        if (currentPos.y + BlockHeight > ScrollPos && currentPos.y < ScrollPos + ContainerHeight)
                        {
                            ThingStyleDef styleDef = thing.StyleDef;
                            CompChildNodeProccesser comp_targetModeParent = (thing.def.graphicData != null && (styleDef == null || styleDef.UIIcon == null) && thing.def.uiIconPath.NullOrEmpty() && !(thing is Pawn || thing is Corpse)) ? comp?.ParentProccesser : null;
                            if (comp_targetModeParent != null)
                            {
                                thing.holdingOwner = null;
                                comp.NodeProccesser.ResetRenderedTexture();
                            }
                            Widgets.ThingIcon(new Rect(currentPos.x + 1, currentPos.y + 1, BlockHeight - 1, BlockHeight - 2), thing);
                            if (comp_targetModeParent != null)
                            {
                                thing.holdingOwner = comp_targetModeParent.ChildNodes;
                                comp.NodeProccesser.ResetRenderedTexture();
                            }
                            Widgets.Label(new Rect(currentPos.x + BlockHeight, currentPos.y + 1, ContainerWidth - BlockHeight - 1, BlockHeight - 2), $"{properties.Name} : {thing.Label}");

                            if (Widgets.ButtonInvisible(new Rect(currentPos.x, currentPos.y, BlockHeight, BlockHeight)))
                            {
                                iconEvent?.Invoke(id, thing, this);
                            }
                        }
                        if (comp != null)
                        {
                            if (opend)
                            {
                                currentPos.y += comp.DrawChildTreeView(
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
                        else if (currentPos.y + BlockHeight > ScrollPos && currentPos.y < ScrollPos + ContainerHeight && Widgets.ButtonInvisible(new Rect(currentPos.x + BlockHeight, currentPos.y, ContainerWidth - BlockHeight, BlockHeight)))
                        {
                            openEvent?.Invoke(id, thing, this);
                        }
                    }
                    else if (currentPos.y + BlockHeight > ScrollPos && currentPos.y < ScrollPos + ContainerHeight)
                    {
                        Widgets.DrawTextureFitted(new Rect(currentPos.x, currentPos.y, BlockHeight, BlockHeight), properties.UITexture, 1);
                        Widgets.Label(new Rect(currentPos.x + BlockHeight, currentPos.y, ContainerWidth - BlockHeight, BlockHeight), properties.Name);
                        if (Widgets.ButtonInvisible(new Rect(currentPos.x, currentPos.y, ContainerWidth, BlockHeight))) iconEvent?.Invoke(id, thing, this);
                    }
                    currentPos.y += BlockHeight;
                }
            }
            Text.WordWrap = cacheWordWrap;
            Text.Font = GameFont.Small;
            Text.Anchor = cacheAnchor;
            return currentPos.y - DrawPos.y;
        }
    }
}
