//TODO:
//Manual loading
//Implement final textures

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Content.Archaeology;
using StarlightRiver.Content.Dusts.ArtifactSparkles;
using StarlightRiver.Content.Items.BuriedArtifacts;
using StarlightRiver.Helpers;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Content.Archaeology.OreCores
{
    public abstract class OreCore : Artifact
    {
        public int frameNumber = -1;

        public virtual int DustType { get; }

        public virtual int Tile { get; }

        public virtual int ItemType { get; }

        public override string TexturePath => AssetDirectory.OreCores + Name;

        public override Vector2 Size => new Vector2(32, 32);

        public override float SpawnChance => 1f;

        public override int SparkleRate => 40;

        public override bool SpawnProjectile => false;

        public override bool CanBeRevealed() => false;

        public override bool AboveTiles => true;

        public override bool CanGenerate(int i, int j) 
        {
            for (int x = 0; x < Size.X / 16; x++)
                for (int y = 0; y < Size.Y / 16; y++)
                {
                    Tile testTile = Main.tile[x + i, y + j];
                    if (!testTile.HasTile || testTile.TileType != Tile)
                        return false;

                }
            return true;
        }

        public override void Update()
        {
            if (frameNumber == -1)
                frameNumber = Main.rand.Next(3);

            base.Update();
        }

        public override void LoadData(TagCompound tag)
        {
            base.LoadData(tag);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>(TexturePath).Value;

            Vector2 offScreen = new Vector2(Main.offScreenRange);
            if (Main.drawToScreen)
            {
                offScreen = Vector2.Zero;
            }

            for (int i = 0; i < 2; i++)
                for (int j = 0; j < 2; j++)
                {
                    Rectangle frame = new Rectangle(0 + (i * 16), (32 * frameNumber) + (j * 16), 16, 16);
                    Vector2 offset = new Vector2(i, j) * 16;
                    Color color = Lighting.GetColor(Position.ToPoint() + new Point(i, j));
                    spriteBatch.Draw(tex, (WorldPosition - Main.screenPosition) + offScreen + offset, frame, color, 0, Vector2.Zero, 1, SpriteEffects.None, 0f);
                }
        }

        public override void OnKill()
        {
            Item.NewItem(new EntitySource_Misc("Artifact"), WorldPosition, Size, ItemType, Main.rand.Next(10,20));

            for (int i = 0; i < 30; i++)
                Dust.NewDust(WorldPosition, (int)Size.X, (int)Size.Y, DustType);

            Terraria.Audio.SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundImpact, WorldPosition);
        }

        public override bool Open()
        {
            int tileCounter = 0;
            for (int i = 0; i < Size.X / 16; i++)
                for (int j = 0; j < Size.Y / 16; j++)
                {
                    Tile tile = Main.tile[i + Position.X, j + Position.Y];
                    if (tile.HasTile)
                        tileCounter++;
                }

            return tileCounter < 3;
        }
    }

    public class CopperCore : OreCore
    { 
        public override int Tile => TileID.Copper;

        public override int SparkleDust => ModContent.DustType<CopperArtifactSparkle>();

        public override int ItemType => ItemID.CopperOre;

        public override int DustType => DustID.Copper;
    }

    public class TinCore : OreCore 
    {
        public override int Tile => TileID.Tin;

        public override int SparkleDust => ModContent.DustType<TinArtifactSparkle>();

        public override int ItemType => ItemID.TinOre;

        public override int DustType => DustID.Tin;
    }

    public class LeadCore : OreCore
    {
        public override int Tile => TileID.Lead;

        public override int SparkleDust => ModContent.DustType<LeadArtifactSparkle>();

        public override int ItemType => ItemID.LeadOre;

        public override int DustType => DustID.Lead;
    }

    public class IronCore : OreCore
    { 
        public override int Tile => TileID.Iron;

        public override int SparkleDust => ModContent.DustType<IronArtifactSparkle>();

        public override int ItemType => ItemID.IronOre;

        public override int DustType => DustID.Iron;
    }

    public class TungstenCore : OreCore 
    { 
        public override int Tile => TileID.Tungsten;

        public override int SparkleDust => ModContent.DustType<TungstenArtifactSparkle>();

        public override int ItemType => ItemID.TungstenOre;

        public override int DustType => DustID.Tungsten;
    }

    public class SilverCore : OreCore
    { 
        public override int Tile => TileID.Silver;

        public override int SparkleDust => ModContent.DustType<SilverArtifactSparkle>();

        public override int ItemType => ItemID.SilverOre;

        public override int DustType => DustID.Silver;
    }

    public class GoldCore : OreCore 
    {
        public override int Tile => TileID.Gold;

        public override int SparkleDust => ModContent.DustType<GoldArtifactSparkle>();

        public override int ItemType => ItemID.GoldOre;

        public override int DustType => DustID.Gold;
    }

    public class PlatinumCore : OreCore
    { 
        public override int Tile => TileID.Platinum;

        public override int SparkleDust => ModContent.DustType<PlatinumArtifactSparkle>();

        public override int ItemType => ItemID.PlatinumOre;

        public override int DustType => DustID.Platinum;
    }



}