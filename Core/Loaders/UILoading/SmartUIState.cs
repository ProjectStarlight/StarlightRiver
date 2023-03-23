using System.Collections.Generic;
using Terraria.UI;

namespace StarlightRiver.Core.Loaders.UILoading
{
	/// <summary>
	/// An auto-loaded UI State, that knows information about its own visibility.
	/// </summary>
	public abstract class SmartUIState : UIState
	{
		/// <summary>
		/// The UserInterface automatically assigned to this UIState on load.
		/// </summary>
		protected internal virtual UserInterface UserInterface { get; set; }

		/// <summary>
		/// Where this UI state should be inserted relative to the vanilla UI layers.
		/// </summary>
		/// <param name="layers">The vanilla UI layers</param>
		/// <returns>The insertion index of this UI state</returns>
		public abstract int InsertionIndex(List<GameInterfaceLayer> layers);

		/// <summary>
		/// If the UI should be visible and interactable or not
		/// </summary>
		public virtual bool Visible { get; set; } = false;

		/// <summary>
		/// What scale setting this UI should scale with
		/// </summary>
		public virtual InterfaceScaleType Scale { get; set; } = InterfaceScaleType.UI;

		/// <summary>
		/// Allows you to unload anything that might need to be unloaded
		/// </summary>
		public virtual void Unload() { }
	}
}
