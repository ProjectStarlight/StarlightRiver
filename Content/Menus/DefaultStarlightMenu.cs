using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Backgrounds;
using StarlightRiver.Content.CustomHooks;

namespace StarlightRiver.Content.Menus
{
	internal class DefaultStarlightMenu : ModMenu
	{
		const int LifeTime = 120;
		const float SinTime = LifeTime / (float)Math.PI;
		const float ScaleMultiplier = 0.01f;

		public static ParticleSystem sparkles;
		public static ParticleSystem meteor;
		public static int Timer;

        public override string DisplayName => "Moonstone";
        public override int Music => MusicLoader.GetMusicSlot(Mod, "Sounds/Music/AluminumPassive");

		public override void SetStaticDefaults()
		{
			sparkles = new ParticleSystem(AssetDirectory.Dust + "MoonstoneShimmer", updateSparkles);
			meteor = new ParticleSystem(AssetDirectory.MiscTextures + "MoonstoneMeteor", updateMeteor);
		}

		private void updateSparkles(Particle particle)
		{
			particle.Alpha = particle.Timer / 120f;
			particle.Scale *= (-(float)Math.Sin(particle.Timer / (SinTime * 0.5f)) * ScaleMultiplier) + 1;
			particle.Position += particle.Velocity;
			//particle.Color *= (float)Math.Sin(particle.Timer / SinTime) * 1.5f;

			particle.Timer--;
		}

		private void updateMeteor(Particle particle)
		{
            particle.Position += particle.Velocity;
            particle.Frame = new Rectangle(0, 82 * ((particle.Timer / 8) % 4), 82, 82);
			particle.Alpha = particle.Timer / (Main.screenHeight / 4.15f) / particle.Scale;
			particle.Scale += 0.00085f * particle.Velocity.Length();

			particle.Timer--;
        }

		public override bool PreDrawLogo(SpriteBatch spriteBatch, ref Vector2 logoDrawCenter, ref float logoRotation, ref float logoScale, ref Color drawColor)
		{
			Timer++;

			Main.dayTime = false;
			//Main.bgStyle = 8;//no effect
			//Main.background = 1;//crashes if changed

			//Texture2D meteor = (Texture2D)ModContent.Request<Texture2D>(AssetDirectory.MiscTextures + "MoonstoneMeteor");
			//spriteBatch.Draw(meteor, new Vector2(200, 200), new Color(130, 130, 130, 0));

			if (Main.rand.NextBool(150))
			{
				float scale = Main.rand.NextFloat(0.4f, 1.6f);
				Vector2 meteorpos = new Vector2(Main.rand.Next(10, (int)(Main.screenWidth * 1.5f)), -10);
				var vel = new Vector2(-Main.rand.NextFloat(2.7f, 3.2f), Main.rand.NextFloat(2f, 2.3f));
				var color = new Color(130, 130, 130, 0);
				meteor.AddParticle(new Particle(meteorpos, vel * scale, 0, scale, color * scale, (int)((Main.screenHeight / 2.075f) / scale), Vector2.Zero, new Rectangle(0, 0, 82, 82)));
			}

			meteor.DrawParticles(Main.spriteBatch);

			Vector2 pos = new Vector2(Main.rand.Next(Main.screenWidth), Main.screenHeight + 10);//Timer % Main.screenWidth
			float div = (-(float)Math.Sin((pos.X / Main.screenWidth) * Math.PI) * 0.8f) + 1;
            div += (div == 0 ? 1 : 0);
			int chance = (int)(3.75f / div);

			//Utils.DrawBorderString(spriteBatch, "Chance:" + chance + " PosX:" + pos.X, new Vector2(50, 50), Color.Green);

			if (Main.rand.Next(chance) == 0)
			{
				//apl/col/scale //-Main.rand.NextFloat(0.05f, 0.18f)), 0, new Color(0.2f, 0.2f, 0.25f, 0f), Main.rand.NextFloat(0.25f, 0.5f)
				var vel = new Vector2(Main.rand.NextFloat(-0.02f, 0.02f), -Main.rand.NextFloat(0.2f, 0.35f));
				var color = new Color(Main.rand.NextFloat(0.18f, 0.20f), Main.rand.NextFloat(0.19f, 0.21f), Main.rand.NextFloat(0.22f, 0.29f), 0f);
				sparkles.AddParticle(new Particle(pos, vel, 0, Main.rand.NextFloat(0.85f, 1f), color, 240, new Vector2(Main.rand.NextFloat(-0.1f, 0.1f), 0), new Rectangle(0, 0, 17, 29)));
			}

			sparkles.DrawParticles(Main.spriteBatch);

			//var tex = ModContent.Request<Texture2D>(AssetDirectory.MoonstoneTile + "GlowSolo", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
			//var myRand = new Random(1283125412);

			//for(int k = 0; k < Main.screenWidth; k += tex.Width * 2)
			//{
			//	float sin = (float)Math.Sin(Timer * 0.01f + myRand.Next(120)) * (float)Math.Sin(Timer * 0.0152f + myRand.Next(120));
			//	float sin2 = (float)Math.Sin(Timer * 0.01f + myRand.Next(120)) * (float)Math.Sin(Timer * 0.0152f + myRand.Next(120));
			//	var color = new Color(100 + (int)(sin2 * 50), 200 + (int)(sin2 * 50), 255) * (0.75f);
			//	Main.spriteBatch.Draw(tex, new Vector2(k + myRand.Next(-10, 10), Main.screenHeight + 30), null, color, 0, new Vector2(tex.Width / 2, tex.Height), 3.0f + sin * 1.0f, 0, 0);
			//}

			float heightScale = ((float)Math.Sin((Timer + 2) * 0.025f) * 5) + 5;

			Texture2D midTex = ModContent.Request<Texture2D>(AssetDirectory.MoonstoneTile + "GlowMid").Value;
			Color overlayColor = new Color(0.12f, 0.135f, 0.23f, 0f) * (((float)Math.Sin(Timer * 0.02f) + 4) / 4);
			for (int k = 0; k < Main.screenWidth; k += midTex.Width)
            {
				Main.spriteBatch.Draw(midTex, new Vector2(k + 8, (Main.screenHeight + 8) + heightScale), null, overlayColor, 0, new Vector2(midTex.Width / 2, midTex.Height), 1f, 0, 0);
            }

			float heightScale2 = ((float)Math.Sin(Timer * 0.025f) * 3) + 3;

			Texture2D glowLines = ModContent.Request<Texture2D>(AssetDirectory.MoonstoneTile + "GlowLines").Value;
			for (float k = 0; k < Main.screenWidth + glowLines.Width; k += glowLines.Width > 1 ? glowLines.Width - 1.00f : 1)//during loading the texture has a width of one
			{
				Main.spriteBatch.Draw(glowLines, new Vector2((k + 8) + ((int)(Timer * 0.5f) % glowLines.Width) - glowLines.Width, Main.screenHeight + 8 + heightScale2), null, overlayColor * 0.45f, 0, new Vector2(glowLines.Width / 2, glowLines.Height), 1f, 0, 0);
			}

			return true;
		}

		public override void Unload()
		{
			sparkles = null;
			meteor = null;
		}
	}
}
