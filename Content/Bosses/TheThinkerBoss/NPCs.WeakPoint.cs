using System;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace StarlightRiver.Content.Bosses.TheThinkerBoss
{
	internal class WeakPoint : ModNPC
	{
		public NPC thinker;

		public override string Texture => AssetDirectory.Invisible;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("The Linker");
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
			NPC.defense = 5;
			NPC.hide = true;

			NPC.HitSound = SoundID.NPCDeath12.WithPitchOffset(-0.25f);
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			database.Entries.Remove(bestiaryEntry);
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

		public override void AI()
		{
			NPC.realLife = thinker?.whoAmI ?? NPC.realLife;

			float r = 0.7f + (float)Math.Sin(Main.GameUpdateCount * 0.01f * 6.28f) * 0.03f;
			float g = 0.3f + (float)Math.Sin(Main.GameUpdateCount * 0.01f * 6.28f + 2f) * 0.05f;
			float b = 0.3f + (float)Math.Sin(Main.GameUpdateCount * 0.01f * 6.28f + 4f) * 0.03f;
			var color = new Color(r, g, b);

			if (thinker?.life <= thinker?.lifeMax / 2f)
			{
				float t = Main.GameUpdateCount * 0.02f;
				float heartBeat = Heartbeat(t * 1.5f) * 0.35f;
				color = new Color(255, 60, 75) * (heartBeat + 0.65f);
			}

			Lighting.AddLight(NPC.Center, color.ToVector3() * 0.5f);
		}

		public override void DrawBehind(int index)
		{
			Main.instance.DrawCacheNPCProjectiles.Add(index);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			if (thinker?.ModNPC is TheThinker think && think.brain.ModNPC is DeadBrain brain && brain.Phase == DeadBrain.Phases.SpawnAnim && brain.Timer <= 100)
				return false;

			Texture2D tex = Assets.Bosses.TheThinkerBoss.Weakpoint.Value;
			Texture2D glow = Assets.Masks.GlowAlpha.Value;
			Texture2D star = Assets.StarTexture.Value;

			float r = 0.7f + (float)Math.Sin(Main.GameUpdateCount * 0.01f * 6.28f) * 0.03f;
			float g = 0.3f + (float)Math.Sin(Main.GameUpdateCount * 0.01f * 6.28f + 2f) * 0.05f;
			float b = 0.3f + (float)Math.Sin(Main.GameUpdateCount * 0.01f * 6.28f + 4f) * 0.03f;
			var color = new Color(r, g, b);

			Color glowColor = color;
			glowColor.A = 0;

			float t = Main.GameUpdateCount * 0.02f;
			float heartBeat = Heartbeat(t) * 0.25f;

			if (thinker?.life <= thinker?.lifeMax / 2f)
			{
				heartBeat = Heartbeat(t * 1.5f) * 0.35f;
				color = new Color(255, 60, 75) * (heartBeat + 0.65f);
				glowColor = new Color(255, 60, 75) * (heartBeat + 0.65f);
				glowColor.A = 0;
			}

			spriteBatch.Draw(glow, NPC.Center - Main.screenPosition, null, glowColor, 0, glow.Size() / 2f, 1 + heartBeat, 0, 0);
			spriteBatch.Draw(glow, NPC.Center - Main.screenPosition, null, glowColor, 0, glow.Size() / 2f, 0.8f + heartBeat, 0, 0);
			spriteBatch.Draw(star, NPC.Center - Main.screenPosition, null, glowColor * 1.5f, Main.GameUpdateCount * 0.1f, star.Size() / 2f, 0.22f + heartBeat, 0, 0);
			spriteBatch.Draw(star, NPC.Center - Main.screenPosition, null, glowColor * 1.5f, Main.GameUpdateCount * -0.15f, star.Size() / 2f, 0.18f + heartBeat, 0, 0);
			spriteBatch.Draw(tex, NPC.Center - Main.screenPosition, null, color, 0, tex.Size() / 2f, 1 + heartBeat, 0, 0);

			return false;
		}

		public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
		{
			if (thinker is null)
				return false;

			position.Y += 8;

			SpriteBatch spriteBatch = Main.spriteBatch;

			Texture2D tex = Assets.GUI.SmallBar0.Value;
			Texture2D texOver = Assets.GUI.SmallBar1.Value;
			float progress = (thinker.life - thinker.lifeMax / 2f) / (thinker.lifeMax / 2f);

			int w = (int)(progress * texOver.Width - 4);

			if (w < 2 && thinker.life > thinker.lifeMax / 2f)
				w = 2;

			var target = new Rectangle((int)(position.X - Main.screenPosition.X) + 2, (int)(position.Y - Main.screenPosition.Y) - 2, w, texOver.Height);
			var source = new Rectangle(2, 0, w, texOver.Height);

			Color color = progress > 0.5f ?
				Color.Lerp(Color.OrangeRed, Color.Yellow, progress * 2 - 1) :
				Color.Lerp(Color.Red, Color.OrangeRed, progress * 2);

			spriteBatch.Draw(tex, position - Main.screenPosition, null, color * 0.8f, 0, tex.Size() / 2, 1, 0, 0);
			spriteBatch.Draw(texOver, target, source, color * 0.8f, 0, tex.Size() / 2, 0, 0);

			return false;
		}
	}
}