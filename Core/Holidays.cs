using System;

namespace StarlightRiver.Core
{
	class Holidays : IOrderedLoadable
	{
		public static bool AprilFirst = false;
		public static bool Anniversary = false;
		public static bool Halloween = false;
		public static bool Christmas = false;
		public static bool NewYears = false;

		public static bool AnySpecialEvent = false;

		public float Priority => 0.5f;

		public void Load()
		{
			AprilFirst = DateTime.Now.Month == 4 && DateTime.Now.Day == 1;

			Anniversary = DateTime.Now.Month == 8 && DateTime.Now.Day == 28;

			Halloween = DateTime.Now.Month == 10 && DateTime.Now.Day == 31;

			Christmas = DateTime.Now.Month == 12 && DateTime.Now.Day == 25;

			NewYears = DateTime.Now.Month == 12 && DateTime.Now.Day == 31;

			AnySpecialEvent = AprilFirst || Anniversary || Halloween || Christmas || NewYears;
		}

		public void Unload()
		{

		}
	}
}