using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;
using LinqKit;

namespace SmartFleet.Web.Framework.DataTables
{
    public class DataTablesLinqQueryBulider
    {
        public static DataTablesQueryModel ParseRequest(HttpRequestBase request)
        {
            var form = request.Form;
            // Retrieve request data
            var draw = int.Parse(form["draw"]);
            var start = int.Parse(form["start"]);
            var length = int.Parse(form["length"]);
            var search = new DataTablesSearchModel()
            {
                Value = form["search[value]"],
                Regx = Convert.ToBoolean(form["search[regex]"])
            };
            var o = 0;
            var orders = new List<DataTablesOrderModel>();
            while (!string.IsNullOrEmpty(form["order[" + o + "][column]"]))
            {
                orders.Add(new DataTablesOrderModel
                {
                    Column = Convert.ToInt32(form["order[" + o + "][column]"]),
                    Dir = form["order[" + o + "][dir]"]
                });
                o++;
            }
            var c = 0;
            var columns = new List<DataTablesColumnModel>();
            while (!string.IsNullOrEmpty(form["columns[" + c + "][data]"]))
            {
                columns.Add(new DataTablesColumnModel
                {
                    Data = form["columns[" + c + "][data]"],
                    Name = form["columns[" + c + "][name]"],
                    Orderable = Convert.ToBoolean(form["columns[" + c + "][orderable]"]),
                    Searchable = Convert.ToBoolean(form["columns[" + c + "][searchable]"]),
                    Search = new DataTablesSearchModel()
                    {
                        Value = form["columns[" + c + "][search][value]"],
                        Regx = Convert.ToBoolean(form["columns[" + c + "][search][regex]"])
                    }
                });
                c++;
            }
            if (!columns.Any()) throw new Exception("the names of columns should not be null");
            var result = new DataTablesQueryModel()
            {
                Draw = draw,
                Start = start,
                Length = length,
                Search = search,
                Orders = orders,
                Columns = columns
            };
            return result;
        }
        public DataTablesResultModel<T> BuildQuery<T>(HttpRequestBase request, IQueryable<T> queryable)
        {


            var dtParams = ParseRequest(request);
            var result = new DataTablesResultModel<T>();
            result.recordsTotal = queryable.Count();
            // search query
            var predicate = PredicateBuilder.New<T>();
            bool searchHasValue = !string.IsNullOrEmpty(dtParams.Search.Value);
            bool clHasValue = false;

            #region the where clause part

            foreach (var p in dtParams.Columns)
            {
                if (!p.Searchable || string.IsNullOrEmpty(p.Search.Value)) continue;
                GetValue(p, predicate, p.Search.Value);

                clHasValue = true;
            }
            foreach (var p in dtParams.Columns)
            {
                if (!string.IsNullOrEmpty(dtParams.Search.Value))
                    GetValue(p, predicate, dtParams.Search.Value);
            }
            if (searchHasValue || clHasValue)
                queryable = queryable.Where(predicate);

            #endregion

            #region the order clause part

            foreach (var p in dtParams.Orders)
            {
                var c = dtParams.Columns[p.Column];
                if (!c.Orderable) continue;
                PropertyInfo pi = typeof(T).GetProperty(c.Data);
                if (pi == null) continue;
                switch (pi.PropertyType.FullName)
                {
                    case "System.String":
                        {
                            queryable = p.Dir == "asc"
                                ? queryable.OrderBy(OrderByExpression<T, string>(c.Data))
                                : queryable.AsQueryable().OrderByDescending(OrderByExpression<T, string>(c.Data));
                        }
                        break;
                    case "System.DateTime":
                        {
                            queryable = p.Dir == "asc"
                                ? queryable.OrderBy(OrderByExpression<T, DateTime>(c.Data))
                                : queryable.AsQueryable().OrderByDescending(OrderByExpression<T, DateTime>(c.Data));
                        }
                        break;
                    case "System.Nullable<DateTime>":
                        {
                            queryable = p.Dir == "asc"
                                ? queryable.OrderBy(OrderByExpression<T, DateTime?>(c.Data))
                                : queryable.AsQueryable()
                                    .OrderByDescending(OrderByExpression<T, DateTime?>(c.Data));
                        }
                        break;
                    case "System.Int32":
                        {
                            queryable = p.Dir == "asc"
                                ? queryable.OrderBy(OrderByExpression<T, Int32>(c.Data))
                                : queryable.AsQueryable().OrderByDescending(OrderByExpression<T, Int32>(c.Data));
                        }
                        break;
                    case "System.Nullable<Int32>":
                        {
                            queryable = p.Dir == "asc"
                                ? queryable.OrderBy(keySelector: OrderByExpression<T, int?>(c.Data))
                                : queryable.AsQueryable()
                                    .OrderByDescending(OrderByExpression<T, int?>(c.Data));
                        }
                        break;
                    case "System.Int64":
                        {
                            queryable = p.Dir == "asc"
                                ? queryable.OrderBy(OrderByExpression<T, Int64>(c.Data))
                                : queryable.AsQueryable().OrderByDescending(OrderByExpression<T, Int64>(c.Data));
                        }
                        break;
                    case "System.Nullable<Int64>":
                        {
                            queryable = p.Dir == "asc"
                                ? queryable.OrderBy(keySelector: OrderByExpression<T, long?>(c.Data))
                                : queryable.AsQueryable()
                                    .OrderByDescending(OrderByExpression<T, long?>(c.Data));
                        }
                        break;

                    case "System.Decimal":
                        {
                            queryable = p.Dir == "asc"
                                ? queryable.OrderBy(OrderByExpression<T, Decimal>(c.Data))
                                : queryable.AsQueryable().OrderByDescending(OrderByExpression<T, Decimal>(c.Data));
                        }
                        break;
                    case "System.Nullable<Decimal>":
                        {
                            queryable = p.Dir == "asc"
                                ? queryable.OrderBy(OrderByExpression<T, decimal?>(c.Data))
                                : queryable.AsQueryable().OrderByDescending(OrderByExpression<T, Decimal?>(c.Data));
                        }
                        break;
                    case "System.Double":
                        {
                            queryable = p.Dir == "asc"
                                ? queryable.OrderBy(OrderByExpression<T, Double>(c.Data))
                                : queryable.AsQueryable().OrderByDescending(OrderByExpression<T, Double>(c.Data));
                        }
                        break;
                    case "System.Nullable<Double>":
                        {
                            queryable = p.Dir == "asc"
                                ? queryable.OrderBy(OrderByExpression<T, double?>(c.Data))
                                : queryable.AsQueryable().OrderByDescending(OrderByExpression<T, Double?>(c.Data));
                        }
                        break;
                    case "System.Nullable<Guid>":
                        {
                            queryable = p.Dir == "asc"
                                ? queryable.OrderBy(OrderByExpression<T, Guid?>(c.Data))
                                : queryable.AsQueryable().OrderByDescending(OrderByExpression<T, Guid?>(c.Data));
                        }
                        break;
                    case "System.Guid":
                        {
                            queryable = p.Dir == "asc"
                                ? queryable.OrderBy(OrderByExpression<T, Guid>(c.Data))
                                : queryable.AsQueryable().OrderByDescending(OrderByExpression<T, Guid>(c.Data));
                        }
                        break;
                }
            }

            #endregion

            result.draw = dtParams.Draw;
            result.length = dtParams.Length;
            result.data = queryable.AsQueryable().Skip((result.draw-1)*result.length).Take(result.length);
            result.recordsFiltered = queryable.Count();
            return result;
        }

