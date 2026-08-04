using UnityEngine;

[CreateAssetMenu(fileName = "newLevel", menuName = "Leveldata")]
public class Leveldata : ScriptableObject 
{
    public int rows;
    public int columns;

    public ArrowData[] arrows;
}