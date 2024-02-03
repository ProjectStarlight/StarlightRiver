using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

namespace StarlightRiver.Core
{
	class DoTResistancePlayer : ModPlayer
	{
		public float DoTResist = 0;

		public override void Load()
		{
			IL_Player.UpdateLifeRegen += InsertResist;
		}

		private void InsertResist(ILContext il)
		{
			ILCursor c = new(il);

			c.TryGotoNext(MoveType.After, n => n.MatchCall(typeof(PlayerLoader), "UpdateBadLifeRegen"));
			c.Emit(OpCodes.Ldarg, 0);
			c.EmitDelegate<Action<Player>>(ReduceDoT);
		}

		public static void ReduceDoT(Player Player)
		{
			if (Player.lifeRegen < 0)
				Player.lifeRegen = (int)(Player.lifeRegen * (1.0f - Player.GetModPlayer<DoTResistancePlayer>().DoTResist));
		}

		public override void ResetEffects()
		{
			DoTResist = 0;
		}
	}
}