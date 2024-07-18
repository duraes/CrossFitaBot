using CrossFitaBot.Core;
using Microsoft.AspNetCore.Mvc;

namespace CrossFitaBot.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScheduleController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get(
            string boxName,
            string email,
            string senha,
            string desiredSlot,
            int daysAhead,
            string desiredActivity)
        {
            SchedulerBot.Schedule(new SchedulerBot.SchedulerBotOptions
            {
                BoxName = boxName
                ,
                Email = email
                ,
                Senha = senha
                ,
                DesiredSlot = desiredSlot
                ,
                DaysAhead = daysAhead
                ,
                DesiredActivity = desiredActivity
                ,
                HeadlessRun = true
            });

            return Ok();
        }
    }
}