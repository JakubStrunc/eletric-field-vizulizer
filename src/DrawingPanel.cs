using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Numerics;
using System.Windows.Forms;
using static UPG_SP_2024.DrawingPanel;
using ElectricFieldVis;
using System.Runtime.Intrinsics.Arm;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using System.Runtime.InteropServices;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView.Painting.ImageFilters;

namespace UPG_SP_2024
{
    /// <summary>
    /// The main panel with the custom visualization
    /// </summary>
    public class DrawingPanel : Panel
    {

        //constanty ke colombova vypoctu
        private const double epsilon0 = 8.854e-12;
        private const double k = 1 / (4 * Math.PI * epsilon0);


        //constanty panelu
        public int cx;
        public int cy;
        private const int gridStep = 50;
        private int gridStepX;
        private int gridStepY;
        public int sideOff = 50;
        public float uniformScale;

        int minX;
        int maxX;
        int minY;
        int maxY;

        //objekty
        //private List<Arrow> arrows;
        public List<Charge> charges = new List<Charge>();
        public List<Probe> probes = new List<Probe>();
        public List<Probe> probesgraph = new List<Probe>();
        public List<Probe> newperobegraph = new List<Probe>();

        //timer
        private System.Windows.Forms.Timer timer;
        public double time = 0;
        public double time_speed { set; get; }
        private int scenario;

        // zoom
        public float zoom = 1.0f;
        public PointF panOffset = new PointF(0, 0);
        private Point panStartPoint;


        //barvicky
        Pen darkGridPen = new Pen(Color.FromArgb(50, 50, 50));
        public readonly Pen arrowPen = new Pen(Color.Gray, 4);
        public readonly Pen arrowprobePen = new Pen(Color.LightGray, 5);
        public readonly Font menuFont = new Font("Arial", 12, FontStyle.Regular);
        Pen contourPen = new Pen(Color.Black, 1);


        //intensity
        float[,] fieldIntensities;
        List<float> intensityList;


        //random vygenerovany list barev k sondam
        public Color[] availableColors =
        {
            Color.Purple,
            Color.Gray,
            Color.Pink,
            Color.LightBlue,
            Color.YellowGreen,
            Color.Orange,
            Color.Cyan,
            Color.Magenta,
            Color.Lime,
            Color.Gold,
            Color.Crimson,
            Color.Teal,
            Color.Violet,
            Color.Salmon,
            Color.Khaki,
            Color.Coral,
            Color.Turquoise,
            Color.Indigo,
            Color.SkyBlue,
            Color.SpringGreen
        };

        //mody na ukazovani
        public bool showFieldLines { set; get; }
        public bool showColorMap { set; get; }
        public bool showArrows { set; get; }
        public bool showGrid { set; get; }
        public bool showLevelLines { set; get; }
        public bool showPaths { set; get; }






        // posouvani
        private Charge selectedCharge = null;
        private Probe selectedProbe = null;
        private Vector2 lastMousePosition;

        public float dx;
        public float dy;


        //side menu
        public sideMenu SideMenu;

        //legenda
        int legendWidth = 300;
        int legendHeight = 20;
        int legendX = 40;
        int legendY;


        public DrawingPanel(int scene, int gridStepX, int gridStepY)
        {
            this.ClientSize = new System.Drawing.Size(800, 600);

            // prirad hodnoty z argumentu
            scenario = scene;
            this.gridStepX = gridStepX;
            this.gridStepY = gridStepY;

            // nastv scenar
            SetChargesForScenario(scene);


            DoubleBuffered = true;

            // akce mysi
            //this.MouseWheel += DrawingPanel_MouseWheel;
            this.MouseDown += DrawingPanel_MouseDown;
            this.MouseMove += DrawingPanel_MouseMove;
            this.MouseUp += DrawingPanel_MouseUp;

            this.MouseDoubleClick += DrawingPanel_MouseDoubleClick;


            // timer inicializace
            this.time_speed = 0.1;
            timer = new System.Windows.Forms.Timer();
            timer.Interval = 100;
            timer.Tick += Timer_Tick;
            timer.Start();

            //mody inicializace
            this.showColorMap = true;
            this.showArrows = true;
            this.showGrid = false;
            this.showFieldLines = false;
            this.showLevelLines = false;
            this.showPaths = true;



            //side menu
            SideMenu = new sideMenu(this);



            //akce na resize
            this.Resize += DrawingPanel_Resize;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);



            // Nastavit panel tak, aby vyplnil celý prostor rodiče
            this.Dock = DockStyle.Fill;

            Graphics g = e.Graphics;

