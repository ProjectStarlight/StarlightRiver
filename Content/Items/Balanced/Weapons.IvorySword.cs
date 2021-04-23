using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;


namespace StarlightRiver.Content.Items.Balanced
{
    public class IvorySword : ModItem
    {
        public override string Texture => AssetDirectory.BalancedItem + Name;

        private int combostate = 0;

        public override void SetDefaults()
        {
            item.width = 44;
            item.height = 44;
            item.useStyle = ItemUseStyleID.HoldingOut;
            item.useAnimation = 16;
            item.useTime = 16;
            item.knockBack = 2f;
            item.damage = 46;
            item.shoot = ProjectileType<IvorySwordProjectile>();
            item.rare = ItemRarityID.LightRed;
            item.autoReuse = false;
            item.noMelee = true;
            item.melee = true;
            item.noUseGraphic = true;
        }

        public override bool CloneNewInstances => true;

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            Vector2 aim = Vector2.Normalize(Main.MouseWorld - player.Center);
            int proj = Projectile.NewProjectile(player.Center + aim * 18, aim * 0.1f, type, damage, knockBack, player.whoAmI);
            Main.projectile[proj].localAI[1] = combostate;

            if (combostate < 2) Main.PlaySound(SoundID.Item65, player.Center); else Main.PlaySound(SoundID.Item64, player.Center);
            combostate++;
            if (combostate > 2) combostate = 0; return false;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Ivory Rapier");
            Tooltip.SetDefault("Huzzah!");
        }
    }

    public class IvorySwordProjectile : ModProjectile
    {
        public override string Texture => AssetDirectory.BalancedItem + Name;

        public override void SetDefaults()
        {
            projectile.friendly = true;
            projectile.width = 200;
            projectile.height = 189;
            projectile.penetrate = -1;
            projectile.timeLeft = 14;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("White Slash");
            Main.projFrames[projectile.type] = 21;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            if (projectile.localAI[1] == 2) Main.player[projectile.owner].velocity *= 0.3f;
        }

        public override void AI()
        {
            projectile.position += Main.player[projectile.owner].velocity;
            projectile.rotation = projectile.velocity.ToRotation();

            if (projectile.localAI[0] == 1) projectile.damage = 0;

            if (projectile.timeLeft <= 8) projectile.localAI[0] = 1;

            if (projectile.timeLeft == 14) projectile.frame = 7 * (int)projectile.localAI[1];

            if (++projectile.frameCounter >= 2)
            {
                projectile.frameCounter = 0;
                if (++projectile.frame >= 7 + 7 * (int)projectile.localAI[1])
                    projectile.frame = 7 * (int)projectile.localAI[1];
            }

            if (projectile.localAI[1] == 2 && projectile.localAI[0] == 0)
            {
                Main.player[projectile.owner].velocity = Vector2.Normalize(Main.player[projectile.owner].Center - projectile.Center) * -6f;
                projectile.knockBack = 25f;
            }
        }
    }
}