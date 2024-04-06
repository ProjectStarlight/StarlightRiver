using StarlightRiver.Core.Systems.BarrierSystem;
using System;

namespace StarlightRiver.Content.Bosses.BrainRedux
{
	internal class Neurysm : ModNPC
	{
		public float opacity;
		public float tellDirection;
		public float tellLen;

		public ref float Timer => ref NPC.ai[0];
		public ref float State => ref NPC.ai[1];
		public ref float Dead => ref NPC.ai[2];
		public ref float TellTime => ref NPC.ai[3];

		public override string Texture => AssetDirectory.BrainRedux + Name;

		public override void SetDefaults()
		{
			NPC.lifeMax = 200;
			NPC.damage = 25;
			NPC.width = 34;
			NPC.height = 34;
			NPC.noTileCollide = true;
			NPC.noGravity = true;
			NPC.aiStyle = -1;
			NPC.knockBackResist = 0f;
			NPC.defense = 3;
		}

		public override bool CheckActive()
		{
			return false;
		}

		public override bool PreKill()
		{
			Dead = 1;
			NPC.life = 1;
			NPC.dontTakeDamage = true;
			NPC.immortal = true;
			return false;
		}

		public override bool CanHitPlayer(Player target, ref int cooldownSlot)
		{
			return opacity > 0.5f;
		}

		public override bool? CanBeHitByItem(Player player, Item item)
		{
			return opacity > 0.5f ? null : false;
		}

		public override bool? CanBeHitByProjectile(Projectile projectile)
		{
			return opacity > 0.5f ? null : false;
		}

		public override void AI()
		{
			Timer++;

			NPC.realLife = NPC.crimsonBoss;
			NPC.life = NPC.lifeMax;

			if (TellTime > 0)
				TellTime--;

			if (State == 1)
				opacity = 1 - Timer / 30f;
			else if (State == 2)
				opacity = Timer / 30f;

			Lighting.AddLight(NPC.Center, new Vector3(0.5f, 0.4f, 0.2f) * 0.5f);
		}

		public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
		{
			return opacity > 0;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value; 

			if (TellTime > 0)
			{
				Texture2D tell = ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrailNoEnd").Value;
				Rectangle source = new Rectangle(0, 0, tell.Width, tell.Height);
				Rectangle target = new Rectangle((int)(NPC.Center.X - Main.screenPosition.X), (int)(NPC.Center.Y - Main.screenPosition.Y), (int)tellLen, 24);
				Vector2 origin = new Vector2(0, 12);
				Color color = Color.Red * (float)Math.Sin(TellTime / 30f * 3.14f) * 0.5f;
				color.A = 0;

				spriteBatch.Draw(tell, target, source, color, tellDirection + 3.14f, origin, 0, 0);
			}

			if (State == 0)
				spriteBatch.Draw(tex, NPC.Center - Main.screenPosition, null, drawColor, NPC.rotation, tex.Size() / 2f, 1, 0, 0);

			if (State == 1)
			{
				for (int k = 0; k < 6; k++)
				{
					float rot = k / 6f * 6.28f + Timer / 30f * 3.14f;
					Vector2 offset = Vector2.UnitX.RotatedBy(rot) * Timer / 30f * 32;
					spriteBatch.Draw(tex, NPC.Center + offset - Main.screenPosition, null, drawColor * opacity * 0.2f, NPC.rotation, tex.Size() / 2f, 1, 0, 0);
				}
			}

			if (State == 2)
			{
				for (int k = 0; k < 6; k++)
				{
					float rot = k / 6f * 6.28f + (1 - Timer / 30f) * 3.14f;
					Vector2 offset = Vector2.UnitX.RotatedBy(rot) * (1 - Timer / 30f) * 32;
					spriteBatch.Draw(tex, NPC.Center + offset - Main.screenPosition, null, drawColor * opacity * 0.2f, NPC.rotation, tex.Size() / 2f, 1, 0, 0);
				}
			}

			return false;
		}
	}
}
