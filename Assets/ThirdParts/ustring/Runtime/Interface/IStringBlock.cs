using System;
using Yoka.UnityString.Core;

namespace Yoka.UnityString.Interface
{
    public interface IStringBlock : IDisposable
    {
        bool Remove(UString str);
    }
}
