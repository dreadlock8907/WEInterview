using UnityEngine;
using UnityEditor;
using WE.Core.Railroad.System;
using WE.Core.Transform.System;
using WE.Debug.Debugger;
using WE.Core.Extensions;
using Unity.Collections;
using System;
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
    }

    public void DebugOnScene(SceneView sceneView)
    {
      using var nodes = railroadUtils.GetAllNodes(Allocator.Temp);
      var nodeInfoBuilder = new System.Text.StringBuilder();
      
      for (int i = 0; i < nodes.Length; i++)
      {
        var entity = nodes[i];
        var nodePosition = transformUtils.GetPosition(entity).ToVector3();
        
        nodeInfoBuilder.Clear();
        nodeInfoBuilder.Append("Node ").Append(entity);
        if (mineUtils.IsMine(entity))
        {
          var multiplier = mineUtils.GetLoadingMultiplier(entity);
          nodeInfoBuilder.Append("\nMine (x").AppendFormat("{0:F1}", multiplier).Append(')');
        }
        if (baseUtils.IsBase(entity))
        {
          var multiplier = baseUtils.GetResourceMultiplier(entity);
          nodeInfoBuilder.Append("\nBase (x").AppendFormat("{0:F1}", multiplier).Append(')');
        }

        var labelPosition = nodePosition + Vector3.up * RailroadDebuggerStyle.Node.Size * 2f;
        var style = new GUIStyle();
        style.normal.textColor = GetNodeColor(entity, false);
        style.fontSize = RailroadDebuggerStyle.Labels.FontSize;
        style.alignment = RailroadDebuggerStyle.Labels.Alignment;
        Handles.Label(labelPosition, nodeInfoBuilder.ToString(), style);
        
        foreach (var connectedEntity in railroadUtils.GetNodeConnections(entity))
        {
          // Draw connection just one time
          if (entity > connectedEntity) continue;
          
          var connectedPosition = transformUtils.GetPosition(connectedEntity).ToVector3();
          var connectionCenter = Vector3.Lerp(nodePosition, connectedPosition, 0.5f);
          var distance = Vector3.Distance(nodePosition, connectedPosition);
          
          var distanceStyle = new GUIStyle();
          distanceStyle.normal.textColor = RailroadDebuggerStyle.Labels.TextColor;
          distanceStyle.fontSize = RailroadDebuggerStyle.Labels.FontSize;
          distanceStyle.alignment = RailroadDebuggerStyle.Labels.Alignment;
          
          Handles.Label(connectionCenter, $"{distance:F1}m", distanceStyle);
        }
      }
    }

    public void DebugOnGizmos()
    {
      using var nodes = railroadUtils.GetAllNodes(Allocator.Temp);

      for (int i = 0; i < nodes.Length; i++)
      {
        var entity = nodes[i];
        var isSelected = entity == editInput.SelectedNodeEntity;
        var nodePosition = transformUtils.GetPosition(entity).ToVector3();
        
        // Draw node
        Gizmos.color = GetNodeColor(entity, isSelected);
        
        var nodeSize = isSelected ? 
            RailroadDebuggerStyle.Node.SelectedSize : 
            RailroadDebuggerStyle.Node.Size;
        
        Gizmos.DrawWireSphere(nodePosition, nodeSize);

        // Draw connections
        foreach (var connectedEntity in railroadUtils.GetNodeConnections(entity))
        {
          // Рисуем связь только один раз (когда entity < connectedEntity)
          if (entity > connectedEntity) continue;
          
          // Подсвечиваем связи, если хотя бы одна из нод выбрана
          Gizmos.color = (isSelected || connectedEntity == editInput.SelectedNodeEntity) ? 
              RailroadDebuggerStyle.Connection.SelectedColor : 
              RailroadDebuggerStyle.Connection.DefaultColor;
              
          var connectedPosition = transformUtils.GetPosition(connectedEntity).ToVector3();
          Gizmos.DrawLine(nodePosition, connectedPosition);
        }
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

    public void DebugOnGUI()
    {
      CreateNodeGUI();
      NodesListGUI();
      EditNodeGUI();
    }

    private void CreateNodeGUI()
    {
      GUILayout.Space(10);
      GUILayout.Label("Create Node", EditorStyles.boldLabel);

      EditorGUILayout.BeginHorizontal();
      GUILayout.Label("Position:", GUILayout.Width(60));
      createInput.Position = EditorGUILayout.Vector3Field("", createInput.Position.ToVector3()).ToFloat3();
      EditorGUILayout.EndHorizontal();

      if (GUILayout.Button("Create Node", GUILayout.Height(30)))
      {
        railroadUtils.CreateNode(createInput.Position);
      }
    }

    private void NodesListGUI()
    {
      using var nodes = railroadUtils.GetAllNodes(Allocator.Temp);
      GUILayout.Space(10);
      GUILayout.Label($"Nodes: {nodes.Length}", EditorStyles.boldLabel);
      for (int i = 0; i < nodes.Length; i++)
      {
        var entity = nodes[i];
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
    }

    private void EditNodeGUI()
    {
      if (editInput.SelectedNodeEntity != -1)
      {
        GUILayout.Space(10);
        GUILayout.Label($"Edit Node {editInput.SelectedNodeEntity}", EditorStyles.boldLabel);

        // Position edit
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Position:", GUILayout.Width(60));
        var currentPosition = transformUtils.GetPosition(editInput.SelectedNodeEntity).ToVector3();
        var newPosition = EditorGUILayout.Vector3Field("", currentPosition);
        if (currentPosition != newPosition)
        {
          transformUtils.UpdatePosition(editInput.SelectedNodeEntity, newPosition.ToFloat3());
        }
        EditorGUILayout.EndHorizontal();

        // Node type buttons
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

        // Node parameters
        if (mineUtils.IsMine(editInput.SelectedNodeEntity))
        {
          GUILayout.Space(5);
          EditorGUILayout.BeginHorizontal("box");
          GUILayout.Label("Loading Multiplier:", GUILayout.Width(110));
          var currentMultiplier = mineUtils.GetLoadingMultiplier(editInput.SelectedNodeEntity);
          var newMultiplier = EditorGUILayout.FloatField(currentMultiplier);
          if (currentMultiplier != newMultiplier)
          {
            mineUtils.Setup(editInput.SelectedNodeEntity, newMultiplier);
          }
          EditorGUILayout.EndHorizontal();
        }
        else if (baseUtils.IsBase(editInput.SelectedNodeEntity))
        {
          GUILayout.Space(5);
          EditorGUILayout.BeginHorizontal("box");
          GUILayout.Label("Resource Multiplier:", GUILayout.Width(110));
          var currentMultiplier = baseUtils.GetResourceMultiplier(editInput.SelectedNodeEntity);
          var newMultiplier = EditorGUILayout.FloatField(currentMultiplier);
          if (currentMultiplier != newMultiplier)
          {
            baseUtils.Setup(editInput.SelectedNodeEntity, newMultiplier);
          }
          EditorGUILayout.EndHorizontal();
        }

        // Connections list
        var connections = railroadUtils.GetNodeConnections(editInput.SelectedNodeEntity);
        GUILayout.Label($"Connections: {connections.Length}");
        foreach (var connectedEntity in connections)
        {
          EditorGUILayout.BeginHorizontal();
          GUILayout.Label($"Node {connectedEntity}");
          if (GUILayout.Button("Unlink", GUILayout.Width(60)))
          {
            railroadUtils.UnlinkNodes(editInput.SelectedNodeEntity, connectedEntity);
          }
          EditorGUILayout.EndHorizontal();
        }

        // Link section
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
          if (nodeEntity == editInput.SelectedNodeEntity) continue;

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
          editInput.LinkTargetEntity = -1; // Reset selection after linking
        }
        EditorGUILayout.EndHorizontal();

        // Delete node button
        GUILayout.Space(10);
        if (GUILayout.Button("Delete Node", EditorStyles.miniButtonRight))
        {
          railroadUtils.DestroyNode(editInput.SelectedNodeEntity);
          editInput.SelectedNodeEntity = -1;
        }
      }
    }

  }
}