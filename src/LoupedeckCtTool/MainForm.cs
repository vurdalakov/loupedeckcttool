namespace Vurdalakov.LoupedeckCtTool
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Management;
    using System.Management.Automation;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    public partial class MainForm : Form
    {
        public MainForm()
        {
            this.InitializeComponent();
        }

        private void Log(String log)
        {
            this.textBoxLog.AppendText(log);
            this.textBoxLog.AppendText("\r\n");
        }

        private void ButtonConfigure_Click(Object sender, EventArgs e)
        {
            this.buttonConfigure.Enabled = false;
            this.textBoxLog.Text = "";

            var numberOfLoupedeckNetworkAdaptersModified = 0;
            var errorOccurred = false;

            Task.Factory.StartNew(() =>
            {
                var loupedeckMacAddresses = new List<String>();

                this.Log("--- Available network adapters:");

                try
                {
                    using (var searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_NetworkAdapter WHERE NetEnabled=True"))
                    {
                        foreach (ManagementObject queryObj in searcher.Get())
                        {
                            try
                            {
                                var macAddress = queryObj["MACAddress"] as String;
                                var interfaceIndex = (UInt32)queryObj["InterfaceIndex"];
                                var pnpDeviceId = queryObj["PNPDeviceID"] as String;
                                var description = queryObj["Description"] as String;
                                this.Log($"Interface: [{interfaceIndex:D2}] '{macAddress}' '{description}' '{pnpDeviceId}'");

                                if (!String.IsNullOrEmpty(macAddress) && !String.IsNullOrEmpty(pnpDeviceId) && pnpDeviceId.ContainsNoCase("VID_2EC2") && (pnpDeviceId.ContainsNoCase("PID_0003") || pnpDeviceId.ContainsNoCase("PID_0004")))
                                {
                                    loupedeckMacAddresses.Add(macAddress.ToUpper());
                                }
                            }
                            catch (Exception ex)
                            {
                                this.Log($"Error getting network adapter properties: {ex.Message}");
                                errorOccurred = true;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.Log($"Error enumerating network adapters: {ex.Message}");
                    errorOccurred = true;
                }

                this.Log($"{loupedeckMacAddresses.Count} Loupedeck interface(s) found");
                this.Log("");

                if (0 == loupedeckMacAddresses.Count)
                {
                    return;
                }

                this.Log("--- Old IP Connection Metric values:");

                LogConnectionMetrics();

                this.Log("");

                this.Log("--- Changing IP Connection Metric values:");

                try
                {
                    var newIpConnectionMetric = 255;

                    using (var searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_NetworkAdapterConfiguration"))
                    {
                        foreach (ManagementObject queryObj in searcher.Get())
                        {
                            try
                            {
                                var macAddress = queryObj["MACAddress"] as String;
                                var interfaceIndex = (UInt32)queryObj["InterfaceIndex"];
                                var oldIpConnectionMetric = (UInt32)queryObj["IPConnectionMetric"];

                                if (!String.IsNullOrEmpty(macAddress) && loupedeckMacAddresses.Contains(macAddress.ToUpper()))
                                {
                                    this.Log($"Interface: [{interfaceIndex:D2}] '{macAddress}': {oldIpConnectionMetric} -> {newIpConnectionMetric}");

                                    try
                                    {
                                        // Set-NetIPInterface -InterfaceIndex 27  -AutomaticMetric disabled -InterfaceMetric 99
                                        var powerShell = PowerShell.Create();
                                        powerShell.AddCommand("Set-NetIPInterface").AddParameter("InterfaceIndex", interfaceIndex).AddParameter("AutomaticMetric", "disabled").AddParameter("InterfaceMetric", newIpConnectionMetric);
                                        powerShell.Invoke();

                                        numberOfLoupedeckNetworkAdaptersModified++;
                                        newIpConnectionMetric++;

                                        this.Log("OK");
                                    }
                                    catch (Exception ex)
                                    {
                                        this.Log($"Error setting network adapter properties: {ex.Message}");
                                        errorOccurred = true;
                                    }
                                }
                            }
                            catch { }
                        }
                    }

                    this.Log($"{numberOfLoupedeckNetworkAdaptersModified} Loupedeck interface(s) modified");
                    this.Log("");

                    this.Log("--- New IP Connection Metric values:");

                    LogConnectionMetrics();

                    this.Log("");

                    this.Log("--- All done");
                }
                catch (Exception ex)
                {
                    this.Log($"Error verifying network adapters: {ex.Message}");
                    errorOccurred = true;
                }

                var resultForm = new ResultForm(errorOccurred, numberOfLoupedeckNetworkAdaptersModified);
                resultForm.ShowDialog(this);

                this.buttonConfigure.Enabled = true;
            });

            void LogConnectionMetrics()
            {
                using (var searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_NetworkAdapterConfiguration"))
                {
                    foreach (ManagementObject queryObj in searcher.Get())
                    {
                        try
                        {
                            var macAddress = queryObj["MACAddress"] as String;
                            var interfaceIndex = (UInt32)queryObj["InterfaceIndex"];
                            var ipConnectionMetric = (UInt32)queryObj["IPConnectionMetric"];
                            this.Log($"Interface: [{interfaceIndex:D2}] '{macAddress}': {ipConnectionMetric}");
                        }
                        catch { }
                    }
                }
            }
        }

        private void checkBoxShowDetails_CheckedChanged(Object sender, EventArgs e)
        {
            if (this.Height < 300)
            {
                this.Width = 1136;
                this.Height = 567;
            }
            else
            {
                this.Width = 815;
                this.Height = 133;
            }
        }

        private delegate void ExecuteThreadSafeDelegate(Action action);
        private ExecuteThreadSafeDelegate _executeThreadSafeDelegate = null;

        private void ExecuteThreadSafe(Action action)
        {
            if (this.InvokeRequired)
            {
                if (null == this._executeThreadSafeDelegate)
                {
                    this._executeThreadSafeDelegate = new ExecuteThreadSafeDelegate(this.ExecuteThreadSafe);
                }

                this.BeginInvoke(this._executeThreadSafeDelegate, new Object[] { action });
            }
            else
            {
                action.Invoke();
            }
        }

        private void linkLabelGithub_LinkClicked(Object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                var url = (sender as LinkLabel).Text.Substring(e.Link.Start, e.Link.Length);
                Process.Start(url);
            }
            catch { }
        }
    }
}
