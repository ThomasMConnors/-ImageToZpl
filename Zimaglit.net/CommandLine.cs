using System;
using System.IO;
using System.CommandLine;                 // from https://www.nuget.org/packages/System.CommandLine  Version: 2.0.0-beta4.22272.1
using ImageToZpl;                         // CommandLine overview at https://learn.microsoft.com/en-us/dotnet/standard/commandline
using ZplParms;


namespace Zimaglit_Net {
   internal class CommandLineOptions {

      // --------------------------- ORIGINAL ZIMAGLIT HELP ---------------------------
      // USAGE: ZIMAGLIT [Drive: path]inputfile [Drive: path]outputfile imagename switches
      //      
      // Each parameter must be separated by at least one space.
      //    
      // Switches are: /?                - HELP (This screen)
      //               /R=rotation       - Degrees of rotation (default = 0)
      //               /I                - Invert the image (black -> white)
      //               /D                - Diffuse dither the image
      //               /X          	     - Compress the image data for transmission

      internal RootCommand BuildCommandLineOptions() {

         var rootCommand = new RootCommand("ZIMAGLIT for .Net (Unofficial port)");

         // --------------------------- INPUT FILE Option ---------------------------
         var inputOption = new Option<FileInfo>(name: "--input", description: "Input file");
         inputOption.AddAlias("-i");
         rootCommand.Add(inputOption);

         // --------------------------- OUTPUT FILE Option ---------------------------
         var outputOption = new Option<FileInfo?>(name: "--output", description: "Output file (default = out.grf)");
         outputOption.AddAlias("-o");
         rootCommand.AddOption(outputOption);

         // --------------------------- IMAGENAME Option ---------------------------
         var imageOption = new Option<string?>(name: "--imagename", description: "Image name in ZPL (default = 'OUT')");
         imageOption.AddAlias("-n");
         rootCommand.AddOption(imageOption);

         // --------------------------- ROTATION Option ---------------------------
         var rotationOption = new Option<int>(name: "--rotation", description: "Degrees of rotation (default = 0)").FromAmong("0", "90", "180", "270", "-90", "-180", "-270");
         rotationOption.AddAlias("-r");
         rootCommand.AddOption(rotationOption);

         // --------------------------- INVERT Option ---------------------------
         var invertOption = new Option<bool>(name: "--invert", description: "Invert the image (black -> white)");
         invertOption.AddAlias("-v");
         rootCommand.AddOption(invertOption);

         // --------------------------- DIFFUSE Option ---------------------------
         var diffuseOption = new Option<float>(name: "--diffuse", description: "Diffuse dither the image");
         diffuseOption.AddAlias("-d");
         rootCommand.AddOption(diffuseOption);

         // --------------------------- COMPRESSION Option ---------------------------
         var compressOption = new Option<bool>(name: "--compress", description: "Compress the image data for transmission");
         compressOption.AddAlias("-c");
         rootCommand.AddOption(compressOption);

#if DEBUG
         // --------------------------- TEST Option ---------------------------
         var testOption = new Option<bool>(name: "--test", description: "Test option functionality");
         testOption.AddAlias("-t");
         rootCommand.AddOption(testOption);
#endif

         rootCommand.SetHandler((inputFile, outputFile, image, rotation, invert, diffuse, compress, test) => { CommandLine.ImageToZplCommandHandler(inputFile!, outputFile!, image!, rotation, invert, diffuse, compress, test); }, inputOption, outputOption, imageOption, rotationOption, invertOption, diffuseOption, compressOption, testOption);

         return rootCommand;
      }
   }

   internal static class CommandLine {

      static int ERROR_FILE_NOT_FOUND = 0x02;

      static internal void ImageToZplCommandHandler(FileInfo? input, FileInfo? output, string? image, int rotation, bool invert, float diffuse, bool compress, bool test) {

#if DEBUG
         Console.WriteLine($"--input = '{input?.FullName}'");
         Console.WriteLine($"--output = '{output?.FullName}'");
         Console.WriteLine($"--imageName = '{image}'");
         Console.WriteLine($"--rotation = '{rotation}'");
         Console.WriteLine($"--invert = '{invert}'");
         Console.WriteLine($"--diffuse = '{diffuse}'");
         Console.WriteLine($"--compress = '{compress}'");
         Console.WriteLine($"--test = '{test}'");
#endif
         ZplParms.ZplParms parms = new ZplParms.ZplParms();

         try {

            if (input is null) {
               Console.WriteLine("Error! You must specify an input file.");
               System.Environment.Exit(ERROR_FILE_NOT_FOUND);
            }

            if (output is null) {
               char sep = Path.DirectorySeparatorChar;
               string? parentDirectory = input.DirectoryName;
               output = new FileInfo($"{parentDirectory}{sep}OUT.GRF");               // Original ZIMAGLIT default extension if an output file is not specified
            }

            image = "UNKNOWN";                                                        // ZPL default image name
            if (image is null) {
            }
            else if (output is not null) {
               image = System.IO.Path.GetFileNameWithoutExtension(output.FullName).ToUpper();
               image = new string(image.Take(8).ToArray());
            }

            if (input is not null && output is not null) {

               ImageToZpl.ImageToZpl imageToZpl = new ImageToZpl.ImageToZpl();

               parms.inputFilePath = input.FullName;
               parms.outputFilePath = output.FullName;
               parms.imageName = image;
               parms.rotation = rotation;
               parms.invert = invert;
               parms.diffuse = diffuse;
               parms.compress = compress;

               imageToZpl.Open(parms);
               imageToZpl.Process();
               imageToZpl.Close();

            }
         } catch (Exception ex) {
            parms.statusMessage = ex.Message;
            Console.WriteLine($"Error! {ex.Message}");
            System.Environment.Exit(ERROR_FILE_NOT_FOUND);
         }

       }
   }
}