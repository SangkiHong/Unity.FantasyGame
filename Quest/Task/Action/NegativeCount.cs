using UnityEngine;

namespace SK.Quest
{
    [CreateAssetMenu(menuName = "Quest/Task/Action/NegativeCount", fileName = "Negative Count")]
    public class NegativeCount : TaskAction
    {
        public override int Run(Task task, int currentSucess, int successCount)
        {
            return successCount < 0 ? currentSucess + successCount : currentSucess;
        }
    }
}