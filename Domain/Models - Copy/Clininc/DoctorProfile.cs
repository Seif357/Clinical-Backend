using Domain.Models.Auth;
using Domain.Models___Copy.Clininc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Domain.Models.Clininc
{
    public class Doctor : User
    {
        public string licenseNumber;
        public DateOnly licenseExpiryDate;
        public string GetModelResponse(Patient_Response responce)
        {
            return responce.ImageUrl;
        }
        public void RequestServiceFromPatient(DoctorRequest request)
        {

        }
        public void UpdateMedicalRecord(MedicalRecord medicalRecord)
        {
        }
    }
}
