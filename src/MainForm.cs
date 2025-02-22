using System.Drawing;
using System.Windows.Forms;

namespace UPG_SP_2024
{
    public partial class MainForm : Form
    {
        public MainForm(int scene, int gridStepX, int gridStepY)
        {
            
            this.WindowState = FormWindowState.Minimized; // ano ja vim je to fuj ale tohle drzi ze se mi menu hezky zobrazuje
            InitializeComponent(scene, gridStepX, gridStepY);
        }

        /*public MainForm()
        {
            InitializeComponent();
        }*/
    }
}
