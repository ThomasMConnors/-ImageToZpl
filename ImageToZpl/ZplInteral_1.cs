using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.IO.Compression;
//using Microsoft.VisualBasic; // Install-Package Microsoft.VisualBasic

#pragma warning disable CA1416    // Prerelease of System.Drawing.Common.dll is only functional on Windows

// Use these settings to set serial port for a serial attached Zebra printer
// mode com1: Baud=9600 Parity=N Data=8

namespace ImageToZpl {

   public partial class ImageToZpl {

      public string Zpl() {
         using (MemoryStream memStream = new MemoryStream()) {
            ImageToMemoryStream(memStream);
            string z = GetZplFromStream(memStream);
            return z;
         }
      }

      private void ImageToMemoryStream(MemoryStream memStream) {

         byte[] byteArray;

            // m_ZplImage.Rotate(Rotation);
            // m_ZplImage.ConvertToBlackAndWhite(GrayMatrix);

            byteArray = Encoding.UTF8.GetBytes(string.Format("{0}{1}", Header, Environment.NewLine));

            memStream.Write(byteArray, 0, byteArray.Length);
            
            //m_ZplImage.Zpl(CompressionLevel, InvertImage).WriteTo(memStream);
           
     }

      private string GetZplFromStream(MemoryStream memStream) {
         memStream.Position = 0;
         using (var reader = new StreamReader(memStream)) {
            return reader.ReadToEnd();
         }
      }

      private int Height {
         get {
            if (_bitmap == null)
               return 0;
            return _bitmap.Height;
         }
      }

      private int Width {
         get {
            if (_bitmap == null)
               return 0;
            return _bitmap.Width;
         }
      }

      private int BytesPerRow {
         get {
            int bpr;
            if (_image == null)
               return 0;
            bpr = _image.Width / 8;
            if (_image.Width % 8 != 0)
               bpr += 1;
            return bpr;
         }
      }

      private int BytesInGraphic {
         get {
            if (_image == null)
               return 0;
            return BytesPerRow * _image.Height;
         }
      }

      private string Header {
         get {
            return string.Format("~DGR:{0},{1},{2},", _imageName, BytesInGraphic, BytesPerRow);
         }
      }

      private string SampleImageZpl {
         get {
            return string.Format("^XA{0}^FO615,102^XGR:{1},1,1^FS{0}^XZ", Environment.NewLine, _imageName);
         }
      }
   }
}