using System;
using System.Windows.Forms;

namespace Kolera_Mtsk.Sayfalar.EkProgramlar
{
    public partial class ProgressForm : Form
    {
        public ProgressForm()
        {
            InitializeComponent();
        }

        public void SetText(string text)
        {
            if (InvokeRequired) Invoke(new Action(() => lblDurum.Text = text));
            else lblDurum.Text = text;
        }

        public void SetProgress(int value)
        {
            int safe = Math.Max(0, Math.Min(100, value));
            if (InvokeRequired) Invoke(new Action(() => progressBar1.Value = safe));
            else progressBar1.Value = safe;
        }
    }
}
