using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Abilities.Faeflame
{
	public class Whip : Ability
    {
        public override string Texture => "StarlightRiver/Assets/Abilities/Faeflame";
        public override float ActivationCostDefault => 0.5f;
        public override Color Color => new Color(255, 247, 126);

        public Vector2 endPoint; //where the "tip" of the whip is in the world
        public bool attached; //if the whip is attached to anything
        public bool endRooted; //if the endpoint is "rooted" to a certain location and cant be moved

        public Vector2 oldMouse;

        public NPC attachedNPC; //if the whip is attached to an npc, what is it attached to?

        public override void Reset()
        {

        }

        public override void OnActivate()
        {
            Player.mount.Dismount(Player);

            endPoint = Player.Center;
        }

        public override void UpdateActive()
        {
            bool control = StarlightRiver.Instance.AbilityKeys.Get<Whip>().Current;

            if(!control)
			{
                endRooted = false;
                attached = false;
                attachedNPC = null;

                endPoint += (Player.Center - endPoint) * 0.15f;

                if (Vector2.Distance(endPoint, Player.Center) < 10)
                    Deactivate();

                oldMouse = Main.MouseScreen;
                return;
            }

            if (!endRooted)
                endPoint += (Main.MouseWorld - endPoint) * 0.05f;
            else
			{
                if (attachedNPC != null && attachedNPC.active)
                    endPoint = attachedNPC.Center;

                //Player.velocity = (Main.MouseScreen - oldMouse) * -1;
                Player.velocity += (Main.MouseScreen - oldMouse) * -0.08f;

                if(Vector2.Distance(Player.Center, endPoint) > 500)
                    Player.velocity += (Player.Center - endPoint) * -0.005f;

                Player.velocity.Y -= 0.43f;

                if (Player.velocity.Length() > 20)
                    Player.velocity = Vector2.Normalize(Player.velocity) * 19.9f;

                Player.velocity *= 0.95f;
            }

            if (Framing.GetTileSafely((int)endPoint.X / 16, (int)endPoint.Y / 16).collisionType == 1) //debug
                endRooted = true;

            oldMouse = Main.MouseScreen;

        }

		public override void DrawActiveEffects(SpriteBatch spriteBatch)
		{
            if (!Active)
                return;

            var tex = ModContent.GetTexture(AssetDirectory.Debug);

            var dist = Vector2.Distance(Player.Center, endPoint);

            for(int k = 0; k < dist; k += 10)
            spriteBatch.Draw(tex, Vector2.Lerp(Player.Center, endPoint, k / (float)dist) - Main.screenPosition, null, Color.White, 0, tex.Size() / 2, 0.25f, 0, 0);
		}

		public override void OnExit()
        {

        }

        public override bool HotKeyMatch(TriggersSet triggers, AbilityHotkeys abilityKeys)
        {
            return abilityKeys.Get<Whip>().Current;
        }
    }
}