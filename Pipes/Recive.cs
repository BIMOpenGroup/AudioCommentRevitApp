namespace AudioConsoleApp.Pipes
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Pipes;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class OnReciveEventArgs
    {
        public List<string> Data { get; set; }
        public string StringData { get; set; }
        public OnReciveEventArgs(List<string> data, string stringData)
        {
            Data = data;
            StringData = stringData;
        }
    }
    public class MessageReciver
    {
        CancellationTokenSource cancelTokenSource;

        public string _pipeName;

        public List<string> RecivedData { get; private set; }

        public delegate void OnReciveEventHandler(object sender, OnReciveEventArgs e);
        public event OnReciveEventHandler ReciveData;

        public MessageReciver(string pipeName)
        {
            _pipeName = pipeName;
            RecivedData = new List<string>();
            cancelTokenSource = new CancellationTokenSource();
        }

        public async Task Start()
        {
            while (!cancelTokenSource.Token.IsCancellationRequested)
            {
                await Recive();
            }
        }

        public void Stop()
        {
            cancelTokenSource.Cancel();
        }

        async Task Recive()
        {
            try
            {
                using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", _pipeName, PipeDirection.In))
                {
                    await pipeClient.ConnectAsync();
                    using (StreamReader sr = new StreamReader(pipeClient))
                    {
                        string fullString = sr.ReadToEnd();
                        if (fullString != null)
                        {
                            List<string> data = fullString.Split(' ').ToList();
                            ReciveData?.Invoke(this, new OnReciveEventArgs(data, fullString));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
