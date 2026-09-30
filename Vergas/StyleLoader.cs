using System.IO;
using System.Reflection;
using System.Windows;

namespace BimBlaster.Vergas;

public static class StyleLoader
{
    public static ResourceDictionary LoadBimBlasterStyles()
    {
        var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        var path = Path.Combine(baseDir, "BimBlasterStyles.xaml");
        return new ResourceDictionary { Source = new Uri(path, UriKind.Absolute) };
    }
}