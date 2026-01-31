using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PuppeteerSharp;


namespace RelieftManagement.Crawler
{
    public class CrawlerService
    {

        public async Task<List<LocationInfo>> CrawlAsync(string province)
        {
            var result = new List<LocationInfo>();

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
            await page.GoToAsync("https://vnexpress.net/tra-cuu-xa-phuong-sau-sap-nhap-4908879.html", new NavigationOptions
            {
                WaitUntil = new[] { WaitUntilNavigation.Networkidle0 }
            });

            await OpenSearchFormAsync(page);

            var wards = await CrawlWardsByProvinceAsync(page, province);

            foreach (var ward in wards)
            {
                var info = await CrawlAreaAndPopulationAsync(page, province, ward);
                if (info != null)
                    result.Add(info);
            }

            return result;
        }

        public async Task<List<string>> CrawlWardsByProvinceAsync(
    IPage page,
    string provinceName)
        {
            var result = new List<string>();

            // ===== INPUT TỈNH =====
            await page.EvaluateFunctionAsync(@"
        (text) => {
            const input = document.querySelector('#tinh-thanh-moi-input-article');
            input.value = '';
            input.focus();
            input.value = text;
            input.dispatchEvent(new Event('input', { bubbles: true }));
        }
    ", provinceName);

            await Task.Delay(400);

            // chọn tỉnh đầu tiên
            await page.EvaluateFunctionAsync(@"
        () => {
            const first = document.querySelector(
                '#tinh-thanh-moi-suggestions-article ul li a'
            );
            if (first) first.click();
        }
    ");

            // ===== INPUT PHƯỜNG/XÃ (FULL XPATH) =====
            await page.EvaluateFunctionAsync(@"
        () => {
            const input = document
                .evaluate(
                    '/html/body/section[5]/div/div[2]/article/div/div[2]/div[3]/form/div[1]/div[2]/label/input',
                    document,
                    null,
                    XPathResult.FIRST_ORDERED_NODE_TYPE,
                    null
                ).singleNodeValue;

            if (!input) return;

            input.focus();
            input.value = 'a';
            input.dispatchEvent(new Event('input', { bubbles: true }));
            input.value = '';
            input.dispatchEvent(new Event('input', { bubbles: true }));
        }
    ");

            await Task.Delay(500);

            // ===== LẤY PHƯỜNG/XÃ =====
            var wards = await page.EvaluateFunctionAsync<string[]>(@"
        () => {
            const ul = document.querySelector('#phuong-xa-moi-suggestions-article ul');
            if (!ul) return [];
            return Array.from(ul.querySelectorAll('li a'))
                .map(a => a.innerText.trim());
        }
    ");

            foreach (var ward in wards)
                result.Add(ward);

            return result;
        }

        public async Task OpenSearchFormAsync(IPage page)
        {
            // đợi DOM ổn định
            await Task.Delay(2000);

            var clicked = await page.EvaluateFunctionAsync<bool>(@"
        () => {
            const buttons = Array.from(document.querySelectorAll('button, a, div'));
            const btn = buttons.find(b =>
                b.innerText && b.innerText.includes('Tra cứu')
            );
            if (!btn) return false;
            btn.scrollIntoView({ block: 'center' });
            btn.click();
            return true;
        }
    ");

            if (!clicked)
                throw new Exception("Không tìm thấy nút mở form (Tra cứu)");

            await page.WaitForSelectorAsync("#tinh-thanh-moi-input-article");
        }


        public async Task<List<string>> CrawlProvincesAsync(IPage page)

        {
            // trigger dropdown
            await page.EvaluateFunctionAsync(@"
        () => {
            const input = document.querySelector('#tinh-thanh-moi-input-article');
            input.focus();
            input.value = 'a';
            input.dispatchEvent(new Event('input', { bubbles: true }));
            input.value = '';
            input.dispatchEvent(new Event('input', { bubbles: true }));
        }
    ");

            await Task.Delay(400);

            var provinces = await page.EvaluateFunctionAsync<string[]>(@"
        () => {
            const ul = document.querySelector('#tinh-thanh-moi-suggestions-article ul');
            if (!ul) return [];
            return Array.from(ul.querySelectorAll('li a'))
                .map(a => a.innerText.trim());
        }
    ");

            return provinces.ToList();
        }

        public async Task<LocationInfo?> CrawlAreaAndPopulationAsync(
     IPage page,
     string province,
     string ward)
        {
            // ===== INPUT PHƯỜNG/XÃ =====
            await page.EvaluateFunctionAsync(@"
        (text) => {
            const input = document
                .querySelector('#phuong-xa-moi-input-article');
            if (!input) return;

            input.value = '';
            input.focus();
            input.value = text;
            input.dispatchEvent(new Event('input', { bubbles: true }));
        }
    ", ward);

            await Task.Delay(400);

            // click suggestion đầu tiên
            await page.EvaluateFunctionAsync(@"
        () => {
            const first = document.querySelector(
                '#phuong-xa-moi-suggestions-article ul li a'
            );
            if (first) first.click();
        }
    ");

            await Task.Delay(600);

            // ===== ĐỌC DIỆN TÍCH & DÂN SỐ =====
            string areaText = await GetTextByXPathAsync(
                page,
                "/html/body/section[5]/div/div[2]/article/div/div[2]/div[3]/form/div[2]/div[2]/div/div[2]/div[2]/div/div"
            );

            string populationText = await GetTextByXPathAsync(
                page,
                "/html/body/section[5]/div/div[2]/article/div/div[2]/div[3]/form/div[2]/div[2]/div/div[3]/div[2]/div/div"
            );

            if (string.IsNullOrWhiteSpace(areaText) ||
                string.IsNullOrWhiteSpace(populationText))
                return null;

            double area = ParseDouble(areaText);
            double population = ParseDouble(populationText);

            return new LocationInfo
            {
                Province = province,
                Ward = ward,
                Area = area,
                Population = (long)population,
            };
        }


        private async Task<string> GetTextByXPathAsync(IPage page, string xpath)
        {
            return await page.EvaluateFunctionAsync<string>(@"
        (xp) => {
            const node = document
                .evaluate(
                    xp,
                    document,
                    null,
                    XPathResult.FIRST_ORDERED_NODE_TYPE,
                    null
                ).singleNodeValue;

            return node ? node.innerText.trim() : '';
        }
    ", xpath);
        }

        private double ParseDouble(string text)
        {
            var cleaned = text
                .Replace("km²", "")
                .Replace("người", "")
                .Replace(".", "")
                .Replace(",", ".")
                .Trim();

            double.TryParse(
                cleaned,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out double value
            );

            return value;
        }
    }
}
