using System;
using System.Configuration;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Tabidus.POC.EncryptDecryptHelper;
using Tabidus.POC.GUI.ServiceReference;

namespace Tabidus.POC.GUI.Forms
{
    public partial class ConfigurationForm : Form
    {
        public ConfigurationForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handle when user click on Test configuration button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnTestConfiguration_Click(object sender, EventArgs e)
        {
            lblConnecting.Visible = true;
            btnSave.Enabled = false;
            btnTestConfiguration.Enabled = false;

            var connectionResult = await TestConnection();
            switch (connectionResult)
            {
                case 0:
                    MessageBox.Show(this, "Connect successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                case 1:
                    MessageBox.Show(this, string.Format("Server key '{0}' is invalid.", txtServerKey.Text),
                        "Server key", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
                default:
                    MessageBox.Show(this,
                        string.Format("Cannot connect to server '{0}:{1}'", txtServerAddress.Text, numServerPort.Value),
                        "Cannot connect", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
            }

            lblConnecting.Visible = false;
            btnSave.Enabled = true;
            btnTestConfiguration.Enabled = true;
        }

        /// <summary>
        /// Try to connect to POC Server
        /// </summary>
        /// <returns>
        /// <para>Return error code when connect to server.</para>
        /// <para>0 - success</para>
        /// <para>1 - server key error</para>
        /// <para>2 - connection error</para>
        /// </returns>
        private async Task<int> TestConnection()
        {
            if (string.IsNullOrEmpty(txtServerAddress.Text))
            {
                MessageBox.Show(this, "You must enter the server address", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
            if (string.IsNullOrEmpty(txtServerKey.Text))
            {
                MessageBox.Show(this, "You must enter the server key", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }

            try
            {
                var myBinding = new NetTcpBinding { Security = { Mode = SecurityMode.None } };
                var endpointAddress = new EndpointAddress(string.Format("net.tcp://{0}:{1}/POCService", txtServerAddress.Text, numServerPort.Value));
                var channelFactory = new ChannelFactory<IPOCService>(myBinding, endpointAddress);

                var pocServiceClient = channelFactory.CreateChannel(endpointAddress);
                if (pocServiceClient == null)
                    return 2;


                var r = await pocServiceClient.GetPagingDataAsync(EncryptionHelper.EncryptString("{\"Page\":0,\"Rows\":1}", txtServerKey.Text));
                return string.IsNullOrEmpty(r) ? 1 : 0;
            }
            catch
            {
                return 2;
            }
        }

        /// <summary>
        /// Handle when user click on Save button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnSave_Click(object sender, EventArgs e)
        {
            lblConnecting.Visible = true;
            btnSave.Enabled = false;
            btnTestConfiguration.Enabled = false;

            var connectionResult = await TestConnection();

            if (connectionResult == 1)
            {
                MessageBox.Show(this, string.Format("Server key '{0}' is invalid.", txtServerKey.Text),
                    "Server key", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (connectionResult == 2)
            {
                if (
                    MessageBox.Show(this,
                        string.Format("Cannot connect to server '{0}:{1}'. Do you want to continue?",
                            txtServerAddress.Text, numServerPort.Value), "Cannot connect", MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.OK)
                    connectionResult = 0;
            }

            if (connectionResult == 0)
            {
                SaveConfiguration();
                Application.Exit();

                //MainWindow mypage = new MainWindow();
                //mypage.Show();
            }
            else
            {
                lblConnecting.Visible = false;
                btnSave.Enabled = true;
                btnTestConfiguration.Enabled = true;
            }
        }

        /// <summary>
        /// Save configuration to file
        /// </summary>
        private void SaveConfiguration()
        {
            var uri = string.Format("net.tcp://{0}:{1}/POCService", txtServerAddress.Text, numServerPort.Value);

            var config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);
            var systemServiceModalSectionGroups = config.SectionGroups["system.serviceModel"];
            

            var clientSectionInformation = systemServiceModalSectionGroups.Sections["client"].SectionInformation;
            var xmlText = clientSectionInformation.GetRawXml();
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlText);

            var endpointNode = xmlDoc.SelectSingleNode("/client/endpoint[@name='NetTcpBinding_IPOCService']");
            endpointNode.Attributes["address"].Value = uri;

            clientSectionInformation.SetRawXml(xmlDoc.OuterXml);

            config.AppSettings.Settings.Remove("MESSAGE_KEY");
            config.AppSettings.Settings.Add("MESSAGE_KEY", txtServerKey.Text);


            config.Save(ConfigurationSaveMode.Modified);
        }

        private void ConfigurationForm_Load(object sender, EventArgs e)
        {
            Activate();
        }

        private void txtServerAddress_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
