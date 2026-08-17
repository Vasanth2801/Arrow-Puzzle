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

        int arrowCount = Mathf.Clamp(chainLength + levelNumber / 10, 2, rows * columns);

        level.arrows = new ArrowData[arrowCount];

        HashSet<Vector2Int> usedPositions = new HashSet<Vector2Int>();

        for (int i = 0; i < arrowCount; i++)
        {
            Vector2Int position;

            do
            {
                position = new Vector2Int(Random.Range(0, rows), Random.Range(0, columns));
            } while (usedPositions.Contains(position));


            usedPositions.Add(position);

            ArrowData arrow = new ArrowData();

            arrow.gridPosition = position;

            arrow.direction = GetRandomDirection();

            level.arrows[i] = arrow;
        }

        return level;
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
            
            if(!IsInsideGrid(next, level))
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

    private bool IsInsideGrid(Vector2Int position, Leveldata level)
    {
        return position.x >= 0 && position.x < level.rows && position.y >= 0 && position.y < level.columns;
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