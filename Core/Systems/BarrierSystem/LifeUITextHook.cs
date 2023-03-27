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

			Terraria.GameContent.UI.ResourceSets.IL_ClassicPlayerResourcesDisplaySet.DrawLife += ShiftText;
			Terraria.GameContent.UI.ResourceSets.On_CommonResourceBarMethods.DrawLifeMouseOver += DrawBarrierMouseOver;
		}

		/// <summary>
		/// Replaces default health bar mouse over text with one that includes barrier text
		/// </summary>
		/// <param name="orig"></param>
		private void DrawBarrierMouseOver(Terraria.GameContent.UI.ResourceSets.On_CommonResourceBarMethods.orig_DrawLifeMouseOver orig)
		{
			Player localPlayer = Main.LocalPlayer;
			BarrierPlayer barrierPlayer = localPlayer.GetModPlayer<BarrierPlayer>();

			if (barrierPlayer.maxBarrier <= 0)
			{
				orig.Invoke();
			}
			else if (!Main.mouseText)
			{
				localPlayer.cursorItemIconEnabled = false;
				string text = localPlayer.statLife + "/" + localPlayer.statLifeMax2;
				text += "\n[c/64c8ff:" + barrierPlayer.barrier + "/" + barrierPlayer.maxBarrier + "]";
				Main.instance.MouseTextHackZoom(text);
				Main.mouseText = true;
			}
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