using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using static Terraria.ModLoader.ModContent;


namespace StarlightRiver.Content.Items.Balanced
{
	public class IvorySword : ModItem
    {
        public override string Texture => AssetDirectory.BalancedItem + Name;

        private int combostate = 0;

        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 44;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 16;
            Item.useTime = 16;
            Item.knockBack = 2f;
            Item.damage = 46;
            Item.shoot = ProjectileType<IvorySwordProjectile>();
            Item.rare = ItemRarityID.LightRed;
            Item.autoReuse = false;
            Item.noMelee = true;
            Item.DamageType = DamageClass.Melee;
            Item.noUseGraphic = true;
        }

        public override bool Shoot(Player Player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 aim = Vector2.Normalize(Main.MouseWorld - Player.Center);
            int proj = Projectile.NewProjectile(source, Player.Center + aim * 18, aim * 0.1f, type, damage, knockback, Player.whoAmI);
            Main.projectile[proj].localAI[1] = combostate;

            if (combostate < 2) Terraria.Audio.SoundEngine.PlaySound(SoundID.Item65, Player.Center); else Terraria.Audio.SoundEngine.PlaySound(SoundID.Item64, Player.Center);
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
            Projectile.friendly = true;
            Projectile.width = 200;
            Projectile.height = 189;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 14;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("White Slash");
            Main.projFrames[Projectile.type] = 21;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            if (Projectile.localAI[1] == 2) Main.player[Projectile.owner].velocity *= 0.3f;
        }

        public override void AI()
        {
            Projectile.position += Main.player[Projectile.owner].velocity;
            Projectile.rotation = Projectile.velocity.ToRotation();

            if (Projectile.localAI[0] == 1) Projectile.damage = 0;

            if (Projectile.timeLeft <= 8) Projectile.localAI[0] = 1;

            if (Projectile.timeLeft == 14) Projectile.frame = 7 * (int)Projectile.localAI[1];

            if (++Projectile.frameCounter >= 2)
            {
                Projectile.frameCounter = 0;
                if (++Projectile.frame >= 7 + 7 * (int)Projectile.localAI[1])
                    Projectile.frame = 7 * (int)Projectile.localAI[1];
            }

            if (Projectile.localAI[1] == 2 && Projectile.localAI[0] == 0)
            {
                Main.player[Projectile.owner].velocity = Vector2.Normalize(Main.player[Projectile.owner].Center - Projectile.Center) * -6f;
                Projectile.knockBack = 25f;
            }
        }
    }
}