using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectStarlight.Interchange.Utilities;

namespace ProjectStarlight.Interchange
{
    /// <summary>
    ///     Simple class containing GIF data for drawing and updating.
    /// </summary>
    public class TextureGIF
    {
        /// <summary>
        ///     The width of the GIF.
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        ///     The height of the GIF.
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        ///     Whether the GIF is paused.
        /// </summary>
        public bool IsPaused { get; private set; }

        /// <summary>
        ///     Whether the GIF has ended. Never true of the GIF loops (<seealso cref="ShouldLoop"/>).
        /// </summary>
        public bool HasEnded => FrameIndex >= Frames.Length && FrameTick >= TicksPerFrame && !ShouldLoop;

        /// <summary>
        ///     Whether the GIF loops. Prevents the GIF from ending unless <seealso cref="Stop"/> is called.
        /// </summary>
        public bool ShouldLoop { get; set; }

        /// <summary>
        ///     The amount of ticks per frame. Once the tick threshold is reached, goes to a new frame.
        /// </summary>
        public int TicksPerFrame { get; set; }

        /// <summary>
        ///     The current tick the frame is on.
        /// </summary>
        public int FrameTick { get; private set; }

        /// <summary>
        ///     The index of <seealso cref="Frames"/> that should be drawn.
        /// </summary>
        public int FrameIndex { get; private set; }

        /// <summary>
        ///     An array of <seealso cref="Texture2D"/>s representing the frames of a GIF.
        /// </summary>
        public Texture2D[] Frames { get; private set; }

        /// <summary>
        ///     Gets the current frame in accordance to <seealso cref="Frames"/>, using <seealso cref="FrameIndex"/> as the index.
        /// </summary>
        public Texture2D CurrentFrame => HasEnded ? Frames.Last() : Frames[FrameIndex];

        /// <summary>
        ///     Creates a <see cref="TextureGIF"/> instance from an array of <see cref="Texture2D"/>s, where the <see cref="Texture2D"/>s serve as frames.
        /// </summary>
        internal TextureGIF(Texture2D[] frames, int ticksPerFrame)
        {
            FrameUtilities.VerifyGIFDimensions(frames, out int width, out int height);
            Width = width;
            Height = height;
            TicksPerFrame = ticksPerFrame;
            FrameIndex = 0;
            Frames = frames;
        }

        // ReSharper disable once InvalidXmlDocComment
        /// <summary>
        ///     Restarts (or officially starts, if not started previously) the GIF. Render the GIF with <see cref="Draw"/>.
        /// </summary>
        public void Play()
        {
            FrameTick = 0;
            FrameIndex = 0;

            if (IsPaused)
                SwitchPauseState();
        }

        /// <summary>
        ///     Completely stops a GIF. If you want to pause a GIF, use <see cref="SwitchPauseState"/>.
        /// </summary>
        public void Stop()
        {
            // This essentially sets HasEnded to true.
            FrameTick = Frames.Length - 1;
            FrameTick = TicksPerFrame;
            ShouldLoop = false;
        }

        /// <summary>
        ///     Attempts to change the array of <see cref="Texture2D"/>s to draw.
        /// </summary>
        /// <param name="frames">The <see cref="Texture2D"/>s to set.</param>
        /// <returns>Returns <c>true</c> if successful, otherwise <c>false</c>.</returns>
        public bool TryChangeGIF(Texture2D[] frames)
        {
            if (FrameIndex >= frames.Length)
                return false;

            try
            {
                FrameUtilities.VerifyGIFDimensions(frames, out int width, out int height);
                Width = width;
                Height = height;
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Pauses and unpauses the GIF depending on its current state.
        /// </summary>
        public void SwitchPauseState() =>
            // TODO: Find out if there's anything that needs to be done when pausing and unpausing.
            IsPaused = !IsPaused;

        public void Draw(SpriteBatch spriteBatch, Vector2 position, Color color) =>
            spriteBatch.Draw(CurrentFrame, position, color);

        public void Draw(SpriteBatch spriteBatch, Vector2 position, Rectangle? sourceRectangle, Color color) =>
            spriteBatch.Draw(CurrentFrame, position, sourceRectangle, color);

        public void Draw(SpriteBatch spriteBatch, Vector2 position, Rectangle? sourceRectangle, Color color,
            float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth) =>
            spriteBatch.Draw(CurrentFrame, position, sourceRectangle, color, rotation, origin, scale, effects,
                layerDepth);

        public void Draw(SpriteBatch spriteBatch, Vector2 position, Rectangle? sourceRectangle, Color color,
            float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth) =>
            spriteBatch.Draw(CurrentFrame, position, sourceRectangle, color, rotation, origin, scale, effects,
                layerDepth);

        public void Draw(SpriteBatch spriteBatch, Rectangle destinationRectangle, Color color) =>
            spriteBatch.Draw(CurrentFrame, destinationRectangle, color);

        public void Draw(SpriteBatch spriteBatch, Rectangle destinationRectangle, Rectangle? sourceRectangle,
            Color color) => spriteBatch.Draw(CurrentFrame, destinationRectangle, sourceRectangle, color);

        public void Draw(SpriteBatch spriteBatch, Rectangle destinationRectangle, Rectangle? sourceRectangle,
            Color color, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth) =>
            spriteBatch.Draw(CurrentFrame, destinationRectangle, sourceRectangle, color, rotation, origin, effects,
                layerDepth);

        /// <summary>
        ///     Increment ticks by one if the GIF is not paused and has not ended.
        /// </summary>
        public void UpdateGIF()
        {
            if (!IsPaused && !HasEnded) 
                ForwardTicks(1);
        }

        /// <summary>
        ///     Increments the specified amount of ticks. You can use this to skip frames as well.
        /// </summary>
        public void ForwardTicks(int tickAmount)
        {
            for (int i = 0; i < tickAmount; i++)
            {
                FrameTick++;

                if (FrameTick < TicksPerFrame)
                    return;

                FrameTick = 0;

                if (FrameIndex < Frames.Length - 1) 
                    FrameIndex++;
                else if (ShouldLoop)
                    FrameIndex = 0;
            }
        }
    }
}