using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using SQLite;

public static class SQLiteConnectionExtensions
{
    public static void DropIndex<T>(this SQLiteConnection db, Expression<Func<T, object>> property) where T : class
    {
        var tableName = typeof(T).Name;

        Expression body = property.Body;

        if (body.NodeType == ExpressionType.Convert)
        {
            body = ((UnaryExpression)body).Operand;
        }


        var columnName = ((MemberExpression)body).Member.Name;

        var cmd = db.CreateCommand("");
        cmd.CommandText = $"DROP INDEX IF EXISTS {tableName}_{columnName}";
        cmd.ExecuteNonQuery();
    }

    public static bool TableExist(this SQLiteConnection db, string name)
    {
        var sql = $"SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name=?;";
        var cmd = db.CreateCommand(sql, name);
        var result = cmd.ExecuteScalar<int>();

        return result > 0;
    }

    //public static void AddRegex(this SQLiteConnection connection)
    //{
    //    connection.CreateFunction(
    //        "regexp",
    //        (string pattern, string input)
    //            => Regex.IsMatch(input, pattern));
    //}

}
