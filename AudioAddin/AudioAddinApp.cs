namespace AudioComment.Addin
{
    using Autodesk.Revit.Attributes;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;
    using Autodesk.Revit.UI.Events;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Pipes;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    [Transaction(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class AudioAddinApp: IExternalApplication
    {
        string audioAddinAppTabName = "testAudioApp";
        static string pipeName = "AudioConsole1";
        static NamedPipeClientStream client;
        static StreamWriter writer;
        static StreamReader reader;
        static Task clientConnectTask;

        public Result OnStartup(UIControlledApplication uiControlApplication)
        {
            try
            {
                //RibbonPanel ribbonPanelCreated = CreationRibbonPanel(uiControlApplication, audioAddinAppTabName);
                AddPushButton(uiControlApplication);
                uiControlApplication.Idling += OnIdling;
                Task.Factory.StartNew(() => StartNamePipeClient());
                //System.Threading.Thread.Sleep(2000);
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("fail", ex.Message);
                return Result.Cancelled;
            }
        }

        private static void OnIdling(object sender, IdlingEventArgs args)
        {
            try
            {
                if (!client.IsConnected && clientConnectTask.IsCompleted)
                {
                    clientConnectTask = client.ConnectAsync();
                }
                var test = reader?.ReadLine();
                if (test != null)
                {
                    if (test == "quit")
                    {
                        TaskDialog.Show(test, test);
                    }
                    else if (test.Length > 100)
                    {
                        TaskDialog.Show("bytes", test.Length.ToString());
                    }
                    else
                    {
                        TaskDialog.Show(test, test);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        void StartNamePipeClient()
        {
            client = new NamedPipeClientStream(pipeName);
            clientConnectTask = client.ConnectAsync();
            reader = new StreamReader(client);
            writer = new StreamWriter(client);
        }

        public Result OnShutdown(UIControlledApplication uiControlApplication)
        {
            uiControlApplication.Idling -= OnIdling;
            return Result.Succeeded;
        }

        //public RibbonPanel CreationRibbonPanel(UIControlledApplication _application, string _nameRibbonPanel)
        //{
        //    //string tabName = audioAddinAppTabName;
        //    //IList<RibbonPanel> list_Ribbon = _application.GetRibbonPanels();
        //    //if (list_Ribbon.Where(a => a.Name == tabName).FirstOrDefault() != null)
        //    //{
        //    //    tabName += "1";
        //    //}
        //    //_application.CreateRibbonTab(tabName);
        //    //RibbonPanel ribbonPanelCreated = _application.CreateRibbonPanel(tabName, _nameRibbonPanel);
        //    //return ribbonPanelCreated;
        //}

        public void AddPushButton(UIControlledApplication uiControlApplication)
        {
            try
            {
                RibbonPanel ribbonPanel = uiControlApplication.CreateRibbonPanel("AudioAddin");
                string _assemblyPath = $"C:\\code\\AudioCommentRevitAppConsole\\AudioAddin\\bin\\Debug\\AudioAddin.dll";
                PushButtonData pushButtonData = new PushButtonData(audioAddinAppTabName,
                                                                    audioAddinAppTabName,
                                                                    _assemblyPath,
                                                                    "AudioComment.Addin.AudioAddinCommand");
                PushButton button = ribbonPanel.AddItem(pushButtonData) as PushButton;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("fail", ex.Message);
            }
        }
    }
}