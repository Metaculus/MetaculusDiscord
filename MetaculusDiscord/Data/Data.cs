using MetaculusDiscord.Model;

namespace MetaculusDiscord.Data;

public class Data
{
    private TransientStorage<BasicMetaculusResponse> _responses;

    public Data()
    {
        _responses = new TransientStorage<BasicMetaculusResponse>();
    }

    public void AddResponse(BasicMetaculusResponse response)
    {
        _responses.Add(response);
    }

    public BasicMetaculusResponse GetResponse(ulong id) => _responses.Get(id);
}
// what does not need to be put into a database
public class TransientStorage<T> where T : IIdentifiable
{
    private Dictionary<ulong, T> _dictionary = new Dictionary<ulong, T>();
    public void Add(T item) => _dictionary.Add(item.Id, item);
    public T Get(ulong id) => _dictionary[id];


}