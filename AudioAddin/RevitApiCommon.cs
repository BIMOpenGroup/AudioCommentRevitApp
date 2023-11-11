namespace AudioComment.RevitApiCommon
{
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;
    using System.Collections.Generic;

    static class RevitApiCommon
    {
        /// <summary>
        ///     Autodesk Revit UI Application instance.
        /// </summary>
        public static UIApplication UiApplication { get; set; }

        /// <summary>
        ///     Autodesk Revit Application instance.
        /// </summary>
        public static Autodesk.Revit.ApplicationServices.Application Application => UiApplication.Application;

        /// <summary>
        ///     Current Autodesk Revit UI document.
        /// </summary>
        public static UIDocument UiDocument => UiApplication.ActiveUIDocument;

        /// <summary>
        ///     Current Autodesk Revit document.
        /// </summary>
        public static Document Document => UiApplication.ActiveUIDocument.Document;

        /// <summary>
        ///     Current Autodesk Revit active view.
        /// </summary>
        public static View ActiveView => UiApplication.ActiveUIDocument.ActiveGraphicalView;

        public static ICollection<ElementId> DeleteSelectedElements()
        {
            var transaction = new Transaction(Document);
            transaction.Start("Delete elements");

            var selectedIds = UiDocument.Selection.GetElementIds();
            var deletedIds = Document.Delete(selectedIds);

            transaction.Commit();
            return deletedIds;
        }

        public static void TestTaskDialog(string message)
        {
            var transaction = new Transaction(Document);
            transaction.Start("TestTaskDialog");
            TaskDialog.Show("TestTaskDialog", message);
            transaction.Commit();
        }
    }
}
