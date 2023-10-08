using NAudio.CoreAudioApi;
using NAudio.Wave.SampleProviders;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioConsoleApp
{
    public class AudioDevice
    {
        public MMDevice Device { get; set; }
        public bool IsSelected { get; set; }
        public string FriendlyName { get; set; }
        private IWaveIn __Capture;
        public IWaveIn Capture
        {
            get
            {
                return __Capture;
            }
            set
            {
                if (__Capture != null)
                {
                    __Capture.DataAvailable -= this.__Capture_DataAvailable;
                }

                __Capture = value;

                if (__Capture != null)
                {
                    __Capture.DataAvailable += this.__Capture_DataAvailable;
                }
            }
        }

        private void __Capture_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (Buffer != null)
            {
                Buffer.AddSamples(e.Buffer, 0, e.BytesRecorded);
            }
        }

        public IWaveProvider Provider { get; set; }
        public BufferedWaveProvider Buffer { get; set; }

        public IWaveProvider Output { get; set; }

    }

    public class MainWindowDataModel
    {

        public ObservableCollection<AudioDevice> AudioDevices { get; private set; } = new ObservableCollection<AudioDevice>();

        public WaveFormat Format = new WaveFormat(16000, 8, 1);

        public MainWindowDataModel()
        {
            using (var Enumerator = new MMDeviceEnumerator())
            {
                var Devices = Enumerator.EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active).ToList();
                var Device = Devices[1];
                //foreach (var Device in Devices)
                //{

                    Device.AudioEndpointVolume.Mute = false;
                    var Capture =
                        Device.DataFlow == DataFlow.Capture
                        ? new WasapiCapture(Device)
                        : new WasapiLoopbackCapture(Device)
                        ;

                    var Provider = new WaveInProvider(Capture);
                    var Silence = new SilenceProvider(Capture.WaveFormat);
                    var Buffer = new BufferedWaveProvider(Capture.WaveFormat);

                    var Output = Device.DataFlow == DataFlow.Capture
                        ? new MixingWaveProvider32(new IWaveProvider[] { Provider })
                        : new MixingWaveProvider32(new IWaveProvider[] { Silence, Buffer })
                        ;


                    AudioDevices.Add(new AudioDevice()
                    {
                        Device = Device,
                        IsSelected = true,
                        FriendlyName = Device.FriendlyName,
                        Capture = Capture,
                        Provider = Provider,
                        Buffer = Buffer,
                        Output = Output,
                    });

                    Capture.StartRecording();

                //}

                var C = new MixingSampleProvider(from x in AudioDevices select x.Output.ToSampleProvider());
                var test = C.Take(TimeSpan.FromSeconds(5)).ToWaveProvider();
                Thread.Sleep(5000);
                //C.AddMixerInput(new SignalGenerator());
                var wavFile = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "test.wav");
                var mp3File = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "test.mp3");
                WaveFileWriter.CreateWaveFile(wavFile, test);
            }
        }
    }
}
