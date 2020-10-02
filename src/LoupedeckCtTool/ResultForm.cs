namespace Vurdalakov.LoupedeckCtTool
{
    using System;
    using System.Windows.Forms;

    public partial class ResultForm : Form
    {
        public ResultForm(Boolean errorOccurred, Int32 numberOfLoupedeckNetworkAdaptersModified)
        {
            this.InitializeComponent();

            this.Text = errorOccurred ? "Error" : "Success";

            var errorMessage = errorOccurred ? "\r\n\r\nCheck the 'Show details' checkbox to get more information." : "";
            this.labelResult.Text = $"{numberOfLoupedeckNetworkAdaptersModified} Loupedeck CT/Live interface(s) modified.{errorMessage}";
        }
    }
}
