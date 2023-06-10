using StarlightRiver.Content.Items.BaseTypes;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Misc
{
	class AlchemistShackles : CursedAccessory
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public AlchemistShackles() : base(ModContent.Request<Texture2D>(AssetDirectory.MiscItem + "AlchemistShackles").Value) { }

		public override void Load()
		{
			On_Player.AddBuff += Player_AddBuff;
			StarlightItem.GetHealLifeEvent += AlchemistLifeModify;
			StarlightItem.GetHealManaEvent += AlchemistManaModify;
		}

		public override void Unload()
		{
			On_Player.AddBuff -= Player_AddBuff;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Alchemist's Shackles");
			Tooltip.SetDefault("Resource potions are more effective when their respective resource is lower \nCursed : Potion sickness effects last 15 seconds longer");
		}

		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(silver: 50);
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.Shackle, 2);
			recipe.AddIngredient(ItemID.LesserHealingPotion, 2);
			recipe.AddIngredient(ItemID.LesserManaPotion, 2);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}

		public static void Player_AddBuff(On_Player.orig_AddBuff orig, Player self, int type, int time1, bool quiet = true, bool foodHack = false)
		{
			SmartAccessory instance = GetEquippedInstance(self, ModContent.ItemType<AlchemistShackles>());

			if (instance != null && instance.Equipped(self) && (type == BuffID.PotionSickness || type == BuffID.ManaSickness))
				orig(self, type, time1 + 900, quiet, foodHack);
			else
				orig(self, type, time1, quiet, foodHack);
		}

		private void AlchemistLifeModify(Item Item, Player player, bool quickHeal, ref int healValue)
		{
			if (Equipped(player))
			{
				float mult = 2 - player.statLife / (float)player.statLifeMax2;
				healValue = (int)(healValue * mult);
			}
		}

		private void AlchemistManaModify(Item item, Player player, bool quickHeal, ref int healValue)
		{
			if (Equipped(player))
			{
				float mult = 2 - player.statMana / (float)player.statManaMax2;
				healValue = (int)(healValue * mult);
			}
		}
	}
}