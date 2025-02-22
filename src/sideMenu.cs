using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using UPG_SP_2024;
using System.Numerics;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

namespace ElectricFieldVis
{
    public class sideMenu
    {
        public Button menuButton {  get; set; }
        public Panel sideMenuPanel {  get; set; }
        private int Width;
        private int Height;
        private DrawingPanel drawingPanel;

        public sideMenu(DrawingPanel drawingPanel) {
            
            this.drawingPanel = drawingPanel;

            this.Width = drawingPanel.Width;
            this.Height = drawingPanel.Height;

            
            //oteviraci tlacitko
            InitializeMenuButton();
            //sidemenu
            InitializeSideMenu();
        }
        private void InitializeMenuButton()
        {
            // menu button
            menuButton = new Button
            {
                Text = "<",
                Size = new Size(30, 50),
                ForeColor = Color.White,
                BackColor = Color.Gray,
                
            };
            
            menuButton.Click += MenuButton_Click;
            this.drawingPanel.Controls.Add(menuButton);
        }

        private void InitializeSideMenu()
        {
            // pozice sidemenu
            sideMenuPanel = new Panel
            {
                Size = new Size(200, this.Height),
                Location = new Point(this.Width - 200, 0),
                BackColor = Color.Black,
                Visible = false


            };
            this.drawingPanel.Controls.Add(sideMenuPanel);

            // ukaz vyber
            ShowMainOptions();

            // resize event
            this.drawingPanel.Resize += this.drawingPanel.DrawingPanel_Resize;
        }


        private void MenuButton_Click(object sender, EventArgs e)
        {
            // oteviraci a zaviraci tlacitko pozice text
            sideMenuPanel.Visible = !sideMenuPanel.Visible;
            if (sideMenuPanel.Visible)
            {
                menuButton.Location = new Point(sideMenuPanel.Location.X - menuButton.Width, menuButton.Location.Y);
                menuButton.Text =  ">";
            }
            else
            {
                menuButton.Location = new Point(drawingPanel.Width - menuButton.Width, menuButton.Location.Y);
                menuButton.Text = "<";
            }
        }

        

