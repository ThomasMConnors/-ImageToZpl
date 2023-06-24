using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace ZplParms {
   public class ZplParms {

      public String? inputFilePath { get; set; }
      public String? outputFilePath { get; set; }
      public FileInfo? inputFile { get; set; }
      public FileInfo? outputFile { get; set; }
      public string? imageName { get; set; }
      public int rotation { get; set; }
      public bool invert { get; set; }
      public float diffuse { get; set; }
      public bool compress { get; set; }

      public string? statusMessage { get; set; }

#if DEBUG
      public bool test { get; set; }
#endif

   }

}