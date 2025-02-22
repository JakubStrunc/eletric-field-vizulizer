using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UPG_SP_2024;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace ElectricFieldVis
{
    public class Probe : Form
    {
        public Vector2 pointPosition { get; set; }
        public  double angularVelocity { get; set; }
        public int turnradius { get; set; }

        public int radius = 10;

        public double fieldAtProbe { get; set; }

        private Color Color;

        public SKColor SKColor { get; set; }

        public Vector2 Position { get; set; }

        public bool dragging { get; set; }

        public bool putIntoGraph { get; set; }

        private DrawingPanel drawingPanel;

        private double startTime;

        public bool isStatic;

        

        


        public Probe(Vector2 position, DrawingPanel drawingpanel, bool isstatic = false, Color ? color = null)
        {
            pointPosition = position;
            turnradius = 100;
            angularVelocity = Math.PI / 6;
            drawingPanel = drawingpanel;
            dragging = false;
            startTime = drawingpanel.time;

            isStatic = isstatic;

            
            Color = color ?? drawingpanel.availableColors[new Random().Next(drawingpanel.availableColors.Length)];
            SKColor = new SKColor(Color.R, Color.G, Color.B, Color.A);

            putIntoGraph = false;


            //inicalizuj form
            InitializeForm();

            
        }


        public bool isHit(MouseEventArgs mouse)
        {
            float dx = mouse.X - (this.Position.X * drawingPanel.zoom - drawingPanel.panOffset.X);
            float dy = mouse.Y - (this.Position.Y * drawingPanel.zoom - drawingPanel.panOffset.Y);
            return (dx * dx + dy * dy) <= (this.radius * drawingPanel.zoom) * (this.radius * drawingPanel.zoom);
        }

        public void Draw(Graphics g)
        {
            // radius
            int probeRadius = (int)(this.radius * drawingPanel.uniformScale);

            // staticka sonda
            if (isStatic)
            {
                this.Position = new Vector2(drawingPanel.cx + this.pointPosition.X * drawingPanel.zoom, drawingPanel.cy + this.pointPosition.Y * drawingPanel.zoom);
            }
            // pohybliva sonda
            else
            {
                this.Position = new Vector2(drawingPanel.cx + this.pointPosition.X + (float)(this.turnradius * Math.Cos(this.angularVelocity * (drawingPanel.time - startTime))) * drawingPanel.uniformScale, drawingPanel.cy + this.pointPosition.Y + (float)(this.turnradius * Math.Sin(this.angularVelocity * (drawingPanel.time - startTime))) * drawingPanel.uniformScale);
                if (dragging) g.DrawEllipse(Pens.DarkGray, drawingPanel.cx + this.pointPosition.X * drawingPanel.zoom - this.turnradius * drawingPanel.uniformScale, drawingPanel.cy + this.pointPosition.Y * drawingPanel.zoom - this.turnradius * drawingPanel.uniformScale, this.turnradius * drawingPanel.uniformScale * 2, this.turnradius * drawingPanel.uniformScale * 2);

            }

            
            //vypocitej smer sipky pro probe
            Vector2 position = new Vector2(this.Position.X - drawingPanel.cx, this.Position.Y - drawingPanel.cy);
            Vector2 E = drawingPanel.ScaledCalculateElectricField(position);
            Vector2 dir = Vector2.Normalize(E) * 30 * drawingPanel.uniformScale;
            drawingPanel.arrowprobePen.Width = 4 * drawingPanel.uniformScale;
            Arrow arrow = new Arrow(this.Position, dir);
            arrow.DrawArrow(g, drawingPanel.arrowprobePen);


            //nakresli probe
            using (GraphicsPath gprobe = new GraphicsPath())
            {
                gprobe.AddEllipse(this.Position.X - probeRadius, this.Position.Y - probeRadius, 2 * probeRadius, 2 * probeRadius);
                g.DrawPath(Pens.Black, gprobe);

                using (PathGradientBrush probebrush = new PathGradientBrush(gprobe))
                {
                    probebrush.CenterPoint = new PointF(this.Position.X + this.radius + this.radius / 2, this.Position.Y + this.radius / 2);
                    probebrush.CenterColor = this.Color;
                    probebrush.InterpolationColors = new ColorBlend()
                    {
                        Colors = new Color[] { this.Color, Color.White },
                        Positions = new float[] { 0f, 1f }
                    };

                    g.FillPath(probebrush, gprobe);
                }
            }



            // vypocitej eletricke pole
            E = drawingPanel.CalculateElectricField(position);
            this.fieldAtProbe = Math.Sqrt(E.X * E.X + E.Y * E.Y);

            //vytiskni text k probe
            using (Font font = new Font("Arial", 10 * drawingPanel.uniformScale, FontStyle.Bold))
            {
                string chargeText = fieldAtProbe.ToString("0.###E+00") + " N/C";
                SizeF textSize = g.MeasureString(chargeText, font);

                float textX_charge = this.Position.X - textSize.Width / 2;
                float textY_charge = this.Position.Y + 20 - textSize.Height / 2;
                //Debug.WriteLine($"c:{drawingPanel.showColorMap}, p:{drawingPanel.showPaths})");
                Brush textBrush = drawingPanel.showColorMap ? Brushes.Black : Brushes.LightGreen;
                g.DrawString(chargeText, font, textBrush, textX_charge, textY_charge);
            }

            /*Brush brush = Brushes.White;
            string label = "Position" + (this.Position.X - drawingPanel.cx).ToString();
            g.DrawString(label, font, brush, 25, 25);*/
        }

        public void InitializeForm()
        {


            // editovaci okna
            this.ClientSize = new Size(300, 200);
            this.BackColor = System.Drawing.Color.White;

            var txtPositionX = new TextBox { Left = 100, Top = 10, Width = 100 };
            var txtPositionY = new TextBox { Left = 100, Top = 40, Width = 100 };
            var txtRadius = new TextBox { Text = (turnradius / 100).ToString(), Left = 100, Top = 70, Width = 100 };

            // lebely
            var lblPositionX = new Label { Text = "Position X:", Left = 10, Top = 10, Width = 80 };
            var lblPositionY = new Label { Text = "Position Y:", Left = 10, Top = 40, Width = 80 };
            var lblRadius = new Label { Text = "Radius:", Left = 10, Top = 70, Width = 80 };
           

            // Checkbox pro pridani do grafu
            var chkGraph = new CheckBox
            {
                Text = "Enable Graph",
                Checked = false,
                Left = 10,
                Top = 100,
                AutoSize = true
            };

            // checkbox pro static probe
            var chkStatic = new CheckBox
            {
                Text = "Static Probe",
                Checked = isStatic, 
                Left = 10,
                Top = 130,
                AutoSize = true
            };

            // ok button
            var btnOk = new Button
            {
                Text = "OK",
                Left = 10,
                Top = 170,
                Width = 80
            };

            //potrvrzeni button handler
            btnOk.Click += (sender, e) =>
            {

                // update
                if (txtPositionX.Text != "")
                {
                    pointPosition = new Vector2(float.Parse(txtPositionX.Text) * 100, pointPosition.Y);
                }

                if (txtPositionY.Text != "")
                {
                    pointPosition = new Vector2(pointPosition.X, float.Parse(txtPositionY.Text) * 100);
                }
                this.turnradius = int.Parse(txtRadius.Text) * 100;
                this.putIntoGraph = chkGraph.Checked;
                this.isStatic = chkStatic.Checked;

                

                Close();
                
            };

            // delete button
            var btnDelete = new Button
            {
                Text = "Delete",
                Left = 100,
                Top = 170,
                Width = 80
            };
            //vymazani button handler
            btnDelete.Click += (sender, e) =>
            {
                drawingPanel.probes.Remove(this);
                drawingPanel.Invalidate();
                Close();
            };

            // pridej do okna
            Controls.Add(lblPositionX);
            Controls.Add(txtPositionX);
            Controls.Add(lblPositionY);
            Controls.Add(txtPositionY);
            Controls.Add(lblRadius);
            Controls.Add(txtRadius);
            Controls.Add(chkGraph);
            Controls.Add(chkStatic);
            Controls.Add(btnOk);
            Controls.Add(btnDelete);

        }



        public void ShowEditForm()
        {
            //ukaz okno
            ShowDialog();
        }
    }
}
