using StarlightRiver.Helpers;
using System.Collections.Generic;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Vitric.Temple
{
	class WindsRoomActor : DummyTile
	{
		public override int DummyType => ProjectileType<WindsRoomActorDummy>();

		public override string Texture => AssetDirectory.Invisible;

		public override void SetStaticDefaults()
		{
			this.QuickSetFurniture(1, 1, DustType<Dusts.Air>(), SoundID.Shatter, false, Color.Black);
		}
	}

	class WindsRoomActorItem : QuickTileItem
	{
		public WindsRoomActorItem() : base("Winds Room Actor", "Debug Item", "WindsRoomActor", 0, AssetDirectory.VitricTile + "WindsRoomOrnamentLeft", true) { }
	}

	class WindsRoomActorDummy : Dummy
	{
		public WindsRoomActorDummy() : base(TileType<WindsRoomActor>(), 16, 16) { }

		public override void SafeSetDefaults()
		{
			Projectile.hide = true;
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
			behindNPCsAndTiles.Add(index);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D backdrop = Request<Texture2D>(AssetDirectory.VitricTile + "WindsRoomBackground").Value;
			SpriteBatch spriteBatch = Main.spriteBatch;
			Vector2 pos = Projectile.Center + new Vector2(-backdrop.Width / 2, -backdrop.Height + 8) - Main.screenPosition;

			var bgTarget = backdrop.Size().ToRectangle();
			bgTarget.Offset(pos.ToPoint());
			bgTarget.Width -= 300;
			bgTarget.X += 150;
			bgTarget.Height -= 100;
			bgTarget.Y += 100;
			TempleTileUtils.DrawBackground(spriteBatch, bgTarget);

			return true;
		}

		public override void PostDraw(Color lightColor)
		{
			SpriteBatch spriteBatch = Main.spriteBatch;

			Texture2D backdrop = Request<Texture2D>(AssetDirectory.VitricTile + "WindsRoomBackground").Value;
			Texture2D backdropGlow = Request<Texture2D>(AssetDirectory.VitricTile + "WindsRoomBackgroundGlow").Value;
			Vector2 pos = Projectile.Center + new Vector2(-backdrop.Width / 2, -backdrop.Height + 8) - Main.screenPosition;

			spriteBatch.End();
			spriteBatch.Begin(); //this reset is neccisary for some reason?

			LightingBufferRenderer.DrawWithLighting(pos, backdrop);

			spriteBatch.End();
			spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

			spriteBatch.Draw(backdropGlow, pos, Color.White);

			Lighting.AddLight(Projectile.Center + new Vector2(0, -400), new Vector3(1, 0.8f, 0.5f));
			Lighting.AddLight(Projectile.Center + new Vector2(-200, -200), new Vector3(1, 0.8f, 0.5f));
			Lighting.AddLight(Projectile.Center + new Vector2(200, -200), new Vector3(1, 0.8f, 0.5f));

			Lighting.AddLight(Projectile.Center + new Vector2(0, -32), new Vector3(1, 0.8f, 0.5f));

			Color lighting = Lighting.GetColor((int)Projectile.Center.X / 16, (int)Projectile.Center.Y / 16 - 6);

			Texture2D left = Request<Texture2D>(AssetDirectory.VitricTile + "WindsRoomOrnamentLeft").Value;
			Texture2D leftGlow = Request<Texture2D>(AssetDirectory.VitricTile + "WindsRoomOrnamentLeftGlow").Value;
			Vector2 posLeft = Projectile.Center + new Vector2(-100 + (float)System.Math.Cos(Main.GameUpdateCount / 45f) * 2, -140 + (float)System.Math.Sin(Main.GameUpdateCount / 45f) * 6) - Main.screenPosition;

			spriteBatch.Draw(left, posLeft, null, lighting, (float)System.Math.Cos(Main.GameUpdateCount / 30f) * 0.05f, Vector2.Zero, 1, 0, 0);
			spriteBatch.Draw(leftGlow, posLeft, null, Color.White, (float)System.Math.Cos(Main.GameUpdateCount / 30f) * 0.05f, Vector2.Zero, 1, 0, 0);

			Texture2D right = Request<Texture2D>(AssetDirectory.VitricTile + "WindsRoomOrnamentRight").Value;
			Texture2D rightGlow = Request<Texture2D>(AssetDirectory.VitricTile + "WindsRoomOrnamentRightGlow").Value;
			Vector2 posRight = Projectile.Center + new Vector2(0 - (float)System.Math.Cos(Main.GameUpdateCount / 30f + 5) * 2, -220 + (float)System.Math.Sin(Main.GameUpdateCount / 30f + 5) * 8) - Main.screenPosition;

			spriteBatch.Draw(right, posRight, null, lighting, (float)System.Math.Cos(Main.GameUpdateCount / 30f + 5) * 0.05f, Vector2.Zero, 1, 0, 0);
			spriteBatch.Draw(rightGlow, posRight, null, Color.White, (float)System.Math.Cos(Main.GameUpdateCount / 30f + 5) * 0.05f, Vector2.Zero, 1, 0, 0);
		}
	}
}