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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.importFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.expandAllDirectoriesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.shrinkAllDirectoriesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.FileTree = new System.Windows.Forms.TreeView();
            this.fileContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.importFileContext = new System.Windows.Forms.ToolStripMenuItem();
            this.exportFileContext = new System.Windows.Forms.ToolStripMenuItem();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.menuStrip1.SuspendLayout();
            this.fileContextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.viewToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(500, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.toolStripSeparator2,
            this.importFileToolStripMenuItem,
            this.exportFileToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.openToolStripMenuItem.Text = "Open PAK";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(175, 6);
            // 
            // importFileToolStripMenuItem
            // 
            this.importFileToolStripMenuItem.Name = "importFileToolStripMenuItem";
            this.importFileToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.importFileToolStripMenuItem.Text = "Import Selected File";
            this.importFileToolStripMenuItem.Click += new System.EventHandler(this.importFileToolStripMenuItem_Click);
            // 
            // exportFileToolStripMenuItem
            // 
            this.exportFileToolStripMenuItem.Name = "exportFileToolStripMenuItem";
            this.exportFileToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.exportFileToolStripMenuItem.Text = "Export Selected File";
            this.exportFileToolStripMenuItem.Click += new System.EventHandler(this.exportFileToolStripMenuItem_Click);
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
            this.expandAllDirectoriesToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.expandAllDirectoriesToolStripMenuItem.Text = "Expand All Directories";
            this.expandAllDirectoriesToolStripMenuItem.Click += new System.EventHandler(this.expandAllDirectoriesToolStripMenuItem_Click);
            // 
            // shrinkAllDirectoriesToolStripMenuItem
            // 
            this.shrinkAllDirectoriesToolStripMenuItem.Name = "shrinkAllDirectoriesToolStripMenuItem";
            this.shrinkAllDirectoriesToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.shrinkAllDirectoriesToolStripMenuItem.Text = "Shrink All Directories";
            this.shrinkAllDirectoriesToolStripMenuItem.Click += new System.EventHandler(this.shrinkAllDirectoriesToolStripMenuItem_Click);
            // 
            // FileTree
            // 
            this.FileTree.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.FileTree.Location = new System.Drawing.Point(-1, 24);
            this.FileTree.Name = "FileTree";
            this.FileTree.Size = new System.Drawing.Size(500, 679);
            this.FileTree.TabIndex = 5;
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
            this.imageList1.Images.SetKeyName(0, "directory_icon.png");
            this.imageList1.Images.SetKeyName(1, "file_icon.png");
            this.imageList1.Images.SetKeyName(2, "text_icon.png");
            // 
            // Explorer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(500, 703);
            this.Controls.Add(this.FileTree);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.Name = "Explorer";
            this.Text = "Alien: Isolation PAK Tool";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.fileContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.TreeView FileTree;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem expandAllDirectoriesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem shrinkAllDirectoriesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem importFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportFileToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip fileContextMenu;
        private System.Windows.Forms.ToolStripMenuItem exportFileContext;
        private System.Windows.Forms.ToolStripMenuItem importFileContext;
        private System.Windows.Forms.ImageList imageList1;
    }
}

