using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Items.Prototypes
{
    internal class PulseCannon : PrototypeWeapon
    {
        public PulseCannon() : base(10, BreakType.MaxUses)
        {
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Pulse Cannon");
            Tooltip.SetDefault("Pushes enemies away with incredible force");
        }

        public override void SetDefaults()
        {
            item.damage = 0;
            item.useStyle = ItemUseStyleID.HoldingUp;
            item.useTime = 90;
            item.useAnimation = 90;
        }

        public override bool SafeUseItem(Player player)
        {
            Main.PlaySound(SoundID.NPCHit43, player.Center);
            Vector2 dir = Vector2.Normalize(player.Center - Main.MouseWorld);
            for (int k = 0; k <= 100; k++)
            {
                Dust.NewDustPerfect(player.Center, DustType<Dusts.Starlight>(), dir.RotatedByRandom(0.5f) * -Main.rand.NextFloat(200));
            }
            for (int k = 1; k <= 6; k++)
            {
                for (float n = 0; n <= 6.28f; n += 0.02f)
                {
                    Vector2 off = new Vector2((float)Math.Cos(n), (float)Math.Sin(n) * 2) * (20 + k * 5);
                    Dust.NewDustPerfect(player.Center + off.RotatedBy(dir.ToRotation()), DustType<Dusts.Starlight>(), dir * -k * 40);
                }
            }
            foreach (NPC npc in Main.npc.Where(npc => Vector2.Distance(player.Center, npc.Center) <= 800))
            {
                npc.velocity += Vector2.Normalize(player.Center - npc.Center) * -30;
            }
            return true;
        }
    }
}