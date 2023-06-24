using System;
using System.IO;
using System.CommandLine;

// See https://learn.microsoft.com/en-us/dotnet/api/system.windows.media.imaging.bitmapsource?view=windowsdesktop-8.0
// See https://learn.microsoft.com/en-us/dotnet/api/system.drawing.image.rotateflip?view=windowsdesktop-7.0#system-drawing-image-rotateflip(system-drawing-rotatefliptype)
// See https://www.vdos.info/download.html on how to test the old ZIMAGLIT MS-DOS program on modern 64-bit Windows
// See https://supportcommunity.zebra.com/s/article/Command-Line-Conversion-of-PCX-Files-for-ZPL-Printers?language=en_US for a PCX-only workaround on some Zebra printers

namespace Zimaglit_Net;

   internal class Zimaglit {
      static async Task<int> Main(string[] args) {

         CommandLineOptions cmdLnOptions = new CommandLineOptions();

         RootCommand rootCommand = cmdLnOptions.BuildCommandLineOptions();

         return await rootCommand.InvokeAsync(args);
      }

   }