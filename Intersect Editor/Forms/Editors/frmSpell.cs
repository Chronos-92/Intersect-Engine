﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DarkUI.Controls;
using DarkUI.Forms;
using Intersect.Editor.Classes;
using Intersect.Editor.Classes.Core;
using Intersect.Editor.Forms.Editors;
using Intersect.Enums;
using Intersect.GameObjects;
using Intersect.GameObjects.Maps.MapList;
using Intersect.Localization;
using Intersect.Utilities;

namespace Intersect.Editor.Forms
{
    public partial class FrmSpell : EditorForm
    {
        private List<SpellBase> mChanged = new List<SpellBase>();
        private byte[] mCopiedItem;
        private SpellBase mEditorItem;

        public FrmSpell()
        {
            ApplyHooks();
            InitializeComponent();
            lstSpells.LostFocus += itemList_FocusChanged;
            lstSpells.GotFocus += itemList_FocusChanged;
        }

        protected override void GameObjectUpdatedDelegate(GameObjectType type)
        {
            if (type == GameObjectType.Spell)
            {
                InitEditor();
                if (mEditorItem != null && !SpellBase.Lookup.Values.Contains(mEditorItem))
                {
                    mEditorItem = null;
                    UpdateEditor();
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            foreach (var item in mChanged)
            {
                item.RestoreBackup();
                item.DeleteBackup();
            }

            Hide();
            Globals.CurrentEditor = -1;
            Dispose();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            //Send Changed items
            foreach (var item in mChanged)
            {
                PacketSender.SendSaveObject(item);
                item.DeleteBackup();
            }

            Hide();
            Globals.CurrentEditor = -1;
            Dispose();
        }

        private void lstSpells_Click(object sender, EventArgs e)
        {
            if (mChangingName) return;
            mEditorItem =
                SpellBase.Lookup.Get<SpellBase>(
                    Database.GameObjectIdFromList(GameObjectType.Spell, lstSpells.SelectedIndex));
            UpdateEditor();
        }

        private void frmSpell_Load(object sender, EventArgs e)
        {
            cmbProjectile.Items.Clear();
            cmbProjectile.Items.AddRange(Database.GetGameObjectList(GameObjectType.Projectile));
            cmbCastAnimation.Items.Clear();
            cmbCastAnimation.Items.Add(Strings.Get("general", "none"));
            cmbCastAnimation.Items.AddRange(Database.GetGameObjectList(GameObjectType.Animation));
            cmbHitAnimation.Items.Clear();
            cmbHitAnimation.Items.Add(Strings.Get("general", "none"));
            cmbHitAnimation.Items.AddRange(Database.GetGameObjectList(GameObjectType.Animation));
            cmbEvent.Items.Clear();
            cmbEvent.Items.Add(Strings.Get("general", "none"));
            cmbEvent.Items.AddRange(Database.GetGameObjectList(GameObjectType.CommonEvent));

            cmbSprite.Items.Clear();
            cmbSprite.Items.Add(Strings.Get("general", "none"));
            var spellNames = GameContentManager.GetSmartSortedTextureNames(GameContentManager.TextureType.Spell);
            cmbSprite.Items.AddRange(spellNames);

            cmbTransform.Items.Clear();
            cmbTransform.Items.Add(Strings.Get("general", "none"));
            var spriteNames = GameContentManager.GetSmartSortedTextureNames(GameContentManager.TextureType.Entity);
            cmbTransform.Items.AddRange(spriteNames);

            nudWarpX.Maximum = (int) Options.MapWidth;
            nudWarpY.Maximum = (int) Options.MapHeight;

            cmbWarpMap.Items.Clear();
            cmbWarpMap.Items.AddRange(MapList.GetOrderedMaps().Select(map => map?.Name).ToArray());
            cmbWarpMap.SelectedIndex = 0;

            nudStr.Maximum = Options.MaxStatValue;
            nudMag.Maximum = Options.MaxStatValue;
            nudDef.Maximum = Options.MaxStatValue;
            nudMR.Maximum = Options.MaxStatValue;
            nudSpd.Maximum = Options.MaxStatValue;

            InitLocalization();
            UpdateEditor();
        }

        private void InitLocalization()
        {
            Text = Strings.Get("spelleditor", "title");
            toolStripItemNew.Text = Strings.Get("spelleditor", "new");
            toolStripItemDelete.Text = Strings.Get("spelleditor", "delete");
            toolStripItemCopy.Text = Strings.Get("spelleditor", "copy");
            toolStripItemPaste.Text = Strings.Get("spelleditor", "paste");
            toolStripItemUndo.Text = Strings.Get("spelleditor", "undo");

            grpSpells.Text = Strings.Get("spelleditor", "spells");

            grpGeneral.Text = Strings.Get("spelleditor", "general");
            lblName.Text = Strings.Get("spelleditor", "name");
            lblType.Text = Strings.Get("spelleditor", "type");

            cmbType.Items.Clear();
            for (var i = 0; i < 5; i++)
            {
                cmbType.Items.Add(Strings.Get("spelleditor", "type" + i));
            }

            lblIcon.Text = Strings.Get("spelleditor", "icon");
            lblDesc.Text = Strings.Get("spelleditor", "description");
            lblCastAnimation.Text = Strings.Get("spelleditor", "castanimation");
            lblHitAnimation.Text = Strings.Get("spelleditor", "hitanimation");

            grpRequirements.Text = Strings.Get("spelleditor", "requirements");
            btnDynamicRequirements.Text = Strings.Get("spelleditor", "requirementsbutton");

            grpSpellCost.Text = Strings.Get("spelleditor", "cost");
            lblHPCost.Text = Strings.Get("spelleditor", "hpcost");
            lblMPCost.Text = Strings.Get("spelleditor", "manacost");
            lblCastDuration.Text = Strings.Get("spelleditor", "casttime");
            lblCooldownDuration.Text = Strings.Get("spelleditor", "cooldown");

            grpTargetInfo.Text = Strings.Get("spelleditor", "targetting");
            lblTargetType.Text = Strings.Get("spelleditor", "targettype");

            cmbTargetType.Items.Clear();
            for (var i = 0; i < 4; i++)
            {
                cmbTargetType.Items.Add(Strings.Get("spelleditor", "targettype" + i));
            }

            lblCastRange.Text = Strings.Get("spelleditor", "castrange");
            lblProjectile.Text = Strings.Get("spelleditor", "projectile");
            lblHitRadius.Text = Strings.Get("spelleditor", "hitradius");

            grpCombat.Text = Strings.Get("spelleditor", "combatspell");
            grpDamage.Text = Strings.Get("spelleditor", "damagegroup");
            lblCritChance.Text = Strings.Get("spelleditor", "critchance");
            lblDamageType.Text = Strings.Get("spelleditor", "damagetype");
            lblHPDamage.Text = Strings.Get("spelleditor", "hpdamage");
            lblManaDamage.Text = Strings.Get("spelleditor", "mpdamage");
            chkFriendly.Text = Strings.Get("spelleditor", "friendly");

            cmbDamageType.Items.Clear();
            for (var i = 0; i < 3; i++)
            {
                cmbDamageType.Items.Add(Strings.Get("spelleditor", "damagetype" + i));
            }

            lblScalingStat.Text = Strings.Get("spelleditor", "scalingstat");
            lblScaling.Text = Strings.Get("spelleditor", "scalingamount");

            grpHotDot.Text = Strings.Get("spelleditor", "hotdot");
            chkHOTDOT.Text = Strings.Get("spelleditor", "ishotdot");
            lblTick.Text = Strings.Get("spelleditor", "hotdottick");

            grpStats.Text = Strings.Get("spelleditor", "stats");
            lblStr.Text = Strings.Get("spelleditor", "attack");
            lblDef.Text = Strings.Get("spelleditor", "defense");
            lblSpd.Text = Strings.Get("spelleditor", "speed");
            lblMag.Text = Strings.Get("spelleditor", "abilitypower");
            lblMR.Text = Strings.Get("spelleditor", "magicresist");

            grpEffectDuration.Text = Strings.Get("spelleditor", "boostduration");
            lblBuffDuration.Text = Strings.Get("spelleditor", "duration");
            grpEffect.Text = Strings.Get("spelleditor", "effectgroup");
            lblEffect.Text = Strings.Get("spelleditor", "effectlabel");

            cmbExtraEffect.Items.Clear();
            for (var i = 0; i < 7; i++)
            {
                cmbExtraEffect.Items.Add(Strings.Get("spelleditor", "effect" + i));
            }

            lblSprite.Text = Strings.Get("spelleditor", "transformsprite");

            grpDash.Text = Strings.Get("spelleditor", "dash");
            lblRange.Text = Strings.Get("spelleditor", "dashrange", scrlRange.Value);
            grpDashCollisions.Text = Strings.Get("spelleditor", "dashcollisions");
            chkIgnoreMapBlocks.Text = Strings.Get("spelleditor", "ignoreblocks");
            chkIgnoreActiveResources.Text = Strings.Get("spelleditor", "ignoreactiveresources");
            chkIgnoreInactiveResources.Text = Strings.Get("spelleditor", "ignoreinactiveresources");
            chkIgnoreZDimensionBlocks.Text = Strings.Get("spelleditor", "ignorezdimension");

            grpWarp.Text = Strings.Get("spelleditor", "warptomap");
            lblMap.Text = Strings.Get("warping", "map", "");
            lblX.Text = Strings.Get("warping", "x", "");
            lblY.Text = Strings.Get("warping", "y", "");
            lblWarpDir.Text = Strings.Get("warping", "direction", "");

            cmbDirection.Items.Clear();
            for (var i = -1; i < 4; i++)
            {
                cmbDirection.Items.Add(Strings.Get("directions", i.ToString()));
            }

            btnVisualMapSelector.Text = Strings.Get("warping", "visual");

            grpEvent.Text = Strings.Get("spelleditor", "event");
            lblEvent.Text = Strings.Get("spelleditor", "eventlabel");

            btnSave.Text = Strings.Get("spelleditor", "save");
            btnCancel.Text = Strings.Get("spelleditor", "cancel");
        }

        public void InitEditor()
        {
            lstSpells.Items.Clear();
            lstSpells.Items.AddRange(Database.GetGameObjectList(GameObjectType.Spell));
            cmbScalingStat.Items.Clear();
            for (var i = 0; i < Options.MaxStats; i++)
            {
                cmbScalingStat.Items.Add(Globals.GetStatName(i));
            }
        }

        private void UpdateEditor()
        {
            if (mEditorItem != null)
            {
                pnlContainer.Show();

                txtName.Text = mEditorItem.Name;
                txtDesc.Text = mEditorItem.Desc;
                cmbType.SelectedIndex = mEditorItem.SpellType;

                nudCastDuration.Value = mEditorItem.CastDuration * 100;
                nudCooldownDuration.Value = mEditorItem.CooldownDuration * 100;

                cmbCastAnimation.SelectedIndex =
                    Database.GameObjectListIndex(GameObjectType.Animation, mEditorItem.CastAnimation) + 1;
                cmbHitAnimation.SelectedIndex =
                    Database.GameObjectListIndex(GameObjectType.Animation, mEditorItem.HitAnimation) + 1;

                cmbSprite.SelectedIndex = cmbSprite.FindString(TextUtils.NullToNone(mEditorItem.Pic));
                if (cmbSprite.SelectedIndex > 0)
                {
                    picSpell.BackgroundImage = Image.FromFile("resources/spells/" + cmbSprite.Text);
                }
                else
                {
                    picSpell.BackgroundImage = null;
                }
                nudHPCost.Value = mEditorItem.VitalCost[(int) Vitals.Health];
                nudMpCost.Value = mEditorItem.VitalCost[(int) Vitals.Mana];

                UpdateSpellTypePanels();
                if (mChanged.IndexOf(mEditorItem) == -1)
                {
                    mChanged.Add(mEditorItem);
                    mEditorItem.MakeBackup();
                }
            }
            else
            {
                pnlContainer.Hide();
            }
            UpdateToolStripItems();
        }

        private void UpdateSpellTypePanels()
        {
            grpTargetInfo.Hide();
            grpCombat.Hide();
            grpWarp.Hide();
            grpDash.Hide();
            grpEvent.Hide();
            cmbTargetType.Enabled = true;

            if (cmbType.SelectedIndex == (int) SpellTypes.CombatSpell)
            {
                grpTargetInfo.Show();
                grpCombat.Show();
                cmbTargetType.SelectedIndex = mEditorItem.TargetType;
                UpdateTargetTypePanel();

                nudHPDamage.Value = mEditorItem.VitalDiff[(int) Vitals.Health];
                nudMPDamage.Value = mEditorItem.VitalDiff[(int) Vitals.Mana];
                nudStr.Value = mEditorItem.StatDiff[(int) Stats.Attack];
                nudDef.Value = mEditorItem.StatDiff[(int) Stats.Defense];
                nudSpd.Value = mEditorItem.StatDiff[(int) Stats.Speed];
                nudMag.Value = mEditorItem.StatDiff[(int) Stats.AbilityPower];
                nudMR.Value = mEditorItem.StatDiff[(int) Stats.MagicResist];

                chkFriendly.Checked = Convert.ToBoolean(mEditorItem.Friendly);
                cmbDamageType.SelectedIndex = mEditorItem.DamageType;
                cmbScalingStat.SelectedIndex = mEditorItem.ScalingStat;
                nudScaling.Value = mEditorItem.Scaling;
                nudCritChance.Value = mEditorItem.CritChance;

                chkHOTDOT.Checked = Convert.ToBoolean(mEditorItem.Data1);
                nudBuffDuration.Value = mEditorItem.Data2 * 100;
                nudTick.Value = mEditorItem.Data4 * 100;
                cmbExtraEffect.SelectedIndex = mEditorItem.Data3;
                cmbExtraEffect_SelectedIndexChanged(null, null);
            }
            else if (cmbType.SelectedIndex == (int) SpellTypes.Warp)
            {
                grpWarp.Show();
                for (int i = 0; i < MapList.GetOrderedMaps().Count; i++)
                {
                    if (MapList.GetOrderedMaps()[i].MapNum == mEditorItem.Data1)
                    {
                        cmbWarpMap.SelectedIndex = i;
                        break;
                    }
                }
                nudWarpX.Value = mEditorItem.Data2;
                nudWarpY.Value = mEditorItem.Data3;
                cmbDirection.SelectedIndex = mEditorItem.Data4;
            }
            else if (cmbType.SelectedIndex == (int) SpellTypes.WarpTo)
            {
                grpTargetInfo.Show();
                cmbTargetType.SelectedIndex = (int) SpellTargetTypes.Single;
                cmbTargetType.Enabled = false;
                UpdateTargetTypePanel();
            }
            else if (cmbType.SelectedIndex == (int) SpellTypes.Dash)
            {
                grpDash.Show();
                scrlRange.Value = mEditorItem.CastRange;
                lblRange.Text = Strings.Get("spelleditor", "dashrange", scrlRange.Value);
                chkIgnoreMapBlocks.Checked = Convert.ToBoolean(mEditorItem.Data1);
                chkIgnoreActiveResources.Checked = Convert.ToBoolean(mEditorItem.Data2);
                chkIgnoreInactiveResources.Checked = Convert.ToBoolean(mEditorItem.Data3);
                chkIgnoreZDimensionBlocks.Checked = Convert.ToBoolean(mEditorItem.Data4);
            }
            else if (cmbType.SelectedIndex == (int) SpellTypes.Event)
            {
                grpEvent.Show();
                cmbEvent.SelectedIndex = Database.GameObjectListIndex(GameObjectType.CommonEvent, mEditorItem.Data1) +
                                         1;
            }
        }

        private void UpdateTargetTypePanel()
        {
            lblHitRadius.Hide();
            nudHitRadius.Hide();
            lblCastRange.Hide();
            nudCastRange.Hide();
            lblProjectile.Hide();
            cmbProjectile.Hide();
            if (cmbTargetType.SelectedIndex == (int) SpellTargetTypes.Single)
            {
                lblCastRange.Show();
                nudCastRange.Show();
                nudCastRange.Value = mEditorItem.CastRange;
                if (cmbType.SelectedIndex == (int) SpellTypes.CombatSpell)
                {
                    lblHitRadius.Show();
                    nudHitRadius.Show();
                    nudHitRadius.Value = mEditorItem.HitRadius;
                }
            }
            if (cmbTargetType.SelectedIndex == (int) SpellTargetTypes.AoE &&
                cmbType.SelectedIndex == (int) SpellTypes.CombatSpell)
            {
                lblHitRadius.Show();
                nudHitRadius.Show();
                nudHitRadius.Value = mEditorItem.HitRadius;
            }
            if (cmbTargetType.SelectedIndex < (int) SpellTargetTypes.Self)
            {
                lblCastRange.Show();
                nudCastRange.Show();
                nudCastRange.Value = mEditorItem.CastRange;
            }
            if (cmbTargetType.SelectedIndex == (int) SpellTargetTypes.Projectile)
            {
                lblProjectile.Show();
                cmbProjectile.Show();
                cmbProjectile.SelectedIndex =
                    Database.GameObjectListIndex(GameObjectType.Projectile, mEditorItem.Projectile);
            }
        }

        private void txtName_TextChanged(object sender, EventArgs e)
        {
            mChangingName = true;
            mEditorItem.Name = txtName.Text;
            lstSpells.Items[Database.GameObjectListIndex(GameObjectType.Spell, mEditorItem.Index)] = txtName.Text;
            mChangingName = false;
        }

        private void cmbType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbType.SelectedIndex != mEditorItem.SpellType)
            {
                mEditorItem.SpellType = (byte) cmbType.SelectedIndex;
                mEditorItem.Data1 = 0;
                mEditorItem.Data2 = 0;
                mEditorItem.Data3 = 0;
                mEditorItem.Data4 = 0;
                UpdateSpellTypePanels();
            }
        }

