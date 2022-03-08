using NetEasy;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Content.WorldGeneration;
using StarlightRiver.Core;
using StarlightRiver.NPCs;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
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
            item.value = Item.sellPrice(0, 1, 0, 0);
            item.rare = ItemRarityID.LightRed;
        }

        public override void SafeUpdateEquip(Player player)
        {
            player.GetModPlayer<FrogPlayer>().equipped = true;
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

        public override void OnHitByItem(NPC npc, Player player, Item item, int damage, float knockback, bool crit)
        {
            if (player.GetModPlayer<FrogPlayer>().equipped == true && npc.life <= 0)
            {
                TrySummonFrog(npc);
            }
        }

        public override void OnHitByProjectile(NPC npc, Projectile projectile, int damage, float knockback, bool crit)
        {
            Player player = Main.player[projectile.owner];
            if (player.GetModPlayer<FrogPlayer>().equipped == true && npc.life <= 0)
            {
                TrySummonFrog(npc);
            }
        }

        private static void TrySummonFrog(NPC npc)
        {
            if (npc.type == NPCID.Frog || (Main.rand.NextBool(5) && npc.catchItem > 0))
            {
                NPC frog = Main.npc[NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y + 5, NPCID.Frog)];
                if (npc.type == NPCID.Frog)
                {
                    frog.scale = npc.scale + 0.01f;

                    int oldWidth = frog.width;
                    int oldHeight = frog.height;

                    frog.width = (int)(frog.scale * frog.width);
                    frog.height = (int)(frog.scale * frog.height);

                    frog.Center = npc.Center - new Vector2(0, frog.height - npc.height);
                }
            }
        }
    }
}