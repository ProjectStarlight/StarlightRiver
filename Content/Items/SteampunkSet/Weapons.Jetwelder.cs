using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StarlightRiver.Core;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.UI.Chat;

using System;
using System.Collections.Generic;

namespace StarlightRiver.Content.Items.SteampunkSet
{
    public class Jetwelder : ModItem
    {
        public override string Texture => AssetDirectory.SteampunkItem + Name;

        private bool clickingRight = false;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Jetwelder");
            Tooltip.SetDefault("Collect scrap from damaging enemies \nRight click to use scrap to summon robots");
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
            item.value = Item.sellPrice(0, 1, 0, 0);
            item.rare = 3;
            item.UseSound = SoundID.Item44;
            item.channel = true;
            item.noMelee = true;
            item.summon = true;
            item.shoot = ModContent.ProjectileType<JetwelderFlame>();
            item.autoReuse = false;
        }

        public override bool CanUseItem(Player player)
        {
            if (clickingRight && player.altFunctionUse == 2)
                return false;
            return base.CanUseItem(player);
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            JetwelderPlayer modPlayer = player.GetModPlayer<JetwelderPlayer>();
            if (player.altFunctionUse == 2)
            {
                clickingRight = true;
                type = ModContent.ProjectileType<JetwelderSelector>();
            }
            return true;
        }

        public override void UpdateInventory(Player player)
        {
            if (!Main.mouseRight)
                clickingRight = false;
        }

        public override void HoldItem(Player player)
        {
            if (!Main.mouseRight)
                clickingRight = false;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(0, 0);
        }

