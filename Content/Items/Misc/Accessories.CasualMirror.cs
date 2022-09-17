using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.DataStructures;

namespace StarlightRiver.Content.Items.Misc
{
	class CasualMirror : CursedAccessory
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public CasualMirror() : base(ModContent.Request<Texture2D>(AssetDirectory.MiscItem + "CasualMirror").Value) { }

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Casual Mirror");
			Tooltip.SetDefault("Regenerate life when you would take damage-over-time.\nCursed : take damage-over-time when you would regenerate life.\n This includes natural regeneration");
		}

		public override void SafeUpdateEquip(Player Player)
		{
			Player.GetModPlayer<CasualMirrorPlayer>().equipped = true;
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
				Player.lifeRegen *= -1;
			}
		}
		public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
		{
			if (equipped && hitDirection == 0 && damageSource.SourceOtherIndex == 8)
			{
				damageSource = PlayerDeathReason.ByCustomReason(Player.name + " didn't read the tooltip");
			}
			return true;
		}
	}
}
