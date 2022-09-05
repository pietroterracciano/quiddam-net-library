using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTest.Models
{
    public class StudentModel
    {
        private readonly Int32 _i32ID;
        private String _sFirstname, _sLastname, _sEmail, _sLanguage, _sLogin, _sPassword;
        private DateTime? _dtBirthday;
        private GENDER _eGender;

        public enum GENDER
        {
            UNKNOWN = 1,
            MALE = 2,
            FEMALE = 4,
        }

        public Int32 ID {
            get { return _i32ID; }
        }
        public String Firstname
        {
            get { return _sFirstname; }
            set { _sFirstname = value; }
        }
        public String Lastname
        {
            get { return _sLastname; }
            set { _sLastname = value; }
        }
        public String Language
        {
            get { return _sLanguage; }
            set { _sLanguage = value; }
        }
        public String Email
        {
            get { return _sEmail; }
            set { _sEmail = value; }
        }
        public String Login
        {
            get { return _sLogin; }
            set { _sLogin = value; }
        }
        public String Password
        {
            get { return _sPassword; }
            set { _sPassword = value; }
        }
        public DateTime? Birthday
        {
            get { return _dtBirthday; }
            set { _dtBirthday = value; }
        }
        public GENDER Gender
        {
            get { return _eGender; }
            set { _eGender = value; }
        }

        public StudentModel(Int32 i32ID)
        {
            _i32ID = i32ID;

            _sFirstname =
                _sLastname =
                _sEmail = 
                _sLogin = 
                _sPassword = "";

            _dtBirthday = null;

            _eGender = GENDER.UNKNOWN;
        }
    }
}
