using UnityEngine;

namespace Sangki.Quest
{
    [CreateAssetMenu(menuName = "Quest/Task/Action/PositiveCount", fileName = "Positive Count")]
    public class PositiveCount : TaskAction
    {
        public override int Run(Task task, int currentSucess, int successCount)
        {
            return successCount > 0 ? currentSucess + successCount : currentSucess;
        }
    }
}
