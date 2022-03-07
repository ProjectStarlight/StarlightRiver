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
            item.width = 32;
            item.height = 32;
            item.useTime = 80;
            item.useAnimation = 80;
            item.useStyle = ItemUseStyleID.HoldingOut;
            item.noMelee = true;
            item.autoReuse = false;
            item.shoot = ProjectileType<GraveBusterHeld>();
            item.shootSpeed = 1;
            item.channel = true;
            item.value = Item.sellPrice(0, 0, 20, 0);
            item.rare = ItemRarityID.Blue;
            item.noUseGraphic = true;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
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

        Player owner => Main.player[projectile.owner];

        private bool initialized = false;

        private Vector2 currentDirection => projectile.rotation.ToRotationVector2();

        public override void SetDefaults()
        {
            projectile.hostile = false;
            projectile.ranged = true;
            projectile.width = 2;
            projectile.height = 2;
            projectile.aiStyle = -1;
            projectile.friendly = false;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.timeLeft = 999;
            projectile.ignoreWater = true;
            projectile.alpha = 255;
        }

        public override void AI()
        {
            owner.heldProj = projectile.whoAmI;
            if (owner.itemTime <= 1)
            {
                DestroyGraves();
                projectile.active = false;
            }
            if (projectile.timeLeft % 6 == 0 && owner.itemTime > 15)
            {
                Vector2 range = new Vector2(25, 25);
                Vector2 startPos = (projectile.Center / 16) - range;
                Vector2 endPos = (projectile.Center / 16) + range;

                for (int i = (int)startPos.X; i < (int)endPos.X; i++)
                {
                    for (int j = (int)startPos.Y; j < (int)endPos.Y; j++)
                    {
                        Tile tile = Main.tile[i, j];
                        Tile tile2 = Main.tile[i + 1, j + 1];
                        if (tile.type == 85 && tile.active() && tile2.type == 85 && tile2.active())
                        {
                            Vector2 graveCenter = new Vector2(i + 1, j + 1) * 16;
                            Vector2 offset = Main.rand.NextVector2Circular(8, 8);
                            Projectile.NewProjectile(graveCenter + offset, Vector2.Zero, ModContent.ProjectileType<GraveSlash>(), 0, 0, projectile.owner);
                        }
                    }
                }
            }
            projectile.Center = owner.Center;

            if (!initialized)
            {
                initialized = true;
                projectile.rotation = projectile.DirectionTo(Main.MouseWorld).ToRotation();
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];
            Vector2 position = (owner.Center + (currentDirection * 4)) - Main.screenPosition;

            if (owner.direction == 1)
            {
                SpriteEffects effects1 = SpriteEffects.None;
                Main.spriteBatch.Draw(texture, position, null, lightColor, currentDirection.ToRotation(), new Vector2(texture.Width / 2, texture.Height), projectile.scale, effects1, 0.0f);
            }

            else
            {
                SpriteEffects effects1 = SpriteEffects.FlipHorizontally;
                Main.spriteBatch.Draw(texture, position, null, lightColor * .91f, currentDirection.ToRotation() - 3.14f, new Vector2(texture.Width / 2, texture.Height), projectile.scale, effects1, 0.0f);

            }
            return false;
        }

        private void DestroyGraves()
        {
            Vector2 range = new Vector2(25, 25);
            Vector2 startPos = (projectile.Center / 16) - range;
            Vector2 endPos = (projectile.Center / 16) + range;

            for (int i = (int)startPos.X; i < (int)endPos.X; i++)
            {
                for (int j = (int)startPos.Y; j < (int)endPos.Y; j++)
                {
                    Tile tile = Main.tile[i, j];

                    Tile tile2 = Main.tile[i + 1, j + 1];
                    if (tile.type == 85 && tile.active() && tile2.type == 85 && tile2.active())
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

                    if (tile.type == 85 && tile.active())
                    {
                        tile.active(false);
                        if (!Main.tile[i, j].active() && Main.netMode != NetmodeID.SinglePlayer)
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
            projectile.hostile = false;
            projectile.melee = true;
            projectile.width = 32;
            projectile.height = 32;
            projectile.aiStyle = -1;
            projectile.friendly = false;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.timeLeft = BASETIMELEFT - 2;
            projectile.ignoreWater = true;
            projectile.alpha = 255;
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

            float progress = (BASETIMELEFT - projectile.timeLeft) / (float)BASETIMELEFT;

            int widthExtra = (int)(6 * Math.Sin(progress * 3.14f));

            int min = (BASETIMELEFT - (20 + widthExtra)) - projectile.timeLeft;
            int max = (BASETIMELEFT + (widthExtra)) - projectile.timeLeft;

            int average = (min + max) / 2;
            for (int i = min; i < max; i++)
            {
                float offset = (float)Math.Pow(Math.Abs(i - average) / (float)(max - min), 2);
                Vector2 offsetVector = (direction.RotatedBy(1.57f) * offset * 10);

                cache.Add(projectile.Center + (direction * i));
            }

            trail = new Trail(Main.instance.GraphicsDevice, 20 + (widthExtra * 2), new TriangularTip((int)((32) * 0.6f)), factor => 10 * (1 - Math.Abs((1 - factor) - (projectile.timeLeft / (float)(BASETIMELEFT + 5)))) * (projectile.timeLeft / (float)BASETIMELEFT), factor =>
            {
                return Color.Lerp(Color.Red, Color.DarkRed, factor.X) * 0.8f;
            });

            trail.Positions = cache.ToArray();

            float offset2 = (float)Math.Pow(Math.Abs((max + 1) - average) / (float)(max - min), 2);

            Vector2 offsetVector2 = (direction.RotatedBy(1.57f) * offset2 * 10);
            trail.NextPosition = projectile.Center + (direction * (max + 1));
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
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.Dust + "NeedlerDust";
            return true;
        }
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