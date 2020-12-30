using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using StarlightRiver.Core;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Permafrost
{
    class AuroraPick : ModItem, IGlowingItem
    {
        float charge = 0;
        bool charged = false;

        public override string Texture => AssetDirectory.PermafrostItem + "AuroraPick";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("[PH]Aurora Pickaxe");
            Tooltip.SetDefault("Variant 0");
        }

        public override void SetDefaults()
        {
            item.rare = ItemRarityID.Green;
            item.value = Item.sellPrice(0, 1, 0, 0);
            item.damage = 10;
            item.useTime = 24;
            item.useAnimation = 24;
            item.pick = 45;
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
            if (Main.mouseRight)
            {
                if (charge < 1) charge += 1f / (item.useTime * 2f);
                else
                {
                    if (!charged) Main.PlaySound(SoundID.MaxMana);
                    charged = true;
                    charge = 1;
                }

                player.itemAnimation = (int)(charge * player.itemAnimationMax);
                player.itemRotation = (1.3f - charge * 3.2f) * player.direction;
            }
            else if (charge > 0 && charged)
            {
                bool inRange = Vector2.DistanceSquared(player.Center, new Vector2(Player.tileTargetX, Player.tileTargetY) * 16) < Math.Pow(Player.tileRangeX * 16, 2);

                charge -= 0.1f;
                player.itemAnimation = (int)(charge * player.itemAnimationMax);

                if (charge > 0.69f && charge < 0.71f && inRange) //trigger VFX early so they line up
                {
                    Vector2 pos = new Vector2(Player.tileTargetX + 0.5f, Player.tileTargetY + 0.5f) * 16;
                    Projectile.NewProjectile(pos, Vector2.Zero, ProjectileType<AuroraPickVFX>(), 0, 0, player.whoAmI);
                    Main.PlaySound(SoundID.DD2_WitherBeastAuraPulse.SoundId, (int)pos.X, (int)pos.Y, SoundID.DD2_WitherBeastAuraPulse.Style, 1.5f, 2f);
                }

                if (charge <= 0)
                {
                    if (inRange)
                    {
                        for (int x = -1; x <= 1; x++)
                            for (int y = -1; y <= 1; y++)
                            {
                                int xReal = Player.tileTargetX + x;
                                int yReal = Player.tileTargetY + y;

                                if (Framing.GetTileSafely(xReal, yReal).type != TileID.Trees)
                                    player.PickTile(xReal, yReal, item.pick);
                            }
                    }

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

        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(mod);
            r.AddIngredient(ItemID.CopperPickaxe);
            r.AddIngredient(ItemType<Tiles.Permafrost.AuroraIceBar>());
            r.SetResult(this);
            r.AddRecipe();

            r = new ModRecipe(mod);
            r.AddIngredient(ItemID.TinPickaxe);
            r.AddIngredient(ItemType<Tiles.Permafrost.AuroraIceBar>());
            r.SetResult(this);
            r.AddRecipe();
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

            var tex3 = GetTexture(Texture + "Glow");
            DrawData data3 = new DrawData(tex3, info.itemLocation - Main.screenPosition, null, color * charge * 0.5f, player.itemRotation, info.spriteEffects == 0 ? new Vector2(0, tex3.Height) : new Vector2(tex3.Width, tex3.Height), player.HeldItem.scale, info.spriteEffects, 0);
            Main.playerDrawData.Add(data3);

            Lighting.AddLight(player.Center, color.ToVector3() * charge * 0.2f);
        }
    }

    class AuroraPickVFX : ModProjectile, IDrawAdditive
    {
        Color thisColor = Color.White;

        public override string Texture => AssetDirectory.Invisible;

        public override void SetDefaults()
        {
            projectile.timeLeft = 30;
            projectile.width = 48;
            projectile.height = 48;
            projectile.friendly = true;
            projectile.damage = 0;
            projectile.tileCollide = false;

            switch (Main.rand.Next(3))
            {
                case 0: thisColor = new Color(150, 255, 255); break;
                case 1: thisColor = new Color(200, 150, 255); break;
                case 2: thisColor = new Color(150, 255, 150); break;
            }
        }

        public override void AI()
        {
            if (projectile.timeLeft % 2 == 0)
            {
                int d = Dust.NewDust(projectile.position, 48, 48, DustType<Dusts.Aurora>(), 0, 0, 0, thisColor * 1.1f, 1);
                Main.dust[d].customData = Main.rand.NextFloat(0.2f, 0.5f);
            }
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            Texture2D tex = GetTexture("StarlightRiver/Assets/Keys/Glow");
            float progress = projectile.timeLeft < 15 ? projectile.timeLeft / 15f : 1 - (projectile.timeLeft - 15) / 15f;

            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, thisColor * progress * 0.7f, 0, tex.Size() / 2, 0.4f, 0, 0);
            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, thisColor * progress * 0.5f, 0, tex.Size() / 2, 0.7f, 0, 0);
            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, thisColor * progress * 0.3f, 0, tex.Size() / 2, 1.1f, 0, 0);
        }
    }
}
