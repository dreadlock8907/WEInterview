using UnityEngine;
using UnityEditor;
using Unity.Collections;

using WE.Debug.Debugger;
using WE.Core.Train.System;
using WE.Core.Railroad.System;
using WE.Core.Transform.System;
using WE.Core.Extensions;
using WE.Core.Cargo.System;
using WE.Core.Mine.System;

namespace WE.Debug.Train
{
  public class TrainDebugger : IDebugger
  {
    private readonly TrainUtilsSystem trainUtils;
    private readonly RailroadUtilsSystem railroadUtils;
    private readonly TransformUtilsSystem transformUtils;
    private readonly CargoUtilsSystem cargoUtils;
    private readonly MineUtilsSystem mineUtils;
    private readonly TrainDebuggerInput.Create createInput;
    private readonly TrainDebuggerInput.Edit editInput;

    public string Name => "Train";

    public TrainDebugger(
      TrainUtilsSystem trainUtils,
      RailroadUtilsSystem railroadUtils,
      TransformUtilsSystem transformUtils,
      CargoUtilsSystem cargoUtils,
      MineUtilsSystem mineUtils)
    {
      this.trainUtils = trainUtils;
      this.railroadUtils = railroadUtils;
      this.transformUtils = transformUtils;
      this.cargoUtils = cargoUtils;
      this.mineUtils = mineUtils;
      this.createInput = new TrainDebuggerInput.Create();
      this.editInput = new TrainDebuggerInput.Edit();
    }

    public void DebugOnGUI()
    {
      DrawCreateTrainGUI();
      DrawTrainsListGUI();
      DrawEditTrainGUI();
    }

    public void DebugOnScene(SceneView sceneView)
    {
      using var trains = trainUtils.GetAllTrains(Unity.Collections.Allocator.Temp);
      foreach (var train in trains)
      {
        DrawTrainGizmo(train);
        DrawTrainLabel(train);
        if (train == editInput.SelectedTrain)
          DrawTrainPath(train);
      }
    }

    public void DebugOnGizmos()
    {
    }

    public void OnDeselect()
    {
      editInput.SelectedTrain = -1;
    }

    private void DrawCreateTrainGUI()
    {
      EditorGUILayout.BeginVertical("box");
      GUILayout.Label("Create Train", EditorStyles.boldLabel);
      DrawStartNodeSelector();
      DrawTrainParameters();
      DrawCreateButton();
      EditorGUILayout.EndVertical();
    }

    private void DrawStartNodeSelector()
    {
      EditorGUILayout.LabelField("Start Node:");
      using var nodes = railroadUtils.GetAllNodes(Unity.Collections.Allocator.Temp);
      
      var options = new string[nodes.Length + 1];
      var optionValues = new int[nodes.Length + 1];
      options[0] = "Select node...";
      optionValues[0] = -1;

      for (int i = 0; i < nodes.Length; i++)
      {
        options[i + 1] = $"Node {nodes[i]}";
        optionValues[i + 1] = nodes[i];
      }

      var selectedIndex = 0;
      for (int i = 0; i < optionValues.Length; i++)
      {
        if (optionValues[i] == createInput.SelectedNode)
        {
          selectedIndex = i;
          break;
        }
      }

      var newSelectedIndex = EditorGUILayout.Popup(selectedIndex, options);
      createInput.SelectedNode = optionValues[newSelectedIndex];
    }

    private void DrawTrainParameters()
    {
      createInput.MoveSpeed = EditorGUILayout.FloatField("Move Speed:", createInput.MoveSpeed);
      createInput.LoadingTime = EditorGUILayout.FloatField("Loading Speed:", createInput.LoadingTime);
      createInput.MaxResource = EditorGUILayout.IntField("Max Resource:", createInput.MaxResource);
    }

    private void DrawCreateButton()
    {
      GUI.enabled = createInput.SelectedNode >= 0;
      if (GUILayout.Button("Create Train"))
      {
        trainUtils.CreateTrain(
          createInput.SelectedNode,
          createInput.MaxResource,
          createInput.MoveSpeed,
          createInput.LoadingTime
        );
      }
      GUI.enabled = true;
    }

    private void DrawTrainsListGUI()
    {
      EditorGUILayout.BeginVertical("box");
      using var trains = trainUtils.GetAllTrains(Unity.Collections.Allocator.Temp);
      
      if (trains.Length == 0)
      {
        EditorGUILayout.LabelField("No trains");
        EditorGUILayout.EndVertical();
        return;
      }

      EditorGUILayout.LabelField($"Trains: {trains.Length}", EditorStyles.boldLabel);

      foreach (var train in trains)
        DrawTrainListItem(train);

      EditorGUILayout.EndVertical();
    }

