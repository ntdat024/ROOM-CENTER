using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace OPENSOURCE
{
    [Transaction(TransactionMode.Manual)]
    public class RoomCenterCmd : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument uidoc = uiApp.ActiveUIDocument;
            Document doc = uidoc.Document;
            View activeView = doc.ActiveView;

            var collector = new FilteredElementCollector(doc, activeView.Id)
                .OfCategory(BuiltInCategory.OST_Rooms)
                .WhereElementIsNotElementType()
                .Cast<Room>()
                .ToList();

            if (collector.Count == 0)
            {
                MessageBox.Show("No room in view!", "Messgae");
                return Result.Failed;
            }

            using (Transaction t = new Transaction(doc, " "))
            {
                t.Start();

                foreach (Room room in collector)
                {
                    try
                    {
                        GeometryElement geometry = room.get_Geometry(new Options());
                        Solid roomSolid = null;

                        using (IEnumerator<GeometryObject> enumerator = geometry.GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                            {
                                Solid solid = enumerator.Current as Solid;
                                if (room != null)
                                {
                                    roomSolid = solid;
                                    break;
                                }
                            }
                        }

                        XYZ center = roomSolid.ComputeCentroid();

                        LocationPoint locationPoint = room.Location as LocationPoint;
                        XYZ point = locationPoint.Point;

                        XYZ newPoint = new XYZ(center.X - point.X, center.Y - point.Y, point.Z);
                        if (room.IsPointInRoom(center))
                        {
                            room.Location.Move(newPoint);
                        }
                    }
                    catch { }

                }

                t.Commit();
            }

            MessageBox.Show("Done!", "Message");

            return Result.Succeeded;
        }
    }
}
