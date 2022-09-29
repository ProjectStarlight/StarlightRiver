using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

		public AlchemistShackles() : base(ModContent.Request<Texture2D>(AssetDirectory.MiscItem + "CasualMirror").Value) { }

		public override void Load()
		{
			On.Terraria.Player.AddBuff += Player_AddBuff;
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

		public override void SafeUpdateEquip(Player Player)
		{
			Player.GetModPlayer<AlchemistShacklesPlayer>().equipped = true;
		}

		public static void Player_AddBuff(On.Terraria.Player.orig_AddBuff orig, Player self, int type, int time1, bool quiet = true, bool foodHack = false)
        {
			if (self.GetModPlayer<AlchemistShacklesPlayer>().equipped && (type == BuffID.PotionSickness || type == BuffID.ManaSickness))
			{
				orig(self, type, time1 + 900, quiet, foodHack);
			}
			else
				orig(self, type, time1, quiet, foodHack);
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
        public override void GetHealLife(Item Item, Player Player, bool quickHeal, ref int healValue)
        {
			float mult = 2 - ((float)Player.statLife / (float)Player.statLifeMax2);
            if (Player.GetModPlayer<AlchemistShacklesPlayer>().equipped)
            {
				healValue = (int)(healValue * mult);
            }
        }

        public override void GetHealMana(Item Item, Player Player, bool quickHeal, ref int healValue)
        {
			float mult = 2 - ((float)Player.statMana / (float)Player.statManaMax2);
			if (Player.GetModPlayer<AlchemistShacklesPlayer>().equipped)
			{
				healValue = (int)(healValue * mult);
			}
		}
    }
}
