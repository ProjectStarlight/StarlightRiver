using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;

namespace StarlightRiver.Content.Items.Vitric
{
    internal class VitricHamaxe : ModItem, IGlowingItem
    {
        public int heat = 0;
        public int heatTime = 0;

        public override string Texture => AssetDirectory.VitricItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Vitric Hamaxe");
            Tooltip.SetDefault("Right click to charge heat\nHeat increases speed\nHeat dissipates over time");
        }

        public override void SetDefaults()
        {
            item.damage = 26;
            item.melee = true;
            item.width = 36;
            item.height = 32;
            item.useTime = 15;
            item.useAnimation = 32;
            item.axe = 20;
            item.hammer = 60;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.knockBack = 3.5f;
            item.value = 1000;
            item.rare = ItemRarityID.Green;
            item.autoReuse = true;
            item.UseSound = SoundID.Item18;
            item.useTurn = true;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            if(player.altFunctionUse == 2)
            {
                item.axe = 0;
                item.hammer = 0;
                item.noMelee = true;
            }
            else
            {
                item.axe = 20;
                item.hammer = 60;
                item.noMelee = false;
            }

            return true;
        }

        public override void HoldItem(Player player)
        {
            if (item.hammer > 0)
                return;

            if(Main.mouseRight)
            {
                if (player.itemAnimation == 12)
                {
                    player.itemAnimation = 13;

                    if (heat < 100)
                    {
                        heat++;

                        var off = Vector2.One.RotatedByRandom(6.28f) * 20;
                        Dust.NewDustPerfect(player.MountedCenter + (Vector2.One * 40).RotatedBy(player.itemRotation - (player.direction == 1 ? MathHelper.PiOver2 : MathHelper.Pi)) + off, DustType<Dusts.Stamina>(), -off * 0.05f);
                    }

                    if (heat == 98)
                        Main.PlaySound(SoundID.DD2_BetsyFireballShot, player.Center);
                }
            }
        }

        public override float UseTimeMultiplier(Player player)
        {
            return 1 + heat / 100f;
        }

        public override void UpdateInventory(Player player)
        {
            heatTime++;

            if (heatTime >= 10)
            {
                if (heat > 0)
                    heat--;

                heatTime = 0;
            }
        }

        public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            var tex = GetTexture(Texture + "Glow");
            var color = Color.White * (heat / 100f);

            spriteBatch.Draw(tex, position, frame, color, 0, origin, scale, 0, 0);
        }

        public void DrawGlowmask(PlayerDrawInfo info)
        {
            var player = info.drawPlayer;

            if (player.itemAnimation == 0)
                return;

            var tex = GetTexture(Texture + "Glow");
            var color = Color.White * (heat / 100f);
            var origin = player.direction == 1 ? new Vector2(0, tex.Height) : new Vector2(tex.Width, tex.Height);

            var data = new DrawData(tex, info.itemLocation - Main.screenPosition, null, color, player.itemRotation, origin, item.scale, player.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
            Main.playerDrawData.Add(data);
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemType<SandstoneChunk>(), 10);
            recipe.AddIngredient(ItemType<VitricOre>(), 20);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}