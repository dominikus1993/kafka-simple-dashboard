using KafkaSimpleDashboard.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KafkaSimpleDashboard.Server.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace KafkaSimpleDashboard.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class KafkaTopicsController : ControllerBase
    {
        private readonly ILogger<KafkaTopicsController> logger;
        private readonly IKafkaTopicsAdminService _adminService;

        public KafkaTopicsController(IKafkaTopicsAdminService adminService, ILogger<KafkaTopicsController> logger)
        {
            this.logger = logger;
            _adminService = adminService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<KafkaTopic>>> Get()
        {
            var result = await _adminService.GetAll();
            return result.Match<ActionResult<IEnumerable<KafkaTopic>>>(topics => Ok(topics),
                () => NoContent());
        }
    }
}