        public  IQueryable<T> FilterIQueryable<T>(HttpRequestBase request, IQueryable<T> queryable)
        {


            var dtParams = ParseRequest(request);
        //    var result = new DataTablesResultModel<T>();
          //  result.recordsTotal = queryable.Count();
            // search query
            var predicate = PredicateBuilder.New<T>();
            bool searchHasValue = !string.IsNullOrEmpty(dtParams.Search.Value);
            bool clHasValue = false;

            #region the where clause part

            foreach (var p in dtParams.Columns)
            {
                if (!p.Searchable || string.IsNullOrEmpty(p.Search.Value)) continue;
                GetValue(p, predicate, p.Search.Value);

                clHasValue = true;
            }
            foreach (var p in dtParams.Columns)
            {
                if (!string.IsNullOrEmpty(dtParams.Search.Value))
                    GetValue(p, predicate, dtParams.Search.Value);
            }
            if (searchHasValue || clHasValue)
                queryable = queryable.Where(predicate);

            #endregion

            #region the order clause part

            foreach (var p in dtParams.Orders)
            {
                var c = dtParams.Columns[p.Column];
                if (!c.Orderable) continue;
                PropertyInfo pi = typeof(T).GetProperty(c.Data);
                if (pi == null) continue;
                switch (pi.PropertyType.FullName)
                {
                    case "System.String":
                        {
                            queryable = p.Dir == "asc"
                                ? queryable.OrderBy(OrderByExpression<T, string>(c.Data))
                                : queryable.AsQueryable().OrderByDescending(OrderByExpression<T, string>(c.Data));
                        }
                        break;
                    case "System.DateTime":
                        {
                            queryable = p.Dir == "asc"
                                ? queryable.OrderBy(OrderByExpression<T, DateTime>(c.Data))
                                : queryable.AsQueryable().OrderByDescending(OrderByExpression<T, DateTime>(c.Data));
                        }
                        break;
                    case "System.Nullable<DateTime>":
                        {
                            queryable = p.Dir == "asc"
                                ? queryable.OrderBy(OrderByExpression<T, DateTime?>(c.Data))
                                : queryable.AsQueryable()
                                    .OrderByDescending(OrderByExpression<T, DateTime?>(c.Data));
                        }
                        break;
                    case "System.Int32":
                        {
                            queryable = p.Dir == "asc"
                                ? queryable.OrderBy(OrderByExpression<T, Int32>(c.Data))
                                : queryable.AsQueryable().OrderByDescending(OrderByExpression<T, Int32>(c.Data));
                        }
                        break;
                    case "System.Nullable<Int32>":
                        {
                            queryable = p.Dir == "asc"
                                ? queryable.OrderBy(keySelector: OrderByExpression<T, int?>(c.Data))
                                : queryable.AsQueryable()
                                    .OrderByDescending(OrderByExpression<T, int?>(c.Data));
                        }
                        break;
                    case "System.Int64":
                        {
                            queryable = p.Dir == "asc"
                                ? queryable.OrderBy(OrderByExpression<T, Int64>(c.Data))
                                : queryable.AsQueryable().OrderByDescending(OrderByExpression<T, Int64>(c.Data));
                        }
                        break;
                    case "System.Nullable<Int64>":
                        {
                            queryable = p.Dir == "asc"
                                ? queryable.OrderBy(keySelector: OrderByExpression<T, long?>(c.Data))
                                : queryable.AsQueryable()
                                    .OrderByDescending(OrderByExpression<T, long?>(c.Data));
                        }
                        break;

                    case "System.Decimal":
                        {
                            queryable = p.Dir == "asc"
                                ? queryable.OrderBy(OrderByExpression<T, Decimal>(c.Data))
                                : queryable.AsQueryable().OrderByDescending(OrderByExpression<T, Decimal>(c.Data));
                        }
                        break;
                    case "System.Nullable<Decimal>":
                        {
                            queryable = p.Dir == "asc"
                                ? queryable.OrderBy(OrderByExpression<T, decimal?>(c.Data))
                                : queryable.AsQueryable().OrderByDescending(OrderByExpression<T, Decimal?>(c.Data));
                        }
                        break;
                    case "System.Double":
                        {
                            queryable = p.Dir == "asc"
                                ? queryable.OrderBy(OrderByExpression<T, Double>(c.Data))
                                : queryable.AsQueryable().OrderByDescending(OrderByExpression<T, Double>(c.Data));
                        }
                        break;
                    case "System.Nullable<Double>":
                        {
                            queryable = p.Dir == "asc"
                                ? queryable.OrderBy(OrderByExpression<T, double?>(c.Data))
                                : queryable.AsQueryable().OrderByDescending(OrderByExpression<T, Double?>(c.Data));
                        }
                        break;
                    case "System.Nullable<Guid>":
                        {
                            queryable = p.Dir == "asc"
                                ? queryable.OrderBy(OrderByExpression<T, Guid?>(c.Data))
                                : queryable.AsQueryable().OrderByDescending(OrderByExpression<T, Guid?>(c.Data));
                        }
                        break;
                    case "System.Guid":
                        {
                            queryable = p.Dir == "asc"
                                ? queryable.OrderBy(OrderByExpression<T, Guid>(c.Data))
                                : queryable.AsQueryable().OrderByDescending(OrderByExpression<T, Guid>(c.Data));
                        }
                        break;
                }
            }

            #endregion

           
            return queryable.AsQueryable().Skip(dtParams.Start).Take(dtParams.Length);
        }


