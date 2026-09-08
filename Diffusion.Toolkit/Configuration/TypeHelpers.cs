using System.Reflection;

namespace Diffusion.Toolkit.Configuration;

public static class TypeHelpers
{
    public static void Copy(object source, object dest)
    {
        var props = source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in props)
        {
            var value = prop.GetValue(source);

            //if (value.GetType().IsClass)
            //{
            //    var newObject = Activator.CreateInstance(value.GetType());
            //    Copy(value, newObject);
            //    prop.SetValue(dest, newObject);
            //}
            //else
            //{
            //    prop.SetValue(dest, value);
            //}
            prop.SetValue(dest, value);
        }
    }
}