using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace StarlightRiver.Content.Items.StarJuice
{
	internal class StarLamp : StarjuiceStoringItem
    {
        public StarLamp() : base(500) { }

        public override string Texture => "StarlightRiver/Assets/Items/Starjuice/StarLamp";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Starlight Illuminator");
            Tooltip.SetDefault("Consumes starlight to produce light");
        }

        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 16;
            Item.useStyle = ItemUseStyleID.SwingThrow;
            Item.useAnimation = 10;
            Item.useTime = 10;
            Item.rare = ItemRarityID.Green;
        }

        public override void UpdateInventory(Player Player)
        {
            if (charge > 0)
            {
                if (Main.time % 60 == 0 && !Main.fastForwardTime) charge--;
                Lighting.AddLight(Player.Center, new Vector3(1f, 1.7f, 1.9f) * (charge / (float)maxCharge) * (Player.HeldItem == Item ? 0.6f : 0.4f));
            }
        }

        public override void PostUpdate()
        {
            if (charge > 0)
            {
                if (Main.time % 60 == 0 && !Main.fastForwardTime) charge--;
                Lighting.AddLight(Item.Center, new Vector3(1f, 1.7f, 1.9f) * (charge / (float)maxCharge) * 0.4f);
            }
        }
    }
}