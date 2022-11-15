//TODO:
//Obtainment

using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using StarlightRiver.NPCs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Snow
{
	public class MeadHorn : SmartAccessory
    {
        public override string Texture => AssetDirectory.SnowItem + Name;

		public MeadHorn() : base("Mead Horn", "Potions last 50% longer") { }

		public override void Load()
		{
			On.Terraria.Player.AddBuff += MeadhornBuff;
		}

		public override void Unload()
		{
			On.Terraria.Player.AddBuff -= MeadhornBuff;
		}

		private void MeadhornBuff(On.Terraria.Player.orig_AddBuff orig, Player self, int type, int timeToAdd, bool quiet, bool foodHack)
		{
			if (!Equipped(self) || Main.debuff[type])
			{
				orig(self, type, timeToAdd, quiet, foodHack);
				return;
			}
			orig(self, type, (int)(timeToAdd * 1.5f), quiet, foodHack);
		}

		public override void SafeSetDefaults()
        {
            Item.value = Item.sellPrice(0, 1, 0, 0);
            Item.rare = ItemRarityID.Green;
        }
    }
}