            g.SmoothingMode = SmoothingMode.AntiAlias;

            // zoom and pan
            g.TranslateTransform(panOffset.X, panOffset.Y);
            g.ScaleTransform(zoom, zoom);

            //vypocet stredu okna - velice dulezite
            cx = this.Width / 2;
            cy = this.Height / 2;

            // scale faktor
            uniformScale = Math.Min(this.Width / 800f, this.Height / 600f) * zoom;


            if (!(showPaths))
            {
                //nakresli barevnou mapu
                if (showColorMap || showLevelLines) DrawColorMap(g, cx, cy, uniformScale);



                // nakresli grid
                if (showGrid) DrawGrid(g, cx, cy, uniformScale);

                // nakresli sipky
                if (showArrows) GenerateArrows(g, cx, cy, uniformScale);

                //nakresli silocary
                if (showFieldLines) DrawFieldLines(g);

            }

            //nakresli grid k modu path
            if (showPaths) DrawGrid(g, cx, cy, uniformScale);

            // nakresli charge
            foreach (var charge in charges)
            {
                charge.Draw(g);
            }

            // nakresli proby
            if (!showPaths)
            {
                foreach (var probe in probes)
                {
                    probe.Draw(g);
                }
            }

            //legenda
            if (!(showPaths))
            {
                if (showColorMap)
                {
                    
                    int step = (int)(10 * uniformScale);
                    int widthSteps = this.Width / step;
                    int heightSteps = this.Height / step;

                    // Inicializace seznamu pro intenzity
                    fieldIntensities = new float[widthSteps + 1, heightSteps + 1];
                    List<float> intensityList = new List<float>();

                    for (int xStep = 0; xStep <= widthSteps; xStep++)
                    {
                        int x = xStep * step;
                        for (int yStep = 0; yStep <= heightSteps; yStep++)
                        {
                            int y = yStep * step;

                            // spcitejte intezitu na eletricfield
                            Vector2 position = new Vector2((x - cx) / uniformScale, (y - cy) / uniformScale);
                            Vector2 field = CalculateElectricField(position);
                            float intensity = field.Length();

                            // ulozte intenzitu
                            fieldIntensities[xStep, yStep] = intensity;
                            intensityList.Add(intensity);
                        }
                    }

                    // min a max hodnota
                    intensityList.Sort();
                    float minIntensity = intensityList[(int)(0.05 * intensityList.Count)];
                    float maxIntensity = intensityList[(int)(0.95 * intensityList.Count)];

                    // vykresli legendu
                    DrawLegend(g, minIntensity, maxIntensity);
                }
            }

            /*
            Pen hitboxPen = new Pen(Color.FromArgb(128, Color.Yellow), 2); 
            Brush hitboxBrush = new SolidBrush(Color.FromArgb(50, Color.Yellow)); 
            
            // Dlouhe debugovani hitboxu
            foreach (var charge in charges)
            {
                
                float screenX = charge.Position.X * zoom * uniformScale + panOffset.X + cx ;
                float screenY = charge.Position.Y * zoom * uniformScale + panOffset.Y + cy ;

                

                // Draw the hitbox as a transparent circle around the charge
                float scaledRadius = charge.radius * zoom;
                g.FillEllipse(hitboxBrush, screenX - scaledRadius, screenY - scaledRadius, scaledRadius * 2, scaledRadius * 2);
                g.DrawEllipse(hitboxPen, screenX - scaledRadius, screenY - scaledRadius, scaledRadius * 2, scaledRadius * 2);
            }*/

            // Dlouhe debugovani hitboxu


            /*foreach (var probe in probes)
            {
                
                float screenX = probe.Position.X * zoom - panOffset.X;
                float screenY = probe.Position.Y * zoom - panOffset.Y;

               
                float scaledRadius = probe.radius * zoom;

                // Draw the hitbox as a transparent circle around the probe
                g.FillEllipse(hitboxBrush, screenX - scaledRadius, screenY - scaledRadius, scaledRadius * 2, scaledRadius * 2);
                g.DrawEllipse(hitboxPen, screenX - scaledRadius, screenY - scaledRadius, scaledRadius * 2, scaledRadius * 2);
            }*/

            /*Font font = new Font("Arial", 10 * uniformScale, FontStyle.Bold);
            Brush brush = Brushes.White;
            string label = "Scene: " + (showColorMap).ToString();
            g.DrawString(label, font, brush, 25, 25);*/

            //g.DrawLine(Pens.Pink, cx, 0, cx, this.Height);
            //g.DrawLine(Pens.Pink, 0, cy ,this.Width,cy);



        }

