namespace AudioComment.Addin
{
    using Autodesk.Revit.Attributes;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;
    using Autodesk.Revit.UI.Selection;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Pipes;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;
    using static Autodesk.Revit.DB.SpecTypeId;

    [Transaction(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class AudioAddinCommand : IExternalCommand
    {
        static string consoleApp = "C:\\code\\AudioCommentRevitAppConsole\\AudioConsoleAppNetF\\bin\\Debug\\AudioConsoleAppNetF.exe";
        //static TaskDialog taskDiag;
        static Element SelectElement;
        //static Schema schema;
        static byte[] soundData;
        static string pipeName = "AudioConsole1";
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
                Process[] test0 = Process.GetProcessesByName(consoleApp);
                IntPtr handle = FindWindow(null, consoleApp);
                if (handle == IntPtr.Zero)
                {
                    Process command = new Process();
                    ProcessStartInfo commandInfo = new ProcessStartInfo(consoleApp);
                    commandInfo.UseShellExecute = true;
                    command.StartInfo = commandInfo;
                    command.Start();
                }
                else
                {
                    SetForegroundWindow(handle);
                }

                //Task.Delay(1000).Wait();

                //Task.Factory.StartNew(() => StartClient());

                //var doc = commandData.Application.ActiveUIDocument.Document;
                //var test1 = commandData.Application.ActiveUIDocument.Selection.PickObject(ObjectType.Element);
                //SelectElement = doc.GetElement(test1);
                //var external_event = ExternalEvent.Create(new TehnicalParametersCreator());


                ////var taskDiag = new TaskDialog("test1");
                ////taskDiag.CommonButtons = TaskDialogCommonButtons.Ok;
                ////taskDiag.MainInstruction = "старт1";
                ////taskDiag.EnableMarqueeProgressBar = true;
                ////TaskDialogResult test2 = taskDiag.Show();

                ////var test = reader.ReadToEndAsync();

                ////var taskDiag2 = new TaskDialog("test1");
                ////taskDiag2.CommonButtons = TaskDialogCommonButtons.Ok;
                ////taskDiag2.MainInstruction = test.Result;
                ////TaskDialogResult test3 = taskDiag.Show();

                ////if (test2 == TaskDialogResult.Ok)
                ////{
                ////    writer.WriteLine("quit");
                ////}-+*
                //var taskDiag = new TaskDialog("test1");
                //taskDiag.CommonButtons = TaskDialogCommonButtons.Ok;
                //taskDiag.MainInstruction = "старт1";
                //taskDiag.EnableMarqueeProgressBar = true;
                //TaskDialogResult test2 = taskDiag.Show();
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("fail", ex.Message);
                return Result.Failed;
            }
        }

        void StartClient()
        {
            using (NamedPipeClientStream client = new NamedPipeClientStream(pipeName))
            {
                client.Connect();
                reader = new StreamReader(client);
                writer = new StreamWriter(client);

                while (true)
                {
                    var test = reader.ReadLine();
                    if (test == "quit")
                    {
                        _message = test;
                        break;
                    }
                    else if (test.Length > 100)
                    {
                        _message = test;
                    }
                }
                //while (true)
                //{
                //    string input = Console.ReadLine();
                //    if (String.IsNullOrEmpty(input)) break;
                //    writer.WriteLine(input);
                //    writer.Flush();
                //    Console.WriteLine(reader.ReadLine());
                //    reader.ReadLine(); 
                //}
            }
        }

        class TehnicalParametersCreator : IExternalEventHandler
        {
            public string GetName()
            {
                return "TehnicalParametersCreator";
            }

            public void Execute(UIApplication app)
            {
                StoreSoundDataInElement(SelectElement, soundData);
                //revit_version = app.Application.VersionNumber;
                //Creator();
            }
            void StoreSoundDataInElement(Element elem, byte[] bytes)
            {
                using (Transaction createSchemaAndStoreData = new Transaction(elem.Document, "Create sound message"))
                {
                    //createSchemaAndStoreData.Start();
                    //SchemaBuilder schemaBuilder = new SchemaBuilder(new Guid("a3dcbbab-1faf-403d-a951-e3de809e0808"));
                    //schemaBuilder.SetReadAccessLevel(AccessLevel.Public); // allow anyone to read the object
                    //schemaBuilder.SetWriteAccessLevel(AccessLevel.Public); // AccessLevel.Vendor - restrict writing to this vendor only
                    //schemaBuilder.SetVendorId("BIM-openSorce"); // required because of restricted write-access
                    //schemaBuilder.SetSchemaName("AudioAddin");
                    //FieldBuilder fieldBuilder = schemaBuilder.AddSimpleField("SoundMessageLocation", typeof(byte[]));
                    ////fieldBuilder.SetSpec(SpecTypeId.Length);
                    //fieldBuilder.SetDocumentation("A stored location value representing a bytes of soundmessage.");

                    //schema = schemaBuilder.Finish(); // register the Schema object
                    //Entity entity = new Entity(schema); // create an entity (object) for this schema (class)
                    //                                    // get the field from the schema
                    //Field fieldSpliceLocation = schema.GetField("SoundMessageLocation");
                    //// set the value for this entity
                    //entity.Set<byte[]>(fieldSpliceLocation, bytes);
                    //elem.SetEntity(entity); // store the entity in the element

                    //createSchemaAndStoreData.Commit();
                }
            }
        }

    }
}