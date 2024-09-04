using UnityEngine;

namespace _DPS
{
    public class NodeObjectComponent : MonoBehaviour
    {
        public LevelBuilder.NodeObjectType NodeType;
        public NodeAttach NodeObjectRef;
        public int MaxOfThisKind = 1000;
        public float NodeProbability = 25;
        public float Scale = 1f;
        public float OffsetY;
        public float OffsetX;

        public void PopulateNodeObjectComponent(
            LevelBuilder.NodeObjectType nodeType,
            NodeAttach nodeObjectRef,
            int maxOfThisKind,
            float nodeProbability,
            float scale,
            float offsetY,
            float offsetX
        )
        {
            NodeType = nodeType;
            NodeObjectRef = nodeObjectRef;
            MaxOfThisKind = maxOfThisKind;
            NodeProbability = nodeProbability;
            Scale = scale;
            OffsetY = offsetY;
            OffsetX = offsetX;
        }
    }
}
