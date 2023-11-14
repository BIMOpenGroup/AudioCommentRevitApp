namespace AudioComment.Addin
{
    using Autodesk.Revit.Attributes;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;
    using Autodesk.Revit.UI.Selection;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    [Transaction(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class AudioCommandRecord : IExternalCommand
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
                    while (elementid == null)
                    {
                        var taskDialog = new TaskDialog("Необходимо выбрать элементы");
                        taskDialog.CommonButtons = TaskDialogCommonButtons.Yes;
                        taskDialog.MainContent = "Выберите элемент для записи";
                        var selectResult = taskDialog.Show();
                        if (selectResult == TaskDialogResult.Yes)
                        {
                            elementid = uidoc.Selection.PickObject(ObjectType.Element, "Select element or ESC to reset the view").ElementId;
                        }
                        else if (selectResult == TaskDialogResult.Close || selectResult == TaskDialogResult.Cancel)
                        {
                            return Result.Cancelled;
                        }
                    }
                }
                else if (selEmenets.Count > 0)
                {
                    elementid = selEmenets.First();
                }

                if (elementid != null)
                {
                    byte[] sound = null;
                    var taskDialog = new TaskDialog("Запись звука");
                    taskDialog.CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No;
                    taskDialog.MainContent = "Включить запись звука?";
                    var startSoundResult = taskDialog.Show();
                    if (startSoundResult == TaskDialogResult.Yes)
                    {
                        using (var recorder = new RecordWav())
                        {
                            var test = Task.Factory.StartNew(() => { 
                                recorder.Create();
                                recorder.buttonRecord();
                            });
                            var taskDialog2 = new TaskDialog("Запись звука");
                            taskDialog2.CommonButtons = TaskDialogCommonButtons.Ok | TaskDialogCommonButtons.Cancel;
                            taskDialog2.MainContent = "Выключить запись";
                            var stopSoundResult = taskDialog2.Show();
                            if (stopSoundResult == TaskDialogResult.Ok | stopSoundResult == TaskDialogResult.Cancel)
                            {
                                recorder.buttonStop();
                                recorder.waveToMP3Task.Wait();
                                sound = recorder.GetBytes();
                            }
                            else
                            {
                                return Result.Cancelled;
                            }
                            if (sound != null)
                            {
                                var element = uidoc.Document.GetElement(elementid);
                                AudioAddinApp._extStorageUtils.StoreSoundDataInElement(element, sound);
                            }
                        };
                    }
                    else if (startSoundResult == TaskDialogResult.Close || startSoundResult == TaskDialogResult.Cancel)
                    {
                        return Result.Cancelled;
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
    }
}