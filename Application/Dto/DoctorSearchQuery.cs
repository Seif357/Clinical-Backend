namespace Application.Dto;

public class DoctorSearchQuery
{
    public string?  Search           { get; set; }   // name / username / email
    public bool?    IsLicenseVerified { get; set; }
    public bool?    HasSchedule       { get; set; }
    public string?  SortBy            { get; set; }  // "name" | "registeredAt"
    public bool     Descending        { get; set; }  = false;
    public int      Page              { get; set; }  = 1;
    public int      PageSize          { get; set; }  = 20;
}