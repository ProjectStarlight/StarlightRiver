using System;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace StarlightRiver.Content.Bosses.BrainRedux
{
	internal class WeakPoint : ModNPC
	{
		public override string Texture => AssetDirectory.Invisible;

		public override void SetStaticDefaults()
		{

		}

		public override void SetDefaults()
		{
			NPC.lifeMax = 4000;
			NPC.damage = 2;
			NPC.width = 46;
			NPC.height = 46;
			NPC.noTileCollide = true;
			NPC.noGravity = true;
			NPC.aiStyle = -1;
			NPC.knockBackResist = 0f;
			NPC.defense = 3;

			NPC.HitSound = SoundID.NPCDeath12.WithPitchOffset(-0.25f);
		}

		public override bool CheckActive()
		{
			return false;
		}

		public override bool PreKill()
		{
			return false;
		}

		public override bool CanHitPlayer(Player target, ref int cooldownSlot)
		{
			return false;
		}

		public float Heartbeat(float t)
		{
			float omega = 2 * MathF.PI;
			float alpha = 0.5f;
			float beta = 2 * MathF.PI;

			float pulse = MathF.Sin(omega * t);
			float decay = MathF.Exp(-alpha * (t % (2 * MathF.PI / omega)));
			float modulation = 1 + MathF.Cos(beta * t);

			return MathF.Pow(pulse, 2) * decay * modulation;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			var tex = Assets.Bosses.BrainRedux.Neurysm.Value;
			var glow = Assets.Keys.GlowAlpha.Value;

			float r = 0.85f + (float)Math.Sin(Main.GameUpdateCount * 0.01f * 6.28f) * 0.5f;
			float g = 0.85f + (float)Math.Sin(Main.GameUpdateCount * 0.01f * 6.28f + 2f) * 0.5f;
			float b = 0.85f + (float)Math.Sin(Main.GameUpdateCount * 0.01f * 6.28f + 4f) * 0.5f;
			var color = new Color(r, g, b);

			var glowColor = color;
			glowColor.A = 0;

			float t = Main.GameUpdateCount * 0.02f;
			float heartBeat = Heartbeat(t) * 0.25f;

			spriteBatch.Draw(glow, NPC.Center - Main.screenPosition, null, glowColor, 0, glow.Size() / 2f, 1 + heartBeat, 0, 0);
			spriteBatch.Draw(tex, NPC.Center - Main.screenPosition, null, color, 0, tex.Size() / 2f, 1 + heartBeat, 0, 0);

			return false;
		}

		public override void AI()
		{
			NPC.realLife = DeadBrain.TheBrain?.thinker?.whoAmI ?? NPC.realLife;

			float r = 0.85f + (float)Math.Sin(Main.GameUpdateCount * 0.01f * 6.28f) * 0.5f;
			float g = 0.85f + (float)Math.Sin(Main.GameUpdateCount * 0.01f * 6.28f + 2f) * 0.5f;
			float b = 0.85f + (float)Math.Sin(Main.GameUpdateCount * 0.01f * 6.28f + 4f) * 0.5f;
			var color = new Color(r, g, b);

			Lighting.AddLight(NPC.Center, color.ToVector3() * 0.5f);
		}
	}
}
