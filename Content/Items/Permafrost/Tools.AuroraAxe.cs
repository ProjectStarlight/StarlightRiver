using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using StarlightRiver.Core;
using static Terraria.ModLoader.ModContent;
using StarlightRiver.Helpers;

namespace StarlightRiver.Content.Items.Permafrost
{
    class AuroraAxe : ModItem, IGlowingItem
    {
        float charge = 0;
        bool charged = false;

        public override string Texture => AssetDirectory.PermafrostItem + "AuroraAxe";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("[PH]Aurora Axe");
            Tooltip.SetDefault("Variant 0");
        }

        public override void SetDefaults()
        {
            item.rare = ItemRarityID.Green;
            item.value = Item.sellPrice(0, 1, 0, 0);
            item.damage = 10;
            item.useTime = 15;
            item.useAnimation = 30;
            item.axe = 8;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.autoReuse = true;
        }

        public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            var tex = GetTexture(Texture + "Glow");

            float time = Main.GameUpdateCount % 400f;
            float sin2 = (float)Math.Sin(time * 0.2f * 0.2f);
            float cos = (float)Math.Cos(time * 0.2f);
            Color color = new Color(100 * (1 + sin2) / 255f, 140 * (1 + cos) / 255f, 180 / 255f);
            if (color.R < 80) color.R = 80;
            if (color.G < 80) color.G = 80;

