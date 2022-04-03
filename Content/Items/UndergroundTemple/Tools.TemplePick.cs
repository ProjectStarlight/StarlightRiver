using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.UndergroundTemple
{
	class TemplePick : ModItem
    {
        public override string Texture => AssetDirectory.CaveTempleItem + Name;

        private int Charge;
        private bool Whirling;
        private int Direction;

        public override bool AltFunctionUse(Player Player) => true;

        public override bool CanUseItem(Player Player) => !Whirling && Charge == 0;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Whirlwind Pickaxe");
            Tooltip.SetDefault("Hold right click to charge up a spinning pickaxe dash");
        }

        public override void SetDefaults()
        {
            Item.rare = ItemRarityID.Green;
            Item.pick = 45;
            Item.useTime = 16;
            Item.useAnimation = 16;
            Item.useStyle = ItemUseStyleID.SwingThrow;
            Item.damage = 8;
            Item.autoReuse = true;
            Item.channel = true;
            Item.UseSound = SoundID.Item1;
            Item.melee = true;
        }

        public override bool? UseItem(Player Player)
        {
            if (Player.altFunctionUse == 2)
            {
                Item.noUseGraphic = true;
                Item.noMelee = true;
            }
            else
            {
                Item.noUseGraphic = false;
                Item.noMelee = false;
            }
            return true;
        }

        public override void UpdateInventory(Player Player) //strange hook to be doing this in but it seemeed the best solution at the time.
        {
            if (Player.HeldItem == Item) //bleghhh
            {
                if (Main.mouseRight && Charge < 120) //this is gonna go to shiiittt in MPPPPP
                {
                    Dust d = Dust.NewDustPerfect(Player.Center, DustType<Content.Dusts.PickCharge>(), Vector2.UnitY.RotatedBy(Charge / 120f * 6.28f) * 30, 0, Color.LightYellow, 2);
                    d.customData = Player.whoAmI;

                    if (Charge == 119)
                    {
                        Terraria.Audio.SoundEngine.PlaySound(SoundID.MaxMana, Player.Center);

                        for (int k = 0; k < 100; k++)
                            Dust.NewDustPerfect(Player.Center, DustType<Content.Dusts.Stamina>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(10));
                    }
                }

                if (!Main.mouseRight && !Whirling && Charge == 120)
                {
                    Whirling = true;
                    Direction = Main.MouseWorld.X > Player.Center.X ? 1 : -1;
                }

                if (Whirling)
                {
                    Charge--;
                    Player.velocity.X = Direction * 8;
                    Player.direction = Charge / 3 % 2 == 0 ? 1 : -1;

                    if (Charge % 3 == 0)
                        for (int k = 0; k < 3; k++)
                        {
                            int i = (int)(Player.Center.X / 16) + Direction;
                            int j = (int)(Player.position.Y / 16) + k;
                            Player.PickTile(i, j, Item.pick);
                        }

                    if (Charge % 10 == 0) Terraria.Audio.SoundEngine.PlaySound(SoundID.Item63, Player.Center);

                    if (Charge <= 0) Whirling = false;
                }
            }

            if (!Main.mouseRight && Charge > 0 && !Whirling || Player.HeldItem != Item) Charge = 0;

            if (Main.mouseRight && Player.HeldItem == Item && Charge < 120) Charge++;
        }
    }
}
