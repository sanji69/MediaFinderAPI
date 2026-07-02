using MediaFinder.DTOs.Offers;

namespace MediaFinder.Interface
{
    public interface IPhysicalOfferProvider
    {
        Task<List<PhysicalOfferDto>> SearchAsync(PhysicalOfferSearchQuery query);
    }
}