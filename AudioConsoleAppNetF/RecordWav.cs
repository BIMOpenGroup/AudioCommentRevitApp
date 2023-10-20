namespace AudioComment.Console
{
    using NAudio.Wave;
    using System;
    using System.IO;

    public class RecordWav : IDisposable
    {
        WaveInEvent waveIn = new WaveInEvent();
        WaveFileWriter writer = null;
        string wavFile = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "test1.wav");
        string mp3File = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "test1.mp3");
        AudioVisualization visualizator;
        public RecordWav()
        {
            //bool closing = false;
            visualizator = new AudioVisualization();
            //pipeSender = new MessageSender("AudioCons");
            //_pipeClient = new PipeClient();
        }

        public void Dispose()
        {
            writer?.Dispose();
            writer = null;
            waveIn.Dispose();
        }

        public void buttonRecord()
        {
            waveIn = new NAudio.Wave.WaveInEvent
            {
                DeviceNumber = 0, // indicates which microphone to use
                WaveFormat = new NAudio.Wave.WaveFormat(rate: 44100, bits: 16, channels: 1),
                BufferMilliseconds = 20
            };

            waveIn.RecordingStopped += (s, a) =>
            {
                writer?.Flush();
                writer?.Dispose();
                WaveToMP3(wavFile, mp3File);
            };
            writer = new WaveFileWriter(wavFile, waveIn.WaveFormat);
            waveIn.DataAvailable += (s, a) =>
            {
                writer.Write(a.Buffer, 0, a.BytesRecorded);
                if (writer.Position > waveIn.WaveFormat.AverageBytesPerSecond * 30)
                {
                    waveIn.StopRecording();
                }
            };
            waveIn.StartRecording();
            visualizator.StartVisualization();
        }

        public byte[] buttonStop()
        {
            waveIn.StopRecording();
            visualizator.StopVisualization();
            return File.ReadAllBytes(mp3File);
            //SentToRevit();
        }

        public static void WaveToMP3(string waveFileName, string mp3FileName, int bitRate = 128)
        {

            using (var reader = new WaveFileReader(waveFileName))
            {
                try
                {
                    MediaFoundationEncoder.EncodeToMp3(reader, mp3FileName);
                    reader.Close();
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        //private void SentToRevit()
        //{
        //    byte[] bytesMp3 = File.ReadAllBytes(mp3File);
        //    //Task.Factory.StartNew(async () =>  await pipeSender.SendByte(bytesMp3));
        //}
    }
}
