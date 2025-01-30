using UnityEngine;
using UnityEditor;
using Unity.Collections;
using System.Collections.Generic;

using WE.Core.Railroad.System;
using WE.Core.Transform.System;
using WE.Debug.Debugger;
using WE.Core.Extensions;
using WE.Core.Mine.System;
using WE.Core.Base.System;

namespace WE.Debug.Railroad
{
  public class RailroadDebugger : IDebugger
  {
    private readonly RailroadUtilsSystem railroadUtils;
    private readonly TransformUtilsSystem transformUtils;
    private readonly MineUtilsSystem mineUtils;
    private readonly BaseUtilsSystem baseUtils;

    private readonly RailroadDebuggerInput.Create createInput;
    private readonly RailroadDebuggerInput.Edit editInput;
    private readonly System.Text.StringBuilder nodeInfoBuilder = new();
    private readonly RailroadDebuggerSerializer serializer;

    public string Name => "Railroad";

    public RailroadDebugger(RailroadUtilsSystem railroadUtils, TransformUtilsSystem transformUtils,
                           MineUtilsSystem mineUtils, BaseUtilsSystem baseUtils)
    {
      this.railroadUtils = railroadUtils;
      this.transformUtils = transformUtils;
      this.mineUtils = mineUtils;
      this.baseUtils = baseUtils;
      this.createInput = new RailroadDebuggerInput.Create();
      this.editInput = new RailroadDebuggerInput.Edit();
      this.serializer = new RailroadDebuggerSerializer(railroadUtils, transformUtils, mineUtils, baseUtils);
    }

    public void DebugOnGizmos()
    {
      using var nodes = railroadUtils.GetAllNodes(Allocator.Temp);
      foreach (var entity in nodes)
      {
        DrawNodeGizmo(entity);
        DrawNodeConnectionsGizmo(entity);
      }
    }

    public void DebugOnScene(SceneView sceneView)
    {
      using var nodes = railroadUtils.GetAllNodes(Allocator.Temp);
      foreach (var entity in nodes)
      {
        DrawNodeLabel(entity);
        DrawNodeConnections(entity);
      }
    }

    public void DebugOnGUI()
    {
      CreateNodeGUI();
      SaveLoadGUI();
      NodesListGUI();
      EditNodeGUI();
    }

    private void DrawNodeLabel(int entity)
    {
      var nodePosition = transformUtils.GetPosition(entity).ToVector3();
      BuildNodeInfo(entity);

      var labelPosition = nodePosition + Vector3.up * RailroadDebuggerStyle.Node.Size * 2f;
      var style = new GUIStyle
      {
        normal = { textColor = GetNodeColor(entity, false) },
        fontSize = RailroadDebuggerStyle.Labels.FontSize,
        alignment = RailroadDebuggerStyle.Labels.Alignment
      };

      Handles.Label(labelPosition, nodeInfoBuilder.ToString(), style);
    }

    private void BuildNodeInfo(int entity)
    {
      nodeInfoBuilder.Clear();
      nodeInfoBuilder.Append("Node ").Append(entity);

      if (mineUtils.IsMine(entity))
      {
        var multiplier = mineUtils.GetMiningMultiplier(entity);
        nodeInfoBuilder.Append("\nMine (x").AppendFormat("{0:F1}", multiplier).Append(')');
      }

      if (baseUtils.IsBase(entity))
      {
        var multiplier = baseUtils.GetResourceMultiplier(entity);
        nodeInfoBuilder.Append("\nBase (x").AppendFormat("{0:F1}", multiplier).Append(')');
      }
    }

    private void DrawNodeConnections(int entity)
    {
      var nodePosition = transformUtils.GetPosition(entity).ToVector3();

      foreach (var connectedEntity in railroadUtils.GetNodeConnections(entity))
      {
        if (entity > connectedEntity)
          continue;

        DrawConnectionLabel(nodePosition, connectedEntity);
      }
    }

    private void DrawConnectionLabel(Vector3 fromPosition, int toEntity)
    {
      var toPosition = transformUtils.GetPosition(toEntity).ToVector3();
      var connectionCenter = Vector3.Lerp(fromPosition, toPosition, 0.5f);
      var distance = Vector3.Distance(fromPosition, toPosition);

      var style = new GUIStyle
      {
        normal = { textColor = RailroadDebuggerStyle.Labels.TextColor },
        fontSize = RailroadDebuggerStyle.Labels.FontSize,
        alignment = RailroadDebuggerStyle.Labels.Alignment
      };

      Handles.Label(connectionCenter, $"{distance:F1}m", style);
    }

    private void DrawNodeGizmo(int entity)
    {
      var isSelected = entity == editInput.SelectedNodeEntity;
      var nodePosition = transformUtils.GetPosition(entity).ToVector3();

      Gizmos.color = GetNodeColor(entity, isSelected);
      var nodeSize = isSelected ?
          RailroadDebuggerStyle.Node.SelectedSize :
          RailroadDebuggerStyle.Node.Size;

      Gizmos.DrawWireSphere(nodePosition, nodeSize);
    }

