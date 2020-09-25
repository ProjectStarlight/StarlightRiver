using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using StarlightRiver.Core;
using StarlightRiver.Projectiles.WeaponProjectiles.Slime;

namespace StarlightRiver.Items.Slime
{
    public class SlimeStaff : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Slime Slinger");
            Tooltip.SetDefault("Yabba Dabba Doo");
            Item.staff[item.type] = true;
        }

        public int projectileCount;
        public int[] projIndexArray;

        public override void SetDefaults()
        {
            projectileCount = 3;

            item.damage = 20;
            item.magic = true;
            item.mana = 10;
            item.width = 18;
            item.height = 34;
            item.useTime = 30;
            item.useAnimation = 30;
            item.value = Item.sellPrice(0, 0, 10, 0);//todo
            item.rare = ItemRarityID.Green;
            item.useStyle = ItemUseStyleID.HoldingOut;
            item.UseSound = SoundID.Item43;
            item.knockBack = 0f;
            item.shoot = ModContent.ProjectileType<SlimeStaffProjectile>();
            item.shootSpeed = 5f;
            item.noMelee = true;
            item.autoReuse = true;

            projIndexArray = new int[projectileCount];
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            for (int k = 0; k < projectileCount; k++)//checks for alreacy actives projectiles and sizes
            {
                //TODO, this should spawn in set points around the player
                Vector2 newVelocity = new Vector2(speedX, speedY).RotatedBy(Main.rand.NextFloat(-1.5f, 1.5f)) * Main.rand.NextFloat(0.5f, 3f);
                projIndexArray[k] = Projectile.NewProjectile(position, newVelocity, type, damage, knockBack, player.whoAmI);
            }

            foreach (int index in projIndexArray)
            {
                if (Main.projectile[index].active && Main.projectile[index].type == ModContent.ProjectileType<SlimeStaffProjectile>())
                {
                    (Main.projectile[index].modProjectile as SlimeStaffProjectile).parentItem = this;
                }
            }
            return false;
        }
    }
}