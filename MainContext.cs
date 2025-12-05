using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace ClipSpeak
{
    public class MainContext : ApplicationContext
    {
        private NotifyIcon _notifyIcon;
        private ClipboardMonitor _clipboardMonitor;
        private TtsClient _ttsClient;
        private AudioPlayer _audioPlayer;

        // Settings (In a real app, load/save to file)
        private string _apiUrl = "http://localhost:8880/v1/audio/speech";
        private string _voice = "af_bella";
        private float _speed = 1.0f;
        private bool _enabled = true;

        private float _volume = 1.0f;

        public MainContext()
        {
            // Initialize Components
            _clipboardMonitor = new ClipboardMonitor();
            _ttsClient = new TtsClient();
            _audioPlayer = new AudioPlayer();
            _audioPlayer.Volume = _volume;

            // Setup Tray Icon
            _notifyIcon = new NotifyIcon();
            try 
            {
                _notifyIcon.Icon = new Icon("app.ico");
            }
            catch
            {
                _notifyIcon.Icon = SystemIcons.Application; // Fallback
            }
            _notifyIcon.Text = "ClipSpeak";
            _notifyIcon.Visible = true;

            var contextMenu = new ContextMenuStrip();
            
            // Stop Speech
            contextMenu.Items.Add("Stop Speech", null, (s, e) => _audioPlayer.Stop());
            
            // Pause Monitoring
            var pauseItem = new ToolStripMenuItem("Pause Monitoring");
            pauseItem.CheckOnClick = true;
            pauseItem.Checked = !_enabled;
            pauseItem.Click += (s, e) => {
                _enabled = !pauseItem.Checked;
            };
            contextMenu.Items.Add(pauseItem);

            contextMenu.Items.Add("-");
            contextMenu.Items.Add("Settings", null, OnSettings);
            contextMenu.Items.Add("-");
            contextMenu.Items.Add("Exit", null, OnExit);
            _notifyIcon.ContextMenuStrip = contextMenu;
            _notifyIcon.DoubleClick += OnSettings;

            // Wire Events
            _clipboardMonitor.ClipboardTextChanged += OnClipboardTextChanged;
        }

        private async void OnClipboardTextChanged(object sender, string text)
        {
            if (!_enabled) return;

            // Basic filtering: ignore short text or URLs if desired. 
            // For now, just process everything.
            if (string.IsNullOrWhiteSpace(text)) return;

            try
            {
                _ttsClient.ApiUrl = _apiUrl;
                // Optional: Show a balloon tip or change icon to indicate processing
                // _notifyIcon.ShowBalloonTip(1000, "ClipSpeak", "Reading...", ToolTipIcon.Info);

                var audioStream = await _ttsClient.GetAudioAsync(text, _voice, _speed);
                _audioPlayer.Play(audioStream);
            }
            catch (Exception ex)
            {
                // _notifyIcon.ShowBalloonTip(3000, "ClipSpeak Error", ex.Message, ToolTipIcon.Error);
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
            }
        }

        private void OnSettings(object sender, EventArgs e)
        {
            using (var settingsForm = new SettingsForm(_apiUrl, _voice, _speed, _volume, _enabled))
            {
                if (settingsForm.ShowDialog() == DialogResult.OK)
                {
                    _apiUrl = settingsForm.ApiUrl;
                    _voice = settingsForm.Voice;
                    _speed = settingsForm.Speed;
                    _volume = settingsForm.Volume;
                    _enabled = settingsForm.AppEnabled;
                    
                    _audioPlayer.Volume = _volume;

                    // Update context menu state if needed
                    foreach (ToolStripItem item in _notifyIcon.ContextMenuStrip.Items)
                    {
                        if (item is ToolStripMenuItem menuItem && menuItem.Text == "Pause Monitoring")
                        {
                            menuItem.Checked = !_enabled;
                            break;
                        }
                    }
                }
            }
        }

        private void OnExit(object sender, EventArgs e)
        {
            _notifyIcon.Visible = false;
            _clipboardMonitor.Dispose();
            _audioPlayer.Dispose();
            Application.Exit();
        }
    }
}
