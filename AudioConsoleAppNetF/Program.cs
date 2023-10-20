namespace AudioComment.Console
{
    using NAudio.Wave;
    using Spectre.Console;
    using System;
    using System.IO;
    using System.IO.Pipes;
    using System.Threading;
    using System.Threading.Tasks;

    public class AudioConsoleAppNetF
    {
        static string wavFile = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "test1.wav");
        static string mp3File = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "test1.mp3");

        static RecordWav recorder;
        static string _message = "0";
        static string pipeName = "AudioConsole1";

        static StreamWriter writer;
        static StreamReader reader;

        [Serializable]
        class SoundData
        {
            public int Id;
            public byte[] Sound;
            public string Text;
        }

        [STAThread]
        static public void Main(string[] args)
        {
            recorder = new RecordWav();
            Task.Factory.StartNew(() => StartServer());
            CreateMenu();
        }

        static async void CreateMenu()
        {
            while (true)
            {
                AnsiConsole.Clear();
                AnsiConsole.Markup($"[bold {Colors.mainColor}]AudioConsoleApp[/]\n");
                AnsiConsole.Markup($"message from Revit {_message}\n");
                Console.WriteLine("\n");
                var launchMode = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"[underline {Colors.mainColor}]Select action[/]\n")
                        .HighlightStyle(Colors.selectionStyle)
                        .PageSize(4)
                        .AddChoices(new[] { "Record", "Stop", "Play", "Quit" })
                    );
                if (launchMode == "Record")
                {
                    recorder.buttonRecord();
                }
                else if (launchMode == "Play")
                {
                    //var test = Recorder.streamMp3.GetBuffer();
                    //PlayFromByte();
                }
                else if (launchMode == "Stop")
                {
                    try
                    {
                        var sound = recorder.buttonStop();
                        _message = "Stop";
                        Task.Delay(1000).Wait();
                        _message = Convert.ToBase64String(sound);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                }
                else if (launchMode == "Quit")
                {
                    recorder.Dispose();
                    _message = "quit";
                    break;
                }
                else
                {
                    Console.WriteLine("no one will see this message if the program is working properly");
                }

            }
        }

        static void StartServer()
        {
            using (NamedPipeServerStream server = new NamedPipeServerStream(pipeName))
            {
                server.WaitForConnection();
                reader = new StreamReader(server);
                writer = new StreamWriter(server);
                while (true)
                {
                    if (!String.IsNullOrEmpty(_message))
                    {
                        writer.WriteLineAsync(_message);
                        writer.Flush();
                        _message = "";
                    }
                    //var line = reader.ReadLine();
                    //Console.WriteLine(line);
                    //string input = Console.ReadLine();
                    //if (String.IsNullOrEmpty(input)) break;
                    //writer.WriteLine(input);
                    //writer.Flush();
                }
            }
        }

        //[DllImport("user32.dll")]
        //static extern bool SetWindowPos(IntPtr hWnd,
        //                        IntPtr hWndInsertAfter,
        //                        int X,
        //                        int Y,
        //                        int cx,
        //                        int cy,
        //                        uint uFlags);
        //static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        //const uint SWP_NOSIZE = 0x0001, SWP_NOMOVE = 0x0002, SWP_SHOWWINDOW = 0x0040;

        //[DllImport("kernel32.dll")]
        //static extern IntPtr GetConsoleWindow();

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

        public static void PlayFromByte()
        {
            byte[] bytesMp3 = File.ReadAllBytes(mp3File);

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
