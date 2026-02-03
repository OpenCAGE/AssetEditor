namespace AlienPAK
{
    partial class Explorer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Explorer));
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.FileTree = new System.Windows.Forms.TreeView();
            this.fileContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.importFileContext = new System.Windows.Forms.ToolStripMenuItem();
            this.exportFileContext = new System.Windows.Forms.ToolStripMenuItem();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.saveChangesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.expandAllDirectoriesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.shrinkAllDirectoriesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.elementHost1 = new System.Windows.Forms.Integration.ElementHost();
            this.explorerControlsWPF1 = new AlienPAK.ExplorerControlsWPF();
            this.fileContextMenu.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // FileTree
            // 
            this.FileTree.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.FileTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FileTree.HideSelection = false;
            this.FileTree.Location = new System.Drawing.Point(0, 0);
            this.FileTree.Name = "FileTree";
            this.FileTree.Size = new System.Drawing.Size(486, 679);
            this.FileTree.TabIndex = 5;
            this.FileTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.FileTree_AfterSelect);
            // 
            // fileContextMenu
            // 
            this.fileContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.importFileContext,
            this.exportFileContext});
            this.fileContextMenu.Name = "fileContextMenu";
            this.fileContextMenu.Size = new System.Drawing.Size(132, 48);
            // 
            // importFileContext
            // 
            this.importFileContext.Name = "importFileContext";
            this.importFileContext.Size = new System.Drawing.Size(131, 22);
            this.importFileContext.Text = "Import File";
            this.importFileContext.Click += new System.EventHandler(this.importFileContext_Click);
            // 
            // exportFileContext
            // 
            this.exportFileContext.Name = "exportFileContext";
            this.exportFileContext.Size = new System.Drawing.Size(131, 22);
            this.exportFileContext.Text = "Export File";
            this.exportFileContext.Click += new System.EventHandler(this.exportFileContext_Click);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "Folder Icon.png");
            this.imageList1.Images.SetKeyName(1, "file_icon.png");
            this.imageList1.Images.SetKeyName(2, "FolderOpened Icon.png");
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.importFileToolStripMenuItem,
            this.exportFileToolStripMenuItem,
            this.exportAllToolStripMenuItem,
            this.toolStripSeparator1,
            this.saveChangesToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // importFileToolStripMenuItem
            // 
            this.importFileToolStripMenuItem.Name = "importFileToolStripMenuItem";
            this.importFileToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.importFileToolStripMenuItem.Text = "Import Selected File";
            this.importFileToolStripMenuItem.Click += new System.EventHandler(this.importFileToolStripMenuItem_Click);
            // 
            // exportFileToolStripMenuItem
            // 
            this.exportFileToolStripMenuItem.Name = "exportFileToolStripMenuItem";
            this.exportFileToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.exportFileToolStripMenuItem.Text = "Export Selected File";
            this.exportFileToolStripMenuItem.Click += new System.EventHandler(this.exportFileToolStripMenuItem_Click);
            // 
            // exportAllToolStripMenuItem
            // 
            this.exportAllToolStripMenuItem.Name = "exportAllToolStripMenuItem";
            this.exportAllToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.exportAllToolStripMenuItem.Text = "Export All";
            this.exportAllToolStripMenuItem.Click += new System.EventHandler(this.exportAllToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(184, 6);
            // 
            // saveChangesToolStripMenuItem
            // 
            this.saveChangesToolStripMenuItem.Name = "saveChangesToolStripMenuItem";
            this.saveChangesToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveChangesToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.saveChangesToolStripMenuItem.Text = "Save Changes";
            this.saveChangesToolStripMenuItem.ToolTipText = "Ctrl+S";
            this.saveChangesToolStripMenuItem.Click += new System.EventHandler(this.saveChangesToolStripMenuItem_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.expandAllDirectoriesToolStripMenuItem,
            this.shrinkAllDirectoriesToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.viewToolStripMenuItem.Text = "View";
            // 
            // expandAllDirectoriesToolStripMenuItem
            // 
            this.expandAllDirectoriesToolStripMenuItem.Name = "expandAllDirectoriesToolStripMenuItem";
            this.expandAllDirectoriesToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.expandAllDirectoriesToolStripMenuItem.Text = "Expand All Directories";
            this.expandAllDirectoriesToolStripMenuItem.Click += new System.EventHandler(this.expandAllDirectoriesToolStripMenuItem_Click);
            // 
            // shrinkAllDirectoriesToolStripMenuItem
            // 
            this.shrinkAllDirectoriesToolStripMenuItem.Name = "shrinkAllDirectoriesToolStripMenuItem";
            this.shrinkAllDirectoriesToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.shrinkAllDirectoriesToolStripMenuItem.Text = "Shrink All Directories";
            this.shrinkAllDirectoriesToolStripMenuItem.Click += new System.EventHandler(this.shrinkAllDirectoriesToolStripMenuItem_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.viewToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(785, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 24);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.FileTree);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.elementHost1);
            this.splitContainer1.Size = new System.Drawing.Size(785, 679);
            this.splitContainer1.SplitterDistance = 486;
            this.splitContainer1.TabIndex = 18;
            // 
            // elementHost1
            // 
            this.elementHost1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.elementHost1.Location = new System.Drawing.Point(0, 0);
            this.elementHost1.Name = "elementHost1";
            this.elementHost1.Size = new System.Drawing.Size(295, 679);
            this.elementHost1.TabIndex = 17;
            this.elementHost1.Text = "elementHost1";
            this.elementHost1.Child = this.explorerControlsWPF1;
            // 
            // Explorer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(785, 703);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.Name = "Explorer";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "OpenCAGE Asset Editor";
            this.Load += new System.EventHandler(this.Explorer_Load);
            this.fileContextMenu.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TreeView FileTree;
        private System.Windows.Forms.ContextMenuStrip fileContextMenu;
        private System.Windows.Forms.ToolStripMenuItem exportFileContext;
        private System.Windows.Forms.ToolStripMenuItem importFileContext;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.Integration.ElementHost elementHost1;
        private ExplorerControlsWPF explorerControlsWPF1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem expandAllDirectoriesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem shrinkAllDirectoriesToolStripMenuItem;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem exportAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveChangesToolStripMenuItem;
    }
}

