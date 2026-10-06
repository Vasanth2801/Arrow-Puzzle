using System.Collections;
using UnityEngine;

    public sealed class SolverTestMode : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private float delayBetweenTaps = 0.48f;

        public void AutoSolveCurrentLevel()
        {
            StopAllCoroutines();
            StartCoroutine(SolveRoutine());
        }

        private IEnumerator SolveRoutine()
        {
            var board = gameManager.GetComponent<BoardView>();
            var result = gameManager.Solver.Solve(board.CurrentLevel);
            if (!result.solvable)
            {
                Debug.LogError($"Solver test failed: level {board.CurrentLevel.levelNumber} is not solvable.");
                yield break;
            }

            for (int i = 0; i < result.extractionOrder.Count; i++)
            {
                var view = board.FindView(result.extractionOrder[i]);
                if (view == null)
                {
                    Debug.LogError("Solver test failed: path view missing.");
                    yield break;
                }
                board.TryExtract(view);
                yield return new WaitForSecondsRealtime(delayBetweenTaps);
            }

            Debug.Log($"Solver test passed: level {board.CurrentLevel.levelNumber}, taps={result.extractionOrder.Count}.");
        }
    }