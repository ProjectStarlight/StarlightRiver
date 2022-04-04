using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

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
            Item.channel = true;
            Item.damage = 30;
            Item.width = 60;
            Item.height = 60;
            Item.useTime = 320;
            Item.useAnimation = 320;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.DamageType = DamageClass.Melee;
            Item.noMelee = true;
            Item.knockBack = 12;
            Item.useTurn = false;
            Item.value = Item.sellPrice(0, 1, 42, 0);
            Item.rare = ItemRarityID.Orange;
            Item.autoReuse = false;
            Item.shoot = ModContent.ProjectileType<AxeHead>();
            Item.shootSpeed = 6f;
            Item.noUseGraphic = true;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            return base.Shoot(Player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack);
        }
        public override bool CanUseItem(Player Player)
        {
            return base.CanUseItem(Player);
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
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.width = 48;
            Projectile.height = 48;
            Projectile.aiStyle = -1;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.alpha = 255;
        }

        bool released = false;
        readonly int chargeTime = 60;
        float angularMomentum = 1;
        double radians = 0;
        int lingerTimer = 0;

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Main.spriteBatch.Draw(Request<Texture2D>(AssetDirectory.OvergrowItem + "AxeHead").Value, ((Main.player[Projectile.owner].Center - Main.screenPosition) + new Vector2(0, Main.player[Projectile.owner].gfxOffY)).PointAccur(), null, lightColor, (float)radians + 3.9f, new Vector2(0, 84), Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (Projectile.ai[0] >= 60)
            {
                Texture2D tex = Request<Texture2D>("StarlightRiver/Assets/Tiles/Interactive/WispSwitchGlow2").Value;
                Vector2 pos = (Projectile.Center - Main.screenPosition) + new Vector2(0, Main.player[Projectile.owner].gfxOffY);
                spriteBatch.Draw(tex, pos, tex.Frame(), Color.LightYellow * (6.28f - StarlightWorld.rottime) * 0.2f, 0, tex.Size() / 2, StarlightWorld.rottime * 0.17f, 0, 0);
                spriteBatch.Draw(tex, pos, tex.Frame(), Color.LightYellow * (6.28f - (StarlightWorld.rottime + 3.14f)) * 0.2f, 0, tex.Size() / 2, (StarlightWorld.rottime + 3.14f) * 0.17f, 0, 0);
                spriteBatch.Draw(tex, pos, tex.Frame(), Color.LightYellow * (6.28f - (StarlightWorld.rottime - 3.14f)) * 0.2f, 0, tex.Size() / 2, (StarlightWorld.rottime - 3.14f) * 0.17f, 0, 0);
            }
        }

        private void Smash(Vector2 position)
        {
            Player Player = Main.player[Projectile.owner];
            Player.GetModPlayer<StarlightPlayer>().Shake += (int)(Projectile.ai[0] * 0.2f);
            for (int k = 0; k <= 100; k++)
            {
                Dust.NewDustPerfect(Projectile.oldPosition + new Vector2(Projectile.width / 2, Projectile.height / 2), DustType<Content.Dusts.Stone>(), new Vector2(0, 1).RotatedByRandom(1) * Main.rand.NextFloat(-1, 1) * Projectile.ai[0] / 10f);
            }
            Projectile.NewProjectile(Projectile.Center.X, Projectile.Center.Y - 32, 0, 0, ModContent.ProjectileType<AxeFire>(), Projectile.damage / 3, Projectile.knockBack / 2, Projectile.owner, 15, Player.direction);
        }

        public override bool PreAI()
        {
            Projectile.scale = Projectile.ai[0] < 10 ? (Projectile.ai[0] / 10f) : 1;
            Player Player = Main.player[Projectile.owner];
            int degrees = (int)(((Player.ItemAnimation) * -0.7) + 55) * Player.direction;
            if (Player.direction == 1)
            {
                degrees += 180;
            }
            radians = degrees * (Math.PI / 180);
            if (Player.channel && !released)
            {
                if (Projectile.ai[0] == 0)
                {
                    Player.ItemTime = 180;
                    Player.ItemAnimation = 180;
                }
                if (Projectile.ai[0] < chargeTime)
                {
                    Projectile.ai[0]++;
                    float rot = Main.rand.NextFloat(6.28f);
                    Dust.NewDustPerfect(Projectile.Center + Vector2.One.RotatedBy(rot) * 35, DustType<Content.Dusts.GoldWithMovement>(), -Vector2.One.RotatedBy(rot) * 1.5f, 0, default, Projectile.ai[0] / 100f);
                    if (Projectile.ai[0] < chargeTime / 1.5f || Projectile.ai[0] % 2 == 0)
                        angularMomentum = -1;
                    else
                        angularMomentum = 0;
                }
                else
                {
                    if (Projectile.ai[0] == chargeTime)
                    {
                        for (int k = 0; k <= 100; k++)
                            Dust.NewDustPerfect(Projectile.Center, DustType<Content.Dusts.GoldWithMovement>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(2), 0, default, 1.5f);
                        Terraria.Audio.SoundEngine.PlaySound(SoundID.NPCDeath7, Projectile.Center);
                        Projectile.ai[0]++;
                    }
                    Dust.NewDustPerfect(Projectile.Center, DustType<Content.Dusts.GoldWithMovement>(), Vector2.One.RotatedByRandom(6.28f));
                    angularMomentum = 0;
                }
                Projectile.damage = 20 + (int)Projectile.ai[0];
            }
            else
            {
                Projectile.scale = 1;
                if (angularMomentum < 10)
                {
                    angularMomentum += 1.2f;
                }
                if (!released)
                {
                    released = true;
                    Projectile.friendly = true;
                }
                if (Projectile.ai[0] > chargeTime)
                {
                    Dust.NewDustPerfect(Projectile.Center, DustType<Content.Dusts.GoldWithMovement>(), Vector2.One.RotatedByRandom(6.28f));
                }
            }

            Projectile.position.Y = Player.Center.Y - (int)(Math.Sin(radians * 0.96) * 86) - (Projectile.height / 2);
            Projectile.position.X = Player.Center.X - (int)(Math.Cos(radians * 0.96) * 86) - (Projectile.width / 2);
            if (lingerTimer == 0)
            {
                Player.ItemTime++;
                Player.ItemAnimation++;
                if (Player.ItemTime > angularMomentum + 1)
                {
                    Player.ItemTime -= (int)angularMomentum;
                    Player.ItemAnimation -= (int)angularMomentum;
                }
                else
                {
                    Player.ItemTime = 2;
                    Player.ItemAnimation = 2;
                }
                if (Player.ItemTime == 2 || (Main.tile[(int)Projectile.Center.X / 16, (int)((Projectile.Center.Y + 24) / 16)].collisionType == 1 && released))
                {
                    lingerTimer = 30;
                    if (Projectile.ai[0] >= chargeTime)
                    {
                        this.Smash(Projectile.Center);

                    }
                    Projectile.damage = (int)Projectile.damage / 3;
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Item70, Projectile.Center);
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.NPCHit42, Projectile.Center);
                }
            }
            else
            {
                lingerTimer--;
                if (lingerTimer == 1)
                {
                    Projectile.active = false;
                    Player.ItemTime = 2;
                    Player.ItemAnimation = 2;
                }
                Player.ItemTime++;
                Player.ItemAnimation++;
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
            Projectile.hostile = false;
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.damage = 1;
            Projectile.penetrate = -1;
            Projectile.alpha = 255;
            Projectile.timeLeft = 3;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
            Projectile.ignoreWater = true;
        }

        //Projectile.ai[0]: how many more pillars. Each one is one less
        //Projectile.ai[1]: 0: center, -1: going left, 1: going right
        bool activated = false;
        float startposY = 0;
        public override bool PreAI()
        {
            if (startposY == 0)
            {
                startposY = Projectile.position.Y;
                if (Main.tile[(int)Projectile.Center.X / 16, (int)(Projectile.Center.Y / 16)].collisionType == 1)
                {
                    Projectile.active = false;
                }
            }
            Projectile.velocity.X = 0;
            if (!activated)
            {
                Projectile.velocity.Y = 24;
            }
            else
            {
                Projectile.velocity.Y = -6;
                for (int i = 0; i < 5; i++)
                {
                    int dust = Dust.NewDust(Projectile.Center, Projectile.width, Projectile.height * 2, DustType<Content.Dusts.GoldWithMovement>());
                    Main.dust[dust].velocity = Vector2.Zero;
                    Main.dust[dust].noGravity = true;
                }
                if (Projectile.timeLeft == 5 && Projectile.ai[0] > 0)
                {
                    if (Projectile.ai[1] == -1 || Projectile.ai[1] == 0)
                    {
                        Projectile.NewProjectile(Projectile.Center.X - Projectile.width, startposY, 0, 0, ModContent.ProjectileType<AxeFire>(), Projectile.damage, Projectile.knockBack, Projectile.owner, Projectile.ai[0] - 1, -1);
                    }
                    if (Projectile.ai[1] == 1 || Projectile.ai[1] == 0)
                    {
                        Projectile.NewProjectile(Projectile.Center.X + Projectile.width, startposY, 0, 0, ModContent.ProjectileType<AxeFire>(), Projectile.damage, Projectile.knockBack, Projectile.owner, Projectile.ai[0] - 1, 1);
                    }
                }
            }
            return false;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (oldVelocity.Y != Projectile.velocity.Y && !activated)
            {
                startposY = Projectile.position.Y;
                Projectile.velocity.Y = -6;
                activated = true;
                Projectile.timeLeft = 10;
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