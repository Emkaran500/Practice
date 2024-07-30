namespace Exam.Controllers;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Exam.Entity;
using Azure.Storage.Blobs;

[ApiController]
public class AvatarController : ControllerBase
{
    private readonly UserManager<User> userManager;
    private readonly BlobServiceClient blobServiceClient;
    private const string connectionString = "";

    public AvatarController(UserManager<User> userManager)
    {
        this.userManager = userManager;
        this.blobServiceClient = new BlobServiceClient(connectionString);
    }

    [HttpGet("api/my/avatar")]
    public async Task<FileStreamResult> GetAvatar() {
        var currentUser = await userManager.GetUserAsync(base.User);
        var containerClient = blobServiceClient.GetBlobContainerClient("mycontainer");
        var blobClient = containerClient.GetBlobClient(currentUser.AvatarUrl);
        using var fileStream = System.IO.File.Create($"{currentUser.AvatarUrl}");
        var response = await blobClient.DownloadToAsync(fileStream);

        return base.File(response.ContentStream, contentType: "image");
    }

    [HttpPost("api/my/avatar")]
    public async void CreateAvatar() {
        var currentUser = await userManager.GetUserAsync(base.User);
        var containerClient = blobServiceClient.GetBlobContainerClient("mycontainer");
        var blobClient = containerClient.GetBlobClient(currentUser.AvatarUrl);

        currentUser.AvatarUrl = "aaa";
    }
}