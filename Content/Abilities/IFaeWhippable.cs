using Microsoft.Xna.Framework;
using StarlightRiver.Content.Abilities.Faewhip;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Content.Abilities
{
	public interface IFaeWhippable
	{
		/// <summary>
		/// Runs every frame while the whip is attached to this object. Use this to make effects such as pulling.
		/// </summary>
		/// <param name="whip">The whip connected to this object</param>
		public void UpdateWhileWhipped(Whip whip) { }

		/// <summary>
		/// Called when the whip first attaches to this object
		/// </summary>
		/// <param name="whip">The whip connecting to this object</param>
		public void OnAttach(Whip whip) { }

		/// <summary>
		/// Called when the player stops using the whip while it was attached to this object
		/// </summary>
		/// <param name="whip">The whip previously connected to this object</param>
		public void OnRelease(Whip whip) { }

		/// <summary>
		/// The condition for the whip to forcibly disconnect from this object. Common cases include an NPC or projectile dying,
		/// an object losing its whippable properties, or a timer expiring.
		/// </summary>
		/// <returns>If the whip should forcibly detach or not</returns>
		public bool DetachCondition();

		/// <summary>
		/// The condition for a whip to stop and bind to this object. By default using IFaeWhippable disables
		/// the default hitbox colission for NPCs, so be sure to re-add it here if that is the desired colission
		/// logic.
		/// </summary>
		/// <param name="whipPosition">The position of the "tip" of the whip</param>
		/// <returns>If the whip should grab this object or not</returns>
		public bool IsWhipColliding(Vector2 whipPosition);

		/// <summary>
		/// Should only be implemented on ModNPCs. Determines if the default hitbox colission and grabbing should be used.
		/// </summary>
		/// <returns>If the default colission and binding behavior for NPCs should apply to this NPC.</returns>
		public bool NormalNPCInteraction() => false;
	}
}
