using System.IO;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Core.Systems.PersistentDataSystem
{
	internal abstract class PersistentDataStore : IOrderedLoadable
	{
		public float Priority => 1f;

		public void Load()
		{
			PersistentDataStoreSystem.PutDataStore(this);

			string dir = Path.Join(Main.SavePath, "StarlightRiverPersistent");

			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			string currentPath = Path.Join(Main.SavePath, "StarlightRiverPersistent", GetType().Name);
			LoadFromFile(currentPath);
		}

		public void Unload() { }

		/// <summary>
		/// Forces a save of this data.
		/// </summary>
		public void ForceSave()
		{
			if (Main.netMode == NetmodeID.Server)
				return;

			string currentPath = Path.Join(Main.SavePath, "StarlightRiverPersistent", GetType().Name);

			ExportToFile(currentPath);
		}

		/// <summary>
		/// Write the persistent data to it's file. Called as a part of ForceSave
		/// </summary>
		/// <param name="path">The path of the file to save the data to</param>
		public void ExportToFile(string path)
		{
			var tag = new TagCompound();
			SaveGlobal(tag);

			if (!File.Exists(path))
			{
				FileStream stream = File.Create(path);
				stream.Close();
			}

			TagIO.ToFile(tag, path);
		}

		/// <summary>
		/// Attempts to load the persistent data from it's file location
		/// </summary>
		/// <param name="path">The file location for the data</param>
		public void LoadFromFile(string path)
		{
			if (!File.Exists(path))
				return;

			TagCompound tag = TagIO.FromFile(path);

			if (tag != null)
				LoadGlobal(tag);
			else
				StarlightRiver.Instance.Logger.Error($"Failed to load persistent data: {GetType().Name}");
		}

		public abstract void SaveGlobal(TagCompound tag);

		public abstract void LoadGlobal(TagCompound tag);
	}
}