using Microsoft.Win32;
using System;
using System.Buffers.Text;
using System.Collections;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

#pragma warning disable CA1416    // Prerelease of System.Drawing.Common.dll is only functional on Windows

namespace ImageToZpl {
   public class BmpToZpl {

      private Bitmap? _bitmap;
      private MemoryStream? _memoryStream;

      public BmpToZpl() {
      }

      public BmpToZpl(Bitmap original, float threshold = 0.8f, int degrees = 0, int CompressionLevel = 0, bool invertImage = false) : this() {

         _bitmap = ConvertToBlackAndWhite(original, threshold);

         _bitmap = Rotate(_bitmap, degrees);

         CompressionLevel = 3;

         _memoryStream = Zpl(_bitmap, CompressionLevel, invertImage);

#if DEBUG
         //       _test_WriteBitmapToConsole();
         //       _test_WriteMemoryStreamToConsole();
         //
         //       WriteBitmapToConsole(original);
         //       WriteMemoryStreamToConsole(_memoryStream, 80, true);
         //       _test_WriteBitArray();
         //       _test_WriteByteArray();
#endif

      }
      private Bitmap ConvertToBlackAndWhite(Bitmap original, float threshold = 0.8f) {

         // Bitmap Clone with a Format16bppGrayScale argument does not work in .net yet.
         // Bitmap bitmap2 = m_bitmap.Clone(new Rectangle(0, 0, m_bitmap.Width, m_bitmap.Height), PixelFormat.Format16bppGrayScale); 

         // Adapted from SwitchOnTheCode (website now archived) using their MakeGrayscale3 sample
         // http://www.switchonthecode.com/tutorials/csharp-tutorial-convert-a-color-image-to-grayscale

         // Create a blank bitmap the same size as original
         Bitmap newBitmap = (Bitmap)original.Clone();

         // Get a graphics object from the new image
         using (Graphics gr = Graphics.FromImage(newBitmap)) {

            // Create the grayscale ColorMatrix
            ColorMatrix grayMatrix = new ColorMatrix(
               new float[][] {
                  new float[] {.299f, .299f, .299f, 0, 0},
                  new float[] {.587f, .587f, .587f, 0, 0},
                  new float[] {.114f, .114f, .114f, 0, 0},
                  new float[] {.000f, .000f, .000f, 1, 0},
                  new float[] {.000f, .000f, .000f, 0, 1}
               }
            );

            // Create some image attributes
            // See https://www.codeproject.com/articles/3772/colormatrix-basics-simple-image-color-adjustment
            // See https://learn.microsoft.com/en-us/dotnet/api/system.drawing.imaging.imageattributes.setthreshold?view=windowsdesktop-7.0#system-drawing-imaging-imageattributes-setthreshold(system-single)

            using (ImageAttributes ia = new System.Drawing.Imaging.ImageAttributes()) {

               // Set the color matrix attribute
               ia.SetColorMatrix(grayMatrix);

               ia.SetThreshold(threshold, ColorAdjustType.Bitmap);

               // Using the grayscale color matrix drawn into the newBitmap
               gr.DrawImage(newBitmap, new Rectangle(0, 0, newBitmap.Width, newBitmap.Height), 0, 0, newBitmap.Width, newBitmap.Height, GraphicsUnit.Pixel, ia);
            }
         }
         return newBitmap;
      }

      private Bitmap Rotate(Bitmap original, int degrees) {

         Bitmap newBitmap = (Bitmap)original.Clone();

         switch (degrees) {
            case 0: {
                  break;                  // Do nothing
               }

            case 90:
            case -270: {
                  newBitmap.RotateFlip(RotateFlipType.Rotate270FlipNone);
                  break;
               }

            case 180:
            case -180: {
                  newBitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
                  break;
               }

            case 270:
            case -90: {
                  newBitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
                  break;
               }

            default: {
                  break;                  // Do nothing
               }

         }
         return newBitmap;
      }

