﻿using System;
using System.Collections;
using Base;
using UnityEngine;

namespace ExceptionCaptureModule
{
    public static class ExceptionMonoBehaviourExtensions
    {
        public static void SafeStartCoroutine(this MonoBehaviour behaviour, IEnumerator coroutine)
        {
            behaviour.StartCoroutine(WrapCoroutine(coroutine));
        }

        private static IEnumerator WrapCoroutine(IEnumerator coroutine)
        {
            while (true)
            {
                object current;
                try
                {
                    if(!coroutine.MoveNext())
                    {
                        break;
                    }
                    current = coroutine.Current;
                }
                catch (Exception ex)
                {
                    GameApp.Instance.GetService<CExceptionSystem>().HandleException(ex, coroutine.ToString());
                    break;
                }
                yield return current;
            }
        }
    }
}