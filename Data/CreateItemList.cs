using UnityEngine;
using UnityEditor;

namespace Sangki
{
    public class CreateItemList
    {
        [MenuItem("Assets/Create/Item List")]
        public static ItemList Create(string listName)
        { 
            ItemList asset = ScriptableObject.CreateInstance<ItemList>();

            AssetDatabase.CreateAsset(asset, "Assets/Resources/Data/ItemList/" + listName + ".asset");
            AssetDatabase.SaveAssets();
            return asset;
        }
    }
}