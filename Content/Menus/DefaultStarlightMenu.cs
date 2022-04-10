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

namespace StarlightRiver.Content.Menus
{
	internal class DefaultStarlightMenu : ModMenu
	{
		public static ParticleSystem sparkles;
		public static int Timer;

		public override int Music => MusicLoader.GetMusicSlot(Mod, "Sounds/Music/AluminumPassive");

		public override void SetStaticDefaults()
		{
			sparkles = new ParticleSystem(AssetDirectory.Dust + "Aurora", updateSparkles);
		}

		private void updateSparkles(Particle particle)
		{
			particle.Alpha = particle.Timer / 600f;
			particle.Rotation += particle.StoredPosition.X;
			particle.Position += particle.Velocity;
			particle.Timer--;
		}

		public override bool PreDrawLogo(SpriteBatch spriteBatch, ref Vector2 logoDrawCenter, ref float logoRotation, ref float logoScale, ref Color drawColor)
		{
			Timer++;

			Main.dayTime = false;

			if (Main.rand.Next(5) == 0)
			{
				var pos = new Vector2(Main.rand.Next(Main.screenWidth), Main.screenHeight + 20);
				var vel = new Vector2(0, -Main.rand.NextFloat(0.5f, 1.5f));
				var color = new Color(Main.rand.Next(100, 180), Main.rand.Next(180, 255), 255);
				sparkles.AddParticle(new Particle(pos, vel, 0, Main.rand.NextFloat(0.1f, 0.2f), color, 600, new Vector2(Main.rand.NextFloat(-0.1f, 0.1f), 0), new Rectangle(0, 0, 100, 100)));
			}

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.UIScaleMatrix);

			sparkles.DrawParticles(Main.spriteBatch);

			var tex = ModContent.Request<Texture2D>(AssetDirectory.MoonstoneTile + "GlowSolo", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
			var myRand = new Random(1283125412);

			for(int k = 0; k < Main.screenWidth; k += tex.Width * 2)
			{
				float sin = (float)Math.Sin(Timer * 0.01f + myRand.Next(120)) * (float)Math.Sin(Timer * 0.0152f + myRand.Next(120));
				float sin2 = (float)Math.Sin(Timer * 0.01f + myRand.Next(120)) * (float)Math.Sin(Timer * 0.0152f + myRand.Next(120));
				var color = new Color(100 + (int)(sin2 * 50), 200 + (int)(sin2 * 50), 255) * (0.75f);
				Main.spriteBatch.Draw(tex, new Vector2(k + myRand.Next(-10, 10), Main.screenHeight + 30), null, color, 0, new Vector2(tex.Width / 2, tex.Height), 3.0f + sin * 1.0f, 0, 0);
			}

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, default, default, default, default, default, Main.UIScaleMatrix);

			return true;
		}
	}
}
