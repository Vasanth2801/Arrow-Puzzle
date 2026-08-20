#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class ArrowLevelGeneratorWindow : EditorWindow
{
    private int numberOfLevels = 10;
    private int rows = 9;
    private int columns = 4;
    private int baseChainLength = 3;
    private int generationSpeed = 12345;
    private string saveFolder = "Assets/Resources/Levels/Generated";
    private HashSet<string> generatedFingerPrints = new HashSet<string>();

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

        baseChainLength = EditorGUILayout.IntField("Chain Length", baseChainLength);

        generationSpeed = EditorGUILayout.IntField("Generation Speed", generationSpeed);

        saveFolder = EditorGUILayout.TextField("Save Folder", saveFolder);

        GUILayout.Space(10);

        if(GUILayout.Button("Generate Levels"))
        {
            GenerateLevels();
        }

        if(GUILayout.Button("Validate Generated Levels"))
        {
            ValidateGeneratedLevels();
        }
    }

    private void GenerateLevels()
    {
        if(numberOfLevels <= 0)
        {
            Debug.LogError("Number of levels must be greater then 0");
            return;
        }

        if(rows <= 0 || columns <= 0)
        {
            Debug.LogError("Rows and Colums must be greater then zero");
            return;
        }

        if(rows * columns < 2)
        {
            Debug.LogError("The grid is too small");
            return;
        }

        generatedFingerPrints.Clear();


        CreateFolderIfNeeded();

        int generated = 0;
        int attempts = 0;

        int maxAttempts = Mathf.Max(numberOfLevels * 200, 1000);

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

            string fingerPrint = GetLevelFingerPrint(level);

            if(generatedFingerPrints.Contains(fingerPrint))
            {
                DestroyImmediate(level);

                continue;
            }

            generatedFingerPrints.Add(fingerPrint);

            if(!AssetDatabase.IsValidFolder(saveFolder))
            {
                Debug.LogError("Folder does not exist: " + saveFolder);

                DestroyImmediate(level);
                return;
            }

            string assetPath = saveFolder + "/Level_" + (generated + 1).ToString("000") + ".asset";

            AssetDatabase.CreateAsset(level, assetPath);

            generated++;

            if(generated % 10 == 0 || generated == numberOfLevels)
            {
                Debug.Log("Generated " + generated + "/ " + numberOfLevels);
            }

            Debug.Log("Generated valid level: " + generated);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Finished.Generated" + generated + " valid levels");
        }
    }

    private Leveldata CreateLevel(int levelNumber)
    {
        Leveldata level = ScriptableObject.CreateInstance<Leveldata>();

        level.speed = generationSpeed + levelNumber;

        Random.InitState(level.speed);

        level.rows = rows;
        level.columns = columns;

        level.difficulty = CalculateDifficulty(levelNumber);

        int arrowCount = GetArrowCount(levelNumber);

        arrowCount = Mathf.Clamp(arrowCount, 2, rows * columns);

        level.arrows = new ArrowData[arrowCount];

        HashSet<Vector2Int> usedPositions = new HashSet<Vector2Int>();

        int currentIndex = 0;

        int chainCount = GetChainCount(levelNumber);

        for (int chain = 0; chain < chainCount; chain++)
        {
            if (currentIndex >= arrowCount)
            {
                break;
            }

            int remainingArrows = arrowCount - currentIndex;

            int remainingChains = chainCount - chain;

            int minimumForRemaining = remainingChains - 1;

            int maxChainLength = remainingArrows - minimumForRemaining;

            int requestedChainLength = GetChainLength(levelNumber);

            int actualChainLength = Mathf.Clamp(requestedChainLength, 1, maxChainLength);

            bool created = TryCreateChain(level.arrows, currentIndex, actualChainLength, usedPositions);

            if (!created)
            {
                break;
            }

            currentIndex += actualChainLength;
        }

        int dependencyCount = GetDependencyCount(levelNumber);

        for (int i = 0; i < dependencyCount; i++)
        {
            if (currentIndex + 1 >= arrowCount)
            {
                break;
            }

            bool created = CreateBlockedDependency(level.arrows, currentIndex, usedPositions);

            if (created)
            {
                currentIndex += 2;
            }
        }

        for (int i = currentIndex; i < arrowCount; i++)
        {
            Vector2Int position;

            if (!TryGetRandomFreePosition(usedPositions, out position))
            {
                DestroyImmediate(level);

                return null;
            }

            ArrowData arrow = new ArrowData();

            arrow.gridPosition = position;

            arrow.direction = GetRandomDirection();

            level.arrows[i] = arrow;

            usedPositions.Add(position);
        }

        return level;
    }

    private int CalculateDifficulty(int levelNumber)
    {
        int level = levelNumber + 1;

        if(level <= 10)
        {
            return 1;
        }

        if(level <= 50)
        {
            return 2;
        }

        if(level <= 100)
        {
            return 3;
        }

        if(level <= 250)
        {
            return 4;
        }

        if(level <= 500)
        {
            return 5;
        }

        return 6;
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

        int safetyCounter = 0;

        int maxSteps = level.rows * level.columns + 10;

        while(true)
        {
            safetyCounter++;

            if(safetyCounter > maxSteps)
            {
                return false;
            }

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

    private string GetLevelFingerPrint(Leveldata level)
    {
        List<string> arrowData = new List<string>();

        foreach(ArrowData arrow in level.arrows)
        {
            string data = arrow.gridPosition.x + ", " + arrow.gridPosition.y + ", " + (int) arrow.direction;
            arrowData.Add(data);
        }

        arrowData.Sort();

        return string.Join(", ", arrowData);    
    }

    private void ValidateGeneratedLevels()
    {
        CreateFolderIfNeeded();

        string[] guids = AssetDatabase.FindAssets("t:Leveldata", new[] { saveFolder });

        int valid = 0;
        int invalid = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            Leveldata level = AssetDatabase.LoadAssetAtPath<Leveldata>(path);

            if (level == null)
            {
                invalid++;

                continue;
            }

            if (level.arrows == null || level.arrows.Length == 0)
            {
                Debug.LogError("Invalid Level: " + path);

                invalid++;

                continue;
            }

            if (IsLevelSolvable(level))
            {
                valid++;
            }
            else
            {
                Debug.LogError("UnSolvable Level: " + path);

                invalid++;
            }
        }
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

    private int GetChainCount(int levelNumber)
    {
        int level = levelNumber + 1;

        if(level <= 50)
        {
            return 1;
        }

        if(level <= 250)
        {
            return 2;
        }

        return 3;
    }

    private int GetChainLength(int levelNumber)
    {
        int level = levelNumber + 1;

        if (level <= 10)
        {
            return Mathf.Max(2, baseChainLength - 1);
       }

        if(level <= 50)
        {
            return Random.Range(2, 4);
        }

        if(level <= 100)
        {
            return Random.Range(3, 5);
        }

        if(level <= 250)
        {
            return Random.Range(3, 6);
        }

        if(level <= 500)
        {
            return Random.Range(4, 7);
        }

        return Random.Range(5, 8);
    }

    private int GetDependencyCount(int levelNumber)
    {
        int level = levelNumber + 1;

        if(level <= 50)
        {
            return 0;
        }

        if(level <= 100)
        {
            return 1;
        }

        if(level <= 250)
        {
            return 2;
        }

        return 3;
    }

    private bool TryCreateChain(ArrowData[] arrows, int startIndex, int chainLength, HashSet<Vector2Int> usedPositions)
    {
        List<ArrowDirection> directions = new List<ArrowDirection>();

        directions.Add(ArrowDirection.Up);

        directions.Add(ArrowDirection.Right);

        directions.Add(ArrowDirection.Down);

        directions.Add(ArrowDirection.Left);

        for (int i = directions.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);

            ArrowDirection temp = directions[i];

            directions[i] = directions[randomIndex];

            directions[randomIndex] = temp;
        }

        foreach (ArrowDirection direction in directions)
        {
            Vector2Int start = GetValidChainStart(direction, chainLength);

            if (start == new Vector2Int(-1, -1))
            {
                continue;
            }

            bool collision = false;

            for (int i = 0; i < chainLength; i++)
            {
                Vector2Int position = GetChainPosition(start, direction, i);

                if (usedPositions.Contains(position))
                {
                    collision = true;

                    break;
                }
            }

            if (collision)
            {
                continue;
            }

            for (int i = 0; i < chainLength; i++)
            {
                Vector2Int position = GetChainPosition(start,direction,i);

                ArrowData arrow = new ArrowData();

                arrow.gridPosition = position;

                arrow.direction = direction;

                arrows[startIndex + i] = arrow;

                usedPositions.Add(position);
            }

            return true;
        }

        return false;
    }

    private bool CreateBlockedDependency(ArrowData[] arrows, int firstIndex, HashSet<Vector2Int> usedPositions)
    {
        for(int attempt = 0; attempt < 50; attempt++)
        {
            ArrowDirection firstDirection = GetRandomDirection();

            Vector2Int firstPosition;

            if(!TryGetRandomFreePosition(usedPositions, out firstPosition))
            {
                return false;
            }

            Vector2Int secondPosition = ArrowUtility.GetNextPosition(firstPosition, firstDirection);

            if(!IsInsideGrid(secondPosition))
            {
                continue;
            }

            if(usedPositions.Contains(secondPosition))
            {
                continue;
            }

            ArrowData firstArrow = new ArrowData();

            firstArrow.gridPosition = firstPosition;

            firstArrow.direction = firstDirection;

            ArrowData secondArrow = new ArrowData();

            secondArrow.gridPosition = secondPosition;

            secondArrow.direction = GetRandomDirection();

            arrows[firstIndex] = firstArrow;

            arrows[firstIndex + 1] = secondArrow;

            usedPositions.Add(firstPosition);

            usedPositions.Add(secondPosition);

            return true;
;       }

        return false;
    }

    private Vector2Int GetValidChainStart(ArrowDirection direction, int length)
    {
        List<Vector2Int> possibleStarts = new List<Vector2Int>();

        for(int row = 0; row < rows; row++)
        {
            for(int col = 0; col < columns; col++)
            {
                Vector2Int start = new Vector2Int(row, col);

                bool isValid = true;

                for(int i =0; i < length; i++)
                {
                    Vector2Int position = GetChainPosition(start, direction, i);

                    if(!IsInsideGrid(position))
                    {
                        isValid = false;

                        break;
                    }
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
                return new Vector2Int(start.x + index, start.y);

            case ArrowDirection.Down:
                return new Vector2Int(start.x - index, start.y);

            case ArrowDirection.Right:
                return new Vector2Int(start.x, start.y + index);

            case ArrowDirection.Left:
                return new Vector2Int(start.x, start.y - index);
        }

        return start;
    }

    private bool TryGetRandomFreePosition(HashSet<Vector2Int> usedPositions, out Vector2Int position)
    {
        for(int attempt = 0; attempt < 100; attempt++)
        {
            position = new Vector2Int(Random.Range(0, rows), Random.Range(0, columns));

            if(!usedPositions.Contains(position))
            {
                return true;
            }
        }

        position = new Vector2Int(-1, -1);

        return false;
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