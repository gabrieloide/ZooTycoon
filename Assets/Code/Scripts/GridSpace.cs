using UnityEngine;
public class GridSpace
{
    private bool isOccupied;
    Vector2 gridPosition;

    public GridSpace(bool isOccupied, float x, float y)
    {
        this.isOccupied = isOccupied;
        this.gridPosition = new Vector2(x, y);
    }

    public bool IsOccupied()
    {
        return isOccupied;
    }

    public void SetOccupied(bool occupied)
    {
        isOccupied = occupied;
    }

    public Vector2 GetGridPosition()
    {
        return gridPosition;
    }
}