        private void cmbSprite_SelectedIndexChanged(object sender, EventArgs e)
        {
            mEditorItem.Pic = cmbSprite.Text;
            picSpell.BackgroundImage = cmbSprite.SelectedIndex > 0 ? Image.FromFile("resources/spells/" + cmbSprite.Text) : null;
        }

        private void cmbTargetType_SelectedIndexChanged(object sender, EventArgs e)
        {
            mEditorItem.TargetType = cmbTargetType.SelectedIndex;
            UpdateTargetTypePanel();
        }

        private void chkHOTDOT_CheckedChanged(object sender, EventArgs e)
        {
            mEditorItem.Data1 = Convert.ToInt32(chkHOTDOT.Checked);
        }

        private void txtDesc_TextChanged(object sender, EventArgs e)
        {
            mEditorItem.Desc = txtDesc.Text;
        }

        private void cmbExtraEffect_SelectedIndexChanged(object sender, EventArgs e)
        {
            mEditorItem.Data3 = cmbExtraEffect.SelectedIndex;

            lblSprite.Visible = false;
            cmbTransform.Visible = false;
            picSprite.Visible = false;

            if (cmbExtraEffect.SelectedIndex == 6) //Transform
            {
                lblSprite.Visible = true;
                cmbTransform.Visible = true;
                picSprite.Visible = true;

                cmbTransform.SelectedIndex = cmbTransform.FindString(TextUtils.NullToNone(mEditorItem.Data5));
                if (cmbTransform.SelectedIndex > 0)
                {
                    Bitmap bmp = new Bitmap(picSprite.Width, picSprite.Height);
                    var g = Graphics.FromImage(bmp);
                    Image src = Image.FromFile("resources/entities/" + cmbTransform.Text);
                    g.DrawImage(src,
                        new Rectangle(picSprite.Width / 2 - src.Width / 8, picSprite.Height / 2 - src.Height / 8,
                            src.Width / 4, src.Height / 4),
                        new Rectangle(0, 0, src.Width / 4, src.Height / 4), GraphicsUnit.Pixel);
                    g.Dispose();
                    src.Dispose();
                    picSprite.BackgroundImage = bmp;
                }
                else
                {
                    picSprite.BackgroundImage = null;
                }
            }
        }

