using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ClipSpeak
{
    public class ClipboardMonitor : IDisposable
    {
        private ClipboardNotificationWindow _notificationWindow;
        public event EventHandler<string> ClipboardTextChanged;

        public ClipboardMonitor()
        {
            _notificationWindow = new ClipboardNotificationWindow(this);
        }

        private void OnClipboardUpdate()
        {
            try
            {
                if (Clipboard.ContainsText())
                {
                    string text = Clipboard.GetText();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        ClipboardTextChanged?.Invoke(this, text);
                    }
                }
            }
            catch (Exception ex)
            {
                // Clipboard access can sometimes fail if another app is holding it
                System.Diagnostics.Debug.WriteLine($"Clipboard access failed: {ex.Message}");
            }
        }

        public void Dispose()
        {
            _notificationWindow?.Dispose();
        }

        private class ClipboardNotificationWindow : NativeWindow, IDisposable
        {
            private const int WM_CLIPBOARDUPDATE = 0x031D;
            private readonly ClipboardMonitor _monitor;

            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool AddClipboardFormatListener(IntPtr hwnd);

            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

            public ClipboardNotificationWindow(ClipboardMonitor monitor)
            {
                _monitor = monitor;
                CreateHandle(new CreateParams());
                AddClipboardFormatListener(this.Handle);
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WM_CLIPBOARDUPDATE)
                {
                    _monitor.OnClipboardUpdate();
                }
                base.WndProc(ref m);
            }

            public void Dispose()
            {
                RemoveClipboardFormatListener(this.Handle);
                DestroyHandle();
            }
        }
    }
}
