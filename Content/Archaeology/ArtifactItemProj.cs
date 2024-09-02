﻿using System;
using System.IO;
using Terraria.DataStructures;

namespace StarlightRiver.Content.Archaeology
{
	public class ArtifactItemProj : ModProjectile
	{
		public static Color glowColorToAssign;
		public static int itemTypeToAssign;
		public static Vector2 sizeToAssign;
		public static int sparkleTypeToAssign;

		public override string Texture => itemTexture;

		public string itemTexture = AssetDirectory.Invisible;
		public Color glowColor = Color.Gold;
		public int itemType = -1;
		public Vector2 size = Vector2.Zero;
		public int sparkleType = -1;

		public float fadeIn;
		public bool spawnedDust = false;

		public float Fade => EaseFunction.EaseCircularOut.Ease(fadeIn) * EaseFunction.EaseCircularOut.Ease((float)Math.Min(Projectile.timeLeft / 30f - 0.15f, 1));

		public float Rotation => 1000 * EaseFunction.EaseCircularIn.Ease(1 - Progress);

		public float Progress => Projectile.timeLeft / 93f;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Artifact");
		}

		public override void SetDefaults()
		{
			Projectile.width = 2;
			Projectile.height = 2;
			Projectile.timeLeft = 66;
			Projectile.friendly = false;
			Projectile.tileCollide = false;
		}

		public override void OnSpawn(IEntitySource source)
		{
			glowColor = glowColorToAssign;
			itemType = itemTypeToAssign;
			size = sizeToAssign;
			sparkleType = sparkleTypeToAssign;
		}

		public override void AI()
		{
			if (fadeIn < 1)
			{
				fadeIn += 0.03f;
				if (fadeIn < 0.75f && Main.rand.NextBool(20 - (int)(25 * fadeIn)))
				{
					Vector2 pos = Projectile.position + size * new Vector2(Main.rand.NextFloat(), Main.rand.NextFloat());
					Dust.NewDustPerfect(pos, sparkleType, Vector2.Zero);
				}
			}
			else
			{
				fadeIn = 1;
			}

			if (Projectile.timeLeft < 10)
			{
				Vector2 pos = Projectile.position + size * new Vector2(Main.rand.NextFloat(), Main.rand.NextFloat());
				Dust.NewDustPerfect(pos, sparkleType, Main.rand.NextVector2Circular(0.5f, 0.5f));
			}

			Projectile.velocity.Y *= 0.96f;

			Lighting.AddLight(Projectile.Center + size / 2, glowColor.ToVector3() * Fade);
		}

		public override bool PreDraw(ref Color lightColor)
		{

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, BlendState.Additive, SamplerState.PointClamp, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);

			Texture2D mainTex = ModContent.Request<Texture2D>(Texture).Value;
			Texture2D glowTex = Assets.Keys.Glow.Value;
			Texture2D beamTex = Assets.Keys.Shine.Value;

			Vector2 pos = Projectile.Center + mainTex.Size() / 2 - Main.screenPosition;
			Main.spriteBatch.Draw(glowTex, pos, null, glowColor * (0.9f + (float)Math.Sin(Main.GameUpdateCount / 25f) * 0.1f), 0f, glowTex.Size() / 2, mainTex.Size() / glowTex.Size() * 1.4f * Fade, SpriteEffects.None, 0f);

			float rot = Main.GameUpdateCount * 2 + Rotation;

			DrawBeam(beamTex, pos, 1 - GetProgress(0), rot / 125f, 0.16f * GetProgress(0));
			DrawBeam(beamTex, pos, 1 - GetProgress(34), rot / 175f + 2.2f, 0.18f * GetProgress(34));
			DrawBeam(beamTex, pos, 1 - GetProgress(70), rot / 160f + 5.4f, 0.19f * GetProgress(70));
			DrawBeam(beamTex, pos, 1 - GetProgress(15), rot / 150f + 3.14f, 0.16f * GetProgress(15));
			DrawBeam(beamTex, pos, 1 - GetProgress(98), rot / 180f + 4.0f, 0.17f * GetProgress(98));
			DrawBeam(beamTex, pos, 1 - GetProgress(41), rot / 200f, 0.16f * GetProgress(41));
			DrawBeam(beamTex, pos, 1 - GetProgress(26), rot / 105f + 2.2f, 0.18f * GetProgress(26));
			DrawBeam(beamTex, pos, 1 - GetProgress(94), rot / 110f + 5.4f, 0.18f * GetProgress(94));
			DrawBeam(beamTex, pos, 1 - GetProgress(32), rot / 190f + 3.14f, 0.16f * GetProgress(32));
			DrawBeam(beamTex, pos, 1 - GetProgress(108), rot / 132f + 4.0f, 0.18f * GetProgress(108));

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, default, Main.DefaultSamplerState, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);

			Main.spriteBatch.Draw(mainTex, pos, null, lightColor, 0f, mainTex.Size() / 2, Projectile.scale, SpriteEffects.None, 0f);
			return false;
		}

		private void DrawBeam(Texture2D tex, Vector2 pos, float colorMult, float rotationMult, float scaleMult)
		{
			Main.spriteBatch.Draw(tex, pos, null, glowColor * 2 * colorMult, rotationMult, new Vector2(tex.Width / 2, tex.Height), Fade * scaleMult, 0, 0);
		}

		private float GetProgress(float off)
		{
			return (Main.GameUpdateCount + off * 6) % 300 / 300f;
		}

		public override void Kill(int timeLeft)
		{
			var itemDrop = new Rectangle((int)Projectile.position.X + (int)(size.X / 2), (int)Projectile.position.Y + (int)(size.Y / 2), 1, 1);
			Item.NewItem(new EntitySource_Misc("Artifact"), itemDrop, itemType);
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.WriteRGB(glowColor);
			writer.Write(itemType);
			writer.WriteVector2(size);
			writer.Write(sparkleType);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			glowColor = reader.ReadRGB();
			itemType = reader.ReadInt32();
			size = reader.ReadVector2();
			sparkleType = reader.ReadInt32();
		}
	}
}