        public void SetChargesForScenario(int scene)
        {
            setProbe(); // priprav probe

            //nastav zpravny scenar
            switch (scene)
            {
                case 0:
                    charges.Clear();
                    charges.Add(new Charge("1.0", new Vector2(0, 0), this));
                    break;
                case 1:
                    charges.Clear();
                    charges.Add(new Charge("1.0", new Vector2(-100, 0), this));
                    charges.Add(new Charge("1.0", new Vector2(100, 0), this));
                    break;
                case 2:
                    charges.Clear();
                    charges.Add(new Charge("-1.0", new Vector2(-100, 0), this));
                    charges.Add(new Charge("2.0", new Vector2(100, 0), this));
                    break;
                case 3:
                    charges.Clear();
                    charges.Add(new Charge("1.0", new Vector2(-100, -100), this));
                    charges.Add(new Charge("2.0", new Vector2(100, -100), this));
                    charges.Add(new Charge("-3.0", new Vector2(100, 100), this));
                    charges.Add(new Charge("-4.0", new Vector2(-100, 100), this));
                    break;

                case 4:
                    charges.Clear();
                    charges.Add(new Charge("1 + 0.5 * sin ( pi / 2 * t )", new Vector2(-100, 0), this));
                    charges.Add(new Charge("1 - 0.5 * sin ( pi / 2 * t )", new Vector2(100, 0), this));
                    break;

                default:
                    charges.Clear();
                    charges.Add(new Charge("1.0", new Vector2(0, 0), this)); // +1C at (0,0)
                    break;
            }
        }

        private void setProbe()
        {
            probes.Clear();
            probes.Add(new Probe(new Vector2(0, 0), this, false, Color.Gray));
        }

        private void DrawGrid(Graphics g, int cx, int cy, float uniformScale)
        {
            // vypocitej meze (jeste predtim nez jsem prestal delat zoom)
            int scaledGridStep = (int)(gridStep * uniformScale);

            maxX = (int)((this.Width - this.panOffset.X + sideOff) / zoom);
            minX = (int)((0 - this.panOffset.X - sideOff) / zoom);
            maxY = (int)((this.Height - this.panOffset.Y + sideOff) / zoom);
            minY = (int)((0 - this.panOffset.Y - sideOff) / zoom);

            // nakresli verticalni liny ze stredu
            for (int x = (int)(cx); x >= minX; x -= scaledGridStep)
            {
                g.DrawLine(darkGridPen, x, minY, x, maxY); // zleva do stredu
            }
            for (int x = cx + scaledGridStep; x <= maxX; x += scaledGridStep)
            {
                g.DrawLine(darkGridPen, x, minY, x, maxY); // zprava do stredu
            }

            // nakresli horizontalni liny
            for (int y = cy; y >= minY; y -= scaledGridStep)
            {
                g.DrawLine(darkGridPen, minX, y, maxX, y); // nad cetrem
            }
            for (int y = cy + scaledGridStep; y <= maxY; y += scaledGridStep)
            {
                g.DrawLine(darkGridPen, minX, y, maxX, y); // pod centrem
            }

        }
        private void GenerateArrows(Graphics g, int cx, int cy, float uniformScale)
        {
            // grid na scalu
            int scaledGridStepX = (int)(gridStepX * uniformScale);
            int scaledGridStepY = (int)(gridStepY * uniformScale);

            // centry kazdeho gritu
            float halfStepX = scaledGridStepX / 2f;
            float halfStepY = scaledGridStepY / 2f;

            // vypocitej meze (jeste predtim nez jsem prestal delat zoom)
            maxX = (int)((this.Width - this.panOffset.X + sideOff) / zoom);
            minX = (int)((0 - this.panOffset.X - sideOff) / zoom);
            maxY = (int)((this.Height - this.panOffset.Y + sideOff) / zoom);
            minY = (int)((0 - this.panOffset.Y - sideOff) / zoom);

            // proste nakresli sipky po okne
            for (int x = cx; x <= maxX; x += scaledGridStepX)
            {
                for (int y = cy; y <= maxY; y += scaledGridStepY)
                {
                    DrawArrowAtGridPoint(g, cx, cy, x, y, halfStepX, halfStepY, uniformScale);
                }
                for (int y = cy - scaledGridStepY; y >= minY; y -= scaledGridStepY)
                {
                    DrawArrowAtGridPoint(g, cx, cy, x, y, halfStepX, halfStepY, uniformScale);
                }
            }
            for (int x = cx - scaledGridStepX; x >= minX; x -= scaledGridStepX)
            {
                for (int y = cy; y <= maxY; y += scaledGridStepY)
                {
                    DrawArrowAtGridPoint(g, cx, cy, x, y, halfStepX, halfStepY, uniformScale);
                }
                for (int y = cy - scaledGridStepY; y >= minY; y -= scaledGridStepY)
                {
                    DrawArrowAtGridPoint(g, cx, cy, x, y, halfStepX, halfStepY, uniformScale);
                }
            }
        }

