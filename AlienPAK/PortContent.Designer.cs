namespace AlienPAK
{
    partial class PortContent
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PortContent));
            this.overwrite = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.levelList = new System.Windows.Forms.ComboBox();
            this.export = new System.Windows.Forms.Button();
            this.copyAll = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // overwrite
            // 
            this.overwrite.AutoSize = true;
            this.overwrite.Checked = true;
            this.overwrite.CheckState = System.Windows.Forms.CheckState.Checked;
            this.overwrite.Location = new System.Drawing.Point(15, 54);
            this.overwrite.Name = "overwrite";
            this.overwrite.Size = new System.Drawing.Size(253, 17);
            this.overwrite.TabIndex = 13;
            this.overwrite.Text = "Overwrite destination content by the same name";
            this.overwrite.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(128, 13);
            this.label1.TabIndex = 12;
            this.label1.Text = "Export composite to level:";
            // 
            // levelList
            // 
            this.levelList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.levelList.FormattingEnabled = true;
            this.levelList.Location = new System.Drawing.Point(15, 25);
            this.levelList.Name = "levelList";
            this.levelList.Size = new System.Drawing.Size(422, 21);
            this.levelList.TabIndex = 11;
            // 
            // export
            // 
            this.export.Location = new System.Drawing.Point(324, 71);
            this.export.Name = "export";
            this.export.Size = new System.Drawing.Size(113, 23);
            this.export.TabIndex = 10;
            this.export.Text = "Export";
            this.export.UseVisualStyleBackColor = true;
            this.export.Click += new System.EventHandler(this.export_Click);
            // 
            // copyAll
            // 
            this.copyAll.AutoSize = true;
            this.copyAll.Checked = true;
            this.copyAll.CheckState = System.Windows.Forms.CheckState.Checked;
            this.copyAll.Location = new System.Drawing.Point(15, 77);
            this.copyAll.Name = "copyAll";
            this.copyAll.Size = new System.Drawing.Size(222, 17);
            this.copyAll.TabIndex = 14;
            this.copyAll.Text = "Copy all associated textures and materials";
            this.copyAll.UseVisualStyleBackColor = true;
            // 
            // PortContent
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(449, 103);
            this.Controls.Add(this.copyAll);
            this.Controls.Add(this.overwrite);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.levelList);
            this.Controls.Add(this.export);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "PortContent";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PortContent";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.CheckBox overwrite;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox levelList;
        private System.Windows.Forms.Button export;
        private System.Windows.Forms.CheckBox copyAll;
    }
}