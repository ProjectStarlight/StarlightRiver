using NetEasy;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using StarlightRiver.NPCs;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.ItemDropRules;
using Microsoft.Xna.Framework;

namespace StarlightRiver.Content.Items.Misc
{
    public class SoulOfFrog : SmartAccessory
    {
        public override string Texture => AssetDirectory.MiscItem + Name;

        public SoulOfFrog() : base("Soul Of Frog", "Frogs are resurrected on death, growing stronger with each return from the brink.\n" +
                                                    "Killing other critters has a chance to reveal them as actually being a frog the whole time\n" +
                                                    "They will then be resurrected as if they were a frog the whole time, because they were") { }

        public override void SafeSetDefaults()
        {
            Item.value = 1;
            Item.rare = ItemRarityID.LightRed;
        }

        public override void SafeUpdateEquip(Player Player)
        {
            Player.GetModPlayer<FrogPlayer>().equipped = true;
        }
    }

    public class FrogPlayer : ModPlayer
    {
        public bool equipped = false;

        public override void ResetEffects()
        {
            equipped = false;
        }
    }

    public class SoulOfFrogGNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;


        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            if (npc.type == NPCID.Frog)
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<SoulOfFrog>(), 1000));
        }

        public override void OnHitByItem(NPC NPC, Player Player, Item Item, int damage, float knockback, bool crit)
        {
            if (Player.GetModPlayer<FrogPlayer>().equipped == true && NPC.life <= 0)
            {
                TrySummonFrog(NPC);
            }
        }

        public override void OnHitByProjectile(NPC NPC, Projectile Projectile, int damage, float knockback, bool crit)
        {
            Player Player = Main.player[Projectile.owner];
            if (Player.GetModPlayer<FrogPlayer>().equipped == true && NPC.life <= 0)
            {
                TrySummonFrog(NPC);
            }
        }

        private static void TrySummonFrog(NPC NPC)
        {
            if (NPC.type == NPCID.Frog || (Main.rand.NextBool(5) && NPC.catchItem > 0))
            {
                NPC frog = Main.npc[NPC.NewNPC(NPC.GetSource_FromThis(), (int)NPC.Center.X, (int)NPC.Center.Y + 5, NPCID.Frog)];
                if (NPC.type == NPCID.Frog)
                {
                    frog.scale = NPC.scale + 0.01f;

                    int oldWidth = frog.width;
                    int oldHeight = frog.height;

                    frog.width = (int)(frog.scale * frog.width);
                    frog.height = (int)(frog.scale * frog.height);

                    frog.Center = NPC.Center - new Vector2(0, frog.height - NPC.height);
                }
            }
        }
    }
}