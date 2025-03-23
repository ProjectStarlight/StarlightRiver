﻿using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Items.BaseTypes;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Misc
{
	internal class HungryStomach : CursedAccessory
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public override void SetStaticDefaults()
		{
			Tooltip.SetDefault("Restore {{Starlight}} by damaging foes\nMelee weapons restore twice as much\nDisables natural {{Starlight}} regeneration");
			DisplayName.SetDefault("Hungry Stomach");
		}

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.Red;
			Item.value = Item.sellPrice(gold: 2);
		}

		public override void Load()
		{
			StarlightPlayer.PostUpdateEquipsEvent += DisableRegen;
			StarlightPlayer.OnHitNPCEvent += LeechStaminaMelee;
			StarlightProjectile.OnHitNPCEvent += LeechStaminaRanged;
			StarlightNPC.ModifyNPCLootEvent += DropFromDeerclops;
			StarlightItem.ModifyItemLootEvent += DropFromDeerclopsBag;
		}

		private void DropFromDeerclopsBag(Item item, ItemLoot itemLoot)
		{
			if (item.type == ItemID.DeerclopsBossBag)
				itemLoot.Add(ItemDropRule.Common(Type, 2)); //drop 50% of the time
		}

		private void DropFromDeerclops(NPC npc, NPCLoot npcloot)
		{
			if (npc.type == NPCID.Deerclops)
				npcloot.Add(ItemDropRule.ByCondition(new Conditions.NotExpert(), Type, 2)); //drop 50% of the time
		}

		public override void SafeUpdateAccessory(Player Player, bool hideVisual)
		{
			GUI.StaminaBar.overrideTexture = Assets.GUI.StaminaBlood.Value;
		}

		private void DisableRegen(StarlightPlayer Player)
		{
			if (Equipped(Player.Player))
				Player.Player.GetHandler().StaminaRegenRate = 0;
		}

		private void LeechStaminaRanged(Projectile Projectile, NPC target, NPC.HitInfo info, int damageDone)
		{
			if (Equipped(Main.player[Projectile.owner]))
				Main.player[Projectile.owner].GetHandler().Stamina += damageDone / (Projectile.DamageType == DamageClass.Melee ? 100f : 200f);
		}

		private void LeechStaminaMelee(Player Player, Item Item, NPC target, NPC.HitInfo info, int damageDone)
		{
			if (Equipped(Player))
				Player.GetHandler().Stamina += damageDone / 100f;
		}
	}
}