using SMCAxisController.DataModel;

namespace SMCAxisController.StateMachine;

public class SamplesGrid
{
    public const string RowNamePlaceholder = "ROW";
    public const string ColumnNamePlaceholder = "COLUMN";
    public string SequenceNamePrefix{ get; set; } = "MoveTo_ROW_COLUMN";
    public string RowsAxisName{ get; set; } = "Axis-X";
    public string ColumnsAxisName{ get; set; } = "Axis-Y";
    public string HeightAxisName{ get; set; } = "Axis-Z";
    public int RowBegin { get; set; }
    public int ColumnBegin { get; set; }
    public int Rows { get; set; } = 1;
    public int Columns { get; set; } = 1;
    public int RowsOffset{get;set;}
    public int ColumnsOffset{get;set;}
    public int VerticalTargetPosition{get;set;}
    public MovementParameters RowMovementParameters{get;set;} = new MovementParameters();
    public MovementParameters ColumnMovementParameters{get;set;} = new MovementParameters();
    public MovementParameters VerticalMovementParameters{get;set;} = new MovementParameters();
    public Tuple<int, int> GetPosition(int row, int column)
    {
        if (row < 0 || row >= Rows)
            throw new ArgumentOutOfRangeException(nameof(row), $"Row index must be between 0 and {Rows - 1}.");

        if (column < 0 || column >= Columns)
            throw new ArgumentOutOfRangeException(nameof(column), $"Column index must be between 0 and {Columns - 1}.");

        int absoluteRow = RowBegin + row * RowsOffset;
        int absoluteColumn = ColumnBegin + column * ColumnsOffset;
        
        return new Tuple<int, int>(absoluteRow, absoluteColumn);
    }
}