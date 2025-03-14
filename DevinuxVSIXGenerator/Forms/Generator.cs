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

namespace DevinuxVSIXGenerator.Forms
{
    public partial class Generator : Form
    {
        public Generator(DevinuxEntityModel[] m, string defaultNameSpace , string currentProjectPath)
        {
            InitializeComponent();
            btnGenerate.Enabled = false;
            this.Load += (s, e) => {
                LoadDb();
                textBox1.Text = string.Join("\r\n", m.Select(x => x.GetCSharpCode()));
            };
            btnUpdate.Click += (s, e) => {
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
            btnOpenDatabase.Click += (s, e) => {
                try
                {
                    SelectFileInExplorer(pt);
                }
                catch (Exception ex) { ex.ShowMessage(); }
            };
            btnGenerate.Click += (s, e) =>
            {
                try
                {
                    if (checkedListBox1.CheckedItems != null && checkedListBox1.CheckedItems.Count > 0)
                    {
                        using (Jint.Engine eng = new Jint.Engine(cfg => { cfg.AllowClr(); cfg.CatchClrExceptions(); }))
                        {
                            eng.SetValue("file", new Action<string, string>((string Path, string content) =>
                            {
                                content.SaveFile(currentProjectPath + Path);
                            }));
                            foreach (Operator item in checkedListBox1.CheckedItems)
                            {
                                foreach (DevinuxEntityModel model in m)
                                {
                                    var jscode = $@"
var model = {JsonConvert.SerializeObject(model)};
var ns = '{defaultNameSpace}';

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
        private string pt = (Application.StartupPath + "//operator.xml");
        void SaveDb(string content)
        {
            System.IO.File.WriteAllText(pt, content, Encoding.UTF8);
            "save update database".ShowMessage();
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
                        checkedListBox1.Items.Clear();
                        checkedListBox1.Items.AddRange(op.Operator.ToArray());
                        btnGenerate.Enabled = true;

                    }
                }
                else
                {
                    btnGenerate.Enabled = false;
                    "your db file has not found . please download before do it".ShowMessage();
                }
            }
            catch (Exception ex) { ex.ShowMessage(); }
        }
    }
    [XmlRoot(ElementName = "operator")]
    public class Operator
    {

        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }

        [XmlAttribute(AttributeName = "title")]
        public string Title { get; set; }

        [XmlAttribute(AttributeName = "group")]
        public string Group { get; set; }

        [XmlAttribute(AttributeName = "description")]
        public string Description { get; set; }

        [XmlText]
        public string Text { get; set; }

        public override string ToString()
        {
            return $"{Group} > {Title}";
        }
    }

    [XmlRoot(ElementName = "database")]
    public class Database
    {

        [XmlElement(ElementName = "operator")]
        public List<Operator> Operator { get; set; }
    }
}
