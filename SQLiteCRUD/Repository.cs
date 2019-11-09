using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;
using Dapper;


namespace SQLiteCRUD
{
    public class Repository
    {
        public string ConnString { get; private set; }

        public Repository(string connString)
        {
            ConnString = connString;
        }

        public int Create<TEntity>(TEntity record, string autoIncrementPK = "Id")
        {
            if (record != null)
            {
                IEnumerable<string> properties = new List<PropertyInfo>(typeof(TEntity).GetProperties()).Select(prop => prop.Name);

                using (IDbConnection conn = new SQLiteConnection(ConnString))
                {
                    string query = $"insert " +
                                   $"into {typeof(TEntity).Name} " +
                                   $"({string.Join(",", properties.Where(prop => prop != autoIncrementPK))}) " +
                                   $"values " +
                                   $"({string.Join(",", properties.Where(prop => prop != autoIncrementPK).Select(prop => $"@{prop}"))})";

                    return conn.Execute(query, record); // Returns number of affected rows
                }
            }
            else
            {
                throw new ArgumentNullException("Please verify input parameters are non null"); 
            }
        }

        public IEnumerable<TEntity> Read<TEntity>()
        { 
            using (IDbConnection conn = new SQLiteConnection(ConnString))
            {
                return conn.Query<TEntity>($"select * from {typeof(TEntity).Name}");
            }
        }

        public int Update<TEntity>(TEntity record, string where = "Id")
        {
            if (!string.IsNullOrWhiteSpace(where))
            {
                return Update<TEntity>(record, new List<string>() { where });
            }
            else
            {
                return Update<TEntity>(record, new List<string>());
            }
        }

        public int Update<TEntity>(TEntity record, List<string> where)
        {
            if (record != null & where != null)
            {
                IEnumerable<string> properties = new List<PropertyInfo>(typeof(TEntity).GetProperties()).Select(prop => prop.Name);

                if (where.All(w => properties.Contains(w)))
                {
                    string whereClause = where.Count() > 0 ? $"where {string.Join(" and ", where.Select(w => $"{w} = @{w}"))}" : string.Empty;

                    using (IDbConnection conn = new SQLiteConnection(ConnString))
                    {
                        string query = $"update {typeof(TEntity).Name} " +
                                       $"set {string.Join(",", properties.Where(prop => !where.Contains(prop)).Select(prop => $"{prop} = @{prop}"))} " +
                                       $"{whereClause}";
                        return conn.Execute(query, record);
                    }
                }
                else
                {
                    throw new ArgumentException("'Where' contains columns which are not in record, please verify input parameters");
                }
            }
            else
            {
                throw new ArgumentNullException("Please verify input parameters are non null");
            }
        }

        public int Delete<TEntity>(TEntity record, string where = "Id")
        {
            if (!string.IsNullOrWhiteSpace(where))
            {
                return Delete<TEntity>(record, new List<string>() { where });
            }
            else
            {
                return Delete<TEntity>(record, new List<string>());
            }
        }

        public int Delete<TEntity>(TEntity record, List<string> where)
        {
            if (record != null & where != null)
            {
                IEnumerable<string> properties = new List<PropertyInfo>(typeof(TEntity).GetProperties()).Select(prop => prop.Name);

                if (where.All(w => properties.Contains(w)))
                {
                    string whereClause = where.Count() > 0 ? $"where ({string.Join(" and ", where.Select(prop => $"{prop} = @{prop}"))})" : string.Empty;

                    using (IDbConnection conn = new SQLiteConnection(ConnString))
                    {
                        string query = $"delete from {typeof(TEntity).Name} {whereClause}";
                        return conn.Execute(query, record);
                    }
                }
                else
                {
                    throw new ArgumentException("'Where' contains columns which are not in record, please verify input parameters");
                }
            }
            else
            {
                throw new ArgumentNullException("Please verify input parameters are non null");
            }
        }

        public int DeleteAll<TEntity>()
        {
            using (IDbConnection conn = new SQLiteConnection(ConnString))
            {
                return conn.Execute($"delete from {typeof(TEntity).Name}");
            }
        }
    }
}
