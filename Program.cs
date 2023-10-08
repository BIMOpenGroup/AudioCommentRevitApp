namespace AudioConsoleApp
{
    using global::AudioConsoleApp.Pipes;
    using NAudio.Wave;
    using Spectre.Console;
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Runtime.Intrinsics.Arm;
    using static System.Net.Mime.MediaTypeNames;

    public class AudioConsoleApp
    {
        static string wavFile = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "test1.wav");
        static string mp3File = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "test1.mp3");
        static bool dateRecived = false;
        static MessageReciver pipeReciver = new MessageReciver("AudioConsoleAppR");
        static MessageSender pipeSender = new MessageSender("AudioConsoleAppS");
        static string quitKey;
        static RecordWav Recorder;

        [STAThread]
        static public void Main(string[] args)
        {
            //pipeReciver = new MessageReciver("AudioConsoleApp");
            //pipeSender = new MessageSender("AudioConsoleApp");
            pipeReciver.ReciveData += OnReciveData;
            Task.Factory.StartNew(async () => await pipeReciver.Start());
            Recorder = new RecordWav();
            CreateMenu();
            IntPtr handle = GetConsoleWindow();
            SetWindowPos(handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);

            
            //test.buttonRecord();
            var test1 = true;
            while (test1)
            {
                quitKey = Console.ReadKey().Key.ToString().ToLower();
                if (quitKey == "q")
                {
                    test1 = false;
                    Thread.Sleep(2000);
                    Recorder.buttonStop();
                }
            }
            //test.buttonStop();
            Thread.Sleep(5000);


            //WaveToMP3(wavFile, mp3File);
            //Console.ReadLine();
            Recorder.Dispose();
        }

        static void CreateMenu()
        {
            while (true)
            {
                AnsiConsole.Clear();
                AnsiConsole.Markup($"[bold {Colors.mainColor}]RSNiniManager[/]\n");
                Console.WriteLine("run args:");
                Console.WriteLine("\n");
                var launchMode = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"[underline {Colors.mainColor}]Select launch mode[/]\n")
                        .HighlightStyle(Colors.selectionStyle)
                        .PageSize(4)
                        .AddChoices(new[] { "Record", "Play", "Stop", "Quit" })
                    );
                if (launchMode == "Record")
                {
                    //Recorder.WhriteToSteam();
                    Recorder.buttonRecord();
                }
                else if (launchMode == "Play")
                {
                    //var test = Recorder.streamMp3.GetBuffer();
                    PlayFromByte();
                }
                else if (launchMode == "Stop")
                {
                    Recorder.buttonStop();
                    quitKey = "q";
                }
                else if (launchMode == "Quit")
                {
                    break;
                }
                else
                {
                    Console.WriteLine("no one will see this message if the program is working properly");
                }
            }
        }

        static void OnReciveData(object sender, OnReciveEventArgs e)
        {
            dateRecived = true;
            quitKey = "q";
            pipeSender.Send("test");
            //Play();
            //Thread.Sleep(2000);
            PlayFromByte();
            Thread.Sleep(2000);
        }

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd,
                                IntPtr hWndInsertAfter,
                                int X,
                                int Y,
                                int cx,
                                int cy,
                                uint uFlags);
        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        const uint SWP_NOSIZE = 0x0001, SWP_NOMOVE = 0x0002, SWP_SHOWWINDOW = 0x0040;

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        public static void Play()
        {
            using (var audioFile = new AudioFileReader(mp3File))
            using (var outputDevice = new WaveOutEvent())
            {
                outputDevice.Init(audioFile);
                outputDevice.Play();
                while (outputDevice.PlaybackState == PlaybackState.Playing)
                {
                    Thread.Sleep(1000);
                }
            }
        }

        static Stream outputStream;

        public static void PlayFromByte()
        {
            //byte[] bytes = new byte[1024];
            byte[] bytesMp3= File.ReadAllBytes(mp3File);
            //byte[] bytesWav = File.ReadAllBytes(wavFile);

            //IWaveProvider provider = new RawSourceWaveStream(
            //                         new MemoryStream(bytes1), new WaveFormat());

            //using (var outputDevice = new WaveOutEvent())
            //{
            //    outputDevice.Init(provider);
            //    outputDevice.Play();
            //    while (outputDevice.PlaybackState == PlaybackState.Playing)
            //    {
            //        Thread.Sleep(1000);
            //    }
            //}

            //using (var fs = File.OpenRead(mp3File))
            //using (var rdr = new Mp3FileReader(fs))
            //using (var wavStream = WaveFormatConversionStream.CreatePcmStream(rdr))
            //using (var baStream = new BlockAlignReductionStream(wavStream))
            //using (var waveOut = new WaveOutEvent())
            //{
            //    waveOut.Init(baStream);
            //    waveOut.Play();
            //    while (waveOut.PlaybackState == PlaybackState.Playing)
            //    {
            //        Thread.Sleep(100);
            //    }
            //}

            var ms = new MemoryStream(bytesMp3);
            var rdr = new Mp3FileReader(ms);
            var wavStream = WaveFormatConversionStream.CreatePcmStream(rdr);
            var rs = new RawSourceWaveStream(wavStream, new WaveFormat(rate: 44100, bits: 16, channels: 1));
            var wo = new WaveOutEvent();
            wo.Init(rs);
            wo.Play();
            while (wo.PlaybackState == PlaybackState.Playing)
            {
                Thread.Sleep(500);
            }
            wo.Dispose();
        }
    }
}