    private void DrawNodeConnectionsGizmo(int entity)
    {
      var nodePosition = transformUtils.GetPosition(entity).ToVector3();
      var isSelected = entity == editInput.SelectedNodeEntity;

      foreach (var connectedEntity in railroadUtils.GetNodeConnections(entity))
      {
        if (entity > connectedEntity)
          continue;

        Gizmos.color = (isSelected || connectedEntity == editInput.SelectedNodeEntity) ?
            RailroadDebuggerStyle.Connection.SelectedColor :
            RailroadDebuggerStyle.Connection.DefaultColor;

        var connectedPosition = transformUtils.GetPosition(connectedEntity).ToVector3();
        Gizmos.DrawLine(nodePosition, connectedPosition);
      }
    }

    private Color GetNodeColor(int entity, bool isSelected)
    {
      if (isSelected)
        return RailroadDebuggerStyle.Node.SelectedColor;

      if (mineUtils.IsMine(entity))
        return RailroadDebuggerStyle.Node.MineColor;

      if (baseUtils.IsBase(entity))
        return RailroadDebuggerStyle.Node.BaseColor;

      return RailroadDebuggerStyle.Node.DefaultColor;
    }

    private void CreateNodeGUI()
    {
      GUILayout.Space(10);
      GUILayout.Label("Create Node", EditorStyles.boldLabel);
      DrawCreateNodePosition();
      DrawCreateNodeButton();
    }

    private void DrawCreateNodePosition()
    {
      EditorGUILayout.BeginHorizontal();
      GUILayout.Label("Position:", GUILayout.Width(60));
      createInput.Position = EditorGUILayout.Vector3Field("", createInput.Position.ToVector3()).ToFloat3();
      EditorGUILayout.EndHorizontal();
    }

    private void DrawCreateNodeButton()
    {
      if (GUILayout.Button("Create Node", GUILayout.Height(30)))
        railroadUtils.CreateNode(createInput.Position);
    }

    private void NodesListGUI()
    {
      using var nodes = railroadUtils.GetAllNodes(Allocator.Temp);
      GUILayout.Space(10);
      GUILayout.Label($"Nodes: {nodes.Length}", EditorStyles.boldLabel);
      foreach (var entity in nodes)
        DrawNodesListItem(entity);
    }

    private void DrawNodesListItem(int entity)
    {
      var position = transformUtils.GetPosition(entity).ToVector3();
      var isSelected = entity == editInput.SelectedNodeEntity;

      EditorGUILayout.BeginHorizontal();
      var oldBgColor = GUI.backgroundColor;
      var style = new GUIStyle(EditorStyles.miniButton);
      style.normal.textColor = GetNodeColor(entity, false);

      if (isSelected)
      {
        style.fontStyle = FontStyle.Bold;
        style.normal.background = EditorGUIUtility.whiteTexture;
        GUI.backgroundColor = RailroadDebuggerStyle.UI.SelectedBackground;
      }
      if (GUILayout.Button($"Node {entity} at {position}", style))
      {
        editInput.SelectedNodeEntity = (entity == editInput.SelectedNodeEntity) ? -1 : entity;
      }
      GUI.backgroundColor = oldBgColor;

      EditorGUILayout.EndHorizontal();
    }

    private void EditNodeGUI()
    {
      if (editInput.SelectedNodeEntity == -1) 
        return;

      GUILayout.Space(10);
      GUILayout.Label($"Edit Node {editInput.SelectedNodeEntity}", EditorStyles.boldLabel);

      DrawPositionEdit();
      DrawNodeTypeButtons();
      DrawNodeParameters();
      DrawConnectionsList();
      DrawLinkSection();
      DrawDeleteButton();
    }

    private void DrawPositionEdit()
    {
      EditorGUILayout.BeginHorizontal();
      GUILayout.Label("Position:", GUILayout.Width(60));
      var currentPosition = transformUtils.GetPosition(editInput.SelectedNodeEntity).ToVector3();
      var newPosition = EditorGUILayout.Vector3Field("", currentPosition);
      if (currentPosition != newPosition)
        transformUtils.UpdatePosition(editInput.SelectedNodeEntity, newPosition.ToFloat3());
      EditorGUILayout.EndHorizontal();
    }

    private void DrawNodeTypeButtons()
    {
      GUILayout.Space(10);
      GUILayout.Label("Node Type:", EditorStyles.boldLabel);
      EditorGUILayout.BeginHorizontal();

      if (GUILayout.Button("Set as Default", EditorStyles.miniButtonLeft))
      {
        if (mineUtils.IsMine(editInput.SelectedNodeEntity))
          mineUtils.RemoveMine(editInput.SelectedNodeEntity);
        if (baseUtils.IsBase(editInput.SelectedNodeEntity))
          baseUtils.RemoveBase(editInput.SelectedNodeEntity);
      }

      if (GUILayout.Button("Set as Mine", EditorStyles.miniButtonMid))
      {
        if (baseUtils.IsBase(editInput.SelectedNodeEntity))
          baseUtils.RemoveBase(editInput.SelectedNodeEntity);
        mineUtils.Setup(editInput.SelectedNodeEntity, 1);
      }

      if (GUILayout.Button("Set as Base", EditorStyles.miniButtonRight))
      {
        if (mineUtils.IsMine(editInput.SelectedNodeEntity))
          mineUtils.RemoveMine(editInput.SelectedNodeEntity);
        baseUtils.Setup(editInput.SelectedNodeEntity, 1);
      }

      EditorGUILayout.EndHorizontal();
    }

