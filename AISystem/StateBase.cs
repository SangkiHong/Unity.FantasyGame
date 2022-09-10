namespace SK.AISystem
{
    public class StateBase
    {
        public virtual void StateInit() { }
        public virtual void FixedTick() { }
        public virtual void Tick() { }
        public virtual void StateExit() { }
    }
}
