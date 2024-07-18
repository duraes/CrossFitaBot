using CrossFitaBot.Core;
using Microsoft.Extensions.CommandLineUtils;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace CrossFitaBot
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var cmd = new CommandLineApplication();

            var argUser = cmd.Option("-u | --user <value>", "User Login", CommandOptionType.SingleValue);
            var argPassword = cmd.Option("-p | --password <value>", "User Password", CommandOptionType.SingleValue);
            var argBox = cmd.Option("-b | --box <value>", "Box Name", CommandOptionType.SingleValue);
            var argTargetSlotStartTime = cmd.Option("-t | --targetSlotStartTime <value>", "Target Slot Start Time in XX:XX 24h format (06:00)", CommandOptionType.SingleValue);
            var argDaysAhead = cmd.Option("-d | --daysAhead <value>", "Number of days ahead to search for the target slot", CommandOptionType.SingleValue);
            var argActivity = cmd.Option("-a | --activity <value>", "Class Name", CommandOptionType.SingleValue);
            var argSeleniumHost = cmd.Option("-h | --seleniumhost <value>", "Class Name", CommandOptionType.SingleValue);

            cmd.OnExecute(() =>
            {
                string boxName = argBox.Value();
                string email = argUser.Value();
                string senha = argPassword.Value();
                string desiredSlot = argTargetSlotStartTime.Value();
                int daysAhead = int.Parse(argDaysAhead.Value());
                var desiredActivity = argActivity.Value();

                SchedulerBot.Schedule(new SchedulerBot.SchedulerBotOptions
                {
                    BoxName = boxName
                    , Email = email
                    , Senha = senha
                    , DesiredSlot = desiredSlot
                    , DaysAhead = daysAhead
                    , DesiredActivity = desiredActivity
                    , HeadlessRun = false
                },
                @"http://taskrunner.local:4444/wd/hub" //argSeleniumHost.Value()
                );

                return 0;
            });

            cmd.HelpOption("-? | -h | --help");
            cmd.Execute(args);

            
        }
    }
}
