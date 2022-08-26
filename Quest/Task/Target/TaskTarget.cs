using UnityEngine;

namespace SK.Quest
{
    public abstract class TaskTarget : ScriptableObject
    {
        public abstract object Value { get; }
        public abstract bool IsEqual(object target);
    }
}
