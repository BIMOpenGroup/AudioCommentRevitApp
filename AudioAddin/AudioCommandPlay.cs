namespace AudioComment.Addin
{
    using Autodesk.Revit.Attributes;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;
    using Autodesk.Revit.UI.Selection;
    using Autodesk.Revit.DB.ExtensibleStorage;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Collections.Generic;
    using NAudio.Wave;
    using System.Threading;

    [Transaction(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class AudioCommandPlay : IExternalCommand
    {
        static string consoleApp = "C:\\code\\AudioCommentRevitAppConsole\\AudioConsoleAppNetF\\bin\\Debug\\AudioConsoleAppNetF.exe";
        //static TaskDialog taskDiag;
        static Element SelectElement;
        //static Schema schema;
        static byte[] soundData;
        static string pipeName = "AudioConsole2";
        static string _message = "000";

        static StreamWriter writer;
        static StreamReader reader;

        [Serializable]
        public class SoundData
        {
            public int Id;
            public byte[] Sound;
            public string Text;
        }

        private static StringBuilder sortOutput = null;
        private static int numOutputLines = 0;
        private static bool isCMDactive = true;

        [DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

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
                    //while (elementid == null)
                    //{
                    //    var taskDialog = new TaskDialog("Необходимо выбрать элементы");
                    //    taskDialog.CommonButtons = TaskDialogCommonButtons.Yes;
                    //    taskDialog.MainContent = "Выберите элемент для записи";
                    //    var selectResult = taskDialog.Show();
                    //    if (selectResult == TaskDialogResult.Yes)
                    //    {
                    //        elementid = uidoc.Selection.PickObject(ObjectType.Element, "Select element or ESC to reset the view").ElementId;
                    //    }
                    //    else if (selectResult == TaskDialogResult.Close || selectResult == TaskDialogResult.Cancel)
                    //    {
                    //        return Result.Cancelled;
                    //    }
                    //}
                }
                else if (selEmenets.Count > 0)
                {
                    elementid = selEmenets.First();
                }

                if (elementid != null) 
                {
                    using (var recorder = new RecordWav())
                    {
                        var element = uidoc.Document.GetElement(elementid);
                        byte[] sound = GetSoundDataInElement(element);
                        PlayFromByte(sound);
                    }
                    //    byte[] sound = null;
                    //    var taskDialog = new TaskDialog("Запись звука");
                    //    taskDialog.CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No;
                    //    taskDialog.MainContent = "Включить запись звука?";
                    //    var startSoundResult = taskDialog.Show();
                    //    if (startSoundResult == TaskDialogResult.Yes)
                    //    {
                    //        recorder.buttonRecord();
                    //        var taskDialog2 = new TaskDialog("Запись звука");
                    //        taskDialog2.CommonButtons = TaskDialogCommonButtons.Ok | TaskDialogCommonButtons.Cancel;
                    //        taskDialog2.MainContent = "Выключить запись";
                    //        var stopSoundResult = taskDialog2.Show();
                    //        if (stopSoundResult == TaskDialogResult.Ok | stopSoundResult == TaskDialogResult.Cancel)
                    //        {
                    //            sound = recorder.buttonStop();
                    //        }
                    //        else
                    //        {
                    //            return Result.Cancelled;
                    //        }
                    //        if (sound != null)
                    //        {
                    //            var element = uidoc.Document.GetElement(elementid);
                    //            StoreSoundDataInElement(element, sound);
                    //        }
                    //    }
                    //    else if (startSoundResult == TaskDialogResult.Close || startSoundResult == TaskDialogResult.Cancel)
                    //    {
                    //        return Result.Cancelled;
                    //    }
                    //};
                }

                //RunClient(consoleApp);

                //Process[] test0 = Process.GetProcessesByName(consoleApp);
                //IntPtr handle = FindWindow(null, consoleApp);
                //if (handle == IntPtr.Zero)
                //{
                //    Process command = new Process();
                //    ProcessStartInfo commandInfo = new ProcessStartInfo(consoleApp);
                //    commandInfo.UseShellExecute = true;
                //    command.StartInfo = commandInfo;
                //    command.Start();
                //}
                //else
                //{
                //    SetForegroundWindow(handle);
                //}
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("fail", ex.Message);
                return Result.Failed;
            }
        }

        public static void PlayFromByte(byte[] bytesMp3)
        {
            //byte[] bytesMp3 = File.ReadAllBytes(mp3File);

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

        //void StoreSoundDataInElement(Element elem, byte[] bytes)
        //{
        //    using (Transaction createSchemaAndStoreData = new Transaction(elem.Document, "Create sound message"))
        //    {
        //        createSchemaAndStoreData.Start();
        //        Autodesk.Revit.DB.ExtensibleStorage.SchemaBuilder schemaBuilder = new SchemaBuilder(new Guid("a3dcbbab-1faf-403d-a951-e3de809e0808"));
        //        schemaBuilder.SetReadAccessLevel(AccessLevel.Public); // allow anyone to read the object
        //        schemaBuilder.SetWriteAccessLevel(AccessLevel.Public); // AccessLevel.Vendor - restrict writing to this vendor only
        //        schemaBuilder.SetVendorId("BIM-openSorce"); // required because of restricted write-access
        //        schemaBuilder.SetSchemaName("AudioAddin");
        //        FieldBuilder fieldBuilder = schemaBuilder.AddSimpleField("SoundMessageLocation", typeof(System.String));
        //        //fieldBuilder.SetSpec(SpecTypeId.Length);
        //        fieldBuilder.SetDocumentation("A stored location value representing a bytes of soundmessage.");

        //        Schema schema = schemaBuilder.Finish(); // register the Schema object
        //        Entity entity = new Entity(schema); // create an entity (object) for this schema (class)
        //                                            // get the field from the schema
        //        Field fieldSpliceLocation = schema.GetField("SoundMessageLocation");
        //        // set the value for this entity
        //        var stringSound = Convert.ToBase64String(bytes);
        //        entity.Set<System.String>(fieldSpliceLocation, stringSound);
        //        elem.SetEntity(entity); // store the entity in the element

        //        createSchemaAndStoreData.Commit();
        //    }
        //}

        byte[] GetSoundDataInElement(Element elem)
        {
            Schema sch = Schema.Lookup(new Guid("a3dcbbab-1faf-403d-a951-e3de809e0808"));
            Entity ent = elem.GetEntity(sch);
            Field fieldName = sch.GetField("SoundMessageLocation");
            string base64SoundString = ent.Get<string>(fieldName);
            return Convert.FromBase64String(base64SoundString);
        }
    }
}