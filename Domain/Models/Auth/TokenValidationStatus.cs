namespace Domain.Models.Auth;

public enum TokenValidationStatus
{
    Valid=1,        
    Expired,    
    Invalid 
}