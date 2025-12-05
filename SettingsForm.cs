using System;
using System.Drawing;
using System.Windows.Forms;

namespace ClipSpeak
{
    public class SettingsForm : Form
    {
        private TextBox txtApiUrl;

        private TextBox txtSpeed;
        private CheckBox chkEnabled;
        private Button btnSave;
        private Button btnCancel;

        public string ApiUrl => txtApiUrl.Text;
        public string Voice => cmbVoice.Text;
        public float Speed
        {
            get
            {
                if (float.TryParse(txtSpeed.Text, out float result))
                    return result;
                return 1.0f;
            }
        }
        public bool AppEnabled => chkEnabled.Checked;

        private TrackBar trkVolume;
        private Label lblVolumeValue;

        public float Volume => trkVolume.Value / 100.0f;

        private ComboBox cmbVoice;
        private Button btnRefreshVoices;
        private Label lblStatus;

        public SettingsForm(string currentUrl, string currentVoice, float currentSpeed, float currentVolume, bool enabled)
        {
            InitializeComponent();
            txtApiUrl.Text = currentUrl;
            cmbVoice.Text = currentVoice;
            txtSpeed.Text = currentSpeed.ToString();
            trkVolume.Value = (int)(currentVolume * 100);
            lblVolumeValue.Text = $"{trkVolume.Value}%";
            chkEnabled.Checked = enabled;
        }

        private void InitializeComponent()
        {
            this.Text = "ClipSpeak Settings";
            this.Size = new Size(450, 350);
            try 
            {
                this.Icon = new Icon("app.ico");
            }
            catch { }
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            var lblUrl = new Label { Text = "API URL:", Location = new Point(20, 20), AutoSize = true };
            txtApiUrl = new TextBox { Location = new Point(100, 18), Width = 260 };
            
            lblStatus = new Label { Text = "Not Connected", Location = new Point(370, 20), AutoSize = true, ForeColor = Color.Gray };

            var lblVoice = new Label { Text = "Voice:", Location = new Point(20, 60), AutoSize = true };
            cmbVoice = new ComboBox { Location = new Point(100, 58), Width = 220, DropDownStyle = ComboBoxStyle.DropDown }; // Allow typing
            btnRefreshVoices = new Button { Text = "↻", Location = new Point(330, 57), Width = 30, Height = 23 };
            btnRefreshVoices.Click += async (s, e) => await RefreshVoices();

            var lblSpeed = new Label { Text = "Speed:", Location = new Point(20, 100), AutoSize = true };
            txtSpeed = new TextBox { Location = new Point(100, 98), Width = 100 };

            var lblVolume = new Label { Text = "Volume:", Location = new Point(20, 140), AutoSize = true };
            trkVolume = new TrackBar { Location = new Point(100, 135), Width = 200, Minimum = 0, Maximum = 100, TickFrequency = 10 };
            lblVolumeValue = new Label { Text = "100%", Location = new Point(310, 140), AutoSize = true };
            
            trkVolume.Scroll += (s, e) => lblVolumeValue.Text = $"{trkVolume.Value}%";

            chkEnabled = new CheckBox { Text = "Enable Clipboard Monitoring", Location = new Point(100, 180), AutoSize = true };

            btnSave = new Button { Text = "Save", Location = new Point(190, 260), DialogResult = DialogResult.OK };
            btnCancel = new Button { Text = "Cancel", Location = new Point(280, 260), DialogResult = DialogResult.Cancel };

            this.Controls.Add(lblUrl);
            this.Controls.Add(txtApiUrl);
            this.Controls.Add(lblStatus);
            this.Controls.Add(lblVoice);
            this.Controls.Add(cmbVoice);
            this.Controls.Add(btnRefreshVoices);
            this.Controls.Add(lblSpeed);
            this.Controls.Add(txtSpeed);
            this.Controls.Add(lblVolume);
            this.Controls.Add(trkVolume);
            this.Controls.Add(lblVolumeValue);
            this.Controls.Add(chkEnabled);
            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);

            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;
        }

        private async System.Threading.Tasks.Task RefreshVoices()
        {
            lblStatus.Text = "Connecting...";
            lblStatus.ForeColor = Color.Orange;
            
            try 
            {
                var client = new TtsClient { ApiUrl = txtApiUrl.Text };
                var voices = await client.GetVoicesAsync();
                
                cmbVoice.Items.Clear();
                if (voices.Count > 0)
                {
                    cmbVoice.Items.AddRange(voices.ToArray());
                    lblStatus.Text = "Connected";
                    lblStatus.ForeColor = Color.Green;
                }
                else
                {
                    lblStatus.Text = "No Voices";
                    lblStatus.ForeColor = Color.Red;
                }
            }
            catch
            {
                lblStatus.Text = "Error";
                lblStatus.ForeColor = Color.Red;
            }
        }
    }
}
