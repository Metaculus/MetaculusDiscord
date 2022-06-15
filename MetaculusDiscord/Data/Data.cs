using MetaculusDiscord.Model;

namespace MetaculusDiscord.Data;

public class Data
{
    private TransientStorage<ResponseLinks> _responses;


    public Data()
    {
        _responses = new TransientStorage<ResponseLinks>();
    }

    public void StoreLinks(ResponseLinks responseLinks)
    {
        _responses.Add(responseLinks);
    }

    public ResponseLinks GetResponse(ulong id)
    {
        return _responses.Get(id);
    }
}

// what does not need to be put into a database
public class TransientStorage<T> where T : IIdentifiable
{
    private readonly Dictionary<ulong, T> _dictionary = new();

    public void Add(T item)
    {
        _dictionary.Add(item.Id, item);
    }

    public T Get(ulong id)
    {
        return _dictionary[id];
    }
}