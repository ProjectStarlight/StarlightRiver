using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Vitric
{
	internal class VitricPick : ModItem, IGlowingItem
    {
        public int heat = 0;
        public int heatTime = 0;

        public override string Texture => AssetDirectory.VitricItem + Name;

        public override void Load()
        {
            On.Terraria.Player.PickTile += GenerateHeat;

            
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Vitric Pickaxe");
            Tooltip.SetDefault("Hellstone does not drop lava\nMining hellstone generates heat\n Heat increases speed");
        }

        public override void SetDefaults()
        {
            Item.damage = 10;
            Item.melee = true;
            Item.width = 38;
            Item.height = 38;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.pick = 85;
            Item.useStyle = ItemUseStyleID.SwingThrow;
            Item.knockBack = 5f;
            Item.value = 1000;
            Item.rare = ItemRarityID.Green;
            Item.autoReuse = true;
            Item.UseSound = SoundID.Item18;
            Item.useTurn = true;
        }

        private void GenerateHeat(On.Terraria.Player.orig_PickTile orig, Player self, int x, int y, int pickPower)
        {
            var myPick = self.HeldItem.ModItem as VitricPick;
            var tile = Framing.GetTileSafely(x, y);
            var type = tile.type;

            orig(self, x, y, pickPower);

            tile = Framing.GetTileSafely(x, y);

            if (myPick != null && type == TileID.Hellstone)
            {
                if (myPick.heat < 20)
                    myPick.heat++;

                tile.lava(false);
                tile .LiquidAmount = 0;
                tile.liquidType(0);
                tile.skipLiquid(true);
                NetMessage.SendTileRange(0, x, y, 1, 1);
            }
        }

        public override float UseTimeMultiplier(Player Player)
        {
            return 1 + heat / 40f;
        }

        public override void UpdateInventory(Player Player)
        {
            heatTime++;

            if(heatTime >= 40)
            {
                if (heat > 0)
                    heat--;

                heatTime = 0;
            }
        }

        public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color ItemColor, Vector2 origin, float scale)
        {
            var tex = Request<Texture2D>(Texture + "Glow").Value;
            var color = Color.White * (heat / 20f);


            spriteBatch.Draw(tex, position, frame, color, 0, origin, scale, 0, 0);
        }

        public void DrawGlowmask(PlayerDrawInfo info)
        {
            var Player = info.drawPlayer;

            if (Player.ItemAnimation == 0)
                return;

            var tex = Request<Texture2D>(Texture + "Glow").Value;
            var color = Color.White * (heat / 20f);
            var origin = Player.direction == 1 ? new Vector2(0, tex.Height) : new Vector2(tex.Width, tex.Height);

            var data = new DrawData(tex, info.ItemLocation - Main.screenPosition, null, color, Player.ItemRotation, origin, Item.scale, Player.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
            Main.playerDrawData.Add(data);
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(Mod);
            recipe.AddIngredient(ItemType<SandstoneChunk>(), 10);
            recipe.AddIngredient(ItemType<VitricOre>(), 20);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}