        private void DrawArrowAtGridPoint(Graphics g, int cx, int cy, int x, int y, float halfStepX, float halfStepY, float uniformScale)
        {
            // dej to do stredu
            Vector2 position = new Vector2(x + halfStepX - cx, y + halfStepY - cy);
            Vector2 E = ScaledCalculateElectricField(position);

            if (E.Length() > 0)
            {
                // velikost sipek
                float arrowScale = Math.Min(halfStepX / 25f, halfStepY / 25f);
                //Debug.WriteLine(arrowScale);

                //smer
                Vector2 dir = Vector2.Normalize(E) * 25 * arrowScale * uniformScale;

                // nakresli sipku
                Vector2 start = new Vector2(x + halfStepX - dir.X / 2, y + halfStepY - dir.Y / 2);
                Arrow arrow = new Arrow(start, dir);
                arrowPen.Width = 4 * arrowScale * uniformScale;
                arrow.DrawArrow(g, arrowPen);
            }
        }

        public Vector2 CalculateElectricField(Vector2 position)
        {
            Vector2 E_total = new Vector2(0, 0);
            float threshold = 1e-6f;
            foreach (var charge in charges)
            {

                //Debug.WriteLine(position);
                Vector2 r = position / uniformScale - charge.Position;

                double r_mag = Math.Sqrt(r.X * r.X + r.Y * r.Y);

                if (r_mag > threshold)
                {
                    // normalizace smeru
                    Vector2 r_normalized = new Vector2((float)(r.X / r_mag), (float)(r.Y / r_mag));

                    // Coulombuv zakon
                    float fieldMagnitude = (float)(k * charge.Q) / (float)(r_mag * r_mag * r_mag);

                    Vector2 E = fieldMagnitude * r_normalized;
                    E_total += E;
                }
            }

            return E_total * 1000000; //fun bug ktery jsem objevil protoze je ty pozice naboju mam po stovkach
        }
        public Vector2 ScaledCalculateElectricField(Vector2 position)
        {
            Vector2 E_total = new Vector2(0, 0);
            float threshold = 1e-6f;

            foreach (var charge in charges)
            {
                Vector2 r = position - charge.Position * uniformScale;

                double r_mag = Math.Sqrt(r.X * r.X + r.Y * r.Y);

                if (r_mag > threshold)
                {
                    // normalizace smeru
                    Vector2 r_normalized = new Vector2((float)(r.X / r_mag), (float)(r.Y / r_mag));

                    // Coulombuv zakon
                    float fieldMagnitude = (float)(k * charge.Q) / (float)(r_mag * r_mag * r_mag);

                    Vector2 E = fieldMagnitude * r_normalized;
                    E_total += E;
                }
            }

            return E_total;
        }



