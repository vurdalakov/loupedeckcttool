namespace Vurdalakov.LoupedeckCtTool
{
    using System;
    using System.Windows.Forms;

    public partial class ResultForm : Form
    {
        public ResultForm(Boolean errorOccurred, String message)
        {
            this.InitializeComponent();

            this.Text = errorOccurred ? "Error" : "Success";

            this.labelResult.Text = message;
        }
    }
}
