using System;
using System.Collections.Generic;
using System.Linq;

namespace Base
{
    public class DependencyGraph<T>
    {
        private readonly Dictionary<Type, HashSet<Type>> _dependencies = new();
        private readonly Dictionary<Type, HashSet<Type>> _reverseDependencies = new();

        public void AddNode(T module)
        {
            var type = module.GetType();
            if (!_dependencies.ContainsKey(type))
            {
                _dependencies[type] = new HashSet<Type>();
                _reverseDependencies[type] = new HashSet<Type>();
            }
        }

        public void AddDependency(T dependent, Type dependencyType)
        {
            var dependentType = dependent.GetType();
            
            // 添加正向依赖
            if (!_dependencies.ContainsKey(dependentType))
                _dependencies[dependentType] = new HashSet<Type>();
            
            _dependencies[dependentType].Add(dependencyType);

            // 添加反向依赖
            if (!_reverseDependencies.ContainsKey(dependencyType))
                _reverseDependencies[dependencyType] = new HashSet<Type>();
            
            _reverseDependencies[dependencyType].Add(dependentType);
        }

        public void RemoveNode(Type type)
        {
            // 移除正向依赖
            if (_dependencies.TryGetValue(type, out var dependencies))
            {
                foreach (var depType in dependencies)
                {
                    if (_reverseDependencies.TryGetValue(depType, out var reverseDeps))
                    {
                        reverseDeps.Remove(type);
                    }
                }
                _dependencies.Remove(type);
            }

            // 移除反向依赖
            if (_reverseDependencies.TryGetValue(type, out var reverseDependents))
            {
                foreach (var depType in reverseDependents)
                {
                    if (_dependencies.TryGetValue(depType, out var deps))
                    {
                        deps.Remove(type);
                    }
                }
                _reverseDependencies.Remove(type);
            }
        }

        public List<Type> GetExecutionOrder()
        {
            var order = new List<Type>();
            var inDegree = new Dictionary<Type, int>();
            var queue = new Queue<Type>();

            // 初始化入度
            foreach (var node in _dependencies)
            {
                inDegree[node.Key] = 0;
            }

            // 计算初始入度
            foreach (var dependencies in _dependencies.Values)
            {
                foreach (var dep in dependencies)
                {
                    inDegree[dep]++;
                }
            }

            // 找到初始入度为0的节点
            foreach (var node in inDegree.Where(x => x.Value == 0))
            {
                queue.Enqueue(node.Key);
            }

            // 拓扑排序
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                order.Add(current);

                if (_reverseDependencies.TryGetValue(current, out var dependents))
                {
                    foreach (var dependent in dependents)
                    {
                        if (--inDegree[dependent] == 0)
                        {
                            queue.Enqueue(dependent);
                        }
                    }
                }
            }

            return order.Count == _dependencies.Count ? order : null;
        }

        public bool HasCircularDependency()
        {
            return GetExecutionOrder() == null;
        }
    }
}
