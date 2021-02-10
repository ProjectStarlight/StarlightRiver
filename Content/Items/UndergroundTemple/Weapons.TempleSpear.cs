using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Content.Projectiles;

namespace StarlightRiver.Content.Items.UndergroundTemple
{
    class TempleSpear : ModItem
    {
        public override string Texture => AssetDirectory.CaveTempleItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Point of Light");
            Tooltip.SetDefault("Struck foes glow\nSlain foes leave behind a bright light");
        }

        public override void SetDefaults()
        {
            item.melee = true;
            item.width = 32;
            item.height = 32;
            item.damage = 11;
            item.crit = 10;
            item.useStyle = ItemUseStyleID.HoldingOut;
            item.useTime = 30;
            item.useAnimation = 30;
            item.noUseGraphic = true;
            item.noMelee = true;
            item.knockBack = 2;
            item.rare = ItemRarityID.Blue;
            item.shoot = ProjectileType<TempleSpearProjectile>();
            item.shootSpeed = 1;
            item.UseSound = SoundID.Item15;
        }
    }

    class TempleSpearProjectile : SpearProjectile
    {
        public TempleSpearProjectile() : base(30, 25, 100) { }

        public override string Texture => AssetDirectory.CaveTempleItem + Name;

        public override void PostAI()
        {
            //Dust effects
            Dust d = Dust.NewDustPerfect(projectile.Center, 264, projectile.velocity.RotatedBy(-0.5f));
            d.noGravity = true;
            d.color = new Color(255, 255, 200) * (projectile.timeLeft / (30f * Main.player[projectile.owner].meleeSpeed));
            d.scale = projectile.timeLeft / (30f * Main.player[projectile.owner].meleeSpeed);

            d = Dust.NewDustPerfect(projectile.Center, 264, projectile.velocity.RotatedBy(0.5f));
            d.noGravity = true;
            d.color = new Color(255, 255, 200) * (projectile.timeLeft / (30f * Main.player[projectile.owner].meleeSpeed));
            d.scale = projectile.timeLeft / (30f * Main.player[projectile.owner].meleeSpeed);
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            //inflicting debuff + light orbs on kill
            target.AddBuff(BuffType<Buffs.Illuminant>(), 600);
            if (damage >= target.life) Projectile.NewProjectile(target.Center, new Vector2(0, -1), ProjectileType<TempleSpearLight>(), 0, 0);
        }
    }

    class TempleSpearLight : ModProjectile
    {
        public override string Texture => AssetDirectory.CaveTempleItem + Name;

        public override void SetDefaults()
        {
            projectile.width = 16;
            projectile.height = 16;
            projectile.timeLeft = 3600;
        }

        public override void AI()
        {
            projectile.velocity *= 0.99f;
            Lighting.AddLight(projectile.Center, new Vector3(1, 1, 1) * projectile.timeLeft / 3600f);
            Dust d = Dust.NewDustPerfect(projectile.Center, 264);
            d.noGravity = true;
            d.color = new Color(255, 255, 200) * (projectile.timeLeft / 3600f);
        }
    }

}
