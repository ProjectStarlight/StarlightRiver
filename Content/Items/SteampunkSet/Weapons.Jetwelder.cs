using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using System;
using System.Collections.Generic;
using System.Linq;

namespace StarlightRiver.Content.Items.SteampunkSet
{
    public class Jetwelder : ModItem
    {
        public override string Texture => AssetDirectory.SteampunkItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Jetwelder");
            Tooltip.SetDefault("I shall update this later");
        }

        public override void SetDefaults()
        {
            item.damage = 20;
            item.knockBack = 3f;
            item.mana = 10;
            item.width = 32;
            item.height = 32;
            item.useTime = 36;
            item.useAnimation = 36;
            item.useStyle = ItemUseStyleID.HoldingOut;
            item.value = Item.buyPrice(0, 5, 0, 0);
            item.rare = ItemRarityID.Green;
            item.UseSound = SoundID.Item44;
            item.channel = true;
            item.noMelee = true;
            item.summon = true;
            item.shoot = ModContent.ProjectileType<JetwelderFlame>();
            item.autoReuse = false;
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            JetwelderPlayer modPlayer = player.GetModPlayer<JetwelderPlayer>();
            if (player.altFunctionUse == 2)
            {
                if (Main.mouseLeft)
                    return false;
                type = ModContent.ProjectileType<JetwelderSelector>();
            }
            return true;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(0, 0);
        }

