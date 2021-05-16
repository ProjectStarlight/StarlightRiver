using Microsoft.Xna.Framework;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

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

	class SeaglassRingTile : ModTile
	{
		public override bool Autoload(ref string name, ref string texture)
		{
			texture = AssetDirectory.Assets + "Items/Beach/" + name;
			return base.Autoload(ref name, ref texture);
		}

		public override void SetDefaults()
		{
			QuickBlock.QuickSetFurniture(this, 1, 1, DustID.Iron, SoundID.Coins, true, new Color(150, 250, 250));
		}

		public override void NearbyEffects(int i, int j, bool closer)
		{
			if (Main.rand.Next(20) == 0)
				Dust.NewDust(new Vector2(i, j) * 16, 16, 16, 15, 0, 0, 0, default, 0.5f);
		}

		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
		{
			if (!fail)
				Item.NewItem(new Rectangle(i * 16, j * 16, 16, 16), ModContent.ItemType<SeaglassRing>());
		}
	}
}
