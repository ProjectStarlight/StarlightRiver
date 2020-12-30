using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using System;

namespace StarlightRiver.Content.Items.Overgrow
{
    public class OvergrowBow : ModItem
    {
        public override string Texture => AssetDirectory.OvergrowItem + "OvergrowBow";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Overgrown Bow");
        }

        public override void SetDefaults()
        {
            item.damage = 20;
            item.ranged = true;
            item.width = 40;
            item.height = 20;
            item.useTime = 30;
            item.useAnimation = 30;
            item.useStyle = ItemUseStyleID.HoldingOut;
            item.noMelee = true;
            item.knockBack = 4;
            item.rare = ItemRarityID.Green;
            item.UseSound = SoundID.Item5;
            item.shoot = ProjectileID.PurificationPowder;
            item.shootSpeed = 14f;
            item.useAmmo = AmmoID.Arrow;
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            int proj = Projectile.NewProjectile(player.Center.X, player.Center.Y, 0f, 0f, ProjectileType<LeafSpawner>(), damage, knockBack, player.whoAmI);
            LeafSpawner spawner = Main.projectile[proj].modProjectile as LeafSpawner;
            spawner.Parent = Projectile.NewProjectile(player.Center, new Vector2(speedX, speedY), type, damage, knockBack, player.whoAmI);
            return false;
        }
    }

    internal class LeafSpawner : ModProjectile
    {
        public override string Texture => AssetDirectory.Invisible;

        public override void SetDefaults()
        {
            projectile.width = 1;
            projectile.height = 1;
            projectile.damage = 0;
            projectile.aiStyle = -1;
            projectile.ignoreWater = true;
            projectile.friendly = true;
        }

        public int Parent { get; set; }

        public override void AI()
        {
            projectile.ai[0]++;
            if (!Main.projectile[Parent].active) projectile.Kill();
            projectile.position = Main.projectile[Parent].position;
            if (projectile.ai[0] % 10 == 0) Projectile.NewProjectile(projectile.Center, new Vector2(0, 0), ProjectileType<BowLeaf>(), projectile.damage, projectile.knockBack, projectile.owner);
        }
    }

    internal class BowLeaf : ModProjectile
    {
        public override string Texture => AssetDirectory.OvergrowItem + Name;

        public override void SetDefaults()
        {
            projectile.width = 14;
            projectile.height = 13;
            projectile.damage = 9;
            projectile.aiStyle = -1;
            projectile.ignoreWater = true;
            projectile.friendly = true;
            projectile.timeLeft = 160;
            Main.projFrames[projectile.type] = 6;
        }

        public override void AI()
        {
            projectile.ai[0]++;

            if (++projectile.frameCounter >= 18)
            {
                projectile.frameCounter = 0;

                if (++projectile.frame >= 6) 
                    projectile.frame = 0;
            }

            projectile.velocity = new Vector2(2 * (float)Math.Sin(MathHelper.ToRadians(projectile.ai[0] * MathHelper.Pi)), 1 + 1.2f * (float)Math.Sin(MathHelper.ToRadians(projectile.ai[0] * (MathHelper.Pi * 2))));
        }
    }
}