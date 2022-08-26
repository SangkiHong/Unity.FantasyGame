namespace SK.States
{
    public abstract class State
    {
        public virtual void Enter() { }

        public virtual void FixedTick() { }
        public virtual void Tick() { }


        public virtual void Exit() { }
    }
}
