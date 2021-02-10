using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Tiles.Permafrost
{
    internal class BossGate : DummyTile
    {
        public override int DummyType => ProjectileType<BossGateDummy>();

        public override bool Autoload(ref string name, ref string texture)
        {
            texture = "StarlightRiver/Assets/Invisible";
            return true;
        }

        public override void SetDefaults()
        {
            QuickBlock.QuickSetFurniture(this, 21, 4, 0, SoundID.Tink, false, new Color(100, 120, 200));
        }

        public override void SafeNearbyEffects(int i, int j, bool closer)
        {
            Main.tileSolid[Type] = !StarlightWorld.HasFlag(WorldFlags.SquidBossDowned);
        }
    }

    internal class BossGateDummy : Dummy
    {
        public BossGateDummy() : base(TileType<BossGate>(), 21 * 16, 4 * 16) { }

        public override void SafeSetDefaults()
        {
            projectile.hide = true;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D tex = GetTexture("StarlightRiver/Assets/Tiles/Permafrost/BossGate");

            Vector2 off = Vector2.UnitX * (projectile.ai[0] / 120 * projectile.width / 2);

            if (projectile.ai[0] > 0 && projectile.ai[0] < 120)
                off += Vector2.One.RotatedByRandom(6.28f) * 0.5f;

            var color = Lighting.GetColor((int)projectile.Center.X / 16, (int)projectile.Center.Y / 16);
            spriteBatch.Draw(tex, projectile.position - Main.screenPosition - off, color);
            spriteBatch.Draw(tex, projectile.position - Main.screenPosition + Vector2.UnitX * projectile.width / 2 + off, null, color, 0, Vector2.Zero, 1, SpriteEffects.FlipHorizontally, 0);
        }

        public override void Update()
        {
            if (StarlightWorld.HasFlag(WorldFlags.SquidBossDowned) && projectile.ai[0] < 120)
                projectile.ai[0]++;
            if (!StarlightWorld.HasFlag(WorldFlags.SquidBossDowned) && projectile.ai[0] > 0)
                projectile.ai[0]--;

            if (projectile.ai[0] > 0 && projectile.ai[0] < 120)
            {
                Dust.NewDustPerfect(projectile.position + Vector2.UnitY * Main.rand.NextFloat(projectile.height), DustType<Dusts.Stone>());
                Dust.NewDustPerfect(projectile.position + new Vector2(projectile.width, Main.rand.NextFloat(projectile.height)), DustType<Dusts.Stone>());
            }

            if (projectile.ai[0] == 119)
            {
                Main.PlaySound(SoundID.Tink, (int)projectile.Center.X, (int)projectile.Center.Y, 0, 1, -2);
                Main.LocalPlayer.GetModPlayer<StarlightPlayer>().Shake += 7;
            }
        }

        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
        {
            drawCacheProjsBehindNPCsAndTiles.Add(index);
        }
    }
}