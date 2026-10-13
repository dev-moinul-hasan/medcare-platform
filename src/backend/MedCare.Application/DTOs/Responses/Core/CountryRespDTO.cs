
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedCare.Application.DTOs.Responses.Core;

public sealed record CountryRespDTO
{   
    public int CountryId { get; set; }
    public required string CountryName { get; set; }
    public string? ISOCode { get; set; }
    public bool IsAvailable { get; set; }
}
