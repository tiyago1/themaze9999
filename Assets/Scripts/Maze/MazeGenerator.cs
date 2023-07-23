/* ----------------------------
 * 2D Maze Generator for Unity.
 * Uses Deapth-First searching 
 * and Recursive Backtracking.
 * ----------------------------
 * Generates a 2x2 centre room in 
 * the middle of the Maze.
 * ----------------------------
 * Author: c00pala
 * ~13/05/2018~
 * ---------------------------- */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Maze;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

public class MazeGenerator : MonoBehaviour
{
    #region Variables:

    // ------------------------------------------------------
    // User defined variables - set in editor:
    // ------------------------------------------------------
    [Header("Maze generation values:")]
    [Tooltip("How many cells tall is the maze. MUST be an even number. " + "If number is odd, it will be reduced by 1.\n\n" + "Minimum value of 4.")]
    public int mazeRows;

    [Tooltip("How many cells wide is the maze. Must be an even number. " + "If number is odd, it will be reduced by 1.\n\n" + "Minimum value of 4.")]
    public int mazeColumns;

    [Header("Maze object variables:")] [Tooltip("Cell prefab object.")] [SerializeField]
    private GameObject cellPrefab;

    [Tooltip("If you want to disable the main sprite so the cell has no background, set to TRUE. This will create a maze with only walls.")]
    public bool disableCellSprite;

    // ------------------------------------------------------
    // System defined variables - You don't need to touch these:
    // ------------------------------------------------------

    // Variable to store size of centre room. Hard coded to be 2.
    private int centreSize = 2;

    // Dictionary to hold and locate all cells in maze.
    public Dictionary<Vector2, Cell> allCells = new Dictionary<Vector2, Cell>();

    // List to hold unvisited cells.
    private List<Cell> unvisited = new List<Cell>();

    // List to store 'stack' cells, cells being checked during generation.
    private List<Cell> stack = new List<Cell>();

    // Array will hold 4 centre room cells, from 0 -> 3 these are:
    // Top left (0), top right (1), bottom left (2), bottom right (3).
    private Cell[] centreCells = new Cell[4];

    // Cell variables to hold current and checking Cells.
    private Cell currentCell;
    private Cell checkCell;

    public Cell enterCell;

