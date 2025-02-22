using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UPG_SP_2024;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.Measure;
using SkiaSharp;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.WinForms;
using System.Numerics;
using LiveChartsCore.SkiaSharpView.VisualElements;

namespace ElectricFieldVis
{
    public class Graph
    {
        private Form graphForm;
        private Dictionary<Probe, LineSeries<double>> probeSeries;
        private CartesianChart chart;
        private DrawingPanel drawingPanel;
        private double startTime;
        private CancellationTokenSource cancellationTokenSource;
        private List<Probe> showingProbes;
        public Graph(DrawingPanel drawingpanel)
        {
            drawingPanel = drawingpanel;
            
            startTime = drawingPanel.time;
            probeSeries = new Dictionary<Probe, LineSeries<double>>();
            cancellationTokenSource = new CancellationTokenSource();

            //warning 
            if (drawingPanel.probesgraph == null || !drawingPanel.probesgraph.Any())
            {
                MessageBox.Show("No sellected probes to graph. Please, double click on probes and add them.",
                                "No Probes",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                return;
            }
            //priprav graf
            InitializeGraph(drawingPanel.probesgraph);
        }
        public Graph(DrawingPanel drawingpanel, Probe singleProbe)
        {
            drawingPanel = drawingpanel;
            startTime = drawingPanel.time;
            probeSeries = new Dictionary<Probe, LineSeries<double>>();
            cancellationTokenSource = new CancellationTokenSource();

            //priprav graf
            InitializeGraph(new List<Probe> { singleProbe });


            
        }

        private void InitializeGraph(List<Probe> probes)
        {
            // vytvoreni stranky
            graphForm = new Form
            {
                Text = "Electric Field Intensity Over Time",
                Size = new Size(800, 600),
                BackColor = Color.White
            };

            // inicializace grafu
            chart = new CartesianChart
            {
                Dock = DockStyle.Fill,
                

            };

            chart.Title = new LabelVisual()
            {
                Text = "Electric Field Intensity Over Time",
                TextSize = 20
            };

            chart.LegendPosition = LiveChartsCore.Measure.LegendPosition.Bottom;

            // inicializace osy X a Y
            chart.XAxes = new[]
            {
                new Axis
                {
                    Name = "Time (s)",
                   // LabelsRotation = -45,
                    NameTextSize = 18,
                    MinStep = 0.1,
                    Labeler = value => value.ToString("F1"),
                    UnitWidth = 1,
                    TextSize = 12,
                    
                }
            };

            chart.YAxes = new[]
            {
                new Axis
                {
                    Name = "Electric Field Intensity (N/C)",
                    NameTextSize = 18
                }
            };

            // vytvor serii bodu pro intezitu pole 
            foreach (var probe in probes)
            {
                var series = new LineSeries<double>
                {
                    Name = $"Probe {drawingPanel.probesgraph.IndexOf(probe)}",
                    Stroke = new SolidColorPaint(probe.SKColor, 2),
                    GeometrySize = 0,
                    Fill = null
                    //DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                    //DataLabelsFormatter = point => $"{point:F2} N/C"
                };

                probeSeries[probe] = series;
            }



            // pridani momentu do chartu
            chart.Series = probeSeries.Values.Cast<ISeries>().ToArray();
            graphForm.Controls.Add(chart);

            // zavreni stranky
            graphForm.FormClosing += (s, e) =>
            {
                if (cancellationTokenSource != null && !cancellationTokenSource.Token.IsCancellationRequested)
                {
                    cancellationTokenSource.Cancel();
                }
                cancellationTokenSource?.Dispose();
                cancellationTokenSource = null;
                drawingPanel.probesgraph.Clear();
            };

            graphForm.Show();



            // spusteni vykresleni az po inicializaci
            StartGraphUpdates(probes);
        }

        private async void StartGraphUpdates(List<Probe> probes)
        {
            
            while (true)
            {
                // kotrola vypnute stranky
                if (cancellationTokenSource == null || cancellationTokenSource.Token.IsCancellationRequested)
                    break;

                double elapsedTime = drawingPanel.time - startTime;

                // bezpecny pristup ke grafu
                if (graphForm != null && !graphForm.IsDisposed)
                {
                    graphForm.Invoke(new Action(() =>
                    {
                        if (chart != null && probeSeries != null)
                        {
                            foreach (var probe in probes)
                            {
                                if (probeSeries.ContainsKey(probe))
                                {
                                    var series = probeSeries[probe];

                                    ///zajisti inicializaci
                                    if (series.Values == null)
                                    {
                                        series.Values = new double[0];
                                    }

                                    
                                    // pridej novy bod
                                    series.Values = series.Values.Append(probe.fieldAtProbe).ToArray();
                                }
                            }

                            // update osy x
                            var xAxis = chart.XAxes.FirstOrDefault();
                            if (xAxis != null)
                            {
                                xAxis.Labels = Enumerable.Range(0, probeSeries.First().Value.Values.Count)
                                                        .Select(i => i.ToString())
                                                        .ToArray();
                            }
                        }
                    }));
                }

                
               await Task.Delay(1000);
            }
        }
    }



}



