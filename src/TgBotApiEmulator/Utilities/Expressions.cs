using System.Linq.Expressions;

namespace TgBotApiEmulator.Utilities;

public enum ComparisonOperator
{
    Equal,
    NotEqual,
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual
}

public class ExpressionBuilder
{
    public static Expression<Func<T, bool>> CreateDateTimeComparison<T>(
        Expression<Func<T, DateTime?>> propertyAccessor,
        DateTime targetDate,
        ComparisonOperator comparison)
    {
        var parameter = propertyAccessor.Parameters[0];
        var propertyExpression = propertyAccessor.Body;
        if (propertyExpression is UnaryExpression unaryExpression &&
            unaryExpression.NodeType == ExpressionType.Convert)
        {
            propertyExpression = unaryExpression.Operand;
        }

        Expression constant;
        if (propertyExpression.Type == typeof(DateTime?))
        {
            constant = Expression.Constant(targetDate, typeof(DateTime?));
        }
        else
        {
            constant = Expression.Constant(targetDate);
        }

        Expression body = comparison switch
        {
            ComparisonOperator.Equal => Expression.Equal(propertyExpression, constant),
            ComparisonOperator.NotEqual => Expression.NotEqual(propertyExpression, constant),
            ComparisonOperator.GreaterThan => Expression.GreaterThan(propertyExpression, constant),
            ComparisonOperator.GreaterThanOrEqual => Expression.GreaterThanOrEqual(propertyExpression, constant),
            ComparisonOperator.LessThan => Expression.LessThan(propertyExpression, constant),
            ComparisonOperator.LessThanOrEqual => Expression.LessThanOrEqual(propertyExpression, constant),
            _ => throw new ArgumentException("Invalid comparison operator", nameof(comparison))
        };

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    public static Expression<Func<T, bool>> CreateStringContainsComparisonWithNullCheck<T>(
        Expression<Func<T, string?>> propertyAccessor,
        string targetValue,
        StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase)
    {
        var parameter = propertyAccessor.Parameters[0];
        var propertyExpression = propertyAccessor.Body;

        var targetValueConstant = Expression.Constant(targetValue, typeof(string));
        var stringComparisonConstant = Expression.Constant(stringComparison, typeof(StringComparison));

        var containsMethod = typeof(string).GetMethod(
            nameof(string.Contains),
            [typeof(string)]);//, typeof(StringComparison)]);

        if (containsMethod == null)
        {
            throw new InvalidOperationException("Could not find Contains method with StringComparison parameter");
        }

        var containsCall = Expression.Call(
            propertyExpression,
            containsMethod,
            targetValueConstant);//, stringComparisonConstant);

        var propertyNotNull = Expression.NotEqual(propertyExpression, Expression.Constant(null, typeof(string)));
        var andExpression = Expression.AndAlso(propertyNotNull, containsCall);

        return Expression.Lambda<Func<T, bool>>(andExpression, parameter);
    }
}
