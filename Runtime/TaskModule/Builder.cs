using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using LogModule;

namespace TaskModule
{
    public abstract class Builder
    {
        protected readonly List<Func<UniTask>> Tasks = new List<Func<UniTask>>();

        public abstract UniTaskVoid RunAsync();

        public void Clear()
        {
            Tasks.Clear();
        }
        
        public Builder Append(Action action)
        {
            Tasks.Add(() => TaskHelper.Action(action));
            return this;
        }

        public Builder Append(Func<UniTask> uniTask)
        {
            Tasks.Add(uniTask);
            return this;
        }

        public Builder Delay(float seconds, bool ignoreTimeScale = false, PlayerLoopTiming timing = PlayerLoopTiming.Update)
        {
            Tasks.Add(() => TaskHelper.Delay(seconds, ignoreTimeScale, timing));
            return this;
        }

        public Builder DelayFrame(int frame = 1, PlayerLoopTiming timing = PlayerLoopTiming.Update)
        {
            Tasks.Add(() => TaskHelper.DelayFrame(frame, timing));
            return this;
        }
    }

    // 顺序执行构建器
    public class SequenceBuilder : Builder
    {
        public override async UniTaskVoid RunAsync()
        {
            foreach (Func<UniTask> uniTask in Tasks)
            {
                await uniTask.Invoke();
            }
            TaskHelper.sequencePool.Release(this);
        }
    }

    // 并行执行构建器
    public class ParallelBuilder : Builder
    {
        public override async UniTaskVoid RunAsync()
        {
            await UniTask.WhenAll(Enumerable.Select(Tasks, uniTask => uniTask.Invoke()).ToList());
            TaskHelper.ParallelPool.Release(this);
        }
    }
}