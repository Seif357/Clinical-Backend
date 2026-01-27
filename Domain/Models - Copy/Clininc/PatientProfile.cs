using Domain.Models.Auth;
using Domain.Models___Copy.Clininc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Domain.Models.Clininc
{
    public class Patient:User
    {
        public DateTime? DateOfBirth;
        public string Gender;
        public MedicalRecord MedicalRecord;
        public void Request_Appointment(Appointment_Request appointment_Request)
        {

        }
        public void RespondToDoctorRequest(Patient_Response response)
        {

        }
    }
}
