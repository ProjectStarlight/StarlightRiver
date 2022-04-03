using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Misc
{
    class GraveBuster : ModItem
    {
        public override string Texture => AssetDirectory.MiscItem + Name;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Gravebuster");
            Tooltip.SetDefault("Destroys nearby graves \nFavorite it to prevent yourself from dropping graves on death \n'You like the taste of brains, we don't like zombies'");
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.useTime = 80;
            Item.useAnimation = 80;
            Item.useStyle = ItemUseStyleID.HoldingOut;
            Item.noMelee = true;
            Item.autoReuse = false;
            Item.shoot = ProjectileType<GraveBusterHeld>();
            Item.shootSpeed = 1;
            Item.channel = true;
            Item.value = Item.sellPrice(0, 0, 20, 0);
            Item.rare = ItemRarityID.Blue;
            Item.noUseGraphic = true;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(Mod);
            recipe.AddIngredient(ItemID.FallenStar, 3);
            recipe.AddRecipeGroup(RecipeGroupID.IronBar, 5);
            recipe.AddRecipeGroup("StarlightRiver:Graves", 1);
            recipe.AddTile(TileID.Anvils);

            recipe.SetResult(this);

            recipe.AddRecipe();
        }
    }
    public class GraveBusterHeld : ModProjectile
    {
        public override string Texture => AssetDirectory.MiscItem + Name;

        public override void SetStaticDefaults() => DisplayName.SetDefault("Grave Buster");

        Player owner => Main.player[Projectile.owner];

        private bool initialized = false;

        private Vector2 currentDirection => Projectile.rotation.ToRotationVector2();

        public override void SetDefaults()
        {
            Projectile.hostile = false;
            Projectile.ranged = true;
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.aiStyle = -1;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 999;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            owner.heldProj = Projectile.whoAmI;
            if (owner.ItemTime <= 1)
            {
                DestroyGraves();
                Projectile.active = false;
            }
            if (Projectile.timeLeft % 6 == 0 && owner.ItemTime > 15)
            {
                Vector2 range = new Vector2(25, 25);
                Vector2 startPos = (Projectile.Center / 16) - range;
                Vector2 endPos = (Projectile.Center / 16) + range;

                for (int i = (int)startPos.X; i < (int)endPos.X; i++)
                {
                    for (int j = (int)startPos.Y; j < (int)endPos.Y; j++)
                    {
                        Tile tile = Main.tile[i, j];
                        Tile tile2 = Main.tile[i + 1, j + 1];
                        if (tile.type == 85 && tile.HasTile && tile2.type == 85 && tile2.HasTile)
                        {
                            Vector2 graveCenter = new Vector2(i + 1, j + 1) * 16;
                            Vector2 offset = Main.rand.NextVector2Circular(8, 8);
                            Projectile.NewProjectile(graveCenter + offset, Vector2.Zero, ModContent.ProjectileType<GraveSlash>(), 0, 0, Projectile.owner);
                        }
                    }
                }
            }
            Projectile.Center = owner.Center;

            if (!initialized)
            {
                initialized = true;
                Projectile.rotation = Projectile.DirectionTo(Main.MouseWorld).ToRotation();
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[Projectile.type];
            Vector2 position = (owner.Center + (currentDirection * 4)) - Main.screenPosition;

            if (owner.direction == 1)
            {
                SpriteEffects effects1 = SpriteEffects.None;
                Main.spriteBatch.Draw(texture, position, null, lightColor, currentDirection.ToRotation(), new Vector2(texture.Width / 2, texture.Height), Projectile.scale, effects1, 0.0f);
            }

            else
            {
                SpriteEffects effects1 = SpriteEffects.FlipHorizontally;
                Main.spriteBatch.Draw(texture, position, null, lightColor * .91f, currentDirection.ToRotation() - 3.14f, new Vector2(texture.Width / 2, texture.Height), Projectile.scale, effects1, 0.0f);

            }
            return false;
        }

        private void DestroyGraves()
        {
            Vector2 range = new Vector2(25, 25);
            Vector2 startPos = (Projectile.Center / 16) - range;
            Vector2 endPos = (Projectile.Center / 16) + range;

            for (int i = (int)startPos.X; i < (int)endPos.X; i++)
            {
                for (int j = (int)startPos.Y; j < (int)endPos.Y; j++)
                {
                    Tile tile = Main.tile[i, j];

                    Tile tile2 = Main.tile[i + 1, j + 1];
                    if (tile.type == 85 && tile.HasTile && tile2.type == 85 && tile2.HasTile)
                    {
                        Vector2 graveCenter = new Vector2(i + 1, j + 1) * 16;
                        for (int t = 0; t < 10; t++)
                        {
                            Dust dust = Dust.NewDustDirect(graveCenter - new Vector2(16, 16), 0, 0, ModContent.DustType<GraveBusterDust>());
                            dust.velocity = Main.rand.NextVector2Circular(7, 7);
                            dust.scale = Main.rand.NextFloat(1.5f, 1.9f);
                            dust.alpha = 70 + Main.rand.Next(60);
                            dust.rotation = Main.rand.NextFloat(6.28f);
                        }
                    }

                    if (tile.type == 85 && tile.HasTile)
                    {
                        tile.active(false);
                        if (!Main.tile[i, j].HasTile && Main.netMode != NetmodeID.SinglePlayer)
                        {
                            NetMessage.SendData(MessageID.TileChange, -1, -1, null, 0, (float)i, (float)j, 0f, 0, 0, 0);
                        }
                    }
                }
            }
        }
    }
    public class GraveSlash : ModProjectile, IDrawPrimitive
    {
        public override string Texture => AssetDirectory.MiscItem + "GraveBuster";

        private readonly int BASETIMELEFT = 25;

        BasicEffect effect;

        private List<Vector2> cache;
        private Trail trail;

        private Vector2 direction = Vector2.Zero;

        public override void SetStaticDefaults() => DisplayName.SetDefault("Slash");

        public override void SetDefaults()
        {
            Projectile.hostile = false;
            Projectile.melee = true;
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.aiStyle = -1;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = BASETIMELEFT - 2;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            if (effect == null)
            {
                effect = new BasicEffect(Main.instance.GraphicsDevice);
                effect.VertexColorEnabled = true;
            }

            if (direction == Vector2.Zero)
                direction = Main.rand.NextFloat(6.28f).ToRotationVector2() * (32) * 0.06f;
            cache = new List<Vector2>();

            float progress = (BASETIMELEFT - Projectile.timeLeft) / (float)BASETIMELEFT;

            int widthExtra = (int)(6 * Math.Sin(progress * 3.14f));

            int min = (BASETIMELEFT - (20 + widthExtra)) - Projectile.timeLeft;
            int max = (BASETIMELEFT + (widthExtra)) - Projectile.timeLeft;

            int average = (min + max) / 2;
            for (int i = min; i < max; i++)
            {
                float offset = (float)Math.Pow(Math.Abs(i - average) / (float)(max - min), 2);
                Vector2 offsetVector = (direction.RotatedBy(1.57f) * offset * 10);

                cache.Add(Projectile.Center + (direction * i));
            }

            trail = new Trail(Main.instance.GraphicsDevice, 20 + (widthExtra * 2), new TriangularTip((int)((32) * 0.6f)), factor => 10 * (1 - Math.Abs((1 - factor) - (Projectile.timeLeft / (float)(BASETIMELEFT + 5)))) * (Projectile.timeLeft / (float)BASETIMELEFT), factor =>
            {
                return Color.Lerp(Color.Red, Color.DarkRed, factor.X) * 0.8f;
            });

            trail.Positions = cache.ToArray();

            float offset2 = (float)Math.Pow(Math.Abs((max + 1) - average) / (float)(max - min), 2);

            Vector2 offsetVector2 = (direction.RotatedBy(1.57f) * offset2 * 10);
            trail.NextPosition = Projectile.Center + (direction * (max + 1));
        }

        public void DrawPrimitives()
        {
            if (effect == null)
                return;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.World = world;
            effect.View = view;
            effect.Projection = projection;

            trail?.Render(effect);
        }
    }

    public class GraveBusterDust : ModDust
    {
        public override string Texture => AssetDirectory.Dust + "NeedlerDust";
        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.scale *= Main.rand.NextFloat(0.4f, 1.25f);
            dust.frame = new Rectangle(0, 0, 34, 36);
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            Color gray = new Color(25, 25, 25);
            Color ret;
            if (dust.alpha < 80)
            {
                ret = Color.Red;
            }
            else if (dust.alpha < 140)
            {
                ret = Color.Lerp(Color.Red, gray, (dust.alpha - 80) / 80f);
            }
            else
                ret = gray;
            return ret * ((255 - dust.alpha) / 255f);
        }

        public override bool Update(Dust dust)
        {
            if (dust.velocity.Length() > 3)
                dust.velocity *= 0.85f;
            else
                dust.velocity *= 0.92f;
            if (dust.alpha > 100)
            {
                dust.scale += 0.01f;
                dust.alpha += 2;
            }
            else
            {
                Lighting.AddLight(dust.position, dust.color.ToVector3() * 0.1f);
                dust.scale *= 0.985f;
                dust.alpha += 4;
            }
            dust.position += dust.velocity;
            if (dust.alpha >= 255)
                dust.active = false;

            return false;
        }
    }
}