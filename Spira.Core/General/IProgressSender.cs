using System;

namespace Spira.Core
{
    public interface IProgressSender
    {
        event Action<long> ProgressTotalChanged;
        event Action<long> ProgressIncrement;
    }
}