using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Misc
{
    class Sorcerwrench : ModItem
    {
        public override string Texture => AssetDirectory.MiscItem + Name;

        public Projectile proj;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sorcerwrench");
            Tooltip.SetDefault("Select an area of blocks to be broken\n" +
            "Consumes 2 mana per block broken");
        }

        public override void SetDefaults()
        {
            item.width = 32;
            item.height = 32;
            item.useTime = 40;
            item.useAnimation = 40;
            item.useStyle = ItemUseStyleID.HoldingOut;
            Item.staff[item.type] = true;
            item.noMelee = true;
            item.autoReuse = false;
            item.shoot = ProjectileType<SorcerwrenchProjectile>();
            item.shootSpeed = 1;
            item.channel = true;
            item.value = Item.buyPrice(1, 0, 0, 0);
            item.rare = ItemRarityID.Orange;
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            position = Main.MouseWorld;
            proj = Projectile.NewProjectileDirect(position, Vector2.Zero, type, damage, knockBack, player.whoAmI);
            return false;
        }
    }

    public class SorcerwrenchProjectile : ModProjectile, IDrawOverTiles
    {
        public override string Texture => AssetDirectory.MiscItem + "Sorcerwrench";

        public override bool Autoload(ref string name)
        {
            On.Terraria.Main.DrawInterface_Resources_Mana += DrawRottenMana;
            return base.Autoload(ref name);
        }

        private const int DESTRUCTIONTIME = 100;

        private const int MANAPERTILE = 2;

        private bool initialized = false;

        private Vector2 startCorner = Vector2.Zero;

        private Vector2 startCornerSmall => startCorner / 16;

        private Vector2 endCorner = Vector2.Zero;

        private Vector2 endCornerGoal = Vector2.Zero;



        private float manaUsed;


        private bool released = false;


        private List<Vector2> tilesToDestroy = new List<Vector2>();



        private Player owner => Main.player[projectile.owner];

        public override void SetDefaults()
        {
            projectile.width = 2;
            projectile.height = 2;
            projectile.timeLeft = 150;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
        }

        public override void AI()
        {
            if (!initialized)
            {
                initialized = true;
                startCorner.X = endCorner.X = (int)Math.Floor(projectile.Center.X / 16) * 16;
                startCorner.Y = endCorner.Y = (int)Math.Floor(projectile.Center.Y / 16) * 16;
            }
            if (owner.channel && !released)
            {
                projectile.timeLeft = DESTRUCTIONTIME;
                projectile.Center = Main.MouseWorld;

                owner.ChangeDir(Main.MouseWorld.X > owner.position.X ? 1 : -1);
                Vector2 direction = owner.DirectionTo(Main.MouseWorld);
                owner.itemTime = owner.itemAnimation = 2;
                owner.itemRotation = direction.ToRotation();

                if (owner.direction != 1)
                    owner.itemRotation -= 3.14f;

                owner.itemRotation = MathHelper.WrapAngle(owner.itemRotation);

                Vector2 endCornerGoalGoal;
                endCornerGoalGoal.X = (int)Math.Floor(Main.MouseWorld.X / 16) * 16;
                endCornerGoalGoal.Y = (int)Math.Floor(Main.MouseWorld.Y / 16) * 16;

                Vector2 difference = Vector2.Zero;
                endCornerGoal = startCorner;

                Vector2 dir2 = endCornerGoalGoal - startCorner;

                if (Math.Abs(dir2.X) > 12 && Math.Abs(dir2.Y) > 12)
                {
                    dir2.Normalize();

                    int xDifferenceInt = 0;
                    int yDifferenceInt = 0;

                    int selectedTiles = 0;

                    manaUsed = 0;

                    int tries = 0;
                    while (selectedTiles * MANAPERTILE < owner.statMana)
                    {
                        if (manaUsed > owner.statMana)
                            break;
                        difference += dir2 * 8;
                        if ((int)Math.Abs((difference.X / 16)) > xDifferenceInt)
                        {
                            for (int y = (int)startCornerSmall.Y; PastIncrement(y, (yDifferenceInt * (int)Math.Sign(dir2.Y)) + (int)startCornerSmall.Y, (int)Math.Sign(dir2.Y)); y+= Math.Sign(dir2.Y))
                            {
                                if (CanKillTile((xDifferenceInt * Math.Sign(dir2.X)) + (int)startCornerSmall.X, y))
                                {
                                    manaUsed += MANAPERTILE;
                                }
                            }

                            if (manaUsed > owner.statMana)
                                break;
                            xDifferenceInt = (int)Math.Abs((difference.X / 16));
                        }
                        if ((int)Math.Abs((difference.Y / 16)) > yDifferenceInt)
                        {
                            for (int x = (int)startCornerSmall.X; PastIncrement(x, (xDifferenceInt * (int)Math.Sign(dir2.X)) + (int)startCornerSmall.X, (int)Math.Sign(dir2.X)); x += Math.Sign(dir2.X))
                            {
                                if (CanKillTile(x, (yDifferenceInt * Math.Sign(dir2.Y)) + (int)startCornerSmall.Y))
                                {
                                    manaUsed += MANAPERTILE;
                                }
                            }

                            if (manaUsed > owner.statMana)
                                break;
                            yDifferenceInt = (int)Math.Abs((difference.Y / 16));
                        }

                        if (yDifferenceInt > 20 || xDifferenceInt > 20)
                            break;

                        endCornerGoal = startCorner + new Vector2(xDifferenceInt  * 16 * Math.Sign(dir2.X), yDifferenceInt * 16 * Math.Sign(dir2.Y));
                        if (!PastIncrement((int)endCornerGoal.X, (int)endCornerGoalGoal.X, Math.Sign(dir2.X)) || !PastIncrement((int)endCornerGoal.Y, (int)endCornerGoalGoal.Y, Math.Sign(dir2.Y)))
                        {
                            break;
                        }

                        tries++;
                        if (tries > 4999)
                        {
                            Main.NewText("Sorcerwrench error: too many tries! Report to developers of Starlight River immediately!");
                            break;
                        }
                    }
                }

                endCorner = Vector2.Lerp(endCorner, endCornerGoal, 0.35f);

                if (Math.Abs(endCorner.X - endCornerGoal.X) < 5)
                    endCorner.X = endCornerGoal.X;
                if (Math.Abs(endCorner.Y - endCornerGoal.Y) < 5)
                    endCorner.Y = endCornerGoal.Y;

                if (Main.mouseRight)
                    projectile.active = false;
            }
            else if (projectile.timeLeft > 2)
            {
                if (!released)
                {
                    owner.statMana -= (int)manaUsed;
                    released = true;
                    Vector2 startCornerSmall =  startCorner / 16;
                    Vector2 endCornerGoalSmall = endCornerGoal / 16;

                    int xIncrement = Math.Sign(endCornerGoalSmall.X - startCornerSmall.X);
                    int yIncrement = Math.Sign(endCornerGoalSmall.Y - startCornerSmall.Y);

                    if (xIncrement == 0 || yIncrement == 0)
                        return;

                    if (xIncrement < 0)
                    {
                        startCornerSmall.X -= 1;
                        endCornerGoalSmall.X -= 1;
                    }

                    if (yIncrement < 0)
                    {
                        startCornerSmall.Y -= 1;
                        endCornerGoalSmall.Y -= 1;
                    }

                    for (int i = (int)startCornerSmall.X; PastIncrement(i, (int)endCornerGoalSmall.X, xIncrement, false); i += xIncrement)
                    {
                        for (int j = (int)startCornerSmall.Y; PastIncrement(j, (int)endCornerGoalSmall.Y, yIncrement, false); j += yIncrement)
                        {
                            bool canKillTile = CanKillTile(i, j);

                            if (!PastIncrement(i, (int)endCornerGoalSmall.X, xIncrement, false) && !PastIncrement(j, (int)endCornerGoalSmall.Y, yIncrement, false))
                                canKillTile = false;
                            if (canKillTile)
                            {
                                tilesToDestroy.Add(new Vector2(i,j));
                            }
                        }
                    }
                }

                float opacity = (DESTRUCTIONTIME - projectile.timeLeft) / (float)DESTRUCTIONTIME;
                opacity =EaseFunction.EaseQuadIn.Ease(opacity);
                foreach(Vector2 position in tilesToDestroy)
                {
                    Lighting.AddLight(position * 16, Color.White.ToVector3() * opacity);
                }
            }
            else
            {
                projectile.active = false;

                foreach (Vector2 pos in tilesToDestroy)
                {
                    int i = (int)pos.X;
                    int j = (int)pos.Y;

                    for (int l = 0; l < 1; l++) //one time loop so its easy to change if I want to :trollge:
                    {
                        Dust dust = Dust.NewDustDirect(new Vector2(i * 16, j * 16), 0, 0, ModContent.DustType<SorcerwrenchDust>());
                        dust.velocity = Main.rand.NextVector2Circular(3, 3);
                        dust.scale = Main.rand.NextFloat(0.8f, 1.2f);
                        dust.alpha = 70 + Main.rand.Next(60);
                        dust.rotation = Main.rand.NextFloat(6.28f);

                        Dust dust2 = Dust.NewDustDirect(new Vector2(i * 16, j * 16), 0, 0, ModContent.DustType<SorcerwrenchSparkle>());
                        dust2.velocity = Main.rand.NextVector2Circular(6, 6);
                        dust2.scale = Main.rand.NextFloat(0.1f, 0.2f);
                        dust2.alpha = Main.rand.Next(60);
                        dust2.rotation = Main.rand.NextFloat(6.28f);
                    }

                    WorldGen.KillTile(i, j, false, false, false);
                    if (!Main.tile[i, j].active() && Main.netMode != NetmodeID.SinglePlayer)
                    {
                        NetMessage.SendData(MessageID.TileChange, -1, -1, null, 0, (float)i, (float)j, 0f, 0, 0, 0);
                    }
                }
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (projectile.timeLeft < DESTRUCTIONTIME - 2)
                return false;
            int xIncrement = Math.Sign(endCorner.X - startCorner.X) * 2;
            int yIncrement = Math.Sign(endCorner.Y - startCorner.Y) * 2;

            for (int i = (int)startCorner.X; PastIncrement(i, (int)endCorner.X, xIncrement); i+= xIncrement)
            {
                for (int j = (int)startCorner.Y; PastIncrement(j, (int)endCorner.Y, yIncrement); j += yIncrement)
                {
                    if (i == startCorner.X || Math.Abs(i - endCorner.X) < 2 || j == startCorner.Y || Math.Abs(j - endCorner.Y) < 2)
                        DrawPixel(spriteBatch, Color.Red, new Vector2(i, j));
                    else
                    {
                        Color shadeColor = Color.LightSalmon;
                        DrawPixel(spriteBatch, shadeColor * 0.3f, new Vector2(i, j));
                    }
                }
            }


            return false;
        }

        private static void DrawPixel(SpriteBatch spriteBatch, Color color, Vector2 worldPos)
        {
            spriteBatch.Draw(Main.magicPixel, worldPos - Main.screenPosition, new Rectangle(0, 0, 1, 1), color, 0, Vector2.Zero, 2, SpriteEffects.None, 0f);
        }

        private static bool PastIncrement(int val, int end, int increment, bool includeEqual = true)
        {
            if (increment == 0)
                return false;
            if (includeEqual)
            {
                if (increment > 0)
                    return val <= end;
                else
                    return val >= end;
            }
            if (increment > 0)
                return val < end;
            else
                return val > end;
        }

        private static bool CanKillTile(int i, int j)
        {
            if (Main.tile[i, j] != null && Main.tile[i, j].active())
            {
                if (Main.tileDungeon[(int)Main.tile[i, j].type] || Main.tile[i, j].type == 88 || Main.tile[i, j].type == 21 || Main.tile[i, j].type == 26 || Main.tile[i, j].type == 107 || Main.tile[i, j].type == 108 || Main.tile[i, j].type == 111 || Main.tile[i, j].type == 226 || Main.tile[i, j].type == 237 || Main.tile[i, j].type == 221 || Main.tile[i, j].type == 222 || Main.tile[i, j].type == 223 || Main.tile[i, j].type == 211 || Main.tile[i, j].type == 404)
                {
                    return false;
                }
                if (!Main.hardMode && Main.tile[i, j].type == 58)
                {
                    return false;
                }
                if (!TileLoader.CanExplode(i, j))
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        private void DrawRottenMana(On.Terraria.Main.orig_DrawInterface_Resources_Mana orig)
        {
            orig();

            Player player = Main.LocalPlayer;

            if (player.HeldItem.type != ModContent.ItemType<Sorcerwrench>())
                return;

            var item = player.HeldItem.modItem as Sorcerwrench;
            if (item.proj == null || !item.proj.active)
                return;
            var projectile = item.proj.modProjectile as SorcerwrenchProjectile;

            if (projectile == null)
                return;

            if (projectile.released)
                return;

            int manaMissing = (player.statManaMax2 - player.statMana) / 20;

            for (int i = 1; i < player.statManaMax2 / 20 + 1; i++) //iterate each mana star
            {
                int manaDrawn = i * 20; //the amount of mana drawn by this star and all before it

                float starHeight = MathHelper.Clamp(((Main.player[Main.myPlayer].statMana - (i - 1) * 20) / 20f) / 4f + 0.75f, 0.75f, 1); //height of the current star based on current mana

                if (player.statMana <= i * 20 && player.statMana >= (i - 1) * 20) //pulsing star for the "current" star
                    starHeight += Main.cursorScale - 1;

                var rottenManaAmount = player.statManaMax2 - projectile.manaUsed; //amount of mana to draw as rotten

                if (rottenManaAmount < manaDrawn + manaMissing && i < (player.statManaMax2 / 20 + 1) - manaMissing)
                {
                    if (manaDrawn - rottenManaAmount < 20)
                    {
                        var tex1 = GetTexture(AssetDirectory.GravediggerItem + "RottenMana");
                        var pos1 = new Vector2(Main.screenWidth - 25, (30 + Main.manaTexture.Height / 2f) + (Main.manaTexture.Height - Main.manaTexture.Height * starHeight) / 2f + (28 * (i - 1)));

                        int off = (int)(rottenManaAmount % 20 / 20f * tex1.Height);
                        var source = new Rectangle(0, off, tex1.Width, tex1.Height - off);
                        pos1.Y += off;

                        Main.spriteBatch.Draw(tex1, pos1, source, Color.White, 0f, tex1.Size() / 2, starHeight, 0, 0);
                        continue;
                    }

                    var tex = GetTexture(AssetDirectory.GravediggerItem + "RottenMana");
                    var pos = new Vector2(Main.screenWidth - 25, (30 + Main.manaTexture.Height / 2f) + (Main.manaTexture.Height - Main.manaTexture.Height * starHeight) / 2f + (28 * (i - 1)));

                    Main.spriteBatch.Draw(tex, pos, null, Color.White, 0f, tex.Size() / 2, starHeight, 0, 0);
                }
            }
        }

        public void DrawOverTiles(SpriteBatch spriteBatch)
        {
            float opacity = (DESTRUCTIONTIME - projectile.timeLeft) / (float)DESTRUCTIONTIME;
            opacity = EaseFunction.EaseQuadIn.Ease(opacity);

            Color color = Color.Lerp(Color.Salmon, Color.White, opacity) * opacity;

            int xIncrement = Math.Sign(endCornerGoal.X - startCorner.X) * 2;
            int yIncrement = Math.Sign(endCornerGoal.Y - startCorner.Y) * 2;

            for (int i = (int)startCorner.X; PastIncrement(i, (int)endCornerGoal.X, xIncrement); i += xIncrement)
            {
                for (int j = (int)startCorner.Y; PastIncrement(j, (int)endCornerGoal.Y, yIncrement); j += yIncrement)
                {
                    DrawPixel(spriteBatch, color, new Vector2(i, j));
                }
            }
        }
    }
    public class SorcerwrenchDust : ModDust
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.Dust + "NeedlerDust";
            return true;
        }
        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.scale *= Main.rand.NextFloat(0.8f, 2f);
            dust.frame = new Rectangle(0, 0, 34, 36);
            dust.color = Color.Lerp(Color.White, Color.Salmon, EaseFunction.EaseQuadIn.Ease(Main.rand.NextFloat() / 2));
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color * ((255 - dust.alpha) / 255f) * 0.6f;
        }

        public override bool Update(Dust dust)
        {
            if (Math.Abs(dust.velocity.Length()) > 3)
                dust.velocity *= 0.9f;
            else
                dust.velocity *= 0.95f;

            Lighting.AddLight(dust.position, Color.White.ToVector3() * 1.4f * ((255 - dust.alpha) / 255f));
            if (dust.alpha > 100)
            {
                //dust.scale += 0.01f;
                dust.alpha += 4;
            }
            else
            {
                dust.scale *= 0.985f;
                dust.alpha += 8;
            }
            dust.position += dust.velocity;
            if (dust.alpha >= 255)
                dust.active = false;

            return false;
        }
    }
    public class SorcerwrenchSparkle : ModDust
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.Dust + "Aurora";
            return true;
        }
        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.scale *= Main.rand.NextFloat(0.2f, 0.4f);
            dust.noLight = false;
            dust.frame = new Rectangle(0, 0, 100, 100);
            dust.color = Color.Lerp(Color.White, Color.Salmon, EaseFunction.EaseQuadIn.Ease(Main.rand.NextFloat()));
            dust.shader = new Terraria.Graphics.Shaders.ArmorShaderData(new Ref<Effect>(StarlightRiver.Instance.GetEffect("Effects/GlowingDust")), "GlowingDustPass");
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color * ((255 - dust.alpha) / 255f) * 0.6f;
        }

        public override bool Update(Dust dust)
        {
            if (Math.Abs(dust.velocity.Length()) > 3)
                dust.velocity *= 0.85f;
            else
                dust.velocity *= 0.92f;

            dust.shader.UseColor(dust.color * ((255 - dust.alpha) / 255f) * 0.6f);

            Lighting.AddLight(dust.position, Color.White.ToVector3() * 1.4f * ((255 - dust.alpha) / 255f));
            if (dust.alpha > 100)
            {
                //dust.scale += 0.01f;
                dust.alpha += 4;
            }
            else
            {
                dust.scale *= 0.985f;
                dust.alpha += 8;
            }
            dust.position += dust.velocity;
            if (dust.alpha >= 255)
                dust.active = false;

            return false;
        }
    }

    public class SorcerwrenchGlobalNPC : GlobalNPC
    {
        public override void SetupShop(int type, Chest shop, ref int nextSlot)
        {
            if (type == NPCID.Mechanic)
            {
                shop.item[nextSlot].SetDefaults(ModContent.ItemType<Sorcerwrench>());
                nextSlot++;
            }
        }
    }
}