        private void DrawColorMap(Graphics g, int cx, int cy, float uniformScale)
        {
            int step = (int)(10 * uniformScale);
            int widthSteps = this.Width / step;
            int heightSteps = this.Height / step;

            // pripravit intenzity pole
            fieldIntensities = new float[widthSteps + 1, heightSteps + 1];
            intensityList = new List<float>();

            // vypocitej intenzity pole
            for (int xStep = 0; xStep <= widthSteps; xStep++)
            {
                int x = xStep * step;
                for (int yStep = 0; yStep <= heightSteps; yStep++)
                {
                    int y = yStep * step;
                    float intensity = 0;

                    // pripravit intenzity pole na zaklade vzdalenosti chargeru
                    foreach (var charge in charges)
                    {
                        float dx = (x - cx) - (charge.Position.X * uniformScale);
                        float dy = (y - cy) - (charge.Position.Y * uniformScale);
                        float r = (float)Math.Sqrt(dx * dx + dy * dy);


                        if (r > 1e-6f) // Zabraneni delani 0
                        {
                            intensity += (float)charge.Q / r;
                        }


                    }
                    intensity = Math.Min(intensity, 1e6f);
                    // uloz
                    fieldIntensities[xStep, yStep] = intensity;
                    intensityList.Add(intensity);
                }
            }

            // najdi max/min
            intensityList.Sort();
            float minIntensity = intensityList[(int)(0.05 * intensityList.Count)];
            float maxIntensity = intensityList[(int)(0.95 * intensityList.Count)];
            float minlevelIntensity = intensityList[(int)(0.01 * intensityList.Count)];
            float maxlevelIntensity = intensityList[(int)(0.99 * intensityList.Count)];



            // max/min kolem 0
            float maxAbsIntensity = Math.Max(Math.Abs(minIntensity), Math.Abs(maxIntensity));
            if (maxAbsIntensity == 0)
            {
                maxAbsIntensity = 1e-5f;
            }

            if (showColorMap)
            {
                SolidBrush brush = new SolidBrush(Color.Black);
                //projdi kazdy ctverec a nakresli tam ctverec
                for (int xStep = 0; xStep <= widthSteps; xStep++)
                {
                    int x = cx + (xStep - widthSteps / 2 - 1) * step;

                    for (int yStep = 0; yStep <= heightSteps; yStep++)
                    {
                        int y = cy + (yStep - heightSteps / 2 - 1) * step;

                        float intensity = fieldIntensities[xStep, yStep];

                        // normalizovana hodnota symetricky 
                        float normalizedIntensity = intensity / maxAbsIntensity;

                        // najdi barvu
                        normalizedIntensity = Math.Clamp(normalizedIntensity, -1, 1);
                        brush.Color = normalizedIntensity < 0
                            ? NegativeMapIntensityToColor(normalizedIntensity)
                            : MapIntensityToColor(normalizedIntensity);


                        // nakresli ctverec
                        g.FillRectangle(brush, x, y, step + 1, step + 1);
                    }
                }
            }

            if (showLevelLines)
            {

                // max pocet
                int maxContourLevels = 20;

                // vypocet poctu vrstevnic
                int contourLevels = CalculateAdaptiveContourLevels(minlevelIntensity, maxlevelIntensity, maxContourLevels);

                // intenzita vrstevnic
                float contourInterval = (maxlevelIntensity - minlevelIntensity) / contourLevels;

                //vykresleni vrstevnic
                for (int i = 0; i <= contourLevels; i++)
                {
                    float contourLevel = minlevelIntensity + i * contourInterval;
                    DrawContourLines(g, contourLevel, widthSteps, heightSteps, step);
                }
            }
        }


        private int CalculateAdaptiveContourLevels(float minIntensity, float maxIntensity, int maxContourLevels)
        {
            // rozsah
            float range = maxIntensity - minIntensity;
            int levels = (int)Math.Ceiling(range / 10); // Nastav "hustotu" dle potřeby

            // pocet
            return Math.Clamp(levels, 10, maxContourLevels);
        }

        private void DrawContourLines(Graphics g, float contourLevel, int widthSteps, int heightSteps, int step)
        {

            for (int xStep = 0; xStep < widthSteps - 1; xStep++)
            {
                int x = xStep * step;
                for (int yStep = 0; yStep < heightSteps - 1; yStep++)
                {
                    int y = yStep * step;

                    // intezity v rozich bunky
                    float bl = fieldIntensities[xStep, yStep];
                    float br = fieldIntensities[xStep + 1, yStep];
                    float tl = fieldIntensities[xStep, yStep + 1];
                    float tr = fieldIntensities[xStep + 1, yStep + 1];

                    // na vyhledavani pruseciku vrstevnic
                    List<PointF> points = new List<PointF>();

                    // levy okraj
                    if ((bl < contourLevel && tl > contourLevel) || (bl > contourLevel && tl < contourLevel))
                    {
                        float t = (contourLevel - bl) / (tl - bl);
                        points.Add(new PointF(x, y + step * t));
                    }

                    // pravy okraj
                    if ((br < contourLevel && tr > contourLevel) || (br > contourLevel && tr < contourLevel))
                    {
                        float t = (contourLevel - br) / (tr - br);
                        points.Add(new PointF(x + step, y + step * t));
                    }

                    // dolni okraj
                    if ((bl < contourLevel && br > contourLevel) || (bl > contourLevel && br < contourLevel))
                    {
                        float t = (contourLevel - bl) / (br - bl);
                        points.Add(new PointF(x + step * t, y));
                    }

                    // horni okraj
                    if ((tl < contourLevel && tr > contourLevel) || (tl > contourLevel && tr < contourLevel))
                    {
                        float t = (contourLevel - tl) / (tr - tl);
                        points.Add(new PointF(x + step * t, y + step));
                    }

                    // vykresleni vrstevnice
                    if (points.Count == 2)
                    {
                        using (Pen pen = new Pen(showColorMap ? Color.Black : Color.Gray, 1.5f * uniformScale))
                        {
                            g.DrawLine(pen, points[0], points[1]);
                        }
                    }
                }
            }
        }

