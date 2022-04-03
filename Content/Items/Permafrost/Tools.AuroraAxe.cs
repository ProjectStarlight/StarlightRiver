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
            Item.rare = ItemRarityID.Green;
            Item.value = Item.sellPrice(0, 1, 0, 0);
            Item.damage = 10;
            Item.useTime = 15;
            Item.useAnimation = 30;
            Item.axe = 8;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.autoReuse = true;
        }

        public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color ItemColor, Vector2 origin, float scale)
        {
            var tex = Request<Texture2D>(Texture + "Glow").Value;

            float time = Main.GameUpdateCount % 400f;
            float sin2 = (float)Math.Sin(time * 0.2f * 0.2f);
            float cos = (float)Math.Cos(time * 0.2f);
            Color color = new Color(100 * (1 + sin2) / 255f, 140 * (1 + cos) / 255f, 180 / 255f);
            if (color.R < 80) color.R = 80;
            if (color.G < 80) color.G = 80;

            spriteBatch.Draw(tex, position, frame, color * charge * 0.5f, 0, origin, scale, 0, 0);
        }

        public override void HoldItem(Player Player)
        {
            Item.noUseGraphic = charged;

            if (Main.mouseRight)
            {
                if (charge < 1) charge += 1f / (Item.useAnimation * 2f);
                else
                {
                    if (!charged) Terraria.Audio.SoundEngine.PlaySound(SoundID.MaxMana);
                    charged = true;
                    charge = 1;
                }

                Player.ItemAnimation = 2 + (int)(charge * 7);
                Player.ItemRotation = (1.1f - charge / 2f) * Player.direction;
            }
            else if (charge > 0 && charged)
            {
                if (charge == 1)
                    Projectile.NewProjectile(Player.Center, Vector2.Zero, ProjectileType<AuroraAxeVFX>(), 0, 0, Player.whoAmI, Player.direction);

                charge -= 0.025f;
                Player.ItemAnimation = 9;

                if (Main.GameUpdateCount % 8 == 0) //I think this should work? maximum deviation should be +- 1 hit, and pickTile shoooould sync? I hope? idk this is mostly built on optimism and laziness. TODO: be less lazy
                {
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Item1.SoundId, (int)Player.Center.X, (int)Player.Center.Y, SoundID.Item1.Style, 1, -0.5f);

                    int center = (int)Player.Center.X / 16;
                    int centerY = (int)Player.Center.Y / 16;
                    for (int x = center - 3; x <= center + 3; x++)
                    {
                        Tile tile = Framing.GetTileSafely(x, centerY);
                        if (tile.type == TileID.Trees) Player.PickTile(x, centerY, 12);
                    }
                }

                if (charge <= 0)
                {
                    charged = false;
                    Player.ItemAnimation = 0;
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
            Player Player = info.drawPlayer;

            float time = Main.GameUpdateCount % 400f;
            float sin2 = (float)Math.Sin(time * 0.2f * 0.2f);
            float cos = (float)Math.Cos(time * 0.2f);
            Color color = new Color(100 * (1 + sin2) / 255f, 140 * (1 + cos) / 255f, 180 / 255f);
            if (color.R < 80) color.R = 80;
            if (color.G < 80) color.G = 80;

            if (charge > 0 && charged)
            {
                Texture2D tex = Request<Texture2D>("StarlightRiver/Assets/Items/Permafrost/AuroraAxeOut").Value;
                Texture2D tex2 = Request<Texture2D>("StarlightRiver/Assets/Items/Permafrost/AuroraAxeOutGlow").Value;

                DrawData data = new DrawData(tex, (Player.Center - Main.screenPosition - Vector2.UnitY * (2 - Player.gfxOffY)).PointAccur(), null, Lighting.GetColor((int)Player.Center.X / 16, (int)Player.Center.Y / 16), 0, tex.Size() / 2, 1, info.spriteEffects, 0);
                DrawData data2 = new DrawData(tex2, (Player.Center - Main.screenPosition - Vector2.UnitY * (2 - Player.gfxOffY)).PointAccur(), null, color * charge * 0.5f, 0, tex.Size() / 2, 1, info.spriteEffects, 0);
                Main.playerDrawData.Add(data);
                Main.playerDrawData.Add(data2);
            }

            else
            {
                var tex3 = Request<Texture2D>(Texture + "Glow").Value;
                DrawData data3 = new DrawData(tex3, info.ItemLocation - Main.screenPosition, null, color * charge * 0.5f, Player.ItemRotation, info.spriteEffects == 0 ? new Vector2(0, tex3.Height) : new Vector2(tex3.Width, tex3.Height), Player.HeldItem.scale, info.spriteEffects, 0);
                Main.playerDrawData.Add(data3);
            }

            Lighting.AddLight(Player.Center, color.ToVector3() * charge * 0.2f);
        }

        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(Mod);
            r.AddIngredient(ItemID.CopperAxe);
            r.AddIngredient(ItemType<Tiles.Permafrost.AuroraIceBar>());
            r.SetResult(this);
            r.AddRecipe();

            r = new ModRecipe(Mod);
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
            Projectile.timeLeft = 240;
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.damage = 0;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 4;

            switch (Main.rand.Next(3))
            {
                case 0: thisColor = new Color(150, 255, 255); break;
                case 1: thisColor = new Color(200, 150, 255); break;
                case 2: thisColor = new Color(150, 255, 150); break;
            }
        }

        public override void AI()
        {
            Player Player = Main.player[Projectile.owner];

            if (Projectile.timeLeft % 8 == 0)
            {
                int d = Dust.NewDust(Projectile.position, 16, 16, DustType<Dusts.Aurora>(), 0, 0, 0, thisColor * 0.6f, 1);
                Main.dust[d].customData = Main.rand.NextFloat(0.1f, 0.3f);
            }

            Dust dus = Dust.NewDustPerfect(Projectile.Center, DustType<Dusts.Aurora>(), Vector2.Zero, 0, thisColor * 1.1f);
            dus.customData = 0.8f;
            dus.fadeIn = 8.6f;
            dus.rotation = Main.rand.NextFloat(2f);

            float time = (Projectile.ai[0] == -1 ? 3.14f : 0f) + Projectile.timeLeft / 240f * 6.28f * 2.5f;
            Projectile.Center = Player.Center + new Vector2((float)Math.Cos(time) * 58, (float)Math.Sin(time) * 16);

            Player.UpdateRotation(time);
            if (Projectile.timeLeft <= 1) Player.UpdateRotation(0);
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            Texture2D tex = Request<Texture2D>("StarlightRiver/Assets/Keys/Glow").Value;
            float progress = Projectile.timeLeft < 120 ? Projectile.timeLeft / 120f : 1 - (Projectile.timeLeft - 120) / 120f;

            spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, thisColor * progress * 0.7f, 0, tex.Size() / 2, 0.1f, 0, 0);
            spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, thisColor * progress * 0.5f, 0, tex.Size() / 2, 0.3f, 0, 0);
            spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, thisColor * progress * 0.3f, 0, tex.Size() / 2, 0.5f, 0, 0);
        }
    }
}
