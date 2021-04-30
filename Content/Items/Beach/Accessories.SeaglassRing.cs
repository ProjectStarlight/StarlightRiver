using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Beach
{
	class SeaglassRing : SmartAccessory
	{
		public override string Texture => AssetDirectory.Assets + "Items/Beach/" + Name;

		public SeaglassRing() : base("Seaglass Ring", "+10 Maximum Barrier\nBarrier recharge starts slightly faster") { }

		public override void SafeSetDefaults()
		{
			item.rare = ItemRarityID.Blue;
		}

		public override void SafeUpdateEquip(Player player)
		{
			var mp = player.GetModPlayer<ShieldPlayer>();

			mp.RechargeDelay -= 30;
			mp.MaxShield += 10;
		}
	}
}
