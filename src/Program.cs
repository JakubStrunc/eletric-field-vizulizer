using System.Diagnostics;
using System.Windows.Forms;

namespace UPG_SP_2024
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>


        [STAThread]
        static void Main(string[] args)
        {
            int scene = 0;
            int gridStepX = 50;
            int gridStepY = 50;

            if (args.Length > 0)
            {
                // prvni argument scena
                if (!int.TryParse(args[0], out scene))
                {
                    return; // neplatny format
                }

                // zpracovani rozlozeni sipek
                for (int i = 1; i < args.Length; i++)
                {
                    string arg = args[i];

                    if (arg.StartsWith("-g"))
                    {
                        
                        string gridInput = arg.Substring(2);
                        string[] gridValues = gridInput.Split('x');

                        if (gridValues.Length == 2 &&
                            int.TryParse(gridValues[0], out gridStepX) &&
                            int.TryParse(gridValues[1], out gridStepY))
                        {
                            continue; // uspech
                        }

                        return; // neplatny format mrizky
                    }

                    return; // neplatny argument
                }
            }

            Debug.WriteLine($"X{gridStepX} Y{gridStepY}");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm(scene, gridStepX, gridStepY));
        }

        /*static void Main()
        {
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }*/
    }
}