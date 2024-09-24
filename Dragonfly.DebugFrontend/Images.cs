using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Dragonfly.DebugFrontend
{
    internal class Images
    {
        public static readonly Images Instance = new Images();
        public readonly Bitmap None;
        public readonly Bitmap Error;

        public readonly Bitmap WP;
        public readonly Bitmap WN;
        public readonly Bitmap WB;
        public readonly Bitmap WR;
        public readonly Bitmap WQ;
        public readonly Bitmap WK;

        public readonly Bitmap BP;
        public readonly Bitmap BN;
        public readonly Bitmap BB;
        public readonly Bitmap BR;
        public readonly Bitmap BQ;
        public readonly Bitmap BK;

        private const string Path = "avares://Dragonfly.DebugFrontend/Assets/Pieces/";

        private Images()
        {
            None = ImageHelper.LoadPiece("none");
            Error = ImageHelper.LoadPiece("error");

            WP = ImageHelper.LoadPiece("wP");
            WN = ImageHelper.LoadPiece("wN");
            WB = ImageHelper.LoadPiece("wB");
            WR = ImageHelper.LoadPiece("wR");
            WQ = ImageHelper.LoadPiece("wQ");
            WK = ImageHelper.LoadPiece("wK");

            BP = ImageHelper.LoadPiece("bP");
            BN = ImageHelper.LoadPiece("bN");
            BB = ImageHelper.LoadPiece("bB");
            BR = ImageHelper.LoadPiece("bR");
            BQ = ImageHelper.LoadPiece("bQ");
            BK = ImageHelper.LoadPiece("bK");
        }

        public static class ImageHelper
        {
            public static Bitmap LoadFromResource(Uri resourceUri)
            {
                var ret = new Bitmap(AssetLoader.Open(resourceUri));

                ret = ret.CreateScaledBitmap(new Avalonia.PixelSize(50, 50), BitmapInterpolationMode.HighQuality);

                return ret;
            }

            public static Bitmap LoadPiece(string pieceName)
            {
                return LoadFromResource(new Uri(Path + pieceName + ".png"));
            }

            public static async Task<Bitmap?> LoadFromWeb(Uri url)
            {
                using var httpClient = new HttpClient();
                try
                {
                    var response = await httpClient.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    var data = await response.Content.ReadAsByteArrayAsync();
                    return new Bitmap(new MemoryStream(data));
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"An error occurred while downloading image '{url}' : {ex.Message}");
                    return null;
                }
            }
        }
    }
}
