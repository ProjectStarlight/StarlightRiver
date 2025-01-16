﻿using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Content.Items.Potions;
using StarlightRiver.Core.Systems.BarrierSystem;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Misc
{
	public class PandorasShield : SmartAccessory
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public PandorasShield() : base("Pandora's Shield", "When you {{Graze}} a projectile, gain a portion of the damage as {{barrier}}") { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.sellPrice(gold: 1);
		}

		public override void SafeUpdateEquip(Player Player)
		{
			GrazePlayer gp = Player.GetModPlayer<GrazePlayer>();
			gp.doGrazeLogic = true;

			if (gp.justGrazed)
			{
				float scale = 1f;
				if (Main.expertMode)
					scale = 2f;
				if (Main.masterMode)
					scale = 3f;

				int amount = (int)(gp.lastGrazeDamage * scale);
				amount = Utils.Clamp(amount, 0, 200);
				Player.AddBuff(ModContent.BuffType<ShieldDegenReduction>(), 180 + amount * 2);
				Player.GetModPlayer<BarrierPlayer>().barrier += amount;
				CombatText.NewText(Player.Hitbox, new Color(150, 255, 255), amount);
			}
		}
	}
}