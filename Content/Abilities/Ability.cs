﻿using StarlightRiver.Content.Abilities.Faewhip;
using StarlightRiver.Content.Abilities.ForbiddenWinds;
using StarlightRiver.Content.Abilities.Hint;
using StarlightRiver.Content.Packets;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ID;

namespace StarlightRiver.Content.Abilities
{
	public abstract class Ability
	{
		protected Ability()
		{
			Reset();
		}

		public AbilityHandler User { get; internal set; }
		public Player Player => User.Player;
		public float ActivationCostBonus { get; set; }
		public bool Active => ReferenceEquals(User.ActiveAbility, this);

		public virtual string Name => "No name";
		public virtual string Tooltip => "No tooltip";
		public abstract string Texture { get; }
		public virtual float ActivationCostDefault { get; }
		public virtual string PreviewTexture => Texture + "Preview";
		public virtual string PreviewTextureOff => Texture + "PreviewOff";
		public virtual bool Available => User.ActiveAbility == null && User.Stamina >= ActivationCost(User);
		public virtual Color Color => Color.White;

		public abstract bool HotKeyMatch(TriggersSet triggers, AbilityHotkeys abilityKeys);
		public virtual void ModifyDrawInfo(ref PlayerDrawSet drawInfo) { }
		public virtual void Reset() { }
		/// <summary>
		/// for visual effects like dusts and gores. Only executed by clients.
		/// </summary>
		public virtual void UpdateActiveEffects() { }
		public virtual void DrawActiveEffects(SpriteBatch spriteBatch) { }
		public virtual void UpdateFixed() { }

		public virtual void OnActivate() { }
		public virtual void OnDeactivate() { }
		public virtual void UpdateActive() { }
		public virtual void OnExit() { }

		public float ActivationCost(AbilityHandler handler)
		{
			return (ActivationCostDefault + ActivationCostBonus) * handler.StaminaCostMultiplier + handler.StaminaCostBonus;
		}

		public void Activate(AbilityHandler user)
		{
			user.ActiveAbility = this;

			var packet = new StartAbility(user.Player.whoAmI, this);
			packet.Send(-1, -1, false);

			NetMessage.SendData(MessageID.PlayerControls);
		}

		public void Deactivate()
		{
			OnDeactivate();
			User.ActiveAbility = null;
		}

		// TODO load, cache, and unload. super ugly rn
		public static Ability[] GetAbilityInstances()
		{
			return new Ability[]
			{
				new HintAbility(),
				new Dash(),
				new Whip(),
			};
		}
	}
}