using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Items.Misc
{
    class TwistSword : ModItem, IGlowingItem
    {
        private int charge = 240;

        public override void SetStaticDefaults() => DisplayName.SetDefault("Twisted Greatsword");

        public override void SetDefaults()
        {
            item.damage = 18;
            item.melee = true;
            item.width = 40;
            item.height = 20;
            item.useTime = 10;
            item.useAnimation = 10;
            item.useStyle = ItemUseStyleID.HoldingUp;
            item.noMelee = true;
            item.knockBack = 2;
            item.rare = ItemRarityID.Orange;
            item.channel = true;
        }

        public override bool CanUseItem(Player player) => charge > 40;

        public override bool UseItem(Player player)
        {
            Projectile.NewProjectile(player.Center, Vector2.Zero, ProjectileType<TwistSwordSlash>(), item.damage, item.knockBack, player.whoAmI);
            return true;
        }

        public override void HoldItem(Player player)
        {
            if (player.channel)
            {
                if (player.velocity.Y > 1.5f) player.velocity.Y = 1.5f;
                if (player.velocity.X < 5 && player.controlRight) player.velocity.X += 0.2f;
                if (player.velocity.X > -5 && player.controlLeft) player.velocity.X -= 0.2f;
                charge--;
            }

            if (charge <= 0) player.channel = false;
        }

        public override void UpdateInventory(Player player)
        {
            if (charge < 240 && player.velocity.Y == 0) charge++;
        }

        public void DrawGlowmask(PlayerDrawInfo info)
        {
            if (charge < 240)
            {
                Point pos = (info.drawPlayer.position + new Vector2(0, -20) - Main.screenPosition).ToPoint();
                Rectangle target = new Rectangle(pos.X, pos.Y, (int)(charge / 240f * 24f), 2);
                Vector3 color = Vector3.Lerp(Color.Red.ToVector3(), Color.LimeGreen.ToVector3(), charge / 240f);
                Main.playerDrawData.Add(new Terraria.DataStructures.DrawData(Main.magicPixel, target, new Color(color.X, color.Y, color.Z)));
            }
        }
    }

    class TwistSwordSlash : ModProjectile
    {
        public override void SetStaticDefaults() => Main.projFrames[projectile.type] = 10;

        public override void SetDefaults()
        {
            projectile.width = 160;
            projectile.height = 50;
            projectile.friendly = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.timeLeft = 2;
            projectile.extraUpdates = 3;
        }

        public override void AI()
        {
            projectile.ai[0]++;

            Player player = Main.player[projectile.owner];
            projectile.Center = player.Center + new Vector2(0, player.gfxOffY);
            if (player.channel) projectile.timeLeft = 2;

            //visuals
            float rot = (projectile.ai[0] % 80) / 80f * 6.28f;
            float x = (float)Math.Cos(-rot) * 80;
            float y = (float)Math.Sin(-rot) * 20;
            Vector2 off = new Vector2(x, y);
            Dust.NewDustPerfect(player.Center + off, DustType<Dusts.BlueStamina>(), off * 0.01f, 0, default, 2f);
            Dust.NewDustPerfect(player.Center + off, DustType<Dusts.BlueStamina>(), off * Main.rand.NextFloat(0.01f, 0.04f));

            Lighting.AddLight(projectile.Center + off, new Vector3(0.1f, 0.25f, 0.6f));

            if (projectile.ai[0] % 40 == 0)
            {
                player.direction *= -1;
                Main.PlaySound(SoundID.Item1);
            }

            if (++projectile.frameCounter >= 8)
            {
                projectile.frameCounter = 0;
                if (++projectile.frame >= 10) projectile.frame = 0;
            }
        }
    }
}
