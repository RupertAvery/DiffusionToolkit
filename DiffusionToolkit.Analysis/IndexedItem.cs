using System.Drawing;
using System.Xml.Linq;

namespace Diffusion.Analysis
{
    public class IndexedItem<T>
    {
        public int Index { get; set; }
        public T Value { get; set; }

        public IndexedItem(T value, int index)
        {
            this.Index = index;
            this.Value = value;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17; // Initial value, preferably a prime number
                hash = hash * 23 + Index.GetHashCode(); 
                hash = hash * 23 + (Value?.GetHashCode() ?? 0); 
                return hash;
            }
        }
    }
}
