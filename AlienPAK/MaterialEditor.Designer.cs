﻿namespace AlienPAK
{
    partial class MaterialEditor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MaterialEditor));
            this.materialList = new System.Windows.Forms.ListBox();
            this.selectMaterial = new System.Windows.Forms.Button();
            this.elementHost1 = new System.Windows.Forms.Integration.ElementHost();
            this.materialEditorControlsWPF1 = new AlienPAK.MaterialEditorControlsWPF();
            this.duplicateMaterial = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // materialList
            // 
            this.materialList.FormattingEnabled = true;
            this.materialList.Location = new System.Drawing.Point(1, 0);
            this.materialList.Name = "materialList";
            this.materialList.Size = new System.Drawing.Size(384, 680);
            this.materialList.TabIndex = 21;
            this.materialList.SelectedIndexChanged += new System.EventHandler(this.materialList_SelectedIndexChanged);
            // 
            // selectMaterial
            // 
            this.selectMaterial.Location = new System.Drawing.Point(714, 638);
            this.selectMaterial.Name = "selectMaterial";
            this.selectMaterial.Size = new System.Drawing.Size(119, 31);
            this.selectMaterial.TabIndex = 22;
            this.selectMaterial.Text = "Select Material";
            this.selectMaterial.UseVisualStyleBackColor = true;
            this.selectMaterial.Click += new System.EventHandler(this.selectMaterial_Click);
            // 
            // elementHost1
            // 
            this.elementHost1.Location = new System.Drawing.Point(391, 0);
            this.elementHost1.Name = "elementHost1";
            this.elementHost1.Size = new System.Drawing.Size(450, 676);
            this.elementHost1.TabIndex = 20;
            this.elementHost1.Text = "elementHost1";
            this.elementHost1.Child = this.materialEditorControlsWPF1;
            // 
            // duplicateMaterial
            // 
            this.duplicateMaterial.Location = new System.Drawing.Point(589, 638);
            this.duplicateMaterial.Name = "duplicateMaterial";
            this.duplicateMaterial.Size = new System.Drawing.Size(119, 31);
            this.duplicateMaterial.TabIndex = 23;
            this.duplicateMaterial.Text = "Duplicate Material";
            this.duplicateMaterial.UseVisualStyleBackColor = true;
            this.duplicateMaterial.Click += new System.EventHandler(this.duplicateMaterial_Click);
            // 
            // MaterialEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(845, 681);
            this.Controls.Add(this.duplicateMaterial);
            this.Controls.Add(this.selectMaterial);
            this.Controls.Add(this.materialList);
            this.Controls.Add(this.elementHost1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MaterialEditor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Material Editor";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Integration.ElementHost elementHost1;
        private MaterialEditorControlsWPF materialEditorControlsWPF1;
        private System.Windows.Forms.ListBox materialList;
        private System.Windows.Forms.Button selectMaterial;
        private System.Windows.Forms.Button duplicateMaterial;
    }
}