using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace DevinuxVSIXGenerator.Forms
{
    [Serializable]
    public class ConfigFile {
        [Browsable(true)]
        [EditorAttribute(typeof(FolderNameEditor2), typeof(System.Drawing.Design.UITypeEditor))] 
        public string Application { set; get; }
        [Browsable(true)]
        [EditorAttribute(typeof(FolderNameEditor2), typeof(System.Drawing.Design.UITypeEditor))] 
        public string Domain { set; get; }
        [Browsable(true)]
        [EditorAttribute(typeof(FolderNameEditor2), typeof(System.Drawing.Design.UITypeEditor))] 
        public string Endpoint { set; get; }
        [Browsable(true)]
        [EditorAttribute(typeof(FolderNameEditor2), typeof(System.Drawing.Design.UITypeEditor))] 
        public string Persistence { set; get; }
        [Browsable(true)]
        [EditorAttribute(typeof(FolderNameEditor2), typeof(System.Drawing.Design.UITypeEditor))] 
        public string Common { set; get; }
        [Browsable(true)]
        [EditorAttribute(typeof(FolderNameEditor2), typeof(System.Drawing.Design.UITypeEditor))] 
        public string UI { set; get; }
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

        [XmlAttribute(AttributeName = "base")]
        public string Base { get; set; }

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
    public class DevinuxEntityModel
    {
        public string Name { set; get; }
        public List<DevinuxEntityModelProps> Items { set; get; } = new List<DevinuxEntityModelProps>();
        public string GetCSharpCode() => $@"public class {Name} 
{{
{(this.Items != null && this.Items.Count > 0 ? Items.Select(x => "    " + x.GetCSharpCode()).ToArray().StringJoin("\r\n") : "")}
}}";
    }
    public class DevinuxEntityModelProps
    {
        public string Name { set; get; }
        public string TypeName { set; get; }

        public string GetCSharpCode() => $"public {TypeName} {Name} {{ set; get; }}";
    }
}