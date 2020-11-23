using System.Collections.Generic;
using System.Linq;
using Terraria;

using StarlightRiver.Core;

namespace StarlightRiver.RiftCrafting
{
    public class RiftRecipe
    {
        public List<RiftIngredient> Ingredients;
        public List<int> SpawnPool;
        public int Tier;
        public int Result;

        public RiftRecipe(List<RiftIngredient> ingredients, List<int> spawnpool, int tier, int resultType)
        {
            Ingredients = ingredients;
            SpawnPool = spawnpool;
            Tier = tier;
            Result = resultType;
        }

        public bool CheckRecipe(List<Item> items)
        {
            foreach (RiftIngredient ingredient in Ingredients)
            {
                if (items.Count(item => item.type == ingredient.type) < ingredient.count) return false;
            }
            return true;
        }
    }

    public struct RiftIngredient
    {
        public int type;
        public int count;

        public RiftIngredient(int typ, int cnt)
        {
            type = typ;
            count = cnt;
        }
    }
}