    // Array of all possible neighbour positions.
    private Vector2[] neighbourPositions = new Vector2[] {new Vector2(-1, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(0, -1)};

    // Size of the cells, used to determine how far apart to place cells during generation.
    private float cellSize;

    private GameObject mazeParent;

    [Inject] private PlayerController _playerController;
    [Inject] private SignalBus _signalBus;

    public Cell StartCell;
    public Cell EndCell;

    public int AttemptCount = 0;

    #endregion

    /* This Start run is an example, you can delete this when 
     * you want to start calling the maze generator manually. 
     * To generate a maze is really easy, just call the GenerateMaze() function
     * pass a rows value and columns value as parameters and the generator will
     * do the rest for you. Enjoy!
     */

    public void Initialize()
    {
        Debug.LogError("MazeGenerator Initialize()");
        AttemptCount = 0;
        GenerateMaze(mazeRows, mazeColumns);
        // _signalBus.Subscribe<AStartCompleted>(OnAStartCompleted);
    }

    private void OnAStartCompleted(int stepCount)
    {
        Debug.LogError(AttemptCount + " OnAStartCompleted " + stepCount);
        if (AttemptCount >= 5)
        {
            return;
        }

        if (stepCount == -1)
        {
            Debug.LogError("yapma bunu");
        }

        float pow = Mathf.Pow(mazeRows, 1.15f);
        int minStepCount = Mathf.RoundToInt(pow);
        Debug.LogError("MIN STEP COUNT " + minStepCount);
        if (stepCount < minStepCount)
        {
            AttemptCount++;
            GenerateMaze(mazeRows, mazeColumns);
        }
        else
        {
            _signalBus.Fire<MazeGenerateFinished>();
        }
        // StartCoroutine(Process(obj.StepCount));
    }

    // public IEnumerator Test()
    // {
    //     Debug.LogError("0");
    //     GenerateMaze(mazeRows, mazeColumns);
    //     Debug.LogError("1");
    //
    //     for (int i = 0; i < 5; i++)
    //     {
    //         Debug.LogError("2");
    //         yield return new WaitForSecondsRealtime(.5f);
    //         Debug.LogError("3");
    //         int stepCount = AStar();
    //         yield return new WaitForSecondsRealtime(.5f);
    //
    //         Debug.LogError("4");
    //
    //         Debug.LogError("Attempt " + i + " stepCount " + stepCount);
    //         if (stepCount == -1)
    //         {
    //             Debug.LogError("yapma bunu");
    //         }
    //
    //         float pow = Mathf.Pow(mazeRows, 1.15f);
    //         int minStepCount = Mathf.RoundToInt(pow);
    //         Debug.LogError("MIN STEP COUNT " + minStepCount);
    //         if (stepCount < minStepCount)
    //         {
    //             GenerateMaze(mazeRows, mazeColumns);
    //         }
    //         else
    //         {
    //             break;
    //         }
    //     }
    //
    //     _signalBus.Fire<MazeGenerateFinished>();
    // }

    private void GenerateMaze(int rows, int columns)
    {
        Debug.LogError("AttemptCount GenerateMaze " + AttemptCount);
        if (mazeParent != null) DeleteMaze();

        mazeRows = rows;
        mazeColumns = columns;
        CreateLayout();
    }

    // Creates the grid of cells.
    public void CreateLayout()
    {
        InitValues();

        // Set starting point, set spawn point to start.
        Vector2 startPos = new Vector2(-(cellSize * (mazeColumns / 2)) + (cellSize / 2), -(cellSize * (mazeRows / 2)) + (cellSize / 2));
        Vector2 spawnPos = startPos;

        bool isFill = false;
        for (int x = 1; x <= mazeColumns; x++)
        {
            isFill = !isFill;

            for (int y = 1; y <= mazeRows; y++)
            {
                var cell = GenerateCell(spawnPos, new Vector2(x, y));
                cell.cScript.GrayArea.gameObject.SetActive(isFill);
                // Increase spawnPos y.
                spawnPos.y += cellSize;
                isFill = !isFill;
            }

            // Reset spawnPos y and increase spawnPos x.
            spawnPos.y = startPos.y;
            spawnPos.x += cellSize;
        }

        CreateCentre();
        RunAlgorithm();
        MakeEnter();
        MakeExit();
        AStar();
    }

    // This is where the fun stuff happens.
    public void RunAlgorithm()
    {
        // Get start cell, make it visited (i.e. remove from unvisited list).
        unvisited.Remove(currentCell);

        Debug.Log("RunAlgorithm 0");
        // While we have unvisited cells.
        while (unvisited.Count > 0)
        {
            List<Cell> unvisitedNeighbours = GetUnvisitedNeighbours(currentCell);
            if (unvisitedNeighbours.Count > 0)
            {
                // Get a random unvisited neighbour.
                checkCell = unvisitedNeighbours[Random.Range(0, unvisitedNeighbours.Count)];
                // Add current cell to stack.
                stack.Add(currentCell);
                // Compare and remove walls.
                CompareWalls(currentCell, checkCell);
                // Make currentCell the neighbour cell.
                currentCell = checkCell;
                // Mark new current cell as visited.
                unvisited.Remove(currentCell);
            }
            else if (stack.Count > 0)
            {
                // Make current cell the most recently added Cell from the stack.
                currentCell = stack[stack.Count - 1];
                // Remove it from stack.
                stack.Remove(currentCell);
            }
        }

        Debug.Log("RunAlgorithm 1");
    }

    public void MakeExit()
    {
        // Create and populate list of all possible edge cells.
        List<Cell> edgeCells = new List<Cell>();

        foreach (KeyValuePair<Vector2, Cell> cell in allCells)
        {
            if (cell.Key.x == 0 || cell.Key.x == mazeColumns || cell.Key.y == 0 || cell.Key.y == mazeRows)
            {
                edgeCells.Add(cell.Value);
            }
        }

        EndCell = edgeCells[Random.Range(0, edgeCells.Count)];

        // Debug.Log("MakeExit 0");
        // Get edge cell randomly from list.
        if ((int) EndCell.gridPos.x == (int) StartCell.gridPos.x || (int) EndCell.gridPos.y == (int) StartCell.gridPos.y)
        {
            edgeCells.Remove(EndCell);
            EndCell = edgeCells[Random.Range(0, edgeCells.Count)];
        }

        // Debug.Log("MakeExit 1");

        // Remove appropriate wall for chosen edge cell.
        if (EndCell.gridPos.x == 0) SetGate(EndCell.cScript, 1, false);
        else if (EndCell.gridPos.x == mazeColumns) SetGate(EndCell.cScript, 2, false);
        else if (EndCell.gridPos.y == mazeRows) SetGate(EndCell.cScript, 3, false);
        else SetGate(EndCell.cScript, 4, false);

        EndCell.cScript.SetType(CellType.Empty);
        EndCell.cScript.SetLight(Color.green);
        // EndCell.cScript.GrayArea.GetComponent<SpriteMask>().enabled = false;

        // Debug.Log("Maze generation exit finished." + EndCell.gridPos);
    }

    public void MakeEnter()
    {
        // Create and populate list of all possible edge cells.
        List<Cell> edgeCells = new List<Cell>();

        foreach (KeyValuePair<Vector2, Cell> cell in allCells)
        {
            if (cell.Key.x == 0 || cell.Key.x == mazeColumns || cell.Key.y == 0 || cell.Key.y == mazeRows)
            {
                edgeCells.Add(cell.Value);
            }
        }

        // Get edge cell randomly from list.

        StartCell = edgeCells[Random.Range(0, edgeCells.Count)];
        StartCell.cScript.GrayArea.GetComponent<SpriteMask>().enabled = false;

        // Remove appropriate wall for chosen edge cell.

        if (StartCell.gridPos.x == 0) SetGate(StartCell.cScript, 1, true);
        else if (StartCell.gridPos.x == mazeColumns) SetGate(StartCell.cScript, 2, true);
        else if (StartCell.gridPos.y == mazeRows) SetGate(StartCell.cScript, 3, true);
        else SetGate(StartCell.cScript, 4, true);

        StartCell.cScript.SetLight(Color.red);
        // Debug.Log("Maze generation enter finished. " + StartCell.cScript);
    }

    public Cell getCellByGridPos(Vector2 gridPos)
    {
        Cell value = null;
        foreach (KeyValuePair<Vector2, Cell> cell in allCells)
        {
            if (gridPos.x == cell.Key.x && gridPos.y == cell.Key.y)
            {
                value = cell.Value;
                break;
            }
        }

        return value;
    }

    public List<Cell> GetNeighbours(Cell cell)
    {
        List<Cell> neighbours = new List<Cell>();
        Vector2 Nright = new Vector2(cell.gridPos.x + 1, cell.gridPos.y);
        Vector2 Nleft = new Vector2(cell.gridPos.x - 1, cell.gridPos.y);
        Vector2 Ndown = new Vector2(cell.gridPos.x, cell.gridPos.y - 1);
        Vector2 Nup = new Vector2(cell.gridPos.x, cell.gridPos.y + 1);


        if (allCells.ContainsKey(Nright))
        {
            neighbours.Add(getCellByGridPos(Nright));
        }

        if (allCells.ContainsKey(Nleft))
        {
            neighbours.Add(getCellByGridPos(Nleft));
        }

        if (allCells.ContainsKey(Ndown))
        {
            neighbours.Add(getCellByGridPos(Ndown));
        }

        if (allCells.ContainsKey(Nup))
        {
            neighbours.Add(getCellByGridPos(Nup));
        }

        return neighbours;
    }

    public bool isWalkable(Cell firstCell, Cell secondCell)
    {
        if ((int) firstCell.gridPos.x == (int) secondCell.gridPos.x)
        {
            if ((int) firstCell.gridPos.y - (int) secondCell.gridPos.y == 1 && !firstCell.cScript.wallD.IsActive)
            {
                return true;
            }

            if ((int) secondCell.gridPos.y - (int) firstCell.gridPos.y == 1 && !secondCell.cScript.wallD.IsActive)
            {
                return true;
            }
        }

        if ((int) firstCell.gridPos.y == (int) secondCell.gridPos.y)
        {
            if ((int) firstCell.gridPos.x - (int) secondCell.gridPos.x == 1 && !firstCell.cScript.wallL.IsActive)
            {
                return true;
            }

            if ((int) secondCell.gridPos.x - (int) firstCell.gridPos.x == 1 && !secondCell.cScript.wallL.IsActive)
            {
                return true;
            }
        }

        return false;
    }

    public void AStar()
    {
        Cell startingCell = StartCell;
        Cell targetCell = EndCell;

        List<Cell> openSet = new List<Cell>();
        HashSet<Cell> closedSet = new HashSet<Cell>();
        int returnValue = -1;
        openSet.Add(startingCell);
        Debug.Log("AStar 0");
        bool isReachedToEndCell = false;
        while (openSet.Count > 0)
        {
            Cell currentCell = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentCell.fCost || openSet[i].fCost == currentCell.fCost && openSet[i].hCost < currentCell.hCost)
                {
                    currentCell = openSet[i];
                }
            }

            openSet.Remove(currentCell);
            closedSet.Add(currentCell);

            if (currentCell.gridPos == EndCell.gridPos)
            {
                isReachedToEndCell = true;
                break;
            }

            foreach (Cell neighbour in GetNeighbours(currentCell))
            {
                if (!isWalkable(neighbour, currentCell) || closedSet.Contains(neighbour))
                {
                    continue;
                }

                int newMovementCostToNeighbour = currentCell.gCost + GetDistance(currentCell, neighbour);
                if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, EndCell);
                    neighbour.parent = currentCell;

                    if (!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                    }
                }
            }
        }

        if (isReachedToEndCell)
        {
            returnValue = RetracePath(StartCell, EndCell);
        }
        else
        {
            Debug.LogError("Not reached");
        }

        OnAStartCompleted(returnValue);
        // _signalBus.Fire(new AStartCompleted() {StepCount = returnValue});
    }

    int RetracePath(Cell startCell, Cell endCell)
    {
        List<Vector2> path = new List<Vector2>();
        Cell currentCell = endCell;

        Debug.Log("RetracePath 0");
        while (currentCell != startCell)
        {
            path.Add(currentCell.gridPos);
            currentCell = currentCell.parent;
        }

        Debug.Log("RetracePath 1");
        path.Reverse();
        Debug.Log("RetracePath 2");

        foreach (Vector2 oppenheimer in path)
        {
            Cell barbie = getCellByGridPos(oppenheimer);
            barbie.cellObject.GetComponent<SpriteRenderer>().color = Color.red;
            barbie.cellObject.GetComponent<SpriteRenderer>().enabled = true;
        }

        return path.Count;
    }

    int GetDistance(Cell cellA, Cell cellB)
    {
        int dstX = (int) Mathf.Abs(cellA.gridPos.x - cellB.gridPos.x);
        int dstY = (int) Mathf.Abs(cellA.gridPos.y - cellB.gridPos.y);

        if (dstX > dstY)
        {
            return 14 * dstY + 10 * (dstX - dstY);
        }

        return 14 * dstX + 10 * (dstY - dstX);
    }

    public List<Cell> GetUnvisitedNeighbours(Cell curCell)
    {
        // Create a list to return.
        List<Cell> neighbours = new List<Cell>();
        // Create a Cell object.
        Cell nCell = curCell;
        // Store current cell grid pos.
        Vector2 cPos = curCell.gridPos;

        foreach (Vector2 p in neighbourPositions)
        {
            // Find position of neighbour on grid, relative to current.
            Vector2 nPos = cPos + p;
            // If cell exists.
            if (allCells.ContainsKey(nPos)) nCell = allCells[nPos];
            // If cell is unvisited.
            if (unvisited.Contains(nCell)) neighbours.Add(nCell);
        }

        return neighbours;
    }

    // Compare neighbour with current and remove appropriate walls.
    public void CompareWalls(Cell cCell, Cell nCell)
    {
        // If neighbour is left of current.
        if (nCell.gridPos.x < cCell.gridPos.x)
        {
            RemoveWall(nCell.cScript, 2);
            RemoveWall(cCell.cScript, 1);
        }
        // Else if neighbour is right of current.
        else if (nCell.gridPos.x > cCell.gridPos.x)
        {
            RemoveWall(nCell.cScript, 1);
            RemoveWall(cCell.cScript, 2);
        }
        // Else if neighbour is above current.
        else if (nCell.gridPos.y > cCell.gridPos.y)
        {
            RemoveWall(nCell.cScript, 4);
            RemoveWall(cCell.cScript, 3);
        }
        // Else if neighbour is below current.
        else if (nCell.gridPos.y < cCell.gridPos.y)
        {
            RemoveWall(nCell.cScript, 3);
            RemoveWall(cCell.cScript, 4);
        }
    }

    // Function disables wall of your choosing, pass it the script attached to the desired cell
    // and an 'ID', where the ID = the wall. 1 = left, 2 = right, 3 = up, 4 = down.
    public void RemoveWall(CellScript cScript, int wallID)
    {
        if (wallID == 1) cScript.wallL.SetActive(false);
        else if (wallID == 2) cScript.wallR.SetActive(false);
        else if (wallID == 3) cScript.wallU.SetActive(false);
        else if (wallID == 4) cScript.wallD.SetActive(false);
    }

    public void SetGate
    (
        CellScript cScript,
        int wallID,
        bool isEnter
    )
    {
        Color color = isEnter ? Color.red : Color.green;
        if (wallID == 1) cScript.wallL.GetComponent<SpriteRenderer>().color = color;
        else if (wallID == 2) cScript.wallR.GetComponent<SpriteRenderer>().color = color;
        else if (wallID == 3) cScript.wallU.GetComponent<SpriteRenderer>().color = color;
        else if (wallID == 4) cScript.wallD.GetComponent<SpriteRenderer>().color = color;
    }

    public void CreateCentre()
    {
        // Get the 4 centre cells using the rows and columns variables.
        // Remove the required walls for each.
        centreCells[0] = allCells[new Vector2((mazeColumns / 2), (mazeRows / 2) + 1)];
        RemoveWall(centreCells[0].cScript, 4);
        RemoveWall(centreCells[0].cScript, 2);
        centreCells[1] = allCells[new Vector2((mazeColumns / 2) + 1, (mazeRows / 2) + 1)];
        RemoveWall(centreCells[1].cScript, 4);
        RemoveWall(centreCells[1].cScript, 1);
        centreCells[2] = allCells[new Vector2((mazeColumns / 2), (mazeRows / 2))];
        RemoveWall(centreCells[2].cScript, 3);
        RemoveWall(centreCells[2].cScript, 2);
        centreCells[3] = allCells[new Vector2((mazeColumns / 2) + 1, (mazeRows / 2))];
        RemoveWall(centreCells[3].cScript, 3);
        RemoveWall(centreCells[3].cScript, 1);

        // Create a List of ints, using this, select one at random and remove it.
        // We then use the remaining 3 ints to remove 3 of the centre cells from the 'unvisited' list.
        // This ensures that one of the centre cells will connect to the maze but the other three won't.
        // This way, the centre room will only have 1 entry / exit point.
        List<int> rndList = new List<int> {0, 1, 2, 3};
        int startCell = rndList[Random.Range(0, rndList.Count)];
        rndList.Remove(startCell);
        currentCell = centreCells[startCell];
        foreach (int c in rndList)
        {
            unvisited.Remove(centreCells[c]);
        }
    }

    public Cell GenerateCell(Vector2 pos, Vector2 keyPos)
    {
        // Create new Cell object.
        Cell newCell = new Cell();

        // Store reference to position in grid.
        newCell.gridPos = keyPos;
        // Set and instantiate cell GameObject.
        newCell.cellObject = Instantiate(cellPrefab, pos, cellPrefab.transform.rotation);
        // Child new cell to parent.
        if (mazeParent != null) newCell.cellObject.transform.parent = mazeParent.transform;
        // Set name of cellObject.
        newCell.cellObject.name = "Cell - X:" + keyPos.x + " Y:" + keyPos.y;
        // Get reference to attached CellScript.
        newCell.cScript = newCell.cellObject.GetComponent<CellScript>();
        newCell.cScript.SetType(CellType.Fog);
        // Disable Cell sprite, if applicable.
        if (disableCellSprite) newCell.cellObject.GetComponent<SpriteRenderer>().enabled = false;

        // Add to Lists.
        allCells[keyPos] = newCell;
        unvisited.Add(newCell);
        return newCell;
    }

    public void DeleteMaze()
    {
        if (mazeParent != null) Destroy(mazeParent);
    }

    public void InitValues()
    {
        // Check generation values to prevent generation failing.
        if (IsOdd(mazeRows)) mazeRows--;
        if (IsOdd(mazeColumns)) mazeColumns--;

        if (mazeRows <= 3) mazeRows = 4;
        if (mazeColumns <= 3) mazeColumns = 4;

        // Determine size of cell using localScale.
        cellSize = cellPrefab.transform.localScale.x;

        // Create an empty parent object to hold the maze in the scene.
        mazeParent = new GameObject();
        mazeParent.transform.position = Vector2.zero;
        mazeParent.name = "Maze";
    }

    public bool IsOdd(int value)
    {
        return value % 2 != 0;
    }

    public class Cell
    {
        public Vector2 gridPos;
        public GameObject cellObject;
        public CellScript cScript;
        public Cell parent;

        public int gCost;
        public int hCost;

        public int fCost
        {
            get { return gCost + hCost; }
        }
    }
}