      private MemoryStream Zpl(Bitmap original, int CompressionLevel, bool invertImage) {

         BitArray bitArray;
         Color color;
         Color black = Color.Black;

         string lineOutPriorUncompressed;
         string lineOutUncompressed;
         string lineOutCompressed;
         byte[] byteArray;
         MemoryStream memStream;
         ASCIIEncoding enc;

         bitArray = new BitArray(original.Width);
         lineOutPriorUncompressed = string.Empty;
         memStream = new MemoryStream();
         enc = new ASCIIEncoding();

         for (int y = 0; y < original.Height; y++) {

            for (int x = 0; x < original.Width; x++) {

               color = original.GetPixel(x, y);

               bitArray[x] = (color.R | color.G | color.B) > 0;

               if (invertImage) {
                  bitArray[x] = !bitArray[x];
               }
            }

            byteArray = ToByteArray(bitArray);

            lineOutUncompressed = WriteByteArray(byteArray);

            lineOutCompressed = string.Empty;

            switch (CompressionLevel) {
               case 0: {
                     lineOutCompressed = lineOutUncompressed;
                     break;
                  }

               case 1: {      // Use ":" for same prior line; Use ',' for '0' or '!' for 'F' until end of line
                     lineOutCompressed = Compress1(lineOutUncompressed, lineOutPriorUncompressed);
                     break;
                  }

               case 2: {      // Use an encoding scheme representing n number of consecutive characters
                     lineOutCompressed = Compress2_2(lineOutUncompressed);
                     break;
                  }

               case 3: {      // Use CompressionLevel options 1 and 2
                     lineOutCompressed = Compress1(lineOutUncompressed, lineOutPriorUncompressed);
                     lineOutCompressed = Compress2_2(lineOutCompressed);
                     break;
                  }

               default: {
                     break;
                  }

            }

            Console.WriteLine(lineOutCompressed);

            memStream.Write(enc.GetBytes(lineOutCompressed), 0, lineOutCompressed.Length);

            lineOutPriorUncompressed = lineOutUncompressed;

         }

         memStream.Seek(0, SeekOrigin.Begin);

         return memStream;
      }

      private byte[] ToByteArray(BitArray bitArray) {

         byte[] byteArray;
         int byteCount;
         int bitIndex;
         int byteIndex;

         byteCount = bitArray.Count / 8;    // Note: The / Operator on two ints will return the integer quotient, which drops the remainder.

         if (bitArray.Count % 8 != 0) {
            byteCount += 1;
         }

         byteArray = new byte[byteCount];
         bitIndex = 0;
         byteIndex = 0;

         for (int i = 0; i < bitArray.Count; i++) {
            if (bitArray[i]) {
               byteArray[byteIndex] = Convert.ToByte(byteArray[byteIndex] | Convert.ToByte(1 << 7 - bitIndex));
            }

            bitIndex += 1;

            if (bitIndex == 8) {
               byteIndex += 1;
               bitIndex = 0;
            }
         }

         return byteArray;
      }

      private string WriteBitArray(BitArray bitArray) {
         var sb = new StringBuilder(bitArray.Length);
         for (int i = 0; i < bitArray.Count; i++)
            sb.AppendFormat("{0:X1}", Convert.ToInt32(bitArray[i]));
         return sb.ToString();
      }

      private string WriteByteArray(byte[] byteArray) {
         var sb = new StringBuilder(byteArray.Length);
         for (int i = 0; i < byteArray.Length; i++)
            sb.AppendFormat("{0:X02}", byteArray[i]);
         return sb.ToString();
      }

      public string Compress1(string line, string uncompressedPriorLine) {
         if (string.Compare(line, uncompressedPriorLine, true) == 0) {
            return ":";
         }

         return Compress1(line);
      }

      private string Compress1(string line) {
         Match match;
         string pattern;
         string replacement;
         string compressed;

         if (line.EndsWith("0")) {
            pattern = "0{2,}$";
            replacement = ",";
         }
         else if (line.EndsWith("F")) {
            pattern = "F+$";
            replacement = "!";
         }
         else {
            return line;
         }

         match = Regex.Match(line, pattern, RegexOptions.Compiled);

         if (match.Success) {
            compressed = Regex.Replace(line, pattern, replacement, RegexOptions.Compiled);
         }
         else {
            compressed = line;
         }

         return compressed;
      }

      private MemoryStream? Compress2(MemoryStream? memStream) {
         int count;
         char priorChar;
         char currentChar;
         byte[] buffer;
         var enc = new ASCIIEncoding();
         MemoryStream memStreamOut;
         if (memStream == null) {
            return null;
         }

         memStream.Seek(0, SeekOrigin.Begin);
         count = 1;
         priorChar = (char)memStream.ReadByte();
         memStreamOut = new MemoryStream();
         while (memStream.Position < memStream.Length) {
            currentChar = (char)memStream.ReadByte();
            if (char.IsControl(currentChar))
               continue;
            if (currentChar == priorChar) {
               count += 1;
               if (count > 419) {
                  buffer = enc.GetBytes(Compress2Count(priorChar, 419));
                  memStreamOut.Write(buffer, 0, buffer.Length);
                  count = 1;
               }
            }
            else {
               buffer = enc.GetBytes(Compress2Count(priorChar, count));
               memStreamOut.Write(buffer, 0, buffer.Length);
               count = 1;
               priorChar = currentChar;
            }
         }

         buffer = enc.GetBytes(Compress2Count(priorChar, count));
         memStreamOut.Write(buffer, 0, buffer.Length);
         memStreamOut.Seek(0, SeekOrigin.Begin);
         return memStreamOut;
      }

