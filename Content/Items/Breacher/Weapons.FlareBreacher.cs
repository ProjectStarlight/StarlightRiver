using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Items.Breacher
{
    public class FlareBreacher : ModItem
    {
        public override string Texture => AssetDirectory.BreacherItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Flare Breacher");
            Tooltip.SetDefault("Left click to launch explosive flares \nRight click to launch a target flare");
        }

        public override void SetDefaults()
        {
            item.useStyle = ItemUseStyleID.HoldingOut;
            item.useAnimation = 30;
            item.useTime = 30;
            item.knockBack = 2f;
            item.UseSound = SoundID.Item11;
            item.width = 24;
            item.height = 28;
            item.damage = 28;
            item.rare = ItemRarityID.Orange;
            item.value = Item.sellPrice(0, 10, 0, 0);
            item.noMelee = true;
            item.autoReuse = true;
            item.useTurn = false;
            item.useAmmo = AmmoID.Flare;
            item.ranged = true;
            item.shoot = ModContent.ProjectileType<ExplosiveFlare>();
            item.shootSpeed = 17;
        }

        public override Vector2? HoldoutOffset() => new Vector2(0, 0);


        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            position.Y -= 4;
            type = ModContent.ProjectileType<ExplosiveFlare>();
            return base.Shoot(player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack);
        }
    }

    internal class ExplosiveFlare : ModProjectile
    {
        int enemyID;
        bool stuck = false;
        int explosionTimer = 100;
        Vector2 offset = Vector2.Zero;

        public override string Texture => AssetDirectory.BreacherItem + Name;

        public override void SetDefaults()
        {
            projectile.width = 10;
            projectile.height = 16;

            projectile.ranged = true;
            projectile.friendly = true;
            projectile.aiStyle = 1;
            aiType = 163;
            projectile.penetrate = -1;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Explosive Flare");
            Main.projFrames[projectile.type] = 2;
        }

        public override bool PreAI()
        {
            if (stuck)
            {
                NPC target = Main.npc[enemyID];
                projectile.position = target.position + offset;
                explosionTimer--;

                if (explosionTimer <= 0 || !target.active)
                    Explode(target);
                return false;
            }
            else
                projectile.rotation = projectile.velocity.ToRotation() + 1.57f;

            return base.PreAI();
        }

		public override void AI()
		{
            var dust = Dust.NewDustPerfect(projectile.Center + Vector2.UnitX.RotatedBy(projectile.rotation) * projectile.width / 2, DustID.Fire, Vector2.UnitX.RotatedBy(-projectile.rotation), 0, default, 0.5f);
            dust.noGravity = true;
		}

		private void Explode(NPC target)
        {
            for(float k = -0.5f; k < 0.5f; k += 0.1f)

            projectile.active = false;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            if (!stuck && target.life > 0)
            {
                stuck = true;
                projectile.friendly = false;
                projectile.tileCollide = false;
                enemyID = target.whoAmI;
                offset = projectile.position - target.position;
                offset -= projectile.velocity;
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            var tex = Main.projectileTexture[projectile.type];
            var source = new Rectangle(0, 0, projectile.width, projectile.height);

            if (stuck)
                source.Y += projectile.height * (Main.GameUpdateCount % 10 < 5 ? 1 : 0);

            Main.spriteBatch.Draw(tex, projectile.position - Main.screenPosition, source, lightColor, projectile.rotation, projectile.Size / 2, projectile.scale, 0, 0);

            return false;
        }
    }
    internal class FlareExplosion : ModProjectile
    {
        public override string Texture => AssetDirectory.Invisible;
        public override void SetDefaults()
        {
            projectile.width = 24;
            projectile.height = 24;
            projectile.alpha = 255;//invisible
            projectile.ranged = true;
            projectile.friendly = true;
            projectile.timeLeft = 14;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.extraUpdates = 1;
        }

        public override void SetStaticDefaults() => DisplayName.SetDefault("Flare Explosion");

        public override void AI() { }
    }
}