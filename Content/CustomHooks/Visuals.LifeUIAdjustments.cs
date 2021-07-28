using MonoMod.Cil;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace StarlightRiver.Content.CustomHooks
{
	class LifeUIAdjustments : HookGroup
	{
		public override SafetyLevel Safety => SafetyLevel.Questionable; //Adjusts a few measurements for the vanilla health UI

		public override void Load()
		{
			if (Main.dedServ)
				return;

			IL.Terraria.Main.DrawInterface_Resources_Life += ShiftText;
		}

		private void ShiftText(ILContext il)
		{
			ILCursor c = new ILCursor(il);

			for (int k = 0; k < 2; k++)
			{
				c.TryGotoNext(n => n.MatchLdcI4(500));
				c.Index++;
				c.EmitDelegate<Func<int, int>>(stringConcatDelegate);
			}
		}

		private int stringConcatDelegate(int arg)
		{
			Player player = Main.LocalPlayer;
			var sp = player.GetModPlayer<ShieldPlayer>();

			if (sp.Shield <= 0 && sp.MaxShield <= 0)
				return arg;

			return arg - (int)(Main.fontMouseText.MeasureString($"  {sp.Shield}/{sp.MaxShield}").X / 2) - 6;
		}
	}
}
