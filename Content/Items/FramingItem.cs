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
            item.width = 30;
            item.height = 30;
            item.useTurn = true;
            item.autoReuse = true;
            item.useAnimation = 20;
            item.useTime = 20;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.value = 0;
            //item.createTile = ModContent.TileType<Tiles.GemTiles.AmethystSolid>();
            //item.createTile = ModContent.TileType<Tiles.Projector>();
        }
        public override bool AltFunctionUse(Player player)
        {
            return true;
        }
        public override bool CanUseItem(Player player)
        {
            Tile selectedTile = Main.tile[(int)(Main.MouseWorld.X / 16), (int)(Main.MouseWorld.Y / 16)];
            if (player.altFunctionUse == 2)
            {
                //if (player.controlDown)
                //    if (player.controlSmart)
                //        ShipWorld.shipViewRotation -= 0.05f;
                //    else
                //        ShipWorld.shipViewRotation -= 0.005f;
                //if (player.controlUp)
                //    if (player.controlSmart)
                //        ShipWorld.shipViewRotation += 0.05f;
                //    else
                //        ShipWorld.shipViewRotation += 0.005f;
                //ShipWorld.thrusterMode = 2;
                //Main.NewText("rotation amount " + ZeroG.shipViewRotationAmount);
                //return false;//this makes it update every tick instead of every swing

                if (player.controlDown)
                    if (player.controlSmart)
                        selectedTile.frameY--;
                    else
                        selectedTile.frameX--;
                if (player.controlUp)
                    if (player.controlSmart)
                        selectedTile.frameY++;
                    else
                        selectedTile.frameX++;

                Main.NewText("Frame X: " + selectedTile.frameX + " | Frame Y: " + selectedTile.frameY);
                return false;
            }
            else
            {
                if (player.controlDown)
                    if (player.controlSmart)
                        selectedTile.frameY--;
                    else
                        selectedTile.frameX--;
                if (player.controlUp)
                    if (player.controlSmart)
                        selectedTile.frameY++;
                    else
                        selectedTile.frameX++;

                Main.NewText("Frame X: " + selectedTile.frameX + " | Frame Y: " + selectedTile.frameY + " | Slope: " + selectedTile.slope());
                //Main.NewText("Frame X: " + Main.tile[(int)(Main.MouseWorld.X / 16), (int)(Main.MouseWorld.Y / 16)].frameX + " Frame Y: " + Main.tile[(int)(Main.MouseWorld.X / 16), (int)(Main.MouseWorld.Y / 16)].frameY);

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
                //			//Main.NewText("Origin : " + ModContent.GetModTile(baseTile.type).Name + " : " + ModContent.GetModTile(baseTile.type).mod);
                //			//Main.NewText("Placed : " + mod.GetTile(ModContent.GetModTile(baseTile.type).Name).Name + " : " + mod.GetTile(ModContent.GetModTile(baseTile.type).Name).mod);

                //			targetTile.type = mod.GetTile(ModContent.GetModTile(baseTile.type).Name).Type;
                //			targetTile.frameX = baseTile.frameX;
                //			targetTile.frameY = baseTile.frameY;
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
				//int npcs = 0;
				//for (int i = 0; i < Main.maxNPCs; i++)
				//{
				//	if (Main.npc[i].active)
				//	{
				//		Main.NewText(Main.npc[i].position);
				//		npcs++;
				//	}
				//}
				//Main.NewText("npcs: " + npcs);
				//Main.NewText("player pos: " + player.position);
				debug shit */
            }
            return base.CanUseItem(player);
        }
    }
}
