using System.IO;
using System.Reflection;

namespace NetRadio.ViewModels
{
    static class Settings
    {
        public readonly static string ResourcePath = Path.GetDirectoryName(
                  Assembly.GetExecutingAssembly().Location) + @"\Resources\";

        public static bool IsDirty { get; set; }
    }
}
