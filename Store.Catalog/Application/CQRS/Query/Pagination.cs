using CSharpFunctionalExtensions;

namespace Store.Catalog.Application.CQRS.Query;

internal static class Pagination
{
    private const int MaxPageSize = 100;

    public static Result<(int Skip, int Take)> Normalize(int page, int pageSize)
    {
        if (page < 1)
            return Result.Failure<(int Skip, int Take)>("Page must be greater than zero");
        if (pageSize < 1)
            return Result.Failure<(int Skip, int Take)>("PageSize must be greater than zero");

        var take = Math.Min(pageSize, MaxPageSize);
        var skip = (page - 1) * take;
        return Result.Success((skip, take));
    }
}
