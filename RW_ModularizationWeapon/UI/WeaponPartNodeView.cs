using RW_NodeTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RW_ModularizationWeapon.UI
{
    internal class WeaponPartNodeView
    {
        private WeaponPartNodeView(string id, CompModularizationWeapon parent)
        {
            parentWeapon = parent;
            parent = parent.GetPart(id);
            if (parentWeapon == null || !parentWeapon.Validity) throw new Exception("weapon not validity");
            else if(parent != null && parent.Validity)
            {
                childs = new List<WeaponPartNodeView>(parent.NodeProccesser.RegiestedNodeId.Count);
                foreach(string child in parent.NodeProccesser.RegiestedNodeId)
                {
                    childs.Add(new WeaponPartNodeView(child,parent));
                }
            }
        }

        public static WeaponPartNodeView TryGetWeaponPartNodeView(string id, CompModularizationWeapon parent)
        {
            try { return new WeaponPartNodeView(id, parent); }
            catch { return null; }
        }

        public Vector2 DrawSize(Vector2 BlockSize)
        {
            Vector2 result = BlockSize;
            if(open)
            {
                foreach(WeaponPartNodeView view in childs)
                {
                    if(view != null)
                    {
                        Vector2 childSize = view.DrawSize(BlockSize);
                        result.y += childSize.y;
                        result.x = Math.Max(childSize.x + BlockSize.y, result.x);
                    }
                }
            }
            return result;
        }

        public float DrawSelection(Vector2 DrawPos, Vector2 ParentSize, Vector2 BlockSize)
        {
            Vector2 drawSize = DrawSize(BlockSize);
            drawSize.x = Math.Max(drawSize.x, ParentSize.x - BlockSize.y);
            if(open)
            {
                Vector2 currentPos = DrawPos + Vector2.one * BlockSize.y;
                foreach(WeaponPartNodeView view in childs)
                {
                    if(view != null) currentPos.y += view.DrawSelection(currentPos,drawSize,BlockSize);
                }
                Widgets.DrawBoxSolid(new Rect(DrawPos.x,DrawPos.y,drawSize.x,BlockSize.y),Color.white * .25f);//select
            }
            // Widgets.DrawBoxSolid(new Rect(DrawPos.x,DrawPos.y,drawSize.x,BlockSize.y),Color.white * .125f);//hover
            Widgets.DrawHighlightIfMouseover(new Rect(DrawPos.x,DrawPos.y,drawSize.x,BlockSize.y));//hover
            Color
            cache = GUI.color;
            GUI.color = Color.white;
            Widgets.DrawBox(new Rect(DrawPos.x,DrawPos.y,BlockSize.y,BlockSize.y),2);
            GUI.color = cache;
            Thing part = parentWeapon.GetPart(id);
            Widgets.ThingIcon(new Rect(DrawPos.x,DrawPos.y,BlockSize.y,BlockSize.y),part);
            Widgets.Label(new Rect(DrawPos.x + BlockSize.y,DrawPos.y,drawSize.x - BlockSize.y,BlockSize.y),part.Label);
            if(Widgets.ButtonInvisible(new Rect(DrawPos.x + BlockSize.y,DrawPos.y,drawSize.x - BlockSize.y,BlockSize.y)))
            {
                if(open = !open) openEvent?.Invoke();
                else closeEvent?.Invoke();
            }
            if(Widgets.ButtonInvisible(new Rect(DrawPos.x,DrawPos.y,BlockSize.y,BlockSize.y))) iconEvent?.Invoke();

            return drawSize.y;
        }


        public bool open;
        public string id;
        public CompModularizationWeapon parentWeapon;
        public List<WeaponPartNodeView> childs;
        public Action openEvent;
        public Action closeEvent;
        public Action iconEvent;
    }
}
