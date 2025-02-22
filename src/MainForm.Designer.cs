using System.Drawing;
using System.Windows.Forms;
using static System.Formats.Asn1.AsnWriter;

namespace UPG_SP_2024
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent(int scene, int gridStepX, int gridStepY)
        //private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            drawingPanel = new DrawingPanel(scene, gridStepX, gridStepY);
            SuspendLayout();
            // 
            // drawingPanel
            // 
            drawingPanel.Dock = DockStyle.Fill;
            drawingPanel.Location = new Point(0, 0);
            drawingPanel.Name = "drawingPanel";
            drawingPanel.Size = new Size(896, 748);
            drawingPanel.TabIndex = 0;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveCaptionText;
            ClientSize = new Size(896, 748);
            Controls.Add(drawingPanel);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(3, 4, 3, 4);
            MinimumSize = new Size(800, 600);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "A23B0042P - Semestrální práce KIV/UPG 2024/2025";
            ResumeLayout(false);
        }

        #endregion

        private DrawingPanel drawingPanel;
    }
}
