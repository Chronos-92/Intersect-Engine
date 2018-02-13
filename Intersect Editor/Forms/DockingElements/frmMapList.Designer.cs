﻿using System.Windows.Forms;
using DarkUI.Controls;
using DarkUI.Renderers;
using Intersect.Editor.Forms.Controls;

namespace Intersect.Editor.Forms
{
    partial class FrmMapList
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMapList));
            this.toolStrip1 = new DarkUI.Controls.DarkToolStrip();
            this.btnChronological = new ToolStripButton();
            this.toolSelectMap = new ToolStripButton();
            this.toolStripSeparator1 = new ToolStripSeparator();
            this.btnNewMap = new ToolStripButton();
            this.btnNewFolder = new ToolStripButton();
            this.btnRename = new ToolStripButton();
            this.btnDelete = new ToolStripButton();
            this.newMapToolStripMenuItem = new ToolStripMenuItem();
            this.renameToolStripMenuItem = new ToolStripMenuItem();
            this.deleteToolStripMenuItem = new ToolStripMenuItem();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.newFolderToolStripMenuItem = new ToolStripMenuItem();
            this.mapTreeList = new MapTreeList();
            this.toolStrip1.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.AutoSize = false;
            this.toolStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.toolStrip1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStrip1.GripStyle = ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new ToolStripItem[] {
            this.btnChronological,
            this.toolSelectMap,
            this.toolStripSeparator1,
            this.btnNewMap,
            this.btnNewFolder,
            this.btnRename,
            this.btnDelete});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Padding = new System.Windows.Forms.Padding(0);
            this.toolStrip1.Size = new System.Drawing.Size(189, 25);
            this.toolStrip1.TabIndex = 4;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btnChronological
            // 
            this.btnChronological.DisplayStyle = ToolStripItemDisplayStyle.Image;
            this.btnChronological.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.btnChronological.Image = ((System.Drawing.Image)(resources.GetObject("btnChronological.Image")));
            this.btnChronological.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnChronological.Name = "btnChronological";
            this.btnChronological.Size = new System.Drawing.Size(23, 22);
            this.btnChronological.Text = "Order Chronologically";
            this.btnChronological.Click += new System.EventHandler(this.btnChronological_Click);
            // 
            // toolSelectMap
            // 
            this.toolSelectMap.DisplayStyle = ToolStripItemDisplayStyle.Image;
            this.toolSelectMap.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolSelectMap.Image = ((System.Drawing.Image)(resources.GetObject("toolSelectMap.Image")));
            this.toolSelectMap.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolSelectMap.Name = "toolSelectMap";
            this.toolSelectMap.Size = new System.Drawing.Size(23, 22);
            this.toolSelectMap.Text = "Select Current Map";
            this.toolSelectMap.Click += new System.EventHandler(this.toolSelectMap_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripSeparator1.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // btnNewMap
            // 
            this.btnNewMap.DisplayStyle = ToolStripItemDisplayStyle.Image;
            this.btnNewMap.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.btnNewMap.Image = ((System.Drawing.Image)(resources.GetObject("btnNewMap.Image")));
            this.btnNewMap.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnNewMap.Name = "btnNewMap";
            this.btnNewMap.Size = new System.Drawing.Size(23, 22);
            this.btnNewMap.Text = "New Map";
            this.btnNewMap.Click += new System.EventHandler(this.btnNewMap_Click);
            // 
            // btnNewFolder
            // 
            this.btnNewFolder.DisplayStyle = ToolStripItemDisplayStyle.Image;
            this.btnNewFolder.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.btnNewFolder.Image = ((System.Drawing.Image)(resources.GetObject("btnNewFolder.Image")));
            this.btnNewFolder.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnNewFolder.Name = "btnNewFolder";
            this.btnNewFolder.Size = new System.Drawing.Size(23, 22);
            this.btnNewFolder.Text = "New Folder";
            this.btnNewFolder.Click += new System.EventHandler(this.btnNewFolder_Click);
            // 
            // btnRename
            // 
            this.btnRename.DisplayStyle = ToolStripItemDisplayStyle.Image;
            this.btnRename.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.btnRename.Image = ((System.Drawing.Image)(resources.GetObject("btnRename.Image")));
            this.btnRename.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRename.Name = "btnRename";
            this.btnRename.Size = new System.Drawing.Size(23, 22);
            this.btnRename.Text = "Rename";
            this.btnRename.Click += new System.EventHandler(this.btnRename_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.DisplayStyle = ToolStripItemDisplayStyle.Image;
            this.btnDelete.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.btnDelete.Image = ((System.Drawing.Image)(resources.GetObject("btnDelete.Image")));
            this.btnDelete.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(23, 22);
            this.btnDelete.Text = "Delete Map or Folder";
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // newMapToolStripMenuItem
            // 
            this.newMapToolStripMenuItem.ForeColor = System.Drawing.Color.Gainsboro;
            this.newMapToolStripMenuItem.Name = "newMapToolStripMenuItem";
            this.newMapToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.newMapToolStripMenuItem.Text = "New Map";
            this.newMapToolStripMenuItem.Click += new System.EventHandler(this.btnNewMap_Click);
            // 
            // renameToolStripMenuItem
            // 
            this.renameToolStripMenuItem.ForeColor = System.Drawing.Color.Gainsboro;
            this.renameToolStripMenuItem.Name = "renameToolStripMenuItem";
            this.renameToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.renameToolStripMenuItem.Text = "Rename";
            this.renameToolStripMenuItem.Click += new System.EventHandler(this.btnRename_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.ForeColor = System.Drawing.Color.Gainsboro;
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.contextMenuStrip1.Items.AddRange(new ToolStripItem[] {
            this.newMapToolStripMenuItem,
            this.newFolderToolStripMenuItem,
            this.renameToolStripMenuItem,
            this.deleteToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.RenderMode = ToolStripRenderMode.System;
            this.contextMenuStrip1.Size = new System.Drawing.Size(153, 114);
            // 
            // newFolderToolStripMenuItem
            // 
            this.newFolderToolStripMenuItem.ForeColor = System.Drawing.Color.Gainsboro;
            this.newFolderToolStripMenuItem.Name = "newFolderToolStripMenuItem";
            this.newFolderToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.newFolderToolStripMenuItem.Text = "New Folder";
            this.newFolderToolStripMenuItem.Click += new System.EventHandler(this.btnNewFolder_Click);
            // 
            // mapTreeList
            // 
            this.mapTreeList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mapTreeList.Location = new System.Drawing.Point(0, 25);
            this.mapTreeList.Name = "mapTreeList";
            this.mapTreeList.Size = new System.Drawing.Size(189, 182);
            this.mapTreeList.TabIndex = 5;
            // 
            // frmMapList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(189, 207);
            this.CloseButton = false;
            this.CloseButtonVisible = false;
            this.Controls.Add(this.mapTreeList);
            this.Controls.Add(this.toolStrip1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FrmMapList";
            this.Text = "Map List";
            this.Load += new System.EventHandler(this.frmMapList_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private DarkToolStrip toolStrip1;
        private ToolStripButton btnNewFolder;
        private ToolStripButton btnRename;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripButton btnChronological;
        private ToolStripButton btnDelete;
        private ToolStripButton btnNewMap;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem deleteToolStripMenuItem;
        private ToolStripMenuItem renameToolStripMenuItem;
        private ToolStripMenuItem newMapToolStripMenuItem;
        public Controls.MapTreeList mapTreeList;
        private ToolStripButton toolSelectMap;
        private ToolStripMenuItem newFolderToolStripMenuItem;
    }
}