using Microsoft.Xna.Framework;
using NetEasy;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Helpers;
using System;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Core 
{
	public class ControlsPlayer : ModPlayer
    {
        //this is used to keep track of player controls that are otherwise not possible to keep in sync (wtf tml why does terraria sync altfunctionuse but not for modded items)


        /// <summary>
        /// technically called the "interact" key in game
        /// </summary>
        public bool mouseRight = false;

        private bool oldMouseRight = false;

        public Vector2 mouseWorld;

        private Vector2 oldMouseWorld;

        /// <summary>
        /// Set this to true when something wants to send controls
        /// sets itself to false after one send
        /// </summary>
        public bool sendControls = false;


        /// <summary>
        /// set this to true when something wants to recieve updates on the mouseworld changes 
        /// this particular mouse listener will send many changes and is generally tight tolerance, sending much more frequently
        /// </summary>
        public bool mouseListener = false;

        /// <summary>
        /// set this to true when something wants to recieve updates on mouseworld changes 
        /// this particular mouse listener will only send changes when the rotation to the player changes, sending much less frequently
        /// </summary>
        public bool mouseRotationListener = false;

        /// <summary>
        /// set this to true when something wants to listen for the value of right click changing
        /// sends immediately when right click value changes. sets it self to false each frame
        /// </summary>
        public bool rightClickListener = false;
        public override void PreUpdate()
        {
            if (Main.myPlayer == player.whoAmI)
            {
                mouseRight = PlayerInput.Triggers.Current.MouseRight;
                mouseWorld = Main.MouseWorld;

                if (rightClickListener && mouseRight != oldMouseRight)
                {
                    oldMouseRight = mouseRight;
                    sendControls = true;
                    rightClickListener = false;
                }

                if (mouseListener && Vector2.Distance(mouseWorld, oldMouseWorld) > 10f)
                {
                    oldMouseWorld = mouseWorld;
                    sendControls = true;
                    mouseListener = false;
                } 
                
                if (mouseRotationListener && Math.Abs((mouseWorld - player.MountedCenter).ToRotation() - (oldMouseWorld - player.MountedCenter).ToRotation()) > 0.15f)
                {
                    oldMouseWorld = mouseWorld;
                    sendControls = true;
                    mouseRotationListener = false;
                }

                if (sendControls)
                {
                    sendControls = false;
                    ControlsPacket packet = new ControlsPacket(this);
                    packet.Send(-1, player.whoAmI, false);
                }

            }
        }
    }

    [Serializable]
    public class ControlsPacket : Module
    {
        public readonly sbyte whoAmI;
        public readonly byte controls;
        public readonly short xDist;
        public readonly short yDist;

        public ControlsPacket(ControlsPlayer cPlayer)
        {
            whoAmI = (sbyte)cPlayer.player.whoAmI;

            if (cPlayer.mouseRight) controls |= 0b10000000;

            xDist = (short)(cPlayer.mouseWorld.X - cPlayer.player.position.X);
            yDist = (short)(cPlayer.mouseWorld.Y - cPlayer.player.position.Y);

        }

        protected override void Receive()
        {
            ControlsPlayer player = Main.player[whoAmI].GetModPlayer<ControlsPlayer>();
            if ((controls & 0b10000000) == 0b10000000)
                player.mouseRight = true;
            else
                player.mouseRight = false;

            player.mouseWorld = new Vector2(xDist + player.player.position.X, yDist + player.player.position.Y);

            if (Main.netMode == Terraria.ID.NetmodeID.Server)
            {
                Send(-1, player.player.whoAmI, false);
                return;
            }
        }
    }
}
