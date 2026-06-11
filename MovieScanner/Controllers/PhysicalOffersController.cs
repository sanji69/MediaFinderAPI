using MediaFinder.DTOs.Offers;
using MediaFinder.Interface;
using Microsoft.AspNetCore.Mvc;

namespace MediaFinder.Controllers
{
    [ApiController]
    [Route("api/physical-offers")]
    public class PhysicalOffersController : ControllerBase
    {
        private readonly IPhysicalOfferProvider _physicsOfferProvider;

        public PhysicalOffersController(IPhysicalOfferProvider physicsOfferProvider)
        {
            _physicsOfferProvider = physicsOfferProvider;
        }

        [HttpGet]
        public async Task<ActionResult<List<PhysicalOfferDto>>> Search([FromQuery] string title, [FromQuery] string mediaType, [FromQuery] int? seasonNumber = null, string? language = null, string? countryCode = null)
        {
            var query = new PhysicalOfferSearchQuery
            {
                Title = title,
                MediaType = mediaType,
                SeasonNumber = seasonNumber,
                Language = language,
                CountryCode = countryCode
            };

            var offers = await _physicsOfferProvider.SearchAsync(query);

            return Ok(offers);
        }
    }
}
