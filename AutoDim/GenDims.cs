using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Structure;

namespace AutoDim
{
    [Transaction(TransactionMode.Manual)]
    class GenDims : IExternalCommand
    {
        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            // get all the columns element in current view
            Document doc = revit.Application.ActiveUIDocument.Document;
            View activeView = doc.ActiveView;
            FilteredElementCollector collector = new FilteredElementCollector(doc, activeView.Id);
            ElementCategoryFilter filter = new ElementCategoryFilter(BuiltInCategory.OST_StructuralColumns);
            ICollection<Element> columns = collector.WherePasses(filter).WhereElementIsNotElementType().ToElements();


            FilteredElementCollector collector2 = new FilteredElementCollector(doc, activeView.Id);
            ElementCategoryFilter filter2 = new ElementCategoryFilter(BuiltInCategory.OST_Grids);
            IList<Element> Grids = collector2.WherePasses(filter2).WhereElementIsNotElementType().ToElements();


            // get all the horizontal grids
            List<Element> hori = HorizontalGrids(Grids);
            List<Element> vertical = VerticalGrids(Grids);

            // get all the vertical grids


            //TaskDialog dialog = new TaskDialog("Column infos");
            //dialog.MainInstruction = "Element IDs of Columns in the View:";
            foreach (Element column in columns)
            {
                XYZ p = (column.Location as LocationPoint).Point;
                // how to creat a reference from the column center point?
                // Create a reference to the column center
                ReferencePoint referencePoint = doc.FamilyCreate.NewReferencePoint(p);
                Reference colRef = new Reference(referencePoint);
                

                Grid hGrid = ClosestGrid(p, hori) as Grid;
                Reference horiRef = new Reference(hGrid);
                ReferenceArray horiRefs = new ReferenceArray();
                horiRefs.Append(horiRef);
                horiRefs.Append(colRef);
                IntersectionResult result1 = (hGrid.Curve as Line).Project(p);
                Line dimension1 = Line.CreateBound(p, result1.XYZPoint);

                ReferenceArray vertiRefs = new ReferenceArray();
                Grid vGrid = ClosestGrid(p, vertical) as Grid;
                Reference vertiRef = new Reference(vGrid);
                vertiRefs.Append(vertiRef);
                vertiRefs.Append(colRef);
                IntersectionResult result2 = (vGrid.Curve as Line).Project(p);
                Line dimension2 = Line.CreateBound(p, result2.XYZPoint);
                
                using (Transaction transaction = new Transaction(doc, "new dimension"))
                {
                    transaction.Start();

                    Dimension new1 = doc.Create.NewDimension(activeView, dimension1, horiRefs);
                    Dimension new2 = doc.Create.NewDimension(activeView, dimension2, vertiRefs);
                    transaction.Commit();
                }

            }

            //dialog.Show();

            return Result.Succeeded;
        }

        private List<Element> HorizontalGrids(IList<Element> grids)
        {
            List <Element> HorizontalGrids = new List<Element>();
            foreach (Element grid in grids)
            {
                Grid g = grid as Grid;

                Line gridLine = g.Curve as Line;
                XYZ absDirection = new XYZ(Math.Abs(gridLine.Direction.X), Math.Abs(gridLine.Direction.Y), Math.Abs(gridLine.Direction.Z));
                if (absDirection.IsAlmostEqualTo(XYZ.BasisX))
                {
                    HorizontalGrids.Add(grid);
                }
            }

            return HorizontalGrids;
        }

        private List<Element> VerticalGrids(IList<Element> grids)
        {
            List<Element> HorizontalGrids = new List<Element>();
            foreach (Element grid in grids)
            {
                Grid g = grid as Grid;

                Line gridLine = g.Curve as Line;
                XYZ absDirection = new XYZ(Math.Abs(gridLine.Direction.X), Math.Abs(gridLine.Direction.Y), Math.Abs(gridLine.Direction.Z));
                if (absDirection.IsAlmostEqualTo(XYZ.BasisY))
                {
                    HorizontalGrids.Add(grid);
                }
            }

            return HorizontalGrids;
        }

        private Element ClosestGrid(XYZ p, List<Element> grids)
        {
            double minDistance = double.MaxValue;
            Element closestGrid = null;
            foreach (Element g in grids)
            {
                Grid grid = g as Grid;

                Line gridLine = grid.Curve as Line;

                // Calculate the perpendicular distance from the point to the grid line
                double distance = gridLine.Distance(p);
                // Update closest grid if the current grid is closer
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestGrid = g;
                }
            }

            return closestGrid;
        }
    }
}
