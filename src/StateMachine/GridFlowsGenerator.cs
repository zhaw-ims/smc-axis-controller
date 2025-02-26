using System;
using System.Collections.Generic;

namespace SMCAxisController.StateMachine;
public static class GridFlowsGenerator
{
    public static List<SequenceFlow> GenerateGridFlows(List<MoveSequence> moveSequences, SequenceFlow generatedFlowPattern)
    {
        var flows = new List<SequenceFlow>();
        foreach (var moveSequence in moveSequences)
        {
            flows.Add(GenerateGridFlow(moveSequence, generatedFlowPattern));
        }
        return flows;
    }
    public static SequenceFlow GenerateGridFlow(MoveSequence moveSequence, SequenceFlow generatedFlowPattern)
    {
        // Assume the moveSequence.Name is in the format "MoveToSample_{row}_{column}"
        string rowStr = "0";
        string colStr = "0";
        var parts = moveSequence.Name.Split('_');
        if (parts.Length >= 3)
        {
            rowStr = parts[parts.Length - 2];
            colStr = parts[parts.Length - 1];
        }

        // Replace placeholders in the flow name
        var newFlowName = generatedFlowPattern.Name
            .Replace(SamplesGrid.RowNamePlaceholder, rowStr)
            .Replace(SamplesGrid.ColumnNamePlaceholder, colStr);

        // Create a new flow and clone the steps, replacing the placeholder in steps if needed.
        var newFlow = new SequenceFlow
        {
            Name = newFlowName,
            Steps = new List<SequenceStep>()
        };

        string sequencePrefix = parts.Length >= 3 ? parts[parts.Length - 3] : string.Empty;
        
        foreach (var step in generatedFlowPattern.Steps)
        {
            var newStep = new SequenceStep();
            
            if (!string.IsNullOrEmpty(sequencePrefix) && step.SequenceRef.Contains(sequencePrefix))
            {
                newStep.SequenceRef = moveSequence.Name;
            }
            else
            {
                newStep.SequenceRef = step.SequenceRef;
            }
            newFlow.Steps.Add(newStep);
        }

        return newFlow;
    }
}

