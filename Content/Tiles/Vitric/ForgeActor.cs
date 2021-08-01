using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Bosses.GlassMiniboss;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System.Collections.Generic;

namespace StarlightRiver.Content.Tiles.Vitric
{
    class ForgeActor : DummyTile
    {
        public override int DummyType => ProjectileType<ForgeActorDummy>();

        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.Invisible;
            return true;
        }

        public override void SetDefaults() => (this).QuickSetFurniture(1, 1, DustType<Content.Dusts.Air>(), SoundID.Shatter, false, Color.Black);
    }

    class ForgeActorDummy : Dummy
    {
        public ForgeActorDummy() : base(TileType<ForgeActor>(), 16, 16) { }

		public override void SafeSetDefaults()
		{
            projectile.hide = true;
		}

		public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
		{
            drawCacheProjsBehindNPCsAndTiles.Add(index);
        }

		public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
		{
            Vector2 pos = projectile.position + new Vector2(-608, -289) - Main.screenPosition;
            Texture2D backdrop = GetTexture(AssetDirectory.GlassMiniboss + "Backdrop");
            Texture2D backdropGlow = GetTexture(AssetDirectory.GlassMiniboss + "BackdropGlow");

            var frame = new Rectangle(0, (backdrop.Height / 3) * (int)(Main.GameUpdateCount / 8 % 3), backdrop.Width, backdrop.Height / 3);

            LightingBufferRenderer.DrawWithLighting(pos, backdrop, frame);
            spriteBatch.Draw(backdropGlow, pos, frame, Color.White);

            if(Main.rand.Next(3) == 0)
                Dust.NewDustPerfect(projectile.Center + new Vector2(204 + Main.rand.Next(-10, 10), -94), DustType<Dusts.LavaSpark>(), -Vector2.UnitY.RotatedByRandom(0.1f).RotatedBy(Main.rand.NextBool() ? 0.6f : -0.6f) * Main.rand.NextFloat(1, 1.5f), 0, new Color(255, Main.rand.Next(150, 200), 80), 0.30f);

            if (Main.rand.Next(3) == 0)
                Dust.NewDustPerfect(projectile.Center + new Vector2(0 + Main.rand.Next(-10, 10), -240), DustType<Dusts.LavaSpark>(), -Vector2.UnitY.RotatedByRandom(0.1f).RotatedBy(Main.rand.NextBool() ? 0.6f : -0.6f) * Main.rand.NextFloat(1, 1.5f), 0, new Color(255, Main.rand.Next(150, 200), 80), 0.30f);

            if (Main.rand.Next(5) == 0)
            {
                var d = Dust.NewDustPerfect(projectile.Center + new Vector2(-110 + Main.rand.Next(120), -130), DustType<Dusts.LavaSpark>(), -Vector2.UnitY.RotatedByRandom(0.1f) * Main.rand.NextFloat(0.1f, 0.6f), 0, new Color(255, 220, 120), Main.rand.NextFloat(0.2f));
                d.noGravity = true;

                d = Dust.NewDustPerfect(projectile.Center + new Vector2(-250 + Main.rand.Next(80), -66), DustType<Dusts.LavaSpark>(), -Vector2.UnitY.RotatedByRandom(0.1f) * Main.rand.NextFloat(0.1f, 0.6f), 0, new Color(255, 220, 120), Main.rand.NextFloat(0.2f));
                d.noGravity = true;

                var dist = Main.rand.Next(80);
                d = Dust.NewDustPerfect(projectile.Center + new Vector2(10 + dist, -130 + dist), DustType<Dusts.LavaSpark>(), -Vector2.UnitY.RotatedByRandom(0.1f) * Main.rand.NextFloat(0.1f, 0.6f), 0, new Color(255, 220, 120), Main.rand.NextFloat(0.2f));
                d.noGravity = true;

                dist = Main.rand.Next(60);
                d = Dust.NewDustPerfect(projectile.Center + new Vector2(-110 + dist, -196 - dist), DustType<Dusts.LavaSpark>(), -Vector2.UnitY.RotatedByRandom(0.1f) * Main.rand.NextFloat(0.1f, 0.6f), 0, new Color(255, 220, 120), Main.rand.NextFloat(0.2f));
                d.noGravity = true;
            }

            for (int k = 0; k < 5; k++)
            {
                Lighting.AddLight(projectile.Center + new Vector2(200, -280 + k * 35), new Vector3(1, 0.8f, 0.5f));
            }

            Lighting.AddLight(projectile.Center + new Vector2(-20, -280), new Vector3(1, 0.8f, 0.5f) * 1.1f);
            Lighting.AddLight(projectile.Center + new Vector2(-80, -220), new Vector3(1, 0.8f, 0.5f) * 1.1f);
            Lighting.AddLight(projectile.Center + new Vector2(40, -90), new Vector3(1, 0.8f, 0.5f) * 1.1f);

            Lighting.AddLight(projectile.Center + new Vector2(-260, -80), new Vector3(1, 0.8f, 0.5f) * 0.8f);

            for (int k = 0; k < 3; k++)
            {
                Lighting.AddLight(projectile.Center + new Vector2(130, 200 + k * 35), new Vector3(1, 0.8f, 0.5f) * 1.1f);
                Lighting.AddLight(projectile.Center + new Vector2(-130, 200 + k * 35), new Vector3(1, 0.8f, 0.5f) * 1.1f);
            }
        }
    }
}