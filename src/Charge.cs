using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data;
using System.Linq.Expressions;
using System.Drawing.Drawing2D;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using UPG_SP_2024;
using System.Diagnostics;


namespace ElectricFieldVis
{
    public class Charge : Form
    {
        public double Q { set; get; }
        private string func;
        private List<string> str_Q;

        public Vector2 Position { set; get; }

        public int radius { set; get; }

        private DrawingPanel drawingPanel;
        

        private TextBox txtPositionX;
        private TextBox txtPositionY;
        private TextBox txtChargeQ;

        public SplineEditor Spline;

        private bool followPath;

        private static bool warningShown = false;



        public Charge(string q, Vector2 position, DrawingPanel drawingpanel)
        {
            func = q;
            str_Q =  ShuntingYard.ConvertToPostfix(q.Split(' '));
            float pos_x = position.X;
            float pos_y = -position.Y;
            Position = new Vector2(pos_x, pos_y);

            drawingPanel = drawingpanel;

            Spline = new SplineEditor(new PointF(Position.X, Position.Y), drawingpanel);


            //priprav form
            InitializeForm();


        }

        public bool isHitByMouse(MouseEventArgs mouse)
        {
            float dx = mouse.X - (Position.X * drawingPanel.zoom * drawingPanel.uniformScale + drawingPanel.panOffset.X + drawingPanel.cx);
            float dy = mouse.Y - (Position.Y * drawingPanel.zoom * drawingPanel.uniformScale + drawingPanel.panOffset.Y + drawingPanel.cy);
            return (dx * dx + dy * dy) <= (radius * drawingPanel.zoom) * (radius * drawingPanel.zoom);
        }



        private double updateQ(double time)
        {
            if (str_Q == null || str_Q.Count == 0) return 0;
            return ShuntingYard.EvaluatePostfix(str_Q, time);
        }

        
        public void Draw(Graphics g)
        {
            this.Q = updateQ(drawingPanel.time); // update polarity
            this.radius = (int)(15 * drawingPanel.uniformScale + Math.Abs(this.Q) * 10 * drawingPanel.uniformScale); // vypocitej radius

            //Debug.WriteLine(drawingPanel.time);
            //Debug.WriteLine(Position.ToString());
            int x;
            int y;

            //pohybujici se naboj
            if (followPath && Spline.invalide)
            {
                
                

                //index snimku
                int index = (int)(drawingPanel.time * 20) % Spline.path.Count();

                // pozice na indexu
                float xSpline = Spline.path[index].X;
                float ySpline = Spline.path[index].Y;
                float xSplineAhead = Spline.path[(int)(index + (2 * (drawingPanel.time_speed / 0.1))) % Spline.path.Count()].X;
                float ySplineAhead = Spline.path[(int)(index + (2 * (drawingPanel.time_speed / 0.1))) % Spline.path.Count()].Y;

                // nastaveni pozice na pozice
                Position = new Vector2(xSplineAhead, ySplineAhead);

                //nastaveni pozice na vykreslovani
                x = drawingPanel.cx + (int)(xSpline * drawingPanel.uniformScale) - this.radius;
                y = drawingPanel.cy + (int)(ySpline * drawingPanel.uniformScale) - this.radius;
                
            }
            //staticky charge
            else
            {
                x = drawingPanel.cx + (int)(this.Position.X * drawingPanel.uniformScale) - this.radius;
                y = drawingPanel.cy + (int)(this.Position.Y * drawingPanel.uniformScale) - this.radius;
            }


            //vykresli charge
            using (GraphicsPath gcharge = new GraphicsPath())
            {
                gcharge.AddEllipse(x, y, 2 * this.radius, 2 * this.radius);
                Color color = this.Q > 0 ? Color.Red : Color.Blue;

                using (var elipseBrush = new PathGradientBrush(gcharge))
                {
                    elipseBrush.CenterPoint = new PointF(x + this.radius + this.radius/2, y + this.radius/2);
                    elipseBrush.CenterColor = color;
                    elipseBrush.InterpolationColors = new ColorBlend
                    {
                        Colors = new Color[] { color, Color.White },
                        Positions = new float[] { 0f, 1f }
                    };
                    g.FillPath(elipseBrush, gcharge);
                }

                g.DrawPath(Pens.Black, gcharge);
            }

            // text polarity charge
            string chargeText = this.Q.ToString("F1") + "C";
            using (Font font = new Font("Arial", 8 * drawingPanel.uniformScale, FontStyle.Bold))
            {
                SizeF textSize = g.MeasureString(chargeText, font);
                float textX = x + this.radius - textSize.Width / 2;
                float textY = y + this.radius - textSize.Height / 2;
                g.DrawString(chargeText, font, Brushes.Black, textX, textY);
            }

            /*
            Brush brush = Brushes.White;
            string label = "Position" + (this.Position).ToString();
            g.DrawString(label, font, brush, 25, 25);*/

            if (drawingPanel.showPaths) Spline.Draw(g);
        }

        public void InitializeForm()
        {
            this.ClientSize = new Size(250, 200);
            this.BackColor = System.Drawing.Color.White; 

            // textove pole
            txtPositionX = new TextBox { Left = 100, Top = 10, Width = 50 };
            txtPositionY = new TextBox { Left = 100, Top = 40, Width = 50 };
            txtChargeQ = new TextBox { Text = func, Left = 100, Top = 70, Width = 150 };

            // labely
            var lblPositionX = new Label { Text = "Position X:", Left = 10, Top = 10 };
            var lblPositionY = new Label { Text = "Position Y:", Left = 10, Top = 40 };
            var lblChargeQ = new Label { Text = "Charge Q:", Left = 10, Top = 70 };

            // checkbox pro sledovani trasy
            var chkPath = new CheckBox
            {
                Text = "Follow Path",
                Checked = followPath,
                Left = 10,
                Top = 100,
                AutoSize = true
            };

            // ok button
            var btnOk = new Button
            {
                Text = "OK",
                Left = 10,
                Top = 140,
                Width = 50
            };

            //delete button
            var btnDelete = new Button
            {
                Text = "Delete",
                Left = 70,
                Top = 140,
                Width = 50
            };

            // ok button handler
            btnOk.Click += (sender, e) =>
            {
                if (txtPositionX.Text != "")
                {
                    this.Position = new Vector2(float.Parse(txtPositionX.Text) * 100, this.Position.Y);
                }

                if (txtPositionY.Text != "")
                {
                    this.Position = new Vector2(this.Position.X, float.Parse(txtPositionY.Text) * -100);
                }

                this.func = txtChargeQ.Text;
                this.str_Q = ShuntingYard.ConvertToPostfix(txtChargeQ.Text.Split(' '));
                this.followPath = chkPath.Checked;

                this.DialogResult = DialogResult.OK;
                Close();
            };

            // delete handler
            btnDelete.Click += (sender, e) =>
            {
                drawingPanel.charges.Remove(this);
                drawingPanel.Invalidate();
                Close();
            };

            // Pridani do formulare
            Controls.Add(txtPositionX);
            Controls.Add(txtPositionY);
            Controls.Add(txtChargeQ);
            Controls.Add(lblPositionX);
            Controls.Add(lblPositionY);
            Controls.Add(lblChargeQ);
            Controls.Add(chkPath);
            Controls.Add(btnOk);
            Controls.Add(btnDelete);
        }



        public void ShowEditForm()
        {
            ShowDialog();
        }



    }
}