            spriteBatch.Draw(tex, position, frame, color * charge * 0.5f, 0, origin, scale, 0, 0);
        }

        public override void HoldItem(Player player)
        {
            item.noUseGraphic = charged;

            if (Main.mouseRight)
            {
                if (charge < 1) charge += 1f / (item.useAnimation * 2f);
                else
                {
                    if (!charged) Main.PlaySound(SoundID.MaxMana);
                    charged = true;
                    charge = 1;
                }

                player.itemAnimation = 2 + (int)(charge * 7);
                player.itemRotation = (1.1f - charge / 2f) * player.direction;
            }
            else if (charge > 0 && charged)
            {
                if (charge == 1)
                    Projectile.NewProjectile(player.Center, Vector2.Zero, ProjectileType<AuroraAxeVFX>(), 0, 0, player.whoAmI, player.direction);

                charge -= 0.025f;
                player.itemAnimation = 9;

                if (Main.GameUpdateCount % 8 == 0) //I think this should work? maximum deviation should be +- 1 hit, and pickTile shoooould sync? I hope? idk this is mostly built on optimism and laziness. TODO: be less lazy
                {
                    Main.PlaySound(SoundID.Item1.SoundId, (int)player.Center.X, (int)player.Center.Y, SoundID.Item1.Style, 1, -0.5f);

                    int center = (int)player.Center.X / 16;
                    int centerY = (int)player.Center.Y / 16;
                    for (int x = center - 3; x <= center + 3; x++)
                    {
                        Tile tile = Framing.GetTileSafely(x, centerY);
                        if (tile.type == TileID.Trees) player.PickTile(x, centerY, 12);
                    }
                }

                if (charge <= 0)
                {
                    charged = false;
                    player.itemAnimation = 0;
                }
            }
            else
            {
                charge = 0;
                charged = false;
            }
        }

        public void DrawGlowmask(PlayerDrawInfo info)
        {
            Player player = info.drawPlayer;

            float time = Main.GameUpdateCount % 400f;
            float sin2 = (float)Math.Sin(time * 0.2f * 0.2f);
            float cos = (float)Math.Cos(time * 0.2f);
            Color color = new Color(100 * (1 + sin2) / 255f, 140 * (1 + cos) / 255f, 180 / 255f);
            if (color.R < 80) color.R = 80;
            if (color.G < 80) color.G = 80;

            if (charge > 0 && charged)
            {
                Texture2D tex = GetTexture("StarlightRiver/Assets/Items/Permafrost/AuroraAxeOut");
                Texture2D tex2 = GetTexture("StarlightRiver/Assets/Items/Permafrost/AuroraAxeOutGlow");

                DrawData data = new DrawData(tex, (player.Center - Main.screenPosition - Vector2.UnitY * (2 - player.gfxOffY)).PointAccur(), null, Lighting.GetColor((int)player.Center.X / 16, (int)player.Center.Y / 16), 0, tex.Size() / 2, 1, info.spriteEffects, 0);
                DrawData data2 = new DrawData(tex2, (player.Center - Main.screenPosition - Vector2.UnitY * (2 - player.gfxOffY)).PointAccur(), null, color * charge * 0.5f, 0, tex.Size() / 2, 1, info.spriteEffects, 0);
                Main.playerDrawData.Add(data);
                Main.playerDrawData.Add(data2);
            }

            else
            {
                var tex3 = GetTexture(Texture + "Glow");
                DrawData data3 = new DrawData(tex3, info.itemLocation - Main.screenPosition, null, color * charge * 0.5f, player.itemRotation, info.spriteEffects == 0 ? new Vector2(0, tex3.Height) : new Vector2(tex3.Width, tex3.Height), player.HeldItem.scale, info.spriteEffects, 0);
                Main.playerDrawData.Add(data3);
            }

            Lighting.AddLight(player.Center, color.ToVector3() * charge * 0.2f);
        }

        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(mod);
            r.AddIngredient(ItemID.CopperAxe);
            r.AddIngredient(ItemType<Tiles.Permafrost.AuroraIceBar>());
            r.SetResult(this);
            r.AddRecipe();

            r = new ModRecipe(mod);
            r.AddIngredient(ItemID.TinAxe);
            r.AddIngredient(ItemType<Tiles.Permafrost.AuroraIceBar>());
            r.SetResult(this);
            r.AddRecipe();
        }
    }

    class AuroraAxeVFX : ModProjectile, IDrawAdditive
    {
        Color thisColor = Color.White;

        public override string Texture => AssetDirectory.Invisible;

        public override void SetDefaults()
        {
            projectile.timeLeft = 240;
            projectile.width = 16;
            projectile.height = 16;
            projectile.friendly = true;
            projectile.damage = 0;
            projectile.tileCollide = false;
            projectile.extraUpdates = 4;

            switch (Main.rand.Next(3))
            {
                case 0: thisColor = new Color(150, 255, 255); break;
                case 1: thisColor = new Color(200, 150, 255); break;
                case 2: thisColor = new Color(150, 255, 150); break;
            }
        }

        public override void AI()
        {
            Player player = Main.player[projectile.owner];

            if (projectile.timeLeft % 8 == 0)
            {
                int d = Dust.NewDust(projectile.position, 16, 16, DustType<Dusts.Aurora>(), 0, 0, 0, thisColor * 0.6f, 1);
                Main.dust[d].customData = Main.rand.NextFloat(0.1f, 0.3f);
            }

            Dust dus = Dust.NewDustPerfect(projectile.Center, DustType<Dusts.Aurora>(), Vector2.Zero, 0, thisColor * 1.1f);
            dus.customData = 0.8f;
            dus.fadeIn = 8.6f;
            dus.rotation = Main.rand.NextFloat(2f);

            float time = (projectile.ai[0] == -1 ? 3.14f : 0f) + projectile.timeLeft / 240f * 6.28f * 2.5f;
            projectile.Center = player.Center + new Vector2((float)Math.Cos(time) * 58, (float)Math.Sin(time) * 16);

            player.UpdateRotation(time);
            if (projectile.timeLeft <= 1) player.UpdateRotation(0);
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            Texture2D tex = GetTexture("StarlightRiver/Assets/Keys/Glow");
            float progress = projectile.timeLeft < 120 ? projectile.timeLeft / 120f : 1 - (projectile.timeLeft - 120) / 120f;

            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, thisColor * progress * 0.7f, 0, tex.Size() / 2, 0.1f, 0, 0);
            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, thisColor * progress * 0.5f, 0, tex.Size() / 2, 0.3f, 0, 0);
            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, thisColor * progress * 0.3f, 0, tex.Size() / 2, 0.5f, 0, 0);
        }
    }
}
