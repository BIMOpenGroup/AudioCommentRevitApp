using NAudio.Wave;
using System;
using System.IO;

namespace AudioComment.Addin
{
    public class RecordWav : IDisposable
    {
        WaveInEvent waveIn = new WaveInEvent();
        WaveFileWriter writer = null;
        string wavFile = Path.Combine("C:\\test\\soundTest", "test1.wav");
        string mp3File = Path.Combine("C:\\test\\soundTest", "test1.mp3");

        public RecordWav()
        {
            try
            {
                waveIn = new NAudio.Wave.WaveInEvent
                {
                    DeviceNumber = 0, // indicates which microphone to use new Mp3WaveFormat(sampleRate: 44100, channels: 1, blockSize: 1, bitRate: 32), 
                    WaveFormat = new NAudio.Wave.WaveFormat(rate: 44100, bits: 32, channels: 1),
                    BufferMilliseconds = 20
                };
                writer = new WaveFileWriter(wavFile, waveIn.WaveFormat);

                waveIn.RecordingStopped += (s, a) =>
                {
                    writer?.Flush();
                    writer?.Dispose();
                    WaveToMP3(wavFile, mp3File);
                };

                waveIn.DataAvailable += (s, a) =>
                {
                    writer.Write(a.Buffer, 0, a.BytesRecorded);
                    if (writer.Position > waveIn.WaveFormat.AverageBytesPerSecond * 30)
                    {
                        waveIn.StopRecording();
                    }
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
            //visualizator.StartVisualization();
        }

        public byte[] buttonStop()
        {
            waveIn.StopRecording();
            //visualizator.StopVisualization();
            return File.ReadAllBytes(mp3File);
            //SentToRevit();
        }

        public static void WaveToMP3(string waveFileName, string mp3FileName, int bitRate = 128)
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

        //private void SentToRevit()
        //{
        //    byte[] bytesMp3 = File.ReadAllBytes(mp3File);
        //    //Task.Factory.StartNew(async () =>  await pipeSender.SendByte(bytesMp3));
        //}
    }

}
