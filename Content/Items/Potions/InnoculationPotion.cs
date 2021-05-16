using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using StarlightRiver.Core;
using Terraria;

namespace StarlightRiver.Content.Items.Potions
{
	class InnoculationPotion : ModItem
	{
		public override string Texture => AssetDirectory.PotionsItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Innoculation Potion");
			Tooltip.SetDefault("+30% DoT resistance");
		}

		public override void SetDefaults()
		{
			item.width = 18;
			item.height = 30;
			item.maxStack = 30;
			item.useStyle = Terraria.ID.ItemUseStyleID.EatingUsing;
			item.consumable = true;
			item.buffType = ModContent.BuffType<InnoculationPotionBuff>();
			item.buffTime = 180 * 60;
			item.UseSound = Terraria.ID.SoundID.Item3;
		}
	}

	class InnoculationPotionBuff : ModBuff
	{
		public override bool Autoload(ref string name, ref string texture)
		{
			texture = AssetDirectory.PotionsItem + name;
			return base.Autoload(ref name, ref texture);
		}

		public override void SetDefaults()
		{
			DisplayName.SetDefault("Innoculated");
			Description.SetDefault("+30% to DoT Resistance");
		}

		public override void Update(Player player, ref int buffIndex)
		{
			player.GetModPlayer<DoTResistancePlayer>().DoTResist += 0.3f;
		}
	}
}
