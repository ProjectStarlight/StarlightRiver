using StarlightRiver.Core.Systems.PersistentDataSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Content.PersistentData
{
	internal class TutorialDataStore : PersistentDataStore
	{
		public bool ignoreMasterWarning;

		public override void LoadGlobal(TagCompound tag)
		{
			ignoreMasterWarning = tag.GetBool("MasterWarning");
		}

		public override void SaveGlobal(TagCompound tag)
		{
			tag.Add("MasterWarning", ignoreMasterWarning);
		}
	}
}