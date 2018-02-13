﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using DarkUI.Controls;
using DarkUI.Forms;
using Intersect.Editor.Classes;
using Intersect.Editor.Classes.Core;
using Intersect.Editor.Forms.Editors;
using Intersect.Enums;
using Intersect.GameObjects;
using Intersect.Localization;
using Intersect.Utilities;
using Microsoft.Xna.Framework.Graphics;

namespace Intersect.Editor.Forms
{
    public partial class FrmAnimation : EditorForm
    {
        private List<AnimationBase> mChanged = new List<AnimationBase>();
        private byte[] mCopiedItem;
        private AnimationBase mEditorItem;

        private int mLowerFrame;

        private bool mPlayLower;
        private bool mPlayUpper;
        private int mUpperFrame;
        private RenderTarget2D mLowerDarkness;

        //Mono Rendering Variables
        private SwapChainRenderTarget mLowerWindow;

        private RenderTarget2D mUpperDarkness;
        private SwapChainRenderTarget mUpperWindow;

        public FrmAnimation()
        {
            ApplyHooks();
            InitializeComponent();
            lstAnimations.LostFocus += itemList_FocusChanged;
            lstAnimations.GotFocus += itemList_FocusChanged;
        }

        protected override void GameObjectUpdatedDelegate(GameObjectType type)
        {
            if (type != GameObjectType.Animation) return;
            InitEditor();
            if (mEditorItem == null || AnimationBase.Lookup.Values.Contains(mEditorItem)) return;
            mEditorItem = null;
            UpdateEditor();
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

        private void lstAnimations_Click(object sender, EventArgs e)
        {
            if (mChangingName) return;
            mEditorItem =
                AnimationBase.Lookup.Get<AnimationBase>(
                    Database.GameObjectIdFromList(GameObjectType.Animation, lstAnimations.SelectedIndex));
            UpdateEditor();
        }

        private void frmAnimation_Load(object sender, EventArgs e)
        {
            //Animation Sound
            cmbSound.Items.Clear();
            cmbSound.Items.Add(Strings.Get("general", "none"));
            cmbSound.Items.AddRange(GameContentManager.SmartSortedSoundNames);

            //Lower Animation Graphic
            cmbLowerGraphic.Items.Clear();
            cmbLowerGraphic.Items.Add(Strings.Get("general", "none"));
            cmbLowerGraphic.Items.AddRange(
                GameContentManager.GetSmartSortedTextureNames(GameContentManager.TextureType.Animation));

            //Upper Animation Graphic
            cmbUpperGraphic.Items.Clear();
            cmbUpperGraphic.Items.Add(Strings.Get("general", "none"));
            cmbUpperGraphic.Items.AddRange(
                GameContentManager.GetSmartSortedTextureNames(GameContentManager.TextureType.Animation));

            mLowerWindow = new SwapChainRenderTarget(EditorGraphics.GetGraphicsDevice(), picLowerAnimation.Handle,
                picLowerAnimation.Width, picLowerAnimation.Height);
            mUpperWindow = new SwapChainRenderTarget(EditorGraphics.GetGraphicsDevice(), picUpperAnimation.Handle,
                picUpperAnimation.Width, picUpperAnimation.Height);
            mLowerDarkness = EditorGraphics.CreateRenderTexture(picLowerAnimation.Width, picLowerAnimation.Height);
            mUpperDarkness = EditorGraphics.CreateRenderTexture(picUpperAnimation.Width, picUpperAnimation.Height);

            InitLocalization();
            UpdateEditor();
        }

        private void InitLocalization()
        {
            Text = Strings.Get("animationeditor", "title");
            toolStripItemNew.Text = Strings.Get("animationeditor", "new");
            toolStripItemDelete.Text = Strings.Get("animationeditor", "delete");
            toolStripItemCopy.Text = Strings.Get("animationeditor", "copy");
            toolStripItemPaste.Text = Strings.Get("animationeditor", "paste");
            toolStripItemUndo.Text = Strings.Get("animationeditor", "undo");

            grpAnimations.Text = Strings.Get("animationeditor", "animations");

            grpGeneral.Text = Strings.Get("animationeditor", "general");
            lblName.Text = Strings.Get("animationeditor", "name");
            lblSound.Text = Strings.Get("animationeditor", "sound");
            labelDarkness.Text = Strings.Get("animationeditor", "simulatedarkness", scrlDarkness.Value);
            btnSwap.Text = Strings.Get("animationeditor", "swap");

            grpLower.Text = Strings.Get("animationeditor", "lowergroup");
            lblLowerGraphic.Text = Strings.Get("animationeditor", "lowergraphic");
            lblLowerHorizontalFrames.Text = Strings.Get("animationeditor", "lowerhorizontalframes");
            lblLowerVerticalFrames.Text = Strings.Get("animationeditor", "lowerverticalframes");
            lblLowerFrameCount.Text = Strings.Get("animationeditor", "lowerframecount");
            lblLowerFrameDuration.Text = Strings.Get("animationeditor", "lowerframeduration");
            lblLowerLoopCount.Text = Strings.Get("animationeditor", "lowerloopcount");
            grpLowerPlayback.Text = Strings.Get("animationeditor", "lowerplayback");
            lblLowerFrame.Text = Strings.Get("animationeditor", "lowerframe", scrlLowerFrame.Value);
            btnPlayLower.Text = Strings.Get("animationeditor", "lowerplay");
            grpLowerFrameOpts.Text = Strings.Get("animationeditor", "lowerframeoptions");
            btnLowerClone.Text = Strings.Get("animationeditor", "lowerclone");

            grpUpper.Text = Strings.Get("animationeditor", "uppergroup");
            lblUpperGraphic.Text = Strings.Get("animationeditor", "uppergraphic");
            lblUpperHorizontalFrames.Text = Strings.Get("animationeditor", "upperhorizontalframes");
            lblUpperVerticalFrames.Text = Strings.Get("animationeditor", "upperverticalframes");
            lblUpperFrameCount.Text = Strings.Get("animationeditor", "upperframecount");
            lblUpperFrameDuration.Text = Strings.Get("animationeditor", "upperframeduration");
            lblUpperLoopCount.Text = Strings.Get("animationeditor", "upperloopcount");
            grpUpperPlayback.Text = Strings.Get("animationeditor", "upperplayback");
            lblUpperFrame.Text = Strings.Get("animationeditor", "upperframe", scrlUpperFrame.Value);
            btnPlayUpper.Text = Strings.Get("animationeditor", "upperplay");
            grpUpperFrameOpts.Text = Strings.Get("animationeditor", "upperframeoptions");
            btnUpperClone.Text = Strings.Get("animationeditor", "upperclone");

            btnSave.Text = Strings.Get("animationeditor", "save");
            btnCancel.Text = Strings.Get("animationeditor", "cancel");
        }

        public void InitEditor()
        {
            lstAnimations.Items.Clear();
            lstAnimations.Items.AddRange(Database.GetGameObjectList(GameObjectType.Animation));
        }

        private void UpdateEditor()
        {
            if (mEditorItem != null)
            {
                pnlContainer.Show();

                txtName.Text = mEditorItem.Name;
                cmbSound.SelectedIndex = cmbSound.FindString(TextUtils.NullToNone(mEditorItem.Sound));

                cmbLowerGraphic.SelectedIndex =
                    cmbLowerGraphic.FindString(TextUtils.NullToNone(mEditorItem.LowerAnimSprite));

                nudLowerHorizontalFrames.Value = mEditorItem.LowerAnimXFrames;
                nudLowerVerticalFrames.Value = mEditorItem.LowerAnimYFrames;
                nudLowerFrameCount.Value = mEditorItem.LowerAnimFrameCount;
                UpdateLowerFrames();

                nudLowerFrameDuration.Value = mEditorItem.LowerAnimFrameSpeed;
                tmrLowerAnimation.Interval = (int) nudLowerFrameDuration.Value;
                nudLowerLoopCount.Value = mEditorItem.LowerAnimLoopCount;

                cmbUpperGraphic.SelectedIndex =
                    cmbUpperGraphic.FindString(TextUtils.NullToNone(mEditorItem.UpperAnimSprite));

                nudUpperHorizontalFrames.Value = mEditorItem.UpperAnimXFrames;
                nudUpperVerticalFrames.Value = mEditorItem.UpperAnimYFrames;
                nudUpperFrameCount.Value = mEditorItem.UpperAnimFrameCount;
                UpdateUpperFrames();

                nudUpperFrameDuration.Value = mEditorItem.UpperAnimFrameSpeed;
                tmrUpperAnimation.Interval = (int) nudUpperFrameDuration.Value;
                nudUpperLoopCount.Value = mEditorItem.UpperAnimLoopCount;

                LoadLowerLight();
                DrawLowerFrame();
                LoadUpperLight();
                DrawUpperFrame();

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

        private void txtName_TextChanged(object sender, EventArgs e)
        {
            mChangingName = true;
            mEditorItem.Name = txtName.Text;
            lstAnimations.Items[Database.GameObjectListIndex(GameObjectType.Animation, mEditorItem.Index)] =
                txtName.Text;
            mChangingName = false;
        }

        private void cmbSound_SelectedIndexChanged(object sender, EventArgs e)
        {
            mEditorItem.Sound = TextUtils.SanitizeNone(cmbSound?.Text);
        }

        private void cmbLowerGraphic_SelectedIndexChanged(object sender, EventArgs e)
        {
            mEditorItem.LowerAnimSprite = TextUtils.SanitizeNone(cmbLowerGraphic?.Text);
        }

        private void cmbUpperGraphic_SelectedIndexChanged(object sender, EventArgs e)
        {
            mEditorItem.UpperAnimSprite = TextUtils.SanitizeNone(cmbUpperGraphic?.Text);
        }

        private void tmrLowerAnimation_Tick(object sender, EventArgs e)
        {
            if (mPlayLower)
            {
                mLowerFrame++;
                if (mLowerFrame >= (int) nudLowerFrameCount.Value)
                {
                    mLowerFrame = 0;
                }
            }
        }

        void UpdateLowerFrames()
        {
            LightBase[] newArray;
            scrlLowerFrame.Maximum = (int) nudLowerFrameCount.Value;
            if (mEditorItem.LowerAnimFrameCount !=
                mEditorItem.LowerLights.Length)
            {
                newArray = new LightBase[mEditorItem.LowerAnimFrameCount];
                for (int i = 0; i < newArray.Length; i++)
                {
                    if (i < mEditorItem.LowerLights.Length)
                    {
                        newArray[i] = mEditorItem.LowerLights[i];
                    }
                    else
                    {
                        newArray[i] = new LightBase(-1, -1);
                    }
                }
                mEditorItem.LowerLights = newArray;
            }
        }

        void UpdateUpperFrames()
        {
            LightBase[] newArray;
            scrlUpperFrame.Maximum = (int) nudUpperFrameCount.Value;
            if (mEditorItem.UpperAnimFrameCount !=
                mEditorItem.UpperLights.Length)
            {
                newArray = new LightBase[mEditorItem.UpperAnimFrameCount];
                for (int i = 0; i < newArray.Length; i++)
                {
                    if (i < mEditorItem.UpperLights.Length)
                    {
                        newArray[i] = mEditorItem.UpperLights[i];
                    }
                    else
                    {
                        newArray[i] = new LightBase(-1, -1);
                    }
                }
                mEditorItem.UpperLights = newArray;
            }
        }

        void DrawLowerFrame()
        {
            if (mLowerWindow == null || mEditorItem == null) return;
            if (!mPlayLower) mLowerFrame = scrlLowerFrame.Value - 1;
            GraphicsDevice graphicsDevice = EditorGraphics.GetGraphicsDevice();
            EditorGraphics.EndSpriteBatch();
            graphicsDevice.SetRenderTarget(mLowerDarkness);
            graphicsDevice.Clear(Microsoft.Xna.Framework.Color.Black);
            if (mLowerFrame < mEditorItem.LowerLights.Length)
            {
                EditorGraphics.DrawLight(
                    picLowerAnimation.Width / 2 +
                    mEditorItem.LowerLights[mLowerFrame].OffsetX,
                    picLowerAnimation.Height / 2 +
                    mEditorItem.LowerLights[mLowerFrame].OffsetY,
                    mEditorItem.LowerLights[mLowerFrame], mLowerDarkness);
            }
            EditorGraphics.DrawTexture(EditorGraphics.GetWhiteTex(), new RectangleF(0, 0, 1, 1),
                new RectangleF(0, 0, mLowerDarkness.Width, mLowerDarkness.Height),
                System.Drawing.Color.FromArgb((byte) (((float) (100 - scrlDarkness.Value) / 100f) * 255), 255, 255,
                    255), mLowerDarkness,
                BlendState.Additive);
            EditorGraphics.EndSpriteBatch();
            graphicsDevice.SetRenderTarget(mLowerWindow);
            graphicsDevice.Clear(Microsoft.Xna.Framework.Color.White);
            Texture2D animTexture = GameContentManager.GetTexture(GameContentManager.TextureType.Animation,
                cmbLowerGraphic.Text);
            if (animTexture != null)
            {
                long w = animTexture.Width / (int) nudLowerHorizontalFrames.Value;
                long h = animTexture.Height / (int) nudLowerVerticalFrames.Value;
                long x = 0;
                if (mLowerFrame > 0)
                {
                    x = (mLowerFrame % (int) nudLowerHorizontalFrames.Value) * w;
                }
                long y = (int) Math.Floor(mLowerFrame / nudLowerHorizontalFrames.Value) * h;
                EditorGraphics.DrawTexture(animTexture, new RectangleF(x, y, w, h),
                    new RectangleF(picLowerAnimation.Width / 2 - (int) w / 2,
                        (int) picLowerAnimation.Height / 2 - (int) h / 2, w, h), mLowerWindow);
            }
            EditorGraphics.DrawTexture(mLowerDarkness, 0, 0, mLowerWindow, EditorGraphics.MultiplyState);
            EditorGraphics.EndSpriteBatch();
            mLowerWindow.Present();
        }

        void DrawUpperFrame()
        {
            if (mUpperWindow == null || mEditorItem == null) return;
            if (!mPlayUpper) mUpperFrame = scrlUpperFrame.Value - 1;
            GraphicsDevice graphicsDevice = EditorGraphics.GetGraphicsDevice();
            EditorGraphics.EndSpriteBatch();
            graphicsDevice.SetRenderTarget(mUpperDarkness);
            graphicsDevice.Clear(Microsoft.Xna.Framework.Color.Black);
            if (mUpperFrame < mEditorItem.UpperLights.Length)
            {
                EditorGraphics.DrawLight(
                    picUpperAnimation.Width / 2 +
                    mEditorItem.UpperLights[mUpperFrame].OffsetX,
                    picUpperAnimation.Height / 2 +
                    mEditorItem.UpperLights[mUpperFrame].OffsetY,
                    mEditorItem.UpperLights[mUpperFrame], mUpperDarkness);
            }
            EditorGraphics.DrawTexture(EditorGraphics.GetWhiteTex(), new RectangleF(0, 0, 1, 1),
                new RectangleF(0, 0, mUpperDarkness.Width, mUpperDarkness.Height),
                System.Drawing.Color.FromArgb((byte) (((float) (100 - scrlDarkness.Value) / 100f) * 255), 255, 255,
                    255), mUpperDarkness,
                BlendState.Additive);
            EditorGraphics.EndSpriteBatch();
            graphicsDevice.SetRenderTarget(mUpperWindow);
            graphicsDevice.Clear(Microsoft.Xna.Framework.Color.White);
            Texture2D animTexture = GameContentManager.GetTexture(GameContentManager.TextureType.Animation,
                cmbUpperGraphic.Text);
            if (animTexture != null)
            {
                long w = animTexture.Width / (int) nudUpperHorizontalFrames.Value;
                long h = animTexture.Height / (int) nudUpperVerticalFrames.Value;
                long x = 0;
                if (mUpperFrame > 0)
                {
                    x = (mUpperFrame % (int) nudUpperHorizontalFrames.Value) * w;
                }
                long y = (int) Math.Floor(mUpperFrame / nudUpperHorizontalFrames.Value) * h;
                EditorGraphics.DrawTexture(animTexture, new RectangleF(x, y, w, h),
                    new RectangleF(picUpperAnimation.Width / 2 - (int) w / 2,
                        (int) picUpperAnimation.Height / 2 - (int) h / 2, w, h), mUpperWindow);
            }
            EditorGraphics.DrawTexture(mUpperDarkness, 0, 0, mUpperWindow, EditorGraphics.MultiplyState);
            EditorGraphics.EndSpriteBatch();
            mUpperWindow.Present();
        }

        private void tmrUpperAnimation_Tick(object sender, EventArgs e)
        {
            if (mPlayUpper)
            {
                mUpperFrame++;
                if (mUpperFrame >= (int) nudUpperFrameCount.Value)
                {
                    mUpperFrame = 0;
                }
            }
        }

        private void frmAnimation_FormClosed(object sender, FormClosedEventArgs e)
        {
            Globals.CurrentEditor = -1;
        }

        private void scrlLowerFrame_Scroll(object sender, ScrollValueEventArgs e)
        {
            lblLowerFrame.Text = Strings.Get("animationeditor", "lowerframe", scrlLowerFrame.Value);
            LoadLowerLight();
            DrawLowerFrame();
        }

        private void LoadLowerLight()
        {
            lightEditorLower.CanClose = false;
            lightEditorLower.LoadEditor(mEditorItem.LowerLights[scrlLowerFrame.Value - 1]);
        }

        private void LoadUpperLight()
        {
            lightEditorUpper.CanClose = false;
            lightEditorUpper.LoadEditor(mEditorItem.UpperLights[scrlUpperFrame.Value - 1]);
        }

        private void scrlUpperFrame_Scroll(object sender, ScrollValueEventArgs e)
        {
            lblUpperFrame.Text = Strings.Get("animationeditor", "upperframe", scrlUpperFrame.Value);
            LoadUpperLight();
            DrawUpperFrame();
        }

        private void btnPlayLower_Click(object sender, EventArgs e)
        {
            mPlayLower = !mPlayLower;
            if (mPlayLower)
            {
                btnPlayLower.Text = Strings.Get("animationeditor", "lowerstop");
            }
            else
            {
                btnPlayLower.Text = Strings.Get("animationeditor", "lowerplay");
            }
        }

        private void btnLowerClone_Click(object sender, EventArgs e)
        {
            if (scrlLowerFrame.Value > 1)
            {
                mEditorItem.LowerLights[scrlLowerFrame.Value - 1] =
                    new LightBase(mEditorItem.LowerLights[scrlLowerFrame.Value - 2]);
                LoadLowerLight();
                DrawLowerFrame();
            }
        }

        private void btnPlayUpper_Click(object sender, EventArgs e)
        {
            mPlayUpper = !mPlayUpper;
            if (mPlayUpper)
            {
                btnPlayUpper.Text = Strings.Get("animationeditor", "upperstop");
            }
            else
            {
                btnPlayUpper.Text = Strings.Get("animationeditor", "upperplay");
            }
        }

        private void btnUpperClone_Click(object sender, EventArgs e)
        {
            if (scrlUpperFrame.Value > 1)
            {
                mEditorItem.UpperLights[scrlUpperFrame.Value - 1] =
                    new LightBase(mEditorItem.UpperLights[scrlUpperFrame.Value - 2]);
                LoadUpperLight();
                DrawUpperFrame();
            }
        }

        private void scrlDarkness_Scroll(object sender, ScrollValueEventArgs e)
        {
            labelDarkness.Text = Strings.Get("animationeditor", "simulatedarkness", scrlDarkness.Value);
        }

        private void btnSwap_Click(object sender, EventArgs e)
        {
            string lowerAnimSprite = mEditorItem.LowerAnimSprite;
            int lowerAnimXFrames = mEditorItem.LowerAnimXFrames;
            int lowerAnimYFrames = mEditorItem.LowerAnimYFrames;
            int lowerAnimFrameCount = mEditorItem.LowerAnimFrameCount;
            int lowerAnimFrameSpeed = mEditorItem.LowerAnimFrameSpeed;
            int lowerAnimLoopCount = mEditorItem.LowerAnimLoopCount;
            LightBase[] lowerLights = mEditorItem.LowerLights;
            mEditorItem.LowerAnimSprite = mEditorItem.UpperAnimSprite;
            mEditorItem.LowerAnimXFrames = mEditorItem.UpperAnimXFrames;
            mEditorItem.LowerAnimYFrames = mEditorItem.UpperAnimYFrames;
            mEditorItem.LowerAnimFrameCount = mEditorItem.UpperAnimFrameCount;
            mEditorItem.LowerAnimFrameSpeed = mEditorItem.UpperAnimFrameSpeed;
            mEditorItem.LowerAnimLoopCount = mEditorItem.UpperAnimLoopCount;
            mEditorItem.LowerLights = mEditorItem.UpperLights;

            mEditorItem.UpperAnimSprite = lowerAnimSprite;
            mEditorItem.UpperAnimXFrames = lowerAnimXFrames;
            mEditorItem.UpperAnimYFrames = lowerAnimYFrames;
            mEditorItem.UpperAnimFrameCount = lowerAnimFrameCount;
            mEditorItem.UpperAnimFrameSpeed = lowerAnimFrameSpeed;
            mEditorItem.UpperAnimLoopCount = lowerAnimLoopCount;
            mEditorItem.UpperLights = lowerLights;

            mUpperFrame = 0;
            mLowerFrame = 0;

            UpdateEditor();
        }

        private void toolStripItemNew_Click(object sender, EventArgs e)
        {
            PacketSender.SendCreateObject(GameObjectType.Animation);
        }

        private void toolStripItemDelete_Click(object sender, EventArgs e)
        {
            if (mEditorItem != null && lstAnimations.Focused)
            {
                if (DarkMessageBox.ShowWarning(Strings.Get("animationeditor", "deleteprompt"),
                        Strings.Get("animationeditor", "deletetitle"), DarkDialogButton.YesNo,
                        Properties.Resources.Icon) ==
                    DialogResult.Yes)
                {
                    PacketSender.SendDeleteObject(mEditorItem);
                }
            }
        }

        private void toolStripItemCopy_Click(object sender, EventArgs e)
        {
            if (mEditorItem != null && lstAnimations.Focused)
            {
                mCopiedItem = mEditorItem.BinaryData;
                toolStripItemPaste.Enabled = true;
            }
        }

        private void toolStripItemPaste_Click(object sender, EventArgs e)
        {
            if (mEditorItem != null && mCopiedItem != null && lstAnimations.Focused)
            {
                mEditorItem.Load(mCopiedItem);
                UpdateEditor();
            }
        }

        private void toolStripItemUndo_Click(object sender, EventArgs e)
        {
            if (mChanged.Contains(mEditorItem) && mEditorItem != null)
            {
                if (DarkMessageBox.ShowWarning(Strings.Get("animationeditor", "undoprompt"),
                        Strings.Get("animationeditor", "undotitle"), DarkDialogButton.YesNo,
                        Properties.Resources.Icon) ==
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
            toolStripItemCopy.Enabled = mEditorItem != null && lstAnimations.Focused;
            toolStripItemPaste.Enabled = mEditorItem != null && mCopiedItem != null && lstAnimations.Focused;
            toolStripItemDelete.Enabled = mEditorItem != null && lstAnimations.Focused;
            toolStripItemUndo.Enabled = mEditorItem != null && lstAnimations.Focused;
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

        private void nudLowerHorizontalFrames_ValueChanged(object sender, EventArgs e)
        {
            mEditorItem.LowerAnimXFrames = (int) nudLowerHorizontalFrames.Value;
        }

        private void nudLowerVerticalFrames_ValueChanged(object sender, EventArgs e)
        {
            mEditorItem.LowerAnimYFrames = (int) nudLowerVerticalFrames.Value;
        }

        private void nudLowerFrameCount_ValueChanged(object sender, EventArgs e)
        {
            mEditorItem.LowerAnimFrameCount = (int) nudLowerFrameCount.Value;
            UpdateLowerFrames();
        }

        private void nudLowerFrameDuration_ValueChanged(object sender, EventArgs e)
        {
            mEditorItem.LowerAnimFrameSpeed = (int) nudLowerFrameDuration.Value;
            tmrLowerAnimation.Interval = (int) nudLowerFrameDuration.Value;
        }

        private void nudLowerLoopCount_ValueChanged(object sender, EventArgs e)
        {
            mEditorItem.LowerAnimLoopCount = (int) nudLowerLoopCount.Value;
        }

        private void nudUpperHorizontalFrames_ValueChanged(object sender, EventArgs e)
        {
            mEditorItem.UpperAnimXFrames = (int) nudUpperHorizontalFrames.Value;
        }

        private void nudUpperVerticalFrames_ValueChanged(object sender, EventArgs e)
        {
            mEditorItem.UpperAnimYFrames = (int) nudUpperVerticalFrames.Value;
        }

        private void nudUpperFrameCount_ValueChanged(object sender, EventArgs e)
        {
            mEditorItem.UpperAnimFrameCount = (int) nudUpperFrameCount.Value;
            UpdateUpperFrames();
        }

        private void nudUpperFrameDuration_ValueChanged(object sender, EventArgs e)
        {
            mEditorItem.UpperAnimFrameSpeed = (int) nudUpperFrameDuration.Value;
            tmrUpperAnimation.Interval = (int) nudUpperFrameDuration.Value;
        }

        private void nudUpperLoopCount_ValueChanged(object sender, EventArgs e)
        {
            mEditorItem.UpperAnimLoopCount = (int) nudUpperLoopCount.Value;
        }

        private void tmrRender_Tick(object sender, EventArgs e)
        {
            DrawLowerFrame();
            DrawUpperFrame();
        }
    }
}