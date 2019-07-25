using SimpleQuery.Domain.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SimpleQuery.Data.Linq
{
    /// <summary>
    /// 
    /// </summary>
    public class ExpressionQueryTranslator : ExpressionVisitor
    {
        private DbServerType _dbServer;
        private StringBuilder sb;
        private string _orderBy = string.Empty;
        private int? _skip = null;
        private int? _take = null;
        private string _whereClause = string.Empty;
        public Dictionary<DbServerType, Tuple<string, string>> CharacterDialect { get; set; } =
            new Dictionary<DbServerType, Tuple<string, string>>
            {
                { DbServerType.Hana, new Tuple<string, string>("\"", "\"") },
                { DbServerType.FbAdapter, new Tuple<string, string>("\"", "\"") },
                { DbServerType.Oracle, new Tuple<string, string>("\"", "\"") },
                { DbServerType.PostGres, new Tuple<string, string>("\"", "\"") },
                { DbServerType.Sqlite, new Tuple<string, string>("[", "]") },
                { DbServerType.SqlServer, new Tuple<string, string>("[", "]") },
                { DbServerType.MySql, new Tuple<string, string>("`", "`") }
            };

        public int? Skip
        {
            get
            {
                return _skip;
            }
        }

        public int? Take
        {
            get
            {
                return _take;
            }
        }

        public string OrderBy
        {
            get
            {
                return _orderBy;
            }
        }

        public string WhereClause
        {
            get
            {
                return _whereClause;
            }
        }

        public ExpressionQueryTranslator()
        {
        }

        public string Translate(Expression expression, DbServerType dbServerType)
        {
            _dbServer = dbServerType;
            this.sb = new StringBuilder();
            this.Visit(expression);
            _whereClause = this.sb.ToString();
            return _whereClause;
        }

        private static Expression StripQuotes(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
            {
                e = ((UnaryExpression)e).Operand;
            }
            return e;
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "Where")
            {
                this.Visit(m.Arguments[0]);
                LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                this.Visit(lambda.Body);
                return m;
            }
            else if (m.Method.Name == "Take")
            {
                if (this.ParseTakeExpression(m))
                {
                    Expression nextExpression = m.Arguments[0];
                    return this.Visit(nextExpression);
                }
            }
            else if (m.Method.Name == "Skip")
            {
                if (this.ParseSkipExpression(m))
                {
                    Expression nextExpression = m.Arguments[0];
                    return this.Visit(nextExpression);
                }
            }
            else if (m.Method.Name == "OrderBy")
            {
                if (this.ParseOrderByExpression(m, "ASC"))
                {
                    Expression nextExpression = m.Arguments[0];
                    return this.Visit(nextExpression);
                }
            }
            else if (m.Method.Name == "OrderByDescending")
            {
                if (this.ParseOrderByExpression(m, "DESC"))
                {
                    Expression nextExpression = m.Arguments[0];
                    return this.Visit(nextExpression);
                }
            }

            throw new NotSupportedException(string.Format("The method '{0}' is not supported", m.Method.Name));
        }


        protected override Expression VisitUnary(UnaryExpression u)
        {
            switch (u.NodeType)
            {
                case ExpressionType.Not:
                    sb.Append(" NOT ");
                    this.Visit(u.Operand);
                    break;
                case ExpressionType.Convert:
                    this.Visit(u.Operand);
                    break;
                default:
                    throw new NotSupportedException(string.Format("The unary operator '{0}' is not supported", u.NodeType));
            }
            return u;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        protected override Expression VisitBinary(BinaryExpression b)
        {
            sb.Append("(");
            this.Visit(b.Left);

            switch (b.NodeType)
            {
                case ExpressionType.And:
                    sb.Append(" AND ");
                    break;

                case ExpressionType.AndAlso:
                    sb.Append(" AND ");
                    break;

                case ExpressionType.Or:
                    sb.Append(" OR ");
                    break;

                case ExpressionType.OrElse:
                    sb.Append(" OR ");
                    break;

                case ExpressionType.Equal:
                    if (IsNullConstant(b.Right))
                    {
                        sb.Append(" IS ");
                    }
                    else
                    {
                        sb.Append(" = ");
                    }
                    break;

                case ExpressionType.NotEqual:
                    if (IsNullConstant(b.Right))
                    {
                        sb.Append(" IS NOT ");
                    }
                    else
                    {
                        sb.Append(" <> ");
                    }
                    break;

                case ExpressionType.LessThan:
                    sb.Append(" < ");
                    break;

                case ExpressionType.LessThanOrEqual:
                    sb.Append(" <= ");
                    break;

                case ExpressionType.GreaterThan:
                    sb.Append(" > ");
                    break;

                case ExpressionType.GreaterThanOrEqual:
                    sb.Append(" >= ");
                    break;

                default:
                    throw new NotSupportedException(string.Format("The binary operator '{0}' is not supported", b.NodeType));

            }

            this.Visit(b.Right);
            sb.Append(")");
            return b;
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            IQueryable q = c.Value as IQueryable;

            if (q == null && c.Value == null)
            {
                sb.Append("NULL");
            }
            else if (q == null)
            {
                switch (Type.GetTypeCode(c.Value.GetType()))
                {
                    case TypeCode.Boolean:
                        if (this._dbServer == DbServerType.Hana || _dbServer == DbServerType.PostGres)
                            sb.Append(((bool)c.Value) ? "true" : "false");
                        else
                            sb.Append(((bool)c.Value) ? 1 : 0);
                        break;

                    case TypeCode.String:
                        sb.Append("'");
                        sb.Append(c.Value);
                        sb.Append("'");
                        break;

                    case TypeCode.DateTime:
                        sb.Append("'");
                        sb.Append(c.Value);
                        sb.Append("'");
                        break;

                    case TypeCode.Object:
                        throw new NotSupportedException(string.Format("The constant for '{0}' is not supported", c.Value));

                    default:
                        sb.Append(c.Value);
                        break;
                }
            }

            return c;
        }


        public object Evaluate(Expression e)
        {
            switch (e.NodeType)
            {
                case ExpressionType.Constant:
                    return (e as ConstantExpression).Value;
                case ExpressionType.MemberAccess:
                    {
                        var propertyExpression = e as MemberExpression;
                        var field = propertyExpression.Member as FieldInfo;
                        var property = propertyExpression.Member as PropertyInfo;
                        var container = propertyExpression.Expression == null ? null : Evaluate(propertyExpression.Expression);
                        if (field != null)
                            return field.GetValue(container);
                        else if (property != null)
                            return property.GetValue(container, null);
                        else
                            return null;
                    }
                default:
                    return null;
            }
        }
        protected override Expression VisitMember(MemberExpression m)
        {
            if (m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter)
            {
                var prop = m.Member as System.Reflection.PropertyInfo;
                if (prop != null && prop.PropertyType.Name == "Boolean" && m.Expression.ToString().Length == 1)
                {
                    sb.Append($"{CharacterDialect[this._dbServer].Item1}{m.Member.Name}{CharacterDialect[_dbServer].Item2}");
                }
                else
                {
                    sb.Append($"{CharacterDialect[this._dbServer].Item1}{m.Member.Name}{CharacterDialect[_dbServer].Item2}");
                }
                return m;
            }
            else if (m.Expression.NodeType == ExpressionType.MemberAccess)
            {
                var prop = m.Member as System.Reflection.PropertyInfo;
                var value = Evaluate(m);
                if (prop.PropertyType.Name == "Int32")
                    sb.Append(value);
                else if (prop.PropertyType.Name == "String")
                    sb.Append($"'{value}'");
                else
                    sb.Append(value);

                return m;
            }
            else if (m.Expression.NodeType == ExpressionType.Constant)
            {
                var prop = m.Member as System.Reflection.FieldInfo;
                var value = Evaluate(m);
                if (prop.FieldType.Name == "Int32")
                    sb.Append(value);
                else if (prop.FieldType.Name == "String")
                    sb.Append($"'{value}'");
                else if (prop.FieldType.Name == "DateTime")
                {
                    if (value == null)
                        sb.Append($"null");
                    else
                        sb.Append($"'{Convert.ToDateTime(value).ToString("yyyy-MM-dd HH:mm:ss")}'");
                }
                else
                    sb.Append(value);

                return m;
            }
            else
                throw new NotSupportedException(string.Format("The member '{0}' is not supported", m.Member.Name));
        }

        protected bool IsNullConstant(Expression exp)
        {
            return (exp.NodeType == ExpressionType.Constant && ((ConstantExpression)exp).Value == null);
        }

        private bool ParseOrderByExpression(MethodCallExpression expression, string order)
        {
            UnaryExpression unary = (UnaryExpression)expression.Arguments[1];
            LambdaExpression lambdaExpression = (LambdaExpression)unary.Operand;


            MemberExpression body = lambdaExpression.Body as MemberExpression;
            if (body != null)
            {
                if (string.IsNullOrEmpty(_orderBy))
                {
                    _orderBy = string.Format("{0} {1}", body.Member.Name, order);
                }
                else
                {
                    _orderBy = string.Format("{0}, {1} {2}", _orderBy, body.Member.Name, order);
                }

                return true;
            }

            return false;
        }

        private bool ParseTakeExpression(MethodCallExpression expression)
        {
            ConstantExpression sizeExpression = (ConstantExpression)expression.Arguments[1];

            int size;
            if (int.TryParse(sizeExpression.Value.ToString(), out size))
            {
                _take = size;
                return true;
            }

            return false;
        }

        private bool ParseSkipExpression(MethodCallExpression expression)
        {
            ConstantExpression sizeExpression = (ConstantExpression)expression.Arguments[1];

            int size;
            if (int.TryParse(sizeExpression.Value.ToString(), out size))
            {
                _skip = size;
                return true;
            }

            return false;
        }

        internal static string GetWhereCommand<T>(Expression<Func<T, bool>> expression, DbServerType dbServerType) where T : class, new()
        {
            var queryTrans = new ExpressionQueryTranslator();
            var where = $"where { queryTrans.Translate(expression, dbServerType)}";

            return where;
        }
    }
}
