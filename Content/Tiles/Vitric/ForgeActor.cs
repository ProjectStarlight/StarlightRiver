﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Vitric
{
	class ForgeActor : DummyTile
    {
        public override int DummyType => ProjectileType<ForgeActorDummy>();

        public override string Texture => AssetDirectory.Invisible;

        public override void SetStaticDefaults() => (this).QuickSetFurniture(1, 1, DustType<Content.Dusts.Air>(), SoundID.Shatter, false, Color.Black);
    }

    class ForgeActorDummy : Dummy
    {
        public ForgeActorDummy() : base(TileType<ForgeActor>(), 16, 16) { }

		public override void SafeSetDefaults()
		{
            Projectile.hide = true;
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
            behindNPCsAndTiles.Add(index);
		}

		public override void PostDraw(Color lightColor)
		{
            var spriteBatch = Main.spriteBatch;

            Vector2 pos = Projectile.position + new Vector2(-608, -289) - Main.screenPosition;
            Texture2D backdrop = Request<Texture2D>(AssetDirectory.Glassweaver + "Backdrop").Value;
            Texture2D backdropGlow = Request<Texture2D>(AssetDirectory.Glassweaver + "BackdropGlow").Value;

            var frame = new Rectangle(0, (backdrop.Height / 3) * (int)(Main.GameUpdateCount / 8 % 3), backdrop.Width, backdrop.Height / 3);

            LightingBufferRenderer.DrawWithLighting(pos, backdrop, frame);
            spriteBatch.Draw(backdropGlow, pos, frame, Color.White);

            if(Main.rand.Next(3) == 0)
                Dust.NewDustPerfect(Projectile.Center + new Vector2(204 + Main.rand.Next(-10, 10), -94), DustType<Dusts.LavaSpark>(), -Vector2.UnitY.RotatedByRandom(0.1f).RotatedBy(Main.rand.NextBool() ? 0.6f : -0.6f) * Main.rand.NextFloat(1, 1.5f), 0, new Color(255, Main.rand.Next(150, 200), 80), 0.30f);

            if (Main.rand.Next(3) == 0)
                Dust.NewDustPerfect(Projectile.Center + new Vector2(0 + Main.rand.Next(-10, 10), -240), DustType<Dusts.LavaSpark>(), -Vector2.UnitY.RotatedByRandom(0.1f).RotatedBy(Main.rand.NextBool() ? 0.6f : -0.6f) * Main.rand.NextFloat(1, 1.5f), 0, new Color(255, Main.rand.Next(150, 200), 80), 0.30f);

            if (Main.rand.Next(5) == 0)
            {
                var d = Dust.NewDustPerfect(Projectile.Center + new Vector2(-110 + Main.rand.Next(120), -130), DustType<Dusts.LavaSpark>(), -Vector2.UnitY.RotatedByRandom(0.1f) * Main.rand.NextFloat(0.1f, 0.6f), 0, new Color(255, 220, 120), Main.rand.NextFloat(0.2f));
                d.noGravity = true;

                d = Dust.NewDustPerfect(Projectile.Center + new Vector2(-250 + Main.rand.Next(80), -66), DustType<Dusts.LavaSpark>(), -Vector2.UnitY.RotatedByRandom(0.1f) * Main.rand.NextFloat(0.1f, 0.6f), 0, new Color(255, 220, 120), Main.rand.NextFloat(0.2f));
                d.noGravity = true;

                var dist = Main.rand.Next(80);
                d = Dust.NewDustPerfect(Projectile.Center + new Vector2(10 + dist, -130 + dist), DustType<Dusts.LavaSpark>(), -Vector2.UnitY.RotatedByRandom(0.1f) * Main.rand.NextFloat(0.1f, 0.6f), 0, new Color(255, 220, 120), Main.rand.NextFloat(0.2f));
                d.noGravity = true;

                dist = Main.rand.Next(60);
                d = Dust.NewDustPerfect(Projectile.Center + new Vector2(-110 + dist, -196 - dist), DustType<Dusts.LavaSpark>(), -Vector2.UnitY.RotatedByRandom(0.1f) * Main.rand.NextFloat(0.1f, 0.6f), 0, new Color(255, 220, 120), Main.rand.NextFloat(0.2f));
                d.noGravity = true;
            }

            for (int k = 0; k < 5; k++)
            {
                Lighting.AddLight(Projectile.Center + new Vector2(200, -280 + k * 35), new Vector3(1, 0.8f, 0.5f));
            }

            Lighting.AddLight(Projectile.Center + new Vector2(-20, -280), new Vector3(1, 0.8f, 0.5f) * 1.1f);
            Lighting.AddLight(Projectile.Center + new Vector2(-80, -220), new Vector3(1, 0.8f, 0.5f) * 1.1f);
            Lighting.AddLight(Projectile.Center + new Vector2(40, -90), new Vector3(1, 0.8f, 0.5f) * 1.1f);

            Lighting.AddLight(Projectile.Center + new Vector2(-260, -80), new Vector3(1, 0.8f, 0.5f) * 0.8f);

            for (int k = 0; k < 3; k++)
            {
                Lighting.AddLight(Projectile.Center + new Vector2(130, 200 + k * 35), new Vector3(1, 0.8f, 0.5f) * 1.1f);
                Lighting.AddLight(Projectile.Center + new Vector2(-130, 200 + k * 35), new Vector3(1, 0.8f, 0.5f) * 1.1f);
            }
        }
    }
}