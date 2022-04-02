using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Forest
{
	public class SlimeberryBush : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.ForestTile + name;
            return base.Autoload(ref name, ref texture);
        }

        public override void SetDefaults()
        {
            AnchorData anchor = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, 2, 0);
            int[] valid = new int[] { TileID.Grass };

            TileObjectData.newTile.RandomStyleRange = 3;
            TileObjectData.newTile.DrawYOffset = 2;
            QuickBlock.QuickSetFurniture(this, 2, 2, DustID.Grass, SoundID.Dig, false, new Color(200, 255, 220), false, false, "", anchor, default, valid);
        }

        public override void RandomUpdate(int i, int j) 
        {
            Tile tile = Main.tile[i, j]; 
            TileObjectData data = TileObjectData.GetTileData(tile); 
            int fullFrameWidth = data.Width * (data.CoordinateWidth + data.CoordinatePadding); 

            if (tile.frameX == 0 && tile.frameY % 36 == 0)
                if (Main.rand.Next(1) == 0 && tile.frameX == 0)
                    for (int x = 0; x < data.Width; x++)
                        for (int y = 0; y < data.Height; y++)
                        {
                            Tile targetTile = Main.tile[i + x, j + y]; 
                            targetTile.frameX += (short)fullFrameWidth; 
                        }
        }

        public override bool NewRightClick(int i, int j)
        {
            if (Main.tile[i, j].frameX > 35) 
            {
                Tile tile = Main.tile[i, j]; //Selects current tile

                int newX = i; 
                int newY = j;
                if (tile.frameX % 36 == 18) newX = i - 1;
                if (tile.frameY % 36 == 18) newY = j - 1;

                for (int k = 0; k < 2; k++)
                    for (int l = 0; l < 2; ++l)
                        Main.tile[newX + k, newY + l].frameX -= 36; //Changes frames to berry-less

                int rand = Main.rand.Next(3, 5);
                for (int k = 0; k < rand; k++)
                {
                    int index = NPC.NewNPC(i * 16 + Main.rand.Next(32), j * 16 + Main.rand.Next(32), NPCType<BerrySlime>());
                    Main.npc[index].velocity = Vector2.UnitY.RotatedByRandom(0.6f) * -8;
                }
            }
            return true;
        }

        public override void MouseOver(int i, int j)
        {
            if (Main.tile[i, j].frameX >= 32)
            {
                Player player = Main.LocalPlayer;
                player.showItemIcon2 = ItemType<Slimeberry>();
                player.noThrow = 2;
                player.showItemIcon = true;
            }
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new Vector2(i * 16, j * 16), ItemType<SlimeberryBushItem>()); //drop a bush item

            if (frameX > 35)
                Item.NewItem(new Vector2(i, j) * 16, ItemType<Slimeberry>()); //Drops berries if harvestable
        }
    }

    public class Slimeberry : QuickMaterial
	{
        public Slimeberry() : base("Slimeberry", "Ew.", 99, 1, ItemRarityID.Blue, AssetDirectory.ForestTile) { }
	}

    public class SlimeberryBushItem : QuickTileItem
	{
        public SlimeberryBushItem() : base("Slimeberry Bush", "Places a slimeberry bush", TileType<SlimeberryBush>(), ItemRarityID.Blue, AssetDirectory.ForestTile) { }
	}

    public class BerrySlime : ModNPC
	{
        public ref float PhaseTimer => ref npc.ai[0];
        public ref float GlobalTimer => ref npc.ai[1];

        public override string Texture => AssetDirectory.ForestTile + Name;

		public override void SetStaticDefaults()
		{
            DisplayName.SetDefault("Slimeberry");
            Main.npcFrameCount[npc.type] = 2;
		}

		public override void SetDefaults()
		{
            npc.width = 18;
            npc.height = 14;
            npc.aiStyle = -1;
            npc.lifeMax = 5;
            npc.catchItem = (short)ItemType<Slimeberry>();
            npc.HitSound = SoundID.NPCHit1;
            npc.DeathSound = SoundID.NPCDeath1;

            npc.color = new Color(30, Main.rand.Next(170, 250), Main.rand.Next(220, 255));
        }

		public override void AI()
        {
            GlobalTimer++;

            if(GlobalTimer > 300) //die after 5 seconds
			{
                if(npc.velocity.Y == 0)
				{
                    npc.velocity *= 0;
                    npc.frame = new Rectangle(0, npc.height, npc.width, npc.height);

                    npc.alpha += 5;
                    npc.position.Y += 0.1f;
                    npc.noGravity = true;
                    npc.noTileCollide = true;
                    npc.knockBackResist = 0;

                    if (npc.alpha >= 255)
                        npc.active = false;
				}
                return;
			}

            if (npc.velocity.Y == 0) //else jump around like a lunatic
            {
                npc.velocity *= 0;
                npc.frame = new Rectangle(0, npc.height, npc.width, npc.height);

                PhaseTimer++;
                if (PhaseTimer >= 4 && Main.rand.Next(4) == 0)
                {
                    npc.direction = Main.rand.NextBool() ? 1 : -1;
                    npc.velocity += new Vector2(npc.direction * Main.rand.NextFloat(2, 4), -6);
                    npc.netUpdate = true;

                    Terraria.Audio.SoundEngine.PlaySound(SoundID.NPCDeath1.SoundId, (int)npc.Center.X, (int)npc.Center.Y, SoundID.NPCDeath1.Style, 0.5f, Main.rand.NextFloat(0.5f, 1));
                }
            }
            else
            {
                npc.frame = new Rectangle(0, 0, npc.width, npc.height);
                PhaseTimer = 0;
            }
        }

		public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
		{
            spriteBatch.Draw(GetTexture(Texture), npc.position - Main.screenPosition, npc.frame, drawColor.MultiplyRGBA(npc.color * ((255 - npc.alpha) / 255f)), npc.rotation, Vector2.Zero, npc.scale, 0, 0);
            spriteBatch.Draw(GetTexture(Texture + "Shine"), npc.position - Main.screenPosition, npc.frame, drawColor * (0.6f * (255 - npc.alpha) / 255f), npc.rotation, Vector2.Zero, npc.scale, 0, 0);
            return false;
		}

		public override void OnCatchNPC(Player player, Item item)
		{
            for (int k = 0; k < 20; k++)
                Dust.NewDust(npc.position, 16, 16, DustID.t_Slime, 0, 0, 200, npc.color, 0.5f);
        }

		public override void NPCLoot()
		{
            int i = Item.NewItem(npc.Center, ItemID.Gel);
            Main.item[i].color = npc.color;

            for (int k = 0; k < 20; k++)
                Dust.NewDust(npc.position, 16, 16, DustID.t_Slime, 0, 0, 200, npc.color, 0.5f);
		}
	}
}