using System.Reflection;
using Autodesk.Revit.UI;

namespace BimBlaster.Vergas;

public class VergasApplication : IExternalApplication
{
    public Result OnStartup(UIControlledApplication application)
    {
        const string panelName = "BIM BLASTER";
        var panel = application.GetRibbonPanels().FirstOrDefault(p => p.Name == panelName)
                    ?? application.CreateRibbonPanel(panelName);

        var lang = application.ControlledApplication.Language;
        var assemblyPath = Assembly.GetExecutingAssembly().Location;
        var buttonData = new PushButtonData(
            "Vergas",
            LocalizedStrings.Get(lang, "ButtonText"),
            assemblyPath,
            "BimBlaster.Vergas.VergasCommand")
        {
            ToolTip = LocalizedStrings.Get(lang, "ButtonTooltip"),
            LongDescription = LocalizedStrings.Get(lang, "ButtonTooltip"),
            LargeImage = IconLoader.Load("icon32.png"),
            Image = IconLoader.Load("icon16.png")
        };

        panel.AddItem(buttonData);
        return Result.Succeeded;
    }

    public Result OnShutdown(UIControlledApplication application) => Result.Succeeded;
}