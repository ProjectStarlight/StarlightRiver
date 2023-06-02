namespace StarlightRiver.Content.Tiles.Underground.WitShrineGames
{
	class LoseGame : WitShrineGame
	{
		public LoseGame(WitShrineDummy parent) : base(parent) { }

		public override void SetupBoard()
		{
			parent.ResetBoard();
		}

		public override void UpdateBoard()
		{
			for (int k = 0; k <= 6; k++)
			{
				if (parent.Timer == 30 * k)
				{
					for (int x = -k; x <= k; x++)
					{
						for (int y = -k; y <= k; y++)
						{
							int realX = Clamp(parent.Player.X + x);
							int realY = Clamp(parent.Player.Y + y);

							gameBoard[realX, realY] = WitShrineDummy.runeState.Hostile;
						}
					}

					parent.PlayerTimer = 30;
				}
			}

			if (parent.Timer == 210)
				parent.GotoGame(0);
		}
	}
}