using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using WebApiApp.Contracts;

namespace WebApiApp.Services;

public interface ICatalogPdfExportService
{
    byte[] GeneratePdf(
        IEnumerable<CatalogItemResponse> products,
        string? search,
        string? categoryName,
        decimal? minCost,
        decimal? maxCost);
}

public class CatalogPdfExportService : ICatalogPdfExportService
{
    public byte[] GeneratePdf(
        IEnumerable<CatalogItemResponse> products,
        string? search,
        string? categoryName,
        decimal? minCost,
        decimal? maxCost)
    {
        var productList = products.ToList();

        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(8));

                page.Header().PaddingBottom(15).Column(column =>
                {
                    column.Item().AlignCenter().Text("Каталог товаров").SemiBold().FontSize(20);
                    column.Item().PaddingTop(5).AlignCenter().Text(GetFilterSummary(search, categoryName, minCost, maxCost))
                        .FontSize(8)
                        .FontColor(Colors.Grey.Darken2);
                });

                page.Content().Column(column =>
                {
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(30);
                            columns.RelativeColumn(1.6f);
                            columns.RelativeColumn(1.3f);
                            columns.RelativeColumn(2.2f);
                            columns.RelativeColumn(1.1f);
                            columns.RelativeColumn(1.6f);
                            columns.RelativeColumn(1.8f);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderCellStyle).Text("ID").SemiBold();
                            header.Cell().Element(HeaderCellStyle).Text("Название").SemiBold();
                            header.Cell().Element(HeaderCellStyle).Text("Категория").SemiBold();
                            header.Cell().Element(HeaderCellStyle).Text("Описание").SemiBold();
                            header.Cell().Element(HeaderCellStyle).Text("Цена").SemiBold();
                            header.Cell().Element(HeaderCellStyle).Text("Примечание").SemiBold();
                            header.Cell().Element(HeaderCellStyle).Text("Специальное примечание").SemiBold();
                        });

                        if (productList.Count == 0)
                        {
                            table.Cell().ColumnSpan(7)
                                .Element(EmptyCellStyle)
                                .AlignCenter()
                                .Text("По заданным фильтрам товары не найдены");
                            return;
                        }

                        foreach (var product in productList)
                        {
                            table.Cell().Element(DataCellStyle).Text(product.ProductId.ToString());
                            table.Cell().Element(DataCellStyle).Text(product.ProductName ?? "-");
                            table.Cell().Element(DataCellStyle).Text(product.ProductCathegory ?? "-");
                            table.Cell().Element(DataCellStyle).Text(product.ProductDescription ?? "-");
                            table.Cell().Element(DataCellStyle).Text(product.ProductCost.ToString("C"));
                            table.Cell().Element(DataCellStyle).Text(product.ProductNote ?? "-");
                            table.Cell().Element(DataCellStyle).Text(product.ProductNoteSpecial ?? "-");
                        }
                    });
                });

                page.Footer().AlignRight().PaddingTop(15).Text(x =>
                {
                    x.Span("Дата экспорта: ").SemiBold();
                    x.Span(DateTime.UtcNow.ToString("dd.MM.yyyy HH:mm 'UTC'"));
                });
            });
        });

        return document.GeneratePdf();
    }

    private static string GetFilterSummary(
        string? search,
        string? categoryName,
        decimal? minCost,
        decimal? maxCost)
    {
        var filters = new List<string>();

        if (!string.IsNullOrWhiteSpace(search))
        {
            filters.Add($"поиск: {search}");
        }

        if (!string.IsNullOrWhiteSpace(categoryName))
        {
            filters.Add($"категория: {categoryName}");
        }

        if (minCost.HasValue)
        {
            filters.Add($"мин. цена: {minCost.Value:C}");
        }

        if (maxCost.HasValue)
        {
            filters.Add($"макс. цена: {maxCost.Value:C}");
        }

        return filters.Count > 0
            ? $"Фильтры: {string.Join("; ", filters)}"
            : "Фильтры: без ограничений";
    }

    private static IContainer HeaderCellStyle(IContainer container) =>
        container
            .Background(Colors.Blue.Lighten4)
            .Border(1)
            .BorderColor(Colors.Grey.Medium)
            .Padding(6)
            .AlignCenter();

    private static IContainer DataCellStyle(IContainer container) =>
        container
            .Border(1)
            .BorderColor(Colors.Grey.Lighten2)
            .Padding(5)
            .AlignLeft();

    private static IContainer EmptyCellStyle(IContainer container) =>
        container
            .Border(1)
            .BorderColor(Colors.Grey.Lighten2)
            .Padding(10)
            .Background(Colors.Grey.Lighten3);
}
