using Terraria.ID;
using Terraria.ModLoader;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Items.Misc
{
    class HauntedSword : ModItem
    {
        public override string Texture => AssetDirectory.MiscItem + Name;
        public override bool Autoload(ref string name) => false; //TODO: implement

        //private int charge = 0;//unused

        public override void SetStaticDefaults() => DisplayName.SetDefault("Haunted Greatsword");

        public override void SetDefaults()
        {
            item.damage = 20;
            item.summon = true;
            item.width = 40;
            item.height = 20;
            item.useTime = 25;
            item.useAnimation = 25;
            item.useStyle = ItemUseStyleID.HoldingUp;
            item.noMelee = true;
            item.knockBack = 2;
            item.rare = ItemRarityID.Green;
            item.channel = true;
            item.noUseGraphic = true;
        }
    }
}
