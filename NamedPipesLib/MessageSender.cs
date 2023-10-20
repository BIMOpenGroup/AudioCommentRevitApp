namespace AudioComment.NamedPipesLib
{
    using System;
    using System.IO;
    using System.IO.Pipes;
    using System.Threading;
    using System.Threading.Tasks;

    public class MessageSender
    {
        CancellationTokenSource cancelTokenSource;

        string _pipeName;
        public MessageSender(string pipeName)
        {
            _pipeName = pipeName;
            cancelTokenSource = new CancellationTokenSource(10000);
        }
        public async Task Send(string data)
        {
            try
            {
                cancelTokenSource = new CancellationTokenSource(10000);
                using (NamedPipeServerStream pipeServer = new NamedPipeServerStream(_pipeName, PipeDirection.Out, 10,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous))
                {
                    await pipeServer.WaitForConnectionAsync(cancelTokenSource.Token);
                    if (!cancelTokenSource.Token.IsCancellationRequested)
                    {
                        using (StreamWriter sw = new StreamWriter(pipeServer))
                        {
                            sw.AutoFlush = true;
                            sw.WriteLine(data);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void Stop()
        {
            cancelTokenSource.Cancel();
        }
    }
}

