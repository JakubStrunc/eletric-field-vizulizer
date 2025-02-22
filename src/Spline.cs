using LiveChartsCore.Kernel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using UPG_SP_2024;

namespace ElectricFieldVis
{
    public class SplineEditor
    {
        public List<PointF> ControlPoints { get; set; }
        public List<PointF> Tangents { get; set; }
        private PointF? draggedPoint = null;
        private bool isTangent = false;

        public List<PointF> path = new List<PointF>();

        private DrawingPanel drawingPanel;

        public bool invalide = false;

        public SplineEditor(PointF startPoint, DrawingPanel drawingpanel)
        {
            // inicalializace prochazejicich bodu
            ControlPoints = new List<PointF>
            {
                new PointF(startPoint.X+100, startPoint.Y+100),
                new PointF(startPoint.X, startPoint.Y),
                new PointF(startPoint.X-100, startPoint.Y-100),
            };

            drawingPanel = drawingpanel;

            // inicializace tangentu 
            Tangents = new List<PointF>();
            for (int i = 0; i < ControlPoints.Count; i++)
            {
                if (i == 0 || i == ControlPoints.Count - 1)
                {
                    Tangents.Add(CalculateTangent(i));
                }
                else
                {
                    var (tangent1, tangent2) = CalculateTangents(i);
                    Tangents.Add(tangent1); 
                    Tangents.Add(tangent2);
                }
            }
        }

        public void Draw(Graphics g)
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // kresleni prochazejicich bodu
            foreach (var point in ControlPoints)
            {
                float adjustedX = point.X * drawingPanel.uniformScale + drawingPanel.cx;
                float adjustedY = point.Y * drawingPanel.uniformScale + drawingPanel.cy;

                g.FillEllipse(Brushes.Blue, adjustedX - 5 * drawingPanel.uniformScale, adjustedY - 5 * drawingPanel.uniformScale, 10 * drawingPanel.uniformScale, 10 * drawingPanel.uniformScale);
            }

            //Debug.WriteLine(Tangents);

            // kresleni tangentu
            for (int i = 0; i < Tangents.Count; i++)
            {
                float adjustedX = Tangents[i].X * drawingPanel.uniformScale + drawingPanel.cx;
                float adjustedY = Tangents[i].Y * drawingPanel.uniformScale + drawingPanel.cy;

                g.FillEllipse(Brushes.Red, adjustedX - 5 * drawingPanel.uniformScale, adjustedY - 5 * drawingPanel.uniformScale, 10 * drawingPanel.uniformScale, 10 * drawingPanel.uniformScale);

                if (i % 2 == 0 && i + 1 < Tangents.Count) // vykresli dvojici tangentu pro stredni body 
                {
                    float controlX1 = ControlPoints[i / 2].X * drawingPanel.uniformScale + drawingPanel.cx;
                    float controlY1 = ControlPoints[i / 2].Y * drawingPanel.uniformScale + drawingPanel.cy;
                    float controlX2 = ControlPoints[i / 2 + 1].X * drawingPanel.uniformScale + drawingPanel.cx;
                    float controlY2 = ControlPoints[i / 2 + 1].Y * drawingPanel.uniformScale + drawingPanel.cy;

                    g.DrawLine(Pens.Gray, controlX1, controlY1, adjustedX, adjustedY); 
                    g.DrawLine(Pens.Gray, controlX2, controlY2, Tangents[i + 1].X * drawingPanel.uniformScale + drawingPanel.cx, Tangents[i + 1].Y * drawingPanel.uniformScale + drawingPanel.cy); // Right tangent
                }
                else if (i == 0 || i == Tangents.Count - 1) // vykresli jednotlivce pro konecne body
                {
                    float controlX = ControlPoints[i == 0 ? 0 : ControlPoints.Count - 1].X * drawingPanel.uniformScale + drawingPanel.cx;
                    float controlY = ControlPoints[i == 0 ? 0 : ControlPoints.Count - 1].Y * drawingPanel.uniformScale + drawingPanel.cy;

                    g.DrawLine(Pens.Gray, controlX, controlY, adjustedX, adjustedY); // Endpoints tangents
                }
            }

            // nakresli splinu
            DrawBezierSpline(g);
        }

        private void DrawBezierSpline(Graphics g)
        {
            if (ControlPoints.Count < 2) return;

            //Debug.WriteLine(path.Count());
            path.Clear();
            float segmentLength = 2f; // delka kazdeho segmentu
            PointF lastPoint = ControlPoints[0]; // zacatek

            for (int i = 0; i < ControlPoints.Count - 1; i++)
            {
                // nastaveni bodu do bezier
                var p0 = ControlPoints[i];
                var p1 = Tangents[i * 2];
                var p2 = Tangents[i * 2 + 1];
                var p3 = ControlPoints[i + 1];

                // vypocet bodu na intervalu <0,1> 
                float t = 0;
                while (t <= 1)
                {
                    var currentPoint = CubicBezier(p0, p1, p2, p3, t);

                    // pridani bodu pokud je rozdil vetsi nebo rovno segment lenght
                    if (Distance(lastPoint, currentPoint) >= segmentLength)
                    {
                        path.Add(currentPoint);
                        lastPoint = currentPoint;
                    }

                    t += 0.001f; // na dalsi bod
                }
            }


            // vykresleni krivky
            var transformedPath = path.Select(p => new PointF(
                p.X * drawingPanel.uniformScale + drawingPanel.cx,
                p.Y * drawingPanel.uniformScale + drawingPanel.cy
            )).ToArray();
            g.DrawLines(invalide ? Pens.White : Pens.Red, transformedPath);
        }

