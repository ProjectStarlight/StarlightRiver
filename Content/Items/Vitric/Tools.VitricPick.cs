using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using StarlightRiver.Core;
using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;

namespace StarlightRiver.Content.Items.Vitric
{
    internal class VitricPick : ModItem, IGlowingItem
    {
        public int heat = 0;
        public int heatTime = 0;

        public override string Texture => AssetDirectory.VitricItem + Name;

        public override bool Autoload(ref string name)
        {
            On.Terraria.Player.PickTile += GenerateHeat;

            return base.Autoload(ref name);
        }

        public override void SetDefaults()
        {
            item.damage = 10;
            item.melee = true;
            item.width = 38;
            item.height = 38;
            item.useTime = 18;
            item.useAnimation = 18;
            item.pick = 85;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.knockBack = 5f;
            item.value = 1000;
            item.rare = ItemRarityID.Green;
            item.autoReuse = true;
            item.UseSound = SoundID.Item18;
            item.useTurn = true;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Vitric Pickaxe");
            Tooltip.SetDefault("Hellstone does not drop lava\nMining hellstone generates heat\n Heat increases speed");
        }

        private void GenerateHeat(On.Terraria.Player.orig_PickTile orig, Player self, int x, int y, int pickPower)
        {
            var myPick = self.HeldItem.modItem as VitricPick;
            var tile = Framing.GetTileSafely(x, y);

            if (myPick != null && tile.type == TileID.Hellstone)
            {
                if (myPick.heat < 20)
                    myPick.heat++;

                tile.liquid = 0; //succ the lava
                tile.lava(false);
            }

            orig(self, x, y, pickPower);
        }

        public override float UseTimeMultiplier(Player player)
        {
            return 1 + heat / 20f;
        }

        public override void UpdateInventory(Player player)
        {
            heatTime++;

            if(heatTime >= 20)
            {
                if (heat > 0)
                    heat--;

                heatTime = 0;
            }
        }

        public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            var tex = GetTexture(Texture + "Glow");
            var color = Color.White * (heat / 20f);


            spriteBatch.Draw(tex, position, frame, color, 0, origin, scale, 0, 0);
        }

        public void DrawGlowmask(PlayerDrawInfo info)
        {
            var tex = GetTexture(Texture + "Glow");
            var color = Color.White * (heat / 20f);

            var data = new DrawData(tex, info.itemLocation, null, color, info.drawPlayer.itemRotation, default, item.scale, 0, 0);
            Main.playerDrawData.Add(data);
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.FossilOre, 10);
            recipe.AddIngredient(ItemType<VitricGem>(), 5);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}