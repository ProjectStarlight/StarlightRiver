namespace StarlightRiver.Content.Abilities
{
	/// <summary>
	/// To be implemented by NPCs, Projectiles, and Tiles which should give hints when the hint ability is used
	/// </summary>
	internal interface IHintable
	{
		/// <summary>
		/// What should display for the hint for this object
		/// </summary>
		/// <returns>The hint to be displayed</returns>
		public string GetHint();
	}
}
