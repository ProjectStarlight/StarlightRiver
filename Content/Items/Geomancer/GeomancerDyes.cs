using Terraria.ID;

namespace StarlightRiver.Content.Items.Geomancer
{
	public class RainbowCycleDye : ModItem
	{
		public override string Texture => AssetDirectory.GeomancerItem + Name;
		public override void SetDefaults()
		{
			/*				
			this.name = "Gel Dye";
			this.width = 20;
			this.height = 20;
			this.maxStack = 99;
			this.value = Item.sellPrice(0, 1, 50, 0);
			this.rare = 3;
			*/
			// Item.dye is already assigned to this Item prior to SetDefaults because of the GameShaders.Armor.BindShader code in ExampleMod.Load. 
			// This code here remembers Item.dye so that information isn't lost during CloneDefaults. The above code is the data being cloned by CloneDefaults, for informational purposes.
			int dye = Item.dye;
			Item.CloneDefaults(ItemID.GelDye);
			Item.dye = dye;
		}
	}

	public class RainbowCycleDye2 : ModItem
	{
		public override string Texture => AssetDirectory.GeomancerItem + "RainbowCycleDye";
		public override void SetDefaults()
		{
			/*				
			this.name = "Gel Dye";
			this.width = 20;
			this.height = 20;
			this.maxStack = 99;
			this.value = Item.sellPrice(0, 1, 50, 0);
			this.rare = 3;
			*/
			// Item.dye is already assigned to this Item prior to SetDefaults because of the GameShaders.Armor.BindShader code in ExampleMod.Load. 
			// This code here remembers Item.dye so that information isn't lost during CloneDefaults. The above code is the data being cloned by CloneDefaults, for informational purposes.
			int dye = Item.dye;
			Item.CloneDefaults(ItemID.GelDye);
			Item.dye = dye;
		}
	}
}