        private void frmSpell_FormClosed(object sender, FormClosedEventArgs e)
        {
            Globals.CurrentEditor = -1;
        }

        private void scrlRange_Scroll(object sender, ScrollValueEventArgs e)
        {
            lblRange.Text = Strings.Get("spelleditor", "dashrange", scrlRange.Value);
            mEditorItem.CastRange = scrlRange.Value;
        }

        private void chkIgnoreMapBlocks_CheckedChanged(object sender, EventArgs e)
        {
            mEditorItem.Data1 = Convert.ToInt32(chkIgnoreMapBlocks.Checked);
        }

        private void chkIgnoreActiveResources_CheckedChanged(object sender, EventArgs e)
        {
            mEditorItem.Data2 = Convert.ToInt32(chkIgnoreActiveResources.Checked);
        }

        private void chkIgnoreInactiveResources_CheckedChanged(object sender, EventArgs e)
        {
            mEditorItem.Data3 = Convert.ToInt32(chkIgnoreInactiveResources.Checked);
        }

        private void chkIgnoreZDimensionBlocks_CheckedChanged(object sender, EventArgs e)
        {
            mEditorItem.Data4 = Convert.ToInt32(chkIgnoreZDimensionBlocks.Checked);
        }

        private void cmbTransform_SelectedIndexChanged(object sender, EventArgs e)
        {
            mEditorItem.Data5 = cmbTransform.Text;
            if (cmbTransform.SelectedIndex > 0)
            {
                Bitmap bmp = new Bitmap(picSprite.Width, picSprite.Height);
                var g = Graphics.FromImage(bmp);
                Image src = Image.FromFile("resources/entities/" + cmbTransform.Text);
                g.DrawImage(src,
                    new Rectangle(picSprite.Width / 2 - src.Width / 8, picSprite.Height / 2 - src.Height / 8,
                        src.Width / 4, src.Height / 4),
                    new Rectangle(0, 0, src.Width / 4, src.Height / 4), GraphicsUnit.Pixel);
                g.Dispose();
                src.Dispose();
                picSprite.BackgroundImage = bmp;
            }
            else
            {
                picSprite.BackgroundImage = null;
            }
        }

