using System;

namespace DevinuxVSIXGenerator
{
    public class DevinuxCommand
    {
        public virtual int MenuCommandId { get; set; }

        public DevinuxCommand() { }
        public DevinuxCommand(int commandId)
        {
            MenuCommandId = commandId;
        }
        public virtual void OnClick(object sender, EventArgs e)
        {
        }
    }
}