    private void DrawNodeParameters()
    {
      DrawMineParameters();
      DrawBaseParameters();
    }

    private void DrawMineParameters()
    {
      if (!mineUtils.IsMine(editInput.SelectedNodeEntity)) 
        return;
      GUILayout.Space(5);
      EditorGUILayout.BeginHorizontal("box");
      GUILayout.Label("Loading Multiplier:", GUILayout.Width(110));
      var currentMultiplier = mineUtils.GetMiningMultiplier(editInput.SelectedNodeEntity);
      var newMultiplier = EditorGUILayout.FloatField(currentMultiplier);
      if (currentMultiplier != newMultiplier)
        mineUtils.Setup(editInput.SelectedNodeEntity, newMultiplier);
      EditorGUILayout.EndHorizontal();
    }

    private void DrawBaseParameters()
    {
      if (!baseUtils.IsBase(editInput.SelectedNodeEntity)) 
        return;
      GUILayout.Space(5);
      EditorGUILayout.BeginHorizontal("box");
      GUILayout.Label("Resource Multiplier:", GUILayout.Width(110));
      var currentMultiplier = baseUtils.GetResourceMultiplier(editInput.SelectedNodeEntity);
      var newMultiplier = EditorGUILayout.FloatField(currentMultiplier);
      if (currentMultiplier != newMultiplier)
        baseUtils.Setup(editInput.SelectedNodeEntity, newMultiplier);
      EditorGUILayout.EndHorizontal();
    }

    private void DrawConnectionsList()
    {
      var connections = railroadUtils.GetNodeConnections(editInput.SelectedNodeEntity);
      GUILayout.Label($"Connections: {connections.Length}");
      foreach (var connectedEntity in connections)
      {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label($"Node {connectedEntity}");
        if (GUILayout.Button("Unlink", GUILayout.Width(60)))
          railroadUtils.UnlinkNodes(editInput.SelectedNodeEntity, connectedEntity);
        EditorGUILayout.EndHorizontal();
      }
    }

    private void DrawLinkSection()
    {
      GUILayout.Space(5);
      EditorGUILayout.BeginHorizontal();
      GUILayout.Label("Link to:", GUILayout.Width(60));
      using var allNodes = railroadUtils.GetAllNodes(Allocator.Temp);
      var options = new string[allNodes.Length + 1];
      var optionValues = new int[allNodes.Length + 1];
      options[0] = "Select node...";
      optionValues[0] = -1;

      var selectedIndex = 0;
      for (int i = 0; i < allNodes.Length; i++)
      {
        var nodeEntity = allNodes[i];
        if (nodeEntity == editInput.SelectedNodeEntity)
          continue;

        var pos = transformUtils.GetPosition(nodeEntity).ToVector3();
        options[i + 1] = $"Node {nodeEntity} at {pos}";
        optionValues[i + 1] = nodeEntity;

        if (nodeEntity == editInput.LinkTargetEntity)
          selectedIndex = i + 1;
      }

      var newSelectedIndex = EditorGUILayout.Popup(selectedIndex, options);
      editInput.LinkTargetEntity = optionValues[newSelectedIndex];

      if (GUILayout.Button("Link", GUILayout.Width(60)))
      {
        railroadUtils.LinkNodes(editInput.SelectedNodeEntity, editInput.LinkTargetEntity);
        editInput.LinkTargetEntity = -1;
      }
      EditorGUILayout.EndHorizontal();
    }

    private void DrawDeleteButton()
    {
      GUILayout.Space(10);
      if (GUILayout.Button("Delete Node", EditorStyles.miniButtonRight))
      {
        railroadUtils.DestroyNode(editInput.SelectedNodeEntity);
        editInput.SelectedNodeEntity = -1;
      }
    }

    public void OnDeselect()
    {
      editInput.SelectedNodeEntity = -1;
      editInput.LinkTargetEntity = -1;
    }

    private void SaveLoadGUI()
    {
      GUILayout.Space(10);
      GUILayout.Label("Save/Load Network", EditorStyles.boldLabel);
      
      if (GUILayout.Button("Save to JSON"))
      {
        var path = EditorUtility.SaveFilePanel(
          "Save Railroad Network",
          Application.dataPath,
          "railroad_network.json",
          "json"
        );
        serializer.WriteTo(path);
      }
      
      if (GUILayout.Button("Load from JSON"))
      {
        var path = EditorUtility.OpenFilePanel(
          "Load Railroad Network",
          Application.dataPath,
          "json"
        );
        serializer.ReadFrom(path);
      }
    }
  }
}