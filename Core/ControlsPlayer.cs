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

        /// <summary>
        /// Set this to true when something wants to send controls
        /// sets itself to false after one send
        /// </summary>
        public bool sendControls = false;
        public override void PreUpdate()
        {
            if (Main.myPlayer == player.whoAmI)
            {
                mouseRight = PlayerInput.Triggers.Current.MouseRight;

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

        public ControlsPacket(ControlsPlayer cPlayer)
        {
            whoAmI = (sbyte)cPlayer.player.whoAmI;

            if (cPlayer.mouseRight) controls |= 0b10000000;
            
        }

        protected override void Receive()
        {
            ControlsPlayer player = Main.player[whoAmI].GetModPlayer<ControlsPlayer>();
            if ((controls & 0b10000000) == 0b10000000)
                player.mouseRight = true;
            else
                player.mouseRight = false;

            if (Main.netMode == Terraria.ID.NetmodeID.Server)
            {
                Send(-1, player.player.whoAmI, false);
                return;
            }
        }
    }
}
