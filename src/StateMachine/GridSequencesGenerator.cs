using System;
using System.Collections.Generic;
using SMCAxisController.DataModel;

namespace SMCAxisController.StateMachine;

public static class GridSequencesGenerator
{
    public static List<MoveSequence> GenerateGridSequences(SamplesGrid samplesGrid)
    {
        var sequences = new List<MoveSequence>();

        for (int row = 0; row < samplesGrid.Rows; row++)
        {
            for (int column = 0; column < samplesGrid.Columns; column++)
            {
                // Calculate absolute row and column using GetPosition
                var pos = samplesGrid.GetPosition(row, column);

                // Clone the default movement parameters and override the TargetPosition
                MovementParameters rowParams = CloneAndSetTarget(samplesGrid.DefaultRowsMovementParameters, pos.Item1);
                MovementParameters columnParams = CloneAndSetTarget(samplesGrid.DefaultColumnMovementParameters, pos.Item2);
                MovementParameters heightParams = CloneAndSetTarget(samplesGrid.DefaultHeightMovementParameters, samplesGrid.TargetHeight);

                // Create the target positions for each axis
                var targets = new List<TargetPosition>
                {
                    new TargetPosition
                    {
                        ActuatorName = samplesGrid.RowsAxisName,
                        MovementParameters = rowParams
                    },
                    new TargetPosition
                    {
                        ActuatorName = samplesGrid.ColumnsAxisName,
                        MovementParameters = columnParams
                    },
                    new TargetPosition
                    {
                        ActuatorName = samplesGrid.HeightAxisName,
                        MovementParameters = heightParams
                    }
                };

                // Create the move sequence with the designated naming pattern
                var sequenceName = samplesGrid.SequenceNamePrefix
                    .Replace(SamplesGrid.RowNamePlaceholder, row.ToString())
                    .Replace(SamplesGrid.ColumnNamePlaceholder, column.ToString());

                var moveSequence = new MoveSequence
                {
                    Name = sequenceName,
                    TargetPositions = targets
                };

                sequences.Add(moveSequence);
            }
        }

        return sequences;
    }

    // Helper method to clone MovementParameters and set a new TargetPosition
    private static MovementParameters CloneAndSetTarget(MovementParameters original, int newTargetPosition)
    {
        return new MovementParameters
        {
            MovementMode = original.MovementMode,
            Speed = original.Speed,
            TargetPosition = newTargetPosition,
            Acceleration = original.Acceleration,
            Deceleration = original.Deceleration,
            PushingForce = original.PushingForce,
            TriggerLv = original.TriggerLv,
            PushingSpeed = original.PushingSpeed,
            PushingForceForPositioning = original.PushingForceForPositioning,
            Area1 = original.Area1,
            Area2 = original.Area2,
            PositioningWidth = original.PositioningWidth
        };
    }
}

