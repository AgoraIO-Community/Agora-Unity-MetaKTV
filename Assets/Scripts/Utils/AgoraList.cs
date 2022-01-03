using System.Collections.Generic;
namespace agora.KTV
{
    public class AgoraList<T>
{
    private List<T> _list;
    private int max_size = 0;

    public AgoraList(int max_size = 10)
    {
        this.max_size = max_size;
        _list = new List<T>();
    }

    ~AgoraList()
    {
        Clear();
    }
    
    public void Clear()
    {
        lock (_list)
        {
            _list.Clear();
        }
    }

    public void Add(T info)
    {
        lock (_list)
        {
            if (_list.Count >= max_size)
            {
                _list.RemoveAt(0);
            }
            _list.Add(info);
        }
    }

    public T Get(int index)
    {
        lock (_list)
        {
            if (_list.Count >= index + 1)
            {
                return _list[index];
            }

            return default(T);
        }
    }
}
}