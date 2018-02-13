﻿using System;
using Intersect;
using Intersect.Enums;
using Intersect.GameObjects;
using Intersect.Localization;
using IntersectClientExtras.File_Management;
using IntersectClientExtras.Graphics;
using IntersectClientExtras.Gwen;
using IntersectClientExtras.Gwen.Control;
using Intersect_Client.Classes.General;

namespace Intersect_Client.Classes.UI.Game
{
    public class ItemDescWindow
    {
        ImagePanel mDescWindow;

        public ItemDescWindow(int itemnum, int amount, int x, int y, int[] statBuffs, string titleOverride = "",
            string valueLabel = "")
        {
            string title = "";
            if (titleOverride == "")
                title = ItemBase.GetName(itemnum);
            else
                title = titleOverride;

            mDescWindow = new ImagePanel(Gui.GameUi.GameCanvas, "ItemDescWindow");

            var item = ItemBase.Lookup.Get<ItemBase>(itemnum);
            if (item != null)
            {
                ImagePanel icon = new ImagePanel(mDescWindow, "ItemIcon");
                GameTexture itemTex = Globals.ContentManager.GetTexture(GameContentManager.TextureType.Item, item.Pic);
                if (itemTex != null)
                {
                    icon.Texture = itemTex;
                }

                Label itemName = new Label(mDescWindow, "ItemNameLabel");
                itemName.Text = title;

                if (amount > 1)
                {
                    itemName.Text += " " + Strings.Get("itemdesc", "quantity", amount);
                }

                itemName.AddAlignment(Alignments.CenterH);

                Label itemType = new Label(mDescWindow, "ItemTypeLabel");
                Label itemValue = new Label(mDescWindow, "ItemValueLabel");
                itemType.Text = Strings.Get("itemdesc", "itemtype" + item.ItemType);
                itemValue.SetText(valueLabel);

                if (item.ItemType == (int) ItemTypes.Equipment)
                {
                    itemType.Text = Options.EquipmentSlots[item.Data1];
                    if (item.Data1 == Options.WeaponIndex && Convert.ToBoolean(item.Data4) == true)
                    {
                        itemType.Text += " - " + Strings.Get("itemdesc", "2hand");
                    }
                }
                RichLabel itemDesc = new RichLabel(mDescWindow, "ItemDescription");
                Gui.LoadRootUiData(mDescWindow,
                    "InGame.xml"); //Load this up now so we know what color to make the text when filling out the desc
                if (item.Desc.Length > 0)
                {
                    itemDesc.AddText(Strings.Get("itemdesc", "desc", item.Desc), itemDesc.RenderColor);
                    itemDesc.AddLineBreak();
                    itemDesc.AddLineBreak();
                }

                string stats = "";
                if (item.ItemType == (int) ItemTypes.Equipment)
                {
                    stats = Strings.Get("itemdesc", "bonuses");
                    itemDesc.AddText(stats, itemDesc.RenderColor);
                    itemDesc.AddLineBreak();
                    if (item.ItemType == (int) ItemTypes.Equipment && item.Data1 == Options.WeaponIndex)
                    {
                        stats = Strings.Get("itemdesc", "damage", item.Damage);
                        itemDesc.AddText(stats, itemDesc.RenderColor);
                        itemDesc.AddLineBreak();
                    }
                    if (statBuffs != null)
                    {
                        for (int i = 0; i < Options.MaxStats; i++)
                        {
                            stats = Strings.Get("itemdesc", "stat" + i, (item.StatsGiven[i] + statBuffs[i]));
                            itemDesc.AddText(stats, itemDesc.RenderColor);
                            itemDesc.AddLineBreak();
                        }
                    }
                }
                if (item.ItemType == (int) ItemTypes.Equipment && item.Data2 > 0 && item.Data3 > 0)
                {
                    itemDesc.AddText(
                        Strings.Get("itemdesc", "effect", item.Data3,
                            Strings.Get("itemdesc", "effect" + (item.Data2 - 1))), itemDesc.RenderColor);
                }
                //Load Again for positioning purposes.
                Gui.LoadRootUiData(mDescWindow, "InGame.xml");
                itemDesc.SizeToChildren(false, true);
                mDescWindow.SetPosition(x, y);
            }
        }

        public void Dispose()
        {
            Gui.GameUi.GameCanvas.RemoveChild(mDescWindow, false);
            mDescWindow.Dispose();
        }
    }
}