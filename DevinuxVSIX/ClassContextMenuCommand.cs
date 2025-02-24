using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Linq;
using Task = System.Threading.Tasks.Task;

namespace DevinuxVSIX
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class ClassContextMenuCommand
    {
        public const int CommandId = 0x0100;
        public static readonly Guid CommandSet = new Guid("91a50506-7139-42b0-ac62-e378bc222b5a");
        private readonly AsyncPackage package;
        private ClassContextMenuCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }
        public static ClassContextMenuCommand Instance
        {
            get;
            private set;
        }
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in ClassContextMenuCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new ClassContextMenuCommand(package, commandService);
        }
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var dte = (EnvDTE.DTE)Package.GetGlobalService(typeof(EnvDTE.DTE));
            var selectedItem = GetSelectedItem(dte);

            if (selectedItem != null)
            {
                CreateFolders(selectedItem);
            }
        }
        private EnvDTE.ProjectItem GetSelectedItem(EnvDTE.DTE dte)
        {
            if (dte.SelectedItems.Count > 0)
            {
                return dte.SelectedItems.Item(1).ProjectItem;
            }
            return null;
        }
        private void CreateFolders(EnvDTE.ProjectItem selectedItem)
        {
            var folderNames = new[] { "Domain", "Apllication", "Persistance", "Infrastructure", "Common" };

            foreach (var folderName in folderNames.Where(m => m.Trim().Length > 0))
            {

                var folderPath = System.IO.Path.GetDirectoryName(selectedItem.FileNames[1]) + "\\" + folderName;
                if (!System.IO.Directory.Exists(folderPath))
                {
                    System.IO.Directory.CreateDirectory(folderPath);
                    selectedItem.ProjectItems.AddFromDirectory(folderPath);
                }
            }

            VsShellUtilities.ShowMessageBox(
                this.package,
                "Project folders created successfully!",
                "Create Folders",
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
    }
}