        private PointF CubicBezier(PointF p0, PointF p1, PointF p2, PointF p3, float t)
        {
            // vypocet duplicit
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            // vypocet bodu na krivce 
            float x = uuu * p0.X + 3 * uu * t * p1.X + 3 * u * tt * p2.X + ttt * p3.X;
            float y = uuu * p0.Y + 3 * uu * t * p1.Y + 3 * u * tt * p2.Y + ttt * p3.Y;

            
            return new PointF(
                x,y
            );
        }

        private (PointF tangent1, PointF tangent2) CalculateTangents(int index)
        {

            var p0 = ControlPoints[index - 1];
            var p1 = ControlPoints[index];
            var p2 = ControlPoints[index + 1];

            // smer od prochezejiciho bodu
            var dir1 = new PointF(p2.X - p0.X, p2.Y - p0.Y);
            var norm1 = Normalize(dir1);

            // delka od prochezejiciho bodu
            var dist1 = Distance(p0, p1) / 3;
            var dist2 = Distance(p1, p2) / 3;
            var dist = (dist1 + dist2) / 2;

            // vypocet tangentu 
            var tangent1 = new PointF(p1.X  - norm1.X * dist, p1.Y  - norm1.Y * dist);
            var tangent2 = new PointF(p1.X  + norm1.X * dist, p1.Y  + norm1.Y * dist);

            return (tangent1, tangent2);
        }

        private PointF CalculateTangent(int index)
        {
            if (index == 0)
            {
                var dir = new PointF(
                    ControlPoints[index + 1].X - ControlPoints[index].X,
                    ControlPoints[index + 1].Y - ControlPoints[index].Y);
                return new PointF(ControlPoints[index].X + dir.X / 3, ControlPoints[index].Y + dir.Y / 3);
            }
            else
            {
                var dir = new PointF(
                    ControlPoints[index].X - ControlPoints[index - 1].X,
                    ControlPoints[index].Y - ControlPoints[index - 1].Y);
                return new PointF(ControlPoints[index].X + dir.X / 3, ControlPoints[index].Y + dir.Y / 3);
            }

        }
        private void UpdateTangents()
        {
            //prepocitani tangetu
            Tangents.Clear();
            for (int i = 0; i < ControlPoints.Count; i++)
            {
                //single tangent 
                if (i == 0 || i == ControlPoints.Count - 1)
                {
                    Tangents.Add(CalculateTangent(i));
                }
                //double tangenty
                else
                {
                    var (tangent1, tangent2) = CalculateTangents(i);
                    Tangents.Add(tangent1);
                    Tangents.Add(tangent2);
                }
            }
        }
        private void AddControlPoint(PointF location)
        {
            // najdi index vybraneho bodu
            int index = ControlPoints.IndexOf(FindPoint(location, out _) ?? default);

            if (index >= 0 && index < ControlPoints.Count - 1)
            {
                // pridej novy bod mezi vybrany bod a nasledujici
                PointF newPoint = new PointF(
                    (ControlPoints[index].X + ControlPoints[index + 1].X) / 2,
                    (ControlPoints[index].Y + ControlPoints[index + 1].Y) / 2
                );

                ControlPoints.Insert(index + 1, newPoint);
                //update tangentu
                UpdateTangents();
            }
        }

        private void RemoveControlPoint(PointF location)
        {
            // najdi index vybraneho bodu
            int index = ControlPoints.IndexOf(FindPoint(location, out _) ?? default);

            // odstran bod pokud neni prvni nebo posledni
            if (index > 0 && index < ControlPoints.Count - 1)
            {
                ControlPoints.RemoveAt(index);
                UpdateTangents();
            }
        }

        private PointF AdjustTangent(PointF center, PointF fixedTangent)
        {
            // body relatine ke stredu
            var adjustedFixedTangent = new PointF(fixedTangent.X + drawingPanel.cx, fixedTangent.Y + drawingPanel.cy);
            var adjustedCenter = new PointF(center.X + drawingPanel.cx, center.Y + drawingPanel.cy);

            // vypocitej smer stredniho bodu a tangentu
            var direction = new PointF(adjustedCenter.X - adjustedFixedTangent.X, adjustedCenter.Y - adjustedFixedTangent.Y);
            var normalized = Normalize(direction);

            // vypocitej vzdalenost mezi tangentem a strednim bodem
            var distance = Distance(adjustedCenter, adjustedFixedTangent);

            // pozice protejsiho tengetu
            return new PointF(adjustedCenter.X + normalized.X * distance - drawingPanel.cx, adjustedCenter.Y + normalized.Y * distance - drawingPanel.cy);
        }


