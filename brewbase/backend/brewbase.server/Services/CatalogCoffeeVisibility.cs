using brewbase.server.Models;
using Microsoft.EntityFrameworkCore;

namespace brewbase.server.Services;

internal static class CatalogCoffeeVisibility
{
    internal const string ApprovedStatus = "Approved";
    internal const string RemovedStatus = "Removed";
    internal const string CoffeeModule = "coffee";

    internal static IQueryable<Coffee> WhereVisibleInCatalog(
        this IQueryable<Coffee> coffees,
        BrewDbContext context) =>
        coffees.Where(coffee =>
            !context.Articles.Any(article =>
                article.CoffeeId == coffee.Id
                && article.Module == CoffeeModule
                && article.Status == RemovedStatus)
            || context.Articles.Any(article =>
                article.CoffeeId == coffee.Id
                && article.Module == CoffeeModule
                && article.Status == ApprovedStatus));
}
