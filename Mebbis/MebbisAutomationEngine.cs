using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Kolera_Mtsk.Mebbis
{
    public class MebbisAutomationEngine
    {
        private readonly WebBrowser _wb;
        private readonly Timer _timer;
        private IMebbisIslem _current;
        private readonly Queue<IMebbisIslem> _queue = new Queue<IMebbisIslem>();
        public bool IsRunning { get; private set; }
        public event Action SequenceCompleted;

        public MebbisAutomationEngine(WebBrowser wb)
        {
            _wb = wb;

            _timer = new Timer();
            _timer.Interval = 1000;
            _timer.Tick += Tick;
        }

        public void Run(IMebbisIslem islem)
        {
            _queue.Clear();
            _current = islem;
            _current.Baslat();
            IsRunning = true;

            if (!_timer.Enabled)
                _timer.Start();
        }

        public void RunSequence(params IMebbisIslem[] islemler)
        {
            _queue.Clear();

            if (islemler == null || islemler.Length == 0)
                return;

            foreach (var islem in islemler)
            {
                if (islem != null)
                    _queue.Enqueue(islem);
            }

            if (_queue.Count == 0)
                return;

            _current = _queue.Dequeue();
            _current.Baslat();
            IsRunning = true;

            if (!_timer.Enabled)
                _timer.Start();
        }

        public void Stop()
        {
            _queue.Clear();
            _current = null;
            _timer.Stop();
            IsRunning = false;
            SequenceCompleted?.Invoke();
        }

        private void Tick(object sender, EventArgs e)
        {
            if (_current == null) return;

            _current.Tick();

            if (_current.Tamamlandi)
            {
                if (_queue.Count > 0)
                {
                    _current = _queue.Dequeue();
                    _current.Baslat();
                    return;
                }

                _timer.Stop();
                IsRunning = false;
                // Adım-adım kullanımda popup kesintisi web akışını bozabildiği için sessizce bitir.
                SequenceCompleted?.Invoke();
            }
        }
    }
}