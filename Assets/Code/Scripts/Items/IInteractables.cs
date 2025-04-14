namespace AF.TS.Items
{ 
    public interface IInteractables
    {
        public void Interact();
    }

    public interface IIAmTarget
    {
        public void TakeDamage(float damage);
    }
}
