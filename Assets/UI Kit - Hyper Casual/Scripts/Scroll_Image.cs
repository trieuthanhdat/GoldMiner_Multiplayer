using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class Scroll_Image : MonoBehaviour {
    [SerializeField] private RawImage Pattern;
    [SerializeField] private float _x, _y;
 
    void Update()
    {
        Pattern.uvRect = new Rect(Pattern.uvRect.position + new Vector2(_x,_y) * Time.deltaTime, Pattern.uvRect.size);
    }
}
 
