using Microsoft.Xna.Framework;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Misc
{
	class CasualMirror : CursedAccessory
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public CasualMirror() : base(ModContent.GetTexture(AssetDirectory.MiscItem + "CasualMirror")) { }

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Casual Mirror");
			Tooltip.SetDefault("Regeneration and damage over time are swapped \nThis includes natural regeneration");
		}

		public override void SafeUpdateEquip(Player player)
		{
			player.GetModPlayer<CasualMirrorPlayer>().equipped = true;
		}
	}

	class CasualMirrorPlayer : ModPlayer
    {
		public bool equipped = false;

        public override void ResetEffects()
        {
			equipped = false;
        }

		public override void NaturalLifeRegen(ref float regen)
		{
			if (equipped)
			{
				regen *= -1f;
				player.lifeRegen *= -1;
			}
		}
	}
}
