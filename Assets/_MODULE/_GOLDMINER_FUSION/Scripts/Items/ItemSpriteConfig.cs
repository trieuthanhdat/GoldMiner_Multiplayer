using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ItemInfo;

[CreateAssetMenu(fileName = "ItemConfig", menuName = "Gameplay/ItemSpriteConfig")]
public class ItemSpriteConfig : ScriptableObject
{
    public List<SpriteItemWrap> listItemSprite = new List<SpriteItemWrap>();
}
[Serializable]
public class SpriteItemWrap
{
    public ItemDetailType itemType;
    public Sprite itemSprite;
}