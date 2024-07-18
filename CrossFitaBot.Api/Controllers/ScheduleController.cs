using CrossFitaBot.Core;
using Microsoft.AspNetCore.Mvc;

namespace CrossFitaBot.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScheduleController : ControllerBase
    {
        private readonly IConfiguration configuration;

        public ScheduleController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        [HttpGet]
        public IActionResult Get(
            string? boxName,
            string? email,
            string? senha,
            string?   desiredSlot,
            int? daysAhead,
            string? desiredActivity)
        {

            boxName = boxName ?? "Squad";
            email = email ?? "alexbduraes@gmail.com";
            senha = senha ?? "JsA92zqsEVGLh4";
            desiredSlot = desiredSlot ?? "09:00";
            daysAhead =  daysAhead ?? 2;
            desiredActivity = desiredActivity ?? "SQUAD CROSS";

            var host = configuration.GetValue<string>("seleniumhost");

            SchedulerBot.Schedule(new SchedulerBot.SchedulerBotOptions
            {
                BoxName = boxName,
                Email = email,
                Senha = senha,
                DesiredSlot = desiredSlot,
                DaysAhead = daysAhead ?? 3,
                DesiredActivity = desiredActivity,
                HeadlessRun = true
            }, host);

            return Ok();
        }
    }
}