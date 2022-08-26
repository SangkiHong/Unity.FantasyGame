using UnityEngine;

namespace SK.Quest
{
    [CreateAssetMenu(menuName = "Quest/Task/Action/SimpleSet", fileName = "Simple Set")]
    public class SimpleSet : TaskAction
    {
        public override int Run(Task task, int currentSucess, int successCount)
        {
            return successCount;
        }
    }
}
