namespace MiyakoBot.Adapter
{
    public interface IAdapter
    {
        Task RunAsync(CancellationToken cancellationToken);
    }
}
