using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace StarlightRiver.Core
{
	public abstract class PlayerTicker : IOrderedLoadable
	{
		public float Priority => 1;

		public abstract bool Active(Player player);
		public abstract int TickFrequency { get; }

		public void Load()
		{
			StarlightPlayer.spawners.Add(this);
		}

		public void Unload()
		{
			StarlightPlayer.spawners.Remove(this);
		}

		public virtual void Tick(Player player) { }
	}
}
