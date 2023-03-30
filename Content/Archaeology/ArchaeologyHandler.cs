using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.Map;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace StarlightRiver.Content.Archaeology
{
	public class ArchaeologyHandler : ModSystem
	{
		public override void Load()
		{
			Terraria.On_Main.DoDraw_DrawNPCsBehindTiles += DrawArtifacts;
		}

		public override void Unload()
		{
			Terraria.On_Main.DoDraw_DrawNPCsBehindTiles -= DrawArtifacts;
		}

		public override void PreUpdateDusts()
		{
			foreach (KeyValuePair<int, TileEntity> item in TileEntity.ByID)
			{
				if (item.Value is Artifact artifact && artifact.IsOnScreen())
					artifact.CreateSparkles();
			}
		}

		public override void LoadWorldData(TagCompound tag)
		{
			ModContent.GetInstance<ArchaeologyMapLayer>().CalculateDrawables();
		}

		public void DrawArtifacts(Terraria.On_Main.orig_DoDraw_DrawNPCsBehindTiles orig, Main self)
		{
			Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
			foreach (KeyValuePair<int, TileEntity> item in TileEntity.ByID)
			{
				if (item.Value is Artifact artifact && artifact.IsOnScreen())
					artifact.Draw(Main.spriteBatch);
			}

			Main.spriteBatch.End();

			orig(self);
		}
	}

	public class ArchaeologyMapLayer : ModMapLayer
	{
		public List<KeyValuePair<int, TileEntity>> toDraw;

		public void CalculateDrawables()
		{
			toDraw = TileEntity.ByID.Where(x => x.Value is Artifact artifact && artifact.displayedOnMap).ToList();
		}

		public override void Draw(ref MapOverlayDrawContext context, ref string text)
		{
			if (toDraw is null)
				CalculateDrawables();
			foreach (KeyValuePair<int, TileEntity> drawable in toDraw)
			{
				var artifact = (Artifact)drawable.Value;
				Texture2D mapTex = ModContent.Request<Texture2D>(artifact.MapTexturePath).Value;

				if (context.Draw(mapTex, artifact.Position.ToVector2(), Color.White, new SpriteFrame(1, 1, 0, 0), 1, 1, Alignment.Center).IsMouseOver)
					text = "Artifact";
			}
		}
	}
}