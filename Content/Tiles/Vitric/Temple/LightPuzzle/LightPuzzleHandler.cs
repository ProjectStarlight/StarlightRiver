namespace StarlightRiver.Content.Tiles.Vitric.Temple.LightPuzzle
{
	class LightPuzzleHandler : ModSystem
	{
		public static int solvedPoints;

		public static bool Solved => solvedPoints >= 1;
	}
}