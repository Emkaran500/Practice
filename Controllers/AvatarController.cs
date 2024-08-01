namespace Exam.Controllers;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Exam.Entity;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authorization;

[ApiController]
public class AvatarController : ControllerBase
{
    private readonly UserManager<User> userManager;
    private readonly BlobServiceClient blobServiceClient;
    private const string connectionString = "https://emilbabayevstorage.blob.core.windows.net";

    public AvatarController(UserManager<User> userManager)
    {
        this.userManager = userManager;
        this.blobServiceClient = new BlobServiceClient(new Uri(connectionString));
    }

    [HttpGet("api/my/avatar")]
    [Authorize(Roles = "user")]
    public async Task<FileStreamResult> GetAvatar() {
        var currentUser = await userManager.GetUserAsync(base.User);
        var containerClient = blobServiceClient.GetBlobContainerClient("emilcontainer");
        var avatarName = currentUser.AvatarUrl.Split('/')[4];
        var blobClient = containerClient.GetBlobClient(avatarName);
        var fileStream = new MemoryStream();
        await blobClient.DownloadToAsync(fileStream);
        fileStream.Position = 0;

        return base.File(fileStream, contentType: "image/" + avatarName.Split('.')[1]);
    }

    [HttpPost("api/my/avatar")]
    [Authorize(Roles = "user")]
    public async Task CreateAvatar(IFormFile formFile) {
        var containerClient = blobServiceClient.GetBlobContainerClient("emilcontainer");
        var blobClient = containerClient.GetBlobClient(formFile.FileName);
        using var fileStream = new MemoryStream();
        await formFile.CopyToAsync(fileStream);
        fileStream.Position = 0;
        var currentUser = await userManager.GetUserAsync(base.User);
        await blobClient.UploadAsync(fileStream);
        var connectionString = "https://emilbabayevstorage.blob.core.windows.net";
        var containerName = "emilcontainer";
        currentUser.AvatarUrl = $"{connectionString}/{containerName}/{formFile.FileName}";
        await userManager.UpdateAsync(currentUser);
    }
}