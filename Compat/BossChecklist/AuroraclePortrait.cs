using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StarlightRiver.Core;
using MonoMod.Cil;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using StarlightRiver.Content.CustomHooks;
using System.Reflection;
using MonoMod.RuntimeDetour;
using Microsoft.Xna.Framework.Graphics;
using Terraria.UI;
using Mono.Cecil.Cil;

namespace StarlightRiver.Compat.BossChecklist
{
	class AuroraclePortrait
	{
		private static ParticleSystem auroracleSystem = new ParticleSystem("StarlightRiver/Assets/Dusts/Aurora", n =>
		{
			n.Velocity *= 0.995f;
			n.Position += n.Velocity;
			n.Scale *= 0.99f;
			n.Rotation += 0.05f;

			float r = 0.25f * (1f + (float)Math.Sin(n.Timer / 30f + n.Position.X * 0.05f * 0.2));
			float g = 0.35f * (1f + (float)Math.Cos(n.Timer / 30f + n.Position.X * 0.05f));
			float b = 0.45f;

			float a = 1;

			if (n.Timer > 270)
				a = 1 - (n.Timer - 270) / 30f;

			n.Color = Color.Lerp(new Color(r, g, b), Color.White, 0.25f) * (n.Timer / 300f) * a;

			n.Timer--;
		});

		public static void DrawAuroraclePortrait(SpriteBatch spriteBatch, Rectangle rect, Color color)
		{
			if (Main.rand.NextBool(5))
				auroracleSystem.AddParticle(new Particle(rect.Center() + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(100), Vector2.UnitY * -Main.rand.NextFloat(), Main.rand.NextFloat(6.28f), Main.rand.NextFloat(0.25f, 0.45f), new Color(200, 200, 0), 300, Vector2.Zero, new Rectangle(0, 0, 100, 100)));

			float sin = 0.8f + (float)Math.Sin(Main.GameUpdateCount / 100f) * 0.1f;

			var tex0 = ModContent.Request<Texture2D>("StarlightRiver/Assets/BossChecklist/SquidBoss").Value;
			var tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/BossChecklist/SquidBossGlow").Value;
			var tex2 = ModContent.Request<Texture2D>("StarlightRiver/Assets/Keys/Glow").Value;
			spriteBatch.Draw(tex2, rect, null, Color.Black * 0.6f, 0, Vector2.UnitY * 2, 0, 0);

			spriteBatch.End();
			spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.UIScaleMatrix);

			float r = 0.25f * (1f + (float)Math.Sin(Main.GameUpdateCount * 0.1f * 0.2));
			float g = 0.35f * (1f + (float)Math.Cos(Main.GameUpdateCount * 0.1f));
			float b = 0.45f;

			var glowColor = new Color(r, g, b) * sin;

			spriteBatch.Draw(tex, rect.Center(), null, glowColor, 0, tex.Size() / 2, 1, 0, 0);
			auroracleSystem.DrawParticles(spriteBatch);

			spriteBatch.End();
			spriteBatch.Begin(default, default, default, default, default, default, Main.UIScaleMatrix);

			spriteBatch.Draw(tex0, rect.Center(), null, color, 0, tex0.Size() / 2, 1, 0, 0);

			auroracleSystem.SetTexture(ModContent.Request<Texture2D>("StarlightRiver/Assets/Dusts/Aurora").Value);
		}
	}
}
