//namespace AudioComment.Addin
//{
//    using Autodesk.Revit.DB;
//    using System;
//    using System.Linq;

//    public class ElementAudioUpdater : IUpdater
//    {
//        static AddInId m_appId;
//        static UpdaterId m_updaterId;
//        //WallType m_wallType = null;

//        // constructor takes the AddInId for the add-in associated with this updater
//        public ElementAudioUpdater(AddInId id)
//        {
//            m_appId = id;
//            m_updaterId = new UpdaterId(m_appId, new Guid("a4ef9ab1-b36e-47a1-bbf7-cb4327537a57"));
//        }

//        public void Execute(UpdaterData data)
//        {
//            Document doc = data.GetDocument();

//            // Cache the wall type
//            //if (m_wallType == null)
//            //{
//            //    FilteredElementCollector collector = new FilteredElementCollector(doc);
//            //    collector.OfClass(typeof(WallType));
//            //    var wallTypes = from element in collector
//            //                    where
//            //                        element.Name == "Exterior - Brick on CMU"
//            //                    select element;
//            //    if (wallTypes.Count<Element>() > 0)
//            //    {
//            //        m_wallType = wallTypes.Cast<WallType>().ElementAt<WallType>(0);
//            //    }
//            //}

//            //if (m_wallType != null)
//            //{
//            //    // Change the wall to the cached wall type.
//            //    foreach (ElementId addedElemId in data.GetAddedElementIds())
//            //    {
//            //        Wall wall = doc.GetElement(addedElemId) as Wall;
//            //        if (wall != null)
//            //        {
//            //            wall.WallType = m_wallType;
//            //        }
//            //    }
//            //}
//        }

//        public string GetAdditionalInformation()
//        {
//            return " ";
//        }

//        public ChangePriority GetChangePriority()
//        {
//            return ChangePriority.FloorsRoofsStructuralWalls;
//        }

//        public UpdaterId GetUpdaterId()
//        {
//            return m_updaterId;
//        }

//        public string GetUpdaterName()
//        {
//            return " ";
//        }
//    }
//}