        private void toolStripItemNew_Click(object sender, EventArgs e)
        {
            PacketSender.SendCreateObject(GameObjectType.Spell);
        }

        private void toolStripItemDelete_Click(object sender, EventArgs e)
        {
            if (mEditorItem != null && lstSpells.Focused)
            {
                if (DarkMessageBox.ShowWarning(Strings.Get("spelleditor", "deleteprompt"),
                        Strings.Get("spelleditor", "deletetitle"), DarkDialogButton.YesNo, Properties.Resources.Icon) ==
                    DialogResult.Yes)
                {
                    PacketSender.SendDeleteObject(mEditorItem);
                }
            }
        }

        private void toolStripItemCopy_Click(object sender, EventArgs e)
        {
            if (mEditorItem != null && lstSpells.Focused)
            {
                mCopiedItem = mEditorItem.BinaryData;
                toolStripItemPaste.Enabled = true;
            }
        }

        private void toolStripItemPaste_Click(object sender, EventArgs e)
        {
            if (mEditorItem != null && mCopiedItem != null && lstSpells.Focused)
            {
                mEditorItem.Load(mCopiedItem);
                UpdateEditor();
            }
        }

        private void toolStripItemUndo_Click(object sender, EventArgs e)
        {
            if (mChanged.Contains(mEditorItem) && mEditorItem != null)
            {
                if (DarkMessageBox.ShowWarning(Strings.Get("spelleditor", "undoprompt"),
                        Strings.Get("spelleditor", "undotitle"), DarkDialogButton.YesNo, Properties.Resources.Icon) ==
                    DialogResult.Yes)
                {
                    mEditorItem.RestoreBackup();
                    UpdateEditor();
                }
            }
        }

