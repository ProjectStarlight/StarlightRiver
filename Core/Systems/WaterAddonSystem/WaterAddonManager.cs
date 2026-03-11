using Mono.Cecil.Cil;
using MonoMod.Cil;
using StarlightRiver.Content.CustomHooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace StarlightRiver.Core.Systems.WaterAddonSystem;

class WaterAddonManager : ModSystem
{
	public static WaterAddon activeAddon;

	public override void Load()
	{
		StarlightPlayer.PostUpdateEvent += UpdateActiveAddon;

		IL_Main.DoDraw += AddWaterShader;
		//IL.Terraria.Main.DrawTiles += SwapBlockTexture;//TODO: Figure out where this logic moved in vanilla
	}

	private void UpdateActiveAddon(Player Player)
	{
		activeAddon = ModContent.GetContent<WaterAddon>().FirstOrDefault(n => n.Visible);
	}

	public override void Unload()
	{
		activeAddon = null;
	}

	private void SwapBlockTexture(ILContext il)
	{
		var c = new ILCursor(il);
		c.TryGotoNext(n => n.MatchLdsfld(typeof(Main), "liquidTexture"));
		c.Index += 3;
		c.Emit(OpCodes.Ldloc, 16);
		c.Emit(OpCodes.Ldloc, 15);

		c.EmitDelegate<Func<Texture2D, int, int, Texture2D>>(LavaBlockBody);
	}

	private Texture2D LavaBlockBody(Texture2D arg, int x, int y)
	{
		Tile tile = Framing.GetTileSafely(x, y);

		if (tile.LiquidType != LiquidID.Water)
			return arg;

		if (activeAddon is null) //putting this in the same check as above dosent seem to short circuit properly? 
			return arg;

		return activeAddon.BlockTexture(arg, x, y);

	}

	private void AddWaterShader(ILContext il)
	{
		var c = new ILCursor(il);

		//back target
		c.TryGotoNext(n => n.MatchLdfld<Main>("backWaterTarget"));

		c.TryGotoNext(n => n.MatchCallvirt<SpriteBatch>("Draw"));
		c.Index++;
		ILLabel label = il.DefineLabel(c.Next);

		c.TryGotoPrev(n => n.MatchLdfld<Main>("backWaterTarget"));
		c.Index -= 1;
		c.Emit(OpCodes.Pop);
		c.EmitDelegate<Action>(NewDrawBack);
		c.Emit(OpCodes.Br, label);

		//front target
		c.TryGotoNext(n => n.MatchLdsfld<Main>("waterTarget"));

		c.TryGotoNext(n => n.MatchCallvirt<SpriteBatch>("Draw"));
		c.Index++;
		ILLabel label2 = il.DefineLabel(c.Next);

		c.TryGotoPrev(n => n.MatchLdsfld<Main>("waterTarget"));
		c.Emit(OpCodes.Pop);
		c.EmitDelegate<Action>(NewDraw);
		c.Emit(OpCodes.Br, label2);
	}

	private void NewDrawBack()
	{
		SpriteBatch sb = Main.spriteBatch;

		if (activeAddon != null)
		{
			sb.End();
			activeAddon.SpritebatchChangeBack();
		}

		Main.spriteBatch.Draw(Main.instance.backWaterTarget, Main.sceneBackgroundPos - Main.screenPosition, Color.White);

		if (activeAddon != null)
		{
			sb.End();
			sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
		}
	}

	private void NewDraw()
	{
		SpriteBatch sb = Main.spriteBatch;

		if (activeAddon != null)
		{
			sb.End();
			activeAddon.SpritebatchChange();
		}

		Main.spriteBatch.Draw(Main.waterTarget, Main.sceneWaterPos - Main.screenPosition, Color.White);

		if (activeAddon != null)
		{
			sb.End();
			sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
		}
	}
}