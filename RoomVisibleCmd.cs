using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System.Linq;
using System.Windows;

namespace OPENSOURCE
{
    [Transaction(TransactionMode.Manual)]
    public class RoomVisibleCmd : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
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

            try
            {
                Categories categories = doc.Settings.Categories;
                Category sub1 = categories.get_Item(BuiltInCategory.OST_RoomReferenceVisibility);
                Category sub2 = categories.get_Item(BuiltInCategory.OST_RoomInteriorFillVisibility);
                bool b2 = sub1.get_Visible(activeView);
                bool b3 = sub2.get_Visible(activeView);

                using (Transaction t = new Transaction(doc, " "))
                {
                    t.Start();

                    if (!b2 && !b3)
                    {
                        activeView.SetCategoryHidden(sub1.Id, false);
                        activeView.SetCategoryHidden(sub2.Id, false);
                    }
                    else
                    {
                        activeView.SetCategoryHidden(sub1.Id, true);
                        activeView.SetCategoryHidden(sub2.Id, true);
                    }
                    t.Commit();
                }
            }
            catch
            {
                MessageBox.Show("Can not access to rooms in view. Please unlock view template!", "Message");
            }
            

            return Result.Succeeded;
        }
    }
}
