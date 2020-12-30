using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using StarlightRiver.Core;
using StarlightRiver.Content.Items.Slime;

namespace StarlightRiver.Content.Items.Slime
{
    public class SlimeStaff : ModItem
    {
        public override string Texture => AssetDirectory.SlimeItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Slime Slinger");
            Tooltip.SetDefault("Yabba Dabba Doo");
            Item.staff[item.type] = true;
        }

        public int projectileCountMax;
        public int[] projIndexArray;
        private const int distanceFromPlayer = 40;

        public override void SetDefaults()
        {
            projectileCountMax = 3;

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

            projIndexArray = new int[projectileCountMax];
        }

        //public override void UpdateInventory(Player player)//debug
        //{
        //    int interCount = 3;
        //    for (int k = 0; k < interCount; k++)
        //    {
        //        //Vector2 direction = new Vector2(0, -1).RotatedBy((((float)(k - (interCount / 2)) + (interCount % 2 != 0 ? 0 : 0.5f)) / 3) * System.Math.PI * 1.75f);
        //        //Vector2 spawnPoint = player.Center + (direction * distanceFromPlayer);
        //        //Vector2 velocity = Vector2.Normalize(spawnPoint - (player.Bottom + new Vector2(0, 20))) * item.shootSpeed;
        //        //Dust.NewDustPerfect(spawnPoint, 20, Vector2.Zero, 0, Color.White, 1f);
        //    }
        //}
        public override bool CanUseItem(Player player)
        {
            int activeProjScore = ActiveProjectileScore(player);

            return (activeProjScore >= 0 && activeProjScore < projectileCountMax) ? base.CanUseItem(player) : false;
        }

        private int ActiveProjectileScore(Player player)
        {
            int activeProjScore = 0;

            foreach (int index in projIndexArray)//adds up the score for all active projectiles in the list
            {
                Projectile curProj = Main.projectile[index];//variable for cleanliness
                if (curProj.active && curProj.type == ModContent.ProjectileType<SlimeStaffProjectile>() && curProj.owner == player.whoAmI)//active, correct type, and correct owner
                {
                    SlimeStaffProjectile curSlimeProj = curProj.modProjectile as SlimeStaffProjectile;//variable for cleanliness (only used once so...)
                    activeProjScore += curSlimeProj.globSize;
                }
            }

            return activeProjScore;
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            int activeProjScore = ActiveProjectileScore(player);//if the item shoots, this is run twice, no way to use this once for both places

            //this check is kinda redundant, but may as well be here in case of changes in CanUseItem
            if (activeProjScore >= 0 && activeProjScore < projectileCountMax)//if the score is lower than the max amount of projectiles, then spawn new ones to fill up the difference
            {
                int toSpawn = projectileCountMax - activeProjScore;//the amount to spawn
                for (int k = 0; k < toSpawn; k++)//checks for already active projectiles and sizes
                {
                    Vector2 direction = new Vector2(0, -1).RotatedBy((((float)(k - (toSpawn / 2)) + (toSpawn % 2 != 0 ? 0 : 0.5f)) / 3) * System.Math.PI * 1.75f);
                    Vector2 spawnPoint = player.Center + (direction * distanceFromPlayer);
                    Vector2 velocity = Vector2.Normalize(spawnPoint - (player.Bottom + new Vector2(0, 20))) * item.shootSpeed;

                    projIndexArray[k] = Projectile.NewProjectile(spawnPoint, velocity, type, damage, knockBack, player.whoAmI);

                    (Main.projectile[projIndexArray[k]].modProjectile as SlimeStaffProjectile).parentItem = this;//setting the item reference on the projectile's side
                }
            }

            return false;
        }
    }
}