        private void ShowMainOptions()
        {
            // vymaz vyber
            sideMenuPanel.Controls.Clear();

            // obsah

            Label AuthorLabel = new Label
            {
                Text = "By: Jakub Strunc",
                Font = new Font("Arial", 7, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Bottom,
                Height = 50
            };
            sideMenuPanel.Controls.Add(AuthorLabel);

            // path button
            drawingPanel.showPaths = false;
            Button pathButton = new Button
            {
                Text = "Paths",
                Dock = DockStyle.Top,
                Height = 40,
                ForeColor = Color.White,
                BackColor = Color.Black,
                FlatStyle = FlatStyle.Flat
            };
            pathButton.Click += (sender, e) => loadPaths();
            sideMenuPanel.Controls.Add(pathButton);

            // graf button
            Button graphButton = new Button
            {
                Text = "Show Graph",
                Dock = DockStyle.Top,
                Height = 40,
                ForeColor = Color.White,
                BackColor = Color.Black,
                FlatStyle = FlatStyle.Flat
            };
            graphButton.Click += (sender, e) => loadGraph();
            sideMenuPanel.Controls.Add(graphButton);

            // objects button
            Button objectButton = new Button
            {
                Text = "Objects",
                Dock = DockStyle.Top,
                Height = 40,
                ForeColor = Color.White,
                BackColor = Color.Black,
                FlatStyle = FlatStyle.Flat
            };
            objectButton.Click += (sender, e) => ShowObjects();
            sideMenuPanel.Controls.Add(objectButton);

            // medes button
            Button modesButton = new Button
            {
                Text = "Modes",
                Dock = DockStyle.Top,
                Height = 40,
                ForeColor = Color.White,
                BackColor = Color.Black,
                FlatStyle = FlatStyle.Flat
            };
            modesButton.Click += (sender, e) => ShowModes();
            sideMenuPanel.Controls.Add(modesButton);

            //speed button
            Button speedButton = new Button
            {
                Text = "Speed",
                Dock = DockStyle.Top,
                Height = 40,
                ForeColor = Color.White,
                BackColor = Color.Black,
                FlatStyle = FlatStyle.Flat
            };
            speedButton.Click += (sender, e) => ShowSpeedOptions();
            sideMenuPanel.Controls.Add(speedButton);

            // scenario button
            Button scenarioButton = new Button
            {
                Text = "Scenarios",
                Dock = DockStyle.Top,
                Height = 40,
                ForeColor = Color.White,
                BackColor = Color.Black,
                FlatStyle = FlatStyle.Flat
            };
            scenarioButton.Click += (sender, e) => ShowScenarios();
            sideMenuPanel.Controls.Add(scenarioButton);

            // nazev label
            Label titleLabel = new Label
            {
                Text = "Options",
                Font = new Font("Arial", 14, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 50
            };
            sideMenuPanel.Controls.Add(titleLabel);
        }

        private void loadPaths()
        {
            //vycisti sidemenu
            sideMenuPanel.Controls.Clear();

            drawingPanel.showPaths = true;

            // back button
            Button backButton = new Button
            {
                Text = "Back",
                Dock = DockStyle.Top,
                Height = 40,
                ForeColor = Color.White,
                BackColor = Color.Black,
                FlatStyle = FlatStyle.Flat
            };
            backButton.Click += (sender, e) => ShowMainOptions();
            sideMenuPanel.Controls.Add(backButton);

            // reset button
            Button ResetButton = new Button
            {
                Text = "Reset Paths",
                Dock = DockStyle.Top,
                Height = 40,
                ForeColor = Color.White,
                BackColor = Color.Black,
                FlatStyle = FlatStyle.Flat
            };
            ResetButton.Click += (sender, e) => drawingPanel.resetPath();
            sideMenuPanel.Controls.Add(ResetButton);

            // nazeb label
            Label modesLabel = new Label
            {
                Text = "Modes",
                Font = new Font("Arial", 14, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 50
            };
            sideMenuPanel.Controls.Add(modesLabel);
        }

        //nacti graf
        private void loadGraph()
        {
            foreach(var probe in drawingPanel.probes)
            {
                if(probe.putIntoGraph)
                {
                    drawingPanel.probesgraph.Add(probe);
                }
            }
            Graph graph = new Graph(drawingPanel);
        }

        private void ShowModes()
        {
            // vycisti menu
            sideMenuPanel.Controls.Clear();

            // back button
            Button backButton = new Button
            {
                Text = "Back",
                Dock = DockStyle.Top,
                Height = 40,
                ForeColor = Color.White,
                BackColor = Color.Black,
                FlatStyle = FlatStyle.Flat
            };
            backButton.Click += (sender, e) => ShowMainOptions();
            sideMenuPanel.Controls.Add(backButton);

            // levelline checkbox
            CheckBox LevelLinesCheckbox = new CheckBox
            {
                Text = "Level Lines",
                Dock = DockStyle.Top,
                ForeColor = Color.White,
                BackColor = Color.Black,
                Checked = this.drawingPanel.showLevelLines,
                Height = 30
            };
            LevelLinesCheckbox.CheckedChanged += (sender, e) => this.drawingPanel.showLevelLines = LevelLinesCheckbox.Checked;
            sideMenuPanel.Controls.Add(LevelLinesCheckbox);

            // fieldlines checkbox
            CheckBox FieldLinesCheckbox = new CheckBox
            {
                Text = "Field Lines",
                Dock = DockStyle.Top,
                ForeColor = Color.White,
                BackColor = Color.Black,
                Checked = this.drawingPanel.showFieldLines,
                Height = 30
            };
            FieldLinesCheckbox.CheckedChanged += (sender, e) => this.drawingPanel.showFieldLines = FieldLinesCheckbox.Checked;
            sideMenuPanel.Controls.Add(FieldLinesCheckbox);

            // grid checkbox
            CheckBox gridCheckbox = new CheckBox
            {
                Text = "Grid",
                Dock = DockStyle.Top,
                ForeColor = Color.White,
                BackColor = Color.Black,
                Checked = this.drawingPanel.showGrid,
                Height = 30
            };
            gridCheckbox.CheckedChanged += (sender, e) => this.drawingPanel.showGrid = gridCheckbox.Checked;
            sideMenuPanel.Controls.Add(gridCheckbox);

            // arrows checkbox
            CheckBox arrowsCheckbox = new CheckBox
            {
                Text = "Arrows",
                Dock = DockStyle.Top,
                ForeColor = Color.White,
                BackColor = Color.Black,
                Checked = this.drawingPanel.showArrows,
                Height = 30
            };
            arrowsCheckbox.CheckedChanged += (sender, e) => this.drawingPanel.showArrows = arrowsCheckbox.Checked;
            sideMenuPanel.Controls.Add(arrowsCheckbox);

            // color map checkbox
            CheckBox colorMapCheckbox = new CheckBox
            {
                Text = "Color Map",
                Dock = DockStyle.Top,
                ForeColor = Color.White,
                BackColor = Color.Black,
                Checked = this.drawingPanel.showColorMap,
                Height = 30
            };
            colorMapCheckbox.CheckedChanged += (sender, e) => this.drawingPanel.showColorMap = colorMapCheckbox.Checked;
            sideMenuPanel.Controls.Add(colorMapCheckbox);

            // nazev label
            Label modesLabel = new Label
            {
                Text = "Modes",
                Font = new Font("Arial", 14, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 50
            };
            sideMenuPanel.Controls.Add(modesLabel);


        }

        private void ShowSpeedOptions()
        {
            // vycisti sidemenu
            sideMenuPanel.Controls.Clear();

            
            //back button
            Button backButton = new Button
            {
                Text = "Back",
                Dock = DockStyle.Top,
                Height = 40,
                ForeColor = Color.White,
                BackColor = Color.Black,
                FlatStyle = FlatStyle.Flat
            };
            backButton.Click += (sender, e) => ShowMainOptions();
            sideMenuPanel.Controls.Add(backButton);

            // hodnoty rychlosti
            float[] speedValues = { 0.5f, 1f, 1.5f, 2f, 3f };
            foreach (float speed in speedValues)
            {
                Button speedOptionButton = new Button
                {
                    Text = $"x{speed}",
                    Dock = DockStyle.Top,
                    Height = 40,
                    ForeColor = Color.White,
                    BackColor = Color.Black,
                    FlatStyle = FlatStyle.Flat
                };
                speedOptionButton.Click += (sender, e) => this.drawingPanel.time_speed = 0.1 * speed;
                sideMenuPanel.Controls.Add(speedOptionButton);
            }

            // nazev label
            Label speedLabel = new Label
            {
                Text = "Speed Options",
                Font = new Font("Arial", 14, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 50
            };
            sideMenuPanel.Controls.Add(speedLabel);
        }

        private void ShowScenarios()
        {
            //vycisti side menu
            sideMenuPanel.Controls.Clear();

            //back button
            Button backButton = new Button
            {
                Text = "Back",
                Dock = DockStyle.Top,
                Height = 40,
                ForeColor = Color.White,
                BackColor = Color.Black,
                FlatStyle = FlatStyle.Flat
            };
            backButton.Click += (sender, e) => ShowMainOptions();
            sideMenuPanel.Controls.Add(backButton);

            //pridani souboru scenare
            Button addScenarioButton = new Button
            {
                Text = "Load Scenario",
                Dock = DockStyle.Top,
                Height = 40,
                ForeColor = Color.White,
                BackColor = Color.Black,
                FlatStyle = FlatStyle.Flat
            };
            addScenarioButton.Click += (s, e) => loadScenario();
            sideMenuPanel.Controls.Add(addScenarioButton);

            // nase scenare 
            for (int i = 4; i >= 0; i--)
            {
                int scenarioNumber = i;
                Button scenarioOption = new Button
                {
                    Text = "Scenario " + scenarioNumber,
                    Dock = DockStyle.Top,
                    Height = 40,
                    ForeColor = Color.White,
                    BackColor = Color.Black,
                    FlatStyle = FlatStyle.Flat
                };
                scenarioOption.Click += (s, e) => this.drawingPanel.ChangeScenario(scenarioNumber);
                sideMenuPanel.Controls.Add(scenarioOption);
            }

            // nazev label
            Label scenariosLabel = new Label
            {
                Text = "Scenarios",
                Font = new Font("Arial", 14, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 50
            };
            sideMenuPanel.Controls.Add(scenariosLabel);
        }

        private void loadScenario()
        {
            //otevri pruzkumnik souboru a filtruj podle json file
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "JSON Files (*.json)|*.json",
                Title = "Select JSON"
            };

            
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string jsonFilePath = openFileDialog.FileName;
                string jsonContent = File.ReadAllText(jsonFilePath);

                // dej mi data
                var chargesData = JsonConvert.DeserializeObject<List<ChargeData>>(jsonContent);

                // vymaz charge a pridej nove ze souboru
                drawingPanel.charges.Clear();
                foreach (var chargeData in chargesData)
                {

                        var charge = new Charge(chargeData.Q.ToString(), new Vector2(chargeData.PositionX * 100, chargeData.PositionY * 100), drawingPanel);
                        drawingPanel.charges.Add(charge);
                        
                }

                drawingPanel.Invalidate();
            }
        }

        private void ShowObjects()
        {
            //vycisti menu
            sideMenuPanel.Controls.Clear();

            //back button
            Button backButton = new Button
            {
                Text = "Back",
                Dock = DockStyle.Top,
                Height = 40,
                ForeColor = Color.White,
                BackColor = Color.Black,
                FlatStyle = FlatStyle.Flat
            };
            backButton.Click += (sender, e) => ShowMainOptions();
            sideMenuPanel.Controls.Add(backButton);

            
            // pridani pozitivniho charge
            Button addPositiveChargeButton = new Button
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Image = Image.FromFile("Images/positivecharge.png"),
                ImageAlign = ContentAlignment.MiddleCenter,
                Text = "",
            };
            addPositiveChargeButton.Click += (sender, e) => drawingPanel.charges.Add(new Charge("1.0", new Vector2(0, 0), this.drawingPanel));
            sideMenuPanel.Controls.Add(addPositiveChargeButton);

            // pridani zaporneho charge
            Button addNegativeChargeButton = new Button
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Image = Image.FromFile("Images/negativecharge.png"),
                ImageAlign = ContentAlignment.MiddleCenter,
                Text = "",
            };
            addNegativeChargeButton.Click += (sender, e) => drawingPanel.charges.Add(new Charge("-1.0", new Vector2(0, 0), this.drawingPanel));
            sideMenuPanel.Controls.Add(addNegativeChargeButton);
            
            //pridani sondy
            Button addProbeButton = new Button
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Image = Image.FromFile("Images/staticprobe.png"),
                ImageAlign = ContentAlignment.MiddleCenter,
                Text = "",
            };
            addProbeButton.Click += (sender, e) => drawingPanel.probes.Add(new Probe(new Vector2(0, 0), this.drawingPanel));
            sideMenuPanel.Controls.Add(addProbeButton);
            
            // nazev label
            Label titleLabel = new Label
            {
                Text = "Options",
                Font = new Font("Arial", 14, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 50
            };
            sideMenuPanel.Controls.Add(titleLabel);
        }



        


    }
}
