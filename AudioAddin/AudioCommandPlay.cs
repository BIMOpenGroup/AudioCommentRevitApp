namespace AudioComment.Addin
{
    using Autodesk.Revit.Attributes;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;
    using NAudio.Wave;
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;

    [Transaction(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class AudioCommandPlay : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                var uidoc = commandData.Application.ActiveUIDocument;
                var selection = commandData.Application.ActiveUIDocument.Selection;
                var selEmenets = selection.GetElementIds();
                ElementId elementid = null;
                if (selEmenets.Count == 0)
                {
                    return Result.Cancelled;
                }
                else if (selEmenets.Count > 0)
                {
                    elementid = selEmenets.First();
                }

                if (elementid != null)
                {
                    var element = uidoc.Document.GetElement(elementid);
                    string sound = AudioAddinApp._extStorageUtils.GetSoundMessage(uidoc.Document, elementid);
                    if (!string.IsNullOrEmpty(sound))
                    {
                        PlayFromByte(sound);
                    }
                    string usersList = AudioAddinApp._extStorageUtils.GetUsersList(uidoc.Document, elementid);
                    if (!string.IsNullOrEmpty(usersList))
                    {
                        var taskDialog = new TaskDialog("Список прослушавших пользователей");
                        taskDialog.MainContent = $"Прослушали: {usersList}";
                        taskDialog.Show();
                    }
                }
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("fail", ex.Message);
                return Result.Failed;
            }
        }

        public static void PlayFromByte(string sound)
        {
            byte[] bytesMp3 = Convert.FromBase64String(sound);
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