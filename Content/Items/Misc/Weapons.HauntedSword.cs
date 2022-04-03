using StarlightRiver.Core;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Misc
{
	class HauntedSword : ModItem
    {
        public override string Texture => AssetDirectory.MiscItem + Name;
        public override void Load() => false; //TODO: implement

        //private int charge = 0;//unused

        public override void SetStaticDefaults() => DisplayName.SetDefault("Haunted Greatsword");

        public override void SetDefaults()
        {
            Item.damage = 20;
            Item.summon = true;
            Item.width = 40;
            Item.height = 20;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.noMelee = true;
            Item.knockBack = 2;
            Item.rare = ItemRarityID.Green;
            Item.channel = true;
            Item.noUseGraphic = true;
        }
    }
}
