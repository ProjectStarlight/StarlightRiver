using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Permafrost
{
	class WaterStaff : ModItem
	{
        public override string Texture => AssetDirectory.PermafrostItem + "WaterStaff";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Staff of the Waves");
            Tooltip.SetDefault("Places and removes aurora water\nLeft click to place\nRight click to remove");
        }

        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.useTime = 12;
            Item.useAnimation = 12;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.value = 10000;
            Item.rare = ItemRarityID.Orange;
            Item.autoReuse = true;
            Item.useTurn = true;
        }

        public override bool AltFunctionUse(Player Player) => true;

		public override bool? UseItem(Player Player)
        {
            if (WorldGen.InWorld(Player.tileTargetX, Player.tileTargetY))
            {
                if (Player.altFunctionUse != 2)
                {
                    AuroraWaterSystem.PlaceAuroraWater(Player.tileTargetX, Player.tileTargetY);

                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Splash);

                    for (int k = 0; k < 5; k++)
                        Dust.NewDust(new Vector2(Player.tileTargetX, Player.tileTargetY) * 16, 16, 16, ModContent.DustType<Dusts.Glow>(), 0, 0, 0, AuroraColor(Main.rand.NextFloat()), 0.6f);
                }

                if(Player.altFunctionUse == 2)
				{
                    AuroraWaterSystem.RemoveAuroraWater(Player.tileTargetX, Player.tileTargetY);

                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Splash);
                    for (int k = 0; k < 10; k++)
                        Dust.NewDust(new Vector2(Player.tileTargetX, Player.tileTargetY) * 16, 16, 16, ModContent.DustType<Dusts.Glow>(), 0, 0, 0, AuroraColor(Main.rand.NextFloat()), 0.6f);
                }
            }

            return true;
        }

        private Color AuroraColor(float seed)
        {
            return new Color(5, 200, 255);
        }
    }
}
