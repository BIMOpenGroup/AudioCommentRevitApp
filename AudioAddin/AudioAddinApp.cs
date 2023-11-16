namespace AudioComment.Addin
{
    using AudioAddin;
    using Autodesk.Revit.Attributes;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;
    using Autodesk.Revit.UI.Events;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows;
    using System.Windows.Interop;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    [Transaction(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class AudioAddinApp : IExternalApplication
    {
        public static UIControlledApplication _uiControlApplication;
        public static ExtensibleStorageUtils _extStorageUtils;
        public string _userName;

        public Result OnStartup(UIControlledApplication uiControlApplication)
        {
            try
            {
                RibbonPanel ribbonPanel = uiControlApplication.CreateRibbonPanel("AudioAddin");
                ImageSource bitmapWarning = ToImageSource(SystemIcons.Warning);
                ImageSource bitmapAsterisk = ToImageSource(SystemIcons.Asterisk);
                AddPushButton(ribbonPanel, "Record", "AudioComment.Addin.AudioCommandRecord", bitmapWarning);
                AddPushButton(ribbonPanel, "Play", "AudioComment.Addin.AudioCommandPlay", bitmapAsterisk);
                uiControlApplication.Idling += OnIdling;
                _uiControlApplication = uiControlApplication;
                _extStorageUtils = new ExtensibleStorageUtils(
                    "a3dcbbab-1faf-403d-a951-e3de809e0808",
                    "BIM-openSorce",
                    "AudioAddin",
                    "SoundMessageLocation",
                    "UsersWhoListened");
                _userName = Environment.UserName;
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
            uiControlApplication.Idling -= OnIdling;
            //_serverDispatcher.ReciveData -= OnReciveData;
            return Result.Succeeded;
        }

        public void AddPushButton(RibbonPanel ribbonPanel, string name, string className, ImageSource image)
        {
            try
            {
                string _assemblyPath = $"C:\\code\\AudioCommentRevitAppConsole\\AudioAddin\\bin\\Debug\\AudioAddin.dll";
                PushButtonData pushButtonData = new PushButtonData(name,
                                                                    name,
                                                                    _assemblyPath,
                                                                    className);

                pushButtonData.ToolTipImage = image;
                pushButtonData.LargeImage = image;
                pushButtonData.Image = image;
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

        private void OnIdling(object sender, IdlingEventArgs e)
        {
            try
            {
                var uiapp = (UIApplication)sender;
                var uidoc = uiapp.ActiveUIDocument;
                var selection = uidoc?.Selection;
                var doc = uidoc?.Document;
                if (selection != null)
                {
                    var elemsIds = selection.GetElementIds();
                    if (elemsIds.Count > 0)
                    {
                        var newElemensIds = new List<ElementId>();
                        bool isMessage = false;
                        foreach (var id in elemsIds)
                        {
                            string sound = _extStorageUtils.GetSoundMessage(doc, id);
                            string usersList = _extStorageUtils.GetUsersList(doc, id);
                            TaskDialogResult stopSoundResult = TaskDialogResult.None;
                            if (!string.IsNullOrEmpty(sound) && !usersList.Contains(_userName))
                            {
                                var taskDialog2 = new TaskDialog("Прослушать звуковое сообщение?");
                                taskDialog2.MainContent = $"Данный элемент: {id.IntegerValue} содержит звуковое сообщение, хотите прослушать его?";
                                taskDialog2.CommonButtons = TaskDialogCommonButtons.Ok | TaskDialogCommonButtons.Cancel;
                                stopSoundResult = taskDialog2.Show();
                                isMessage = true;
                                if (stopSoundResult == TaskDialogResult.Ok)
                                {
                                    AudioCommandPlay.PlayFromByte(sound);
                                    usersList += $"{usersList},{_userName}";
                                    _extStorageUtils.WriteUsersList(doc, id, usersList);
                                    var taskDialog = new TaskDialog("Списо прослушавших пользователей");
                                    taskDialog.MainContent = $"Прослушали: {usersList}";
                                    taskDialog.Show();
                                }
                            }
                            if (stopSoundResult == TaskDialogResult.Ok || stopSoundResult == TaskDialogResult.None)
                            {
                                newElemensIds.Add(id);
                            }
                        }
                        if (isMessage)
                        {
                            selection.SetElementIds(newElemensIds);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Exception", ex.Message);
            }
        }
    }
}