using WE.Core.Base.Component;
using WE.Core.Mine.Component;
using WE.Core.Train.Component;
using WE.Core.Navigation;
using UnityEngine;

namespace WE.Core.Train.State
{

  public enum TrainState
  {
    Idle,
    Moving,
    Loading,
    Unloading
  }

  public interface ITrainState
  {
    void Process(int entity, ref TrainStateComponent state);
  }

  public class IdleState : ITrainState
  {
    private readonly TrainStateContext context;

    public IdleState(TrainStateContext context) => this.context = context;

    public void Process(int entity, ref TrainStateComponent state)
    {
      if (context.TrainUtils.IsMoving(entity))
        return;

      var moveSpeed = context.TrainUtils.GetMaxSpeed(entity);
      
      var bestTarget = context.CargoUtils.IsFull(entity)
        ? context.NavigationUtils.FindBestNode<BaseComponent>(entity, moveSpeed)
        : context.NavigationUtils.FindBestNode<MineComponent>(entity, moveSpeed);
      
      if (bestTarget.IsEmpty)
        return;

      state.state = TrainState.Moving;
      context.TrainUtils.Move(entity, bestTarget.path);
      bestTarget.Dispose();
    }
  }

  public class MovingState : ITrainState
  {
    private readonly TrainStateContext context;

    public MovingState(TrainStateContext context) => this.context = context;

    public void Process(int entity, ref TrainStateComponent state)
    {
      if (context.TrainUtils.IsMoving(entity))
        return;

      var currentNode = context.TrainUtils.GetCurrentNode(entity);
      state.state = DetermineNextState(currentNode);
    }

    private TrainState DetermineNextState(int node)
    {
      if (context.BaseUtils.IsBase(node)) return TrainState.Unloading;
      if (context.MineUtils.IsMine(node)) return TrainState.Loading;
      return TrainState.Idle;
    }
  }

  public class LoadingState : ITrainState
  {
    private readonly TrainStateContext context;

    public LoadingState(TrainStateContext context) => this.context = context;

    public void Process(int entity, ref TrainStateComponent state)
    {
      if (context.CargoUtils.IsFull(entity))
      {
        state.state = TrainState.Idle;
        return;
      }

      if (context.MineUtils.IsMining(entity))
        return;

      var mineEntity = context.TrainUtils.GetCurrentNode(entity);
      context.MineUtils.Mine(entity, mineEntity);
    }
  }

  public class UnloadingState : ITrainState
  {
    private readonly TrainStateContext context;

    public UnloadingState(TrainStateContext context) => this.context = context;

    public void Process(int entity, ref TrainStateComponent state)
    {
      if (context.CargoUtils.IsEmpty(entity))
      {
        state.state = TrainState.Idle;
        return;
      }

      var baseEntity = context.TrainUtils.GetCurrentNode(entity);
      context.BaseUtils.Unload(entity, baseEntity);
      state.state = TrainState.Idle;
    }
  }

}