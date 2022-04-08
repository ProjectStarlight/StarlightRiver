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
	class PortraitPatch : IPostLoadable
	{
		private static ILHook drawHook;
		private static Type typeInfo;

		private static ParticleSystem ceirosSystem = new ParticleSystem("StarlightRiver/Assets/Keys/GlowSoft", n => 
		{
			n.Velocity.X = (float)Math.Sin(n.Timer / 10f);
			n.Velocity *= 0.975f;
			n.Position += n.Velocity;
			n.Scale *= 0.99f;

			n.Color = Color.Lerp(Color.Red, Color.Yellow, n.Timer / 100f) * (float)Math.Sin(n.Timer / 100f * 3.14f);

			n.Timer--;
		}
		, 1);

		public void PostLoad()
		{
			if (ModLoader.TryGetMod("BossChecklist", out var bcl))
			{
				typeInfo = bcl.Code.GetType("BossChecklist.UIElements.BossLogUIElements").GetNestedType("BossLogPanel", BindingFlags.NonPublic);
				MethodInfo hooked = typeInfo.GetMethod("Draw", BindingFlags.Public | BindingFlags.Instance);

				drawHook = new ILHook(hooked, PatchDraw);
			}
		}

		public static void PatchDraw(ILContext il)
		{
			var c = new ILCursor(il);
			c.TryGotoNext(n => n.MatchStloc(107)); //Color3
			c.Index++;
			c.Emit(OpCodes.Ldloc, 92);
			c.Emit(OpCodes.Ldarg_0);
			c.EmitDelegate<Action<Texture2D, UIElement>>(newDraw);
		}

		public static void newDraw(Texture2D drawnTexture, UIElement self)
		{
			var spriteBatch = Main.spriteBatch;

			if (drawnTexture == ModContent.Request<Texture2D>("StarlightRiver/Assets/BossChecklist/VitricBoss").Value) //todo: More general to avoid this becoming a weird godclass
			{
				if (Main.rand.Next(2) == 0)
					ceirosSystem.AddParticle(new Particle(self.GetDimensions().Center() + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(100), Vector2.UnitY * -3, 0, Main.rand.NextFloat(0.3f), new Color(200, 200, 0), 100, Vector2.Zero));

				float sin = 0.6f + (float)Math.Sin(Main.GameUpdateCount / 100f) * 0.2f;

				var tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/BossChecklist/VitricBossGlow").Value;
				var tex2 = ModContent.Request<Texture2D>("StarlightRiver/Assets/Keys/Glow").Value;
				spriteBatch.Draw(tex2, self.GetDimensions().ToRectangle(), null, Color.Black * 0.6f, 0, Vector2.UnitY * 2, 0, 0);

				spriteBatch.End();
				spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.UIScaleMatrix);

				spriteBatch.Draw(tex, self.GetDimensions().Center(), null, Color.White * sin, 0, tex.Size() / 2, 1, 0, 0);
				ceirosSystem.DrawParticles(spriteBatch);

				spriteBatch.End();
				spriteBatch.Begin(default, default, default, default, default, default, Main.UIScaleMatrix);

				ceirosSystem.SetTexture(ModContent.Request<Texture2D>("StarlightRiver/Assets/Keys/GlowSoft").Value);
			}
		}

		public void PostLoadUnload() 
		{
			drawHook.Dispose();
		}
	}
}
