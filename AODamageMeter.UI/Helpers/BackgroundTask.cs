using System;
using System.Threading;
using System.Threading.Tasks;

namespace AODamageMeter.UI.Helpers
{
    public class BackgroundTask
    {
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationToken _cancellationToken;
        private Task _task;

        public BackgroundTask(Action action, TimeSpan interval)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;
            _task = Task.Factory.StartNew(() =>
            {
                while (!_cancellationToken.IsCancellationRequested)
                {
                    action();
                    _cancellationToken.WaitHandle.WaitOne(interval);
                }
            }, _cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default)

        }
    }
}
