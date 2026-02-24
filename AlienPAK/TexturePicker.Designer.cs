namespace AlienPAK
{
    partial class TexturePicker
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
            this.fileTree = new System.Windows.Forms.TreeView();
            this.previewImage = new System.Windows.Forms.PictureBox();
            this.selectedNameLabel = new System.Windows.Forms.Label();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.textureSearchTextBox = new System.Windows.Forms.TextBox();
            this.textureSearchButton = new System.Windows.Forms.Button();
            this.textureSearchClearButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.previewImage)).BeginInit();
            this.SuspendLayout();
            // 
            // fileTree
            // 
            this.fileTree.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.fileTree.Location = new System.Drawing.Point(12, 38);
            this.fileTree.Name = "fileTree";
            this.fileTree.Size = new System.Drawing.Size(280, 380);
            this.fileTree.TabIndex = 0;
            this.fileTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.fileTree_AfterSelect);
            this.fileTree.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.fileTree_NodeMouseDoubleClick);
            // 
            // previewImage
            // 
            this.previewImage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.previewImage.BackColor = System.Drawing.SystemColors.ControlDark;
            this.previewImage.Location = new System.Drawing.Point(298, 61);
            this.previewImage.Name = "previewImage";
            this.previewImage.Size = new System.Drawing.Size(330, 357);
            this.previewImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.previewImage.TabIndex = 1;
            this.previewImage.TabStop = false;
            // 
            // selectedNameLabel
            // 
            this.selectedNameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.selectedNameLabel.AutoEllipsis = true;
            this.selectedNameLabel.Location = new System.Drawing.Point(295, 9);
            this.selectedNameLabel.Name = "selectedNameLabel";
            this.selectedNameLabel.Size = new System.Drawing.Size(333, 20);
            this.selectedNameLabel.TabIndex = 2;
            this.selectedNameLabel.Text = "No texture selected";
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.Location = new System.Drawing.Point(472, 424);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 3;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.Location = new System.Drawing.Point(553, 424);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 4;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // textureSearchTextBox
            // 
            this.textureSearchTextBox.Location = new System.Drawing.Point(12, 12);
            this.textureSearchTextBox.Name = "textureSearchTextBox";
            this.textureSearchTextBox.Size = new System.Drawing.Size(181, 20);
            this.textureSearchTextBox.TabIndex = 5;
            this.textureSearchTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textureSearchTextBox_KeyDown);
            // 
            // textureSearchButton
            // 
            this.textureSearchButton.Location = new System.Drawing.Point(199, 10);
            this.textureSearchButton.Name = "textureSearchButton";
            this.textureSearchButton.Size = new System.Drawing.Size(46, 23);
            this.textureSearchButton.TabIndex = 6;
            this.textureSearchButton.Text = "Search";
            this.textureSearchButton.UseVisualStyleBackColor = true;
            this.textureSearchButton.Click += new System.EventHandler(this.textureSearchButton_Click);
            // 
            // textureSearchClearButton
            // 
            this.textureSearchClearButton.Location = new System.Drawing.Point(251, 10);
            this.textureSearchClearButton.Name = "textureSearchClearButton";
            this.textureSearchClearButton.Size = new System.Drawing.Size(41, 23);
            this.textureSearchClearButton.TabIndex = 7;
            this.textureSearchClearButton.Text = "Clear";
            this.textureSearchClearButton.UseVisualStyleBackColor = true;
            this.textureSearchClearButton.Click += new System.EventHandler(this.textureSearchClearButton_Click);
            // 
            // TexturePicker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(640, 459);
            this.Controls.Add(this.textureSearchClearButton);
            this.Controls.Add(this.textureSearchButton);
            this.Controls.Add(this.textureSearchTextBox);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.selectedNameLabel);
            this.Controls.Add(this.previewImage);
            this.Controls.Add(this.fileTree);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TexturePicker";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Texture Picker";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.TexturePicker_FormClosed);
            ((System.ComponentModel.ISupportInitialize)(this.previewImage)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView fileTree;
        private System.Windows.Forms.PictureBox previewImage;
        private System.Windows.Forms.Label selectedNameLabel;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.TextBox textureSearchTextBox;
        private System.Windows.Forms.Button textureSearchButton;
        private System.Windows.Forms.Button textureSearchClearButton;
    }
}

