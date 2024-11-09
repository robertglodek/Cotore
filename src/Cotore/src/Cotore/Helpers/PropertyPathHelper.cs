using System.Linq.Expressions;

namespace Cotore.Helpers;

public static class PropertyPathHelper
{
    public static string GetOptionsPropertyPath(string sectionName, string propertyName)
        => $"{sectionName}.{propertyName}".ToLower();

    public static string GetOptionsPropertyPath<T>(Expression<Func<T, object>> propertyExpression)
        => $"{GetFullPropertyPath(propertyExpression)}".ToLower();

    public static string GetOptionsPropertyPath<T>(string sectionName, Expression<Func<T, object>> propertyExpression)
        => $"{sectionName}.{GetFullPropertyPath(propertyExpression)}".ToLower();

    public static string GetFullPropertyPath<T>(Expression<Func<T, object>> propertyExpression)
    {
        if (propertyExpression.Body is not MemberExpression member)
        {
            // Handle UnaryExpression (conversion to object)
            if (propertyExpression.Body is UnaryExpression { Operand: MemberExpression unaryMember })
            {
                member = unaryMember;
            }
            else
            {
                throw new ArgumentException("Invalid expression type. Please provide a member expression.");
            }
        }

        // Build the full property path by traversing the expression tree
        var propertyNames = new Stack<string>();
        while (member != null)
        {
            propertyNames.Push(member.Member.Name);

            if (member.Expression is MemberExpression nextMember)
            {
                member = nextMember;
            }
            else
            {
                break;
            }
        }

        return string.Join(".", propertyNames);
    }
}
