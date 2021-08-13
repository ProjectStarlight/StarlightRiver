using MonoMod.RuntimeDetour;
using System;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Core
{
	class DoTResistancePlayer : ModPlayer
	{
		public float DoTResist = 0;

		public override bool Autoload(ref string name)
		{
			MonoModHooks.RequestNativeAccess();

			IDetour d = new Hook(typeof(PlayerHooks).GetMethod("UpdateBadLifeRegen"), typeof(DoTResistancePlayer).GetMethod("ReduceDoT"));
			d.Apply();

			return base.Autoload(ref name);
		}

		public static void ReduceDoT(Action<Player> orig, Player player)
		{
			orig(player);
			player.lifeRegen = (int)(player.lifeRegen * (1.0f - player.GetModPlayer<DoTResistancePlayer>().DoTResist) );
		}

		public override void ResetEffects()
		{
			DoTResist = 0;
		}
	}
}
