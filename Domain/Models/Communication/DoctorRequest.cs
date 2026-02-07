using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Communication
{
    public class DoctorRequest : ParentEntity
    {
        public string PatientId { get; set; }
        public string DoctorId { get; set; }
        public RequestType RequestType { get; set; }
        public string Message { get; set; }
        public string Subject { get; set; }
        public ICollection<DoctorReqestImage> DoctorReqestImages { get; set; }

    }
}
