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
			Tooltip.SetDefault("Something about a crescent idk \n+20 barrier"); //TODO: EGSHELS FIX!!!!!

		}

		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 28;
			Item.rare = 3;
			Item.value = Item.buyPrice(0, 5, 0, 0);
			Item.accessory = true;
		}

		public override void UpdateAccessory(Player Player, bool hideVisual)
		{
			Player.GetModPlayer<BarrierPlayer>().MaxBarrier += 20;
			Player.GetModPlayer<DianePlayer>().Active = true;

			if (Player.ownedProjectileCounts[ModContent.ProjectileType<DianeCrescant>()] < 1 && !Player.dead)
			{
				Projectile.NewProjectile(Player.GetSource_Accessory(Item), Player.Center, new Vector2(7, 7), ModContent.ProjectileType<DianeCrescant>(), 30, 1.5f, Player.whoAmI);
			}
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<MoonstoneBarItem>(), 4);
			recipe.AddIngredient(ModContent.ItemType<AquaSapphire>(), 1);
			recipe.AddTile(TileID.Anvils);
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
            Main.projPet[Projectile.type] = true;
            Main.projFrames[Projectile.type] = 1;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 1;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 68;
            Projectile.height = 68;
            Projectile.friendly = false;
            Projectile.minion = true;
            Projectile.minionSlots = 0;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 216000;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Magic;
        }

        public override void AI()
        {
            Player Player = Main.player[Projectile.owner];

            if (Player.dead)
                Projectile.active = false;

            if (Player.GetModPlayer<DianePlayer>().Active)
                Projectile.timeLeft = 2;

            if (Attacking)
                AttackMovement(Player);
            else
                IdleMovement(Player);
        }

        private void AttackMovement(Player Player)
        {

        }

        private void IdleMovement(Player Player)
        {
            Vector2 direction = Player.Center - Projectile.Center;
            if (direction.Length() > 150)
            {
                direction.Normalize();
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * speed, 0.03f);
            }
        }
    }
}