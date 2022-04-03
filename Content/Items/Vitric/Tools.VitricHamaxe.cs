using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

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
            Item.damage = 26;
            Item.melee = true;
            Item.width = 36;
            Item.height = 32;
            Item.useTime = 15;
            Item.useAnimation = 32;
            Item.axe = 20;
            Item.hammer = 60;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 3.5f;
            Item.value = 1000;
            Item.rare = ItemRarityID.Green;
            Item.autoReuse = true;
            Item.UseSound = SoundID.Item18;
            Item.useTurn = true;
        }

        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(heat);
            writer.Write(heatTime);
        }

        public override void NetRecieve(BinaryReader reader)
        {
            heat = reader.ReadInt32();
            heatTime = reader.ReadInt32();
        }

        public override void HoldItem(Player Player)
        {
            ControlsPlayer cPlayer = Player.GetModPlayer<ControlsPlayer>();

            if (Main.myPlayer == Player.whoAmI)
                Player.GetModPlayer<ControlsPlayer>().rightClickListener = true;

            Item.noMelee = false;

            if (cPlayer.mouseRight)
            {
                Item.noMelee = true;
                Player.ItemAnimation = 13;

                if (heat < 100)
                {
                    heat++;

                    var off = Vector2.One.RotatedByRandom(6.28f) * 20;
                    Dust.NewDustPerfect(Player.MountedCenter + (Vector2.One * 40).RotatedBy(Player.ItemRotation - (Player.direction == 1 ? MathHelper.PiOver2 : MathHelper.Pi)) + off, DustType<Dusts.Stamina>(), -off * 0.05f);
                }

                if (heat == 98)
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot, Player.Center);
                
            }
        }

        public override float UseTimeMultiplier(Player Player)
        {
            return 1 + heat / 100f;
        }

        public override void UpdateInventory(Player Player)
        {
            heatTime++;

            if (heatTime >= 10)
            {
                if (heat > 0)
                    heat--;

                heatTime = 0;
            }
        }

        public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color ItemColor, Vector2 origin, float scale)
        {
            var tex = Request<Texture2D>(Texture + "Glow").Value;
            var color = Color.White * (heat / 100f);

            spriteBatch.Draw(tex, position, frame, color, 0, origin, scale, 0, 0);
        }

        public void DrawGlowmask(PlayerDrawInfo info)
        {
            var Player = info.drawPlayer;

            if (Player.ItemAnimation == 0)
                return;

            var tex = Request<Texture2D>(Texture + "Glow").Value;
            var color = Color.White * (heat / 100f);
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