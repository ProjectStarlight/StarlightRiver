using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Items.UndergroundTemple
{
    class RuneStaff : ModItem
    {
        public override string Texture => AssetDirectory.CaveTempleItem + Name;

        int charge = 0;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Ancient Rune Staff");
            Tooltip.SetDefault("Hold to summon up to 3 explosive runes\nReleasing will fire the runes towards your mouse\ncontinuing to hold will detonate them for increased damage around you");
        }

        public override void SetDefaults()
        {
            item.magic = true;
            item.mana = 60;
            item.width = 32;
            item.height = 32;
            item.damage = 32;
            item.crit = 5;
            item.useStyle = ItemUseStyleID.HoldingOut;
            item.useTime = 30;
            item.useAnimation = 30;
            item.noUseGraphic = true;
            item.noMelee = true;
            item.knockBack = 2;
            item.rare = ItemRarityID.Green;
            item.channel = true;
        }

        public override void HoldItem(Player player)
        {
            if (player.channel)
            {
                if (charge % 30 == 0 && charge < 90)
                {
                    int index = charge / 30;
                    float rot = MathHelper.Pi / 3f * index - MathHelper.Pi / 3f;
                    int i = Projectile.NewProjectile(player.Center + Vector2.UnitY.RotatedBy(rot) * -45, Vector2.Zero, ProjectileType<RuneStaffProjectile>(), item.damage, item.knockBack, player.whoAmI, 0, charge);
                    Main.projectile[i].frame = index;

                    Main.PlaySound(SoundID.Item8, player.Center);
                }
                charge++;
            }

            else charge = 0;
        }
    }

    class RuneStaffProjectile : ModProjectile, IDrawAdditive
    {
        public override string Texture => AssetDirectory.CaveTempleItem + Name;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            projectile.width = 16;
            projectile.height = 16;
            projectile.timeLeft = 600;
            projectile.friendly = true;
        }

        public override bool CanDamage() => projectile.ai[0] == 1;

        public override void AI()
        {
            Player player = Main.player[projectile.owner];
            int time = 600 - projectile.timeLeft;

            //on release
            if (!Main.mouseLeft && projectile.ai[0] == 0 && time >= 20)
            {
                projectile.ai[0] = 1;
                projectile.ai[1] = 0;
                projectile.velocity = Vector2.Normalize(projectile.Center - Main.MouseWorld) * -18;
                projectile.tileCollide = true;
                projectile.netUpdate = true;

                Main.PlaySound(SoundID.Item82, player.Center);
            }

            projectile.ai[1]++;

            if (projectile.ai[0] == 1)
                for (int k = 0; k < 2; k++)
                    Dust.NewDust(projectile.position + Vector2.One * 4, 8, 8, DustType<Content.Dusts.Stamina>(), 0, 0, 0, default, 0.6f);
            else
            {
                projectile.position += player.velocity;

                //blow the runes if overcharged
                if (projectile.ai[1] >= 120)
                    projectile.timeLeft = 0;
            }
        }

        public override void Kill(int timeLeft)
        {
            Projectile.NewProjectile(projectile.Center, Vector2.Zero, ProjectileType<RuneStaffExplosion>(), projectile.ai[0] == 0 ? 120 : 20, 2, projectile.owner);
            Main.PlaySound(SoundID.DD2_ExplosiveTrapExplode);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            int time = 600 - projectile.timeLeft;

            Texture2D tex = GetTexture(Texture);
            float colorOff = time < 20 ? time / 20f : 1;
            Color color = new Color(255, colorOff, 1 - colorOff);
            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, new Rectangle(0, projectile.frame * 32, 32, 32), color, 0, Vector2.One * 16, colorOff / 2, 0, 0);

            return false;
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            int time = 600 - projectile.timeLeft;
            float colorOff = time < 20 ? time / 20f : 1;
            Texture2D tex = GetTexture("StarlightRiver/Assets/Keys/Glow");

            if (projectile.ai[0] == 1)
            {
                if (projectile.ai[1] < 15)
                {
                    Color color = new Color(255, 230, 100) * (1 - projectile.ai[1] / 15f);
                    for (int k = 0; k < 3; k++)
                        spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, color, 0, tex.Size() / 2, projectile.ai[1] / 7.5f + k / 12f, 0, 0);
                }

                for (int k = 0; k < projectile.oldPos.Length; k++)
                {
                    Color color = new Color(255, colorOff / 2, 1 - colorOff) * ((projectile.oldPos.Length - k) / (float)projectile.oldPos.Length) * 0.5f;
                    if (k <= 4) color *= 1.2f;
                    float scale = colorOff / 2 * (projectile.oldPos.Length - k) / projectile.oldPos.Length * 1.4f;

                    spriteBatch.Draw(tex, projectile.oldPos[k] + projectile.Size / 2 - Main.screenPosition, null, color, 0, tex.Size() / 2, scale, default, default);
                }
            }
            else
            {
                Color color = new Color(255, colorOff / 2, 1 - colorOff);
                spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, color * 0.6f, 0, tex.Size() / 2, colorOff / 2, 0, 0);
            }
        }
    }

    public class RuneStaffExplosion : ModProjectile
    {
        public override string Texture => AssetDirectory.Invisible;

        public override void SetDefaults()
        {
            projectile.width = 128;
            projectile.height = 128;
            projectile.timeLeft = 2;
            projectile.penetrate = -1;
            projectile.friendly = true;
        }

        public override void Kill(int timeLeft)
        {
            for (int k = 0; k < 100; k++)
                Dust.NewDustPerfect(projectile.Center, DustType<Content.Dusts.Stamina>(), Vector2.One.RotatedByRandom(6.28f) * 5);
        }
    }
}