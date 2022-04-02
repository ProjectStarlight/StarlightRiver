using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using System.Collections.Generic;
using StarlightRiver.Core;
using StarlightRiver.Content.Items.Dungeon;
using StarlightRiver.Helpers;
using StarlightRiver.Content.Items.Vitric;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Bosses.VitricBoss;
using StarlightRiver.Content.CustomHooks;
using Terraria.Graphics.Effects;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Moonstone
{
	[AutoloadEquip(EquipType.Neck)]
	public class DianesPendant : ModItem
	{
		public override string Texture => AssetDirectory.MoonstoneItem + Name;
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Diane's Pendant");
			Tooltip.SetDefault("Something about a crescent idk \n+20 barrier"); //EGSHELS FIX!!!!!

		}

		public override void SetDefaults()
		{
			item.width = 30;
			item.height = 28;
			item.rare = 3;
			item.value = Item.buyPrice(0, 5, 0, 0);
			item.accessory = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetModPlayer<ShieldPlayer>().MaxShield += 20;
			player.GetModPlayer<DianePlayer>().Active = true;

			if (player.ownedProjectileCounts[ModContent.ProjectileType<DianeCrescant>()] < 1 && !player.dead)
			{
				Projectile.NewProjectile(player.Center, new Vector2(7, 7), ModContent.ProjectileType<DianeCrescant>(), (int)(30 * player.magicDamage), 1.5f, player.whoAmI);
			}
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ModContent.ItemType<MoonstoneBar>(), 4);
			recipe.AddIngredient(ModContent.ItemType<AquaSapphire>(), 1);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
	internal class DianePlayer : ModPlayer
    {
		public bool Active = false;

        public override void ResetEffects()
        {
			Active = false;
        }
    }

    public class DianeCrescant : ModProjectile
    {
        public override string Texture => AssetDirectory.MoonstoneItem + Name;

        private const int MAXCHARGE = 20;

        public bool Attacking = false;

        public int Charges = 0;

        private float speed = 10;

        private List<NPC> alreadyHit = new List<NPC>();

        public float Charge => ((float)Charges / (float)MAXCHARGE);

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Crescant");
            Main.projPet[projectile.type] = true;
            Main.projFrames[projectile.type] = 1;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 1;
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
            ProjectileID.Sets.MinionSacrificable[projectile.type] = true;
            ProjectileID.Sets.Homing[projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[projectile.type] = true;
        }

        public override void SetDefaults()
        {
            projectile.netImportant = true;
            projectile.width = 68;
            projectile.height = 68;
            projectile.friendly = false;
            projectile.minion = true;
            projectile.minionSlots = 0;
            projectile.penetrate = -1;
            projectile.timeLeft = 216000;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Player player = Main.player[projectile.owner];

            if (player.dead)
                projectile.active = false;

            if (player.GetModPlayer<DianePlayer>().Active)
                projectile.timeLeft = 2;

            if (Attacking)
                AttackMovement(player);
            else
                IdleMovement(player);
        }

        private void AttackMovement(Player player)
        {

        }

        private void IdleMovement(Player player)
        {
            Vector2 direction = player.Center - projectile.Center;
            if (direction.Length() > 150)
            {
                direction.Normalize();
                projectile.velocity = Vector2.Lerp(projectile.velocity, direction * speed, 0.03f);
            }
        }
    }
}