using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Content.Archaeology
{
    public abstract class Artifact : ModTileEntity
	{
        public virtual string TexturePath { get;}

        public virtual Vector2 Size { get; }

        public virtual int SparkleDust { get; }

        public virtual int SparkleRate { get; }

        public Vector2 WorldPosition => Position.ToVector2() * 16;

        public virtual void Draw(SpriteBatch spriteBatch) { }

        public bool IsOnScreen()
        {
            return Helper.OnScreen(new Rectangle((int)WorldPosition.X - (int)Main.screenPosition.X, (int)WorldPosition.Y - (int)Main.screenPosition.Y, (int)Size.X, (int)Size.Y));
        }

        public void CreateSparkles()
        {
            if (Main.rand.NextBool(SparkleRate))
            {
                Dust.NewDustPerfect(WorldPosition + (Size * new Vector2(Main.rand.NextFloat(), Main.rand.NextFloat())), SparkleDust, Vector2.Zero);
            }
        }
    }
}