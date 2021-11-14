using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Misc
{
	class Guillotine : SmartAccessory
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public Guillotine() : base("Golden Guillotine", "Critical strikes gain power as your foes lose health\nExecutes normal enemies on low health") { }

		public override bool Autoload(ref string name)
		{
			StarlightNPC.ModifyHitByItemEvent += ModifyCrit;
			StarlightNPC.ModifyHitByProjectileEvent += ModifyCritProj;
			return base.Autoload(ref name);
		}

        public override void SafeSetDefaults()
		{
			item.rare = ItemRarityID.Orange;
		}

		private void ModifyCritProj(NPC npc, Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			if (Equipped(Main.player[projectile.owner]))
			{
				float multiplier = 2 + CritMultiPlayer.GetMultiplier(projectile);

				if (crit)
					damage += (int)((damage / multiplier) * (1.5f - (npc.life / npc.lifeMax) / 2));

				if (!npc.boss && (npc.life / npc.lifeMax) < 0.1f && (damage * multiplier * 1.5f) > npc.life)
					Execute(npc);
			}
		}

		private void ModifyCrit(NPC npc, Player player, Item item, ref int damage, ref float knockback, ref bool crit)
		{
			if (Equipped(player))
			{
				float multiplier = 2 + CritMultiPlayer.GetMultiplier(item);

				if (crit)
					damage += (int)((damage / multiplier) * (1.5f - (npc.life / npc.lifeMax) / 2));

				if (!npc.boss && (npc.life / npc.lifeMax) < 0.1f && (damage * multiplier * 1.5f) > npc.life)
					Execute(npc);
			}
		}

		private void Execute(NPC npc)
		{
			int flesh = 1;
			if (Helpers.Helper.IsFleshy(npc))
			{
				flesh = 0;
			}
			Projectile.NewProjectile(npc.Center, Vector2.Zero, ModContent.ProjectileType<GuillotineVFX>(), 0, 0, Main.myPlayer, npc.whoAmI, flesh);
			npc.StrikeNPCNoInteraction(9999, 1, 0, false, true);// kill npc locally prior to projectile so that multiple simultaneous hits don't cause a bunch of executions at once
		}
	}

	class GuillotineVFX : ModProjectile
	{
		public override string Texture => AssetDirectory.Invisible;

		public ref float hitNpcIndex => ref projectile.ai[0];

		// 0 for fleshy, anything else for not fleshy
		public ref float wasFleshy => ref projectile.ai[1];

		private bool alreadyHit = false;

		public override void SetDefaults()
		{
			projectile.width = 1;
			projectile.height = 1;
			projectile.friendly = false;
			projectile.hostile = false;
			projectile.timeLeft = 45;
		}

        public override void AI()
        {
            if (!alreadyHit)
            {
				alreadyHit = true;

				CombatText.NewText(projectile.Hitbox, new Color(255, 230, 100), "Ouch!", true);

				NPC npc = Main.npc[(int)hitNpcIndex];
				npc.StrikeNPCNoInteraction(9999, 1, 0, false, true);
				NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npc.whoAmI);

				if (wasFleshy == 0)
				{
					Helpers.Helper.PlayPitched("Impacts/GoreHeavy", 1, 0, projectile.Center);

					for (int k = 0; k < 200; k++)
						Dust.NewDustPerfect(projectile.Center, DustID.Blood, Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(10), 0, default, 2);
				}
				else
				{
					Helpers.Helper.PlayPitched("ChainHit", 1, -0.5f, projectile.Center);
				}
			}
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			var tex = ModContent.GetTexture(AssetDirectory.MiscItem + "Guillotine");
			spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, Color.White * (projectile.timeLeft / 45f), 0, tex.Size() / 2f, (1 - (projectile.timeLeft / 45f)) * 3f, 0, 0);

			return false;
		}

	}
}
