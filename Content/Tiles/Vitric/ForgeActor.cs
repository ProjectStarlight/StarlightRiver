using StarlightRiver.Core.Systems.DummyTileSystem;
using StarlightRiver.Core.Systems.LightingSystem;
using System.Collections.Generic;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Vitric
{
	class ForgeActor : DummyTile
	{
		public override int DummyType => ProjectileType<ForgeActorDummy>();

		public override string Texture => AssetDirectory.Invisible;

		public override void SetStaticDefaults()
		{
			this.QuickSetFurniture(1, 1, DustType<Dusts.Air>(), SoundID.Shatter, false, Color.Black);
		}
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
			Player player = Main.player[Main.myPlayer];

			Vector2 pos = Projectile.position - new Vector2(567, 400) - Main.screenPosition;
			Texture2D backdrop = Request<Texture2D>(AssetDirectory.Glassweaver + "Backdrop").Value;
			Texture2D backdropGlow = Request<Texture2D>(AssetDirectory.Glassweaver + "BackdropGlow").Value;

			Vector2 parallaxOffset = new Vector2(Main.screenPosition.X + Main.screenWidth / 2f - Projectile.position.X, 0) * 0.15f;
			Texture2D farBackdrop = Request<Texture2D>(AssetDirectory.Glassweaver + "FarBackdrop").Value;
			Texture2D farBackdropGlow = Request<Texture2D>(AssetDirectory.Glassweaver + "FarBackdropGlow").Value;

			Texture2D backdropBlack = Request<Texture2D>(AssetDirectory.Glassweaver + "BackdropBlack").Value;

			var frame = new Rectangle(0, 0, backdrop.Width, backdrop.Height);

			LightingBufferRenderer.DrawWithLighting(pos, backdropBlack, frame);

			LightingBufferRenderer.DrawWithLighting(pos + parallaxOffset, farBackdrop, frame);
			//spriteBatch.Draw(backdropGlow, pos, frame, Color.White);

			LightingBufferRenderer.DrawWithLighting(pos, backdrop, frame);
			//spriteBatch.Draw(backdropGlow, pos, frame, Color.White);

			Lighting.AddLight(pos + Main.screenPosition + new Vector2(950, 320), new Vector3(1, 0.8f, 0.4f) * 3f);
			Lighting.AddLight(pos + Main.screenPosition + new Vector2(160, 320), new Vector3(1, 0.8f, 0.4f) * 3f);

			float pulse = (float)System.Math.Sin(Main.GameUpdateCount * 0.1f) + (float)System.Math.Cos(Main.GameUpdateCount * 0.024f);

			Lighting.AddLight(pos + Main.screenPosition + new Vector2(555, 220), new Vector3(1, 0.6f, 0.4f) * (2f + pulse * 0.5f));
		}
	}
}