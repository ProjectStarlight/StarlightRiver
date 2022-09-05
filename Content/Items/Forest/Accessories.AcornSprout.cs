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

        public override void SafeSetDefaults() => Item.rare = ItemRarityID.Blue;

        public override void Load()
        {
            StarlightPlayer.ModifyHitNPCWithProjEvent += SpawnAcorn;           
        }

		public override void Unload()
		{
            StarlightPlayer.ModifyHitNPCWithProjEvent -= SpawnAcorn;
        }   

		private void SpawnAcorn(Player player, Projectile proj, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
            if (target.life - damage <= 0)
            {
                if (proj.minion && proj.owner == player.whoAmI && player.MinionAttackTargetNPC == target.whoAmI)
                {
                    foreach (NPC NPC in Main.npc.Where(n => n.active && n.chaseable && Vector2.DistanceSquared(n.Center, target.Center) < Math.Pow(240, 2)))
                    {
                        Projectile.NewProjectile(player.GetSource_Accessory(Item), player.Center, Vector2.Zero, ModContent.ProjectileType<Acorn>(), 5, 1, player.whoAmI, NPC.whoAmI);
                    }
                }
            }
		}
	}

    class Acorn : ModProjectile
	{
        Vector2 savedPos;

		public override string Texture => AssetDirectory.ForestItem + Name;

		public override void SetDefaults()
		{
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.timeLeft = 60;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.aiStyle = -1;
		}

		public override void AI()
		{
            var target = Main.npc[(int)Projectile.ai[0]];

            if(target != null && target.active)
			{
                if (Projectile.timeLeft == 60)
                    savedPos = Projectile.Center;

                float progress = 1 - (Projectile.timeLeft - 2) / 60f;
                Projectile.Center = Vector2.Lerp(savedPos, target.Center, progress) + new Vector2(0, (4 * progress - 4 * (float)Math.Pow(progress, 2)) * -520);
			}
			else
			{

			}

            Projectile.rotation += 0.1f;
		}
	}
}
