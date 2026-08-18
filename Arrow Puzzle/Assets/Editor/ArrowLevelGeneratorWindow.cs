#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;


public class ArrowLevelGeneratorWindow : EditorWindow
{
    private int numberOfLevels = 10;
    private int rows = 9;
    private int columns = 4;
    private int chainLength = 3;

    private string saveFolder = "Assets/Resources/Levels/Generated";

    [MenuItem("ArrowGame/LevelGenerator")]
    public static void ShowWindow()
    {
        GetWindow<ArrowLevelGeneratorWindow>("Arrow level Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Arrow level Generator", EditorStyles.boldLabel);

        numberOfLevels = EditorGUILayout.IntField("Number of Levels", numberOfLevels);

        rows = EditorGUILayout.IntField("Rows", rows);

        columns = EditorGUILayout.IntField("Columns", columns);

        chainLength = EditorGUILayout.IntField("Chain Length", chainLength);

        saveFolder = EditorGUILayout.TextField("Save Folder", saveFolder);

        GUILayout.Space(10);

        if(GUILayout.Button("Generate Levels"))
        {
            GenerateLevels();
        }
    }

    private void GenerateLevels()
    {
        CreateFolderIfNeeded();

        int generated = 0;
        int attempts = 0;

        int maxAttempts = numberOfLevels * 100;

        while (generated < numberOfLevels && attempts < maxAttempts) 
        {
            attempts++;

            Leveldata level = CreateLevel(generated);

            if(level == null)
            {
                continue;
            }
            
            if(!IsLevelSolvable(level))
            {
                DestroyImmediate(level);
                continue;
            }

            if(!AssetDatabase.IsValidFolder(saveFolder))
            {
                Debug.LogError("Folder does not exist: " + saveFolder);

                DestroyImmediate(level);
                return;
            }

            string assetPath = saveFolder + "/Level_" + (generated + 1).ToString("000") + ".asset";

            AssetDatabase.CreateAsset(level, assetPath);

            generated++;

            Debug.Log("Generated valid level: " + generated);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Finished.Generated" + generated + " valid levels");
        }
    }

    private Leveldata CreateLevel(int levelNumber)
    {
        Leveldata level = ScriptableObject.CreateInstance<Leveldata>();

        level.rows = rows;
        level.columns = columns;

        int arrowCount = GetArrowCount(levelNumber);

        level.arrows = new ArrowData[arrowCount];

        HashSet<Vector2Int> usedPositions = new HashSet<Vector2Int>();

        ArrowDirection chainDirection = GetRandomDirection();

        int actualChainLength = Mathf.Min(chainLength, arrowCount);

        Vector2Int chainStart = GetValidChainStart(chainDirection, actualChainLength);

        if (chainStart == new Vector2Int(-1, -1))
        {
            DestroyImmediate(level);
            return null;
        }

        for (int i = 0; i < actualChainLength; i++)
        {
            Vector2Int position = GetChainPosition(chainStart, chainDirection, i);

            ArrowData arrow = new ArrowData();

            arrow.gridPosition = position;

            arrow.direction = chainDirection;

            level.arrows[i] = arrow;

            usedPositions.Add(position);
        }

        for(int i = actualChainLength; i < arrowCount; i++)
        {
            Vector2Int position = GetRandomFreePositions(usedPositions);

            usedPositions.Add(position);

            ArrowData arrow = new ArrowData();

            arrow.gridPosition = position;

           arrow.direction = GetRandomDirection();

            level.arrows[i] = arrow;
        }

        return level;
    }

    private Vector2Int GetValidChainStart(ArrowDirection direction, int length)
    {
        List<Vector2Int> possibleStarts = new List<Vector2Int>();

        for(int row = 0; row < rows; row++)
        {
            for(int col = 0;  col < columns; col++)
            {
                Vector2Int start = new Vector2Int(row, col);

                bool valid = true;

                for (int i = 0; i < length; i++)
                {
                    Vector2Int position = GetChainPosition(start, direction, i);

                    if (!IsInsideGrid(position))
                    {
                        valid = false;
                        break;
                    }
                }

                if(valid)
                {
                    possibleStarts.Add(start);
                }
            }
        }

        if(possibleStarts.Count == 0)
        {
            return new Vector2Int(-1, -1);
        }

        return possibleStarts[Random.Range(0, possibleStarts.Count)];
    }

