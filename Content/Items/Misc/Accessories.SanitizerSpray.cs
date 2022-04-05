using Microsoft.Xna.Framework;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System.Collections.Generic;
using Terraria;

namespace StarlightRiver.Content.Items.Misc
{
	public class SanitizerSpray : SmartAccessory
    {
        // 30 Tiles.
        private const float transferRadius = 480;

        // 5 Seconds.
        private const int transferredBuffDuration = 300;

        public override string Texture => AssetDirectory.MiscItem + Name;

        public SanitizerSpray() : base("Sanitizer Spray", "Critical strikes have a 25% chance to transfer partial debuff duration to nearby enemies") { }

        public override void Load()
        {
            StarlightPlayer.OnHitNPCEvent += OnHitNPCAccessory;
            StarlightPlayer.OnHitNPCWithProjEvent += OnHitNPCWithProjAccessory;
        }
        private void OnHit(Player Player, bool crit)
        {
            if (Equipped(Player) && crit && Main.rand.NextFloat() < 0.25f)
            {
                TransferRandomDebuffToNearbyEnemies(Player);
            }
        }

        private void OnHitNPCAccessory(Player Player, Item Item, NPC target, int damage, float knockback, bool crit) 
            => OnHit(Player, crit);
        private void OnHitNPCWithProjAccessory(Player Player, Projectile proj, NPC target, int damage, float knockback, bool crit) 
            => OnHit(Player, crit);

        public static void TransferRandomDebuffToNearbyEnemies(Player Player)
        {
            List<int> activeDebuffIds = new List<int>();

            for (int i = 0; i < Player.MaxBuffs; i++)
            {
                if (Helper.IsValidDebuff(Player, i))
                {
                    activeDebuffIds.Add(Player.buffType[i]);
                }
            }

            int type = Main.rand.Next(activeDebuffIds);

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC NPC = Main.npc[i];

                if (NPC.CanBeChasedBy() && Vector2.DistanceSquared(Player.Center, NPC.Center) < transferRadius * transferRadius)
                {
                    NPC.AddBuff(type, transferredBuffDuration);
                }
            }
        }
    }
}