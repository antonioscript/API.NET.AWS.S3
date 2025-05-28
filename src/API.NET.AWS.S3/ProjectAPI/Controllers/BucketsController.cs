using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Microsoft.AspNetCore.Mvc;

namespace ProjectAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BucketsController : ControllerBase
{
    private readonly IAmazonS3 _s3Client;

    public BucketsController(IAmazonS3 s3Client)
    {
        _s3Client = s3Client;
    }


    [HttpPost("create")]
    public async Task<IActionResult> CreateBucketAsync([FromQuery] string bucketName)
    {
        try
        {
            // Verifica se o bucket existe
            try
            {
                await _s3Client.HeadBucketAsync(new HeadBucketRequest { BucketName = bucketName });
                return BadRequest($"Bucket {bucketName} already exists.");
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // Se cair aqui, o bucket não existe — pode criar
            }

            // Cria o bucket
            await _s3Client.PutBucketAsync(new PutBucketRequest
            {
                BucketName = bucketName
            });

            return Ok($"Bucket {bucketName} created.");
        }
        catch (AmazonS3Exception ex)
        {
            return StatusCode((int)ex.StatusCode, $"AWS Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal error: {ex.Message}");
        }
    }

    [HttpGet("get-all")]
    public async Task<IActionResult> GetAllBucketAsync()
    {
        var data = await _s3Client.ListBucketsAsync();
        var buckets = data.Buckets.Select(b => { return b.BucketName; });
        return Ok(buckets);
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteBucketAsync(string bucketName)
    {
        await _s3Client.DeleteBucketAsync(bucketName);
        return NoContent();
    }
}
