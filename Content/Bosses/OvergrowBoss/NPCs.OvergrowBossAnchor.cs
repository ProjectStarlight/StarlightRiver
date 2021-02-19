using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Bosses.OvergrowBoss
{
    internal class OvergrowBossAnchor : ModNPC
    {
        public override string Texture => AssetDirectory.Invisible;

        public override bool CheckActive()
        {
            return false;
        }

        public override void SetDefaults()
        {
            npc.width = 20;
            npc.height = 20;
            npc.lifeMax = 200;
            npc.noGravity = true;
            npc.knockBackResist = 0f;
            npc.HitSound = mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/ChainHit");
        }

        public override void AI()
        {
            if (!StarlightWorld.HasFlag(WorldFlags.OvergrowBossOpen))
                npc.dontTakeDamage = true;
            else npc.dontTakeDamage = false;

            NPC boss = Main.npc.FirstOrDefault(n => n.active && n.type == NPCType<OvergrowBoss>());
            if (boss == null)
                return;

            if (npc.immortal) npc.ai[0]++;
            if (npc.ai[0] >= 30) npc.active = false;
            if (npc.ai[0] > 0)
            {
                Vector2 pos = Vector2.Lerp(npc.Center, boss.Center + Vector2.Normalize(npc.Center - boss.Center) * 80, npc.ai[0] / 30f);
                for (int k = 0; k < 5; k++)
                {
                    Dust.NewDustPerfect(pos, DustType<Dusts.Stamina>(), Vector2.One.RotatedByRandom(6.28f));
                    if (Main.rand.Next(2) == 0) Dust.NewDustPerfect(pos, DustType<Dusts.Stone>(), Vector2.One.RotatedByRandom(6.28f));
                }
                if (npc.ai[0] % 3 == 0) Gore.NewGore(pos, new Vector2(0, 1), mod.GetGoreSlot("Gores/ChainGore"));
                if (npc.ai[0] % 8 == 0) Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/ChainHit").WithPitchVariance(0.4f), pos);
            }
            if (npc.ai[0] == 1)
            {
            }
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            if (!Main.npc.Any(n => n.active && n.type == NPCType<OvergrowBoss>())) return;
            NPC boss = Main.npc.FirstOrDefault(n => n.active && n.type == NPCType<OvergrowBoss>());

            Texture2D tex = GetTexture("StarlightRiver/Assets/Bosses/OvergrowBoss/Chain");
            for (float k = 0; k < 1; k += tex.Height / Vector2.Distance(npc.Center, boss.Center + Vector2.Normalize(npc.Center - boss.Center) * 80))
                if (k > npc.ai[0] / 30f)
                {
                    Vector2 pos = Vector2.Lerp(npc.Center, boss.Center + Vector2.Normalize(npc.Center - boss.Center) * 80, k);
                    spriteBatch.Draw(tex, pos - Main.screenPosition, tex.Frame(), Lighting.GetColor((int)pos.X / 16, (int)pos.Y / 16), (npc.Center - boss.Center).ToRotation() + 1.58f, tex.Frame().Size() / 2, 1, 0, 0);
                }
        }

        public override bool CheckDead()
        {
            npc.HitSound = null;
            npc.life = 1;
            npc.immortal = true;
            return false;
        }
    }
}