    private void DrawTrainListItem(int train)
    {
      var isSelected = train == editInput.SelectedTrain;
      GUI.backgroundColor = isSelected ? TrainDebuggerStyle.UI.SelectedBackground : Color.white;

      EditorGUILayout.BeginHorizontal();
      if (GUILayout.Button($"Train {train}"))
      {
        if (isSelected)
          editInput.SelectedTrain = -1;
        else
        {
          editInput.SelectedTrain = train;
          UpdateEditInputFromTrain(train);
        }
      }
      EditorGUILayout.EndHorizontal();
      
      GUI.backgroundColor = Color.white;
    }

    private void UpdateEditInputFromTrain(int train)
    {
      var trainComponent = trainUtils.GetTrainComponent(train);
      editInput.MoveSpeed = trainComponent.maxSpeed;
      editInput.LoadingTime = cargoUtils.GetLoadingTime(train);
      editInput.MaxResource = cargoUtils.GetMaxResource(train);
    }

    private void DrawEditTrainGUI()
    {
      if (editInput.SelectedTrain < 0 || !trainUtils.IsTrain(editInput.SelectedTrain)) 
        return;

      EditorGUILayout.BeginVertical("box");
      GUILayout.Label($"Edit Train {editInput.SelectedTrain}", EditorStyles.boldLabel);
      DrawEditParameters();
      DrawEditButtons();
      EditorGUILayout.EndVertical();
    }

    private void DrawEditParameters()
    {
      editInput.MoveSpeed = EditorGUILayout.FloatField("Move Speed:", editInput.MoveSpeed);
      editInput.LoadingTime = EditorGUILayout.FloatField("Loading Time:", editInput.LoadingTime);
      editInput.MaxResource = EditorGUILayout.IntField("Max Resource:", editInput.MaxResource);
    }

    private void DrawEditButtons()
    {
      if (GUILayout.Button("Apply"))
      {
        trainUtils.UpdateTrainParameters(
          editInput.SelectedTrain,
          editInput.MaxResource,
          editInput.MoveSpeed,
          editInput.LoadingTime
        );
      }

      if (GUILayout.Button("Delete"))
      {
        trainUtils.DestroyTrain(editInput.SelectedTrain);
        editInput.SelectedTrain = -1;
      }
    }

    private void DrawTrainGizmo(int train)
    {
      var position = transformUtils.GetPosition(train);
      Handles.color = editInput.SelectedTrain == train ? TrainDebuggerStyle.Train.SelectedColor : TrainDebuggerStyle.Train.DefaultColor;

      Handles.CubeHandleCap(
        0,
        position.ToVector3(),
        Quaternion.identity,
        TrainDebuggerStyle.Train.Size.x,
        EventType.Repaint
      );
    }

    private void DrawTrainLabel(int train)
    {
      var position = transformUtils.GetPosition(train);
      var labelPos = position.ToVector3() + TrainDebuggerStyle.Train.LabelOffset;
      
      var style = new GUIStyle
      {
        normal = { textColor = TrainDebuggerStyle.Train.LabelColor },
        fontSize = TrainDebuggerStyle.Train.LabelSize,
        alignment = TrainDebuggerStyle.Train.LabelAlignment
      };
      
      var state = trainUtils.GetTrainState(train);
      var currentResource = cargoUtils.GetCurrentResource(train);
      var maxResource = cargoUtils.GetMaxResource(train);
      var miningProgress = mineUtils.GetMiningProgress(train);
      
      var totalProgress = (currentResource + miningProgress) / maxResource * 100f;
      
      var labelBuilder = new System.Text.StringBuilder();
      labelBuilder.AppendLine($"Train {train}");
      labelBuilder.AppendLine($"{state}");
      labelBuilder.AppendLine($"{currentResource}/{maxResource}");
      if (state == Core.Train.State.TrainState.Loading)
        labelBuilder.AppendLine($"{totalProgress:F0}%");

      Handles.Label(labelPos, labelBuilder.ToString(0, labelBuilder.Length), style);
    }

    private void DrawTrainPath(int train)
    {
      if (!trainUtils.IsMoving(train)) 
        return;
        
      var route = trainUtils.GetTrainRoute(train);
      if (route.Length < 2) 
        return;
        
      Handles.color = TrainDebuggerStyle.Train.SelectedColor;
      
      for (int i = 0; i < route.Length - 1; i++)
      {
        var startPos = transformUtils.GetPosition(route[i]).ToVector3();
        var endPos = transformUtils.GetPosition(route[i + 1]).ToVector3();
        Handles.DrawLine(startPos, endPos);
      }
    }
  }
}