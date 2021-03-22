using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using StarlightRiver.Core;

using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Helpers;

namespace StarlightRiver.Content.Bosses.GlassBoss
{
    class GlassBossLaser : ModProjectile
    {
        public override string Texture => AssetDirectory.GlassBoss + Name;

        public VitricBoss parent;
        public int chargeTime = 300;
        private List<Vector2> nodes = new List<Vector2>();

        private int Timer => chargeTime - projectile.timeLeft;

        public override void SetDefaults()
        {
            projectile.width = 0;
            projectile.height = 0;
            projectile.hostile = true;
            projectile.timeLeft = chargeTime;
        }

        public override void AI()
        {
            if (parent is null)
                return;

            if(Timer == 0)
            {
                nodes.Add(findPointOnRectangle(projectile.rotation, parent.arena, parent.npc.Center));
            }

            projectile.rotation = (projectile.Center - Main.LocalPlayer.Center).ToRotation();
            nodes[0] = findPointOnRectangle(projectile.rotation,
                parent.arena//new Rectangle((int)parent.npc.Center.X - 200, (int)parent.npc.Center.Y - 200, 400, 400)
                , parent.npc.Center);

            projectile.timeLeft = 10; //temporrary for testing
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            var tex = GetTexture(Texture);

            for (int k = 0; k < nodes.Count; k++)
            {
                var node = nodes[k];
                var distance = Vector2.Distance(projectile.Center, node);

                for (float i = 0; i < 1; i += tex.Width / distance)
                {
                    spriteBatch.Draw(tex, Vector2.Lerp(projectile.Center, node, i) - Main.screenPosition, null, Color.White, (projectile.Center - node).ToRotation(), Vector2.Zero, 1, 0, 0);
                }
            }

            for (float i = 0; i < 1; i += tex.Width / 100f)
            {
                spriteBatch.Draw(tex, Vector2.Lerp(projectile.Center, projectile.Center + Vector2.UnitX.RotatedBy((Main.LocalPlayer.Center - projectile.Center).ToRotation()) * 100f, i) - Main.screenPosition, null, Color.Red, (projectile.Center - Main.LocalPlayer.Center ).ToRotation(), Vector2.Zero, 1, 0, 0);
            }

            return false;
        }

        private Vector2 findPointOnRectangle(float rotation, Rectangle box, Vector2 center)
        {
            Vector2 output = new Vector2();
            var saveRotation = rotation;
            rotation = Helper.ConvertAngle(rotation);

            if (!box.Contains(center.ToPoint()))
                throw new ArgumentException("Center point must be contained in the bounding box");

            float thirdCriticalAngle = Helper.ConvertAngle((box.TopRight() - center).ToRotation());
            float fourthCriticalAngle = Helper.ConvertAngle((box.TopLeft() - center).ToRotation());
            float firstCriticalAngle = Helper.ConvertAngle((box.BottomLeft() - center).ToRotation());
            float secondCriticalAngle = Helper.ConvertAngle((box.BottomRight() - center).ToRotation());

            float ratio = box.Width / (float)box.Height;
            float ratio2 = box.Height / (float)box.Width;

            if ((rotation > firstCriticalAngle && rotation < secondCriticalAngle) || 
                (rotation > thirdCriticalAngle && rotation < fourthCriticalAngle))
            {
                float known = (rotation > firstCriticalAngle && rotation < secondCriticalAngle) ? box.Y - center.Y : (box.Y + box.Height) - center.Y;
                output.Y = center.Y + known;
                output.X = center.X + Math.Abs(known) * (float)Math.Sin((rotation + (float)Math.PI / 2) % ((float)Math.PI * 2));
            }
            else
            {
                float known = (rotation > secondCriticalAngle && rotation < thirdCriticalAngle) ? box.X - center.X : (box.X + box.Width) - center.X;
                output.X = center.X + known;
                output.Y = center.Y + Math.Abs(known) * -(float)Math.Sin(rotation);
            }

            Main.NewText("Error: " + (Math.Abs(Helper.CompareAngle(saveRotation, (output - center).ToRotation())) - (float)Math.PI));

            return output;
        }

        private float RefAngle(float input)
        {
            if (input < (float)Math.PI / 2)
                return input;
            else if (input < (float)Math.PI)
                return (float)Math.PI - input;
            else if (input < (float)Math.PI * 1.5f)
                return input - (float)Math.PI;
            else
                return (float)Math.PI * 2 - input;
        }
    }
}
