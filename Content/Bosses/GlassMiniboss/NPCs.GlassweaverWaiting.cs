using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Abilities.ForbiddenWinds;
using StarlightRiver.Content.Abilities.Hint;
using StarlightRiver.Content.GUI;
using StarlightRiver.Content.Items.Vitric;
using StarlightRiver.Content.Packets;
using StarlightRiver.Core.Systems.CameraSystem;
using System;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader.IO;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.GlassMiniboss
{
	class GlassweaverWaiting : ModNPC
	{
		public const int FRAME_WIDTH = 124;

		public const int FRAME_HEIGHT = 92;

		public Player talkingTo;
		public DialogManager manager;

		public static Vector2 ArenaPos => StarlightWorld.vitricBiome.TopLeft() * 16 + new Vector2(0, 80 * 16) + new Vector2(0, 256);

		public override string Texture => AssetDirectory.Glassweaver + Name;

		public ref float Timer => ref NPC.ai[0];
		public ref float State => ref NPC.ai[1];
		public ref float VisualTimer => ref NPC.ai[2];

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Glassweaver");
		}

		public override void SetDefaults()
		{
			NPC.netAlways = true;
			NPC.friendly = true;
			NPC.width = 64;
			NPC.height = 64;
			NPC.aiStyle = -1;
			NPC.damage = 10;
			NPC.defense = 15;
			NPC.lifeMax = 250;
			NPC.dontTakeDamage = true;
			NPC.immortal = true;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.knockBackResist = 0;

			manager = new("GlassweaverDialog.json", NPC);
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			database.Entries.Remove(bestiaryEntry);
		}

		public override bool NeedSaving()
		{
			return true;
		}

		public override void AI()
		{
			NPC.dontTakeDamage = true;
			NPC.immortal = true;

			Timer++;
			VisualTimer++;

			if (State < 0 || State > 7)
				State = StarlightWorld.HasFlag(WorldFlags.GlassweaverDowned) ? 3 : 0;

			if (Main.netMode != NetmodeID.Server) // Client based stuff
			{
				if (talkingTo != null && Vector2.Distance(talkingTo.Center, NPC.Center) > 2000)
				{
					talkingTo = null;
					RichTextBox.CloseDialogue();
				}

				if (talkingTo != null && talkingTo.TalkNPC != NPC)
				{
					talkingTo = null;
					RichTextBox.CloseDialogue();
				}
			}

			if (State == 0 || State >= 2)
			{
				NPC.direction = 1;

				NPC.frame = new Rectangle(0, FRAME_HEIGHT * ((int)(VisualTimer / 10f) % 7), FRAME_WIDTH, FRAME_HEIGHT);
			}

			if (State == 1)
			{
				if (Timer == 15)
				{
					NPC.direction = -1;
					NPC.frame = new Rectangle(0, FRAME_HEIGHT * ((int)(VisualTimer / 10f) % 7), FRAME_WIDTH, FRAME_HEIGHT);
				}

				if (Timer == 30)
					NPC.frame = new Rectangle(FRAME_WIDTH, 0, FRAME_WIDTH, FRAME_HEIGHT);

				if (Timer == 60)
				{
					NPC.velocity.Y -= 5;

					NPC.frame = new Rectangle(FRAME_WIDTH, FRAME_HEIGHT, FRAME_WIDTH, 124);
				}

				if (Timer > 60)
				{
					NPC.noTileCollide = true;
					NPC.noGravity = true;
					NPC.velocity.X = -10;
					NPC.velocity.Y += 0.25f;
				}

				if (Timer > 120)
				{
					NPC.velocity *= 0;
					NPC.noGravity = false;
					NPC.noTileCollide = false;
					NPC.Center = ArenaPos;
					State = 2;
				}
			}

			if (State == 6 && Main.time == 0 && Main.dayTime)
			{
				NPC.Center = StarlightWorld.vitricBiome.Center.ToVector2() * 16 + new Vector2(-86, 1200);
				StarlightWorld.RepairTemple();

				Main.NewText("The Glassweaver has moved into the forge!", new Color(160, 230, 255));

				State = 7;
			}
		}

		public override bool CheckActive()
		{
			if (Main.netMode == NetmodeID.MultiplayerClient && !NPC.active) // Close any dialogs if the npc is inactive.
			{
				talkingTo = null;
				RichTextBox.CloseDialogue();
			}

			return true;
		}

		public override bool CanChat()
		{
			return true;
		}

		/// <summary>
		/// Invoked by DialogManager to trigger the camera pan to the temple
		/// </summary>
		public void PanCamera()
		{
			CameraSystem.AsymetricalPan(180, 240, 150, StarlightWorld.vitricBiome.Center.ToVector2() * 16);
		}

		/// <summary>
		/// Invoked by DialogManager to trigger the MP sync effect
		/// </summary>
		public void SyncGlassweaver()
		{
			GlassweaverWaitingPacket statusPacket = new GlassweaverWaitingPacket(newState: 1, newTimer: 0, npcWhoAmI: NPC.whoAmI);
			statusPacket.Send();
		}

		/// <summary>
		/// Invoked by DialogManager to trigger the fight
		/// </summary>
		public void StartBossfight()
		{
			SacrificeNPCPacket nPacket = new SacrificeNPCPacket((int)NPC.Center.X, (int)NPC.Center.Y, NPCType<Glassweaver>(), NPC.whoAmI);
			nPacket.Send();
		}

		/// <summary>
		/// Invoked by DialogManager to spawn a key on request
		/// </summary>
		public void SpawnKey()
		{
			State = 5;

			if (!Helpers.Helper.HasItem(Main.LocalPlayer, ItemType<TempleEntranceKey>(), 1))
			{
				Main.LocalPlayer.QuickSpawnItem(NPC.GetSource_FromThis(), ItemType<TempleEntranceKey>());
				Main.LocalPlayer.GetModPlayer<HintPlayer>().SetHintState("PreWinds");
			}
		}

		/// <summary>
		/// Invoked by DialogManager to set the flag for the glassweaver moving in the next day
		/// </summary>
		public void SetFlagForMove()
		{
			State = 6;
		}

		/// <summary>
		/// Invoked by DialogManager to grant an infusion slot and infusion
		/// </summary>
		public void GiveInfusion()
		{
			Main.LocalPlayer.GetHandler().InfusionLimit++;
			Infusion.gainAnimationTimer = 240;

			Main.LocalPlayer.QuickSpawnItem(NPC.GetSource_FromThis(), ItemType<StellarRushItem>());
			Main.LocalPlayer.GetModPlayer<HintPlayer>().SetHintState("PostGlassweaverMove");
		}

		public override string GetChat()
		{
			if (Main.netMode == NetmodeID.Server) // Dialog only for client or in singleplayer.
				return "";

			talkingTo = Main.LocalPlayer;

			if (State == 0) //Waiting at entrance
			{
				manager.Start("Intro1");
			}
			else if (State == 2) //Waiting in arena
			{
				manager.Start("Challenge" + Main.rand.Next(1, 4));
			}
			else if (State == 3) //After winning
			{
				manager.Start("AfterFight1");
			}
			else if (State == 4) //After giving you the map
			{
				manager.Start("Interruption");
			}
			else if (State == 5 && !StarlightWorld.HasFlag(WorldFlags.VitricBossDowned)) //After talking after win
			{
				manager.Start("Key" + Main.rand.Next(1, 4));
			}
			else if (StarlightWorld.HasFlag(WorldFlags.VitricBossDowned) && State < 7)
			{
				manager.Start("Moving1");
			}
			else if (State == 7 && Main.LocalPlayer.GetHandler().InfusionLimit == 0)
			{
				manager.Start("Infusion1");
			}

			return "";
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D tex = Request<Texture2D>(Texture).Value;
			Vector2 pos = NPC.Center - Vector2.UnitY * 14 - Main.screenPosition;
			Vector2 origin = new Vector2(FRAME_WIDTH, FRAME_HEIGHT) * 0.5f;

			spriteBatch.Draw(tex, pos, NPC.frame, drawColor, NPC.rotation, origin, NPC.scale, NPC.direction == 1 ? 0 : SpriteEffects.FlipHorizontally, 0);

			if ((
				State == 0 ||
				State == 2 ||
				State == 3 ||
				State == 5 && StarlightWorld.HasFlag(WorldFlags.VitricBossDowned) ||
				State == 7 && Main.LocalPlayer.GetHandler().InfusionLimit == 0)
				&& talkingTo is null)
			{
				Texture2D exclaim = Assets.Misc.Exclaim.Value;
				Vector2 exclaimPos = NPC.Center + Vector2.UnitY * -95 - Main.screenPosition;
				exclaimPos.Y += (float)Math.Sin(Main.GameUpdateCount * 0.025f) * 5;
				spriteBatch.Draw(exclaim, exclaimPos, null, Color.White, (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.15f, exclaim.Size() / 2f, 1, 0, 0);

				float pulseTime = Main.GameUpdateCount % 60 < 50 ? 0 : (Main.GameUpdateCount % 60 - 50) / 10f;

				spriteBatch.Draw(exclaim, exclaimPos, null, Color.White * (1 - pulseTime), (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.15f, exclaim.Size() / 2f, 1 + pulseTime, 0, 0);
			}

			return false;
		}

		public override void SaveData(TagCompound tag)
		{
			tag.Add("State", (int)State);
		}

		public override void LoadData(TagCompound tag)
		{
			State = tag.GetAsInt("State");
		}
	}
}