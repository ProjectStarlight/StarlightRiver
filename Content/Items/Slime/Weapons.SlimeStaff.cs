using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Slime
{
	public class SlimeStaff : ModItem
    {
        public int ProjectileCountMax;
        public int[] projIndexArray;
        private const int distanceFromPlayer = 40;

        public override string Texture => AssetDirectory.SlimeItem + Name;

		public override void Load()
		{
            StarlightNPC.NPCLootEvent += DropSlimeStaff;
			
		}

		public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Slime Slinger");
            Tooltip.SetDefault("Yabba Dabba Doo");
            Item.staff[Item.type] = true;
        }

        public override void SetDefaults()
        {
            ProjectileCountMax = 3;

            Item.damage = 20;
            Item.magic = true;
            Item.mana = 10;
            Item.width = 18;
            Item.height = 34;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.value = Item.sellPrice(0, 0, 10, 0);//todo
            Item.rare = ItemRarityID.Green;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.UseSound = SoundID.Item43;
            Item.knockBack = 0f;
            Item.shoot = ModContent.ProjectileType<SlimeStaffProjectile>();
            Item.shootSpeed = 5f;
            Item.noMelee = true;
            Item.autoReuse = true;

            projIndexArray = new int[ProjectileCountMax];
        }

        private void DropSlimeStaff(NPC NPC)
        {
            if (Main.slimeRain && NPC.aiStyle == 1 && Main.rand.Next(200) == 0)
                Item.NewItem(NPC.Center, ModContent.ItemType<SlimeStaff>());
        }

        //public override void UpdateInventory(Player Player)//debug
        //{
        //    int interCount = 3;
        //    for (int k = 0; k < interCount; k++)
        //    {
        //        //Vector2 direction = new Vector2(0, -1).RotatedBy((((float)(k - (interCount / 2)) + (interCount % 2 != 0 ? 0 : 0.5f)) / 3) * System.Math.PI * 1.75f);
        //        //Vector2 spawnPoint = Player.Center + (direction * distanceFromPlayer);
        //        //Vector2 velocity = Vector2.Normalize(spawnPoint - (Player.Bottom + new Vector2(0, 20))) * Item.shootSpeed;
        //        //Dust.NewDustPerfect(spawnPoint, 20, Vector2.Zero, 0, Color.White, 1f);
        //    }
        //}
        public override bool CanUseItem(Player Player)
        {
            int activeProjScore = ActiveProjectileScore(Player);

            return (activeProjScore >= 0 && activeProjScore < ProjectileCountMax) ? base.CanUseItem(Player) : false;
        }

        private int ActiveProjectileScore(Player Player)
        {
            int activeProjScore = 0;

            foreach (int index in projIndexArray)//adds up the score for all active Projectiles in the list
            {
                Projectile curProj = Main.projectile[index];//variable for cleanliness
                if (curProj.active && curProj.type == ModContent.ProjectileType<SlimeStaffProjectile>() && curProj.owner == Player.whoAmI)//active, correct type, and correct owner
                {
                    SlimeStaffProjectile curSlimeProj = curProj.ModProjectile as SlimeStaffProjectile;//variable for cleanliness (only used once so...)
                    activeProjScore += curSlimeProj.globSize;
                }
            }

            return activeProjScore;
        }

        public override bool Shoot(Player Player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            int activeProjScore = ActiveProjectileScore(Player);//if the Item shoots, this is run twice, no way to use this once for both places

            //this check is kinda redundant, but may as well be here in case of changes in CanUseItem
            if (activeProjScore >= 0 && activeProjScore < ProjectileCountMax)//if the score is lower than the max amount of Projectiles, then spawn new ones to fill up the difference
            {
                int toSpawn = ProjectileCountMax - activeProjScore;//the amount to spawn
                for (int k = 0; k < toSpawn; k++)//checks for already active Projectiles and sizes
                {
                    Vector2 direction = new Vector2(0, -1).RotatedBy((((float)(k - (toSpawn / 2)) + (toSpawn % 2 != 0 ? 0 : 0.5f)) / 3) * System.Math.PI * 1.75f);
                    Vector2 spawnPoint = Player.Center + (direction * distanceFromPlayer);
                    Vector2 velocity = Vector2.Normalize(spawnPoint - (Player.Bottom + new Vector2(0, 20))) * Item.shootSpeed;

                    projIndexArray[k] = Projectile.NewProjectile(spawnPoint, velocity, type, damage, knockBack, Player.whoAmI);

                    (Main.projectile[projIndexArray[k]].ModProjectile as SlimeStaffProjectile).parentItem = this;//setting the Item reference on the Projectile's side
                }
            }

            return false;
        }
    }
}