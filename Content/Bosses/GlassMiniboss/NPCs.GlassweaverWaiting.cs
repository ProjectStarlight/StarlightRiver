using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.GlassMiniboss
{
	class GlassweaverWaiting : ModNPC
	{
		public override string Texture => AssetDirectory.Glassweaver + Name;

		public override void SetStaticDefaults() => DisplayName.SetDefault("Glassweaver");

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

		public override string GetChat()
		{
			// If pre-EOW, warn the Player.
			if (!NPC.downedBoss2)
				return Main.rand.Next(new[]
				{
					"You would not dare fight me in your current state.",
					"Pick another battle. For your own sake.",
					"Do not challenge me, adventurer. I would crush you as you are now."
				});

			// If post-EOW, they're on-par.
			else if (!Main.hardMode)
				return Main.rand.Next(new[]
				{
					"I offer my service to those who can best me in battle. Do you dare?",
					"You may be capable of wielding my finest vitric equipment. Prove yourself, or leave.",
					"Prove your worth by defeating me in battle. Then, I will offer my unparalleled glasswork as yours to wield.",
				});

			// If they're in hardmode, they're more than ready.
			else return Main.rand.Next(new[]
			{
				"Adventurer, prove yourself in combat. It would be an honor to sell you my vitric gear.",
				"Defeat me in battle and I will gladly offer my glasswork.",
				"You are beyond prepared to challenge me. What are you waiting for?"
			});
		}

		public override void SetChatButtons(ref string button, ref string button2)
		{
			button = "Challenge";
		}

		public override void OnChatButtonClicked(bool firstButton, ref bool shop)
		{
			if (firstButton)
			{
				NPC.NewNPC(NPC.GetSource_FromThis(), (int)NPC.Center.X, (int)NPC.Center.Y, NPCType<Glassweaver>());
				NPC.active = false;
			}
		}
	}
}