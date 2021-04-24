using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Dnsy.Api.Controllers
{
    [ApiController]
    [Route("[action]")]
    public class InfoController : ControllerBase
    {
        [Route("/")]
        public ActionResult Index ()
        {
            return Ok(new
            {
                Service = "DNSy API",
                Version = "1.0.0"
            });
        }
    }
}