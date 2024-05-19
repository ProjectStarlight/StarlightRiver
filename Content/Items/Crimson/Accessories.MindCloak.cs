using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Content.Items.Vitric;
using StarlightRiver.Core.Systems.BarrierSystem;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Crimson
{
	internal class MindCloak : SmartAccessory
	{
		public int lastMana;

		public override string Texture => AssetDirectory.CrimsonItem + Name;

		public MindCloak() : base("Cloak of the Mind", "Increases to maximum mana also apply to maximum {{barrier}}") { }

		public override void Load()
		{
			StarlightPlayer.PostUpdateEquipsEvent += RecordMana;
		}

		public override void SafeSetDefaults()
		{
			Item.expert = true;
			Item.rare = ItemRarityID.Expert;
			Item.accessory = true;
			Item.width = 32;
			Item.height = 32;

			Item.value = Item.sellPrice(gold: 2);
		}

		private void RecordMana(StarlightPlayer Player)
		{
			if (Equipped(Player.Player))
				(GetEquippedInstance(Player.Player) as MindCloak).lastMana = Player.Player.statManaMax2 - Player.Player.statManaMax;
		}

		public override void SafeUpdateEquip(Player Player)
		{
			Player.GetModPlayer<BarrierPlayer>().maxBarrier += lastMana;
		}
	}
}
