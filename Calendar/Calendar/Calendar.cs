/*
  	Michael Finnegan, Wednesday 2nd to Friday 4th  Juanary 2019
	This program can display a calendar for any year from 1583 to 2099,
    and print it out on paper or to a PDF file.
    HKEY_CURRENT_USER\Software\Microsoft\VisualStudio\11.0\General
    DWORD: SuppressUppercaseConversion
    Value: 1
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Calendar
{
    class Calendar
    {
        private string[] sMonthArr = new string[]
            {
                "January","February","March","April","May","June",
                "July","August","September","October","November","December"
            };

        private string[] sDayArr = new string[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
        public int ErrNum { get; set; }       //In constructor initialize to 0.
        public string[] arrErr;
        private int[] bigDaysArr;	        //total # of days in all the previous months of all previous years.
        private int year;
        private int[] daysInPrevMonths;	    //total # of days in all the previous months of chosen year.
        private int[] chosenYearMonths;	    //The # of days in each month of the chosen year
        public int[,,] matrix;				//Array of months 0 .. 11 then within each month array matrix of 7 cols by 6 rows.
        //to access a cols use $col = $this->matrix[0][0][0]
        public List<string> notepadTable;       //table made up in the form of an array of strings.

        /*-------------------------------------------
            Constructor
            Receive the year. Check if it is valid. If not set error var,
            else set the year.
        */
        public Calendar(string sYear)
        {
            int result;
            bool b = int.TryParse(sYear, out result);
            DateTime localDate = DateTime.Now;
            if (!b)
            {
                //No year was chosen, therefore choose the current year
                this.year = localDate.Year;
            }
            else
            {
                this.year = result;
                if (this.year < 1583 || this.year > 2099)
                {
                    this.ErrNum = 1;
                    this.arrErr = new string[] { "Year out of range (1583 - 2099)" };

                }
                else
                {
                    //There was a valid year chosen and it is in the range 1583..2099.
                    SetupDaysInPrevMonths();
                    SetupBigDaysArr();
                    CalendarToMatrix();
                    CalendarToNotepadTable();		//There are 38 rows in the table including opening & closing table tags.

                }
            }
        }//End constructor
        /*-------------------------------------------
            This Easter date calculation function came from 
            https://www.drupal.org/node/1180480
        */
        public Dictionary<string, int> EasterSunday(int year)
        {
            int a = year % 19;
            int b = year / 100;
            int c = year % 100;
            int d = b / 4;
            int e = b % 4;
            int f = (b + 8) / 25;
            int g = ((b - f + 1) / 3);
            int h = (19 * a + b - d - g + 15) % 30;
            int i = c / 4;
            int k = c % 4;
            int l = (32 + 2 * e + 2 * i - h - k) % 7;
            int m = ((a + 11 * h + 22 * l) / 451);
            int easterMonth = ((h + l - 7 * m + 114) / 31);//  [3=March, 4=April]
            int p = (h + l - 7 * m + 114) % 31;
            int easterDate = p + 1; //     (date in Easter Month)

            return new Dictionary<string, int>() { { "year", year }, { "month", easterMonth }, { "date", easterDate } };
        }

        /*----------------------------------------------------
         * Centre text within a given field with, returning a string of field width.
         */
        private string CenterText(string text, int fieldWidth)
        {
            if (text == String.Empty || fieldWidth < 2)
            {
                return "Error in string or fieldWidth";
            }

            int lSpaces;
            string spaces = "                                                                      ";
            string output = String.Empty;
            if (text.Length >= fieldWidth)
            {
                output = text.Substring(0, fieldWidth);
            }
            else
            {
                lSpaces = (fieldWidth - text.Length) / 2;
                output = spaces.Substring(0, lSpaces) + text + spaces.Substring(0, lSpaces);
                if (output.Length < fieldWidth)
                {
                    output = " " + output;
                }
            }
            return output;
        }

        /*----------------------------------------------------
         * Return 0 if not a leap year, else return 1.
        */
        private int IsLeapYear(int y)
        {

            return (y % 400 == 0) ? 1 : ((y % 100 == 0) ? 0 : (y % 4 == 0) ? 1 : 0);
        }

        /*---------------------------------------------------
            Return the total number of days in all the previous years,
            including leap years. 
        */
        private int DaysInPreviousYears()
        {

            int pY = this.year - 1;
            int leapDays = -(pY / 100) + (pY / 400) + (pY / 4);
            int prevDays = pY * 365 + leapDays;
            return prevDays;
        }

        /*---------------------------------------------------
           Set up an array containing the number of days in each month of the chosen year.
       */
        private void SetupChosenYearMonths()
        {
            //									0	1	2	3	4	5	6	7	8	9	10	11
            //								    Jan Feb Mar Apr May Jun Jul Aug Sep Oct Nov Dec
            this.chosenYearMonths = new int[] { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

            this.chosenYearMonths[1] = 28 + IsLeapYear(this.year);

        }

        /*----------------------------------------------------
           Setup the accumulated days in all of the months prior to a given month for the chosen year.
       */
        private void SetupDaysInPrevMonths()
        {
            if (this.ErrNum > 0)
            {
                return;
            }
            SetupChosenYearMonths();
            this.daysInPrevMonths = new int[12];
            int acc = 0;
            for (int i = 0; i < 12; i += 1)
            {
                this.daysInPrevMonths[i] = acc;
                acc += this.chosenYearMonths[i];
            }

        }
        /*-------------------------------------------
            Setup the accumulated days in all of the months prior to a given month 
            plus all the years previous to the chosen year.
            function SetupDaysInPrevMonths() must have already been called before calling this function.
        */
        private int SetupBigDaysArr()
        {

            if (this.ErrNum > 0)
            {
                return this.ErrNum;
            }
            this.bigDaysArr = new int[12];
            int prevDays = DaysInPreviousYears();
            for (int i = 0; i < 12; i += 1)
            {
                this.bigDaysArr[i] = prevDays + this.daysInPrevMonths[i];

            }
            return 0;
        }

        /*-------------------------------------------
            Receive a day number 1..31 and a month number 1..12
            then return the column 0..6 that this day is on.
            function this.SetupBigDaysArr() must have already been called before calling this function.
            This is done by the constructor.
        */
        private int GetDayCol(int d, int m)
        {

            int x = (this.bigDaysArr[m - 1] + d - 1);
            int col = x % 7;

            return col;
        }

        /*-------------------------------------------
            Receive a day number 1..31 and a month number 1..12
            then return the row 0..5 that this day is on.
            function setupBigDaysArr() must have already been called before calling this function.
            This is done by the constructor.

        */
        private int GetDayRow(int d, int m)
        {

            int row = ((GetDayCol(1, m) + (d - 1)) / 7);

            return row;
        }
        /*-------------------------------------------
            Receive a day number 1..31 and a month number 1..12.
            Return an associative array containing the col (0..6) and
            the row (0..5) that $d, $m is on.
            The days of the week are arranged as
            0   1   2   3   4   5   6
            Mon Tue Wed Thu Fri Sat Sun
        */
        private Dictionary<string, int> GetDayColRow(int d, int m)
        {

            Dictionary<string, int> colRow = new Dictionary<string, int>
                        {
                            {"col", this.GetDayCol(d, m)},
                            {"row", this.GetDayRow(d, m)}
                        };

            return colRow;
        }

        /*-------------------------------------------
            Initialize the matrix for all of the 12 months. 
            1. Setting the default values to 0.
            2. put the date values in the matrix into their appropriate rows and columns.
        */
        private void InitMatrix()
        {

            this.matrix = new int[12, 6, 7];
            for (int m = 0; m < 12; m += 1)
            {
                for (int r = 0; r < 6; r += 1)
                {
                    for (int c = 0; c < 7; c += 1)
                    {
                        this.matrix[m, r, c] = 0;
                    }
                }
            }

        }

        /*-------------------------------------------
            Store a month in the matrix.
            $mm = 1..12
            The matrix must have already been setup with 0 values.
        */
        private void MonthToMatrix(int mm)
        {

            int m = mm - 1;
            int lastDay = this.chosenYearMonths[m];			//$lastDay = total # of days in the month
            for (int d = 1; d <= lastDay; d += 1)
            {
                Dictionary<string, int> colRow = this.GetDayColRow(d, mm);
                int c = colRow["col"];
                int r = colRow["row"];
                this.matrix[m, r, c] = d;
            }

            return;
        }

        /*-------------------------------------------
            Store all the dates for each of the 12 months in the matrix.
        */
        private void CalendarToMatrix()
        {

            this.InitMatrix();
            for (int m = 1; m <= 12; m += 1)
            {
                MonthToMatrix(m);
            }
            return;
        }


        /*-------------------------------------------
            Receive a month (0..11) and a row (0..5) represents a row of dates to fetch for that month.
            Return a string with the values formatted within a field width of 5.
        */
        private string MonthRowDatesToString(int m, int r)
        {

            string s = String.Empty;
            for (int c = 0; c < 7; c += 1)
            {
                int d = this.matrix[m, r, c];
                if (d > 0)
                {
                    s += String.Format("{0,5}", d);
                }
                else
                {
                    s += String.Format("{0, 5}", " ");
                }
            }

            return s;
        }

        /*-------------------------------------------
            Put the days of the week into a string, each day occupies a field width of 5.
        */
        private string DaysOfTheWeekToString()
        {
            string s = String.Empty;
            for (int i = 0; i < 7; i += 1)
            {
                s += String.Format("{0, 5}", this.sDayArr[i]);
            }

            return s;
        }

        /*------------------------------------------
            Based on the value of $row create and return an appropriate string
            containg a calendar row .
        */
        private string CreateNotepadCalendarRow(int row)
        {

            string s = String.Empty;
            int r = row % 9;
            int lm = 3 * (row / 9);
            string leftMonth = this.sMonthArr[lm] + " " + this.year;
            string middleMonth = this.sMonthArr[lm + 1] + " " + this.year;
            string rightMonth = this.sMonthArr[lm + 2] + " " + this.year;
            switch (r)
            {

                case 0: // Month name on each side
                    s = CenterText(leftMonth, 35) + "  " + CenterText(middleMonth, 35) + "  " + CenterText(rightMonth, 35);
                    s += Environment.NewLine;
                    break;

                case 1:	// Days of the week on each side: left, middle, right.
                    s = DaysOfTheWeekToString() + "  " + DaysOfTheWeekToString() + "  " + DaysOfTheWeekToString();
                    s += Environment.NewLine;
                    break;

                case 8: // A clear line below the month dates.
                    s = Environment.NewLine;
                    break;

                default: //Assume its one of the six date rows they occur at row positions 2..7 in the calendar
                    int datesRow = r - 2;
                    s = MonthRowDatesToString(lm, datesRow) + "  ";
                    s += MonthRowDatesToString(lm + 1, datesRow) + "  ";
                    s += MonthRowDatesToString(lm + 2, datesRow) + Environment.NewLine;
                    break;
            }

            return s;
        }

        /*------------------------------------------
            A calendar of 12 months date values has already been setup in the 3 dimensional matrix array.
            Create a List<string> object calendar based on the data in matrix array and store the calendar
            as 36 rows of strings.

            Months are displayed in triples i.e., January & February, March, then April, May, June below them.
            Each month has a pattern of 9 rows. The pattern repeats in the months below.
            The 9 rows are:
            0		The month title
            1		The days of the week
            2..7	Date rows 0..5
            8		A blank row before the next pair of two months below.
            Therefore there are 4 * 9 = 36 rows.

        */
        private List<string> CalendarToNotepadTable()
        {
            this.notepadTable = new List<string>();
            string s = String.Empty;
            for (int i = 0; i < 36; i += 1)
            {
                s = CreateNotepadCalendarRow(i);
                this.notepadTable.Add(s);
            }


            // Add on an extra line displaying the date of Easter.
            Dictionary<string, int> easterInfo = EasterSunday(this.year);
            s = "  Easter Sunday falls on " + easterInfo["date"]; // easterInfo["year"];
            string month = (easterInfo["month"] == 3) ? " March" : " April";
            s += month + " " + easterInfo["year"] + Environment.NewLine;
            this.notepadTable.Add(s);

            return this.notepadTable;

        }
        //-----------------------------------------
        // convert List<string> notePadTable into a big long string for the real notepad control.
        //-----------------------------------------
        public string GetNotepadText()
        {
            string txt = Environment.NewLine;
            foreach (string s in this.notepadTable)
            {
                txt += s;
            }
            return txt;
        }

    }//End class

}
