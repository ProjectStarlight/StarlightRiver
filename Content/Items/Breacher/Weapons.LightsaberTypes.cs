//TODO:
//Clean up code
//Better Collision
//Fix bug with screen position while it'd thrown
//Better thrown hit cooldown
//All 6 rightclicks (sigh)
//Better sound effects
//Obtainment
//Balance
//Sellprice
//Rarity
//Make it look good when swinging to the left
//Less spritebatch restarts
//Description
//Less spritebatch restarts


using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Core;
using StarlightRiver.Content.Items.Gravedigger;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Terraria.GameContent;

namespace StarlightRiver.Content.Items.Breacher
{
	public class LightsaberProj_Blue : LightsaberProj
	{
		protected override Vector3 BladeColor => new Vector3(0, 0.1f, 0.255f);
	}

	public class LightsaberProj_Green : LightsaberProj
	{
		protected override Vector3 BladeColor => Color.Green.ToVector3();
	}

	public class LightsaberProj_Purple : LightsaberProj
	{
		protected override Vector3 BladeColor => Color.Purple.ToVector3();
	}

	public class LightsaberProj_Red : LightsaberProj
	{
		protected override Vector3 BladeColor => Color.Red.ToVector3();
	}

	public class LightsaberProj_White : LightsaberProj
	{
		protected override Vector3 BladeColor => Color.White.ToVector3();
    }

	public class LightsaberProj_Yellow : LightsaberProj
	{
		protected override Vector3 BladeColor => Color.Yellow.ToVector3();
	}
}