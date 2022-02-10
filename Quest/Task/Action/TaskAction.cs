using UnityEngine;

namespace Sangki.Quest
{
    public abstract class TaskAction : ScriptableObject
    {
        public abstract int Run(Task task, int currentSucess, int successCount);
    }
}
