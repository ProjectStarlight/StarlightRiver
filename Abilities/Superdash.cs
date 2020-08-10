using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Dusts;
using System;
using System.Runtime.Serialization;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Abilities
{
    [DataContract]
    public class Superdash : Ability
    {
        [DataMember] private float timer = 0;
        [DataMember] private Vector2 objective;
        [DataMember] private Vector2 start;

        public Superdash(Player player) : base(3, player)
        {
        }

        public override Texture2D Texture => GetTexture("StarlightRiver/Pickups/Cloak1");
        public override bool CanUse => !player.mount.Active;

        public override void OnCast()
        {
            timer = 4;

            Main.PlaySound(SoundID.Item8);

            for (int k = 0; k <= 10; k++)
            {
                Dust.NewDust(player.Center - new Vector2(100, 100), 200, 200, DustType<Void2>(), 0, 0, 0, default, 1.2f);
            }

            objective = new Vector2
                (
                Main.screenPosition.X + Main.mouseX - player.position.X,
                Main.screenPosition.Y + Main.mouseY - player.position.Y
                );
            start = player.position;
        }

        public override void InUse()
        {
            if (timer >= 1 && !(Vector2.Distance(player.position, start) >= objective.Length() || ((player.position - player.oldPosition).Length() < 14) && timer <= 3)) // superdash action
            {
                player.maxFallSpeed = 999;
                player.velocity = Vector2.Normalize(objective) * 15;
                player.immune = true;
                player.immuneTime = 5;

                Main.PlaySound(SoundID.Item24);

                timer += ((float)Math.PI * 2 / 12);
                if (timer >= (float)Math.PI * 2)
                {
                    timer = 0;
                }
                if (timer <= 2)
                {
                    timer = 2;
                }

                float rot = Vector2.Normalize(player.velocity).ToRotation();
                float x = player.Center.X + (float)Math.Sin(rot) * ((float)Math.Sin(timer) * 20);
                float y = player.Center.Y + (float)Math.Cos(rot) * ((float)Math.Sin(timer) * -20);

                Dust.NewDustPerfect(new Vector2(x, y), DustType<Dusts.Void>());

                for (int k = 0; k <= 10; k++)
                {
                    Dust.NewDust(new Vector2(x, y), 10, 10, DustType<Dusts.Void>(), Main.rand.Next(-20, 20), Main.rand.Next(-20, 20), 0, default, 0.5f);
                }

                for (int k = 0; k <= 10; k++)
                {
                    Dust.NewDust(player.Center - new Vector2(player.height / 2, player.height / 2), player.height, player.height, DustType<Dusts.Void>(), Main.rand.Next(-50, 50), Main.rand.Next(-50, 50), 0, default, 0.4f);
                }
            }

            if (((Vector2.Distance(player.position, start) >= objective.Length() || ((player.position - player.oldPosition).Length() < 14) && timer <= 3) && objective != new Vector2(0, 0)))
            {
                Active = false;
                OnExit();
            }
        }

        public override void OffCooldownEffects()
        {
            for (float k = 0; k <= 6.28f; k += 0.05f)
            {
                Dust dus = Dust.NewDustPerfect(player.Center, DustType<Void4>(), Vector2.One.RotatedBy(k));
                dus.customData = player;
            }
            Main.PlaySound(SoundID.Item105);
        }

        public override void OnExit()
        {
            player.velocity.X = 0;
            player.velocity.Y = 0;
            player.immune = false;
            objective = new Vector2(0, 0);
            timer = 0;

            Cooldown = 120; //Cooldown placed after since duration is variable

            for (int k = 0; k <= 100; k++)
            {
                Dust.NewDust(player.Center - new Vector2(player.height / 2, player.height / 2), player.height, player.height, DustType<Dusts.Void>(), Main.rand.Next(-70, 70), Main.rand.Next(-70, 70), 0, default, 1.2f);
            }

            Main.PlaySound(SoundID.Item38);
        }
    }
}