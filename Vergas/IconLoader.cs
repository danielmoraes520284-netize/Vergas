using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace BimBlaster.Vergas;

public static class IconLoader
{
    public static BitmapImage Load(string relativePath)
    {
        var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        var img = new BitmapImage();
        img.BeginInit();
        img.CacheOption = BitmapCacheOption.OnLoad;
        img.UriSource = new Uri(Path.Combine(baseDir, relativePath));
        img.EndInit();
        img.Freeze();
        return img;
    }
}