﻿using StarlightRiver.Content.Biomes;
using StarlightRiver.Helpers;
using System;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Vitric
{
	public class AncientSandstoneTorchItem : QuickTileItem
	{
		public AncientSandstoneTorchItem() : base("Ancient Vitric Illuminator", "It has an entrancing glow", "AncientSandstoneTorch", 0, AssetDirectory.VitricTile + "AncientSandstoneTorch", true) { }
	}

	internal class AncientSandstoneTorch : ModTile
	{
		public override string Texture => AssetDirectory.VitricTile + Name;

		public override void SetStaticDefaults()
		{
			Main.tileLavaDeath[Type] = false;
			Main.tileFrameImportant[Type] = true;
			Main.tileLighted[Type] = true;

			TileID.Sets.FramesOnKillWall[Type] = true;

			MinPick = 200;
			RegisterItemDrop(ItemType<AncientSandstoneTorchItem>());
			DustType = DustType<Dusts.Air>();
			AddMapEntry(new Color(115, 182, 158));
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			if (!StarlightWorld.HasFlag(WorldFlags.DesertOpen) || !Main.LocalPlayer.InModBiome(ModContent.GetInstance<VitricTempleBiome>()))
				return;
			r = 125 * 0.007f;
			g = 162 * 0.007f;
			b = 158 * 0.007f;
		}

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
		{
			if (!StarlightWorld.HasFlag(WorldFlags.DesertOpen) || !Main.LocalPlayer.InModBiome(ModContent.GetInstance<VitricTempleBiome>()))
				return;

			Texture2D tex = Assets.RiftCrafting.Glow0.Value;
			Texture2D tex2 = Assets.RiftCrafting.Glow1.Value;

			spriteBatch.End();
			spriteBatch.Begin(default, BlendState.Additive, SamplerState.PointClamp, default, default);

			Vector2 pos = (new Vector2(i, j) + Helper.TileAdj) * 16 + Vector2.One * 8 - Main.screenPosition;

			for (int k = 0; k < 3; k++)
			{
				spriteBatch.Draw(tex, pos, tex.Frame(), new Color(125, 162, 158) * (0.65f + (float)Math.Sin(StarlightWorld.visualTimer) * 0.05f), 0, tex.Size() / 2, k * 0.3f, 0, 0);
			}

			spriteBatch.Draw(tex2, pos, tex.Frame(), new Color(125, 162, 158) * (0.65f + (float)Math.Sin(StarlightWorld.visualTimer) * 0.10f), (float)Math.Sin(StarlightWorld.visualTimer) * 0.1f, tex.Size() / 2, 0.6f, 0, 0);
			spriteBatch.Draw(tex2, pos, tex.Frame(), new Color(125, 162, 158) * (0.65f - (float)Math.Sin(StarlightWorld.visualTimer) * 0.10f), 2 + -(float)Math.Sin(StarlightWorld.visualTimer + 1) * 0.1f, tex.Size() / 2, 0.9f, 0, 0);

			spriteBatch.End();
			spriteBatch.Begin(default, default, SamplerState.PointClamp, default, default);
		}
	}
}