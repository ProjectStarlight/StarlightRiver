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
            Item.damage = 20;
            Item.knockBack = 3f;
            Item.mana = 10;
            Item.width = 32;
            Item.height = 32;
            Item.useTime = 36;
            Item.useAnimation = 36;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.value = Item.sellPrice(0, 1, 0, 0);
            Item.rare = 3;
            Item.UseSound = SoundID.Item44;
            Item.channel = true;
            Item.noMelee = true;
            Item.DamageType = DamageClass.Magic;
            Item.shoot = ModContent.ProjectileType<JetwelderFlame>();
            Item.autoReuse = false;
        }

        public override bool CanUseItem(Player Player)
        {
            if (clickingRight && Player.altFunctionUse == 2)
                return false;
            return base.CanUseItem(Player);
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            if (player.altFunctionUse == 2)
                type = ModContent.ProjectileType<JetwelderSelector>();
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
                clickingRight = true;
            return true;
        }

        public override void UpdateInventory(Player Player)
        {
            if (!Main.mouseRight)
                clickingRight = false;
        }

        public override void HoldItem(Player Player)
        {
            if (!Main.mouseRight)
                clickingRight = false;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(0, 0);
        }

        public override bool AltFunctionUse(Player Player) => true;
    }

    public class JetwelderSelector : ModProjectile
    {
        public override string Texture => AssetDirectory.SteampunkItem + "JetwelderSelector";

        private Player Player => Main.player[Projectile.owner];

        private Vector2 direction = Vector2.Zero;

        private float crawlerScale;
        private float jumperScale;
        private float gatlerScale;
        private float finalScale;

        private int projType = -1;

        private float rotation;

        private float scaleCounter = 0f;

        private static float PiOverFour = (float)Math.PI / 4f; //sick of casting

        public override void Load()
        {
            GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, Texture + "_Gore1");
            GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, Texture + "_Gore2");
            GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, Texture + "_Gore3");
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Jetwelder");
        }
        public override void SetDefaults()
        {
            Projectile.hostile = false;
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 999999;
            Projectile.ignoreWater = true;
        }

        public override bool? CanHitNPC(NPC target) => false;

        public override bool PreDraw(ref Color lightColor)
        {
            DrawRobot(Main.spriteBatch,
                      ModContent.Request<Texture2D>(Texture + "_Crawler").Value,
                      ModContent.Request<Texture2D>(Texture + "_Crawler_Gray").Value,
                      -2 * PiOverFour,
                      5,
                      crawlerScale);
            DrawRobot(Main.spriteBatch,
                      ModContent.Request<Texture2D>(Texture + "_Jumper").Value,
                      ModContent.Request<Texture2D>(Texture + "_Jumper_Gray").Value,
                      0 * PiOverFour,
                      10,
                      jumperScale);
            DrawRobot(Main.spriteBatch,
                      ModContent.Request<Texture2D>(Texture + "_Gatler").Value,
                      ModContent.Request<Texture2D>(Texture + "_Gatler_Gray").Value,
                      2 * PiOverFour,
                      15,
                      gatlerScale);
            DrawRobot(Main.spriteBatch,
                      ModContent.Request<Texture2D>(Texture + "_Final").Value,
                      ModContent.Request<Texture2D>(Texture + "_Final_Gray").Value,
                      4 * PiOverFour, //Yes I know this is PI but it's consistant this way
                      20,
                      finalScale);

            return false;
        }

        public override void AI()
        {
            JetwelderPlayer modPlayer = Player.GetModPlayer<JetwelderPlayer>();
            Projectile.velocity = Vector2.Zero;
            Projectile.Center = Player.Center;
            if (Main.mouseRight)
            {
                if (scaleCounter < 1)
                    scaleCounter += 0.05f;

                Projectile.timeLeft = 2;

                direction = Player.DirectionTo(Main.MouseWorld);
                direction.Normalize();

                Player.ChangeDir(Math.Sign(direction.X));

                Player.itemTime = Player.itemAnimation = 2;
                Player.itemRotation = direction.ToRotation();
                if (Player.direction != 1)
                    Player.itemRotation -= 3.14f;

                Player.itemRotation = MathHelper.WrapAngle(Player.itemRotation);

                rotation = MathHelper.WrapAngle(direction.ToRotation());

                if (Main.mouseLeft)
                    Projectile.active = false;

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
                Projectile.active = false;

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
                if (projType == ModContent.ProjectileType<JetwelderFinal>() && modPlayer.scrap >= 20)
                {
                    modPlayer.scrap -= 20;
                }

                //modPlayer.scrap = 20;
                //Main.NewText(modPlayer.scrap.ToString(), Color.Orange);
                Vector2 position = Player.Center;
                if (projType == ModContent.ProjectileType<JetwelderCrawler>() || projType == ModContent.ProjectileType<JetwelderJumper>())
                    position = FindFirstTile(Player.Center, projType);
                if (projType == ModContent.ProjectileType<JetwelderGatler>())
                    position.Y -= 10;

                if (projType != -1)
                {
                    int j;

                    for (j = 0; j < 17; j++)
                    {
                        Vector2 direction = Main.rand.NextFloat(6.28f).ToRotationVector2();
                        Dust.NewDustPerfect((position + (direction * 6)) + new Vector2(0, 35), ModContent.DustType<Dusts.BuzzSpark>(), direction.RotatedBy(Main.rand.NextFloat(-0.2f, 0.2f) - 1.57f) * Main.rand.Next(2, 10), 0, new Color(255, 255, 60) * 0.8f, 1.6f); //PORTTODO: Uncomment when dust shaders don't crash the game
                    }
                    for (j = 0; j < 3; j++)
                    {
                        for (int k = 1; k < 4; k++)
                        {
                            Gore.NewGore(Projectile.GetSource_FromThis(), position + Main.rand.NextVector2Circular(15, 15), Main.rand.NextVector2Circular(5, 5), Mod.Find<ModGore>("JetwelderSelector_Gore" + k.ToString()).Type, 1f);
                        }
                    }

                    Projectile proj = Projectile.NewProjectileDirect(Projectile.InheritSource(Projectile), position, Vector2.Zero, projType, Projectile.damage, Projectile.knockBack, Player.whoAmI);
                    proj.originalDamage = Projectile.damage;
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
                if (Framing.GetTileSafely(posX, posY).HasTile && Main.tileSolid[Framing.GetTileSafely(posX, posY).TileType])
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
            bool gray = Player.GetModPlayer<JetwelderPlayer>().scrap < minScrap;

            Vector2 dir = angle.ToRotationVector2() * 80;
            Vector2 pos = (Player.Center + dir) - Main.screenPosition;
            Texture2D tex = gray ? grayTex : regTex;

            float lerper = MathHelper.Clamp(EaseFunction.EaseCubicOut.Ease(growCounter), 0, 1);
            float colorMult = MathHelper.Lerp(0.66f, 1f, lerper);
            float scale = MathHelper.Lerp(0.75f, 1.33f, lerper);
            scale *= EaseFunction.EaseCubicOut.Ease(scaleCounter);
            spriteBatch.Draw(tex, pos, null, Color.White * colorMult, 0, tex.Size() / 2, scale, SpriteEffects.None, 0f);

            Texture2D barTex = ModContent.Request<Texture2D>(Texture + "_Bar").Value;
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
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Terraria.GameContent.FontAssets.ItemStack.Value, minScrap.ToString(), pos - new Vector2(0, 40 * scale), Player.GetModPlayer<JetwelderPlayer>().scrap >= minScrap ? Color.White : Color.Red, 0f, origin, Vector2.One * EaseFunction.EaseCubicOut.Ease(scaleCounter));*/
        }

    }
    public class JetwelderFlame : ModProjectile
    {
        public override string Texture => AssetDirectory.SteampunkItem + Name;

        private Player Player => Main.player[Projectile.owner];

        private Vector2 direction = Vector2.Zero;

        private float scaleCounter = 0f;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Jetwelder");
            Main.projFrames[Projectile.type] = 6;
        }
        public override void SetDefaults()
        {
            Projectile.hostile = false;
            Projectile.width = 114;
            Projectile.height = 36;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 999999;
            Projectile.ignoreWater = true;
            Projectile.ownerHitCheck = true;
        }

        public override void AI()
        {
            if (scaleCounter < 1)
                scaleCounter += 0.1f;
            Projectile.scale = scaleCounter;

            Projectile.velocity = Vector2.Zero;
            if (Player.channel)
            {
                Projectile.timeLeft = 2;
                Player.itemTime = Player.itemAnimation = 2;

                direction = Player.DirectionTo(Main.MouseWorld);
                direction.Normalize();
                Player.ChangeDir(Math.Sign(direction.X));

                Player.itemRotation = direction.ToRotation();

                if (Player.direction != 1)
                    Player.itemRotation -= 3.14f;

                Player.itemRotation = MathHelper.WrapAngle(Player.itemRotation);
            }
            Projectile.rotation = direction.ToRotation();
            Projectile.Center = Player.Center + new Vector2(0, Player.gfxOffY) + new Vector2(30, -18 * Player.direction).RotatedBy(Projectile.rotation);

            for (int i = 0; i < Projectile.width * Projectile.scale; i++)
                Lighting.AddLight(Projectile.Center + (direction * i), Color.LightBlue.ToVector3() * 0.6f);

            Projectile.frameCounter++;
            if (Projectile.frameCounter % 2 == 0)
                Projectile.frame++;
            Projectile.frame %= Main.projFrames[Projectile.type];
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            if (Main.rand.NextBool(4))
                target.AddBuff(BuffID.OnFire, 150);
            knockback = 0;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;

            int frameHeight = tex.Height / Main.projFrames[Projectile.type];
            Rectangle frame = new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight);


            SpriteEffects effects = Player.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;
            Texture2D bloomTex = ModContent.Request<Texture2D>(Texture + "_Glow").Value;
            Color bloomColor = Color.White;
            bloomColor.A = 0;
            Main.EntitySpriteDraw(bloomTex, Projectile.Center - Main.screenPosition, frame, bloomColor, Projectile.rotation, new Vector2(0, tex.Height / (2 * Main.projFrames[Projectile.type])), Projectile.scale, effects, 0);

            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, frame, Color.White, Projectile.rotation, new Vector2(0, tex.Height / (2 * Main.projFrames[Projectile.type])), Projectile.scale, effects, 0);

            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float collisionPoint = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + (direction * Projectile.width * Projectile.scale), Projectile.height * Projectile.scale, ref collisionPoint);
        }

        public override bool? CanCutTiles()
        {
            return true;
        }
        public override void CutTiles()
        {
            DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
            Utils.PlotTileLine(Projectile.Center, Projectile.Center + (direction * Projectile.width * Projectile.scale), Projectile.height * Projectile.scale, DelegateMethods.CutTiles);
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            if (target.life <= 0 && Main.rand.NextBool(5))
                SpawnScrap(target.Center);
            else if (Main.rand.NextBool(30))
                SpawnScrap(target.Center);
        }

        private void SpawnScrap(Vector2 position)
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
            Item.NewItem(Projectile.GetSource_FromThis(), position, ItemType);
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
            Item.maxStack = 1;
        }

        public override bool ItemSpace(Player Player) => true;
        public override bool OnPickup(Player Player)
        {
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Grab, (int)Player.position.X, (int)Player.position.Y);
            if (Player.GetModPlayer<JetwelderPlayer>().scrap < 20)
                Player.GetModPlayer<JetwelderPlayer>().scrap++;

            //Main.NewText(Player.GetModPlayer<JetwelderPlayer>().scrap.ToString());
            return false;
        }

        protected virtual void SetSize() { }

        public override Color? GetAlpha(Color lightColor) => lightColor;
    }

    public class JetwelderScrap1 : JetwelderScrap
    {
        protected override void SetSize() => Item.Size = new Vector2(32, 32);
    }
    public class JetwelderScrap2 : JetwelderScrap
    {
        protected override void SetSize() => Item.Size = new Vector2(32, 32);
    }
    public class JetwelderScrap3 : JetwelderScrap
    {
        protected override void SetSize() => Item.Size = new Vector2(32, 32);
    }
    public class JetwelderScrap4 : JetwelderScrap
    {
        protected override void SetSize() => Item.Size = new Vector2(32, 32);
    }

    public class JetwelderPlayer : ModPlayer
    {
        public int scrap;
    }

    public class JetWelderPlayerLayer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.SolarShield); //PORTTODO: find out what the top most layer is for players and replace

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Player Player = drawInfo.drawPlayer;

            if (Player.HeldItem.type != ModContent.ItemType<Jetwelder>())
                return;

            Texture2D barTex = ModContent.Request<Texture2D>(AssetDirectory.SteampunkItem + "JetwelderBar").Value;
            Texture2D glowTex = ModContent.Request<Texture2D>(AssetDirectory.SteampunkItem + "JetwelderBar_Glow").Value;

            Vector2 drawPos = (Player.MountedCenter - Main.screenPosition) - new Vector2(0, 40 - Player.gfxOffY);

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
            drawInfo.DrawDataCache.Add(value);

            DrawData value2 = new DrawData(
                        glowTex,
                        new Vector2((int)drawPos.X, (int)drawPos.Y) - new Vector2(0, 1),
                        new Rectangle(0, 0, (int)(glowTex.Width * (Player.GetModPlayer<JetwelderPlayer>().scrap / 20f)), glowTex.Height),
                        Color.White,
                        0f,
                        glowTex.Size() / 2,
                        1,
                        SpriteEffects.None,
                        0
                    );
            drawInfo.DrawDataCache.Add(value2);
        }
    }

    internal class JetwelderCasingGore : ModGore
    {
        public override string Texture => AssetDirectory.SteampunkItem + "JetwelderCasing";

        public override bool Update(Gore gore)
        {
            gore.alpha += 4;
            return base.Update(gore);
        }
    }
}