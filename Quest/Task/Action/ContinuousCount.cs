using UnityEngine;

namespace Sangki.Quest
{
    [CreateAssetMenu(menuName = "Quest/Task/Action/ContinuousCount", fileName = "Continuous Count")]
    public class ContinuousCount : TaskAction
    {
        public override int Run(Task task, int currentSucess, int successCount)
        {
            return successCount > 0 ? currentSucess + successCount : 0;
        }
    }
}