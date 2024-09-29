using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Jobs;
using UnityEditor;

namespace net.rs64.TexTransCore
{
    internal class JobResult<T>
    {
        T _result;
        JobHandle _jobHandle;
        Action _completeAction;

        public T GetResult
        {
            get
            {
                _jobHandle.Complete();
                if (_completeAction != null) { _completeAction.Invoke(); _completeAction = null; }
                return _result;
            }
        }
        public T GetResultUnCheck => _result;
        public JobHandle GetHandle => _jobHandle;

        public JobResult(T result) { _result = result; _jobHandle = default; _completeAction = null; }
        public JobResult(T result, JobHandle jobHandle) { _result = result; _jobHandle = jobHandle; _completeAction = null; }
        public JobResult(T result, JobHandle jobHandle, Action completeAction) { _result = result; _jobHandle = jobHandle; _completeAction = completeAction; }
    }
}
