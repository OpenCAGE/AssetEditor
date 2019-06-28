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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ExportButton = new System.Windows.Forms.Button();
            this.ImportButton = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.FileTree = new System.Windows.Forms.TreeView();
            this.ExpandTree = new System.Windows.Forms.Button();
            this.ShrinkTree = new System.Windows.Forms.Button();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(590, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // ExportButton
            // 
            this.ExportButton.Location = new System.Drawing.Point(506, 642);
            this.ExportButton.Name = "ExportButton";
            this.ExportButton.Size = new System.Drawing.Size(75, 61);
            this.ExportButton.TabIndex = 3;
            this.ExportButton.Text = "Export";
            this.toolTip1.SetToolTip(this.ExportButton, "Export the selected entry in the loaded PAK archive.");
            this.ExportButton.UseVisualStyleBackColor = true;
            this.ExportButton.Click += new System.EventHandler(this.ExportButton_Click);
            // 
            // ImportButton
            // 
            this.ImportButton.Location = new System.Drawing.Point(506, 575);
            this.ImportButton.Name = "ImportButton";
            this.ImportButton.Size = new System.Drawing.Size(75, 61);
            this.ImportButton.TabIndex = 4;
            this.ImportButton.Text = "Import";
            this.toolTip1.SetToolTip(this.ImportButton, "Import a replacement file for the selected entry in the loaded PAK archive.");
            this.ImportButton.UseVisualStyleBackColor = true;
            this.ImportButton.Click += new System.EventHandler(this.ImportButton_Click);
            // 
            // FileTree
            // 
            this.FileTree.Location = new System.Drawing.Point(12, 27);
            this.FileTree.Name = "FileTree";
            this.FileTree.Size = new System.Drawing.Size(488, 672);
            this.FileTree.TabIndex = 5;
            // 
            // ExpandTree
            // 
            this.ExpandTree.Location = new System.Drawing.Point(506, 27);
            this.ExpandTree.Name = "ExpandTree";
            this.ExpandTree.Size = new System.Drawing.Size(75, 29);
            this.ExpandTree.TabIndex = 6;
            this.ExpandTree.Text = "Expand All";
            this.toolTip1.SetToolTip(this.ExpandTree, "Import a replacement file for the selected entry in the loaded PAK archive.");
            this.ExpandTree.UseVisualStyleBackColor = true;
            this.ExpandTree.Click += new System.EventHandler(this.ExpandTree_Click);
            // 
            // ShrinkTree
            // 
            this.ShrinkTree.Location = new System.Drawing.Point(506, 62);
            this.ShrinkTree.Name = "ShrinkTree";
            this.ShrinkTree.Size = new System.Drawing.Size(75, 29);
            this.ShrinkTree.TabIndex = 7;
            this.ShrinkTree.Text = "Shrink All";
            this.toolTip1.SetToolTip(this.ShrinkTree, "Import a replacement file for the selected entry in the loaded PAK archive.");
            this.ShrinkTree.UseVisualStyleBackColor = true;
            this.ShrinkTree.Click += new System.EventHandler(this.ShrinkTree_Click);
            // 
            // Explorer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(590, 711);
            this.Controls.Add(this.ShrinkTree);
            this.Controls.Add(this.ExpandTree);
            this.Controls.Add(this.FileTree);
            this.Controls.Add(this.ImportButton);
            this.Controls.Add(this.ExportButton);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Explorer";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.Button ExportButton;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button ImportButton;
        private System.Windows.Forms.TreeView FileTree;
        private System.Windows.Forms.Button ExpandTree;
        private System.Windows.Forms.Button ShrinkTree;
    }
}

