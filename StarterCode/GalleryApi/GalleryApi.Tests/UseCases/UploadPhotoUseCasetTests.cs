using GalleryApi.Application.DTOs;
using GalleryApi.Application.UseCases.Photos;
using GalleryApi.Domain.Entities;
using GalleryApi.Domain.Interfaces;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace GalleryApi.Tests.UseCases
{
    public class UploadPhotoUseCasetTests
    {
        private readonly Mock<IPhotoRepository> _photoMock;
        private readonly Mock<IAlbumRepository> _albumMock;
        private readonly Mock<IStorageService> _storageMock;
        private readonly UploadPhotoUseCase _useCase;

        private static readonly Guid AlbumId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        private static readonly Guid PhotoId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        public UploadPhotoUseCasetTests()
        {
            _photoMock = new Mock<IPhotoRepository>();
            _albumMock = new Mock<IAlbumRepository>();
            _storageMock = new Mock<IStorageService>();
            _useCase = new UploadPhotoUseCase(_photoMock.Object, _albumMock.Object, _storageMock.Object);
        }

        [Fact]
        public async Task ExecuteAsync_PalauttaaOnnistuneen_KunKaikki_OnKunnossa()
        {
            var album = new Album { Id = AlbumId, Name = "Test Album" };
            var imageUrl = "/uploads/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa/Test-Photo.jpeg";
            var savedPhoto = new Photo
            {
                Id = PhotoId,
                AlbumId = AlbumId,
                Title = "Test Photo",
                FileName = "Test-Photo.jpeg",
                ImageUrl = imageUrl,
                ContentType = "image/jpeg",
                FileSizeBytes = 88500,
                UploadedAt = DateTime.UtcNow
            };

            _albumMock.Setup(a => a.GetByIdAsync(AlbumId)).ReturnsAsync(album);
            _storageMock
                .Setup(s => s.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), AlbumId))
                .ReturnsAsync(imageUrl);
            _photoMock.Setup(p => p.CreateAsync(It.IsAny<Photo>())).ReturnsAsync(savedPhoto);

            var request = new UploadPhotoRequest(
                AlbumId, "Test Photo", Stream.Null, "Test-Photo.jpeg", "image/jpeg", 88500);

            var result = await _useCase.ExecuteAsync(request);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(PhotoId, result.Value.Id);
            Assert.Equal(AlbumId, result.Value.AlbumId);
            Assert.Equal("Test Photo", result.Value.Title);
            Assert.Equal(imageUrl, result.Value.ImageUrl);
        }

        [Fact]
        public async Task ExecuteAsync_PalauttaaVirheen_KunAlbumiaEiLoydy()
        {
            _albumMock.Setup(a => a.GetByIdAsync(AlbumId)).ReturnsAsync((Album?)null);

            var request = new UploadPhotoRequest(
                AlbumId, "Test Photo", Stream.Null, "Test-Photo.jpeg", "image/jpeg", 88500);

            var result = await _useCase.ExecuteAsync(request);

            Assert.False(result.IsSuccess);
            Assert.Contains(AlbumId.ToString(), result.Error);
        }

        [Fact]
        public async Task ExecuteAsync_PalauttaaVirheen_KunTiedostotyyppiOnVaara()
        {
            var album = new Album { Id = AlbumId, Name = "Test Album" };
            _albumMock.Setup(a => a.GetByIdAsync(AlbumId)).ReturnsAsync(album);

            var request = new UploadPhotoRequest(
                AlbumId, "Test Photo", Stream.Null, "document.pdf", "application/pdf", 88500);

            var result = await _useCase.ExecuteAsync(request);

            Assert.False(result.IsSuccess);
            Assert.Contains("application/pdf", result.Error);
        }

        [Fact]
        public async Task ExecuteAsync_PalauttaaVirheen_KunTiedostoOnLiianSuuri()
        {
            var album = new Album { Id = AlbumId, Name = "Test Album" };
            _albumMock.Setup(a => a.GetByIdAsync(AlbumId)).ReturnsAsync(album);

            long tooBigBytes = 11L * 1024 * 1024; // 11 MB, raja on 10 MB

            var request = new UploadPhotoRequest(
                AlbumId, "Test Photo", Stream.Null, "big-photo.jpeg", "image/jpeg", tooBigBytes);

            var result = await _useCase.ExecuteAsync(request);

            Assert.False(result.IsSuccess);
            Assert.NotNull(result.Error);
        }
    }
}
