using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DevinuxVSIXGenerator.Forms
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Generator(
                new DevinuxEntityModel[1]{
                    new DevinuxEntityModel()
                    {
                        Name = "Class1",
                        Items = new List<DevinuxEntityModelProps>() {
                            new DevinuxEntityModelProps(){  TypeName="string" , Name ="Name"} ,
                            new DevinuxEntityModelProps(){  TypeName="int" , Name ="Id"}
                        }
                    }
                },
                "MyRefrences", "c:/", "c:/")
            );

            //Application.Run(new ConfigForm(new ConfigFile()));
        }
    }
}
