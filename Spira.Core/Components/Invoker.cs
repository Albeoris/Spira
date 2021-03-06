﻿using System;
using System.Collections;
using System.Xml;

namespace Spira.Core
{
    public static class Invoker
    {
        public static TResult SafeInvoke<TResult, TArg1>(Func<TArg1, TResult> func, TArg1 arg1, TResult defaultResult = null) where TResult : class
        {
            try
            {
                return func(arg1) ?? defaultResult;
            }
            catch
            {
                return defaultResult;
            }
        }
    }
}