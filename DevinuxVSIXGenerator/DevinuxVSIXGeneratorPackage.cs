using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace DevinuxVSIXGenerator
{
    
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(DevinuxVSIXGeneratorPackage.PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class DevinuxVSIXGeneratorPackage : AsyncPackage
    {

        public static void EnsureAssembliesLoaded()
        {
            
        }



        public const string PackageGuidString = PackageGuids.guidDevinuxVSIXGeneratorPackageString;
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            var dte = (EnvDTE.DTE)Package.GetGlobalService(typeof(EnvDTE.DTE));
            List<DevinuxCommand> commands = new List<DevinuxCommand>();
            commands.Add(new CreateDddFoldersCommands(dte));
            commands.Add(new CreateApplicationServiceCommands(dte));
            commands.Add(new CreateApplicationModelCommands(dte));
            
            await InitialMenusAsync(commands);
        }
        private async Task InitialMenusAsync(List<DevinuxCommand> db)
        {
            var c = await GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            foreach (var i in db)
            {
                var commandID = new CommandID(PackageGuids.guidDevinuxVSIXGeneratorPackageCmdSet, i.MenuCommandId);
                var menuCommand = new OleMenuCommand( (s,e)=> { 
                    EnsureAssembliesLoaded();
                    i.OnClick(s, e);
                } , commandID);
                c.AddCommand(menuCommand);
            }
        }
        
        public void ShowMessage(string message)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            VsShellUtilities.ShowMessageBox(
                this,
                message,
                "TextEditorContextMenuExtension",
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
    }

    public static class Ext
    {
        public static void ShowMessage(this Exception ex)
        {
            System.Windows.Forms.MessageBox.Show(ex.Message);
        }
        public static void ShowMessage(this string ex)
        {
            System.Windows.Forms.MessageBox.Show(ex);
        }
        public static EnvDTE.ProjectItem GetSelectedItem(this EnvDTE.DTE dte)
        {
            if (dte.SelectedItems.Count > 0)
            {
                return dte.SelectedItems.Item(1).ProjectItem;
            }
            return null;
        }
        public static string StringJoin(this string[] arr , string seprator = ",")
        {
            return string.Join(seprator, arr);
        }
        public static void SaveFile(this string Content , string path)
        {
            var dp = System.IO.Path.GetDirectoryName(path);
            if (!System.IO.Directory.Exists(dp)) System.IO.Directory.CreateDirectory(dp);
            System.IO.File.WriteAllText(path, Content, Encoding.UTF8);
        }
        public static void SaveFile(this byte[] Content, string path)
        {
            System.IO.File.WriteAllBytes(path, Encoding.UTF8.GetPreamble().Concat(Content.ToArray()).ToArray());
        }
    }
}