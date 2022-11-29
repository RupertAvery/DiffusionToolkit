using System.Linq.Expressions;
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

    public static bool TableExists<T>(this SQLiteConnection db)
    {
        var tableName = typeof(T).Name;

        var sql = $"SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name=?;";
        var cmd = db.CreateCommand(sql, tableName);
        var result = cmd.ExecuteScalar<int>();

        return result > 0;
    }

}