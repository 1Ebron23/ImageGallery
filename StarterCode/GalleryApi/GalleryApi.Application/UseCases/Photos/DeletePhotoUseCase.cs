using GalleryApi.Application.Common;
using GalleryApi.Domain.Interfaces;

namespace GalleryApi.Application.UseCases.Photos;

public class DeletePhotoUseCase
{
    private readonly IPhotoRepository _photoRepository;
    private readonly IStorageService _storageService;

    public DeletePhotoUseCase(IPhotoRepository photoRepository, IStorageService storageService)
    {
        _photoRepository = photoRepository;
        _storageService = storageService;
    }

    public async Task<Result> ExecuteAsync(Guid photoId)
    {
        // 1. Hae kuva tietokannasta
        var photo = await _photoRepository.GetByIdAsync(photoId);
        if (photo is null)
            return Result.Failure($"Kuvaa {photoId} ei löydy.");

        // 2. Poista tiedosto tallennuspalvelusta
        //    Käytetään FileName + AlbumId, ei ImageUrl
        //    (Azure Blob Storagessa URL ei vastaa suoraan blob-nimeä)
        await _storageService.DeleteAsync(photo.FileName, photo.AlbumId);

        // 3. Poista tietue tietokannasta
        await _photoRepository.DeleteAsync(photoId);

        return Result.Success();
    }
}
