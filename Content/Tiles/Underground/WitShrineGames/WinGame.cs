﻿namespace StarlightRiver.Content.Tiles.Underground.WitShrineGames
{
	class WinGame : WitShrineGame
	{
		public WinGame(WitShrineDummy parent) : base(parent) { }

		public override void SetupBoard()
		{
			parent.ResetBoard();
		}

		public override void UpdateBoard()
		{
			for (int k = 0; k <= 6; k++)
			{
				if (parent.timer == 30 * k)
				{
					for (int x = -k; x <= k; x++)
					{
						for (int y = -k; y <= k; y++)
						{
							int realX = Clamp(parent.Player.X + x);
							int realY = Clamp(parent.Player.Y + y);

							gameBoard[realX, realY] = WitShrineDummy.runeState.Goal;
						}
					}

					parent.PlayerTimer = 30;
				}
			}

			if (parent.timer == 210)
				parent.WinGame();
		}
	}
}