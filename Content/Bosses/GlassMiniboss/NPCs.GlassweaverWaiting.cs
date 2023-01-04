using StarlightRiver.Content.GUI;
using StarlightRiver.Core.Systems.CameraSystem;
using Terraria.ID;
using Terraria.ModLoader.IO;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.GlassMiniboss
{
	class GlassweaverWaiting : ModNPC
	{
		Vector2 ArenaPos => StarlightWorld.vitricBiome.TopLeft() * 16 + new Vector2(-48, 80 * 16) + new Vector2(0, 256);

		public override string Texture => AssetDirectory.Glassweaver + Name;

		public ref float Timer => ref NPC.ai[0];
		public ref float State => ref NPC.ai[1];

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
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.knockBackResist = 0;
		}

		public override void AI()
		{
			Timer++;

			if (State == 1)
			{
				NPC.scale -= 1 / 60f;

				if (Timer > 60)
				{
					NPC.Center = ArenaPos;
					NPC.scale = 1;
					State = 2;
				}
			}
		}

		public override string GetChat()
		{
			if (State == 0) //Waiting at entrance
			{
				RichTextBox.OpenDialogue(NPC, "Glassweaver", "Placeholder_Introduction");
				RichTextBox.AddButton("Tell me more", () =>
				{
					RichTextBox.ClearButtons();
					RichTextBox.SetData(NPC, "Glassweaver", "Placeholder_Introduction_2");

					RichTextBox.AddButton("Show me", () =>
					{
						CameraSystem.AsymetricalPan(180, 240, 150, ArenaPos);
						RichTextBox.SetData(NPC, "Glassweaver", "Placeholder_After_Camera");
					});

					RichTextBox.AddButton("See you later", () =>
					{
						RichTextBox.CloseDialogue();
						State = 1;
						Timer = 0;
					});
				});
			}
			else if (State == 2) //Waiting in arena
			{
				RichTextBox.OpenDialogue(NPC, "Glassweaver", "Placeholder_Challenge");

				RichTextBox.AddButton("Fight", () =>
				{
					NPC.NewNPC(NPC.GetSource_FromThis(), (int)NPC.Center.X, (int)NPC.Center.Y, NPCType<Glassweaver>());
					RichTextBox.CloseDialogue();
					NPC.active = false;
				});

				RichTextBox.AddButton("See you later", () => RichTextBox.CloseDialogue());
			}

			return "";

			// If pre-EOW, warn the Player.
			if (!NPC.downedBoss2)
			{
				return Main.rand.Next(new[]
				{
					"You would not dare fight me in your current state.",
					"Pick another battle. For your own sake.",
					"Do not challenge me, adventurer. I would crush you as you are now."
				});
			}

			// If post-EOW, they're on-par.
			else if (!Main.hardMode)
			{
				return Main.rand.Next(new[]
				{
					"I offer my service to those who can best me in battle. Do you dare?",
					"You may be capable of wielding my finest vitric equipment. Prove yourself, or leave.",
					"Prove your worth by defeating me in battle. Then, I will offer my unparalleled glasswork as yours to wield.",
				});
			}

			// If they're in hardmode, they're more than ready.
			else
			{
				return Main.rand.Next(new[]
				{
				"Adventurer, prove yourself in combat. It would be an honor to sell you my vitric gear.",
				"Defeat me in battle and I will gladly offer my glasswork.",
				"You are beyond prepared to challenge me. What are you waiting for?"
				});
			}
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