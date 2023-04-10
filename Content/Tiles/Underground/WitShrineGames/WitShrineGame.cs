namespace StarlightRiver.Content.Tiles.Underground.WitShrineGames
{
	public abstract class WitShrineGame
	{
		protected WitShrineDummy parent;
		protected WitShrineDummy.runeState[,] gameBoard => parent.gameBoard;

		public WitShrineGame(WitShrineDummy parent)
		{
			this.parent = parent;
		}

		protected int Clamp(float input)
		{
			return (int)MathHelper.Clamp(input, 0, 5);
		}

		public virtual void SetupBoard()
		{

		}

		public virtual void UpdatePlayer(Vector2 Player, Vector2 oldPlayer)
		{

		}

		public virtual void UpdateBoard()
		{

		}
	}
}