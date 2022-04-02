using Microsoft.Xna.Framework;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.DataStructures;

namespace StarlightRiver.Content.Items.Misc
{
	class AlchemistShackles : CursedAccessory
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public AlchemistShackles() : base(ModContent.GetTexture(AssetDirectory.MiscItem + "CasualMirror")) { }

		public override bool Autoload(ref string name)
		{
			On.Terraria.Player.AddBuff += Player_AddBuff;
			return true;
		}

		
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Alchemist's Shackles");
			Tooltip.SetDefault("Mana and health potions are more effective when your health and mana are lower \nPotion sickness effects last 15 seconds longer");
		}

		public override void SafeUpdateEquip(Player player)
		{
			player.GetModPlayer<AlchemistShacklesPlayer>().equipped = true;
		}

		public static void Player_AddBuff(On.Terraria.Player.orig_AddBuff orig, Player self, int type, int time1, bool quiet = true)
        {
			if (self.GetModPlayer<AlchemistShacklesPlayer>().equipped && (type == BuffID.PotionSickness || type == BuffID.ManaSickness))
			{
				orig(self, type, time1 + 900, quiet);
			}
			else
				orig(self, type, time1, quiet);
		}
	}
	class AlchemistShacklesPlayer : ModPlayer
	{
		public bool equipped = false;

		public override void ResetEffects()
		{
			equipped = false;
		}
	}
	class AlchemistShackleGItem : GlobalItem
    {
        public override void GetHealLife(Item item, Player player, bool quickHeal, ref int healValue)
        {
			float mult = 2 - ((float)player.statLife / (float)player.statLifeMax2);
            if (player.GetModPlayer<AlchemistShacklesPlayer>().equipped)
            {
				healValue = (int)(healValue * mult);
            }
        }

        public override void GetHealMana(Item item, Player player, bool quickHeal, ref int healValue)
        {
			float mult = 2 - ((float)player.statMana / (float)player.statManaMax2);
			if (player.GetModPlayer<AlchemistShacklesPlayer>().equipped)
			{
				healValue = (int)(healValue * mult);
			}
		}
    }
}