      public string Compress2_2(string line) {

         int count;
         char priorChar;
         StringBuilder lineOut;

         if (line.Length <= 2) {
            return line;
         }

         lineOut = new StringBuilder(line.Length);

         count = 1;

         priorChar = line[0];

         for (int i = 1; i < line.Length; i++) {

            if (line[i] == priorChar) {
               count += 1;
               continue;
            }

            lineOut.Append(Compress2Count(priorChar, count));

            count = 1;

            priorChar = line[i];

         }

         lineOut.Append(Compress2Count(priorChar, count));

         return lineOut.ToString();
      }

      private string Compress2Count(char value, int count) {

         var sb = new StringBuilder();

         var array1 = new char[] { '\0', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y' };
         var array2 = new char[] { '\0', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };

         // TODO: a 'zzzG' is a valid pattern for large sequences, but it's not handled here

         if (count == 0)
            return string.Empty;
         if (count == 1)
            return char.ToString(value);

         if (count > 419) {
            for (int i = count / 419; i > 0; i--) {
               sb.Append($"zY{value}");
            }
            count %= 419;
         }

         if (char.IsControl(value)) {
            return string.Empty;
         }

         if (value == ',' || value == '!' || value == ':') {
            return string.Empty;
         }

         if (count >= 20) {
            sb.Append(array2[count / 20]);
            count -= count / 20 * 20;
         }

         switch (count) {
            case 0: {
                  sb.Append(value);
                  break;
               }

            case object _ when 1 <= count && count <= 19: {
                  sb.Append(array1[count]);
                  sb.Append(value);
                  break;
               }
         }

         return sb.ToString();
      }

      //write test for this

      public string Compress2Count_2(char value, int count) {

         var sb = new StringBuilder();
         var s = string.Empty;

         var array1 = new char[] { '\0', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y' };
         var array2 = new char[] { '\0', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };

         // TODO: a 'zzzG' is a valid pattern for large sequences,working on a fix

         if (count <= 0)
            return string.Empty;
         if (count == 1)
            return char.ToString(value);

         if (char.IsControl(value)) {
            return string.Empty;
         }

         if (value == ',' || value == '!' || value == ':') {
            return value.ToString();
         }

         if (count >= 400) {
            for (int i = count; i >=400; i -= 400) {
               sb.Append("z");
            }
            count %= 400;
         }

         if (count >= 20) {
            sb.Append(array2[count / 20]);
            count -= count / 20 * 20;
         }

         switch (count) {
            case 0: {
                  sb.Append(value);
                  break;
               }

            case object _ when 1 <= count && count <= 19: {
                  sb.Append(array1[count]);
                  sb.Append(value);
                  break;
               }
         }

         return sb.ToString();
      }



#if DEBUG

      protected long WriteMemoryStreamToConsole(MemoryStream memStream, int lineLength, bool embeddedControl) {

         int bytesRead;
         long byteCount;
         byte[] buffer;

         byteCount = 0;
         buffer = new byte[lineLength];

         memStream.Seek(0, SeekOrigin.Begin);

         while (byteCount < memStream.Length) {
            bytesRead = memStream.Read(buffer, 0, lineLength);
            Console.Write(Encoding.ASCII.GetString(buffer, 0, bytesRead));
            if (!embeddedControl)                  // If memStream does not include newline control chars
               Console.WriteLine();
            byteCount += bytesRead;
         }

         Console.WriteLine();

         return byteCount;

      }

      protected long WriteBitmapToConsole(Bitmap bitmap) {

         for (int x = 0; x < bitmap.Width; x++) {
            for (int y = 0; y < bitmap.Height; y++) {
               Color pixelColor = bitmap.GetPixel(x, y);
               Console.Write(string.Format("{0:x8} ", pixelColor.ToArgb()));
            }
            Console.WriteLine();
         }

         Console.WriteLine();

         return bitmap.Width * bitmap.Height;

      }

      private void _test_WriteBitArray() {

         byte[] bytes = new byte[] { 0x00, 0xFF, 0x00, 0xFF, 0x00, 0xFF, 0x00 };

         BitArray bitArray = new BitArray(bytes);

         Console.WriteLine(WriteBitArray(bitArray));
      }

      private void _test_WriteByteArray() {

         byte[] bytes = new byte[] { 0x00, 0xFF, 0x00, 0xFF, 0x00, 0xFF, 0x00 };

         Console.WriteLine(WriteByteArray(bytes));
      }

      protected void _test_WriteMemoryStreamToConsole() {

         byte[] buffer;
         MemoryStream memStream = new MemoryStream();

         buffer = Encoding.ASCII.GetBytes("ABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZ");
         memStream.Write(buffer);
         WriteMemoryStreamToConsole(memStream, 26, false);

         memStream.Seek(0, SeekOrigin.Begin);

         buffer = Encoding.ASCII.GetBytes("ABCDEFGHIJKLMNOPQRSTUVWXYZ\nABCDEFGHIJKLMNOPQRSTUVWXYZ\nABCDEFGHIJKLMNOPQRSTUVWXYZ\nABCDEFGHIJKLMNOPQRSTUVWXYZ\nABCDEFGHIJKLMNOPQRSTUVWXYZ\nABCDEFGHIJKLMNOPQRSTUVWXYZ\nABCDEFGHIJKLMNOPQRSTUVWXYZ\nABCDEFGHIJKLMNOPQRSTUVWXYZ\nABCDEFGHIJKLMNOPQRSTUVWXYZ\nABCDEFGHIJKLMNOPQRSTUVWXYZ\nABCDEFGHIJKLMNOPQRSTUVWXYZ\nABCDEFGHIJKLMNOPQRSTUVWXYZ\nABCDEFGHIJKLMNOPQRSTUVWXYZ\nABCDEFGHIJKLMNOPQRSTUVWXYZ\nABCDEFGHIJKLMNOPQRSTUVWXYZ\nABCDEFGHIJKLMNOPQRSTUVWXYZ\nABCDEFGHIJKLMNOPQRSTUVWXYZ\nABCDEFGHIJKLMNOPQRSTUVWXYZ\nABCDEFGHIJKLMNOPQRSTUVWXYZ\nABCDEFGHIJKLMNOPQRSTUVWXYZ\nABCDEFGHIJKLMNOPQRSTUVWXYZ\nABCDEFGHIJKLMNOPQRSTUVWXYZ\nABCDEFGHIJKLMNOPQRSTUVWXYZ\nABCDEFGHIJKLMNOPQRSTUVWXYZ\nABCDEFGHIJKLMNOPQRSTUVWXYZ\n");
         memStream.Write(buffer);
         WriteMemoryStreamToConsole(memStream, 26 + ("\n".Length), true);

         memStream.Close();

      }

      protected void _test_WriteBitmapToConsole() {

         Bitmap bitmap = new Bitmap(10, 5);

         for (int x = 0; x < bitmap.Width; x++) {
            for (int y = 0; y < bitmap.Height; y++) {
               Color pixelColor = Color.FromArgb(0, 0, x, y);
               bitmap.SetPixel(x, y, pixelColor);
            }
         }

         WriteBitmapToConsole(bitmap);
      }


      protected void test_Compress0() {
         //outString = Compress2()

      }

      protected void test_Compress1() {

         string outString;

         string s1 = "0101010101010000000000000000000000000000";
         string s2 = "010101010101FFFFFFFFFFFFFFFFFFFFFFFFFFFF";
         string s3 = "01010101010100000000000000000000000000FF";
         string s4 = "010101010101FFFFFFFFFFFFFFFFFFFFFFFFFF00";


         outString = Compress1(s1);                // "010101010101,"
         outString = Compress1(s2);                // "010101010101!"
         outString = Compress1(s3);                // "01010101010100000000000000000000000000!"
         outString = Compress1(s4);                // "010101010101FFFFFFFFFFFFFFFFFFFFFFFFFF,"

         outString = Compress1(s1, s1);            //":"
         outString = Compress1(s2, s2);            //":"
         outString = Compress1(s3, s3);            //":"
         outString = Compress1(s4, s4);            //":"

      }

      protected void _test_Compress2Count() {

         string outString;

         outString = Compress2Count('B', 0);       // ""
         outString = Compress2Count('B', 1);       // "B"
         outString = Compress2Count('B', 19);      // "YB"
         outString = Compress2Count('B', 20);      // "gB"
         outString = Compress2Count('B', 401);     // "zGB"
         outString = Compress2Count('B', 1000);    // "zYBzYBnHB"
      }

      protected void test_Compress3() {
         //outString = Compress2()

      }

   }

#endif

}