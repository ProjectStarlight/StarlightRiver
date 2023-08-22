﻿using System;
using System.IO;
using Terraria.DataStructures;

namespace StarlightRiver.Content.Bosses.SquidBoss
{
	class TentacleTell : ModProjectile, IDrawAdditive
	{
		public static Vector2 endPointToAssign;

		public Vector2 endPoint;

		public override string Texture => AssetDirectory.SquidBoss + "TentacleTellTop";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Tentacle Warning");
		}

		public override void SetDefaults()
		{
			Projectile.width = 1;
			Projectile.height = 1;
			Projectile.aiStyle = -1;
			Projectile.timeLeft = 60;
			Projectile.hostile = false;
			Projectile.friendly = false;
			Projectile.damage = 0;
		}

		public override void OnSpawn(IEntitySource source)
		{
			endPoint = endPointToAssign;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			return false;
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			Texture2D top = ModContent.Request<Texture2D>(Texture).Value;
			Texture2D body = ModContent.Request<Texture2D>(AssetDirectory.SquidBoss + "TentacleTellBody").Value;
			Texture2D glow = ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value;
			Texture2D flat = Terraria.GameContent.TextureAssets.MagicPixel.Value;

			float timer = 60 - Projectile.timeLeft;

			Vector2 basePos = Projectile.Center - Main.screenPosition;
			float dist = Vector2.Distance(Projectile.Center, endPoint);
			float rot = (Projectile.Center - endPoint).ToRotation() + (float)Math.PI / 2f;
			var origin = new Vector2(glow.Width / 2, 0);
			var underGlowTarget = new Rectangle((int)basePos.X, (int)basePos.Y, 100, (int)dist);
			var flashGlowTarget = new Rectangle((int)basePos.X, (int)basePos.Y, (int)(timer / 20f * 200), (int)dist);

			var leftTarget = new Rectangle((int)(basePos.X - 50 * Math.Cos(rot)), (int)(basePos.Y - 50 * Math.Sin(rot)), 4, (int)dist * 2);
			var rightTarget = new Rectangle((int)(basePos.X + 50 * Math.Cos(rot)), (int)(basePos.Y + 50 * Math.Sin(rot)), 4, (int)dist * 2);
			var thinSource = new Rectangle(50, 0, glow.Width - 100, glow.Height);

			spriteBatch.Draw(flat, underGlowTarget, null, new Color(255, 40, 40) * (float)Math.Sin(timer / 60f * 3.14f) * 0.7f, rot, origin, 0, 0);
			spriteBatch.Draw(glow, leftTarget, thinSource, new Color(255, 120, 120) * (float)Math.Sin(timer / 60f * 3.14f), rot, glow.Size() / 4, 0, 0);
			spriteBatch.Draw(glow, rightTarget, thinSource, new Color(255, 120, 120) * (float)Math.Sin(timer / 60f * 3.14f), rot, glow.Size() / 4, 0, 0);

			spriteBatch.Draw(glow, underGlowTarget, null, new Color(255, 80, 80) * (float)Math.Sin(timer / 60f * 3.14f), rot, origin, 0, 0);
			spriteBatch.Draw(glow, flashGlowTarget, null, new Color(255, 120, 120) * (1 - timer / 20f), rot, origin, 0, 0);

			spriteBatch.Draw(top, Projectile.Center - Main.screenPosition, null, new Color(255, 80, 80) * (float)Math.Sin((timer - 30) / 30f * 3.14f), rot, top.Size() / 2, 1, 0, 0);
			spriteBatch.Draw(top, Projectile.Center - Main.screenPosition, null, new Color(255, 120, 120) * (1 - timer / 20f), rot, top.Size() / 2, timer / 10f, 0, 0);

			for (int k = 64; k < dist; k += 32)
			{
				Vector2 pos = Vector2.Lerp(Projectile.Center, endPoint, k / dist) - Main.screenPosition;
				float scale = Math.Max(0.5f, 1 - k * 0.0005f);

				spriteBatch.Draw(body, pos, null, new Color(255, 80, 80) * (float)Math.Sin(Math.Max(0, timer - (dist - k) / dist * 30) / 30f * 3.14f), rot, body.Size() / 2, scale, 0, 0);
				spriteBatch.Draw(body, pos, null, new Color(255, 120, 120) * (1 - timer / 20f), rot, body.Size() / 2, timer / 10f, 0, 0);
			}
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.WriteVector2(endPoint);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			endPoint = reader.ReadVector2();
		}
	}
}