    private Vector2Int GetChainPosition(Vector2Int start, ArrowDirection direction, int index)
    {
        switch(direction)
        {
            case ArrowDirection.Up:
                return new Vector2Int(start.x + index , start.y);
            case ArrowDirection.Down:
                return new Vector2Int(start.x - index, start.y);
            case ArrowDirection.Right:
                return new Vector2Int(start.x, start.y + index);
            case ArrowDirection.Left:
                return new Vector2Int(start.x, start.y  - index);
        }

        return start;
    }

    private Vector2Int GetRandomFreePositions(HashSet<Vector2Int> usedPositions)
    {
        Vector2Int position;

        int attempts = 0;

        do
        {
            position = new Vector2Int(Random.Range(0, rows), Random.Range(0, columns));

            attempts++;
        } while (usedPositions.Contains(position) && attempts < 100);

        return position;
    }


    private bool IsLevelSolvable(Leveldata level)
    {
        List<ArrowData> remaining = new List<ArrowData>(level.arrows);

        while(remaining.Count > 0)
        {
            bool removedArrow = false;

            for(int i =0; i < remaining.Count; i++)
            {
                if (CanArrowExit(remaining[i], remaining, level))
                {
                    remaining.RemoveAt(i);

                    removedArrow = true;

                    break;
                }
            }

            if(!removedArrow)
            {
                return false;
            }
        }

        return true;
    }

    private bool CanArrowExit(ArrowData arrow, List<ArrowData> arrows, Leveldata level)
    {
        Vector2Int current = arrow.gridPosition;

        while(true)
        {
            Vector2Int next = ArrowUtility.GetNextPosition(current, arrow.direction);
            
            if(!IsInsideGrid(next))
            {
                return true;
            }

            foreach(ArrowData other in arrows)
            {
                if(other.gridPosition == next)
                {
                    return false;
                }
            }

            current = next;
        }
    }

    private bool IsInsideGrid(Vector2Int position)
    {
        return position.x >= 0 && position.x < rows && position.y >= 0 && position.y < columns;
    }

    private ArrowDirection GetRandomDirection()
    {
        int direction = Random.Range(0, 4);

        switch(direction)
        {
            case 0:
                return ArrowDirection.Up;
            case 1:
                return ArrowDirection.Right;
            case 2:
                return ArrowDirection.Down;
            case 3:
                return ArrowDirection.Left;
        }

        return ArrowDirection.Up;
    }

    private int GetArrowCount(int levelNumber)
    {

        int level = levelNumber + 1;

        if(levelNumber <= 10)
        {
            return Random.Range(2, 5);
        }
        else if(levelNumber <= 50)
        {
            return Random.Range(4, 7);
        }
        else if(levelNumber <= 100)
        {
            return Random.Range(5, 11);
        }
        else if(levelNumber <= 250)
        {
            return Random.Range(6, 11);
        }
        else if(levelNumber <= 500)
        {
            return Random.Range(8, 14);
        }

        return Random.Range(10, 17);
    }

    private void CreateFolderIfNeeded()
    {
        string resourcesPath = "Assets/Resources";
        string levelsPath = "Assets/Resources/Levels";
        string generatedPath = "Assets/Resources/Levels/Generated";

        if (!AssetDatabase.IsValidFolder(resourcesPath))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }

        if (!AssetDatabase.IsValidFolder(levelsPath))
        {
            AssetDatabase.CreateFolder(resourcesPath, "Levels");
        }

        if (!AssetDatabase.IsValidFolder(generatedPath))
        {
            AssetDatabase.CreateFolder(levelsPath, "Generated");
        }

        AssetDatabase.Refresh();
    }
}
#endif