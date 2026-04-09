using Microsoft.AspNetCore.Mvc;
using TeknoSOS.WebApp.Services;

namespace TeknoSOS.WebApp.Controllers.Api
{
    [Route("api/banners")]
    [ApiController]
    public class BannersController : ControllerBase
    {
        private readonly IBannerService _bannerService;

        public BannersController(IBannerService bannerService)
        {
            _bannerService = bannerService;
        }

        /// <summary>
        /// Track banner view
        /// </summary>
        [HttpPost("{id}/view")]
        public async Task<IActionResult> TrackView(int id)
        {
            try
            {
                await _bannerService.IncrementViewAsync(id);
                return Ok(new { success = true });
            }
            catch
            {
                return Ok(new { success = false });
            }
        }

        /// <summary>
        /// Track banner click
        /// </summary>
        [HttpPost("{id}/click")]
        public async Task<IActionResult> TrackClick(int id)
        {
            try
            {
                await _bannerService.IncrementClickAsync(id);
                return Ok(new { success = true });
            }
            catch
            {
                return Ok(new { success = false });
            }
        }
    }
}
