using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Content.CustomHooks
{
	internal class Screenspace : HookGroup
	{
		public static event Action<SpriteBatch> DrawScreenspaceEvent;

		public override void Load()
		{
			IL_Main.DoDraw += ScreenspaceHook;
		}

		private void ScreenspaceHook(ILContext il)
		{
			ILCursor c = new(il);

			c.TryGotoNext(n => n.MatchLdcI4(36), n => n.MatchCall(typeof(TimeLogger), "DetailedDrawTime"));
			c.EmitDelegate(DrawScreenspace);
		}

		public void DrawScreenspace()
		{		
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, null, null, null, Main.GameViewMatrix.ZoomMatrix);
			DrawScreenspaceEvent?.Invoke(Main.spriteBatch);
			Main.spriteBatch.End();
		}
	}
}
