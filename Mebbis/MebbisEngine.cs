using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Kolera.Mebbis;
namespace Kolera_Mtsk.Mebbis
{
    public class MebbisEngine
    {
        public WebBrowser Browser { get; private set; }

        private Queue<Action> _steps = new Queue<Action>();
        private bool _waitingPage = false;

        public MebbisEngine(WebBrowser browser)
        {
            Browser = browser;
            Browser.DocumentCompleted += DocumentCompleted;
        }

        public void Run(Action action)
        {
            _steps.Enqueue(action);
            Next();
        }

        public void Next()
        {
            if (_waitingPage) return;
            if (_steps.Count == 0) return;

            var step = _steps.Dequeue();
            step.Invoke();
        }

        // ✅ EKLEDİĞİMİZ KRİTİK METOD
        public void WaitPage()
        {
            _waitingPage = true;
        }

        private void DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            _waitingPage = false;
            Next();
        }
    }
}