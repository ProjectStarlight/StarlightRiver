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

            Dust.NewDustPerfect(projectile.Center + new Vector2(200, -280), DustType<Dusts.Air>(), Vector2.Zero);

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