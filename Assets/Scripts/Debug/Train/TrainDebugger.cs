using UnityEngine;
using UnityEditor;

using WE.Debug.Debugger;
using WE.Core.Train.System;
using WE.Core.Railroad.System;
using WE.Core.Transform.System;
using WE.Core.Extensions;
using WE.Core.Cargo.System;

namespace WE.Debug.Train
{
  public class TrainDebugger : IDebugger
  {
    private readonly TrainUtilsSystem trainUtils;
    private readonly RailroadUtilsSystem railroadUtils;
    private readonly TransformUtilsSystem transformUtils;
    private readonly CargoUtilsSystem cargoUtils;
    private readonly TrainDebuggerInput.Create createInput;
    private readonly TrainDebuggerInput.Edit editInput;

    public string Name => "Train";

    public TrainDebugger(
      TrainUtilsSystem trainUtils,
      RailroadUtilsSystem railroadUtils,
      TransformUtilsSystem transformUtils,
      CargoUtilsSystem cargoUtils)
    {
      this.trainUtils = trainUtils;
      this.railroadUtils = railroadUtils;
      this.transformUtils = transformUtils;
      this.cargoUtils = cargoUtils;
      this.createInput = new TrainDebuggerInput.Create();
      this.editInput = new TrainDebuggerInput.Edit();
    }

    public void DebugOnGUI()
    {
      DrawCreateTrainGUI();
      DrawTrainsListGUI();
      DrawEditTrainGUI();
    }

    private void DrawCreateTrainGUI()
    {
      EditorGUILayout.BeginVertical("box");
      GUILayout.Label("Create Train", EditorStyles.boldLabel);

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

      createInput.MoveSpeed = EditorGUILayout.FloatField("Move Speed:", createInput.MoveSpeed);
      createInput.LoadingSpeed = EditorGUILayout.FloatField("Loading Speed:", createInput.LoadingSpeed);
      createInput.MaxResource = EditorGUILayout.IntField("Max Resource:", createInput.MaxResource);

      GUI.enabled = createInput.SelectedNode >= 0;
      if (GUILayout.Button("Create Train"))
      {
        trainUtils.CreateTrain(
          createInput.SelectedNode,
          createInput.MaxResource,
          createInput.MoveSpeed,
          createInput.LoadingSpeed
        );
      }
      GUI.enabled = true;

      EditorGUILayout.EndVertical();
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
      {
        EditorGUILayout.BeginHorizontal();
        DrawTrainListItem(train);
        EditorGUILayout.EndHorizontal();
      }

      EditorGUILayout.EndVertical();
    }

    private void DrawTrainListItem(int train)
    {
      var isSelected = train == editInput.SelectedTrain;
      GUI.backgroundColor = isSelected ? TrainDebuggerStyle.UI.SelectedBackground : Color.white;

      if (GUILayout.Button(new GUIContent($"Train {train}")))
      {
        if (isSelected)
          editInput.SelectedTrain = -1;
        else
        {
          editInput.SelectedTrain = train;
          var trainComponent = trainUtils.GetTrainComponent(train);
          editInput.MoveSpeed = trainComponent.maxSpeed;
          editInput.LoadingSpeed = cargoUtils.GetLoadingSpeed(train);
          editInput.MaxResource = cargoUtils.GetMaxResource(train);
        }
      }
      GUI.backgroundColor = Color.white;
    }

    private void DrawEditTrainGUI()
    {
      if (editInput.SelectedTrain >= 0 && trainUtils.IsTrain(editInput.SelectedTrain))
      {
        GUILayout.BeginVertical("box");
        GUILayout.Label($"Edit Train {editInput.SelectedTrain}", EditorStyles.boldLabel);

        editInput.MoveSpeed = EditorGUILayout.FloatField("Move Speed:", editInput.MoveSpeed);
        editInput.LoadingSpeed = EditorGUILayout.FloatField("Loading Speed:", editInput.LoadingSpeed);
        editInput.MaxResource = EditorGUILayout.IntField("Max Resource:", editInput.MaxResource);

        if (GUILayout.Button("Apply"))
        {
          trainUtils.UpdateTrainParameters(
            editInput.SelectedTrain,
            editInput.MaxResource,
            editInput.MoveSpeed,
            editInput.LoadingSpeed
          );
        }

        if (GUILayout.Button("Delete"))
        {
          trainUtils.DestroyTrain(editInput.SelectedTrain);
          editInput.SelectedTrain = -1;
        }

        GUILayout.EndVertical();
      }
    }

    public void DebugOnScene(SceneView sceneView)
    {
      using var trains = trainUtils.GetAllTrains(Unity.Collections.Allocator.Temp);
      foreach (var train in trains)
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

        var labelPos = position.ToVector3() + TrainDebuggerStyle.Train.LabelOffset;
        var style = new GUIStyle();
        style.normal.textColor = TrainDebuggerStyle.Train.DefaultColor;
        style.fontSize = TrainDebuggerStyle.Train.LabelSize;
        style.alignment = TrainDebuggerStyle.Train.LabelAlignment;
        Handles.Label(labelPos, $"Train {train}", style);
      }
    }

    public void DebugOnGizmos()
    {
    }
  }
}