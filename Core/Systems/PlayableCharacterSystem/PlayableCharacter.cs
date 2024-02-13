using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;

namespace StarlightRiver.Core.Systems.PlayableCharacterSystem
{
	public abstract class PlayableCharacter
	{
		/// <summary>
		/// The player object for this character. All characters are unique player instances.
		/// </summary>
		public Player player;

		/// <summary>
		///  The name of this character
		/// </summary>
		public virtual string Name => "Unnamed character";

		/// <summary>
		/// Called when the player instance for this character is initialized. Allows you to do things like
		/// set specific modded data like abillities
		/// </summary>
		public virtual void Setup() { }

		/// <summary>
		/// Effects which occur before all equipment updates. Can be used to apply things like
		/// stat bonuses or penalties to this character
		/// </summary>
		public virtual void PreUpdate() { }

		/// <summary>
		/// Effects which change the physics of this character, runs before vanilla physics.
		/// </summary>
		public virtual void UpdatePhysics() { }

		/// <summary>
		/// What should happen when the jump button is pressed for this character. Note you may need to manually
		/// add your own checks for things such as solid ground!
		/// </summary>
		/// <returns>If vanilla jump logic should execute</returns>
		public virtual bool OnJumpInput() { return true; }

		/// <summary>
		/// Allows the insertion of PlayerLayers to render this character with. Note all vanilla layers are removed
		/// by default.
		/// </summary>
		/// <param name="drawInfo">this character's drawInfo</param>
		public virtual void ModifyDrawInfo(ref PlayerDrawSet drawInfo) { }
	}
}
