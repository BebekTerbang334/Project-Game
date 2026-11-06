namespace Hutan.Ritual
{
    public interface IInteractable
    {
        string GetPrompt(IncenseInventory inventory);
        string Interact(IncenseInventory inventory);
    }
}
