using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Kolera_Mtsk.Services
{
    /// <summary>
    /// Kucuk ekranlarda kesilen icerikler icin: formu gorev cubugunu koruyarak calisma alanina buyutur.
    /// </summary>
    public static class FormWorkspaceLayoutHelper
    {
        public static void ApplyWorkingAreaMaximized(Form form)
        {
            if (form == null || LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return;

            try
            {
                // MaximizedBounds Form'da protected; disaridan atanamaz. Calisma alani = gorev cubugu ustunu doldur.
                Rectangle wa = Screen.GetWorkingArea(form);
                form.WindowState = FormWindowState.Normal;
                form.StartPosition = FormStartPosition.Manual;
                form.Bounds = wa;
            }
            catch
            {
                try
                {
                    form.WindowState = FormWindowState.Maximized;
                }
                catch
                {
                    /* yoksay */
                }
            }
        }
    }
}
