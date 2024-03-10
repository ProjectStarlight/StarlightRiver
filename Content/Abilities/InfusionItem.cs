using System;

namespace StarlightRiver.Content.Abilities
{
	public abstract partial class InfusionItem : ModItem
	{
		public Color color;

		public abstract InfusionTier Tier { get; }
		public virtual Type AbilityType { get; }

		public Player Player => Main.player[Item.playerIndexTheItemIsReservedFor];

		public virtual bool Equippable => true;

		public Ability Ability
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

		public virtual void OnActivate()
		{
			Ability?.OnActivate();
		}

		public virtual void UpdateActive()
		{
			Ability?.UpdateActive();
		}

		public virtual void UpdateActiveEffects()
		{
			Ability?.UpdateActiveEffects();
		}

		public virtual void UpdateFixed()
		{
			Ability?.UpdateFixed();
		}

		public virtual void OnExit()
		{
			Ability?.OnExit();
		}
	}

	public abstract class InfusionItem<T> : InfusionItem where T : Ability
	{
		public override Type AbilityType => typeof(T);
		public new T Ability => (T)base.Ability;
	}
}