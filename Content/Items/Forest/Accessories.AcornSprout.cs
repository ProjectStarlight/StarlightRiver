using Microsoft.Xna.Framework;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Forest
{
	class AcornSprout : SmartAccessory
    {
        public override string Texture => AssetDirectory.ForestItem + Name;

        public AcornSprout() : base("Acorn Sprout", "Killing summon tagged enemies summons acorns to fall on nearby enemies") { }

        public override void SafeSetDefaults() => item.rare = ItemRarityID.Blue;

        public override bool Autoload(ref string name)
        {
            StarlightPlayer.ModifyHitNPCWithProjEvent += SpawnAcorn;
            return base.Autoload(ref name);
        }

		private void SpawnAcorn(Player player, Projectile proj, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
            if (target.life - damage <= 0)
            {
                if (proj.minion && proj.owner == player.whoAmI && player.MinionAttackTargetNPC == target.whoAmI)
                {
                    foreach (NPC npc in Main.npc.Where(n => n.active && n.chaseable && Vector2.DistanceSquared(n.Center, target.Center) < Math.Pow(240, 2)))
                    {
                        Projectile.NewProjectile(player.Center, Vector2.Zero, ModContent.ProjectileType<Acorn>(), 5, 1, player.whoAmI, npc.whoAmI);
                    }
                }
            }
		}
	}

    class Acorn : ModProjectile
	{
        Vector2 savedPos;

		public override string Texture => "Terraria/Item_27";

		public override void SetDefaults()
		{
            projectile.width = 16;
            projectile.height = 16;
            projectile.timeLeft = 60;
            projectile.friendly = true;
            projectile.tileCollide = false;
            projectile.aiStyle = -1;
		}

		public override void AI()
		{
            var target = Main.npc[(int)projectile.ai[0]];

            if(target != null && target.active)
			{
                if (projectile.timeLeft == 60)
                    savedPos = projectile.Center;

                float progress = 1 - (projectile.timeLeft - 2) / 60f;
                projectile.Center = Vector2.Lerp(savedPos, target.Center, progress) + new Vector2(0, (4 * progress - 4 * (float)Math.Pow(progress, 2)) * -520);
			}
			else
			{

			}

            projectile.rotation += 0.1f;
		}
	}
}
