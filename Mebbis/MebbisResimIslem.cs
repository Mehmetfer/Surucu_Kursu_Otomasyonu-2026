using Kolera.Mebbis;
using Kolera.Mebbis.Models;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Kolera_Mtsk.Mebbis
{
    public class MebbisResimIslem : MebbisIslemBase
    {
        public override string IslemAdi => "Resim İşlemi";

        private int _step = 0;
        private int _retry = 0;
        private DateTime _last = DateTime.MinValue;
        private string _tempImagePath;
        private readonly byte[] _resimData;
        private readonly string _preSavedImagePath;

        public MebbisResimIslem(WebBrowser webBrowser, MebbisKursiyerModel kursiyer, byte[] resimData = null, string preSavedImagePath = null)
            : base(webBrowser, kursiyer)
        {
            _resimData = resimData;
            _preSavedImagePath = preSavedImagePath;
        }

        public override void Baslat()
        {
            base.Baslat();
            _step = 0;
            _retry = 0;
            _last = DateTime.Now;
            _tempImagePath = (!string.IsNullOrWhiteSpace(_preSavedImagePath) && File.Exists(_preSavedImagePath))
                ? _preSavedImagePath
                : PrepareTempImage();
        }

        public override void Tick()
        {
            if (!Basladi || Tamamlandi) return;
            if (WebBrowser == null || WebBrowser.Document == null) return;
            if ((DateTime.Now - _last).TotalMilliseconds < 900) return;

            switch (_step)
            {
                case 0:
                    // Foto sayfasında file input hazır mı kontrol et
                    var fileInput = FindFileInput();
                    if (fileInput != null)
                    {
                        _retry = 0;
                        _step++;
                        _last = DateTime.Now;
                    }
                    else
                    {
                        _retry++;
                        if (_retry > 20) Tamamlandi = true;
                    }
                    break;

                case 1:
                    // flResimSec icin dosya diyaloğunu ac.
                    if (string.IsNullOrWhiteSpace(_tempImagePath) || !File.Exists(_tempImagePath))
                    {
                        Tamamlandi = true;
                        break;
                    }

                    try
                    {
                        var input = FindFileInput();
                        if (input == null)
                        {
                            _retry++;
                            if (_retry > 20) Tamamlandi = true;
                            break;
                        }

                        var frm = WebBrowser.FindForm();
                        frm?.Activate();
                        WebBrowser.Focus();
                        input.InvokeMember("click");

                        _step++;
                        _last = DateTime.Now;
                    }
                    catch
                    {
                        _retry++;
                        if (_retry > 20) Tamamlandi = true;
                    }
                    break;

                case 2:
                    // Diyalog acildiktan sonra tam yolu yaz ve Enter gonder.
                    try
                    {
                        var input = FindFileInput();
                        var frm = WebBrowser.FindForm();
                        frm?.Activate();
                        SendKeys.SendWait("\"" + _tempImagePath + "\"");
                        SendKeys.SendWait("{ENTER}");

                        // Kutuda tam yol + dosya adi gorunsun diye tekrar zorla.
                        TrySetFileInputValue(input, _tempImagePath);

                        TryTriggerPreviewFlow(input);
                        _step++;
                        _last = DateTime.Now;
                    }
                    catch
                    {
                        _retry++;
                        if (_retry > 20) Tamamlandi = true;
                    }
                    break;

                case 3:
                    // Yol gerçekten inputa yazılmış mı kontrol et.
                    try
                    {
                        var input = FindFileInput();
                        var value = input?.GetAttribute("value") ?? string.Empty;

                        if (string.IsNullOrWhiteSpace(value) || value.IndexOf("MEBAKTAR.JPG", StringComparison.OrdinalIgnoreCase) < 0)
                        {
                            _retry++;
                            if (_retry > 8)
                                Tamamlandi = true;
                            _last = DateTime.Now;
                            return;
                        }

                        _step++;
                        _last = DateTime.Now;
                    }
                    catch
                    {
                        _retry++;
                        if (_retry > 8) Tamamlandi = true;
                    }
                    break;

                case 4:
                    // Resim Ekle butonuna bas (id: btnGoster).
                    try
                    {
                        var hazirDiv = WebBrowser.Document?.GetElementById("hazir");
                        if (hazirDiv != null)
                            hazirDiv.Style = "visibility:visible";

                        var btn = FindUploadButton();
                        if (btn != null)
                        {
                            btn.InvokeMember("click");
                            Tamamlandi = true;
                            return;
                        }

                        // Son care: formu dogrudan submit et.
                        try
                        {
                            WebBrowser.Document?.InvokeScript("__doPostBack", new object[] { "btnGoster", string.Empty });
                            WebBrowser.Document?.Forms?[0]?.InvokeMember("submit");
                        }
                        catch { }
                    }
                    catch
                    {
                        // ignore
                    }

                    Tamamlandi = true;
                    break;
            }
        }

        private string PrepareTempImage()
        {
            try
            {
                var data = _resimData ?? Kursiyer?.Foto;
                if (data == null || data.Length == 0)
                {
                    MessageBox.Show("Aktarilacak resim verisi bos geldi.", "MEBBIS Resim Aktar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return null;
                }

                var folder = ResolveMebAktarFolder();

                var path = Path.Combine(folder, "MEBAKTAR.JPG");
                File.WriteAllBytes(path, data);
                return path;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Resim dosyasi olusturulamadi: " + ex.Message, "MEBBIS Resim Aktar", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private string ResolveMebAktarFolder()
        {
            // Kullanıcı isteği: sabit klasör kullan.
            const string fixedFolder = @"D:\Kolera_Mtsk\Kolera_Mtsk\AKTARILANLAR";
            Directory.CreateDirectory(fixedFolder);
            return fixedFolder;
        }

        private HtmlElement FindFileInput()
        {
            if (WebBrowser?.Document == null)
                return null;

            var byId = WebBrowser.Document.GetElementById("flResimSec");
            if (byId != null)
                return byId;

            foreach (HtmlElement input in WebBrowser.Document.GetElementsByTagName("input"))
            {
                var type = (input.GetAttribute("type") ?? string.Empty).Trim().ToLowerInvariant();
                if (type != "file")
                    continue;

                var id = (input.GetAttribute("id") ?? string.Empty).ToLowerInvariant();
                var name = (input.GetAttribute("name") ?? string.Empty).ToLowerInvariant();
                if (id.Contains("resim") || name.Contains("resim") || id.Contains("dosya") || name.Contains("dosya"))
                    return input;

                // Sayfada tek file input varsa onu kullan.
                return input;
            }

            return null;
        }

        private HtmlElement FindUploadButton()
        {
            if (WebBrowser?.Document == null)
                return null;

            var byId = WebBrowser.Document.GetElementById("btnGoster");
            if (byId != null)
                return byId;

            foreach (HtmlElement input in WebBrowser.Document.GetElementsByTagName("input"))
            {
                var type = (input.GetAttribute("type") ?? string.Empty).ToLowerInvariant();
                if (type != "button" && type != "submit" && type != "image")
                    continue;

                var id = (input.GetAttribute("id") ?? string.Empty).ToLowerInvariant();
                var name = (input.GetAttribute("name") ?? string.Empty).ToLowerInvariant();
                var value = (input.GetAttribute("value") ?? string.Empty).ToLowerInvariant();
                if (id.Contains("goster") || name.Contains("goster") || value.Contains("resim ekle"))
                    return input;
            }

            return null;
        }

        private void TryTriggerPreviewFlow(HtmlElement input)
        {
            if (WebBrowser?.Document == null || input == null)
                return;

            try
            {
                // onchange inline script'i var: preview_image(event)
                input.InvokeMember("onchange");
            }
            catch { }

            try
            {
                WebBrowser.Document.InvokeScript("eval", new object[]
                {
                    "try{" +
                    "var f=document.getElementById('flResimSec');" +
                    "if(f){if(typeof preview_image==='function'){preview_image({target:f});}}" +
                    "var hz=document.getElementById('hazir'); if(hz){hz.style.visibility='visible';}" +
                    "}catch(e){}"
                });
            }
            catch { }
        }

        private void TrySetFileInputValue(HtmlElement input, string path)
        {
            if (input == null || string.IsNullOrWhiteSpace(path))
                return;

            try
            {
                input.SetAttribute("value", path);
            }
            catch { }

            try
            {
                WebBrowser.Document?.InvokeScript("eval", new object[]
                {
                    "try{var f=document.getElementById('flResimSec'); if(f){f.value='" + EscapeJs(path) + "';}}catch(e){}"
                });
            }
            catch { }
        }

        private static string EscapeJs(string text)
        {
            return (text ?? string.Empty).Replace("\\", "\\\\").Replace("'", "\\'");
        }
    }
}