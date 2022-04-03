using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

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
            Item.damage = 20;
            Item.ranged = true;
            Item.width = 40;
            Item.height = 20;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.HoldingOut;
            Item.noMelee = true;
            Item.knockBack = 4;
            Item.rare = ItemRarityID.Green;
            Item.UseSound = SoundID.Item5;
            Item.shoot = ProjectileID.PurificationPowder;
            Item.shootSpeed = 14f;
            Item.useAmmo = AmmoID.Arrow;
        }

        public override bool Shoot(Player Player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            int proj = Projectile.NewProjectile(Player.Center.X, Player.Center.Y, 0f, 0f, ProjectileType<LeafSpawner>(), damage, knockBack, Player.whoAmI);
            LeafSpawner spawner = Main.projectile[proj].ModProjectile as LeafSpawner;
            spawner.Parent = Projectile.NewProjectile(Player.Center, new Vector2(speedX, speedY), type, damage, knockBack, Player.whoAmI);
            return false;
        }
    }

    internal class LeafSpawner : ModProjectile
    {
        public override string Texture => AssetDirectory.Invisible;

        public override void SetDefaults()
        {
            Projectile.width = 1;
            Projectile.height = 1;
            Projectile.damage = 0;
            Projectile.aiStyle = -1;
            Projectile.ignoreWater = true;
            Projectile.friendly = true;
        }

        public int Parent { get; set; }

        public override void AI()
        {
            Projectile.ai[0]++;
            if (!Main.projectile[Parent].active) Projectile.Kill();
            Projectile.position = Main.projectile[Parent].position;
            if (Projectile.ai[0] % 10 == 0) Projectile.NewProjectile(Projectile.Center, new Vector2(0, 0), ProjectileType<BowLeaf>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
        }
    }

    internal class BowLeaf : ModProjectile
    {
        public override string Texture => AssetDirectory.OvergrowItem + Name;

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 13;
            Projectile.damage = 9;
            Projectile.aiStyle = -1;
            Projectile.ignoreWater = true;
            Projectile.friendly = true;
            Projectile.timeLeft = 160;
            Main.projFrames[Projectile.type] = 6;
        }

        public override void AI()
        {
            Projectile.ai[0]++;

            if (++Projectile.frameCounter >= 18)
            {
                Projectile.frameCounter = 0;

                if (++Projectile.frame >= 6) 
                    Projectile.frame = 0;
            }

            Projectile.velocity = new Vector2(2 * (float)Math.Sin(MathHelper.ToRadians(Projectile.ai[0] * MathHelper.Pi)), 1 + 1.2f * (float)Math.Sin(MathHelper.ToRadians(Projectile.ai[0] * (MathHelper.Pi * 2))));
        }
    }
}