        public void MouseDoubleClick(PointF location, MouseButtons button)
        {
            //pridej na double click novy prochazejici bod
            if (button == MouseButtons.Left)
            {
                AddControlPoint(location);
            }
        }

        public void MouseDown(PointF location, MouseButtons button)
        {
            //na leve odstran prochazajici bod
            if (button == MouseButtons.Right)
            {
                RemoveControlPoint(location);
            }
            //zjisti jestli se nekliklo na nejakej bod
            else
            {
                draggedPoint = FindPoint(location, out isTangent);
            }
        }

        public void MouseMove(PointF location)
        {
            // prepocet na souradnidce relativne ke 
            
            float logicalMouseX = (location.X * (drawingPanel.uniformScale)  - drawingPanel.cx)/drawingPanel.uniformScale;
            float logicalMouseY = (location.Y * (drawingPanel.uniformScale) - drawingPanel.cy) / drawingPanel.uniformScale;

            if (draggedPoint.HasValue)
            {
                //posouva se tangent
                if (isTangent)
                {
                    int index = Tangents.IndexOf(draggedPoint.Value);
                    if (index != -1)
                    {
                        this.invalide = true;
                        Tangents[index] = new PointF(logicalMouseX, logicalMouseY);

                        // aktalizace souvisejiciho tangentu
                        if (index > 0 && index < Tangents.Count - 1)
                        {
                            if (index % 2 == 1)
                            {
                                Tangents[index + 1] = AdjustTangent(ControlPoints[index / 2 + 1], Tangents[index]);
                            }
                            else
                            {
                                Tangents[index - 1] = AdjustTangent(ControlPoints[index / 2], Tangents[index]);
                            }
                        }
                    }
                }
                //posouva se prochazejici bod
                else
                {
                    int index = ControlPoints.IndexOf(draggedPoint.Value);
                    if (index != -1)
                    {
                        float dx = logicalMouseX - ControlPoints[index].X;
                        float dy = logicalMouseY - ControlPoints[index].Y;
                        ControlPoints[index] = new PointF(logicalMouseX, logicalMouseY);
                        this.invalide = true;
                        // aktualizace tangetu
                        if (index == 0)
                        {
                            Tangents[0] = new PointF(Tangents[0].X + dx, Tangents[0].Y + dy);
                        }
                        else if (index == ControlPoints.Count - 1)
                        {
                            Tangents[Tangents.Count - 1] = new PointF(Tangents[Tangents.Count - 1].X + dx, Tangents[Tangents.Count - 1].Y + dy);
                        }
                        else
                        {
                            Tangents[index * 2] = new PointF(Tangents[index * 2].X + dx, Tangents[index * 2].Y + dy);
                            Tangents[index * 2 - 1] = new PointF(Tangents[index * 2 - 1].X + dx, Tangents[index * 2 - 1].Y + dy);
                        }
                    }
                }

                // update posouvaciho bodu
                draggedPoint = new PointF(logicalMouseX, logicalMouseY);
            }
        }



        public void MouseUp()
        {
            draggedPoint = null;
        }

        private PointF? FindPoint(PointF location, out bool isTangentPoint)
        {
            foreach (var point in ControlPoints)
            {
                if (MouseDistance(point, location) < 10)
                {
                    isTangentPoint = false;
                    return point;
                }
            }

            foreach (var tangent in Tangents)
            {
                if (MouseDistance(tangent, location) < 10)
                {
                    isTangentPoint = true;
                    return tangent;
                }
            }

            isTangentPoint = false;
            return null;
        }

        private PointF Normalize(PointF vector)
        {
            float length = (float)Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
            if (length == 0) return new PointF(0, 0);

            return new PointF(vector.X / length, vector.Y / length);
        }

        private float MouseDistance(PointF p1, PointF p2)
        {
            return (float)Math.Sqrt(Math.Pow(((p1.X * drawingPanel.uniformScale) + drawingPanel.cx) - p2.X, 2) + Math.Pow(((p1.Y * drawingPanel.uniformScale) + drawingPanel.cy) - p2.Y, 2));
        }

        private float Distance(PointF p1, PointF p2)
        {
            return (float)Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }

        public void Reset(PointF Point)
        {
            // reset prochazejicich bodu
            ControlPoints.Clear();
            ControlPoints = new List<PointF>
            {
                new PointF(Point.X+100, Point.Y+100),
                new PointF(Point.X, Point.Y),
                new PointF(Point.X-100, Point.Y-100),
            };

            // reset tangentu
            Tangents.Clear();
            for (int i = 0; i < ControlPoints.Count; i++)
            {
                if (i == 0 || i == ControlPoints.Count - 1)
                {
                    Tangents.Add(CalculateTangent(i));
                }
                else
                {
                    var (tangent1, tangent2) = CalculateTangents(i);
                    Tangents.Add(tangent1); // Left tangent
                    Tangents.Add(tangent2); // Right tangent
                }
            }

            // vymaz path
            path.Clear();
            draggedPoint = null;
        }


    }
}