using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using SQLite;

public static class SQLiteConnectionExtensions
{
    public static bool HasColumn(this SQLiteConnection db, string tableName, string columnName)
    {
        var result =
            db.ExecuteScalar<int>(
                @"SELECT COUNT(1) FROM sqlite_master AS m JOIN pragma_table_info(m.name) AS p WHERE m.type = 'table' and m.name = ? and p.name = ?", tableName, columnName);

        return result > 0;
    }

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
