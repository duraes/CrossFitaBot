using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using SeleniumExtras.WaitHelpers;
using System.Xml.Linq;
using OpenQA.Selenium.Remote;
using System.Diagnostics;

namespace CrossFitaBot.Core
{
    public static class SchedulerBot
    {
        public class SchedulerBotOptions
        {
            public required string BoxName { get; init; }
            public required string Email { get; init; }
            public required string Senha { get; init; }
            public required string DesiredSlot { get; init; }
            public required int DaysAhead { get; init; }
            public required string DesiredActivity { get; init; }
            public bool HeadlessRun { get; init; } = 
                false;
        }

        public static void Schedule(SchedulerBotOptions options, string? host)
        {
            var desiredDate = DateTime.Now.AddDays(options.DaysAhead);

            var dateToken = desiredDate
                    .AddMonths(-1) // Stupid month offset
                    .ToString("yyyy-M-d");

            var driverOptions = new ChromeOptions();
            if (options.HeadlessRun)
            {
               // driverOptions.AddArgument("--headless=new");
            }

            driverOptions.AddArgument("--ignore-ssl-errors=yes");
            driverOptions.AddArgument("--ignore-certificate-errors");
            driverOptions.AddArgument("--incognito");
            driverOptions.AddArgument("--no-sandbox");
            driverOptions.AddArgument("--disable-dev-shm-usage");

            IWebDriver driver = host is null ? 
                new ChromeDriver(driverOptions) 
                : new RemoteWebDriver(new Uri(host), driverOptions);

            try
            {
                Console.WriteLine("Navegando para página de inicial");
                // Navigate to the specified URL
                driver.Navigate().GoToUrl("https://www.regibox.pt/app/app_nova/login.php");

                // Wait for the search box to be present
                //Thread.Sleep(5000);

                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

                wait.IgnoreExceptionTypes(
                    typeof(OpenQA.Selenium.WebDriverTimeoutException),
                    typeof(OpenQA.Selenium.NoSuchElementException)
                );

                Console.WriteLine("Procurando caixa de busca de Box");
                IWebElement searchBox = wait.Until(driver => driver.FindElement(By.CssSelector("input[type=search]")));

                Console.WriteLine($"Preenchendo caixa de busca de Box: {options.BoxName}");
                // Type "squad" into the search box
                searchBox.SendKeys(options.BoxName);

                // #div.list.search-list.searchbar-found > ul > li:visible
                var boxItem = wait.Until(driver => driver.FindElement(By.CssSelector("li.item-content:not(.hidden-by-searchbar)")));

                Console.WriteLine("Clicando na Box");
                boxItem.Click();

                Console.WriteLine("Procurando caixa de email");
                var emailBox = wait.Until(ExpectedConditions.ElementIsVisible(By.Id("login")));

                Console.WriteLine($"Preenchendo email: {options.Email}");
                emailBox.SendKeys(options.Email);

                Console.WriteLine("Procurando caixa de senha");
                var passwordBox = wait.Until(ExpectedConditions.ElementIsVisible(By.Id("password")));

                Console.WriteLine("Preenchendo senha");
                passwordBox.SendKeys(options.Senha);

                Console.WriteLine("Clicando no botão de login");
                IWebElement loginElement = wait.Until(driver => driver.FindElement(By.XPath("//input[@type='button' or @type='submit'][contains(translate(@value, 'abcdefghijklmnopqrstuvwxyz', 'ABCDEFGHIJKLMNOPQRSTUVWXYZ'), 'LOGIN')]")));
                loginElement.Click();

                var elapsed = Stopwatch.StartNew();

                do
                {
                    Console.WriteLine("Procurando link de aulas");
                    IWebElement aulasLink = wait.Until(driver => driver.FindElement(By.LinkText("AULAS")));

                    Console.WriteLine($"Elapsed: {elapsed.Elapsed}");

                    Console.WriteLine("Clicando no link de aulas");
                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", aulasLink);

                    Console.WriteLine($"Elapsed: {elapsed.Elapsed}");

                    Console.WriteLine($"Procurando link de calendário para o dia {desiredDate.ToShortDateString()}");
                    IWebElement dayTrigger = wait.Until(driver => driver.FindElement(By.CssSelector($"[data-date=\"{dateToken}\"]")));

                    Console.WriteLine($"Elapsed: {elapsed.Elapsed}");

                    dayTrigger = wait.Until(driver => driver.FindElement(By.CssSelector($"[data-date=\"{dateToken}\"]")));
                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", dayTrigger);

                    Console.WriteLine($"Elapsed: {elapsed.Elapsed}");

                    Console.WriteLine("Procurando slots");
                    var slots = wait.Until(driver => driver.FindElements(By.CssSelector("#calendar-events > div")));

                    Console.WriteLine($"Elapsed: {elapsed.Elapsed}");

                    if (slots.Any(s=> s.Text.Contains("NÃO HÁ AULAS NESTE DIA")))
                    {
                        Console.WriteLine("NÃO HÁ AULAS NESTE DIA");
                        break;
                    }

                    Console.WriteLine($"Procurando slot desejado: {options.DesiredSlot}");
                    var targetSlot = slots.FirstOrDefault(slot => slot.Text.Contains(options.DesiredActivity) && slot.Text.Contains(options.DesiredSlot));

                    Console.WriteLine($"Elapsed: {elapsed.Elapsed}");

                    if (targetSlot != null)
                    {
                        Console.WriteLine("Slot encontrado");

                        if (targetSlot.Text.Contains("ESTÁS INSCRITO"))
                        {
                            Console.WriteLine("Já inscrito");
                            break;
                        }

                        if (targetSlot.Text.Contains("INSCRIÇÕES EM"))
                        {
                            Console.WriteLine("Inscrições ainda fechadas");
                            continue;
                        }

                        try
                        {
                            Console.WriteLine("Procurando botão de inscrever");
                            var buttons = targetSlot.FindElements(By.CssSelector(".buts_inscrever"));
                            if (buttons.Any())
                            {
                                Console.WriteLine("Clicando no botão de inscrever");
                                buttons.First().Click();

                                Console.WriteLine("Procurando botões do modal de confirmação");
                                var modalButtons = wait.Until(driver => driver.FindElements(By.CssSelector(".dialog-button-bold")));
                                Console.WriteLine($"Elapsed: {elapsed.Elapsed}");

                                var okButton = modalButtons.FirstOrDefault(); //modalButtons.FirstOrDefault(button => button.Text.Contains("OK"));

                                if (okButton != null)
                                {
                                    Console.WriteLine("Clicando no botão de OK");
                                    okButton.Click();
                                    Console.WriteLine($"Elapsed: {elapsed.Elapsed}");
                                    Console.WriteLine("Inscrição solicitada com sucesso");
                                    break;
                                }
                                else
                                {
                                    Console.WriteLine("Botão de OK não encontrado na modal.");
                                }
                            }
                            else
                            {
                                Console.WriteLine("Botão de inscrever não encontrado");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Erro ao tentar inscrever: {ex.Message}", ex.Message);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Slot não encontrado");
                    }

                    Console.WriteLine("Navegando para página de inicial - Retry");
                    driver.Navigate().GoToUrl("https://www.regibox.pt/app/app_nova/index.php");
                } while (elapsed.Elapsed < TimeSpan.FromSeconds(120));
            }
            finally
            {
                // Close the browser
                driver.Quit();

                Console.WriteLine("Finished");
            }
        }
    }
}
