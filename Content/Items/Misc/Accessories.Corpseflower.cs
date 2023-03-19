using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core.Systems.InstancedBuffSystem;
using System.Collections.Generic;

namespace StarlightRiver.Content.Items.Misc
{
	public class Corpseflower : CursedAccessory
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public Corpseflower() : base(ModContent.Request<Texture2D>(AssetDirectory.MiscItem + "Corpseflower").Value) { }

		public override void Load()
		{
			StarlightPlayer.ModifyHitNPCEvent += ApplyDoTItem;
			StarlightPlayer.ModifyHitNPCWithProjEvent += ApplyDoTProjectile;
		}

		private void ApplyDoTProjectile(Player player, Projectile proj, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			if (Equipped(player))
				BuffInflictor.InflictStack<CorpseflowerBuff, CorpseflowerStack>(target, 600, new CorpseflowerStack() { duration = 600, damage = (int)(damage * 0.33f) });
		}

		private void ApplyDoTItem(Player player, Item Item, NPC target, ref int damage, ref float knockback, ref bool crit)
		{
			if (Equipped(player))
				BuffInflictor.InflictStack<CorpseflowerBuff, CorpseflowerStack>(target, 600, new CorpseflowerStack() { duration = 600, damage = (int)(damage * 0.33f) });
		}

		public override void SetStaticDefaults()
		{
			Tooltip.SetDefault("All damage dealt is converted into damage over time\nDamage is decreased by 66%\nYou are unable to critically strike while equipped");
		}
	}

	class CorpseflowerStack : BuffStack
	{
		public int damage;
	}

	class CorpseflowerBuff : StackableBuff<CorpseflowerStack>
	{
		public int totalDamage;
		public override string Name => "CorpseflowerBuff";

		public override string DisplayName => "Corpseflowered";

		public override string Texture => AssetDirectory.MiscItem + Name;

		public override bool Debuff => true;

		public override string Tooltip => "ratio + L";

		public override void Load()
		{
			StarlightNPC.UpdateLifeRegenEvent += StarlightNPC_UpdateLifeRegenEvent;
			StarlightNPC.ResetEffectsEvent += ResetDamage;
		}

		private void StarlightNPC_UpdateLifeRegenEvent(NPC npc, ref int damage)
		{
			if (AnyInflicted(npc))
			{
				if (damage < (GetInstance(npc) as CorpseflowerBuff).totalDamage)
					damage = (GetInstance(npc) as CorpseflowerBuff).totalDamage;
			}
		}

		private void ResetDamage(NPC NPC)
		{
			if (AnyInflicted(NPC))
			{
				(GetInstance(NPC) as CorpseflowerBuff).totalDamage = 0;
			}
		}


		public override CorpseflowerStack GenerateDefaultStackTyped(int duration)
		{
			return new CorpseflowerStack()
			{
				duration = duration,
				damage = 1
			};
		}

		public override void PerStackEffectsNPC(NPC npc, CorpseflowerStack stack)
		{
			npc.lifeRegen -= stack.damage;
			totalDamage += stack.damage;
		}
	}
}
