using Diffusion.Database;
using Microsoft.Extensions.Options;

namespace Diffusion.Toolkit;

/// <summary>
/// A container class used to inject into other classes so that when the datastore changes, they use the value reference to access the updated value
/// </summary>
public class DataStoreOptions : IOptions<DataStore>
{
    public DataStoreOptions(DataStore value)
    {
        Value = value;
    }

    public void UpdateValue(DataStore value)
    {
        Value = value;
    }

    public DataStore Value { get; private set; }
}