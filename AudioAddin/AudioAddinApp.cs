namespace AudioComment.Addin
{
    //using AudioComment.NamedPipesLib;
    using Autodesk.Revit.Attributes;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;
    using Autodesk.Revit.UI.Events;
    using InterprocessCommunication.NamedPipeUtil;
    using InterprocessCommunication.PipeDispatcherAbstract;
    using System;
    using System.Drawing;
    using System.IO;
    using System.IO.Pipes;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Interop;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    [Transaction(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class AudioAddinApp : IExternalApplication
    {
        static UIControlledApplication _uiControlApplication;

        public Result OnStartup(UIControlledApplication uiControlApplication)
        {
            try
            {
                _uiControlApplication = uiControlApplication;
                RibbonPanel ribbonPanel = uiControlApplication.CreateRibbonPanel("AudioAddin");
                BitmapImage bitmapWarning = ToImageSource(SystemIcons.Warning) as BitmapImage;
                BitmapImage bitmapAsterisk = ToImageSource(SystemIcons.Asterisk) as BitmapImage;
                AddPushButton(ribbonPanel, "Record", "AudioComment.Addin.AudioCommandRecord", bitmapWarning);
                AddPushButton(ribbonPanel, "Play", "AudioComment.Addin.AudioCommandPlay", bitmapAsterisk);
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("fail", ex.Message);
                return Result.Cancelled;
            }
        }

        public Result OnShutdown(UIControlledApplication uiControlApplication)
        {
            //uiControlApplication.Idling -= OnIdling;
            //_serverDispatcher.ReciveData -= OnReciveData;
            return Result.Succeeded;
        }

        public void AddPushButton(RibbonPanel ribbonPanel, string name, string className, BitmapImage image)
        {
            try
            {
                string _assemblyPath = $"C:\\code\\AudioCommentRevitAppConsole\\AudioAddin\\bin\\Debug\\AudioAddin.dll";
                PushButtonData pushButtonData = new PushButtonData(name,
                                                                    name,
                                                                    _assemblyPath,
                                                                    className);

                pushButtonData.ToolTipImage = image;
                PushButton button = ribbonPanel.AddItem(pushButtonData) as PushButton;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("fail", ex.Message);
            }
        }

        private ImageSource ToImageSource(Icon icon)
        {
            ImageSource imageSource = Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions()
            );

            return imageSource;
        }
    }
}