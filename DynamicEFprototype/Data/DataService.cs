using System.ComponentModel;
using System.Linq.Expressions;
using DynamicEFprototype.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace DynamicEFprototype.Data;

public class DataService
{
    private readonly ProtoContext _context;

    public DataService(ProtoContext context)
    {
        _context = context;
    }

    private UnaryExpression GetConvertedValue<T>(string value)
    {
        var converter = TypeDescriptor.GetConverter(typeof(T));
        if (!converter.CanConvertFrom(typeof(string))) // 2
            throw new NotSupportedException();

        var propertyValue = converter.ConvertFromInvariantString(value);
        var constant = Expression.Constant(propertyValue);
        var valueExpression = Expression.Convert(constant, typeof(T));

        return valueExpression;
    }

    private Expression<Func<Product, bool>> GenerateFilterLambda(DynamicSearchRequest filter)
    {
        var lender = Expression.Parameter(typeof(Product), "product");
        var column = Expression.Property(lender, filter.Column);
        var filterValue = Expression.Constant(filter.SearchValue);

        Enum.TryParse(filter.Type, out FilterTypesEnum filterType);
        if (!Enum.IsDefined(typeof(FilterTypesEnum), filterType))
            throw new Exception($"{filter.Type} is not a supported filter type");

        UnaryExpression valueExpression = filterType switch
        {
            FilterTypesEnum.String => GetConvertedValue<string>(filter.SearchValue),
            FilterTypesEnum.Guid => GetConvertedValue<Guid>(filter.SearchValue),
            FilterTypesEnum.Int => GetConvertedValue<int>(filter.SearchValue),
            FilterTypesEnum.DateTime => GetConvertedValue<DateTime>(filter.SearchValue),
            FilterTypesEnum.Boolean => GetConvertedValue<bool>(filter.SearchValue),
            _ => throw new Exception($"{filter.Type} is not a supported filter type")
        };

        Enum.TryParse(filter.ComparisonType, out FilterComparisonTypeEnum comparerType);
        if (!Enum.IsDefined(typeof(FilterComparisonTypeEnum), comparerType))
            throw new Exception($"{filter.ComparisonType} is not a supported comparison type");

        BinaryExpression comparer;
        switch (comparerType)
        {
            case FilterComparisonTypeEnum.Equals:
                comparer = Expression.Equal(column, valueExpression);
                break;
            case FilterComparisonTypeEnum.IsGreater:
                comparer = Expression.GreaterThan(column, valueExpression);
                break;
            case FilterComparisonTypeEnum.IsGreaterOrEqual:
                comparer = Expression.GreaterThanOrEqual(column, valueExpression);
                break;
            case FilterComparisonTypeEnum.IsLess:
                comparer = Expression.LessThan(column, valueExpression);
                break;
            case FilterComparisonTypeEnum.IsLessOrEqual:
                comparer = Expression.LessThanOrEqual(column, valueExpression);
                break;
            case FilterComparisonTypeEnum.Contains:
                throw new Exception($"{filter.ComparisonType} not supported as comparison type");
                break;
            case FilterComparisonTypeEnum.ContainsArray:
                throw new Exception($"{filter.ComparisonType} not supported as comparison type");
                break;
            default: throw new Exception($"{filter.ComparisonType} not supported as comparison type");
        }

        return Expression.Lambda<Func<Product, bool>>(comparer, lender);

    }

    public async Task<List<Product>> ProductDynamicSearchAsync(List<DynamicSearchRequest> filters)
    {
        var products = _context.Products.AsQueryable();

        foreach (var filter in filters)
        {
            var lamdaFilter = GenerateFilterLambda(filter);
            products = products.Where(lamdaFilter);
        }

        return await products.ToListAsync();
    }
}