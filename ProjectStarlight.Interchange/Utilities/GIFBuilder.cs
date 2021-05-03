using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using XnaColor = Microsoft.Xna.Framework.Color;

namespace ProjectStarlight.Interchange.Utilities
{
    /// <summary>
    ///     Provides methods for creating <see cref="TextureGIF"/>s.
    /// </summary>
    public static class GIFBuilder
    {
        /// <summary>
        ///     Creates a <see cref="TextureGIF"/> from an array of <see cref="Texture2D"/>s.
        /// </summary>
        public static TextureGIF FromArray(Texture2D[] frames, int ticksPerFrame) =>
            new TextureGIF(frames, ticksPerFrame);

        /// <summary>
        ///     Creates a <see cref="TextureGIF"/> from a <see cref="FileStream"/>, opened and read from the <paramref name="filePath"/>.
        /// </summary>
        public static TextureGIF FromGIFFile(string filePath, GraphicsDevice graphicsDevice, int ticksPerFrame)
        {
            TextureGIF gif;

            using (FileStream stream = File.OpenRead(filePath))
            {
                gif = FromGIFFile(stream, graphicsDevice, ticksPerFrame);
            }

            return gif;
        }

        // https://dejanstojanovic.net/aspnet/2018/march/getting-gif-image-information-using-c/
        /// <summary>
        ///     Creates a <see cref="TextureGIF"/> from a <see cref="FileStream"/>.
        /// </summary>
        public static TextureGIF FromGIFFile(FileStream stream, GraphicsDevice graphicsDevice, int ticksPerFrame)
        {
            using (Image image = Image.FromStream(stream))
            {
                if (!image.RawFormat.Equals(ImageFormat.Gif))
                    throw new Exception("Given file was not a GIF!");

                if (!ImageAnimator.CanAnimate(image))
                    throw new Exception("Can't animate the GIF file!");

                List<Image> frames = new List<Image>();
                List<Texture2D> trueFrames = new List<Texture2D>();

                FrameDimension dimension = new FrameDimension(image.FrameDimensionsList[0]);
                int frameCount = image.GetFrameCount(dimension);

                // Populate list of frames to read
                for (int i = 0; i < frameCount; i++)
                {
                    image.SelectActiveFrame(dimension, i);
                    frames.Add(image.Clone() as Image);
                }

                foreach (Image frame in frames)
                {
                    // finally use bitmaps yay
                    Bitmap gifFrame = new Bitmap(frame);
                    int width = gifFrame.Width;
                    int height = gifFrame.Height;
                    Color[,] framePixels = new Color[width, height];
                    int unflattenedSize = framePixels.Length;
                    Color[] flattenedPixels = new Color[unflattenedSize];

                    for (int x = 0; x < width; x++)
                    for (int y = 0; y < height; y++)
                        framePixels[x, y] = gifFrame.GetPixel(x, y);
                    framePixels = Flip(Rotate(framePixels));

                    int flatIndex = 0;
                    for (int x = 0; x < height; x++)
                    for (int y = 0; y < width; y++)
                        flattenedPixels[flatIndex++] = framePixels[x, y];

                    XnaColor[] usableColors = new XnaColor[flattenedPixels.Length];
                    for (int i = 0; i < usableColors.Length; i++)
                    {
                        Color temp = flattenedPixels[i];
                        usableColors[i] = new XnaColor(temp.R, temp.G, temp.B, temp.A);
                    }

                    Texture2D trueFrame = new Texture2D(graphicsDevice, width, height);
                    trueFrame.SetData(usableColors);
                    trueFrames.Add(trueFrame);
                    frame.Dispose();
                }

                return FromArray(trueFrames.ToArray(), ticksPerFrame);
            }
        }
        private static Color[,] Rotate(Color[,] src)
        {
            int width = src.GetUpperBound(0) + 1;
            int height = src.GetUpperBound(1) + 1;
            Color[,] value = new Color[height, width];

            for (int row = 0; row < height; row++)
            for (int col = 0; col < width; col++)
            {
                int newCol = height - (row + 1);
                value[newCol, col] = src[col, row];
            }
            

            return value;
        }

        private static Color[,] Flip(Color[,] src)
        {
            int rows = src.GetUpperBound(0) + 1;
            int cols = src.GetUpperBound(1) + 1;
            Color[,] value = new Color[rows, cols];

            for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                value[rows - 1 - i, j] = src[i, j];

            return value;
        }
    }
}
