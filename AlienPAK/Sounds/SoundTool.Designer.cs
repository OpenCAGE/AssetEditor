namespace AlienPAK.Sounds
{
    partial class SoundTool
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SoundTool));
            this.soundbanksReferenced = new System.Windows.Forms.ListBox();
            this.soundbanksIncluded = new System.Windows.Forms.ListBox();
            this.soundbanksIncludedPrefetch = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.treeView = new System.Windows.Forms.TreeView();
            this.exportWEM = new System.Windows.Forms.Button();
            this.importWEM = new System.Windows.Forms.Button();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.SuspendLayout();
            // 
            // soundbanksReferenced
            // 
            this.soundbanksReferenced.FormattingEnabled = true;
            this.soundbanksReferenced.Location = new System.Drawing.Point(518, 182);
            this.soundbanksReferenced.Name = "soundbanksReferenced";
            this.soundbanksReferenced.Size = new System.Drawing.Size(480, 147);
            this.soundbanksReferenced.TabIndex = 1;
            // 
            // soundbanksIncluded
            // 
            this.soundbanksIncluded.FormattingEnabled = true;
            this.soundbanksIncluded.Location = new System.Drawing.Point(518, 362);
            this.soundbanksIncluded.Name = "soundbanksIncluded";
            this.soundbanksIncluded.Size = new System.Drawing.Size(480, 147);
            this.soundbanksIncluded.TabIndex = 2;
            // 
            // soundbanksIncludedPrefetch
            // 
            this.soundbanksIncludedPrefetch.FormattingEnabled = true;
            this.soundbanksIncludedPrefetch.Location = new System.Drawing.Point(518, 544);
            this.soundbanksIncludedPrefetch.Name = "soundbanksIncludedPrefetch";
            this.soundbanksIncludedPrefetch.Size = new System.Drawing.Size(480, 147);
            this.soundbanksIncludedPrefetch.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(515, 166);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Referenced in";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(515, 346);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Included in";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(515, 528);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(107, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Included in (prefetch)";
            // 
            // treeView
            // 
            this.treeView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.treeView.ImageIndex = 0;
            this.treeView.ImageList = this.imageList1;
            this.treeView.Location = new System.Drawing.Point(12, 12);
            this.treeView.Name = "treeView";
            this.treeView.SelectedImageIndex = 0;
            this.treeView.Size = new System.Drawing.Size(500, 679);
            this.treeView.TabIndex = 7;
            this.treeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.FileTree_AfterSelect);
            // 
            // exportWEM
            // 
            this.exportWEM.Location = new System.Drawing.Point(594, 72);
            this.exportWEM.Name = "exportWEM";
            this.exportWEM.Size = new System.Drawing.Size(180, 23);
            this.exportWEM.TabIndex = 8;
            this.exportWEM.Text = "Export";
            this.exportWEM.UseVisualStyleBackColor = true;
            this.exportWEM.Click += new System.EventHandler(this.exportWEM_Click);
            // 
            // importWEM
            // 
            this.importWEM.Location = new System.Drawing.Point(780, 72);
            this.importWEM.Name = "importWEM";
            this.importWEM.Size = new System.Drawing.Size(180, 23);
            this.importWEM.TabIndex = 9;
            this.importWEM.Text = "Import";
            this.importWEM.UseVisualStyleBackColor = true;
            this.importWEM.Click += new System.EventHandler(this.importWEM_Click);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "directory_icon.png");
            this.imageList1.Images.SetKeyName(1, "image_icon.ico");
            // 
            // SoundTool
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1009, 703);
            this.Controls.Add(this.importWEM);
            this.Controls.Add(this.exportWEM);
            this.Controls.Add(this.treeView);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.soundbanksIncludedPrefetch);
            this.Controls.Add(this.soundbanksIncluded);
            this.Controls.Add(this.soundbanksReferenced);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "SoundTool";
            this.Text = "SoundTool";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ListBox soundbanksReferenced;
        private System.Windows.Forms.ListBox soundbanksIncluded;
        private System.Windows.Forms.ListBox soundbanksIncludedPrefetch;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TreeView treeView;
        private System.Windows.Forms.Button exportWEM;
        private System.Windows.Forms.Button importWEM;
        private System.Windows.Forms.ImageList imageList1;
    }
}