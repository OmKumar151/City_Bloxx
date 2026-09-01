using UnityEngine;

public class NavigationNode : MonoBehaviour
{
    public enum NodeType
    {
        BoardCell,
        CancelPlacement
    }

    [Header("Node")]
    [SerializeField] private NodeType nodeType = NodeType.BoardCell;

    [Header("Navigation")]
    [SerializeField] private NavigationNode up;
    [SerializeField] private NavigationNode down;
    [SerializeField] private NavigationNode left;
    [SerializeField] private NavigationNode right;

    public NodeType Type => nodeType;

    public NavigationNode Up => up;
    public NavigationNode Down => down;
    public NavigationNode Left => left;
    public NavigationNode Right => right;


    public NavigationNode GetNeighbour(string direction)
    {
        switch (direction)
        {
            case "Up":
                return up;

            case "Down":
                return down;

            case "Left":
                return left;

            case "Right":
                return right;

            default:
                return null;
        }
    }


    public void SetNavigation(
        NavigationNode upNode,
        NavigationNode downNode,
        NavigationNode leftNode,
        NavigationNode rightNode)
    {
        up = upNode;
        down = downNode;
        left = leftNode;
        right = rightNode;
    }
}