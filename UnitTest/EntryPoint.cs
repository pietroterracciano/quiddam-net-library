using QuiddamLibrary;
using System;
using System.Diagnostics;
using System.IO;
using UnitTest.Models;

namespace UnitTest
{
    public class EntryPoint
    {
        private static QuiddamPool _qpStudentsTable;
        private static String[] _aStudentsRows;

        public static void Main(String[] args)
        {
            String sStudentsTable = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "FakeDatabase", "Students.table"));
            _aStudentsRows = sStudentsTable.Split('\n');

            _qpStudentsTable = Quiddam.CreatePool("STUDENTS_TABLE");
            _qpStudentsTable.Config.MaxMemoryConsumption = 4096;

            #region Direct

            Stopwatch oStopwatch1 = Stopwatch.StartNew();

            for(Int32 i=0; i< 100000000; i++)
            {
                Int32 j = i % 5000;
                StudentModel oStudent = GetStudent(j, false);
            }

            oStopwatch1.Stop();

            Console.WriteLine(oStopwatch1.ElapsedMilliseconds);

            #endregion

            #region Using Quiddam in ThreadSafe mode

            Quiddam.Config.IsThreadSafe = true;

            Stopwatch oStopwatch2 = Stopwatch.StartNew();

            for (Int32 i = 0; i < 100000000; i++)
            {
                Int32 j = i % 5000;
                StudentModel oStudent = GetStudent(j, true);
            }

            oStopwatch2.Stop();

            Console.WriteLine(oStopwatch2.ElapsedMilliseconds);

            #endregion

            #region Using Quiddam in NoThreadSafe mode

            Quiddam.Config.IsThreadSafe = false;

            Stopwatch oStopwatch3 = Stopwatch.StartNew();

            for (Int32 i = 0; i < 100000000; i++)
            {
                Int32 j = i % 5000;
                StudentModel oStudent = GetStudent(j, true);
            }

            oStopwatch3.Stop();

            Console.WriteLine(oStopwatch3.ElapsedMilliseconds);

            #endregion
        }

        private static StudentModel GetStudent(Int32 nStudentID, Boolean bTryGetFromRAMCache)
        {
            StudentModel oStudent = null;

            if (nStudentID < 1)
                goto METHOD_RETURN;

            String sQPKeyName = "STUDENT__ID__" + nStudentID;

            if (bTryGetFromRAMCache)
            {
                _qpStudentsTable.GetObject<StudentModel>(sQPKeyName, out oStudent);

                if (oStudent != null)
                    goto METHOD_RETURN;
            }
            
            if (_aStudentsRows == null || _aStudentsRows.Length <= nStudentID)
                goto METHOD_RETURN;

            String sStudentRow = _aStudentsRows[nStudentID];

            if (String.IsNullOrEmpty(sStudentRow))
                goto METHOD_RETURN;

            String[] aStudentData = sStudentRow.Split('\t');

            if (aStudentData == null)
                goto METHOD_RETURN;

            oStudent = new StudentModel(nStudentID);

            for(Int32 i=0; i<aStudentData.Length; i++)
            {
                if (i==0 || String.IsNullOrEmpty(aStudentData[i]))
                    continue;

                String sStudentValue = aStudentData[i].Trim();

                switch(i)
                {
                    case 1:
                        oStudent.Firstname = sStudentValue;
                        break;
                    case 2:
                        oStudent.Lastname = sStudentValue;
                        break;
                    case 3:
                        oStudent.Email = sStudentValue;
                        break;
                    case 4:
                        oStudent.Gender = sStudentValue.ToUpper().Trim().Contains("MALE") ? StudentModel.GENDER.MALE : StudentModel.GENDER.FEMALE;
                        break;
                    case 5:
                        oStudent.Language = sStudentValue;
                        break;
                    case 6:
                        try { oStudent.Birthday = Convert.ToDateTime(sStudentValue); } catch (Exception) { /** Nothing */ };
                        break;
                    case 7:
                        oStudent.Password = sStudentValue;
                        break;
                    case 8:
                        oStudent.Login = sStudentValue;
                        break;
                }
            }

            _qpStudentsTable.AddEditObject(sQPKeyName, oStudent);

            METHOD_RETURN:

            return oStudent;
        }
    }
}