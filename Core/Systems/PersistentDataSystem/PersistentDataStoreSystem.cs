using System;
using System.Collections.Generic;

namespace StarlightRiver.Core.Systems.PersistentDataSystem
{
	internal class PersistentDataStoreSystem
	{
		private readonly static Dictionary<Type, PersistentDataStore> stores = new();

		public static T GetDataStore<T>() where T : PersistentDataStore
		{
			return stores[typeof(T)] as T;
		}

		public static void PutDataStore(PersistentDataStore store)
		{
			Type key = store.GetType();

			if (stores.ContainsKey(key))
				stores[key] = store;
			else
				stores.Add(key, store);
		}
	}
}