        public override bool AltFunctionUse(Player player) => true;
    }

    public class JetwelderSelector : ModProjectile
    {
        public override string Texture => AssetDirectory.SteampunkItem + Name;

        private Player player => Main.player[projectile.owner];

        private Vector2 direction = Vector2.Zero;

        private float crawlerScale;
        private float jumperScale;
        private float gatlerScale;
        private float finalScale;

        private int projType = -1;

        private float rotation;

        private float scaleCounter = 0f;

        private static float PiOverFour = (float)Math.PI / 4f; //sick of casting

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Jetwelder");

        }
        public override void SetDefaults()
        {
            projectile.hostile = false;
            projectile.width = 2;
            projectile.height = 2;
            projectile.aiStyle = -1;
            projectile.friendly = false;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.timeLeft = 999999;
            projectile.ignoreWater = true;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            DrawRobot(spriteBatch,
                      ModContent.GetTexture(Texture + "_Crawler"),
                      ModContent.GetTexture(Texture + "_Crawler_Gray"),
                      -2 * PiOverFour,
                      5,
                      crawlerScale);
            DrawRobot(spriteBatch,
                      ModContent.GetTexture(Texture + "_Jumper"),
                      ModContent.GetTexture(Texture + "_Jumper_Gray"),
                      0 * PiOverFour,
                      10,
                      jumperScale);
            DrawRobot(spriteBatch,
                      ModContent.GetTexture(Texture + "_Gatler"),
                      ModContent.GetTexture(Texture + "_Gatler_Gray"),
                      2 * PiOverFour,
                      15,
                      gatlerScale);
            DrawRobot(spriteBatch,
                      ModContent.GetTexture(Texture + "_Final"),
                      ModContent.GetTexture(Texture + "_Final_Gray"),
                      4 * PiOverFour, //Yes I know this is PI but it's consistant this way
                      20,
                      finalScale);

            return false;
        }

        public override void AI()
        {
            JetwelderPlayer modPlayer = player.GetModPlayer<JetwelderPlayer>();
            projectile.velocity = Vector2.Zero;
            projectile.Center = player.Center;
            if (Main.mouseRight)
            {
                if (scaleCounter < 1)
                    scaleCounter += 0.05f;

                projectile.timeLeft = 2;

                direction = player.DirectionTo(Main.MouseWorld);
                direction.Normalize();

                player.ChangeDir(Math.Sign(direction.X));

                player.itemTime = player.itemAnimation = 2;
                player.itemRotation = direction.ToRotation();
                if (player.direction != 1)
                    player.itemRotation -= 3.14f;

                player.itemRotation = MathHelper.WrapAngle(player.itemRotation);

                rotation = MathHelper.WrapAngle(direction.ToRotation());

                if (Main.mouseLeft)
                    projectile.active = false;

                if (rotation >= PiOverFour * -3  && rotation < PiOverFour * -1 && modPlayer.scrap >= 5)
                {
                    if (crawlerScale < 1)
                        crawlerScale += 0.1f;
                    projType = ModContent.ProjectileType<JetwelderCrawler>(); 
                }
                else if (crawlerScale > 0)
                    crawlerScale -= 0.1f;

                if (rotation >= PiOverFour * -1 && rotation < PiOverFour * 1 && modPlayer.scrap >= 10)
                {
                    if (jumperScale < 1)
                        jumperScale += 0.1f;
                    projType = ModContent.ProjectileType<JetwelderJumper>();
                }
                else if (jumperScale > 0)
                    jumperScale -= 0.1f;

                if (rotation >= PiOverFour * 1 && rotation < PiOverFour * 3 && modPlayer.scrap >= 15)
                {
                    if (gatlerScale < 1)
                        gatlerScale += 0.1f;
                    projType = ModContent.ProjectileType<JetwelderGatler>();
                }
                else if (gatlerScale > 0)
                    gatlerScale -= 0.1f;

                if (rotation >= PiOverFour * 3 || rotation < PiOverFour * -3 && modPlayer.scrap >= 20)
                {
                    if (finalScale < 1)
                        finalScale += 0.1f;
                    projType = ModContent.ProjectileType<JetwelderGatler>();
                }
                else if (finalScale > 0)
                    finalScale -= 0.1f;

            }
            else
            {
                projectile.active = false;

                if (projType == ModContent.ProjectileType<JetwelderGatler>() && modPlayer.scrap >= 5)
                {
                    modPlayer.scrap -= 5;
                }
                if (projType == ModContent.ProjectileType<JetwelderJumper>() && modPlayer.scrap >= 10)
                {
                    modPlayer.scrap -= 10;
                }
                if (projType == ModContent.ProjectileType<JetwelderGatler>() && modPlayer.scrap >= 15)
                {
                    modPlayer.scrap -= 15;
                }
                /*if (projType == ModContent.ProjectileType<JetwelderFinal>() && modPlayer.scrap >= 15)
                {
                    modPlayer.scrap -= 15;
                }*/

                modPlayer.scrap = 20;
                Main.NewText(modPlayer.scrap.ToString(), Color.Orange);
                Vector2 position = player.Center;
                if (projType == ModContent.ProjectileType<JetwelderCrawler>() || projType == ModContent.ProjectileType<JetwelderJumper>())
                    position = FindFirstTile(player.Center, projType);

                if (projType != -1)
                    Projectile.NewProjectile(position, Vector2.Zero, projType, projectile.damage, projectile.knockBack, player.whoAmI);
            }
        }

        private static Vector2 FindFirstTile(Vector2 position, int type)
        {
            int tries = 0;
            while (tries < 99)
            {
                tries++;
                int posX = (int)position.X / 16;
                int posY = (int)position.Y / 16;
                if (Framing.GetTileSafely(posX, posY).active() && Main.tileSolid[Framing.GetTileSafely(posX, posY).type])
                {
                    break;
                }
                position += new Vector2(0, 16);
            }

            Projectile proj = new Projectile();
            proj.SetDefaults(type);
            return position - new Vector2(0, (proj.height / 2));
        }

        private void DrawRobot(SpriteBatch spriteBatch, Texture2D regTex, Texture2D grayTex, float angle, int minScrap, float growCounter)
        {
            Vector2 dir = angle.ToRotationVector2() * 60;
            Vector2 pos = (player.Center + dir) - Main.screenPosition;
            Texture2D tex = (player.GetModPlayer<JetwelderPlayer>().scrap >= minScrap) ? regTex : grayTex;

            float lerper = MathHelper.Clamp(EaseFunction.EaseCubicOut.Ease(growCounter), 0, 1);
            float colorMult = MathHelper.Lerp(0.66f, 1f, lerper);
            float scale = MathHelper.Lerp(0.75f, 1.33f, lerper);
            scale *= EaseFunction.EaseCubicOut.Ease(scaleCounter);
            spriteBatch.Draw(tex, pos, null, Color.White * colorMult, 0, tex.Size() / 2, scale, SpriteEffects.None, 0f);
        }

    }
    public class JetwelderFlame : ModProjectile
    {
        public override string Texture => AssetDirectory.SteampunkItem + Name;

        private Player player => Main.player[projectile.owner];

        private Vector2 direction = Vector2.Zero;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Jetwelder");
            Main.projFrames[projectile.type] = 1;

        }
        public override void SetDefaults()
        {
            projectile.hostile = false;
            projectile.width = 256;
            projectile.height = 64;
            projectile.aiStyle = -1;
            projectile.friendly = true;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.timeLeft = 999999;
            projectile.ignoreWater = true;
            projectile.ownerHitCheck = true;
        }

        public override void AI()
        {
            projectile.velocity = Vector2.Zero;
            if (player.channel)
            {
                projectile.timeLeft = 2;
                player.itemTime = player.itemAnimation = 2;

                direction = player.DirectionTo(Main.MouseWorld);
                direction.Normalize();
                player.ChangeDir(Math.Sign(direction.X));

                player.itemRotation = direction.ToRotation();

                if (player.direction != 1)
                    player.itemRotation -= 3.14f;

                player.itemRotation = MathHelper.WrapAngle(player.itemRotation);
            }
            projectile.rotation = direction.ToRotation();
            projectile.Center = player.Center + new Vector2(0, player.gfxOffY) +  new Vector2(30, -15 * player.direction).RotatedBy(projectile.rotation);

            for (int i = 0; i < projectile.width * projectile.scale; i++)
                Lighting.AddLight(projectile.Center + (direction * i), Color.Cyan.ToVector3() * 0.6f);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D tex = ModContent.GetTexture(Texture);
            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, Color.White, projectile.rotation, new Vector2(0, tex.Height / 2), projectile.scale, SpriteEffects.None, 0f);
            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float collisionPoint = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), projectile.Center, projectile.Center + (direction * projectile.width), projectile.height, ref collisionPoint); 
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            if (target.life <= 0 && Main.rand.NextBool(5))
                SpawnScrap(target.Center);
            else if (Main.rand.NextBool(30))
                SpawnScrap(target.Center);
        }

        private static void SpawnScrap(Vector2 position)
        {
            int ItemType = ModContent.ItemType<JetwelderScrap1>();
            switch (Main.rand.Next(4))
            {
                case 0:
                    ItemType = ModContent.ItemType<JetwelderScrap1>();
                    break;
                case 1:
                    ItemType = ModContent.ItemType<JetwelderScrap2>();
                    break;
                case 2:
                    ItemType = ModContent.ItemType<JetwelderScrap3>();
                    break;
                case 3:
                    ItemType = ModContent.ItemType<JetwelderScrap4>();
                    break;
            }
            Item.NewItem(position, ItemType);
        }
    }
    public abstract class JetwelderScrap : ModItem
    {

        public override string Texture => AssetDirectory.SteampunkItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Scrap");
            Tooltip.SetDefault("You shouldn't see this");
        }

        public override void SetDefaults()
        {
            SetSize();
            item.maxStack = 1;
        }

        public override bool ItemSpace(Player player) => true;
        public override bool OnPickup(Player player)
        {
            Main.PlaySound(SoundID.Grab, (int)player.position.X, (int)player.position.Y);
            if (player.GetModPlayer<JetwelderPlayer>().scrap < 20)
                player.GetModPlayer<JetwelderPlayer>().scrap++;

            Main.NewText(player.GetModPlayer<JetwelderPlayer>().scrap.ToString());
            return false;
        }

        protected virtual void SetSize() { }

        public override Color? GetAlpha(Color lightColor) => lightColor;
    }

    public class JetwelderScrap1 : JetwelderScrap
    {
        protected override void SetSize() => item.Size = new Vector2(32, 32);
    }
    public class JetwelderScrap2 : JetwelderScrap
    {
        protected override void SetSize() => item.Size = new Vector2(32, 32);
    }
    public class JetwelderScrap3 : JetwelderScrap
    {
        protected override void SetSize() => item.Size = new Vector2(32, 32);
    }
    public class JetwelderScrap4 : JetwelderScrap
    {
        protected override void SetSize() => item.Size = new Vector2(32, 32);
    }

    public class JetwelderPlayer : ModPlayer
    {
        public int scrap;
    }
}