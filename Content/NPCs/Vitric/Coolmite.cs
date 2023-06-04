using StarlightRiver.Content.Bosses.GlassMiniboss;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Items.Vitric;
using System;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.NPCs.Vitric
{
	internal class Coolmite : MagmitePassive, IHintable
	{
		bool melting = false;
		int meltingTimer = 0;

		public float MeltingTransparency => (float)meltingTimer / 150;

		public override string Texture => "StarlightRiver/Assets/NPCs/Vitric/Coolmite";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Coolmite");
			Main.npcCatchable[Type] = true;
		}

		public override void SetDefaults()
		{
            base.SetDefaults();
			NPC.catchItem = ItemType<CoolmiteItem>();
			NPC.HitSound = SoundID.Item27;
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
			{
				Bestiary.SLRSpawnConditions.VitricDesert,
				new FlavorTextBestiaryInfoElement("Through the power of Shimmer, this form of Magmite has magically cooled down to safe temperatures without rendering itself immobile. Unfortunately, this is a rather chilly temperature for a creature used to being submerged in lava, so they go to great lengths to heat themselves back up.")
			});
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
            base.SendExtraAI(writer);
			writer.Write(melting);
			writer.Write(meltingTimer);
        }

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			base.ReceiveExtraAI(reader);
			melting = reader.ReadBoolean();
			meltingTimer = reader.Read();
		}

		public override bool PreAI()
		{
            bool runAI = base.PreAI();

			Vector2? lavaPos = FindLava(); // make coolmite target lava if nearby so it can melt back down to a magmite

			if (lavaPos != null)
				targetX = ((Vector2)lavaPos).X;
			else if (NPC.target >= 0)
				targetX = Main.player[NPC.target].Center.X;
			else
				targetX = null;

            return runAI;
		}

		public override void PostAI()
		{
			int x = (int)(NPC.Center.X / 16) + NPC.direction; //check 1 tile infront of la cretura
			int y = (int)((NPC.Center.Y + 8) / 16);
			Tile tile = Framing.GetTileSafely(x, y);

			if (tile.LiquidAmount > 0 && tile.LiquidType == LiquidID.Lava)
			{
				melting = true;
            }

            if (melting)
            {
                meltingTimer++;

                if (meltingTimer % 4 == 0)
                    Gore.NewGoreDirect(NPC.GetSource_FromAI(), NPC.Center, (Vector2.UnitY * -3).RotatedByRandom(0.2f), Mod.Find<ModGore>("MagmiteGore").Type, Main.rand.NextFloat(0.5f, 0.8f));
            }

            if (meltingTimer > 120)
            {
                NPC.active = false;
                NPC magmite = NPC.NewNPCDirect(NPC.GetSource_FromThis(), (int)NPC.position.X, (int)NPC.position.Y, NPCType<MagmitePassive>(), 0, NPC.ai[0], NPC.ai[1], NPC.ai[2], NPC.ai[3], NPC.target);
                magmite.frame = NPC.frame;
                magmite.velocity = NPC.velocity;
                magmite.velocity.Y = -10;

                SoundEngine.PlaySound(SoundID.Item176);

                for (int k = 0; k < 20; k++)
                    Gore.NewGoreDirect(NPC.GetSource_Death(), NPC.Center, (Vector2.UnitY * Main.rand.NextFloat(-8, -1)).RotatedByRandom(0.5f), Mod.Find<ModGore>("MagmiteGore").Type, Main.rand.NextFloat(0.5f, 0.8f));

                for (int k = 0; k < 20; k++)
                    Dust.NewDustDirect(NPC.Center, 16, 16, DustID.Torch, 0, 0, 0, default, 1.5f).velocity *= 3f;
			}

			NPC.shimmerTransparency = MeltingTransparency;
		}

		public override void HitEffect(NPC.HitInfo hit)
		{
			if (NPC.life <= 0 && Main.netMode != NetmodeID.Server)
			{
				for (int k = 0; k < 5; k++)
					Dust.NewDust(NPC.position, 16, 16, DustID.Demonite);

				for (int k = 0; k < 25; k++)
					Dust.NewDust(NPC.position, 16, 16, DustID.Glass);

				SoundEngine.PlaySound(SoundID.Shatter, NPC.Center);
			}
		}

		private Vector2? FindLava()
		{
			Vector2? lavaPos = null;
			for (int i = -25; i < 25; i++)
			{
				for (int j = -5; j < 15; j++)
				{
					Tile tileLava = Main.tile[(int)NPC.Center.X / 16 + i, (int)NPC.Center.Y / 16 + j];

					if (tileLava.LiquidAmount > 0 && tileLava.LiquidType == LiquidID.Lava)
					{
						if (lavaPos == null || ((Vector2)lavaPos - NPC.Center).Length() > new Vector2(i, j).Length() * 16)
						{
							Vector2 checkPos = NPC.Center + new Vector2(i, j) * 16;
							if (Collision.CanHitLine(NPC.Center, 1, 1, checkPos, 1, 1) || Collision.CanHitLine(NPC.Center - Vector2.UnitY * 50, 1, 1, checkPos, 1, 1)) // checks if lava can be reached
								lavaPos = checkPos;
						}
					}
				}
			}

			return lavaPos;
		}
		public string GetHint()
		{
			return "Even cuter in crystal!";
		}
	}

	internal class CoolmiteItem : QuickCritterItem
	{
		public CoolmiteItem() : base("Coolmite", "Fragile! Please handle with care.", Item.sellPrice(silver: 15), ItemRarityID.Orange, NPCType<Coolmite>(), AssetDirectory.VitricItem) { }
	}
}