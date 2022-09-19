using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Helpers;
using StarlightRiver.Content.Items.Vitric;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using System.Linq;
using System.Collections.Generic;
using Terraria.Graphics.Effects;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.UI.Chat;

namespace StarlightRiver.Core
{
	public class GoreDestroyer : ModSystem
	{
		public override void Load() //extremely hacky but it works, ty Mirsario
		{
			On.Terraria.Gore.NewGore_IEntitySource_Vector2_Vector2_int_float += (orig, entitySource, position, velocity, type, scale) =>
			{
				int result = orig(entitySource, position, velocity, type, scale);

				DestroyGore(entitySource, result);

				return result;
			};
			On.Terraria.Gore.NewGoreDirect_IEntitySource_Vector2_Vector2_int_float += (orig, entitySource, position, velocity, type, scale) =>
			{
				Gore result = orig(entitySource, position, velocity, type, scale);

				DestroyGore(entitySource, result);

				return result;
			};
			On.Terraria.Gore.NewGorePerfect_IEntitySource_Vector2_Vector2_int_float += (orig, entitySource, position, velocity, type, scale) =>
			{
				Gore result = orig(entitySource, position, velocity, type, scale);

				DestroyGore(entitySource, result);

				return result;
			};
		}

		private static void DestroyGore(IEntitySource entitySource, int goreID)
		{
			if (entitySource is EntitySource_HitEffect deathSource && deathSource.Entity is NPC npc && npc.GetGlobalNPC<GoreDestroyerNPC>().destroyGore)
				Main.gore[goreID].active = false;

			if (entitySource is EntitySource_Death deathSource3 && deathSource3.Entity is NPC npc3 && npc3.GetGlobalNPC<GoreDestroyerNPC>().destroyGore)
				Main.gore[goreID].active = false;

			if (entitySource is EntitySource_OnHit deathSource2 && deathSource2.EntityStruck is NPC npc2 && npc2.GetGlobalNPC<GoreDestroyerNPC>().destroyGore)
				Main.gore[goreID].active = false;

			if (entitySource is EntitySource_Parent deathSource4 && deathSource4.Entity is NPC npc4 && npc4.GetGlobalNPC<GoreDestroyerNPC>().destroyGore)
				Main.gore[goreID].active = false;

		}

		private static void DestroyGore(IEntitySource entitySource, Gore gore)
		{
			if (entitySource is EntitySource_HitEffect deathSource && deathSource.Entity is NPC npc && npc.GetGlobalNPC<GoreDestroyerNPC>().destroyGore)
				gore.active = false;

			if (entitySource is EntitySource_OnHit deathSource2 && deathSource2.EntityStruck is NPC npc2 && npc2.GetGlobalNPC<GoreDestroyerNPC>().destroyGore)
				gore.active = false;

			if (entitySource is EntitySource_Death deathSource3 && deathSource3.Entity is NPC npc3 && npc3.GetGlobalNPC<GoreDestroyerNPC>().destroyGore)
				gore.active = false;

			if (entitySource is EntitySource_Parent deathSource4 && deathSource4.Entity is NPC npc4 && npc4.GetGlobalNPC<GoreDestroyerNPC>().destroyGore)
				gore.active = false;
		}
	}

	public class GoreDestroyerNPC : GlobalNPC
    {
		public override bool InstancePerEntity => true;

		public bool destroyGore = false;

        public override void ResetEffects(NPC npc)
        {
			destroyGore = false;
        }
    }
}
