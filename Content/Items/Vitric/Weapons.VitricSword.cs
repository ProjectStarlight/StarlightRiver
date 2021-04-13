using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Items.Vitric
{
    internal class VitricSword : ModItem
    {
        public override string Texture => AssetDirectory.VitricItem + Name;

        public bool Broken = false;

        public override void SetDefaults()
        {
            item.damage = 25;
            item.melee = true;
            item.width = 36;
            item.height = 38;
            item.useTime = 18;
            item.useAnimation = 18;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.knockBack = 7.5f;
            item.value = 1000;
            item.rare = ItemRarityID.Green;
            item.UseSound = SoundID.Item1;
            item.autoReuse = false;
            item.useTurn = true;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Ancient Vitric Blade");
            Tooltip.SetDefault("Shatters into enchanted glass shards \nUnable to be used while shattered");
        }

        public override void OnHitNPC(Player player, NPC target, int damage, float knockback, bool crit)
        {
            if (!Broken)
            {
                Main.PlaySound(SoundID.Item107);
                Projectile.NewProjectile(target.Center, Vector2.Normalize(player.Center - target.Center) * -24, mod.ProjectileType("VitricSwordProjectile"), 15, 0, player.whoAmI);
                Projectile.NewProjectile(target.Center, Vector2.Normalize(player.Center - target.Center).RotatedBy(0.3) * -13, mod.ProjectileType("VitricSwordProjectile"), 15, 0, player.whoAmI);
                Projectile.NewProjectile(target.Center, Vector2.Normalize(player.Center - target.Center).RotatedBy(-0.25) * -18, mod.ProjectileType("VitricSwordProjectile"), 15, 0, player.whoAmI);

                for (int k = 0; k <= 20; k++)
                {
                    Dust.NewDust(Vector2.Lerp(player.Center, target.Center, 0.4f), 8, 8, mod.DustType("Air"), (Vector2.Normalize(player.Center - target.Center) * -2).X, (Vector2.Normalize(player.Center - target.Center) * -2).Y);

                    float vel = Main.rand.Next(-300, -100) * 0.1f;
                    int dus = Dust.NewDust(Vector2.Lerp(player.Center, target.Center, 0.4f), 16, 16, mod.DustType("Glass"), (Vector2.Normalize(player.Center - target.Center) * vel).X, (Vector2.Normalize(player.Center - target.Center) * vel).Y);
                    Main.dust[dus].customData = player;
                }
                Broken = true;
            }
        }

        public override bool CanUseItem(Player player)
        {
            if (Main.projectile.Any(projectile => projectile.type == mod.ProjectileType("VitricSwordProjectile") && projectile.owner == player.whoAmI && projectile.active))
                return false;
            else
                Broken = false;
            return true;
        }
    }

    internal class VitricSwordProjectile : ModProjectile
    {
        public override string Texture => AssetDirectory.VitricItem + Name;

        public override void SetDefaults()
        {
            projectile.width = 12;
            projectile.height = 18;
            projectile.friendly = true;
            projectile.penetrate = -1;
            projectile.timeLeft = 120;
            projectile.tileCollide = false;
            projectile.ignoreWater = false;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Enchanted Glass");
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            Main.PlaySound(SoundID.Item27);
        }

        private float f = 1;

        public override void AI()
        {
            f += 0.1f;
            Player player = Main.player[projectile.owner];
            projectile.position += Vector2.Normalize(player.Center - projectile.Center) * f;
            projectile.velocity *= 0.94f;
            projectile.rotation = (player.Center - projectile.Center).Length() * 0.1f;

            if ((player.Center - projectile.Center).Length() <= 32 && projectile.timeLeft < 110)
            {
                projectile.timeLeft = 0;
                Main.PlaySound(SoundID.Item101);
            }
        }
    }
}