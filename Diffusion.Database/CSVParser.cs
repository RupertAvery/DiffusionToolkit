namespace Diffusion.Database;

public class CSVParser
{
    public static ICollection<string> Parse(string text)
    {
        var result = new List<string>();

        var span = text.AsSpan();
        int i = 0;
        var state = 0;
        var start = 0;
        while (i < span.Length)
        {
            switch (span[i])
            {
                case ',':
                    if (state == 0)
                    {
                        result.Add(span.Slice(start, i - start).ToString().Replace("\"\"","\""));
                        start = i + 1;
                    }
                    else if (state == 2)
                    {
                        result.Add(span.Slice(start, i - start - 1).ToString().Replace("\"\"", "\""));
                        start = i + 1;
                        state = 0;
                    }
                    break;
                case '"':
                    if (state == 0)
                    {
                        state = 1;
                        start = i + 1;
                    }
                    else if(state == 1)
                    {
                        state = 2;
                    }
                    break;
            }

            i++;
        }

        if (start < i)
        {
            if (state == 0 || state == 1)
            {
                result.Add(span.Slice(start, i - start).ToString().Replace("\"\"", "\""));
            }
            else if (state == 2)
            {
                result.Add(span.Slice(start, i - start - 1).ToString().Replace("\"\"", "\""));
            }
        }

        return result;
    }
}