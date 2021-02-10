using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Items.UndergroundTemple
{
    class TemplePick : ModItem
    {
        public override string Texture => AssetDirectory.CaveTempleItem + Name;

        private int Charge;
        private bool Whirling;
        private int Direction;

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player) => !Whirling && Charge == 0;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Whirlwind Pickaxe");
            Tooltip.SetDefault("Hold right click to charge up a spinning pickaxe dash");
        }

        public override void SetDefaults()
        {
            item.rare = ItemRarityID.Green;
            item.pick = 45;
            item.useTime = 16;
            item.useAnimation = 16;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.damage = 8;
            item.autoReuse = true;
            item.channel = true;
            item.UseSound = SoundID.Item1;
            item.melee = true;
        }

        public override bool UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                item.noUseGraphic = true;
                item.noMelee = true;
            }
            else
            {
                item.noUseGraphic = false;
                item.noMelee = false;
            }
            return true;
        }

        public override void UpdateInventory(Player player) //strange hook to be doing this in but it seemeed the best solution at the time.
        {
            if (player.HeldItem == item) //bleghhh
            {
                if (Main.mouseRight && Charge < 120) //this is gonna go to shiiittt in MPPPPP
                {
                    Dust d = Dust.NewDustPerfect(player.Center, DustType<Content.Dusts.PickCharge>(), Vector2.UnitY.RotatedBy(Charge / 120f * 6.28f) * 30, 0, Color.LightYellow, 2);
                    d.customData = player.whoAmI;

                    if (Charge == 119)
                    {
                        Main.PlaySound(SoundID.MaxMana, player.Center);

                        for (int k = 0; k < 100; k++)
                            Dust.NewDustPerfect(player.Center, DustType<Content.Dusts.Stamina>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(10));
                    }
                }

                if (!Main.mouseRight && !Whirling && Charge == 120)
                {
                    Whirling = true;
                    Direction = Main.MouseWorld.X > player.Center.X ? 1 : -1;
                }

                if (Whirling)
                {
                    Charge--;
                    player.velocity.X = Direction * 8;
                    player.direction = Charge / 3 % 2 == 0 ? 1 : -1;

                    if (Charge % 3 == 0)
                        for (int k = 0; k < 3; k++)
                        {
                            int i = (int)(player.Center.X / 16) + Direction;
                            int j = (int)(player.position.Y / 16) + k;
                            player.PickTile(i, j, item.pick);
                        }

                    if (Charge % 10 == 0) Main.PlaySound(SoundID.Item63, player.Center);

                    if (Charge <= 0) Whirling = false;
                }
            }

            if (!Main.mouseRight && Charge > 0 && !Whirling || player.HeldItem != item) Charge = 0;

            if (Main.mouseRight && player.HeldItem == item && Charge < 120) Charge++;
        }
    }
}