        private Color MapIntensityToColor(float normalizedIntensity)
        {
            // barvy ze zlute do cervene
            int red = 255;
            int green = (int)(255 * (1 - normalizedIntensity));
            int blue = 0;
            int alpha = 255;

            green = Math.Clamp(green, 0, 255);

            return Color.FromArgb(alpha, red, green, blue);
        }

        private Color NegativeMapIntensityToColor(float normalizedIntensity)
        {
            // barvy ze zlute do zelene
            int red = (int)(255 * (1 + normalizedIntensity));
            int green = 255;
            int blue = 0;
            int alpha = 255;

            red = Math.Clamp(red, 0, 255);

            return Color.FromArgb(alpha, red, green, blue);
        }

        private void DrawLegend(Graphics g, float minField, float maxField)
        {
            legendY = this.Height - legendHeight - 60;

            // hodnoty pro symetrickou skalu
            float maxAbsField = Math.Max(Math.Abs(minField), Math.Abs(maxField));
            minField = -maxAbsField;
            maxField = maxAbsField;

            // defenovani labelu pro legendu
            float[] labels = {
                maxAbsField,
                maxAbsField / 15,
                maxAbsField / 500,
                maxAbsField / 15,
                maxAbsField
            };

            // barevny gradient
            for (int i = 0; i <= legendWidth; i++)
            {
                float normalizedValue = (float)i / legendWidth;
                float fieldValue = minField + normalizedValue * (maxField - minField);

                Color color = fieldValue < 0
                    ? NegativeMapIntensityToColor(fieldValue / maxAbsField)
                    : MapIntensityToColor(fieldValue / maxAbsField);

                using (SolidBrush brush = new SolidBrush(color))
                {
                    g.FillRectangle(brush, legendX + i, legendY, 1, legendHeight);
                }
            }

            // remecek legendy
            using (Pen pen = new Pen(Color.Black))
            {
                g.DrawRectangle(pen, legendX, legendY, legendWidth, legendHeight);
            }

            // popisky a znacky
            using (Font font = new Font("Arial", 8))
            using (SolidBrush textBrush = new SolidBrush(Color.Black))
            {
                for (int i = 0; i < labels.Length; i++)
                {
                    float fieldValue = labels[i];
                    string label;
                    if (i == 0 || i == labels.Length - 1)
                    {
                        label = $"> {fieldValue.ToString("0.##E+00")}";
                    }
                    else
                    {
                        label = fieldValue.ToString("0.##E+00");
                    }

                    int labelX = legendX + (int)(i * legendWidth / (labels.Length - 1)) - 20;
                    int labelY = legendY + legendHeight + 5;

                    g.DrawString(label, font, textBrush, labelX, labelY);

                    using (Pen tickPen = new Pen(Color.Black))
                    {
                        int tickX = legendX + (int)(i * legendWidth / (labels.Length - 1));
                        int tickYStart = legendY + legendHeight;
                        int tickYEnd = tickYStart - 5;
                        g.DrawLine(tickPen, tickX, tickYStart, tickX, tickYEnd);
                    }
                }

                // jednotky
                string unitsText = "[N/C]";
                int unitsX = legendX + legendWidth + 10;
                int unitsY = legendY + legendHeight / 2 - (int)(g.MeasureString(unitsText, font).Height / 2);

                g.DrawString(unitsText, font, textBrush, unitsX, unitsY);
            }
        }


