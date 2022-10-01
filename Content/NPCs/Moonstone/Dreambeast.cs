using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Core;
using StarlightRiver.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.NPCs.Moonstone
{
	internal class Dreambeast : ModNPC
	{
		public VerletChain[] chains = new VerletChain[5];

		public Vector2 homePos;
		public int flashTime;

		private bool AppearVisible => Main.LocalPlayer.HasBuff(ModContent.BuffType<Buffs.Overcharge>());
		private Player Target => Main.player[NPC.target];

		public ref float Phase => ref NPC.ai[0];
		public ref float Timer => ref NPC.ai[1];
		public ref float AttackTimer => ref NPC.ai[2];
		public ref float RandomTime => ref NPC.ai[3];

		public override string Texture => AssetDirectory.Debug;

		public override void SetDefaults()
		{
			NPC.width = 64;
			NPC.height = 64;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.aiStyle = -1;
			NPC.lifeMax = 666666;
			NPC.damage = 66;
			NPC.dontTakeDamage = true;
			NPC.immortal = true;
		}

		public override void AI()
		{
			Timer++;
			AttackTimer++;

			if (!AppearVisible)
				flashTime = 0;

			if (homePos == default)
				homePos = NPC.Center;

			if (flashTime < 30 && Phase != 0)
				flashTime++;

			/*
			for (int k = 0; k < chains.Length; k++)
			{
				var chain = chains[k];

				if (chain is null)
					chain = new VerletChain(50, false, NPC.Center, 5, false);

				chain.UpdateChain(NPC.Center);
			}*/

			if (Phase == 0)
				PassiveBehavior();
			else if (Phase == 1)
				AttackRest();
			else if (Phase == 2)
				AttackCharge();
			else if (Phase == 3)
				AttackShoot();
		}

		/// <summary>
		/// picks a random valid target. Meaning a player within range of the beasts home base and that has the insanity debuff.
		/// </summary>
		private void PickTarget()
		{
			List<Player> possibleTargets = new List<Player>();

			foreach(var player in Main.player)
			{
				if (player.active && player.HasBuff<Overcharge>() && Vector2.Distance(player.Center, homePos) < 1000)
					possibleTargets.Add(player);
			}

			if (possibleTargets.Count <= 0)
			{
				NPC.target = -1;
				return;
			}

			NPC.target = possibleTargets[Main.rand.Next(possibleTargets.Count)].whoAmI;
		}

		/// <summary>
		/// What the NPC will be doing while its not actively attacking anyone.
		/// </summary>
		private void PassiveBehavior()
		{
			//logic for phase transition
			if (Main.player.Any(n => n.active && n.HasBuff<Overcharge>() && Vector2.Distance(n.Center, homePos) < 1000))
			{
				Phase = 1;
				NPC.Opacity = 1;
				AttackTimer = 0;
				return;
			}

			NPC.Center += Vector2.One.RotatedBy(Timer * 0.005f) * 0.25f;

			if (NPC.Opacity < 1)
				NPC.Opacity += 0.05f;

			if (AttackTimer > RandomTime)
			{
				NPC.Opacity = (20 - (AttackTimer - RandomTime)) / 20f;

				if (AttackTimer > RandomTime + 20)
				{
					AttackTimer = 0;
					RandomTime = Main.rand.Next(120, 240);

					NPC.Center = homePos + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(200, 400);
				}
			}
		}

		/// <summary>
		/// What the NPC does while it is attacking but isnt currently executing a specific attack. It chooses its attack from here.
		/// </summary>
		private void AttackRest()
		{
			NPC.velocity *= 0;

			if (AttackTimer > 30)
			{
				NPC.Opacity = (20 - (AttackTimer - 30)) / 20f;

				if (AttackTimer > 50)
				{
					AttackTimer = 0;
					PickTarget();

					if(NPC.target == -1)
					{
						Phase = 0;
						return;
					}

					NPC.Center = Target.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(200, 300);
					Phase = 2;
				}
			}
		}

		/// <summary>
		/// Charge at the targeted player
		/// </summary>
		private void AttackCharge()
		{
			if (NPC.Opacity < 1)
				NPC.Opacity += 0.05f;

			if (AttackTimer == 1)
				NPC.velocity = Vector2.Normalize(NPC.Center - Target.Center) * -40;

			NPC.velocity *= 0.95f;

			if (AttackTimer >= 60)
			{
				//NPC.Opacity = (20 - (AttackTimer - 60)) / 20f;

				if (AttackTimer > 80)
				{
					AttackTimer = 0;
					Phase = 1;
					return;
				}
			}
		}

		private void AttackShoot()
		{
			if (NPC.Opacity < 1)
				NPC.Opacity += 0.05f;

			Main.NewText("Shoot attack");

			AttackTimer = 0;
			Phase = 1;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			var tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Bosses/VitricBoss/VitricBossGodrayHead").Value;

			if (!AppearVisible)
			{
				var effect = Terraria.Graphics.Effects.Filters.Scene["MoonstoneBeastEffect"].GetShader().Shader;
				effect.Parameters["baseTexture"].SetValue(tex);
				effect.Parameters["distortTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/Noise/MiscNoise2").Value);
				effect.Parameters["size"].SetValue(tex.Size());
				effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.005f);
				effect.Parameters["opacity"].SetValue(NPC.Opacity);

				spriteBatch.End();
				spriteBatch.Begin(default, BlendState.Additive, default, default, default, effect, Main.GameViewMatrix.TransformationMatrix);
			}

			spriteBatch.Draw(tex, NPC.Center - Main.screenPosition, null, Color.White * NPC.Opacity, 0, tex.Size() / 2, 1, 0, 0);

			spriteBatch.End();
			spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);

			if (AppearVisible && flashTime > 0)
			{
				var flashTex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Keys/GlowAlpha").Value;
				var color = Color.White * (1 - flashTime / 30f);
				color.A = 0;

				spriteBatch.Draw(flashTex, NPC.Center - Main.screenPosition, null, color, 0, flashTex.Size() / 2, flashTime, 0, 0);
			}

			return false;
		}
	}
}
