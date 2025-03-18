using Jint;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace DevinuxVSIXGenerator.Forms
{
    public partial class Generator : Form
    {
        private string configPath = "";
        private ConfigFile BaseConfigs = new ConfigFile();
        public Generator(DevinuxEntityModel[] m, string defaultNameSpace, string currentProjectPath, string devinuxConfigPath)
        {
            configPath = devinuxConfigPath + "\\config.json";
            InitializeComponent();
            mnuGENERATE.Enabled = false;
            ImageList img = new ImageList() { ColorDepth = ColorDepth.Depth32Bit };
            img.Images.Add(Properties.Resources.folder);
            img.Images.Add(Properties.Resources.accept);
            img.Images.Add(Properties.Resources.add);
            treeView1.ImageList = img;
            this.Load += (s, e) =>
            {
                LoadDb();
                LoadConfig();
                textBox1.Text = string.Join("\r\n", m.Select(x => x.GetCSharpCode()));

                // ChangeConfig();
            };
            mnuUPDATE.Click += (s, e) =>
            {
                try
                {
                    using (WebClient w = new WebClient())
                    {
                        w.Encoding = Encoding.UTF8;
                        var str = w.DownloadString("https://github.com/MajidSabzalian/DevinuxVSIX/raw/refs/heads/master/Database.xml");
                        if (str.StartsWith("<?xml"))
                        {
                            SaveDb(str);
                            LoadDb();
                        }
                        else
                        {
                            new Exception(str).ShowMessage();
                        }
                    }
                }
                catch (Exception ex) { ex.ShowMessage(); }
            };
            mnuCONFIGURATION.Click += (s, e) =>
            {
                try
                {
                    ChangeConfig();
                }
                catch (Exception ex) { ex.ShowMessage(); }
            };
            mnuOPENDATABASEDirectory.Click += (s, e) =>
            {
                try
                {
                    SelectFileInExplorer(pt);
                }
                catch (Exception ex) { ex.ShowMessage(); }
            };
            mnuGENERATE.Click += (s, e) =>
            {
                try
                {
                    var n = GetCheckedNodes();
                    if (n != null && n.Count > 0)
                    {
                        using (Jint.Engine eng = new Jint.Engine(cfg => { cfg.AllowClr(); cfg.CatchClrExceptions(); }))
                        {
                            eng.SetValue("_file", new Action<string,string, string>((string basepath , string Path, string content) =>
                            {
                                var bp = "";
                                switch (basepath) { 
                                    case "application": bp= BaseConfigs.Application; break; 
                                    case "common": bp = BaseConfigs.Common; break; 
                                    case "persistence": bp = BaseConfigs.Persistence; break; 
                                    case "domain": bp = BaseConfigs.Domain; break; 
                                    case "endpoint": bp = BaseConfigs.Endpoint; break; 
                                    case "ui": bp = BaseConfigs.UI; break;
                                    case "default": bp = currentProjectPath; break;
                                };
                                content.SaveFile(bp + Path);
                            }));
                            foreach (Operator item in n.Where(x=>x.Checked == true).Select(x=>(x.Tag as Operator)))
                            {
                                foreach (DevinuxEntityModel model in m)
                                {
                                    var jscode = $@"
var basepath = '{item.Base.ToLower().Trim()}';
var model = {JsonConvert.SerializeObject(model)};
var ns = '{defaultNameSpace}';
var file = (path , content) => _file(basepath , path , content);
{item.Text}

";
                                    eng.Execute(jscode);
                                }
                            }
                        }
                        "done".ShowMessage();
                    }
                    else
                    {
                        "please select item".ShowMessage();
                    }
                }
                catch (Exception ex) { ex.ShowMessage(); }
            };
        }
        private static void SelectFileInExplorer(string filePath) => Process.Start("explorer.exe", $@"/select,""{filePath}""");
        private string pt = (Application.StartupPath + "\\operator.xml");
        void SaveDb(string content)
        {
            System.IO.File.WriteAllText(pt, content, Encoding.UTF8);
            "save update database".ShowMessage();
        }
        private List<TreeNode> GetCheckedNodes()
        {
            return GetCheckedNodes(treeView1.Nodes).Where(m => m.Tag != null).ToList();
        }
        private List<TreeNode> GetCheckedNodes(TreeNodeCollection nodes)
        {
            List<TreeNode> checkedNodes = new List<TreeNode>();
            if (nodes == null || nodes.Count == 0) return checkedNodes;
            foreach (TreeNode node in nodes)
            {
                if (node.Checked) { checkedNodes.Add(node); }
                checkedNodes.AddRange(GetCheckedNodes(node.Nodes));
            }
            return checkedNodes;
        }
        void LoadDb()
        {
            try
            {
                if (System.IO.File.Exists(pt))
                {
                    var f = new System.IO.FileInfo(pt);
                    label1.Text = $"Last Writed Database File : {f.LastWriteTime}";
                    XmlSerializer serializer = new XmlSerializer(typeof(Database));
                    using (StringReader reader = new StringReader(System.IO.File.ReadAllText(pt)))
                    {
                        var op = (Database)serializer.Deserialize(reader);
                        treeView1.Nodes.Clear();
                        var allnodes = op.Operator.GroupBy(m => m.Group).Select(m => {
                            var t = new TreeNode(m.Key, 0,0);
                            t.Nodes.AddRange(m.Select(x => new TreeNode(x.Title,1,1) { Tag = x }).ToArray());
                            return t;
                        }).ToArray();
                        treeView1.Nodes.AddRange(allnodes);
                        mnuGENERATE.Enabled = true;

                    }
                }
                else
                {
                    mnuGENERATE.Enabled = false;
                    "your db file has not found . please download before do it".ShowMessage();
                }
            }
            catch (Exception ex) { ex.ShowMessage(); }
        }

        void SaveConfig()
        {
            try
            {
                System.IO.File.WriteAllText(configPath, JsonConvert.SerializeObject(BaseConfigs));
            }
            catch (Exception ex) { ex.ShowMessage(); }
        }
        void LoadConfig()
        {
            try
            {
                if (System.IO.File.Exists(configPath))
                {
                    BaseConfigs = JsonConvert.DeserializeObject<ConfigFile>(System.IO.File.ReadAllText(configPath));
                }
            }
            catch (Exception ex) { ex.ShowMessage(); }
        }

        void ChangeConfig()
        {
                using (var optionconfig = new Form()
                {
                    StartPosition = FormStartPosition.CenterParent,
                    Width = 400,
                    Height = 350,
                    FormBorderStyle = FormBorderStyle.Sizable,
                    MinimizeBox = false , 
                    MaximizeBox = false,
                    Text = "ReConfig..."
                })
                {
                    PropertyGrid pg = new PropertyGrid() { Dock = DockStyle.Fill };
                    pg.SelectedObject = BaseConfigs;
                    optionconfig.Controls.Add(pg);
                    optionconfig.ShowDialog();
                    {
                        SaveConfig();
                    }
                }
        }
    }
}
