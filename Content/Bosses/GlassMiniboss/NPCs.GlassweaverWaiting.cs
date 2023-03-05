using StarlightRiver.Content.GUI;
using StarlightRiver.Core.Systems.CameraSystem;
using Terraria.ID;
using Terraria.ModLoader.IO;
using static Terraria.ModLoader.ModContent;
using Terraria.Localization;

namespace StarlightRiver.Content.Bosses.GlassMiniboss
{
	class GlassweaverWaiting : ModNPC
	{
		public const int FRAME_WIDTH = 110;
		public const int FRAME_HEIGHT = 92;

		Vector2 ArenaPos => StarlightWorld.vitricBiome.TopLeft() * 16 + new Vector2(0, 80 * 16) + new Vector2(0, 256);

		public override string Texture => AssetDirectory.Glassweaver + Name;

		public ref float Timer => ref NPC.ai[0];
		public ref float State => ref NPC.ai[1];
		public ref float VisualTimer => ref NPC.ai[2];
		public ref float TextState => ref NPC.ai[3];

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Glassweaver");
		}

		public override void SetDefaults()
		{
			NPC.townNPC = true;
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
		}

		public override void AI()
		{
			Timer++;
			VisualTimer++;

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
					NPC.velocity.Y -= 20;
					NPC.frame = new Rectangle(FRAME_WIDTH, FRAME_HEIGHT, FRAME_WIDTH, 112);
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
		}

		private string GlassweaverWaitingText(string text) => Language.GetTextValue("Mods.StarlightRiver.Custom.RichTextBox.GlassweaverWaiting." + text);

		private string GetIntroDialogue()
		{
			return TextState switch
			{
				0 => GlassweaverWaitingText("IntroDialogue.0"),
				1 => GlassweaverWaitingText("IntroDialogue.1"),
				2 => GlassweaverWaitingText("IntroDialogue.2"),
				3 => GlassweaverWaitingText("IntroDialogue.3"),
				4 => GlassweaverWaitingText("IntroDialogue.4"),
				5 => GlassweaverWaitingText("IntroDialogue.5"),
				_ => "This text should never be seen! Please report to https://github.com/ProjectStarlight/StarlightRiver/issues",
			};
		}

		private string GetAfterCameraDialogue()
		{
			return GlassweaverWaitingText("AfterCameraDialogue.0");
		}

		private string GetWaitingDialogue()
		{
			return Main.rand.Next(3) switch
			{
				0 => GlassweaverWaitingText("WaitingDialogue.0"),
				1 => GlassweaverWaitingText("WaitingDialogue.1"),
				2 => GlassweaverWaitingText("WaitingDialogue.2"),
				_ => "This text should never be seen! Please report to https://github.com/ProjectStarlight/StarlightRiver/issues",
			};
		}

		private string GetWinDialogue()
		{
			return TextState switch
			{
				0 => GlassweaverWaitingText("WinDialogue.0"),
				1 => GlassweaverWaitingText("WinDialogue.1"),
				2 => GlassweaverWaitingText("WinDialogue.2"),
				3 => GlassweaverWaitingText("WinDialogue.3"),
				4 => GlassweaverWaitingText("WinDialogue.4"),
				_ => "This text should never be seen! Please report to https://github.com/ProjectStarlight/StarlightRiver/issues",
			};
		}

		private string GetKeyDialogue()
		{
			return Main.rand.Next(3) switch
			{
				0 => GlassweaverWaitingText("KeyDialogue.0"),
				1 => GlassweaverWaitingText("KeyDialogue.1"),
				2 => GlassweaverWaitingText("KeyDialogue.2"),
				_ => "This text should never be seen! Please report to https://github.com/ProjectStarlight/StarlightRiver/issues",
			};
		}

