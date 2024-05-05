using StarlightRiver.Content.Items.BaseTypes;
using System;
using System.Linq;

namespace StarlightRiver.Content.Items.Manabonds
{
	/// <summary>
	/// An abstract class for which to build manabond accessories off of
	/// </summary>
	internal abstract class Manabond : SmartAccessory
	{
		public Manabond(string name, string tooltip) : base(name, tooltip) { }

		/// <summary>
		/// The behavior that the minion should follow while effected by this Manabond.
		/// </summary>
		/// <param name="minion">The minion to effect the behavior of</param>
		/// <param name="mp">The ManabondProjectile on this minion</param>
		public abstract void MinionAI(Projectile minion, ManabondProjectile mp);

		/// <summary>
		/// Used to find the target that a given minion should use for it's magical attack target.
		/// By default this checks if an NPC is active, hostile, can be targeted, and has LoS from the minion.
		/// </summary>
		/// <param name="minion">The minion to find the target for</param>
		/// <param name="mp">The ManabondProjectile of this minion</param>
		/// <returns>The desired target, or null if no valid targets</returns>
		public virtual NPC FindTarget(Projectile minion, ManabondProjectile mp)
		{
			Player owner = Main.player[minion.owner];
			int range = 800;

			// Check player's target first, and prioritize that if possible
			if (owner?.HasMinionAttackTargetNPC ?? false)
			{
				NPC npc = Main.npc[owner.MinionAttackTargetNPC];

				if (Vector2.Distance(minion.Center, npc.Center) <= range && Collision.CanHit(minion.Center, 1, 1, npc.Center, 1, 1))
					return npc;
			}

			// Else, scan for other valid NPCs
			foreach (NPC npc in Main.npc)
			{
				if (!npc.active || npc.friendly || !npc.CanBeChasedBy(this))
					continue;

				if (Vector2.Distance(minion.Center, npc.Center) <= range && Collision.CanHit(minion.Center, 1, 1, npc.Center, 1, 1))
					return npc;
			}

			return null;
		}

		public override bool SafeCanAccessoryBeEquippedWith(Item equipped, Item incoming, Player player)
		{
			if (equipped.ModItem is Manabond && incoming.ModItem is Manabond)
				return false;

			return true;
		}
	}

	/// <summary>
	/// This GlobalProjectile handles the mana system and AI for minions under the effect of a manabond accessory.
	/// This should only ever do anything if a minion's owner has a valid manabond equipped.
	/// </summary>
	internal class ManabondProjectile : GlobalProjectile
	{
		public int mana;
		public int maxMana = 40;

		public int timer = 0;

		public NPC target;

		public override bool InstancePerEntity => true;

		public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
		{
			return entity.minion;
		}

		/// <summary>
		/// Determines if this minion's owner has a manabond on, and thus should have their mana siphoned
		/// </summary>
		/// <param name="projectile">The minion to check the owner's status of</param>
		/// <returns>If the minions owner has a manabond equipped</returns>
		public bool ManaActive(Projectile projectile)
		{
			Player owner = Main.player[projectile.owner];

			return owner.GetModPlayer<AccessoryPlayer>().standardAccessories.Any(n => n.ModItem is Manabond) ||
				owner.GetModPlayer<AccessoryPlayer>().simulatedAccessories.Any(n => n.ModItem is Manabond); // Checks if the player has any manabond on
		}

		/// <summary>
		/// Retrieves the currently equipped manabond of the given projectile's owner, or null if one is not equipped
		/// </summary>
		/// <param name="projectile">The minion to check the owner's manabond of</param>
		/// <returns>The equipped manabond if one is equipped, or null if not</returns>
		public Manabond GetActiveBond(Projectile projectile)
		{
			if (!ManaActive(projectile))
				return null;

			Player owner = Main.player[projectile.owner];

			return owner.GetModPlayer<AccessoryPlayer>().standardAccessories.FirstOrDefault(n => n.ModItem is Manabond)?.ModItem as Manabond ??
				owner.GetModPlayer<AccessoryPlayer>().simulatedAccessories.FirstOrDefault(n => n.ModItem is Manabond)?.ModItem as Manabond;
		}

		public override void PostAI(Projectile projectile)
		{
			if (!ManaActive(projectile))
				return;

			Manabond bond = GetActiveBond(projectile);
			Player owner = Main.player[projectile.owner];

			timer++;

			// Drop target attempt
			if (target != null && (!target.active || Vector2.Distance(projectile.Center, target.Center) > 800))
				target = null;

			// Retarget attempt
			target ??= bond?.FindTarget(projectile, this);

			// Mana siphon attempt
			if (timer % 5 == 0 && owner.statMana >= 5)
			{
				int toSiphon = Math.Min(1, maxMana - mana);

				if (owner.CheckMana(toSiphon, false))
				{
					owner.statMana -= toSiphon;
					mana += toSiphon;
				}

				if (toSiphon > 0 && owner.manaRegenDelay < 15)
					owner.manaRegenDelay = 15;
			}

			// hand off to bond AI
			bond?.MinionAI(projectile, this);
		}

		public override void PostDraw(Projectile projectile, Color lightColor)
		{
			if (!ManaActive(projectile) || Main.myPlayer != projectile.owner)
				return;

			float fill = mana / (float)maxMana;

			Texture2D tex = Assets.GUI.SmallBar1.Value;
			Texture2D tex2 = Assets.GUI.SmallBar0.Value;

			var pos = (projectile.Center + new Vector2(-tex.Width / 2, -40) + Vector2.UnitY * projectile.height / 2f - Main.screenPosition).ToPoint();
			var target = new Rectangle(pos.X, pos.Y, (int)(fill * tex.Width), tex.Height);
			var source = new Rectangle(0, 0, (int)(fill * tex.Width), tex.Height);
			var target2 = new Rectangle(pos.X, pos.Y + 2, tex2.Width, tex2.Height);
			var color = Vector3.Lerp(Color.Purple.ToVector3(), Color.Aqua.ToVector3(), fill);

			Main.spriteBatch.Draw(tex2, target2, new Color(40, 40, 40));
			Main.spriteBatch.Draw(tex, target, source, new Color(color.X, color.Y, color.Z));
		}
	}
}