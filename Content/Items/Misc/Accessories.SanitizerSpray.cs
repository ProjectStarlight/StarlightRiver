using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Helpers;
using System.Collections.Generic;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Misc
{
    public class SanitizerSpray : SmartAccessory
    {
        // 30 Tiles.
        private const float transferRadius = 480;

        // 5 Seconds.
        private const int transferredBuffDuration = 300;

        public override string Texture => AssetDirectory.MiscItem + Name;

        public SanitizerSpray() : base("Sanitizer Spray", "Critical strikes have a 25% chance to transfer one of your debuffs to nearby enemies") { }

        public override bool Autoload(ref string name)
        {
            StarlightPlayer.OnHitNPCEvent += OnHitNPCAccessory;
            StarlightPlayer.OnHitNPCWithProjEvent += OnHitNPCWithProjAccessory;

            return true;
        }
        private void OnHit(Player player, bool crit)
        {
            if (Equipped(player) && crit && Main.rand.NextFloat() < 0.25f)
            {
                TransferRandomDebuffToNearbyEnemies(player);
            }
        }

        private void OnHitNPCAccessory(Player player, Item item, NPC target, int damage, float knockback, bool crit) 
            => OnHit(player, crit);
        private void OnHitNPCWithProjAccessory(Player player, Projectile proj, NPC target, int damage, float knockback, bool crit) 
            => OnHit(player, crit);

        public static void TransferRandomDebuffToNearbyEnemies(Player player)
        {
            List<int> activeDebuffIds = new List<int>();

            for (int i = 0; i < Player.MaxBuffs; i++)
            {
                if (Helper.IsValidDebuff(player, i))
                {
                    activeDebuffIds.Add(player.buffType[i]);
                }
            }

            int type = Main.rand.Next(activeDebuffIds);

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];

                if (npc.CanBeChasedBy() && Vector2.DistanceSquared(player.Center, npc.Center) < transferRadius * transferRadius)
                {
                    npc.AddBuff(type, transferredBuffDuration);
                }
            }
        }
    }
}