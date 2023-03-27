using System;
using Terraria;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.SquidBoss
{
	class ArenaBlocker : ModNPC
	{
		public ref float Timer => ref NPC.ai[0];
		public ref float State => ref NPC.ai[1];

		public override string Texture => AssetDirectory.Invisible;

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			return false;
		}

		public override bool CanHitPlayer(Player target, ref int cooldownSlot)
		{
			return State == 0;
		}

		public override bool CheckActive()
		{
			return !NPC.AnyNPCs(NPCType<SquidBoss>());
		}

		public override void SetDefaults()
		{
			NPC.width = 1600;
			NPC.height = 128;
			NPC.damage = 1;
			NPC.lifeMax = 10;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.dontTakeDamage = true;
		}

		public override void AI()
		{
			if (State == 1 && Timer > 0)
				Timer -= 4;

			NPC.friendly = false;

			for (int k = 0; k < Main.maxProjectiles; k++)
			{
				Projectile proj = Main.projectile[k];
				if (proj.active && proj.Hitbox.Intersects(NPC.Hitbox) && proj.aiStyle == 7) //Hooks
					Main.projectile[k].active = false;
			}
		}

		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			if (Timer > 150)
			{
				Texture2D top = Request<Texture2D>(AssetDirectory.SquidBoss + "TentacleTop").Value;
				Texture2D glow = Request<Texture2D>(AssetDirectory.SquidBoss + "TentacleGlow").Value;
				Texture2D body = Request<Texture2D>(AssetDirectory.SquidBoss + "TentacleBody").Value;

				for (int k = 0; k < Timer - top.Height; k += body.Height + 2)
				{
					Vector2 pos2 = NPC.position + Vector2.UnitY * 96 + new Vector2(k, 4 + (float)Math.Sin(StarlightWorld.visualTimer + k / 25f) * 10);
					spriteBatch.Draw(body, pos2 - screenPos, body.Frame(), Lighting.GetColor((int)pos2.X / 16, (int)pos2.Y / 16), 1.57f, body.Size() / 2, 1, 0, 0);

					Vector2 pos3 = NPC.position + Vector2.UnitY * 96 + new Vector2(NPC.width - k, 4 + (float)Math.Sin(StarlightWorld.visualTimer + k / 25f) * 10);
					spriteBatch.Draw(body, pos3 - screenPos, body.Frame(), Lighting.GetColor((int)pos3.X / 16, (int)pos3.Y / 16), 1.57f, body.Size() / 2, 1, 0, 0);
				}

				float sin = (float)Math.Sin(StarlightWorld.visualTimer + 3.6f) * 10;
				var color = new Color(255, 40, 40);

				Vector2 pos = NPC.position + Vector2.UnitY * 96 + new Vector2(NPC.ai[0] - top.Height + 36, 32 - top.Width / 2 + sin);
				spriteBatch.Draw(top, pos - screenPos, top.Frame(), Lighting.GetColor((int)pos.X / 16, (int)pos.Y / 16), 1.57f, top.Size() / 2, 1, 0, 0);
				spriteBatch.Draw(glow, pos - screenPos, top.Frame(), color, 1.57f, top.Size() / 2, 1, 0, 0);
				Lighting.AddLight(pos, color.ToVector3() * 0.4f);

				Vector2 pos4 = NPC.position + Vector2.UnitY * 96 + new Vector2(1600 - NPC.ai[0] + top.Height - 36, 32 - top.Width / 2 + sin);
				spriteBatch.Draw(top, pos4 - screenPos, top.Frame(), Lighting.GetColor((int)pos4.X / 16, (int)pos4.Y / 16), 4.73f, top.Size() / 2, 1, 0, 0);
				spriteBatch.Draw(glow, pos4 - screenPos, top.Frame(), color, 4.73f, top.Size() / 2, 1, 0, 0);
				Lighting.AddLight(pos4, color.ToVector3() * 0.4f);
			}
		}

		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
		{
			target.immune = true;
			target.immuneTime = 1;

			target.position.Y = NPC.position.Y + NPC.height;
			target.velocity.Y += 5;
			target.noKnockback = true;
		}
	}
}