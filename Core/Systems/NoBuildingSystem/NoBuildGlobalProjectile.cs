using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace StarlightRiver.Core.Systems.NoBuildingSystem
{
	public class NoBuildGlobalProjectile : GlobalProjectile 
	{
		public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
		{
			return entity.aiStyle == ProjAIStyleID.GraveMarker;
		}

		public override void PostAI(Projectile Projectile)
		{
			foreach (Rectangle region in NoBuildSystem.protectedRegions)
			{
				if (region.Contains(new Point((int)Projectile.Center.X / 16, (int)Projectile.Center.Y / 16)))
					Projectile.active = false;
			}

			foreach (Ref<Rectangle> region in NoBuildSystem.RuntimeProtectedRegions)
			{
				if (region.Value.Contains(new Point((int)Projectile.Center.X / 16, (int)Projectile.Center.Y / 16)))
					Projectile.active = false;
			}
		}
	}
}
