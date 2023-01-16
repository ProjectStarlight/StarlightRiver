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
	class CeirosPortrait
	{
		private static ParticleSystem ceirosSystem = new ParticleSystem("StarlightRiver/Assets/Keys/GlowSoft", n =>
		{
			n.Velocity.X = (float)Math.Sin(n.Timer / 10f);
			n.Velocity *= 0.975f;
			n.Position += n.Velocity;
			n.Scale *= 0.99f;

			n.Color = Color.Lerp(Color.Red, Color.Yellow, n.Timer / 100f) * (float)Math.Sin(n.Timer / 100f * 3.14f);

			n.Timer--;
		});

		public static void DrawCeirosPortrait(SpriteBatch spriteBatch, Rectangle rect, Color color)
		{
			if (Main.rand.NextBool(2))
				ceirosSystem.AddParticle(new Particle(rect.Center() + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(100), Vector2.UnitY * -3, 0, Main.rand.NextFloat(0.3f), new Color(200, 200, 0), 100, Vector2.Zero));

			float sin = 0.6f + (float)Math.Sin(Main.GameUpdateCount / 100f) * 0.2f;

			var tex0 = ModContent.Request<Texture2D>("StarlightRiver/Assets/BossChecklist/VitricBoss").Value;
			var tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/BossChecklist/VitricBossGlow").Value;
			var tex2 = ModContent.Request<Texture2D>("StarlightRiver/Assets/Keys/Glow").Value;
			spriteBatch.Draw(tex2, rect, null, Color.Black * 0.6f, 0, Vector2.UnitY * 2, 0, 0);

			spriteBatch.End();
			spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.UIScaleMatrix);

			spriteBatch.Draw(tex, rect.Center(), null, Color.White * sin, 0, tex.Size() / 2, 1, 0, 0);
			ceirosSystem.DrawParticles(spriteBatch);

			spriteBatch.End();
			spriteBatch.Begin(default, default, default, default, default, default, Main.UIScaleMatrix);

			spriteBatch.Draw(tex0, rect.Center(), null, color, 0, tex0.Size() / 2, 1, 0, 0);

			ceirosSystem.SetTexture(ModContent.Request<Texture2D>("StarlightRiver/Assets/Keys/GlowSoft").Value);
		}
	}
}
