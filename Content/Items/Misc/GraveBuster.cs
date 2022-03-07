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
            item.useTime = 40;
            item.useAnimation = 40;
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
                    if (tile.type == 85)
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
}