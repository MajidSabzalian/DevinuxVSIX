using DevinuxVSIXGenerator.Forms;
using EnvDTE;
using EnvDTE80;
using ICSharpCode.NRefactory.CSharp;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DevinuxVSIXGenerator
{
    

    public abstract class DevinuxCommand
    {
        public virtual int MenuCommandId { get; set; }
        public DTE dte;

        public DevinuxCommand() {
        }
        public DevinuxCommand(int commandId)
        {
            MenuCommandId = commandId;
        }
        public virtual void OnClick(object sender, EventArgs e)
        {
        }

        public string GetSelectionText()
        {
            string selectedText = string.Empty;
            var activeDocument = dte.ActiveDocument;
            var textSelection = activeDocument.Selection as TextSelection;
            if (textSelection != null)
            {
                selectedText = textSelection.Text;
            }
            return selectedText;
        }

        public string GetActiveProjectFilePath()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var dte = Package.GetGlobalService(typeof(DTE)) as DTE2;
            if (dte == null)
                throw new InvalidOperationException("DTE service not available.");

            var activeSolutionProjects = dte.ActiveSolutionProjects as Array;
            if (activeSolutionProjects == null || activeSolutionProjects.Length == 0)
                throw new InvalidOperationException("No active project found.");

            var project = activeSolutionProjects.GetValue(0) as EnvDTE.Project;
            if (project == null)
                throw new InvalidOperationException("Failed to get the active project.");

            return project.FullName;
        }
        public string GetActiveProjectFolderPath()
        {
            return System.IO.Path.GetDirectoryName(GetActiveProjectFilePath());
        }
        public string GetActiveProjectNameSpace()
        {
            var dte = Package.GetGlobalService(typeof(DTE)) as DTE2;
            var activeSolutionProjects = dte.ActiveSolutionProjects as Array;
            var project = activeSolutionProjects.GetValue(0) as EnvDTE.Project;
            var vsp = project.Properties.Item("DefaultNamespace");
            return vsp.Value.ToString();
        }
        public List<DevinuxEntityModel> GetEntitiesFromClassDefinitions(string classDefinitions)
        {
            var syntaxTree = new CSharpParser().Parse(classDefinitions);
            var entities = new List<DevinuxEntityModel>();

            foreach (var type in syntaxTree.Descendants.OfType<TypeDeclaration>())
            {
                var e = new DevinuxEntityModel() { Name= type.Name };
                foreach (var member in type.Members)
                {
                    if (member is PropertyDeclaration property)
                    {
                        e.Items.Add(new DevinuxEntityModelProps() { TypeName = property.ReturnType.ToString(), Name = property.Name });
                    }
                }
                entities.Add(e);
            }
            return entities;
        }
    }
}
