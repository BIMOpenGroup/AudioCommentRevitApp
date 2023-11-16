using Autodesk.Revit.DB.Visual;
using NAudio.Wave;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AudioComment.Addin
{
    public class RecordWav : IDisposable
    {
        WaveInEvent waveIn;
        WaveFileWriter writer;
        string tempPath = System.IO.Path.GetTempPath();
        string wavFile = Path.Combine("C:\\test\\soundTest", "test1.wav");
        string mp3File = Path.Combine("C:\\test\\soundTest", "test1.mp3");
        public Task waveToMP3Task;

        public void Create()
        {
            try
            {
                waveIn = new WaveInEvent
                {
                    DeviceNumber = 0,
                    WaveFormat = new WaveFormat(rate: 44100, bits: 32, channels: 1),
                    BufferMilliseconds = 20
                };
                writer = new WaveFileWriter(wavFile, waveIn.WaveFormat);
                waveToMP3Task = new Task(() =>
                {
                    WaveToMP3(wavFile, mp3File);
                });


                waveIn.DataAvailable += (s, a) =>
                {
                    writer.Write(a.Buffer, 0, a.BytesRecorded);
                    if (writer.Position > waveIn.WaveFormat.AverageBytesPerSecond * 30)
                    {
                        waveIn.StopRecording();
                    }
                };

                waveIn.RecordingStopped += (s, a) =>
                {
                    writer?.Flush();
                    writer?.Dispose();
                    waveToMP3Task.Start();
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void Dispose()
        {
            writer?.Dispose();
            writer = null;
            waveIn?.Dispose();
            waveIn = null;
        }

        public void buttonRecord()
        {
            waveIn.StartRecording();
        }

        public void buttonStop()
        {
            waveIn.StopRecording();
            //waveToMP3Task.Wait();
            //return File.ReadAllBytes(mp3File);
        }

        public byte[] GetBytes()
        {
            return File.ReadAllBytes(mp3File);
        }

        public static void WaveToMP3(string waveFileName, string mp3FileName)
        {

            using (var reader = new WaveFileReader(waveFileName))
            {
                try
                {
                    MediaFoundationEncoder.EncodeToMp3(reader, mp3FileName, 256000);
                    reader.Close();
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }

}
