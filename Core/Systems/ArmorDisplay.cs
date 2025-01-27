using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Terraria.WorldBuilding;

namespace StarlightRiver.Core.Systems
{
	internal class ArmorDisplay : GlobalNPC
	{
		public float currentAnim;
		public float maxOverblow;

		public override bool InstancePerEntity => true;

		public override void Load()
		{
			StarlightNPC.PostDrawHealthBarEvent += DrawArmorDisplay;
		}

		public override void SetDefaults(NPC entity)
		{
			currentAnim = entity.defDefense > 0 ? 1 : 0;
		}

		public override void AI(NPC npc)
		{
			var modifiers = npc.GetIncomingStrikeModifiers(DamageClass.Generic, 0);
			float effectiveDefense = Math.Max(modifiers.Defense.ApplyTo(0), 0);
			float armorPenetration = effectiveDefense * Math.Clamp(modifiers.ScalingArmorPenetration.Value, 0, 1) + modifiers.ArmorPenetration.Value;
			effectiveDefense = Math.Max(effectiveDefense - armorPenetration, 0);

			float maxDef = npc.defDefense;

			if (maxDef == 0)
				maxDef = 10;

			float defAmount = effectiveDefense / maxDef;
			currentAnim += (defAmount - currentAnim) / 16;

			if (currentAnim < 0.01f)
				currentAnim = 0;
		}

		public void DrawArmorDisplay(NPC NPC, byte hbPosition, float scale, Vector2 position)
		{
			var mp = NPC.GetGlobalNPC<ArmorDisplay>();
			var modifiers = NPC.GetIncomingStrikeModifiers(DamageClass.Generic, 0);
			float effectiveDefense = Math.Max(modifiers.Defense.ApplyTo(0), 0);

			if (effectiveDefense > 0 || mp.currentAnim > 0)
			{
				float maxDef = NPC.defDefense;

				if (maxDef == 0)
					maxDef = 10;

				float bright = Lighting.Brightness((int)NPC.Center.X / 16, (int)NPC.Center.Y / 16);

				Texture2D tex = Assets.GUI.DefBar1.Value;

				if (NPC.defDefense > 10)
					tex = Assets.GUI.DefBar2.Value;
				if(NPC.defDefense > 40)
					tex = Assets.GUI.DefBar3.Value;

				// Effective defense calculation taken from tmod stat modifiers for defense in damage calculations
				float armorPenetration = effectiveDefense * Math.Clamp(modifiers.ScalingArmorPenetration.Value, 0, 1) + modifiers.ArmorPenetration.Value;
				effectiveDefense = Math.Max(effectiveDefense - armorPenetration, 0);

				float defAmount = effectiveDefense / maxDef;
				mp.currentAnim += (defAmount - mp.currentAnim) / 16;

				float factor = Math.Min(mp.currentAnim, 1);

				var source = new Rectangle(0, 0, (int)(factor * tex.Width), tex.Height);
				var target = new Rectangle((int)(position.X - Main.screenPosition.X), (int)(position.Y - Main.screenPosition.Y), (int)(factor * tex.Width * scale), (int)(tex.Height * scale));

				Main.spriteBatch.Draw(tex, target, source, Color.White * bright, 0, new Vector2(tex.Width / 2, 0), 0, 0);

				if (effectiveDefense < maxDef)
				{
					Texture2D texLine = Assets.GUI.DefBarLine.Value;

					if (NPC.defDefense > 10)
						texLine = Assets.GUI.DefBar2Line.Value;
					if (NPC.defDefense > 40)
						texLine = Assets.GUI.DefBar3Line.Value;

					var sourceLine = new Rectangle((int)(tex.Width * factor), 0, 2, tex.Height);
					var targetLine = new Rectangle((int)(position.X - Main.screenPosition.X) + (int)(tex.Width * factor * scale), (int)(position.Y - Main.screenPosition.Y), (int)(2 * scale), (int)(tex.Height * scale));

					Main.spriteBatch.Draw(texLine, targetLine, sourceLine, Color.White * bright * 2, 0, new Vector2(tex.Width / 2, 0), 0, 0);
				}

				// If they have MORE defense...
				if (mp.currentAnim > 1)
				{
					tex = Assets.GUI.OverDefBar.Value;
					var overblow = mp.currentAnim - 1;

					if (overblow > mp.maxOverblow)
						mp.maxOverblow = overblow;

					factor = Math.Min(overblow / mp.maxOverblow, 1);

					source = new Rectangle(0, 0, (int)(factor * tex.Width), tex.Height);
					target = new Rectangle((int)(position.X - Main.screenPosition.X), (int)(position.Y - Main.screenPosition.Y) + (int)(4 * scale), (int)(factor * tex.Width * scale), (int)(tex.Height * scale));

					Main.spriteBatch.Draw(tex, target, source, Color.White * bright, 0, new Vector2(tex.Width / 2, 0), 0, 0);

					if (overblow < mp.maxOverblow)
					{
						Texture2D texLine = Assets.GUI.OverDefBarLine.Value;

						var sourceLine = new Rectangle((int)(tex.Width * factor), 0, 2, tex.Height);
						var targetLine = new Rectangle((int)(position.X - Main.screenPosition.X) + (int)(tex.Width * factor * scale), (int)(position.Y - Main.screenPosition.Y) + (int)(4 * scale), (int)(2 * scale), (int)(tex.Height * scale));

						Main.spriteBatch.Draw(texLine, targetLine, sourceLine, Color.White * bright * 2, 0, new Vector2(tex.Width / 2, 0), 0, 0);
					}
				}
			}
		}
	}
}
