using Jint;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace DevinuxVSIXGenerator.Forms
{
    public partial class Generator : Form
    {
        public Generator(DevinuxEntityModel[] m)
        {
            InitializeComponent();
            this.Load += (s, e) => {
                LoadDb();
            };

            btnGenerate.Click += (s, e) =>
            {

                if (checkedListBox1.CheckedItems != null && checkedListBox1.CheckedItems.Count > 0)
                {
                    using (Jint.Engine eng = new Jint.Engine(cfg => { cfg.AllowClr(); }))
                    {
                        eng.SetValue("file", new Action<string, string>((string Path, string content) => { }));
                        foreach (GeneratorDBOperation item in checkedListBox1.CheckedItems)
                        {
                            foreach (DevinuxEntityModel model in m)
                            {
                                var jscode = $@"var model = {JsonConvert.SerializeObject(model)};
{item.ScriptBody}";
                                eng.Execute(jscode);
                            }
                        }
                    }
                }
                else
                {
                    "please select item".ShowMessage();
                }
            };
        }
        void LoadDb()
        {
            try
            {
                var pt = (Application.StartupPath + "//operator.json");
                if (System.IO.File.Exists(pt))
                {
                    var op = JsonConvert.DeserializeObject<GeneratorDB>(System.IO.File.ReadAllText(pt));
                    checkedListBox1.Items.Clear();
                    checkedListBox1.Items.AddRange(op.Operators.ToArray());
                }
                else
                {
                    "your db file has not found . please download before do it".ShowMessage();
                }
            }
            catch (Exception ex) { ex.ShowMessage(); }
        }
    }
    public class GeneratorDB { 
        public List<GeneratorDBOperation> Operators { set; get; }
    }
    public class GeneratorDBOperation {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { set; get; }
        public string Group { set; get; }
        public string Description { set; get; }
        public string ScriptBody { set; get; }

        public override string ToString()
        {
            return Group + " > " + Title;
        }
    }
}