        public Expression<Func<T, bool>> ContainsExpression<T>(string term, string property)
        {
            PropertyInfo pi = typeof(T).GetProperty(property);
            ParameterExpression lhsParam = Expression.Parameter(typeof(T));
            if (pi == null) return null;
            Expression lhs = Expression.Property(lhsParam, pi );

            var rf = Expression.Constant(term);
            var tt = Expression.Call(lhs, "ToString", null);
            var bn2 = Expression.Call(tt, "Contains", null, rf);
            var lambda = Expression.Lambda<Func<T, bool>>(bn2, lhsParam);
            return lambda;
        }

        public Expression<Func<T, bool>> OperationsExpression<T>(string term, string property, ExpressionType operation)
        {
            throw new NotImplementedException();
        }

        public Expression<Func<TSource, TTargetKey>> OrderByExpression<TSource, TTargetKey>(string property)
        {
            PropertyInfo pi = typeof(TSource).GetProperty(property);
            ParameterExpression lhsParam = Expression.Parameter(typeof(TSource), "o");
            Expression orderBy = Expression.Property(lhsParam, pi ?? throw new InvalidOperationException());
            var orderByLambda = Expression.Lambda<Func<TSource, TTargetKey>>(orderBy, lhsParam);
            return orderByLambda;
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="p">The p.</param>
        /// <param name="predicate">The predicate.</param>
        /// <param name="searchValue"></param>
        private void GetValue<T>(DataTablesColumnModel p, ExpressionStarter<T> predicate, string searchValue)
        {
            // PropertyInfo pi = typeof(T).GetProperty(p.Data);
            var exp = ContainsExpression<T>(searchValue, p.Data);
            if (exp != null)
                predicate.Or(exp);
        }

    }
}