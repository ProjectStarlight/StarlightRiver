using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Bosses.GlassMiniboss
{
    class GlassweaverWaiting : ModNPC
    {
        public override string Texture => AssetDirectory.GlassMiniboss + Name;

        public override string TownNPCName() => "";

        public override void SetStaticDefaults() => DisplayName.SetDefault("Glassweaver");

        public override void SetDefaults()
        {
            npc.townNPC = true;
            npc.friendly = true;
            npc.width = 64;
            npc.height = 64;
            npc.aiStyle = -1;
            npc.damage = 10;
            npc.defense = 15;
            npc.lifeMax = 250;
            npc.HitSound = SoundID.NPCHit1;
            npc.DeathSound = SoundID.NPCDeath1;
            npc.knockBackResist = 0;
        }

        public override string GetChat()
        {
            // If pre-EOW, warn the player.
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
                NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, NPCType<GlassMiniboss>());
                npc.active = false;
            }
        }
    }
}