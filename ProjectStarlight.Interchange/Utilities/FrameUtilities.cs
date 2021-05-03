using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectStarlight.Interchange.Utilities
{
    /// <summary>
    ///     Simple class providing small, useful methods for some common tasks.
    /// </summary>
    public static class FrameUtilities
    {
        /// <summary>
        ///     Validates GIF framing given the provided frames.
        /// </summary>
        public static void VerifyGIFDimensions(IEnumerable<Texture2D> frames) =>
            VerifyGIFDimensions(frames, out int _, out int _);

        /// <summary>
        ///     Validates GIF framing given the provided frames.
        /// </summary>
        public static void VerifyGIFDimensions(IEnumerable<Texture2D> frames, out int width, out int height)
        {
            width = 0;
            height = 0;

            foreach (Texture2D frame in frames)
            {
                if (width == 0)
                    width = frame.Width;
                else if (width != frame.Width)
                    throw new Exception("Inconsistent frame width in Texture2D array!");

                if (height == 0)
                    height = frame.Height;
                else if (height != frame.Height)
                    throw new Exception("Inconsistent frame height in Texture2D array!");
            }
        }
    }
}
