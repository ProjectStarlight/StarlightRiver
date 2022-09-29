﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Overgrow
{
	internal class TorchOvergrow : ModTile
    {
        public override string Texture => AssetDirectory.OvergrowTile + "TorchOvergrow";

        public override void SetStaticDefaults()
        {
            QuickBlock.QuickSetFurniture(this, 1, 1, DustType<Content.Dusts.GoldWithMovement>(), SoundID.Tink, false, Color.Yellow);
            TileID.Sets.FramesOnKillWall[Type] = true;
        }

        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
        {
            Dust.NewDustPerfect(new Vector2(6 + i * 16, 2 + j * 16), DustType<Content.Dusts.GoldWithMovement>(), new Vector2((float)Math.Sin(StarlightWorld.rottime * 2 + i * j) * 0.3f, -0.4f));
            Lighting.AddLight(new Vector2(i * 16, j * 16), new Vector3(245, 220, 110) * 0.004f);
        }
    }

    public class TorchOvergrowItem : QuickTileItem
    {
        public TorchOvergrowItem() : base("Faerie Torch", "Sparkly!", "WispAltarL", 0, AssetDirectory.OvergrowTile) { }
    }

    internal class BlueTorchOvergrow : ModTile
    {
        public override string Texture => AssetDirectory.OvergrowTile + "BlueTorchOvergrow";

        public override void SetStaticDefaults()
        {
            QuickBlock.QuickSetFurniture(this, 1, 1, DustType<Dusts.BlueStamina>(), SoundID.Tink, false, Color.Teal);
            TileID.Sets.FramesOnKillWall[Type] = true;
        }

        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
        {
            Dust.NewDustPerfect(new Vector2(8 + i * 16, 2 + j * 16), DustType<Dusts.BlueStamina>(), new Vector2((float)Math.Sin(StarlightWorld.rottime * 2 + i * j) * 0.3f, -0.6f), 0, default, 1.5f);
            Lighting.AddLight(new Vector2(i * 16, j * 16), new Vector3(110, 200, 225) * 0.004f);
        }
    }

    public class BlueTorchOvergrowItem : QuickTileItem
    {
        public BlueTorchOvergrowItem() : base("Blue Faerie Torch", "Sparkly! and Blue!", "BlueTorchOvergrow", 0, AssetDirectory.OvergrowTile) { }
    }
}