        private void DrawFieldLines(Graphics g)
        {
            foreach (var charge in charges)
            {
                using (Pen pen = new Pen(showColorMap ? Color.Black : Color.Gray, 1.5f * uniformScale))
                {

                    int numberOfLines = 8; //pocet silocar
                    float stepLength = 5f; // delka kroku
                    float minDistanceSquared = (charge.radius * uniformScale * 0.1f) * (charge.radius * uniformScale * 0.1f); // ctvercova minimalni vzdalenost

                    for (int i = 0; i < numberOfLines; i++)
                    {
                        //zacatek
                        double angle = i * 2 * Math.PI / numberOfLines;
                        Vector2 startPoint = new Vector2(
                            charge.Position.X * uniformScale + cx + (float)(charge.radius * Math.Cos(angle)),
                            charge.Position.Y * uniformScale + cy + (float)(charge.radius * Math.Sin(angle))
                        );

                        //jedem
                        int steps = 0;
                        while (steps < 1000)
                        {
                            //smer 
                            Vector2 electricField = ScaledCalculateElectricField(startPoint - new Vector2(cx, cy));
                            if (electricField.LengthSquared() == 0) break;

                            Vector2 direction = Vector2.Normalize(electricField) * stepLength;
                            Vector2 endPoint = charge.Q > 0
                                ? startPoint + direction
                                : startPoint - direction;

                            //vzdalenost od charge
                            float distanceToChargeSquared = Vector2.DistanceSquared(endPoint, charge.Position + new Vector2(cx, cy));
                            if (distanceToChargeSquared < minDistanceSquared) break;


                            if (charges.Any(otherCharge => charge != otherCharge &&
                        Vector2.DistanceSquared(endPoint, otherCharge.Position + new Vector2(cx, cy)) < minDistanceSquared))
                            {
                                break;
                            }

                            // vykresli ceru
                            g.DrawLine(pen, startPoint.X, startPoint.Y, endPoint.X, endPoint.Y);

                            // sipky na silocarach
                            if (steps % 10 == 0 || steps == 999)
                            {
                                DrawArrow(g, pen, startPoint, endPoint, charge.Q > 0);
                            }

                            startPoint = endPoint;
                            steps++;
                        }
                    }
                }
            }
        }

        private void DrawArrow(Graphics g, Pen pen, Vector2 start, Vector2 end, bool isPositive)
        {
            const float arrowSize = 8f; // velikost sipkty

            // Směr
            Vector2 direction = isPositive ? Vector2.Normalize(end - start) : Vector2.Normalize(start - end);

            // Body sipky
            Vector2 arrowBase = end - direction * arrowSize;
            Vector2 leftWing = new Vector2(
                arrowBase.X + arrowSize * (float)Math.Cos(Math.PI / 6) * -direction.Y,
                arrowBase.Y + arrowSize * (float)Math.Sin(Math.PI / 6) * direction.X
            );
            Vector2 rightWing = new Vector2(
                arrowBase.X - arrowSize * (float)Math.Cos(Math.PI / 6) * -direction.Y,
                arrowBase.Y - arrowSize * (float)Math.Sin(Math.PI / 6) * direction.X
            );

            //vykresleni
            g.DrawLine(pen, end.X, end.Y, leftWing.X, leftWing.Y);
            g.DrawLine(pen, end.X, end.Y, rightWing.X, rightWing.Y);
        }

        // reset cest
        public void resetPath()
        {
            foreach (var charge in charges)
            {
                charge.Spline.Reset(new PointF(charge.Position.X, charge.Position.Y));
                charge.Spline.invalide = false;
            }
        }


        // Zoom handling
        private void DrawingPanel_MouseWheel(object sender, MouseEventArgs e)
        {
            float zoomFactor = 1.1f;
            if (e.Delta > 0)
            {
                zoom *= zoomFactor;
            }
            else
            {
                zoom /= zoomFactor;
            }

            this.Invalidate();
        }

        private void DrawingPanel_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            foreach (var charge in charges)
            {
                //double click na charge
                if (charge.isHitByMouse(e))
                {

                    charge.ShowEditForm();
                    Invalidate();
                    break;
                }

                //double click na splinu
                if (e.Button == MouseButtons.Left)
                {
                    charge.Spline.MouseDoubleClick(e.Location, e.Button);//pridani bodu
                    Invalidate();
                }
                Invalidate();
            }

