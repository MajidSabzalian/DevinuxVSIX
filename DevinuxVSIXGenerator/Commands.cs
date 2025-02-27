using EnvDTE;
using System;
using System.Linq;

namespace DevinuxVSIXGenerator
{
    public class Commands : DevinuxCommand
    {
        public override int MenuCommandId { get => PackageIds.CreateDDDFolders; }
        private readonly DTE dte;
        public Commands(DTE _dte)
        {
            dte = _dte;
        }
        public override void OnClick(object sender, EventArgs e)
        {
            var folderNames = new[] { "Domain", "Apllication", "Persistance", "Infrastructure", "Common" };

            foreach (var folderName in folderNames.Where(m => m.Trim().Length > 0))
            {
                var selectedItem = dte.GetSelectedItem();
                var folderPath = System.IO.Path.GetDirectoryName(selectedItem.FileNames[1]) + "\\" + folderName;
                if (!System.IO.Directory.Exists(folderPath))
                {
                    System.IO.Directory.CreateDirectory(folderPath);
                    selectedItem.ProjectItems.AddFromDirectory(folderPath);
                }
            }
        }
    }
}