using MonoMod.Cil;
using StarlightRiver.Content.CustomHooks;

namespace StarlightRiver.Core.Systems.BarrierSystem
{
	class LifeUITextHook : HookGroup
	{
		//Adjusts a few measurements for the vanilla health UI
		public override void Load()
		{
			if (Main.dedServ)
				return;

			IL.Terraria.GameContent.UI.ResourceSets.ClassicPlayerResourcesDisplaySet.DrawLife += ShiftText;
		}

		private void ShiftText(ILContext il)
		{
			var c = new ILCursor(il);

			for (int k = 0; k < 2; k++)
			{
				c.TryGotoNext(n => n.MatchLdcI4(500));
				c.Index++;
				c.EmitDelegate(StringConcat);
			}
		}

		private int StringConcat(int arg)
		{
			Player Player = Main.LocalPlayer;
			BarrierPlayer sp = Player.GetModPlayer<BarrierPlayer>();

			if (sp.barrier <= 0 && sp.maxBarrier <= 0)
				return arg;

			return arg - (int)(Terraria.GameContent.FontAssets.MouseText.Value.MeasureString($"  {sp.barrier}/{sp.maxBarrier}").X / 2) - 6;
		}
	}
}
