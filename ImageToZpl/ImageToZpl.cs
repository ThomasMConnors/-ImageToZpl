using System;
using System.IO;
using System.Drawing;
using ZplParms;

#pragma warning disable CA1416    // Prerelease of System.Drawing.Common.dll is only functional on Windows

namespace ImageToZpl {
  
   public partial class ImageToZpl {

      private FileStream? _inputFileStream;
      private FileStream? _outputFileStream;

      private string _imageName = "UNKNOWN";   // Zimaglit default Value if a name is not specified, UNKNOWN is used
      private int _degreeRoation = 0;
      private bool _invertImage = false;
      private float _diffuseImage = 0.8f;
      private int _compressionLevel = 0;

      private Image? _image;
      private Bitmap? _bitmap;

      public void Open(ZplParms.ZplParms parms) {

         if (parms.inputFilePath is null || parms.inputFilePath.Length == 0 || parms.outputFilePath is null || parms.outputFilePath.Length == 0) {
            parms.statusMessage = "Error! You must specify an input and output file.";
            throw new Exception(parms.statusMessage);
         }

         try {
            _inputFileStream = new FileStream(parms.inputFilePath, FileMode.Open);
            _outputFileStream = new FileStream(parms.outputFilePath, FileMode.OpenOrCreate);
         } catch (Exception ex) {
            parms.statusMessage = ex.Message;
            Console.WriteLine(ex.Message);
         }

      }

      public void Process() {

         if (_inputFileStream is null) {
            return;
         }

         BmpToZpl bitmapToZpl;

         _image = Image.FromStream(_inputFileStream);             // opens file in almost any format
         _bitmap = new Bitmap(_image);

         bitmapToZpl = new BmpToZpl(_bitmap, _diffuseImage, _degreeRoation, _compressionLevel, _invertImage);

      }

      public void Close() {

         if (_inputFileStream is not null) _inputFileStream.Close();
         if (_outputFileStream is not null) _outputFileStream.Close();

      }

      ~ImageToZpl() {

         this.Close();

      }

   }

}