            foreach (var probe in probes)
            {
                //double click na probe
                if (probe.isHit(e))
                {

                    probe.ShowEditForm();
                    Invalidate();
                    break;
                }
            }
        }


        private void DrawingPanel_MouseDown(object sender, MouseEventArgs e)
        {
            // spline
            foreach (var charge in charges)
            {
                charge.Spline.MouseDown(e.Location, e.Button);
            }

            if (e.Button == MouseButtons.Left)
            {
                //pridani nove sondy na shift + click
                if (Control.ModifierKeys == Keys.Shift)
                {
                    Probe newProbe = new Probe(new Vector2((e.X - panOffset.X - cx) / (zoom * uniformScale),
                                                           (e.Y - panOffset.Y - cy) / (zoom * uniformScale)),
                                               this, true);
                    probes.Add(newProbe);

                    // graf pro sondu
                    Graph graph = new Graph(this, newProbe);
                    Invalidate();
                }
                else
                {
                    // vyber naboje nebo sondy
                    foreach (var charge in charges)
                    {
                        if (charge.isHitByMouse(e))
                        {
                            selectedCharge = charge;
                            lastMousePosition = new Vector2((e.X - panOffset.X) / (zoom * uniformScale),
                                                            (e.Y - panOffset.Y) / (zoom * uniformScale));
                            break;
                        }
                    }

                    foreach (var probe in probes)
                    {
                        if (probe.isHit(e))
                        {
                            selectedProbe = probe;
                            selectedProbe.dragging = true;
                            lastMousePosition = new Vector2((e.X - panOffset.X) / (zoom * uniformScale),
                                                            (e.Y - panOffset.Y) / (zoom * uniformScale));
                            break;
                        }
                    }
                }
            }
        }

        private void DrawingPanel_MouseMove(object sender, MouseEventArgs e)
        {
            // prepocet na logicke souradnice
            float logicalMouseX = (e.X - panOffset.X) / (zoom * uniformScale);
            float logicalMouseY = (e.Y - panOffset.Y) / (zoom * uniformScale);

            float leftBoundary = -this.Width / 2 / (zoom * uniformScale);
            float rightBoundary = this.Width / 2 / (zoom * uniformScale);
            float topBoundary = -this.Height / 2 / (zoom * uniformScale);
            float bottomBoundary = this.Height / 2 / (zoom * uniformScale);

            if (selectedCharge != null && e.Button == MouseButtons.Left)
            {
                timer.Stop();

                // prepocet delta pohybu
                float dx = logicalMouseX - lastMousePosition.X;
                float dy = logicalMouseY - lastMousePosition.Y;
                Vector2 newPosition = selectedCharge.Position + new Vector2(dx, dy);

                // meze
                newPosition.X = Math.Clamp(newPosition.X, leftBoundary, rightBoundary);
                newPosition.Y = Math.Clamp(newPosition.Y, topBoundary, bottomBoundary);

                //kolize
                bool collisionDetected = false;
                foreach (var charge in charges)
                {
                    if (charge != selectedCharge)
                    {
                        float distance = Vector2.Distance(newPosition, charge.Position);
                        if (distance < 2 * charge.radius * uniformScale)
                        {
                            collisionDetected = true;
                            break;
                        }
                    }
                }

                if (!collisionDetected)
                {
                    selectedCharge.Position = newPosition;
                    lastMousePosition = new Vector2(logicalMouseX, logicalMouseY);
                    Invalidate();
                }
            }

            if (selectedProbe != null && e.Button == MouseButtons.Left)
            {
                timer.Stop();

                // prepocet delta pohybu
                float dx = logicalMouseX - lastMousePosition.X;
                float dy = logicalMouseY - lastMousePosition.Y;

                Vector2 newProbePosition = selectedProbe.pointPosition + new Vector2(dx, dy);

                // meze
                newProbePosition.X = Math.Clamp(newProbePosition.X, leftBoundary, rightBoundary);
                newProbePosition.Y = Math.Clamp(newProbePosition.Y, topBoundary, bottomBoundary);

                // Aktualizace pozice sondy
                selectedProbe.pointPosition = newProbePosition;
                lastMousePosition = new Vector2(logicalMouseX, logicalMouseY);

                Invalidate();
            }

            if (e.Button == MouseButtons.Left)
            {
                foreach (var charge in charges)
                {
                    charge.Spline.MouseMove(new PointF(logicalMouseX, logicalMouseY));
                }
                Invalidate();
            }
        }

        private void DrawingPanel_MouseUp(object sender, MouseEventArgs e)
        {
            foreach (var charge in charges)
            {
                charge.Spline.MouseUp();
            }

            if (e.Button == MouseButtons.Left)
            {
                timer.Start();
                selectedCharge = null; // nehejbu s chargem


                if (selectedProbe != null) selectedProbe.dragging = false;
                selectedProbe = null;


            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            time += time_speed;
            this.Invalidate();
        }


        public void ChangeScenario(int newScenario)
        {

            //MessageBox.Show("Switched to Scenario " + newScenario);
            SetChargesForScenario(newScenario);
            Invalidate();
        }

        public void DrawingPanel_Resize(object sender, EventArgs e)
        {

            SideMenu.menuButton.Location = new Point(this.Width - SideMenu.menuButton.Width - 10, this.Height / 2 - SideMenu.menuButton.Height / 2);
            SideMenu.sideMenuPanel.Size = new Size(this.Width / 5, this.Height);
            SideMenu.sideMenuPanel.Location = new Point(this.Width - SideMenu.sideMenuPanel.Width, 0);


        }

        protected override void OnResize(EventArgs eventargs)
        {
            this.Invalidate();
            base.OnResize(eventargs);
        }
    }
}
