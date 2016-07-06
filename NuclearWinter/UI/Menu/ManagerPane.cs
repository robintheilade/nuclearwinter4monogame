namespace NuclearWinter.UI
{
    public abstract class ManagerPane<T> : Group where T : IMenuManager
    {
        public T Manager { get; private set; }

        //----------------------------------------------------------------------
        public ManagerPane(T manager)
        : base(manager.MenuScreen)
        {
            Manager = manager;
        }
    }
}
