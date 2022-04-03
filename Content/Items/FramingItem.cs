using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items
{
	public class FramingItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Framing Item");
            Tooltip.SetDefault("(Up +) (Down -) (Ctrl Y axis) (R-Click Fast)");
        }
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 20;
            Item.useTime = 20;
            Item.useStyle = ItemUseStyleID.SwingThrow;
            Item.value = 0;
            //Item.createTile = ModContent.TileType<Tiles.GemTiles.AmethystSolid>();
            //Item.createTile = ModContent.TileType<Tiles.Projector>();
        }
        public override bool AltFunctionUse(Player Player)
        {
            return true;
        }
        public override bool CanUseItem(Player Player)
        {
            Tile selectedTile = Main.tile[(int)(Main.MouseWorld.X / 16), (int)(Main.MouseWorld.Y / 16)];
            if (Player.altFunctionUse == 2)
            {
                //if (Player.controlDown)
                //    if (Player.controlSmart)
                //        ShipWorld.shipViewRotation -= 0.05f;
                //    else
                //        ShipWorld.shipViewRotation -= 0.005f;
                //if (Player.controlUp)
                //    if (Player.controlSmart)
                //        ShipWorld.shipViewRotation += 0.05f;
                //    else
                //        ShipWorld.shipViewRotation += 0.005f;
                //ShipWorld.thrusterMode = 2;
                //Main.NewText("rotation amount " + ZeroG.shipViewRotationAmount);
                //return false;//this makes it update every tick instead of every swing

                if (Player.controlDown)
                    if (Player.controlSmart)
                        selectedTile.TileFrameY--;
                    else
                        selectedTile.TileFrameX--;
                if (Player.controlUp)
                    if (Player.controlSmart)
                        selectedTile.TileFrameY++;
                    else
                        selectedTile.TileFrameX++;

                Main.NewText("Frame X: " + selectedTile.TileFrameX + " | Frame Y: " + selectedTile.TileFrameY);
                return false;
            }
            else
            {
                if (Player.controlDown)
                    if (Player.controlSmart)
                        selectedTile.TileFrameY--;
                    else
                        selectedTile.TileFrameX--;
                if (Player.controlUp)
                    if (Player.controlSmart)
                        selectedTile.TileFrameY++;
                    else
                        selectedTile.TileFrameX++;

                Main.NewText("Frame X: " + selectedTile.TileFrameX + " | Frame Y: " + selectedTile.TileFrameY + " | Slope: " + selectedTile.slope());
                //Main.NewText("Frame X: " + Main.tile[(int)(Main.MouseWorld.X / 16), (int)(Main.MouseWorld.Y / 16)].TileFrameX + " Frame Y: " + Main.tile[(int)(Main.MouseWorld.X / 16), (int)(Main.MouseWorld.Y / 16)].TileFrameY);

                //for(int g = -2; g < 3; g++)
                //{
                //	for (int h = -2; h < 3; h++)
                //	{
                //		Point16 position = new Point16((int)(Main.MouseWorld.X / 16) + g, (int)(Main.MouseWorld.Y / 16) + h);
                //		Point16 newPosition = new Point16(position.X, position.Y - 20);

                //		Tile baseTile = Main.tile[position.X, position.Y];

                //		Tile targetTile = Main.tile[newPosition.X, newPosition.Y];

                //		if (ModContent.GetModTile(baseTile.type) != null)
                //		{
                //			//Main.NewText("Origin : " + ModContent.GetModTile(baseTile.type).Name + " : " + ModContent.GetModTile(baseTile.type).Mod);
                //			//Main.NewText("Placed : " + Mod.GetTile(ModContent.GetModTile(baseTile.type).Name).Name + " : " + Mod.GetTile(ModContent.GetModTile(baseTile.type).Name).Mod);

                //			targetTile.type = Mod.GetTile(ModContent.GetModTile(baseTile.type).Name).Type;
                //			targetTile.TileFrameX = baseTile.TileFrameX;
                //			targetTile.TileFrameY = baseTile.TileFrameY;
                //			targetTile.inActive(baseTile.inActive());
                //			targetTile.slope(baseTile.slope());
                //			targetTile.active(true);
                //			WorldGen.TileFrame(newPosition.X, newPosition.Y);
                //		}
                //		else
                //		{
                //			targetTile.ClearEverything();
                //			//Main.NewText("null");
                //		}
                //	}
                //}

                /*
				//Main.NewText(ShipWorld.ShipAreaClear(new Point16((int)(Main.MouseWorld.X / 16), (int)(Main.MouseWorld.Y / 16)), 0, true));

				//checks tile frame
				Tile selectedTile = Main.tile[(int)(Main.MouseWorld.X / 16), (int)(Main.MouseWorld.Y / 16)];
				//int NPCs = 0;
				//for (int i = 0; i < Main.maxNPCs; i++)
				//{
				//	if (Main.npc[i].active)
				//	{
				//		Main.NewText(Main.npc[i].position);
				//		NPCs++;
				//	}
				//}
				//Main.NewText("NPCs: " + NPCs);
				//Main.NewText("Player pos: " + Player.position);
				debug */
            }
            return base.CanUseItem(Player);
        }
    }
}
