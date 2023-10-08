using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace AudioConsoleApp
{
    public class RecordWav: IDisposable
    {
        WaveInEvent waveIn = new WaveInEvent();
        WaveFileWriter writer = null;
        string wavFile = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "test1.wav");
        string mp3File = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "test1.mp3");
        //public MemoryStream streamWav = new MemoryStream();
        //public MemoryStream streamMp3 = new MemoryStream();

        public RecordWav()
        {
            bool closing = false;

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
        }

        //public void WhriteToFile()
        //{
        //    writer = new WaveFileWriter(wavFile, waveIn.WaveFormat);
        //}

        //public void WhriteToSteam()
        //{
        //    writer = new WaveFileWriter(streamWav, waveIn.WaveFormat);
        //}

        public void Dispose()
        {
            writer?.Dispose();
            writer = null;
            waveIn.Dispose();
        }

        public void buttonRecord()
        {
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
            AudioVisualization.StartVisualization();
        }

        public void buttonStop()
        {
            waveIn.StopRecording();
        }

        public static void WaveToMP3(string waveFileName, string mp3FileName, int bitRate = 128)
        {
            //using (var reader = new AudioFileReader(waveFileName))
            //using (var writer = new LameMP3FileWriter(mp3FileName, reader.WaveFormat, bitRate))
            //    reader.CopyTo(writer);
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

        //public void SteamToMP3()
        //{
        //    //using (var reader = new AudioFileReader(waveFileName))
        //    //using (var writer = new LameMP3FileWriter(mp3FileName, reader.WaveFormat, bitRate))
        //    //    reader.CopyTo(writer);
        //    var test = streamWav.GetBuffer();
        //    using (var reader = new WaveFileReader(streamWav))
        //    {
        //        try
        //        {
        //            MediaFoundationEncoder.EncodeToMp3(reader, streamMp3);
        //        }
        //        catch (InvalidOperationException ex)
        //        {
        //            Console.WriteLine(ex.Message);
        //        }
        //    }
        //}
    }
}
