using PuppeteerSharp;
using RelieftManagement.Crawler;

class Program
{
    static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Console.WriteLine(" Bắt đầu crawl dữ liệu VNExpress...");

        var locations = new List<LocationInfo>();
        var crawler = new CrawlerService();

        // Puppeteer cần chromium
        await new BrowserFetcher().DownloadAsync();

        using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true,
            Args = new[]
            {
                "--no-sandbox",
                "--disable-setuid-sandbox"
            }
        });

        using var page = await browser.NewPageAsync();
        page.DefaultTimeout = 0;
        page.DefaultNavigationTimeout = 0;

        await page.GoToAsync(
            "https://vnexpress.net/tra-cuu-xa-phuong-sau-sap-nhap-4908879.html",
            new NavigationOptions
            {
                WaitUntil = new[] { WaitUntilNavigation.DOMContentLoaded },
                Timeout = 0
            });

        await crawler.OpenSearchFormAsync(page);

        // ===== CRAWL DANH SÁCH TỈNH =====
        var provinces = await crawler.CrawlProvincesAsync(page);
        Console.WriteLine($" Tìm thấy {provinces.Count} tỉnh");

        foreach (var province in provinces)
        {
            Console.WriteLine($"\n Tỉnh: {province}");

            var wards = await crawler.CrawlWardsByProvinceAsync(page, province);

            foreach (var ward in wards)
            {
                var info = await crawler.CrawlAreaAndPopulationAsync(
                    page,
                    province,
                    ward
                );

                if (info != null)
                {
                    locations.Add(info);
                    Console.WriteLine(info.ToString());
                }
            }
        }

        Console.WriteLine($"\n Hoàn tất! Tổng bản ghi: {locations.Count}");

        // ===== EXPORT EXCEL =====
        var filePath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "Vietnam_Locations.xlsx"
        );

        ExcelExporter.ExportLocations(locations, filePath);

        Console.WriteLine($"📁 Đã xuất Excel: {filePath}");
    }
}

