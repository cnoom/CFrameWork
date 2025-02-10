using NUnit.Framework;
using MessageModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SingletonModel;
using UnityEngine;

// 确保此命名空间与您的测试项目匹配
[TestFixture]
public class CMessageHandleCenterTests
{
    private CMessageHandleCenter _messageHandleCenter;

    [SetUp]
    public void Setup()
    {
        // 初始化单例实例
        _messageHandleCenter = CMessageHandleCenter.Instance;
        if (_messageHandleCenter == null)
        {
            // 如果需要手动初始化单例，请确保这是正确的初始化方式
            var instanceField = typeof(SingletonMonoBehaviour<CMessageHandleCenter>).GetField("instance",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            _messageHandleCenter = new GameObject("CMessageHandleCenter").AddComponent<CMessageHandleCenter>();
            instanceField.SetValue(null, _messageHandleCenter);
        }
    }

    [TearDown]
    public void Teardown()
    {
        // 清理操作，比如取消订阅所有事件等
        _messageHandleCenter.Clear();
    }

    [Test]
    public void TestSubscribeAndPublish()
    {
        bool actionCalled = false;
        Action<string> testAction = (message) => { actionCalled = true; Assert.AreEqual("Test", message); };

        // 订阅消息类型 string 的处理程序
        _messageHandleCenter.Subscribe<string>(testAction);

        // 发布消息
        _messageHandleCenter.Publish("Test");

        Assert.IsTrue(actionCalled, "The action should have been called.");
    }

    [Test]
    public async Task TestPublishAsync()
    {
        bool actionCalled = false;
        Action<string> testAction = (message) => { actionCalled = true; Assert.AreEqual("AsyncTest", message); };

        // 订阅消息类型 string 的处理程序
        _messageHandleCenter.Subscribe<string>(testAction);

        // 异步发布消息
        await _messageHandleCenter.PublishAsync("AsyncTest");

        Assert.IsTrue(actionCalled, "The async action should have been called.");
    }

    [Test]
    public void TestUnsubscribe()
    {
        bool actionCalled = false;
        Action<string> testAction = (message) => { actionCalled = true; };
        
        // 订阅消息类型 string 的处理程序
        _messageHandleCenter.Subscribe<string>(testAction);
        
        // 取消订阅
        _messageHandleCenter.UnSubscribe<string>(testAction);
        
        // 发布消息，预期动作不会被调用
        _messageHandleCenter.Publish("Test");
        
        Assert.IsFalse(actionCalled, "The action should not have been called after unsubscribing.");
    }

    [Test]
    public void TestExceptionHandler()
    {
        bool exceptionHandled = false;
        _messageHandleCenter.ExceptionHandler += (type, ex, stackTrace) =>
        {
            exceptionHandled = true;
            Console.WriteLine($"Exception handled for type: {type.Name}, Exception: {ex.Message}");
        };

        Action<string> throwingAction = (message) => { throw new Exception("Test exception"); };
        
        // 订阅消息类型 string 的处理程序
        _messageHandleCenter.Subscribe<string>(throwingAction);
        
        try
        {
            // 发布消息，预期会抛出异常
            _messageHandleCenter.Publish("Test");
        }
        catch (Exception ex)
        {
            // 这里可以添加额外的断言来验证异常是否正确处理
        }

        Assert.IsTrue(exceptionHandled, "The exception handler should have been called.");
    }

    [Test]
    public void TestPriorityOrder()
    {
        List<int> callOrder = new List<int>();

        Action<int> highPriorityAction = (value) => { callOrder.Add(1); };
        Action<int> mediumPriorityAction = (value) => { callOrder.Add(2); };
        Action<int> lowPriorityAction = (value) => { callOrder.Add(3); };

        // 按不同优先级订阅
        _messageHandleCenter.Subscribe<int>(highPriorityAction, priority: 10);
        _messageHandleCenter.Subscribe<int>(mediumPriorityAction, priority: 5);
        _messageHandleCenter.Subscribe<int>(lowPriorityAction, priority: 1);

        // 发布消息
        _messageHandleCenter.Publish(42);

        CollectionAssert.AreEqual(new List<int> { 1, 2, 3 }, callOrder, "Actions should be called in the correct priority order.");
    }
}