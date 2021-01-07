using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using StarlightRiver.Core;
using StarlightRiver.Content.Items.Vitric;
using StarlightRiver.Helpers;

namespace StarlightRiver.Content.Bosses.GlassMiniboss
{
    [AutoloadHead]
    class GlassweaverTown : ModNPC
    {
        public override bool CanTownNPCSpawn(int numTownNPCs, int money) => true;

        public override bool CheckConditions(int left, int right, int top, int bottom) => true;

        public override string TownNPCName() => "";

        public override string Texture => AssetDirectory.GlassMiniboss + Name;

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
            npc.dontTakeDamage = true;
        }

        public override string GetChat()
        {
            return Main.rand.Next(new[]
            {
                "I am happy to sell you what you need, adventurer.",
                "It takes incredible delicacy to forge equipment from vitric ore. Don't take my offers for granted.",
                "My craft is honed and perfected for users like yourself. Handle it with care.",
                "My Upgrade Weapon and Quest buttons don't do anything yet. This is still a demo, after all."
            });
        }

        public override void SetChatButtons(ref string button, ref string button2)
        {
            button = "Upgrade Weapon";
            button2 = "Altar?";
        }

        // Unsynced on purpose.
        private int altarIndex;
        private readonly string[] altarChat = new[]
        {
            "That altar is an ancient structure, far older than my stay here.",
            "While my knowledge of it is limited, I know of idols used for worshiping great and forgotten entities.",
            "I have found some of them, scattered across the desert. They're rare, and whoever made them seems long gone.",
            "I haven't bothered disturbing that ominous altar myself, though.",
            "If you wish, I could give you an idol..."
        };

        public override void OnChatButtonClicked(bool firstButton, ref bool shop)
        {
            if (!firstButton)
                if (altarIndex < altarChat.Length - 1)
                    Main.npcChatText = altarChat[altarIndex++];
                else
                {
                    int idol = ModContent.ItemType<GlassIdol>();
                    int type = ModContent.ItemType<VitricOre>();

                    if (Main.LocalPlayer.inventory.ConsumeItems(i => i.type == type, 6))
                    {
                        Main.LocalPlayer.QuickSpawnItem(idol);
                        Main.npcChatText = "Take care. They are remarkably fragile.";
                    }
                    else
                        Main.npcChatText = "Adventurer, my supply of idols is limited. " +
                            "If you bring me some vitric ore, I'll make a replica and give you one. " +
                            "Six ore should suffice.";
                }
            else
            {
                Main.LocalPlayer.HeldItem.GetGlobalItem<GlassReplica>().isReplica = true;
                Main.npcChatText = "Held item turned into replica.";
            }
        }
    }
}