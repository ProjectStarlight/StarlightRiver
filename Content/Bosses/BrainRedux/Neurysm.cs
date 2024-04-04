using StarlightRiver.Core.Systems.BarrierSystem;

namespace StarlightRiver.Content.Bosses.BrainRedux
{
	internal class Neurysm : ModNPC
	{
		public float opacity;

		public ref float Timer => ref NPC.ai[0];
		public ref float State => ref NPC.ai[1];
		public ref float Dead => ref NPC.ai[2];

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
			NPC.defense = 5;

			NPC.GetGlobalNPC<BarrierNPC>().maxBarrier = 100;
			NPC.GetGlobalNPC<BarrierNPC>().barrier = 100;
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

			if (State == 1)
			{
				opacity = (1 - Timer / 30f);
			}
			else if (State == 2)
			{
				opacity = (Timer / 30f);
			}

			Lighting.AddLight(NPC.Center, new Vector3(0.5f, 0.4f, 0.2f));
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;

			if (State == 0)
				spriteBatch.Draw(tex, NPC.Center - Main.screenPosition, null, Color.White, NPC.rotation, tex.Size() / 2f, 1, 0, 0);

			if (State == 1)
			{
				for (int k = 0; k < 6; k++)
				{
					float rot = k / 6f * 6.28f + Timer / 30f * 3.14f;
					Vector2 offset = Vector2.UnitX.RotatedBy(rot) * Timer / 30f * 32;
					spriteBatch.Draw(tex, NPC.Center + offset - Main.screenPosition, null, Color.White * opacity * 0.2f, NPC.rotation, tex.Size() / 2f, 1, 0, 0);
				}
			}

			if (State == 2)
			{
				for (int k = 0; k < 6; k++)
				{
					float rot = k / 6f * 6.28f + (1 - Timer / 30f) * 3.14f;
					Vector2 offset = Vector2.UnitX.RotatedBy(rot) * (1 - Timer / 30f) * 32;
					spriteBatch.Draw(tex, NPC.Center + offset - Main.screenPosition, null, Color.White * opacity * 0.2f, NPC.rotation, tex.Size() / 2f, 1, 0, 0);
				}
			}

			return false;
		}
	}
}
