namespace StarlightRiver.Core.Systems.CutsceneSystem
{
	internal abstract class Cutscene
	{
		public int timer = 0;

		/// <summary>
		/// The effects that should occur during this cutscene. You can use the timer field to
		/// track basic time or create your own additional timers as neccisary.
		/// </summary>
		/// <param name="player">The player in the cutscene</param>
		public abstract void InCutscene(Player player);

		/// <summary>
		/// Cleanup effects to happen when your cutscene is dismissed, for things like returning
		/// the camera or disabling effects.
		/// </summary>
		/// <param name="player">The player who's cutscene is ending</param>
		public virtual void EndCutscene(Player player) { }

		public void UpdateCutscene(Player player)
		{
			timer++;
			InCutscene(player);
		}
	}
}