		public override string GetChat()
		{
			if (State == 0) //Waiting at entrance
			{
				
				RichTextBox.OpenDialogue(NPC, GlassweaverWaitingText("Title"), GetIntroDialogue());
				RichTextBox.AddButton(GlassweaverWaitingText("Button.TellMeMore"), () =>
				{
					TextState++;
					RichTextBox.SetData(NPC, GlassweaverWaitingText("Title"), GetIntroDialogue());

					if (TextState >= 5)
					{
						RichTextBox.ClearButtons();

						RichTextBox.AddButton(GlassweaverWaitingText("Button.ShowMe"), () =>
						{
							CameraSystem.AsymetricalPan(180, 240, 150, ArenaPos);
							RichTextBox.SetData(NPC, GlassweaverWaitingText("Title"), GetAfterCameraDialogue());
						});

						RichTextBox.AddButton(GlassweaverWaitingText("Button.SeeYouLater"), () =>
						{
							RichTextBox.CloseDialogue();
							State = 1;
							Timer = 0;
						});

						TextState = 0;
					}
				});
			}
			else if (State == 2) //Waiting in arena
			{
				RichTextBox.OpenDialogue(NPC, GlassweaverWaitingText("Title"), GetWaitingDialogue());

				RichTextBox.AddButton(GlassweaverWaitingText("Button.Fight"), () =>
				{
					NPC.NewNPC(NPC.GetSource_FromThis(), (int)NPC.Center.X, (int)NPC.Center.Y, NPCType<Glassweaver>());
					RichTextBox.CloseDialogue();
					NPC.active = false;
				});

				RichTextBox.AddButton(GlassweaverWaitingText("Button.SeeYouLater"), () => RichTextBox.CloseDialogue());
			}
			else if (State == 3) //After winning
			{
				RichTextBox.OpenDialogue(NPC, GlassweaverWaitingText("Title"), GetWinDialogue());
				RichTextBox.AddButton(GlassweaverWaitingText("Button.TellMeMore"), () =>
				{
					TextState++;
					RichTextBox.SetData(NPC, GlassweaverWaitingText("Title"), GetWinDialogue());

					if (TextState >= 4)
					{
						RichTextBox.ClearButtons();

						RichTextBox.AddButton(GlassweaverWaitingText("Button.INeedAKey"), () =>
						{
							if (Helpers.Helper.HasItem(Main.LocalPlayer, ItemType<Items.Vitric.TempleEntranceKey>(), 1))
							{
								RichTextBox.SetData(NPC, GlassweaverWaitingText("Title"), GetKeyDialogue());
							}
							else
							{
								Item.NewItem(NPC.GetSource_FromThis(), NPC.Center, ItemType<Items.Vitric.TempleEntranceKey>());
							}
						});

						RichTextBox.AddButton(GlassweaverWaitingText("Button.SeeYouLater"), () =>
						{
							StarlightWorld.Flag(WorldFlags.GlassweaverDowned);

							RichTextBox.CloseDialogue();
							State = 4;
							Timer = 0;
						});
					}
				});
			}
			else if (State == 4) //After talking after win
			{
				RichTextBox.SetData(NPC, GlassweaverWaitingText("Title"), GetKeyDialogue());

				RichTextBox.ClearButtons();

				RichTextBox.AddButton(GlassweaverWaitingText("Button.INeedAKey"), () =>
				{
					if (Helpers.Helper.HasItem(Main.LocalPlayer, ItemType<Items.Vitric.TempleEntranceKey>(), 1))
					{
						RichTextBox.SetData(NPC, GlassweaverWaitingText("Title"), GetKeyDialogue());
					}
					else
					{
						Item.NewItem(NPC.GetSource_FromThis(), NPC.Center, ItemType<Items.Vitric.TempleEntranceKey>());
						RichTextBox.CloseDialogue();
					}
				});

				RichTextBox.AddButton(GlassweaverWaitingText("Button.SeeYouLater"), () =>
				{
					RichTextBox.CloseDialogue();
					State = 4;
					Timer = 0;
				});
			}

			return "";
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D tex = Request<Texture2D>(Texture).Value;
			Vector2 pos = NPC.Center - Vector2.UnitY * 14 - Main.screenPosition;
			Vector2 origin = new Vector2(FRAME_WIDTH, FRAME_HEIGHT) * 0.5f;

			spriteBatch.Draw(tex, pos, NPC.frame, drawColor, NPC.rotation, origin, NPC.scale, NPC.direction == 1 ? 0 : SpriteEffects.FlipHorizontally, 0);

			return false;
		}

		public override void SaveData(TagCompound tag)
		{
			tag.Add("State", State);
		}

		public override void LoadData(TagCompound tag)
		{
			State = tag.GetFloat("State");
		}
	}
}