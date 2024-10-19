using System;
using System.Collections.Generic;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    internal interface IActivenessChanger
    {
        bool IsActive { get; }
    }
}
