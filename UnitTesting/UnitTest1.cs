using System.Drawing;
using System.Text;
using ImageToZpl;

#pragma warning disable CA1416    // Prerelease of System.Drawing.Common.dll is only functional on Windows

namespace ImageToZpl_Testing {

   public class BmpToZplTest : BmpToZpl {

      [Fact]
      public void _testForTrue() {
         Assert.True(true);
      }

      [Fact]
      public void Compress2Count_2_Test() {

         BmpToZpl bmpToZpl = new BmpToZpl();

         Assert.True(bmpToZpl.Compress2Count_2('X', 0) == "");
         Assert.True(bmpToZpl.Compress2Count_2('X', 1) == "X");
         Assert.True(bmpToZpl.Compress2Count_2('X', 19) == "YX");
         Assert.True(bmpToZpl.Compress2Count_2('X', 20) == "gX");
         Assert.True(bmpToZpl.Compress2Count_2('X', 21) == "hX");
         Assert.True(bmpToZpl.Compress2Count_2('X', 399) == "yX");
         Assert.True(bmpToZpl.Compress2Count_2('X', 400) == "zX");
         Assert.True(bmpToZpl.Compress2Count_2('X', 401) == "zzX");
         Assert.True(bmpToZpl.Compress2Count_2('X', 1000) == "zzzX");
         Assert.True(bmpToZpl.Compress2Count_2('X', 1001) == "zzzYX");

      }
   }
}

