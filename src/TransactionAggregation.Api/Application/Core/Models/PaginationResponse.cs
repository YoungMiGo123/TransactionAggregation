using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace TransactionAggregation.API.Application.Core.Models;

public class PaginationRequest
{
    [FromQuery]
    public int PageNo { get; set; } = 1;
    [FromQuery]
    public int PageSize { get; set; } = 20;
		
    public PaginationRequest()
    {

    }
    public PaginationRequest(int pageNo, int pageSize)
    {
        PageNo = pageNo;
        PageSize = pageSize;
    }
}

public class PaginationResponse<T>
{
    [Required]
    public IEnumerable<T> Payload { get; set; }
    [Required]
    public int Page { get; set; } 
    [Required]
    public int PageSize { get; set; }
    [Required]
    public int TotalCount { get; set; }
    [Required]
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    public PaginationResponse()
    {
			
    }
		
    public PaginationResponse(IEnumerable<T> payload, int page, int pageSize, int totalCount)
    {
        Payload = payload;
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
    }
}