using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;

namespace ProjectAPI.Controllers;

[Route("api/files")]
[ApiController]
public class FilesController : ControllerBase
{
    private readonly IAmazonS3 _s3Client;
    public FilesController(IAmazonS3 s3Client)
    {
        _s3Client = s3Client;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadFileAsync(IFormFile file, string bucketName)
    {
        var request = new PutObjectRequest()
        {
            BucketName = bucketName,
            Key = file.FileName,
            InputStream = file.OpenReadStream()
        };

        request.Metadata.Add("Content-Type", file.ContentType);
        await _s3Client.PutObjectAsync(request);

        return Ok($"File {file.FileName} uploaded to S3 successfully!");
    }

    [HttpPost("download")]
    public async Task<IActionResult> DownloadFileAsync(string bucketName, string key)
    {
        var s3Object = await _s3Client.GetObjectAsync(bucketName, key);

        return File(s3Object.ResponseStream, s3Object.Headers.ContentType, key);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllFilesAsync(string bucketName)
    {
        var request = new ListObjectsV2Request()
        {
            BucketName = bucketName
        };
        var result = await _s3Client.ListObjectsV2Async(request);
        return Ok(result.S3Objects);
    }


    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteFileAsync(string bucketName, string key)
    {
        await _s3Client.DeleteObjectAsync(bucketName, key);

        return NoContent();
    }
}