        public override bool AltFunctionUse(Player player) => true;
    }

    public class JetwelderSelector : ModProjectile
    {
        public override string Texture => AssetDirectory.SteampunkItem + "JetwelderSelector";

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

        public override bool Autoload(ref string name)
        {
            StarlightRiver.Instance.AddGore(Texture + "_Gore1");
            StarlightRiver.Instance.AddGore(Texture + "_Gore2");
            StarlightRiver.Instance.AddGore(Texture + "_Gore3");
            return base.Autoload(ref name);
        }

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

                if (rotation >= PiOverFour * -3 && rotation < PiOverFour * -1 && modPlayer.scrap >= 5)
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

                if ((rotation >= PiOverFour * 3 || rotation < PiOverFour * -3) && modPlayer.scrap >= 20)
                {
                    if (finalScale < 1)
                        finalScale += 0.1f;
                    projType = ModContent.ProjectileType<JetwelderFinal>();
                }
                else if (finalScale > 0)
                    finalScale -= 0.1f;

            }
            else
            {
                projectile.active = false;

                if (projType == ModContent.ProjectileType<JetwelderCrawler>() && modPlayer.scrap >= 5)
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
                if (projType == ModContent.ProjectileType<JetwelderFinal>() && modPlayer.scrap >= 15)
                {
                    modPlayer.scrap -= 15;
                }

                // modPlayer.scrap = 20;
                //Main.NewText(modPlayer.scrap.ToString(), Color.Orange);
                Vector2 position = player.Center;
                if (projType == ModContent.ProjectileType<JetwelderCrawler>() || projType == ModContent.ProjectileType<JetwelderJumper>())
                    position = FindFirstTile(player.Center, projType);
                if (projType == ModContent.ProjectileType<JetwelderGatler>())
                    position.Y -= 10;

                if (projType != -1)
                {
                    int j;
                    for (j = 0; j < 18; j++)
                    {
                        Vector2 direction = Main.rand.NextFloat(6.28f).ToRotationVector2();
                        Dust.NewDustPerfect((position + (direction * 6)) + new Vector2(0, 35), ModContent.DustType<Dusts.BuzzSpark>(), direction.RotatedBy(Main.rand.NextFloat(-0.2f, 0.2f) - 1.57f) * Main.rand.Next(2, 10), 0, new Color(255, 255, 60) * 0.8f, 1.6f);
                    }
                    for (j = 0; j < 3; j++)
                    {
                        for (int k = 1; k < 4; k++)
                        {
                            Gore.NewGore(position + Main.rand.NextVector2Circular(15, 15), Main.rand.NextVector2Circular(5, 5), ModGore.GetGoreSlot(Texture + "_Gore" + k.ToString()), 1f);
                        }
                    }
                    Projectile.NewProjectile(position, Vector2.Zero, projType, projectile.damage, projectile.knockBack, player.whoAmI);
                }
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
            return position - new Vector2(0, proj.height);
        }

        private void DrawRobot(SpriteBatch spriteBatch, Texture2D regTex, Texture2D grayTex, float angle, int minScrap, float growCounter)
        {
            bool gray = player.GetModPlayer<JetwelderPlayer>().scrap < minScrap;

            Vector2 dir = angle.ToRotationVector2() * 80;
            Vector2 pos = (player.Center + dir) - Main.screenPosition;
            Texture2D tex = gray ? grayTex : regTex;

            float lerper = MathHelper.Clamp(EaseFunction.EaseCubicOut.Ease(growCounter), 0, 1);
            float colorMult = MathHelper.Lerp(0.66f, 1f, lerper);
            float scale = MathHelper.Lerp(0.75f, 1.33f, lerper);
            scale *= EaseFunction.EaseCubicOut.Ease(scaleCounter);
            spriteBatch.Draw(tex, pos, null, Color.White * colorMult, 0, tex.Size() / 2, scale, SpriteEffects.None, 0f);

            Texture2D barTex = ModContent.GetTexture(Texture + "_Bar");
            Vector2 barPos = pos - new Vector2(0, (30 * scale) + 10); 
            int numScrapFive = minScrap / 5;
            Rectangle frame = new Rectangle(
                0,
                gray ? barTex.Height / 2 : 0,
                barTex.Width,
                barTex.Height / 2);

            for (int i = (int)barPos.X - (int)(barTex.Width * 0.5f * numScrapFive); i < (int)barPos.X + (int)(barTex.Width * 0.5f * numScrapFive); i+= barTex.Width)
            {
                spriteBatch.Draw(barTex, new Vector2(i + (barTex.Width / 2), barPos.Y), frame, Color.White * (gray ? 0.33f : 1), 0f, barTex.Size() / new Vector2(2,4), EaseFunction.EaseCubicOut.Ease(scaleCounter), SpriteEffects.None, 0f);
            }
            /*Vector2 origin = new Vector2(6, 13);
            if (minScrap > 9)
                origin.X = 8;
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontItemStack, minScrap.ToString(), pos - new Vector2(0, 40 * scale), player.GetModPlayer<JetwelderPlayer>().scrap >= minScrap ? Color.White : Color.Red, 0f, origin, Vector2.One * EaseFunction.EaseCubicOut.Ease(scaleCounter));*/
        }

    }
    public class JetwelderFlame : ModProjectile
    {
        public override string Texture => AssetDirectory.SteampunkItem + Name;

        private Player player => Main.player[projectile.owner];

        private Vector2 direction = Vector2.Zero;

        private float scaleCounter = 0f;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Jetwelder");
            Main.projFrames[projectile.type] = 6;
        }
        public override void SetDefaults()
        {
            projectile.hostile = false;
            projectile.width = 114;
            projectile.height = 36;
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
            if (scaleCounter < 1)
                scaleCounter += 0.1f;
            projectile.scale = scaleCounter;

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
            projectile.Center = player.Center + new Vector2(0, player.gfxOffY) + new Vector2(30, -18 * player.direction).RotatedBy(projectile.rotation);

            for (int i = 0; i < projectile.width * projectile.scale; i++)
                Lighting.AddLight(projectile.Center + (direction * i), Color.LightBlue.ToVector3() * 0.6f);

            projectile.frameCounter++;
            if (projectile.frameCounter % 2 == 0)
                projectile.frame++;
            projectile.frame %= Main.projFrames[projectile.type];
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            if (Main.rand.NextBool(4))
                target.AddBuff(BuffID.OnFire, 150);
            knockback = 0;
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D tex = ModContent.GetTexture(Texture);

            int frameHeight = tex.Height / Main.projFrames[projectile.type];
            Rectangle frame = new Rectangle(0, frameHeight * projectile.frame, tex.Width, frameHeight);


            SpriteEffects effects = player.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;
            Texture2D bloomTex = ModContent.GetTexture(Texture + "_Glow");
            Color bloomColor = Color.White;
            bloomColor.A = 0;
            spriteBatch.Draw(bloomTex, projectile.Center - Main.screenPosition, frame, bloomColor, projectile.rotation, new Vector2(0, tex.Height / (2 * Main.projFrames[projectile.type])), projectile.scale, effects, 0f);

            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, frame, Color.White, projectile.rotation, new Vector2(0, tex.Height / (2 * Main.projFrames[projectile.type])), projectile.scale, effects, 0f);

            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float collisionPoint = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), projectile.Center, projectile.Center + (direction * projectile.width * projectile.scale), projectile.height * projectile.scale, ref collisionPoint);
        }

        public override bool? CanCutTiles()
        {
            return true;
        }
        public override void CutTiles()
        {
            DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
            Utils.PlotTileLine(projectile.Center, projectile.Center + (direction * projectile.width * projectile.scale), projectile.height * projectile.scale, DelegateMethods.CutTiles);
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

            //Main.NewText(player.GetModPlayer<JetwelderPlayer>().scrap.ToString());
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

        public override void ModifyDrawLayers(List<PlayerLayer> layers)
        {
            if (player.HeldItem.type == ModContent.ItemType<Jetwelder>())
            {
                layers.Insert(layers.FindIndex(x => x.Name == "MiscEffectsFront" && x.mod == "Terraria") + 1, new PlayerLayer(mod.Name, "JetwelderBar",
                       delegate (PlayerDrawInfo info)
                       {
                           DrawBar(info);
                       }));
            }
            base.ModifyDrawLayers(layers);
        }

        private static void DrawBar(PlayerDrawInfo info)
        {
            Player player = info.drawPlayer;
            Texture2D barTex = ModContent.GetTexture(AssetDirectory.SteampunkItem + "JetwelderBar");
            Texture2D glowTex = ModContent.GetTexture(AssetDirectory.SteampunkItem + "JetwelderBar_Glow");

            Vector2 drawPos = (player.MountedCenter - Main.screenPosition) - new Vector2(0, 40 - player.gfxOffY);

            DrawData value = new DrawData(
                        barTex,
                        new Vector2((int)drawPos.X, (int)drawPos.Y),
                        null,
                        Lighting.GetColor((int)(drawPos.X + Main.screenPosition.X) / 16, (int)(drawPos.Y + Main.screenPosition.Y) / 16),
                        0f,
                        barTex.Size() / 2,
                        1,
                        SpriteEffects.None,
                        0
                    );
            Main.playerDrawData.Add(value);

            DrawData value2 = new DrawData(
                        glowTex,
                        new Vector2((int)drawPos.X, (int)drawPos.Y) - new Vector2(0,1),
                        new Rectangle(0,0, (int)(glowTex.Width * (player.GetModPlayer<JetwelderPlayer>().scrap / 20f)), glowTex.Height),
                        Color.White,
                        0f,
                        glowTex.Size() / 2,
                        1,
                        SpriteEffects.None,
                        0
                    );
            Main.playerDrawData.Add(value2);
        }
    }

    internal class JetwelderCasingGore : ModGore
    {
        public override bool Update(Gore gore)
        {
            gore.alpha += 4;
            return base.Update(gore);
        }
    }
}