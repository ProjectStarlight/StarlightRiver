using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using StarlightRiver.Core;
using static Terraria.ModLoader.ModContent;
using StarlightRiver.Helpers;

namespace StarlightRiver.Items.Overgrow
{
    public class ExecutionersAxe : ModItem
    {
        public override string Texture => AssetDirectory.OvergrowItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Executioner's Axe");
        }

        public override void SetDefaults()
        {
            item.channel = true;
            item.damage = 30;
            item.width = 60;
            item.height = 60;
            item.useTime = 320;
            item.useAnimation = 320;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.melee = true;
            item.noMelee = true;
            item.knockBack = 12;
            item.useTurn = false;
            item.value = Item.sellPrice(0, 1, 42, 0);
            item.rare = ItemRarityID.Orange;
            item.autoReuse = false;
            item.shoot = ModContent.ProjectileType<AxeHead>();
            item.shootSpeed = 6f;
            item.noUseGraphic = true;
        }
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            return base.Shoot(player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack);
        }
        public override bool CanUseItem(Player player)
        {
            return base.CanUseItem(player);
        }
        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-10, 0);
        }
    }


    public class AxeHead : ModProjectile
    {
         public override string Texture => AssetDirectory.OvergrowItem + Name;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Executioner's Axe");
        }

        public override void SetDefaults()
        {
            projectile.hostile = false;
            projectile.magic = true;
            projectile.width = 48;
            projectile.height = 48;
            projectile.aiStyle = -1;
            projectile.friendly = false;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.alpha = 255;
        }

        bool released = false;
        readonly int chargeTime = 60;
        float angularMomentum = 1;
        double radians = 0;
        int lingerTimer = 0;

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Main.spriteBatch.Draw(GetTexture(AssetDirectory.OvergrowItem + "AxeHead"), ((Main.player[projectile.owner].Center - Main.screenPosition) + new Vector2(0, Main.player[projectile.owner].gfxOffY)).PointAccur(), null, lightColor, (float)radians + 3.9f, new Vector2(0, 84), projectile.scale, SpriteEffects.None, 0f);
            return false;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (projectile.ai[0] >= 60)
            {
                Texture2D tex = GetTexture("StarlightRiver/Assets/Tiles/Interactive/WispSwitchGlow2");
                Vector2 pos = (projectile.Center - Main.screenPosition) + new Vector2(0, Main.player[projectile.owner].gfxOffY);
                spriteBatch.Draw(tex, pos, tex.Frame(), Color.LightYellow * (6.28f - StarlightWorld.rottime) * 0.2f, 0, tex.Size() / 2, StarlightWorld.rottime * 0.17f, 0, 0);
                spriteBatch.Draw(tex, pos, tex.Frame(), Color.LightYellow * (6.28f - (StarlightWorld.rottime + 3.14f)) * 0.2f, 0, tex.Size() / 2, (StarlightWorld.rottime + 3.14f) * 0.17f, 0, 0);
                spriteBatch.Draw(tex, pos, tex.Frame(), Color.LightYellow * (6.28f - (StarlightWorld.rottime - 3.14f)) * 0.2f, 0, tex.Size() / 2, (StarlightWorld.rottime - 3.14f) * 0.17f, 0, 0);
            }
        }

        private void Smash(Vector2 position)
        {
            Player player = Main.player[projectile.owner];
            player.GetModPlayer<StarlightPlayer>().Shake += (int)(projectile.ai[0] * 0.2f);
            for (int k = 0; k <= 100; k++)
            {
                Dust.NewDustPerfect(projectile.oldPosition + new Vector2(projectile.width / 2, projectile.height / 2), DustType<Content.Dusts.Stone>(), new Vector2(0, 1).RotatedByRandom(1) * Main.rand.NextFloat(-1, 1) * projectile.ai[0] / 10f);
            }
            Projectile.NewProjectile(projectile.Center.X, projectile.Center.Y - 32, 0, 0, ModContent.ProjectileType<AxeFire>(), projectile.damage / 3, projectile.knockBack / 2, projectile.owner, 15, player.direction);
        }

        public override bool PreAI()
        {
            projectile.scale = projectile.ai[0] < 10 ? (projectile.ai[0] / 10f) : 1;
            Player player = Main.player[projectile.owner];
            int degrees = (int)(((player.itemAnimation) * -0.7) + 55) * player.direction;
            if (player.direction == 1)
            {
                degrees += 180;
            }
            radians = degrees * (Math.PI / 180);
            if (player.channel && !released)
            {
                if (projectile.ai[0] == 0)
                {
                    player.itemTime = 180;
                    player.itemAnimation = 180;
                }
                if (projectile.ai[0] < chargeTime)
                {
                    projectile.ai[0]++;
                    float rot = Main.rand.NextFloat(6.28f);
                    Dust.NewDustPerfect(projectile.Center + Vector2.One.RotatedBy(rot) * 35, DustType<Content.Dusts.GoldWithMovement>(), -Vector2.One.RotatedBy(rot) * 1.5f, 0, default, projectile.ai[0] / 100f);
                    if (projectile.ai[0] < chargeTime / 1.5f || projectile.ai[0] % 2 == 0)
                        angularMomentum = -1;
                    else
                        angularMomentum = 0;
                }
                else
                {
                    if (projectile.ai[0] == chargeTime)
                    {
                        for (int k = 0; k <= 100; k++)
                            Dust.NewDustPerfect(projectile.Center, DustType<Content.Dusts.GoldWithMovement>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(2), 0, default, 1.5f);
                        Main.PlaySound(SoundID.NPCDeath7, projectile.Center);
                        projectile.ai[0]++;
                    }
                    Dust.NewDustPerfect(projectile.Center, DustType<Content.Dusts.GoldWithMovement>(), Vector2.One.RotatedByRandom(6.28f));
                    angularMomentum = 0;
                }
                projectile.damage = 20 + (int)projectile.ai[0];
            }
            else
            {
                projectile.scale = 1;
                if (angularMomentum < 10)
                {
                    angularMomentum += 1.2f;
                }
                if (!released)
                {
                    released = true;
                    projectile.friendly = true;
                }
                if (projectile.ai[0] > chargeTime)
                {
                    Dust.NewDustPerfect(projectile.Center, DustType<Content.Dusts.GoldWithMovement>(), Vector2.One.RotatedByRandom(6.28f));
                }
            }

            projectile.position.Y = player.Center.Y - (int)(Math.Sin(radians * 0.96) * 86) - (projectile.height / 2);
            projectile.position.X = player.Center.X - (int)(Math.Cos(radians * 0.96) * 86) - (projectile.width / 2);
            if (lingerTimer == 0)
            {
                player.itemTime++;
                player.itemAnimation++;
                if (player.itemTime > angularMomentum + 1)
                {
                    player.itemTime -= (int)angularMomentum;
                    player.itemAnimation -= (int)angularMomentum;
                }
                else
                {
                    player.itemTime = 2;
                    player.itemAnimation = 2;
                }
                if (player.itemTime == 2 || (Main.tile[(int)projectile.Center.X / 16, (int)((projectile.Center.Y + 24) / 16)].collisionType == 1 && released))
                {
                    lingerTimer = 30;
                    if (projectile.ai[0] >= chargeTime)
                    {
                        this.Smash(projectile.Center);

                    }
                    projectile.damage = (int)projectile.damage / 3;
                    Main.PlaySound(SoundID.Item70, projectile.Center);
                    Main.PlaySound(SoundID.NPCHit42, projectile.Center);
                }
            }
            else
            {
                lingerTimer--;
                if (lingerTimer == 1)
                {
                    projectile.active = false;
                    player.itemTime = 2;
                    player.itemAnimation = 2;
                }
                player.itemTime++;
                player.itemAnimation++;
            }
            return true;
        }
    }


    public class AxeFire : ModProjectile
    {
        public override string Texture => AssetDirectory.OvergrowItem + Name;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Axe Fire");
        }

        public override void SetDefaults()
        {
            projectile.hostile = false;
            projectile.width = 24;
            projectile.height = 24;
            projectile.aiStyle = -1;
            projectile.friendly = true;
            projectile.damage = 1;
            projectile.penetrate = -1;
            projectile.alpha = 255;
            projectile.timeLeft = 3;
            projectile.tileCollide = true;
            projectile.extraUpdates = 1;
            projectile.ignoreWater = true;
        }

        //projectile.ai[0]: how many more pillars. Each one is one less
        //projectile.ai[1]: 0: center, -1: going left, 1: going right
        bool activated = false;
        float startposY = 0;
        public override bool PreAI()
        {
            if (startposY == 0)
            {
                startposY = projectile.position.Y;
                if (Main.tile[(int)projectile.Center.X / 16, (int)(projectile.Center.Y / 16)].collisionType == 1)
                {
                    projectile.active = false;
                }
            }
            projectile.velocity.X = 0;
            if (!activated)
            {
                projectile.velocity.Y = 24;
            }
            else
            {
                projectile.velocity.Y = -6;
                for (int i = 0; i < 5; i++)
                {
                    int dust = Dust.NewDust(projectile.Center, projectile.width, projectile.height * 2, DustType<Content.Dusts.GoldWithMovement>());
                    Main.dust[dust].velocity = Vector2.Zero;
                    Main.dust[dust].noGravity = true;
                }
                if (projectile.timeLeft == 5 && projectile.ai[0] > 0)
                {
                    if (projectile.ai[1] == -1 || projectile.ai[1] == 0)
                    {
                        Projectile.NewProjectile(projectile.Center.X - projectile.width, startposY, 0, 0, ModContent.ProjectileType<AxeFire>(), projectile.damage, projectile.knockBack, projectile.owner, projectile.ai[0] - 1, -1);
                    }
                    if (projectile.ai[1] == 1 || projectile.ai[1] == 0)
                    {
                        Projectile.NewProjectile(projectile.Center.X + projectile.width, startposY, 0, 0, ModContent.ProjectileType<AxeFire>(), projectile.damage, projectile.knockBack, projectile.owner, projectile.ai[0] - 1, 1);
                    }
                }
            }
            return false;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (oldVelocity.Y != projectile.velocity.Y && !activated)
            {
                startposY = projectile.position.Y;
                projectile.velocity.Y = -6;
                activated = true;
                projectile.timeLeft = 10;
            }
            return false;
        }
        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough)
        {
            fallThrough = false;
            return true;
        }

    }
}