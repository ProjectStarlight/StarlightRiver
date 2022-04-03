using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Vitric
{
	internal class VitricSword : ModItem
    {
        public override string Texture => AssetDirectory.VitricItem + Name;

        public bool Broken = false;

        public override void SetDefaults()
        {
            Item.damage = 35;
            Item.melee = true;
            Item.width = 36;
            Item.height = 38;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.SwingThrow;
            Item.knockBack = 7.5f;
            Item.value = 1000;
            Item.rare = ItemRarityID.Green;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = false;
            Item.useTurn = true;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Ancient Vitric Blade");
            Tooltip.SetDefault("Shatters into enchanted glass shards \nUnable to be used while shattered");
        }

		public override bool? CanHitNPC(Player Player, NPC target)
		{
            return !Broken;
		}

		public override void OnHitNPC(Player Player, NPC target, int damage, float knockback, bool crit)
        {
            if (!Broken)
            {
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item107);
                Projectile.NewProjectile(target.Center, Vector2.Normalize(Player.Center - target.Center) * -32, Mod.ProjectileType("VitricSwordProjectile"), 24, 0, Player.whoAmI);
                Projectile.NewProjectile(target.Center, Vector2.Normalize(Player.Center - target.Center).RotatedBy(0.3) * -16, Mod.ProjectileType("VitricSwordProjectile"), 24, 0, Player.whoAmI);
                Projectile.NewProjectile(target.Center, Vector2.Normalize(Player.Center - target.Center).RotatedBy(-0.25) * -24, Mod.ProjectileType("VitricSwordProjectile"), 24, 0, Player.whoAmI);

                for (int k = 0; k <= 20; k++)
                {
                    Dust.NewDust(Vector2.Lerp(Player.Center, target.Center, 0.4f), 8, 8, ModContent.DustType<Dusts.Air>(), (Vector2.Normalize(Player.Center - target.Center) * -2).X, (Vector2.Normalize(Player.Center - target.Center) * -2).Y);

                    float vel = Main.rand.Next(-300, -100) * 0.1f;
                    int dus = Dust.NewDust(Vector2.Lerp(Player.Center, target.Center, 0.4f), 16, 16, ModContent.DustType<Dusts.GlassAttracted>(), (Vector2.Normalize(Player.Center - target.Center) * vel).X, (Vector2.Normalize(Player.Center - target.Center) * vel).Y);
                    Main.dust[dus].customData = Player;
                }
                Broken = true;
            }
        }

        public override bool CanUseItem(Player Player)
        {
            if (Main.projectile.Any(Projectile => Projectile.type == Mod.ProjectileType("VitricSwordProjectile") && Projectile.owner == Player.whoAmI && Projectile.active))
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
            Projectile.width = 12;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = false;
            Projectile.melee = true;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Enchanted Glass");
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item27);
        }

        private float f = 1;

        public override void AI()
        {
            f += 0.1f;
            Player Player = Main.player[Projectile.owner];
            Projectile.position += Vector2.Normalize(Player.Center - Projectile.Center) * f;
            Projectile.velocity *= 0.94f;
            Projectile.rotation = (Player.Center - Projectile.Center).Length() * 0.1f;

            if ((Player.Center - Projectile.Center).Length() <= 32 && Projectile.timeLeft < 110)
            {
                Projectile.timeLeft = 0;
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item101);
            }
        }
    }
}