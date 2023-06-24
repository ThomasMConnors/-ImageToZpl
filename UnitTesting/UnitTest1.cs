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

         Assert.True(bmpToZpl.Compress2Count_2('X', -1) == "");
         Assert.True(bmpToZpl.Compress2Count_2('X', 0) == "");
         Assert.True(bmpToZpl.Compress2Count_2('X', 1) == "X");
         Assert.True(bmpToZpl.Compress2Count_2('X', 19) == "YX");
         Assert.True(bmpToZpl.Compress2Count_2('X', 20) == "gX");
         Assert.True(bmpToZpl.Compress2Count_2('X', 21) == "gGX");
         Assert.True(bmpToZpl.Compress2Count_2('X', 399) == "yYX");
         Assert.True(bmpToZpl.Compress2Count_2('X', 400) == "zX");
         Assert.True(bmpToZpl.Compress2Count_2('X', 401) == "zGX");
         Assert.True(bmpToZpl.Compress2Count_2('X', 1000) == "zzpX");
         Assert.True(bmpToZpl.Compress2Count_2('X', 1001) == "zzpGX");

      }

      [Fact]
      public void Compress2_2_Test() {

         BmpToZpl bmpToZpl = new BmpToZpl();

         string outString;

         string s1 = "0101010101010000000000000000000000000000";
         string s2 = "010101010101FFFFFFFFFFFFFFFFFFFFFFFFFFFF";
         string s3 = "01010101010100000000000000000000000000FF";
         string s4 = "010101010101FFFFFFFFFFFFFFFFFFFFFFFFFF00";
         string s5 = "010101010101FFFFFFFFFFFFFFFFFFFFFFFFF00F";


         //outString = Compress2("0");    // "0"
         //outString = Compress2("00");   // "00"
         //outString = Compress2("000");  // "I0"

         outString = bmpToZpl.Compress2_2(s1);       // "010101010101gN0"
         outString = bmpToZpl.Compress2_2(s2);       // "010101010101gNF"
         outString = bmpToZpl.Compress2_2(s3);       // "010101010101gL0HF"
         outString = bmpToZpl.Compress2_2(s4);       // "010101010101gLFH0"
         outString = bmpToZpl.Compress2_2(s5);       // "010101010101gKFH0F"

      }

      [Fact]
      public void Compress0_test() {

         BmpToZpl bmpToZpl = new BmpToZpl();

         Assert.True(true);

      }
      [Fact]
      public void Compress1_test() {

         BmpToZpl bmpToZpl = new BmpToZpl();

         string priorLine = string.Empty;

         string s1 = "0101010101010000000000000000000000000000";
         string s2 = "010101010101FFFFFFFFFFFFFFFFFFFFFFFFFFFF";
         string s3 = "01010101010100000000000000000000000000FF";
         string s4 = "010101010101FFFFFFFFFFFFFFFFFFFFFFFFFF00";

         Assert.True(bmpToZpl.Compress1(s1, priorLine) == "010101010101");
         Assert.True(bmpToZpl.Compress1(s2, priorLine) == "010101010101!");
         Assert.True(bmpToZpl.Compress1(s3, priorLine) == "01010101010100000000000000000000000000!");
         Assert.True(bmpToZpl.Compress1(s4, priorLine) == "010101010101FFFFFFFFFFFFFFFFFFFFFFFFFF,");

         Assert.True(bmpToZpl.Compress1(s1, s1) == ":");
         Assert.True(bmpToZpl.Compress1(s2, s2) == ":");
         Assert.True(bmpToZpl.Compress1(s3, s3) == ":");
         Assert.True(bmpToZpl.Compress1(s4, s4) == ":");

      }

      [Fact]
      public void Compress2_test() {

         BmpToZpl bmpToZpl = new BmpToZpl();

         string outString;

         Assert.True(true);

         //outString = bmpToZpl.Compress2Count('B', 0);       // ""
         //outString = bmpToZpl.Compress2Count('B', 1);       // "B"
         //outString = bmpToZpl.Compress2Count('B', 19);      // "YB"
         //outString = bmpToZpl.Compress2Count('B', 20);      // "gB"
         //outString = bmpToZpl.Compress2Count('B', 401);     // "zGB"
         //outString = bmpToZpl.Compress2Count('B', 1000);    // "zYBzYBnHB"
      }

      [Fact]
      public void Compress3_test() {

         BmpToZpl bmpToZpl = new BmpToZpl();

         Assert.True(true);

      }


   }
}