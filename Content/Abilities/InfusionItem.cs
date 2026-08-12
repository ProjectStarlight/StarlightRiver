using System;

namespace StarlightRiver.Content.Abilities
{
	public abstract partial class InfusionItem : ModItem
	{
		public Color color;
		public Ability ability;

		public abstract InfusionTier Tier { get; }
		public virtual Type AbilityType { get; }

		public Player Player => Main.player[Item.playerIndexTheItemIsReservedFor];

		public virtual bool Equippable => true;

		public Ability BaseAbility
		{
			get
			{
				// If this ability doesn't have a type, don't try getting its Ability
				if (Main.gameMenu || AbilityType == null)
					return null;

				if (Player.GetHandler().GetAbility(AbilityType, out Ability ability))
					return ability;

				return null;
			}
		}
	}

	public abstract class InfusionItem<T1, T2> : InfusionItem where T1 : Ability where T2 : T1
	{
		public override Type AbilityType => typeof(T1);

		public InfusionItem()
		{
			ability = Activator.CreateInstance<T2>();
		}
	}
}