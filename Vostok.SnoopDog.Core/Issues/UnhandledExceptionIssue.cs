﻿using System.Linq;
using Vostok.SnoopDog.Core.Utils;
using Microsoft.Diagnostics.Runtime;

namespace Vostok.SnoopDog.Core.Issues
{
    public class UnhandledExceptionIssue : IIssue
    {
        public UnhandledExceptionIssue(ClrThread clrThread)
        {
            Title = $"Exception in thread {clrThread.ManagedThreadId}";


            var exception = clrThread.CurrentException;
            ExceptionType = exception.Type.Name.EscapeNull();
            Message = $"Unhandled exception of type {ExceptionType} in thread {clrThread.ManagedThreadId}";
            ExceptionMessage = exception.Message.EscapeNull();
            StackTrace = exception.StackTrace
                .Select(frame => frame.ToString().EscapeNull())
                .ToArray();
        }



        public string Title { get; }
        public string Message { get; }
        public string ExceptionType { get; }
        public string ExceptionMessage { get; }
        public string[] StackTrace { get; }
    }
}