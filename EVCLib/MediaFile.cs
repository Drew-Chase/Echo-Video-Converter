using System.IO;

namespace ChaseLabs.Echo.Video_Converter.Resources
{
    public class MediaFile
    {
        public int ID { get; set; }
        public string FilePath { get; set; }
        public long Size => new FileInfo(FilePath).Length;
        public string FileName => new FileInfo(FilePath).Name;
    }
}
