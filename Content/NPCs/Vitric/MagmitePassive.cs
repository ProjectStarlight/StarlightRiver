using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.NPCs.Vitric
{
	internal class MagmitePassive : ModNPC
    {
        public ref float ActionState => ref NPC.ai[0];
        public ref float ActionTimer => ref NPC.ai[1];
		public ref float GlobalTimer => ref NPC.ai[2];
        public ref float TurnTimer => ref NPC.ai[3];

        public override string Texture => "StarlightRiver/Assets/NPCs/Vitric/MagmitePassive";

        public override void Load()
        {
            //GoreLoader.AddGoreFromTexture<MagmiteGore>(StarlightRiver.Instance, "StarlightRiver/Assets/NPCs/Vitric/MagmiteGore");
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Small Magmite");
            Main.npcCatchable[Type] = true;
        }

        public override void SetDefaults()
        {
            NPC.catchItem = ItemType<MagmitePassiveItem>();
            NPC.width = 24;
            NPC.height = 24;
            NPC.damage = 0;
            NPC.defense = 0;
            NPC.lifeMax = 25;
            NPC.aiStyle = -1;
            NPC.lavaImmune = true;

            ActionState = -1;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                Bestiary.SLRSpawnConditions.VitricDesert,
                new FlavorTextBestiaryInfoElement("[PH] Entry")
            });
        }

        public override Color? GetAlpha(Color drawColor)
        {
            return Color.White;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.WritePackedVector2(NPC.velocity);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            NPC.velocity = reader.ReadPackedVector2();
        }

        public override void AI()
        {
            var x = (int)(NPC.Center.X / 16) + NPC.direction; //check 1 tile infront of la cretura
            var y = (int)(NPC.Center.Y / 16);
            var tile = Framing.GetTileSafely(x, y);
			var tileUp = Framing.GetTileSafely(x, y - 1);
            var tileClose = Framing.GetTileSafely(x - NPC.direction, y - 1);
            var tileFar = Framing.GetTileSafely(x + NPC.direction * 2, y - 1);
            var tileUnder = Framing.GetTileSafely(x, y + 1);

            ActionTimer++;
            GlobalTimer++;

            if (Main.rand.Next(10) == 0)
                Gore.NewGoreDirect(NPC.GetSource_FromAI(), NPC.Center, (Vector2.UnitY * -3).RotatedByRandom(0.2f), Mod.Find<ModGore>("MagmiteGore").Type, Main.rand.NextFloat(0.5f, 0.8f));

            if(ActionState == -1)
			{
                if (tile .LiquidAmount > 0)
                    NPC.velocity.Y = -4;
                else
                {
                    NPC.velocity.X += Main.rand.NextBool() ? 5 : -5;
                    NPC.velocity.Y = -10;
                    ActionState = 0;
                    if (Main.netMode == NetmodeID.Server)
                        NPC.netUpdate = true;
                }
			}

            if (ActionState == 0)
            {
                if (NPC.velocity.Y == 0 && NPC.velocity.X == 0 && tile.Slope == SlopeType.Solid && !tile.IsHalfBlock && 
                    tile.BlockType == BlockType.Solid &&
                    (!tileUp.HasTile || (!Main.tileSolid[tileUp.TileType] && !Main.tileSolidTop[tileUp.TileType])) &&
                    (!tileClose.HasTile || (!Main.tileSolid[tileClose.TileType] && !Main.tileSolidTop[tileClose.TileType]))) //climb up small cliffs
                {
                    ActionState = 1;
                    NPC.velocity *= 0;
                    ActionTimer = 0;
                    return;
                }

                else if(NPC.velocity.X == 0 && tile.HasTile && (!tileUp.HasTile || (!Main.tileSolid[tileUp.TileType] && !Main.tileSolidTop[tileUp.TileType])))
				{
                    NPC.velocity.Y -= 2;
                }

                if (NPC.velocity.X == 0)
                    TurnTimer++;
                else
                    TurnTimer = 0;

                if (TurnTimer > 180)
                {
					NPC.velocity.X = NPC.direction * -1;
                    NPC.target = -1;
                    TurnTimer = 0;
                }

                if(ActionTimer % 60 == 0)
                    NPC.TargetClosest();

                if(NPC.target >= 0)
                    NPC.velocity.X += 0.05f * (Main.player[NPC.target].Center.X > NPC.Center.X ? 1 : -1);

                NPC.velocity.X = Math.Min(NPC.velocity.X, 1.5f);
                NPC.velocity.X = Math.Max(NPC.velocity.X, -1.5f);

                NPC.direction = NPC.velocity.X > 0 ? 1 : -1;
                NPC.spriteDirection = NPC.velocity.X > 0 ? 1 : -1;

                if(tileFar.BlockType == BlockType.Solid && NPC.velocity.Y == 0) //jump up big cliffs
                    NPC.velocity.Y -= 8;

                if((!tileUnder.HasTile || (!Main.tileSolid[tileUnder.TileType] && !Main.tileSolidTop[tileUnder.TileType])) && NPC.velocity.Y == 0) //hop off edges
                    NPC.velocity.Y -= 4;

                if (NPC.velocity.Y != 0)
                {
                    NPC.frame.X = 0;
                    NPC.frame.Y = 0;
                }
                else
                {
                    NPC.frame.X = 42;
                    NPC.frame.Y = (int)((ActionTimer / 5) % 5) * 40;
                }
            }

            if(ActionState == 1)
            {
                if (ActionTimer == 60) 
                {
                    ActionState = 0;
                    ActionTimer = 0;
                    NPC.position.Y -= 16;
                    NPC.position.X += 26 * NPC.direction;
                }

                NPC.frame.X = 84;
                NPC.frame.Y = (int)((ActionTimer / 60f) * 9) * 40;
            }

            NPC.frame.Width = 42;
            NPC.frame.Height = 40;
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            if (NPC.life <= 0 && Main.netMode != NetmodeID.Server)
            {
                for (int k = 0; k < 30; k++)
                    Gore.NewGoreDirect(NPC.GetSource_Death(), NPC.Center, (Vector2.UnitY * Main.rand.NextFloat(-8, -1)).RotatedByRandom(0.5f), Mod.Find<ModGore>("MagmiteGore").Type, Main.rand.NextFloat(0.5f, 0.8f));

                Terraria.Audio.SoundEngine.PlaySound(SoundID.DD2_GoblinHurt, NPC.Center);
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            var pos = NPC.Center - screenPos + new Vector2(0, -8);

            if (ActionState == 1)
            {
                pos += new Vector2(8 * NPC.spriteDirection, -4);

                if (NPC.spriteDirection == -1)
                    pos.X += 4;
            }

            int originX = 18;
            if (NPC.spriteDirection == -1)
                originX = 30;

            spriteBatch.Draw(Request<Texture2D>(Texture).Value, pos, NPC.frame, Color.White, 0, new Vector2(originX, 20), 1, NPC.spriteDirection == -1 ? 0 : SpriteEffects.FlipHorizontally, 0);
            return false;
        }
	}

    internal class MagmiteGore : ModGore
    {
		public override string Texture => AssetDirectory.VitricNpc + "MagmiteGore";

        public override void OnSpawn(Gore gore, IEntitySource source)
        {
            gore.timeLeft = 180;
            gore.sticky = true;
            gore.numFrames = 2;
            gore.behindTiles = true;
        }

        public override Color? GetAlpha(Gore gore, Color lightColor) => Color.White * (gore.scale < 0.5f ? gore.scale * 2 : 1);

        public override bool Update(Gore gore)
        {
            Lighting.AddLight(gore.position, new Vector3(1.6f, 0.8f, 0) * gore.scale);

            gore.scale *= 0.99f;

            if (gore.scale < 0.1f)
                gore.active = false;

            if (gore.velocity.Y == 0)
            {
                gore.velocity.X = 0;
                gore.rotation = 0;
                gore.frame = 1;
            }

            //gore.position += gore.velocity;

            if (gore.frame == 0) gore.velocity.Y += 0.5f;

            return true;
        }
    }

    internal class MagmitePassiveItem : QuickNPCItem
    {
        public MagmitePassiveItem() : base("Magmite", "Release him!", Item.sellPrice(silver: 15), ItemRarityID.Orange, NPCType<MagmitePassive>(), AssetDirectory.VitricItem) { }
    }
    /*
    internal class MagmiteBanner : ModBanner
    {
    public MagmiteBanner() : base("MagmiteBannerItem", NPCType<MagmitePassive>(), AssetDirectory.VitricNpc) { }
    }

    internal class MagmiteBannerItem : QuickBannerItem
    {
        public MagmiteBannerItem() : base("MagmiteBanner", "Small Magmite", AssetDirectory.VitricNpc) { }
    }*/
}