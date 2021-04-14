using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Permafrost
{
    class DiscGate : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = "StarlightRiver/Assets/Tiles/Permafrost/DiscGate";
            return true;
        }

        public override void SetDefaults() => QuickBlock.QuickSet(this, 0, DustID.Ice, SoundID.Tink, new Color(100, 255, 255), ItemType<DiscGateItem>());

        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            Framing.GetTileSafely(i, j).active(false); //to prevent recursion

            for (int x = -1; x <= 1; x++)
                for (int y = -1; y <= 1; y++)
                {
                    Tile tile = Framing.GetTileSafely(i + x, j + y);

                    if (tile.active() && tile.type == TileType<DiscGate>())
                        WorldGen.KillTile(i + x, j + y);
                }
        }
    }

    class DiscHole : DummyTile
    {
        public override int DummyType => ProjectileType<DiscHoleDummy>();

        public override void SetDefaults() => QuickBlock.QuickSet(this, 0, DustID.Ice, SoundID.Tink, new Color(100, 255, 255), ItemType<DiscGateItem>());

        public override bool Autoload(ref string name, ref string texture)
        {
            texture = "StarlightRiver/Assets/Invisible";
            return true;
        }

        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            for (int x = -1; x <= 1; x++)
                for (int y = -1; y <= 1; y++)
                {
                    Tile tile = Framing.GetTileSafely(i + x, j + y);

                    if (tile.type == TileType<DiscGate>())
                        WorldGen.KillTile(i + x, j + y);
                }
        }
    }

    class DiscHoleDummy : Dummy
    {
        public DiscHoleDummy() : base(TileType<DiscHole>(), 16, 16) { }

        public override void SafeSetDefaults()
        {
            projectile.hide = true;
        }

        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
        {
            drawCacheProjsBehindProjectiles.Add(index);
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            var tex = GetTexture("StarlightRiver/Assets/Tiles/Permafrost/DiscHole");
            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, Lighting.GetColor((int)projectile.Center.X / 16, (int)projectile.Center.Y / 16), 0, tex.Size() / 2, 1, 0, 0);
        }

        public void OpenGate() => WorldGen.KillTile(ParentX, ParentY);
    }

    class DiscGateItem : QuickTileItem
    {
        public override string Texture => AssetDirectory.Debug;

        public DiscGateItem() : base("Disc Gate", "Destroyed when opened with an aurora disc", TileType<DiscGate>(), ItemRarityID.White) { }
    }

    class DiscGateItem2 : QuickTileItem
    {
        public override string Texture => AssetDirectory.Debug;

        public DiscGateItem2() : base("Disc Gate 2", "Le Hole", TileType<DiscHole>(), ItemRarityID.White) { }
    }
}
