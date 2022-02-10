using UnityEngine;

namespace Sangki.Quest
{
    public abstract class TaskTarget : ScriptableObject
    {
        public abstract object Value { get; }
        public abstract bool IsEqual(object target);
    }
}
