using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

// Added depenancy for System.Drawing.Common 8.0.0-preview.5.23280.5

#pragma warning disable CA1416    // Prerelease of System.Drawing.Common.dll is only functional on Windows

Bitmap bitmap1, bitmap2;

// This works
bitmap1 = new Bitmap(100, 100, PixelFormat.Format16bppGrayScale);
bitmap2 = bitmap1.Clone(new Rectangle(0, 0, bitmap1.Width, bitmap1.Height), PixelFormat.Format16bppGrayScale);

// This halts with System.OutOfMemoryException: 'Out of memory.'
bitmap1 = new Bitmap(100, 100);
bitmap2 = bitmap1.Clone(new Rectangle(0, 0, bitmap1.Width, bitmap1.Height), PixelFormat.Format16bppGrayScale);