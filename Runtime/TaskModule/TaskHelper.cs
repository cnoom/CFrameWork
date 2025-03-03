using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TaskModule;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

public static class TaskHelper
{
    private static readonly UpdateMonoBehaviour UpdateMono;
    internal static readonly ObjectPool<SequenceBuilder> sequencePool = new ObjectPool<SequenceBuilder>(
        () => new SequenceBuilder(),
        sequence => sequence.Clear(),
        sequence => sequence.Clear()
    );
    internal static ObjectPool<ParallelBuilder> ParallelPool = new ObjectPool<ParallelBuilder>(
        () => new ParallelBuilder(),
        parallel => parallel.Clear(),
        parallel => parallel.Clear()
    );
    static TaskHelper()
    {
        UpdateMono = new GameObject("TaskHelper").AddComponent<UpdateMonoBehaviour>();
        Object.DontDestroyOnLoad(UpdateMono.gameObject);
    }

    public static UpdateMonoBehaviour OnUpdate()
    {
        return UpdateMono;
    }

    // 基础等待操作
    public static UniTask Delay(float seconds, bool ignoreTimeScale = false, PlayerLoopTiming timing = PlayerLoopTiming.Update)
    {
        return UniTask.Delay(TimeSpan.FromSeconds(seconds), ignoreTimeScale, timing);
    }

    public static UniTask DelayFrame(int frame = 1, PlayerLoopTiming timing = PlayerLoopTiming.Update)
    {
        return UniTask.DelayFrame(frame, timing);
    }

    // 立即执行异步操作
    public static UniTask Action(Action action)
    {
        return UniTask.RunOnThreadPool(() =>
        {
            action?.Invoke();
            return UniTask.CompletedTask;
        });
    }

    // 创建顺序执行链
    public static SequenceBuilder Sequence() => sequencePool.Get();

    // 创建并行执行链
    public static ParallelBuilder Parallel() => ParallelPool.Get();

    public class UpdateMonoBehaviour : MonoBehaviour
    {
        private Action updateAction;

        public void Register(Action action)
        {
            updateAction += action;
        }

        public void UnRegister(Action action)
        {
            updateAction -= action;
        }

        private void Update()
        {
            updateAction?.Invoke();
        }
    }
}