using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

		private bool AppearVisible => Main.LocalPlayer.HasBuff(ModContent.BuffType<Buffs.Overcharge>());

		public ref float Phase => ref NPC.ai[0];
		public ref float Timer => ref NPC.ai[1];

		public override string Texture => AssetDirectory.Debug;

		public override void SetDefaults()
		{
			NPC.width = 64;
			NPC.height = 64;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.lifeMax = 666666;
			NPC.damage = 66;
			NPC.dontTakeDamage = true;
			NPC.immortal = true;
		}

		public override void AI()
		{
			Timer++;

			if (homePos == default)
				homePos = NPC.Center;

			/*
			for (int k = 0; k < chains.Length; k++)
			{
				var chain = chains[k];

				if (chain is null)
					chain = new VerletChain(50, false, NPC.Center, 5, false);

				chain.UpdateChain(NPC.Center);
			}*/

			//Temp
			NPC.Center = homePos + Vector2.One.RotatedBy(Timer * 0.005f) * 350;
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

				spriteBatch.End();
				spriteBatch.Begin(default, BlendState.Additive, default, default, default, effect, Main.GameViewMatrix.TransformationMatrix);
			}

			spriteBatch.Draw(tex, NPC.Center - Main.screenPosition, Color.White);

			spriteBatch.End();
			spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);

			return false;
		}
	}
}
