using StarlightRiver.Content.Items.BaseTypes;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Misc
{
	class CasualMirror : CursedAccessory
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public CasualMirror() : base(ModContent.Request<Texture2D>(AssetDirectory.MiscItem + "CasualMirror").Value) { }

		public override void Load()
		{
			StarlightItem.CanUseItemEvent += TurnMirrorIntoItem;
		}

		private bool TurnMirrorIntoItem(Item Item, Player Player)
		{
			List<int> mirrors = new() { ItemID.IceMirror, ItemID.MagicMirror };

			if (mirrors.Contains(Item.type) && Main.npc.Any(n => n.active && (n.type == NPCID.WallofFlesh || n.type == NPCID.WallofFleshEye)))
			{
				Item.TurnToAir();

				Player.QuickSpawnItem(Item.GetSource_Misc("Transform"), Type);
				return false;
			}

			return true;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Causal Mirror");
			Tooltip.SetDefault("Cursed : Your DoT and regeneration effects are inverted.\nRegenerate life when you would take damage-over-time.\nTake damage-over-time when you would regenerate life.\nThis includes natural regeneration!");
		}

		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(gold: 2);
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
				damageSource = PlayerDeathReason.ByCustomReason(Player.name + " didn't read the tooltip");

			return true;
		}
	}
}