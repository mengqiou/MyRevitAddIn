using System;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;

namespace AutoDim
{
    public class PanelButton: IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            // Add a new ribbon panel
            RibbonPanel ribbonPanel1 = application.CreateRibbonPanel("NewRibbonPanel1");

            // Add a new button
            // When clicking this button Class CastOpening will be executed
            PushButton pushButton = ribbonPanel1.AddItem(
                       new PushButtonData("AutoDim", "AutoDim",
                        @"C:\my projects\AutoDim\AutoDim\bin\Debug\AutoDim.dll",
                        "AutoDim.GenDims")) as PushButton;

            // Assign an image to the button
            Uri uriImage = new Uri(@"C:\Program Files\Autodesk\Revit 2024\AddIns\HelloRevit\TestImg.png");
            BitmapImage largeImage = new BitmapImage(uriImage);
            pushButton.LargeImage = largeImage;

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }

}