        private void itemList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                if (e.KeyCode == Keys.Z)
                {
                    toolStripItemUndo_Click(null, null);
                }
                else if (e.KeyCode == Keys.V)
                {
                    toolStripItemPaste_Click(null, null);
                }
                else if (e.KeyCode == Keys.C)
                {
                    toolStripItemCopy_Click(null, null);
                }
            }
            else
            {
                if (e.KeyCode == Keys.Delete)
                {
                    toolStripItemDelete_Click(null, null);
                }
            }
        }

        private void UpdateToolStripItems()
        {
            toolStripItemCopy.Enabled = mEditorItem != null && lstSpells.Focused;
            toolStripItemPaste.Enabled = mEditorItem != null && mCopiedItem != null && lstSpells.Focused;
            toolStripItemDelete.Enabled = mEditorItem != null && lstSpells.Focused;
            toolStripItemUndo.Enabled = mEditorItem != null && lstSpells.Focused;
        }

        private void itemList_FocusChanged(object sender, EventArgs e)
        {
            UpdateToolStripItems();
        }

        private void form_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                if (e.KeyCode == Keys.N)
                {
                    toolStripItemNew_Click(null, null);
                }
            }
        }

        private void chkFriendly_CheckedChanged(object sender, EventArgs e)
        {
            mEditorItem.Friendly = Convert.ToInt32(chkFriendly.Checked);
        }

        private void cmbDamageType_SelectedIndexChanged(object sender, EventArgs e)
        {
            mEditorItem.DamageType = cmbDamageType.SelectedIndex;
        }

        private void cmbScalingStat_SelectedIndexChanged(object sender, EventArgs e)
        {
            mEditorItem.ScalingStat = cmbScalingStat.SelectedIndex;
        }

        private void btnDynamicRequirements_Click(object sender, EventArgs e)
        {
            var frm = new FrmDynamicRequirements(mEditorItem.CastingReqs, RequirementType.Spell);
            frm.ShowDialog();
        }

        private void cmbCastAnimation_SelectedIndexChanged(object sender, EventArgs e)
        {
            mEditorItem.CastAnimation = Database.GameObjectIdFromList(GameObjectType.Animation,
                cmbCastAnimation.SelectedIndex - 1);
        }

        private void cmbHitAnimation_SelectedIndexChanged(object sender, EventArgs e)
        {
            mEditorItem.HitAnimation = Database.GameObjectIdFromList(GameObjectType.Animation,
                cmbHitAnimation.SelectedIndex - 1);
        }

        private void cmbProjectile_SelectedIndexChanged(object sender, EventArgs e)
        {
            mEditorItem.Projectile =
                Database.GameObjectIdFromList(GameObjectType.Projectile, cmbProjectile.SelectedIndex);
        }

        private void cmbEvent_SelectedIndexChanged(object sender, EventArgs e)
        {
            mEditorItem.Data1 = Database.GameObjectIdFromList(GameObjectType.CommonEvent, cmbEvent.SelectedIndex - 1);
        }

        private void btnVisualMapSelector_Click(object sender, EventArgs e)
        {
            FrmWarpSelection frmWarpSelection = new FrmWarpSelection();
            frmWarpSelection.SelectTile(MapList.GetOrderedMaps()[cmbWarpMap.SelectedIndex].MapNum, (int) nudWarpX.Value,
                (int) nudWarpY.Value);
            frmWarpSelection.ShowDialog();
            if (frmWarpSelection.GetResult())
            {
                for (int i = 0; i < MapList.GetOrderedMaps().Count; i++)
                {
                    if (MapList.GetOrderedMaps()[i].MapNum == frmWarpSelection.GetMap())
                    {
                        cmbWarpMap.SelectedIndex = i;
                        break;
                    }
                }
                nudWarpX.Value = frmWarpSelection.GetX();
                nudWarpY.Value = frmWarpSelection.GetY();
            }
        }

        private void cmbWarpMap_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbWarpMap.SelectedIndex > -1 && mEditorItem != null)
            {
                mEditorItem.Data1 = MapList.GetOrderedMaps()[cmbWarpMap.SelectedIndex].MapNum;
            }
        }

        private void nudWarpX_ValueChanged(object sender, EventArgs e)
        {
            mEditorItem.Data2 = (int) nudWarpX.Value;
        }

        private void nudWarpY_ValueChanged(object sender, EventArgs e)
        {
            mEditorItem.Data3 = (int) nudWarpY.Value;
        }

        private void cmbDirection_SelectedIndexChanged(object sender, EventArgs e)
        {
            mEditorItem.Data4 = cmbDirection.SelectedIndex;
        }

        private void nudCastDuration_ValueChanged(object sender, EventArgs e)
        {
            mEditorItem.CastDuration = (int) nudCastDuration.Value / 100;
        }

        private void nudCooldownDuration_ValueChanged(object sender, EventArgs e)
        {
            mEditorItem.CooldownDuration = (int) nudCooldownDuration.Value / 100;
        }

        private void nudHitRadius_ValueChanged(object sender, EventArgs e)
        {
            mEditorItem.HitRadius = (int) nudHitRadius.Value;
        }

        private void nudHPCost_ValueChanged(object sender, EventArgs e)
        {
            mEditorItem.VitalCost[(int) Vitals.Health] = (int) nudHPCost.Value;
        }

        private void nudMpCost_ValueChanged(object sender, EventArgs e)
        {
            mEditorItem.VitalCost[(int) Vitals.Mana] = (int) nudMpCost.Value;
        }

        private void nudHPDamage_ValueChanged(object sender, EventArgs e)
        {
            mEditorItem.VitalDiff[(int) Vitals.Health] = (int) nudHPDamage.Value;
        }

        private void nudMPDamage_ValueChanged(object sender, EventArgs e)
        {
            mEditorItem.VitalDiff[(int) Vitals.Mana] = (int) nudMPDamage.Value;
        }

        private void nudStr_ValueChanged(object sender, EventArgs e)
        {
            mEditorItem.StatDiff[(int) Stats.Attack] = (int) nudStr.Value;
        }

        private void nudMag_ValueChanged(object sender, EventArgs e)
        {
            mEditorItem.StatDiff[(int) Stats.AbilityPower] = (int) nudMag.Value;
        }

        private void nudDef_ValueChanged(object sender, EventArgs e)
        {
            mEditorItem.StatDiff[(int) Stats.Defense] = (int) nudDef.Value;
        }

        private void nudMR_ValueChanged(object sender, EventArgs e)
        {
            mEditorItem.StatDiff[(int) Stats.MagicResist] = (int) nudMR.Value;
        }

        private void nudSpd_ValueChanged(object sender, EventArgs e)
        {
            mEditorItem.StatDiff[(int) Stats.Speed] = (int) nudSpd.Value;
        }

        private void nudBuffDuration_ValueChanged(object sender, EventArgs e)
        {
            mEditorItem.Data2 = (int) nudBuffDuration.Value / 100;
        }

        private void nudTick_ValueChanged(object sender, EventArgs e)
        {
            mEditorItem.Data4 = (int) nudTick.Value / 100;
        }

        private void nudCritChance_ValueChanged(object sender, EventArgs e)
        {
            mEditorItem.CritChance = (int) nudCritChance.Value;
        }

        private void nudScaling_ValueChanged(object sender, EventArgs e)
        {
            mEditorItem.Scaling = (int) nudScaling.Value;
        }

        private void nudCastRange_ValueChanged(object sender, EventArgs e)
        {
            mEditorItem.CastRange = (int) nudCastRange.Value;
        }
    }
}