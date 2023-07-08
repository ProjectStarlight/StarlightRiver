using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.GUI;
using StarlightRiver.Content.Items.Vitric;
using StarlightRiver.Content.Packets;
using StarlightRiver.Core.Systems.CameraSystem;
using System;
using Terraria.ID;
using Terraria.ModLoader.IO;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.GlassMiniboss
{
	class GlassweaverWaiting : ModNPC, IHintable
	{
		public const int FRAME_WIDTH = 124;

		public const int FRAME_HEIGHT = 92;

		public Player talkingTo;

		public int TextState = 0; // Client based instead

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
			NPC.dontTakeDamage = true;
			NPC.immortal = true;

			Timer++;
			VisualTimer++;

			if (State < 0 || State > 5)
				State = StarlightWorld.HasFlag(WorldFlags.GlassweaverDowned) ? 3 : 0;

			if (Main.netMode != NetmodeID.Server) // Client based stuff
			{
				if (talkingTo != null && Vector2.Distance(talkingTo.Center, NPC.Center) > 2000)
				{
					talkingTo = null;
					TextState = 0;
					RichTextBox.CloseDialogue();
				}

				if (talkingTo != null && talkingTo.TalkNPC != NPC)
				{
					talkingTo = null;
					TextState = 0;
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
		}

		public override bool CheckActive()
		{
			if (Main.netMode == NetmodeID.MultiplayerClient && !NPC.active) // Close any dialogs if the npc is inactive.
			{
				talkingTo = null;
				TextState = 0;
				RichTextBox.CloseDialogue();
			}

			return true;
		}

		private string GetIntroDialogue()
		{
			return TextState switch
			{
				0 => "\"Ah, you've finally found me. Come to browse my wares? My commissions are full, but for the right price, I'm sure we can work something out. Or... that look you're giving me... Are you perhaps a fan?\"",
				1 => "\"I- Wh- What? What do you mean, you just 'wandered in here'? Nobody comes to see the Legendary Weaver of Glass without a reason, especially outside of my forge! It must have been... FATE!\"",
				2 => "\"Yes, yes, you see, I've actually been having some trouble lately. I know, it's hard to believe, me, in trouble, right, but I do need the assistance of a daring one such as you. Think of it as... a quest, or something. Adventurers love those, right?\"",
				3 => "\"I am an artist, and crystal is my medium, but to properly work it I need some very powerful equipment, and, well, how should I say this... I did maybe forget to maintain the tools in my chosen forge for a bit. Only a few centuries. Don't give me that look.\"",
				4 => "\"You might not be able to tell with your lack of experience and all, but I give it only a few more years before the magma channels erode and the forgeheart collapses. Sooo... I've been diligently scouting out a nearby Forge Temple. Thing is, it's almost in even more disrepair!\"",
				5 => "\"If you, handsome, wise, kind explorer, are brave enough to venture in and fix up the forge, I'll let you keep whatever you find inside. Here, I'll show you where it is!\"",
				_ => "This text should never be seen! Please report to https://github.com/ProjectStarlight/StarlightRiver/issues",
			};
		}

		private string GetAfterCameraDialogue()
		{
			return "\"No- Wait! You can't just barge in, you need a key. I need to know if I am giving my faith and crystal to the right person, so when you think you're strong enough, challenge me properly and I'll put you through my Glassweaver Gauntlet to really test your mettle! Don't keep me waiting! You know where to find me.\"";
		}

		private string GetWaitingDialogue()
		{
			return Main.rand.Next(3) switch
			{
				0 => "\"Are you finally ready to prove your worth? It's honestly very hard for me to tell, you lot are always so small and scrawny.\"",
				1 => "\"Adventurer, you return! Are you here to challenge me, or just gaze at my wonderful works?\"",
				2 => "\"Ah, my fated champion! Here to liberate my forge from that dastardly Sentinel? Er- forget I said that. I'll tell you about it if you win.\"",
				_ => "This text should never be seen! Please report to https://github.com/ProjectStarlight/StarlightRiver/issues",
			};
		}

		private string GetWinDialogue()
		{
			return TextState switch
			{
				0 => "\"Congratulations, you destroyed many decades of my finest work! I really should have thought this through more. I was going to have those help you clear out the forge, but... I'm sure you'll figure it out. Let me fill you in on what to expect.",
				1 => "\"Here is my Great Map of the Grand Forge Temple. Just another legendary artwork that I'm allowing you to briefly look at, free of charge.\"",
				2 => "It pulls out what looks to be a map, but with completely illegible scribbles in place of anything useful. You're not sure how one capable of creating such intricate crystal work is so inept at moving a pen on paper, but you've seen more bizzare things...",
				3 => "\"You can see the two main rooms of the forge at the heart of the temple. Both are most likely broken in some way, and if you want access to the vault at the bottom, you'll have to get the power system and hammer system in working order.\"",
				4 => "\"If you enter the vault, there's a pretty high likelihood of the Sentinel taking notice. It will not be happy, but most adventurers have no such compunctions about theft from holy forge-temples and the murder of religious guardians, so you'll probably be able to take it out just fine.\"",
				5 => "\"Why are you looking at me like that? Did I not tell you about the Sentinel before? Ah. It's just a... slightly oversized ceramic snake. That's all. It is not capable of melting the entire temple if you do not escape fast enough. Nope. Just talk to me when you defeat that slightly oversized snake so I can finally move in.\"",
				6 => "\"Anyway, here's the Temple Key. Why do you suddenly look so uneasy? Remember your duty, adventurer, and get me my forge temple!\"",
				-1 => "It hands you an old scroll. You unravel the bindings expecting a map, but what you are looking at... is definitely not a map.",
				-2 => "It looks at you, befuddled, before pulling out a stained papyrus that looks to be a map, but with completely illegible scribbles in place of anything useful. You're not sure how one capable of creating such intricate crystal work is so inept at moving a pen on paper, but you've seen more bizzare things... Especially what it gave you previously.",
				-3 => "\"How rude of you to leave me mid-explanation! Should I really trust you with the future of my work? But given you have proven your strength, I do suppose you are the best choice.\"",
				-4 => "\"I haven't even told you how to get into the temple yet. So, as I was saying...\"",
				_ => "This text should never be seen! Please report to https://github.com/ProjectStarlight/StarlightRiver/issues",
			};
		}

		private string GetKeyDialogue()
		{
			if (StarlightWorld.HasFlag(WorldFlags.VitricBossDowned))
				return GetPostDialogue();

			return Main.rand.Next(3) switch
			{
				0 => "\"It doesn't look like you've cleared out the forge. What are you standing there for? I'm busy standing here!\"",
				1 => "\"Don't you have better things to be doing, like fixing my forge?\"",
				2 => "\"Adventurer! You defeated the Sentinel so soon? It's a miracle! Oh. Oh, you didn't. Never mind. Go away.\"",
				_ => "This text should never be seen! Please report to https://github.com/ProjectStarlight/StarlightRiver/issues",
			};
		}

		private string GetPostDialogue()
		{
			return Main.rand.Next(3) switch
			{
				0 => "\"Seems like you got some of that ceramic dust on you... oh dont worry, it probably doesn't cause lung cancer. Anyways thanks for taking care of that sentinel! I'll get my things ready to move in...\"",
				1 => "\"Wow, I wasn't sure you could handle that! Erm, I mean, I totally believed in you the whole time. Totally didnt have money on the sentinel. Anyways, I'll be moving in soon.\"",
				2 => "\"Dont worry about the burn marks, they'll buff out. Thanks for clearing out that sentinel for me, I'll be moving in soon, so dont go poking through my stuff.\"",
				_ => "This text should never be seen! Please report to https://github.com/ProjectStarlight/StarlightRiver/issues",
			};
		}

		public override string GetChat()
		{
			if (Main.netMode == NetmodeID.Server) // Dialog only for client or in singleplayer.
				return "";

			talkingTo = Main.LocalPlayer;

			if (State == 0) //Waiting at entrance
			{
				RichTextBox.OpenDialogue(NPC, "???", GetIntroDialogue());
				RichTextBox.AddButton("\"What? No.\"", () =>
				{
					TextState++;
					RichTextBox.SetData(NPC, "Glassweaver", GetIntroDialogue());
					if (TextState == 1)
					{
						TextState++;
						RichTextBox.ClearButtons();
						RichTextBox.AddButton("\"...\"", () =>
						{
							TextState++;
							RichTextBox.SetData(NPC, "Glassweaver", GetIntroDialogue());
							if (TextState >= 5)
							{
								RichTextBox.ClearButtons();

								RichTextBox.AddButton("\"Show me that temple.\"", () =>
								{
									CameraSystem.AsymetricalPan(180, 240, 150, StarlightWorld.vitricBiome.Center.ToVector2() * 16);
									RichTextBox.SetData(NPC, "Glassweaver", GetAfterCameraDialogue());
									RichTextBox.ClearButtons();

									RichTextBox.AddButton("\"See you around.\"", () =>
									{
										GlassweaverWaitingPacket statusPacket = new GlassweaverWaitingPacket(newState: 1, newTimer: 0, npcWhoAmI: NPC.whoAmI);
										statusPacket.Send();

										RichTextBox.CloseDialogue();
									});
								});
							}
						});
					}
				});
			}
			else if (State == 2) //Waiting in arena
			{
				RichTextBox.OpenDialogue(NPC, "Glassweaver", GetWaitingDialogue());

				RichTextBox.AddButton("\"I'm ready.\"", () =>
				{
					RichTextBox.CloseDialogue();

					SacrificeNPCPacket nPacket = new SacrificeNPCPacket((int)NPC.Center.X, (int)NPC.Center.Y, NPCType<Glassweaver>(), NPC.whoAmI);
					nPacket.Send();
				});

				RichTextBox.AddButton("Close", RichTextBox.CloseDialogue);
			}
			else if (State == 3) //After winning
			{
				RichTextBox.OpenDialogue(NPC, "Glassweaver", GetWinDialogue());
				RichTextBox.AddButton("\"...\"", () =>
				{
					DoNormalWinDialogue();
					if (TextState == -1)
					{
						RichTextBox.ClearButtons();

						(Main.LocalPlayer.QuickSpawnItemDirect(NPC.GetSource_FromThis(), ItemType<ForgeMap>()).ModItem as ForgeMap).isEpic = true;
						State = 4; //only gets set locally

						RichTextBox.AddButton("\"Uhhh... What?\"", () =>
						{
							Main.LocalPlayer.QuickSpawnItemDirect(NPC.GetSource_FromThis(), ItemType<ForgeMap>());
							TextState = -2;
							RichTextBox.SetData(NPC, "Glassweaver", GetWinDialogue());

							RichTextBox.ClearButtons();

							TextState = 2;
							RichTextBox.AddButton("\"...\"", () => DoNormalWinDialogue());
						});

						RichTextBox.AddButton("(Say Nothing)", () =>
						{
							RichTextBox.ClearButtons();

							TextState = 2;
							DoNormalWinDialogue();

							RichTextBox.AddButton("\"...\"", () => DoNormalWinDialogue());
						});
					}
					else if (TextState == 2)
					{
						Main.LocalPlayer.QuickSpawnItemDirect(NPC.GetSource_FromThis(), ItemType<ForgeMap>());
						State = 4; //only gets set locally
					}
				});
			}
			else if (State == 4) //After giving you the map
			{
				TextState = -3;

				RichTextBox.OpenDialogue(NPC, "Glassweaver", GetWinDialogue());
				RichTextBox.AddButton("\"...\"", () =>
				{
					TextState = -4;
					RichTextBox.SetData(NPC, "Glassweaver", GetWinDialogue());

					RichTextBox.ClearButtons();

					RichTextBox.AddButton("\"...\"", () =>
					{
						RichTextBox.ClearButtons();

						TextState = 2;
						DoNormalWinDialogue();

						RichTextBox.AddButton("\"...\"", () => DoNormalWinDialogue());
					});
				});
			}
			else if (State == 5) //After talking after win
			{
				RichTextBox.OpenDialogue(NPC, "Glassweaver", GetKeyDialogue());
				RichTextBox.SetData(NPC, "Glassweaver", GetKeyDialogue());

				RichTextBox.ClearButtons();

				RichTextBox.AddButton("I need a key", () =>
				{
					if (Helpers.Helper.HasItem(Main.LocalPlayer, ItemType<Items.Vitric.TempleEntranceKey>(), 1))
					{
						RichTextBox.SetData(NPC, "Glassweaver", GetKeyDialogue());
					}
					else
					{
						Main.LocalPlayer.QuickSpawnItem(NPC.GetSource_FromThis(), ItemType<Items.Vitric.TempleEntranceKey>());
						RichTextBox.CloseDialogue();
					}
				});

				RichTextBox.AddButton("Rematch", () =>
				{
					RichTextBox.CloseDialogue();

					SacrificeNPCPacket nPacket = new SacrificeNPCPacket((int)NPC.Center.X, (int)NPC.Center.Y, NPCType<Glassweaver>(), NPC.whoAmI);
					nPacket.Send();
				});

				RichTextBox.AddButton("See you later", () =>
				{
					RichTextBox.CloseDialogue();

					Timer = 0;
					State = 5; //only gets set locally
				});
			}

			return "";
		}

		public void DoNormalWinDialogue()
		{
			if (TextState == 1 && Main.rand.NextFloat() < 0.01f)
				TextState = -1;
			else
				TextState++;

			RichTextBox.SetData(NPC, "Glassweaver", GetWinDialogue());
			if (TextState == 4)
			{
				RichTextBox.ClearButtons();

				RichTextBox.AddButton("\"Wait, what?\"", () =>
				{
					TextState++;
					RichTextBox.SetData(NPC, "Glassweaver", GetWinDialogue());
					Main.LocalPlayer.QuickSpawnItem(NPC.GetSource_FromThis(), ItemType<Items.Vitric.TempleEntranceKey>());

					RichTextBox.ClearButtons();

					RichTextBox.AddButton("I need a key.", () =>
					{
						if (Helpers.Helper.HasItem(Main.LocalPlayer, ItemType<Items.Vitric.TempleEntranceKey>(), 1))
						{
							RichTextBox.SetData(NPC, "Glassweaver", GetKeyDialogue());
						}
						else
						{
							Main.LocalPlayer.QuickSpawnItem(NPC.GetSource_FromThis(), ItemType<Items.Vitric.TempleEntranceKey>());
							RichTextBox.CloseDialogue();
						}
					});

					RichTextBox.AddButton("Close", () =>
					{
						StarlightWorld.Flag(WorldFlags.GlassweaverDowned);

						RichTextBox.CloseDialogue();

						Timer = 0;
						State = 5; //only gets set locally
					});
				});
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D tex = Request<Texture2D>(Texture).Value;
			Vector2 pos = NPC.Center - Vector2.UnitY * 14 - Main.screenPosition;
			Vector2 origin = new Vector2(FRAME_WIDTH, FRAME_HEIGHT) * 0.5f;

			spriteBatch.Draw(tex, pos, NPC.frame, drawColor, NPC.rotation, origin, NPC.scale, NPC.direction == 1 ? 0 : SpriteEffects.FlipHorizontally, 0);

			if ((State == 0 || State == 2 || State == 3) && talkingTo is null)
			{
				Texture2D exclaim = Request<Texture2D>("StarlightRiver/Assets/Misc/Exclaim").Value;
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

		public string GetHint()
		{
			return "Is this... creature... really what 'lurks beneath the desert?'";
		}
	}
}