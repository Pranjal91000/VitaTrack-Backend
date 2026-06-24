using Microsoft.AspNetCore.Mvc;
using VitaTrack.Api.Abstractions;
using VitaTrack.Api.Models.WeightTrack;

namespace VitaTrack.Api.Controllers
{
    [ApiController]
    [Route("api/weight-tracking")]
    public class WeightTrackController(IWeightTrackService service): ControllerBase
    {
        private readonly IWeightTrackService weightTrackService = service;

        [HttpPost]
        public async Task<ActionResult<bool>> SaveWeightAsync(WeightTrackSaveInputModel input)
        {
            return await weightTrackService.SaveWeightAsync(input);
        }

        [HttpPut]
        public async Task<ActionResult<bool>> UpdateWeightAsync(WeightTrackUpdateInputModel input)
        {
            return await weightTrackService.UpdateWeightAsync(input);
        }

        [HttpGet]
        public async Task<ActionResult<WeightTrackViewModel>> GetWeightAsync([FromQuery] long id)
        {
            var data = await weightTrackService.GetWeightTracked(id);
            return data;
        }

        [HttpGet]
        public async Task<ActionResult<List<WeightTrackViewModel>>> GetWeightHistoryAsync([FromQuery] DateOnly fromDate, DateOnly toDate)
        {
            return await weightTrackService.GetWeightTrackedHistory(fromDate, toDate);
        }
    }
}
