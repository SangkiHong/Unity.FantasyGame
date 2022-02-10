using UnityEngine;

namespace Sangki.Quest
{
    [CreateAssetMenu(menuName = "Quest/Task/Action/SimpleCount", fileName = "Simple Count")]
    public class SimpleCount : TaskAction
    {
        public override int Run(Task task, int currentSucess, int successCount)
        {
            return currentSucess + successCount;
        }
    }
}
