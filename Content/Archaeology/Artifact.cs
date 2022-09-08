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
        public bool displayedOnMap = false;

        public virtual string TexturePath { get;}

        public virtual Vector2 Size { get; }

        public virtual int SparkleDust { get; }

        public virtual int SparkleRate { get; }

        public virtual Color BeamColor { get; }

        public virtual int ItemType { get; }

        public Vector2 WorldPosition => Position.ToVector2() * 16;

        public virtual void Draw(SpriteBatch spriteBatch) { }

        public override void Update()
        {
            CheckOpen();
        }

        public bool IsOnScreen()
        {
            return Helper.OnScreen(new Rectangle((int)WorldPosition.X - (int)Main.screenPosition.X, (int)WorldPosition.Y - (int)Main.screenPosition.Y, (int)Size.X, (int)Size.Y));
        }

        public void CreateSparkles()
        {
            if (Main.rand.NextBool(SparkleRate))
            {
                Vector2 pos = WorldPosition + (Size * new Vector2(Main.rand.NextFloat(), Main.rand.NextFloat()));

                if (Lighting.GetColor((pos / 16).ToPoint()) != Color.Black)
                    Dust.NewDustPerfect(WorldPosition + (Size * new Vector2(Main.rand.NextFloat(), Main.rand.NextFloat())), SparkleDust, Vector2.Zero);
            }
        }

        public void GenericDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>(TexturePath).Value;
            spriteBatch.Draw(tex, (WorldPosition - Main.screenPosition) + new Vector2(192, 192), null, Lighting.GetColor(Position.ToPoint()), 0, Vector2.Zero, 1, SpriteEffects.None, 0f);
        }

        public void CheckOpen()
        {
            for (int i = 0; i < Size.X / 16; i++)
                for (int j = 0; j < Size.Y / 16; j++)
                {
                    Tile tile = Main.tile[i + Position.X, j + Position.Y];
                    if (tile.HasTile)
                        return;
                }

            Kill(Position.X, Position.Y);

            Projectile proj = Projectile.NewProjectileDirect(new EntitySource_Misc("Artifact"), WorldPosition, new Vector2(0, -0.5f), ModContent.ProjectileType<ArtifactItemProj>(), 0, 0);
            ArtifactItemProj modProj = proj.ModProjectile as ArtifactItemProj;
            modProj.itemTexture = TexturePath;
            modProj.glowColor = BeamColor;
            modProj.itemType = ItemType;
            modProj.size = Size;
            modProj.sparkleType = SparkleDust;

        }
    }
}