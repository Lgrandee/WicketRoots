using UnityEngine;

namespace WicketRoots.Dialogue
{
    [CreateAssetMenu(fileName = "DialogueAsset", menuName = "WicketRoots/Dialogue Asset", order = 0)]
    public class DialogueAsset : ScriptableObject
    {
        [Header("Text Content")]
        [TextArea(2, 5)]
        public string scriptText;

        [TextArea(2, 5)]
        public string bubbleText;

        [Header("Optional Visuals")]
        public Sprite bubbleIcon;
    }
}