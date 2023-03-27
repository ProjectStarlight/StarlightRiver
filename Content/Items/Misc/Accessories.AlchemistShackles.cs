using StarlightRiver.Content.Items.BaseTypes;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Misc
{
	class AlchemistShackles : CursedAccessory
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public AlchemistShackles() : base(ModContent.Request<Texture2D>(AssetDirectory.MiscItem + "CasualMirror").Value) { }

		public override void Load()
		{
			On.Terraria.Player.AddBuff += Player_AddBuff;
			StarlightItem.GetHealLifeEvent += AlchemistLifeModify;
			StarlightItem.GetHealManaEvent += AlchemistManaModify;
		}

		public override void Unload()
		{
			On.Terraria.Player.AddBuff -= Player_AddBuff;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Alchemist's Shackles");
			Tooltip.SetDefault("Mana and health potions are more effective when your health and mana are lower \nCursed : Potion sickness effects last 15 seconds longer");
		}

		public static void Player_AddBuff(On.Terraria.Player.orig_AddBuff orig, Player self, int type, int time1, bool quiet = true, bool foodHack = false)
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