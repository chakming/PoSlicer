using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ImageMagick;
using LanguageExt;

namespace PhotoVerticalSplit
{
    public class Program
    {
        public static readonly List<string> ImageExtensions = new List<string> { ".JPG", ".JPEG", ".TIF", ".TIFF", ".BMP", ".GIF", ".PNG" };

        public static void Main(string[] args)
        {
            var poKitchen = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)+ "\\PoKitchen";
            Directory.CreateDirectory(poKitchen);
            Console.WriteLine($"Folder po now going to: {poKitchen}");

            GetOnlyOneImageInFolder(poKitchen).Match<Unit>(
                None: () =>
                {
                    Console.WriteLine("Po: I have no image to slice in this folder!");
                    return Unit.Default;
                },
                Some: inputPath =>
                {
                    SliceImage(inputPath);
                    return Unit.Default;
                }
            );

            Console.WriteLine("Po: I finished slicing! :*");
            Console.WriteLine("press enter to exit...");
            Console.Read();
        }

        private static void SliceImage(string inputPath)
        {
            var originalFileDir = Path.GetDirectoryName(inputPath);
            var originalFileName = Path.GetFileNameWithoutExtension(inputPath);
            var originalFileExt = Path.GetExtension(inputPath);

            using (var image = new MagickImage(inputPath))
            {
                int i = 0;
                var bestImageHeight = GetBestImageHeight(image);
                var outputDir = $"{originalFileDir}\\PoOutput";
                EmptyOutputDirectory(outputDir);
                foreach (MagickImage tile in image.CropToTiles(image.Width, bestImageHeight))
                {
                    tile.Write($"{outputDir}\\{originalFileName}_{i++}.jpg");
                }
            }
        }

        private static Option<string> GetOnlyOneImageInFolder(string kitchenDir)
        {
            var filesInCurrentDir = Directory.GetFiles(kitchenDir).ToList();
            var file = filesInCurrentDir.FirstOrDefault(filePath =>
            {
                return ImageExtensions.Contains(Path.GetExtension(filePath).ToUpperInvariant());
            });
            return file;
        }

        private static void EmptyOutputDirectory(string outputDir)
        {
            if (Directory.Exists(outputDir)) Directory.Delete(outputDir, true);
            Directory.CreateDirectory(outputDir);
        }

        private static int GetBestImageHeight(MagickImage image)
        {
            var notBadExpectedHeight = 0;
            var maxHitTimes = 10;
            for (var expectedHeight = image.Width * 4 / 3; expectedHeight >= image.Width; expectedHeight -= 1)
            {
                var testingImageNumSplitted = image.Height / expectedHeight;
                if (image.Height - testingImageNumSplitted * expectedHeight == 0)
                {
                    return expectedHeight;
                }
                if (image.Height - testingImageNumSplitted * expectedHeight > expectedHeight / 1.5)
                {
                    notBadExpectedHeight = expectedHeight;

                    if (maxHitTimes-- < 0)
                    {
                        break;
                    }
                }
            }

            return notBadExpectedHeight == 0 ? image.Height : notBadExpectedHeight;
        }


    }
}
