﻿namespace Vostok.SnoopDog.Core.Issues
{
    public interface IIssue
    {
        string Title { get; }
        string Message { get; }
    }
}