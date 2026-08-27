using UnityEngine;

[CreateAssetMenu(fileName = "newLevel", menuName = "Leveldata")]
public class Leveldata : ScriptableObject 
{
    public int rows;
    public int columns;
    public int speed;
    public int difficulty;

    public ArrowData[] arrows;
}