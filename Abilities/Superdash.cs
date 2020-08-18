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
        public override bool Available => !User.mount.Active;

        public override void OnCast()
        {
            timer = 4;

            Main.PlaySound(SoundID.Item8);

            for (int k = 0; k <= 10; k++)
            {
                Dust.NewDust(User.Center - new Vector2(100, 100), 200, 200, DustType<Void2>(), 0, 0, 0, default, 1.2f);
            }

            objective = new Vector2
                (
                Main.screenPosition.X + Main.mouseX - User.position.X,
                Main.screenPosition.Y + Main.mouseY - User.position.Y
                );
            start = User.position;
        }

        public override void UpdateActive()
        {
            if (timer >= 1 && !(Vector2.Distance(User.position, start) >= objective.Length() || ((User.position - User.oldPosition).Length() < 14) && timer <= 3)) // superdash action
            {
                User.maxFallSpeed = 999;
                User.velocity = Vector2.Normalize(objective) * 15;
                User.immune = true;
                User.immuneTime = 5;

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

                float rot = Vector2.Normalize(User.velocity).ToRotation();
                float x = User.Center.X + (float)Math.Sin(rot) * ((float)Math.Sin(timer) * 20);
                float y = User.Center.Y + (float)Math.Cos(rot) * ((float)Math.Sin(timer) * -20);

                Dust.NewDustPerfect(new Vector2(x, y), DustType<Dusts.Void>());

                for (int k = 0; k <= 10; k++)
                {
                    Dust.NewDust(new Vector2(x, y), 10, 10, DustType<Dusts.Void>(), Main.rand.Next(-20, 20), Main.rand.Next(-20, 20), 0, default, 0.5f);
                }

                for (int k = 0; k <= 10; k++)
                {
                    Dust.NewDust(User.Center - new Vector2(User.height / 2, User.height / 2), User.height, User.height, DustType<Dusts.Void>(), Main.rand.Next(-50, 50), Main.rand.Next(-50, 50), 0, default, 0.4f);
                }
            }

            if (((Vector2.Distance(User.position, start) >= objective.Length() || ((User.position - User.oldPosition).Length() < 14) && timer <= 3) && objective != new Vector2(0, 0)))
            {
                Active = false;
                OnExit();
            }
        }

        public override void OffCooldownEffects()
        {
            for (float k = 0; k <= 6.28f; k += 0.05f)
            {
                Dust dus = Dust.NewDustPerfect(User.Center, DustType<Void4>(), Vector2.One.RotatedBy(k));
                dus.customData = User;
            }
            Main.PlaySound(SoundID.Item105);
        }

        public override void OnExit()
        {
            User.velocity.X = 0;
            User.velocity.Y = 0;
            User.immune = false;
            objective = new Vector2(0, 0);
            timer = 0;

            Cooldown = 120; //Cooldown placed after since duration is variable

            for (int k = 0; k <= 100; k++)
            {
                Dust.NewDust(User.Center - new Vector2(User.height / 2, User.height / 2), User.height, User.height, DustType<Dusts.Void>(), Main.rand.Next(-70, 70), Main.rand.Next(-70, 70), 0, default, 1.2f);
            }

            Main.PlaySound(SoundID.Item38);
        }
    }
}